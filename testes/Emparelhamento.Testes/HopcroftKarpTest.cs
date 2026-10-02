using Conde.Emparelhamento;
using Xunit;

namespace Conde.Emparelhamento.Testes;

/// <summary>
/// Emparelhamento maximo: Hopcroft-Karp e o caminho aumentante simples, os dois
/// contra a forca bruta sobre subconjuntos de arestas.
/// </summary>
public class HopcroftKarpTest
{
    /// <summary>
    /// O juiz enumera os 2^E subconjuntos de arestas e fica com o maior que nao
    /// repete vertice. Com ate 16 arestas sao 65.536 subconjuntos por grafo, e o
    /// teste roda 800 grafos.
    /// </summary>
    [Fact]
    public void OsDoisBatemComAForcaBruta()
    {
        var aleatorio = new Random(1973);
        var conferidos = 0;
        for (var caso = 0; caso < 800; caso++)
        {
            var g = Bipartido.Sorteado(4, 4, 0.35 + aleatorio.NextDouble() * 0.3, caso + 1);
            if (g.Arestas > 16) continue;
            var esperado = MaiorPorForcaBruta(g);

            var (hk, _) = HopcroftKarp.Maximo(g);
            var (simples, _) = Berge.Maximo(g);

            Assert.True(hk.Coerente() && simples.Coerente(), "casamento incoerente");
            Assert.True(esperado == hk.Pares, $"hopcroft-karp deu {hk.Pares} e o maximo e {esperado}");
            Assert.True(esperado == simples.Pares, $"caminho aumentante deu {simples.Pares} e o maximo e {esperado}");
            ConferirArestas(g, hk);
            ConferirArestas(g, simples);
            conferidos++;
        }
        Assert.True(conferidos > 600, $"so {conferidos} grafos couberam no limite de arestas");
    }

    private static int MaiorPorForcaBruta(Bipartido g)
    {
        var arestas = g.TodasAsArestas().ToList();
        var melhor = 0;
        for (var mascara = 0; mascara < 1 << arestas.Count; mascara++)
        {
            var usadosA = new HashSet<int>();
            var usadosB = new HashSet<int>();
            var quantos = 0;
            var vale = true;
            for (var i = 0; i < arestas.Count && vale; i++)
            {
                if ((mascara & (1 << i)) == 0) continue;
                if (!usadosA.Add(arestas[i].A) || !usadosB.Add(arestas[i].B)) vale = false;
                else quantos++;
            }
            if (vale) melhor = Math.Max(melhor, quantos);
        }
        return melhor;
    }

    /// <summary>Todo par devolvido tem de ser uma aresta que existe de verdade.</summary>
    private static void ConferirArestas(Bipartido g, Casamento c)
    {
        for (var a = 0; a < g.Esquerda; a++)
        {
            var b = c.ParDoPropoente(a);
            if (b == Casamento.Sozinho) continue;
            Assert.True(g.Ligados(a, b), $"o par ({a}, {b}) nao e aresta do grafo");
        }
    }

    /// <summary>
    /// Berge, 1957: o emparelhamento e maximo se e so se NAO existe caminho
    /// aumentante. Este e o criterio de parada, e aqui ele e conferido nos dois
    /// sentidos: depois de terminar nao pode existir caminho, e em qualquer
    /// emparelhamento menor tem de existir.
    /// </summary>
    [Fact]
    public void BergeValeNosDoisSentidos()
    {
        var aleatorio = new Random(1957);
        var testouMenor = 0;
        for (var caso = 0; caso < 500; caso++)
        {
            var g = Bipartido.Sorteado(5, 5, 0.3, caso + 1);
            var (maximo, _) = HopcroftKarp.Maximo(g);
            Assert.False(Berge.ExisteCaminhoAumentante(g, maximo),
                "achou caminho aumentante num emparelhamento que deveria ser maximo");

            // Tira um par: agora tem de existir caminho aumentante.
            if (maximo.Pares == 0) continue;
            for (var a = 0; a < g.Esquerda; a++)
            {
                if (maximo.PropoenteSozinho(a)) continue;
                var copia = Casamento.De(Enumerable.Range(0, maximo.Tamanho).Select(maximo.ParDoPropoente).ToList());
                copia.Separar(a);
                Assert.True(Berge.ExisteCaminhoAumentante(g, copia),
                    "nao achou caminho aumentante depois de desfazer um par");
                testouMenor++;
                break;
            }
            _ = aleatorio;
        }
        Assert.True(testouMenor > 300, $"so {testouMenor} casos exercitaram o lado do emparelhamento menor");
    }

    /// <summary>
    /// Konig, 1931: emparelhamento maximo e cobertura minima tem o mesmo tamanho.
    /// A cobertura construida precisa cobrir TODAS as arestas, e o tamanho dela
    /// tem de bater com o emparelhamento. Conferir so o tamanho deixaria passar
    /// uma cobertura que nao cobre.
    /// </summary>
    [Fact]
    public void AKoberturaDeKonigCobreETemOTamanhoCerto()
    {
        for (var caso = 0; caso < 600; caso++)
        {
            var g = Bipartido.Sorteado(6, 6, 0.3, caso + 1);
            var (c, _) = HopcroftKarp.Maximo(g);
            var (esquerda, direita) = HopcroftKarp.CoberturaMinima(g, c);

            foreach (var (a, b) in g.TodasAsArestas())
                Assert.True(esquerda.Contains(a) || direita.Contains(b),
                    $"a aresta ({a}, {b}) ficou descoberta");

            Assert.True(c.Pares == esquerda.Count + direita.Count,
                $"emparelhamento {c.Pares} e cobertura {esquerda.Count + direita.Count}");
        }
    }

    /// <summary>
    /// No grafo completo de n por n o emparelhamento e n, e o Hopcroft-Karp
    /// precisa de UMA fase so: todos os caminhos aumentantes tem comprimento um e
    /// sao colhidos de uma vez. E a diferenca para o metodo de um caminho por vez,
    /// que faria n buscas.
    /// </summary>
    [Theory]
    [InlineData(2)]
    [InlineData(10)]
    [InlineData(50)]
    [InlineData(200)]
    public void NoCompletoUmaFaseBasta(int n)
    {
        var g = Bipartido.Completo(n, n);
        var (c, fases) = HopcroftKarp.Maximo(g);
        Assert.Equal(n, c.Pares);
        Assert.Equal(1, fases);

        var (simples, buscas) = Berge.Maximo(g);
        Assert.Equal(n, simples.Pares);
        Assert.Equal(n, buscas);
    }

    /// <summary>
    /// Grafo sem aresta nenhuma: emparelhamento zero, nenhuma fase, e a cobertura
    /// minima e vazia. O caso de borda que um laco mal escrito transforma em laco
    /// infinito.
    /// </summary>
    [Fact]
    public void SemArestaNaoHaEmparelhamento()
    {
        var g = new Bipartido(5, 5);
        var (c, fases) = HopcroftKarp.Maximo(g);
        Assert.Equal(0, c.Pares);
        Assert.Equal(0, fases);
        var (esquerda, direita) = HopcroftKarp.CoberturaMinima(g, c);
        Assert.Empty(esquerda);
        Assert.Empty(direita);
    }
}
