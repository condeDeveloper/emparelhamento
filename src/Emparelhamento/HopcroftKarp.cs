namespace Conde.Emparelhamento;

/// <summary>
/// Hopcroft e Karp, 1973: em vez de um caminho aumentante por vez, um conjunto
/// MAXIMAL de caminhos mais curtos e disjuntos de cada vez.
///
/// O algoritmo faz fases. Cada fase comeca com uma busca em largura que calcula a
/// distancia de cada vertice livre da esquerda ate um livre da direita, pelo
/// caminho alternante, e depois uma busca em profundidade colhe todos os caminhos
/// daquele comprimento que nao se cruzam.
///
/// A conta que faz o custo cair para O(E * raiz(V)) tem duas metades:
///
/// - O comprimento do caminho aumentante mais curto CRESCE a cada fase. Se ele
///   nao crescesse, os caminhos colhidos nao seriam maximais.
/// - Depois de raiz(V) fases, o caminho mais curto ja tem comprimento raiz(V), e
///   a diferenca entre o emparelhamento atual e o maximo e no maximo raiz(V),
///   porque a diferenca simetrica entre os dois e feita de caminhos disjuntos e
///   cada um custa pelo menos raiz(V) vertices.
///
/// Entao sao raiz(V) fases para chegar perto e raiz(V) caminhos para fechar: no
/// total, raiz(V) fases de custo O(E) cada.
///
/// E, no fundo, o Dinic rodando numa rede de capacidade um.
/// </summary>
public static class HopcroftKarp
{
    private const int Infinito = int.MaxValue;

    /// <summary>O emparelhamento maximo, e quantas FASES foram usadas.</summary>
    public static (Casamento Casamento, long Fases) Maximo(Bipartido g)
    {
        ArgumentNullException.ThrowIfNull(g);
        var parDaEsquerda = new int[g.Esquerda];
        var parDaDireita = new int[g.Direita];
        Array.Fill(parDaEsquerda, Casamento.Sozinho);
        Array.Fill(parDaDireita, Casamento.Sozinho);
        var distancia = new int[g.Esquerda];
        long fases = 0;

        while (Niveis(g, parDaEsquerda, parDaDireita, distancia))
        {
            fases++;
            for (var a = 0; a < g.Esquerda; a++)
                if (parDaEsquerda[a] == Casamento.Sozinho)
                    Colher(g, a, parDaEsquerda, parDaDireita, distancia);
        }

        var c = new Casamento(Math.Max(g.Esquerda, g.Direita));
        for (var a = 0; a < g.Esquerda; a++)
            if (parDaEsquerda[a] != Casamento.Sozinho) c.Casar(a, parDaEsquerda[a]);
        return (c, fases);
    }

    /// <summary>
    /// Busca em largura a partir de TODOS os livres da esquerda ao mesmo tempo.
    /// Devolve falso quando nenhum livre da direita e alcancavel: fim.
    /// </summary>
    private static bool Niveis(Bipartido g, int[] parDaEsquerda, int[] parDaDireita, int[] distancia)
    {
        var fila = new Queue<int>();
        for (var a = 0; a < g.Esquerda; a++)
        {
            if (parDaEsquerda[a] == Casamento.Sozinho) { distancia[a] = 0; fila.Enqueue(a); }
            else distancia[a] = Infinito;
        }
        var achou = false;
        while (fila.Count > 0)
        {
            var a = fila.Dequeue();
            foreach (var b in g.Vizinhos(a))
            {
                var outro = parDaDireita[b];
                if (outro == Casamento.Sozinho) { achou = true; continue; }
                if (distancia[outro] != Infinito) continue;
                distancia[outro] = distancia[a] + 1;
                fila.Enqueue(outro);
            }
        }
        return achou;
    }

    /// <summary>
    /// Desce colhendo caminhos do comprimento da fase. A condicao de nivel e o
    /// que garante que os caminhos sao os mais curtos e disjuntos; sem ela o
    /// algoritmo continua CERTO e vira o de um caminho por vez, que e o jeito
    /// silencioso de perder a complexidade.
    /// </summary>
    private static bool Colher(Bipartido g, int a, int[] parDaEsquerda, int[] parDaDireita, int[] distancia)
    {
        foreach (var b in g.Vizinhos(a))
        {
            var outro = parDaDireita[b];
            var podeDescer = outro == Casamento.Sozinho ||
                             (distancia[outro] == distancia[a] + 1 && Colher(g, outro, parDaEsquerda, parDaDireita, distancia));
            if (!podeDescer) continue;
            parDaDireita[b] = a;
            parDaEsquerda[a] = b;
            return true;
        }
        distancia[a] = Infinito;   // beco sem saida: nao visitar de novo nesta fase
        return false;
    }

    /// <summary>
    /// A cobertura minima por vertices, pelo teorema de Konig (1931), construida
    /// a partir do emparelhamento maximo.
    ///
    /// A receita: marque os livres da esquerda e tudo alcancavel por caminho
    /// alternante. A cobertura e (esquerda NAO marcada) uniao (direita marcada). O
    /// tamanho dela e sempre igual ao do emparelhamento, e isso e o teorema.
    /// </summary>
    public static (HashSet<int> Esquerda, HashSet<int> Direita) CoberturaMinima(Bipartido g, Casamento c)
    {
        ArgumentNullException.ThrowIfNull(g);
        ArgumentNullException.ThrowIfNull(c);
        var marcadaEsquerda = new bool[g.Esquerda];
        var marcadaDireita = new bool[g.Direita];
        var fila = new Queue<int>();
        for (var a = 0; a < g.Esquerda; a++)
            if (c.PropoenteSozinho(a)) { marcadaEsquerda[a] = true; fila.Enqueue(a); }

        while (fila.Count > 0)
        {
            var a = fila.Dequeue();
            foreach (var b in g.Vizinhos(a))
            {
                if (c.ParDoPropoente(a) == b) continue;   // so por aresta de FORA
                if (marcadaDireita[b]) continue;
                marcadaDireita[b] = true;
                var outro = c.ParDoRecebedor(b);
                if (outro == Casamento.Sozinho || marcadaEsquerda[outro]) continue;
                marcadaEsquerda[outro] = true;
                fila.Enqueue(outro);
            }
        }

        var esquerda = new HashSet<int>();
        var direita = new HashSet<int>();
        for (var a = 0; a < g.Esquerda; a++) if (!marcadaEsquerda[a]) esquerda.Add(a);
        for (var b = 0; b < g.Direita; b++) if (marcadaDireita[b]) direita.Add(b);
        return (esquerda, direita);
    }
}
