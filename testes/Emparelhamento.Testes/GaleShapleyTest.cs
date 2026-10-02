using Conde.Emparelhamento;
using Xunit;

namespace Conde.Emparelhamento.Testes;

/// <summary>
/// Os teoremas de Gale-Shapley contra o conjunto COMPLETO dos casamentos
/// estaveis, obtido por enumeracao das n! atribuicoes.
///
/// Com o conjunto inteiro em maos, afirmacoes que normalmente se aceitam de boa
/// fe viram conferiveis. O juiz nao sabe o que e Gale-Shapley: ele so sabe a
/// definicao de par bloqueador.
/// </summary>
public class GaleShapleyTest
{
    /// <summary>
    /// O dominio e o que eu digo que e: 5! = 120 atribuicoes por instancia, e
    /// pelo menos uma delas e estavel, sempre. A segunda parte e o teorema de
    /// existencia, que vale para TODA instancia e nao so para as sorteadas.
    /// </summary>
    [Fact]
    public void SempreExisteCasamentoEstavel()
    {
        const int n = 5;
        Assert.Equal(120, ForcaBruta.Fatorial(n));
        Assert.Equal(120, ForcaBruta.Todos(n).Count());

        for (var semente = 1; semente <= 500; semente++)
        {
            var (propoem, recebem) = Preferencias.Sorteadas(n, semente);
            var estaveis = ForcaBruta.Estaveis(propoem, recebem);
            Assert.True(estaveis.Count > 0, $"a instancia {semente} nao teve casamento estavel nenhum");
        }
    }

    /// <summary>
    /// O casamento do Gale-Shapley esta no conjunto dos estaveis, e e coerente.
    /// Conferido em 1.000 instancias de 5 e em 300 de 6, onde o juiz enumera 720.
    /// </summary>
    [Fact]
    public void OResultadoEhEstavelEEstaNoConjunto()
    {
        foreach (var (n, quantas) in new[] { (5, 1000), (6, 300) })
        {
            for (var semente = 1; semente <= quantas; semente++)
            {
                var (propoem, recebem) = Preferencias.Sorteadas(n, semente);
                var (c, _) = GaleShapley.Resolver(propoem, recebem);

                Assert.True(c.Coerente(), $"casamento incoerente na instancia {semente}");
                Assert.True(c.Completo, $"alguem ficou sozinho na instancia {semente}");
                Assert.True(Estabilidade.Estavel(c, propoem, recebem),
                    $"instavel na instancia {semente}: {Estabilidade.Bloqueador(c, propoem, recebem)}");

                var chaves = ForcaBruta.Estaveis(propoem, recebem).Select(x => x.Chave()).ToHashSet();
                Assert.Contains(c.Chave(), chaves);
            }
        }
    }

    /// <summary>
    /// O teorema que nao e obvio: o Gale-Shapley da a CADA propoente o melhor par
    /// que ele consegue em QUALQUER casamento estavel, e da todos ao mesmo tempo.
    ///
    /// Nada na definicao impede que o melhor de um exija o pior de outro, e e por
    /// isso que isto precisa ser conferido em vez de assumido: o juiz calcula, por
    /// propoente, o melhor par entre todos os casamentos estaveis, e o resultado
    /// tem de bater nos n ao mesmo tempo.
    /// </summary>
    [Fact]
    public void QuemPropoeLevaOMelhorParEstavelPossivel()
    {
        const int n = 5;
        for (var semente = 1; semente <= 600; semente++)
        {
            var (propoem, recebem) = Preferencias.Sorteadas(n, semente);
            var (c, _) = GaleShapley.Resolver(propoem, recebem);
            var melhor = ForcaBruta.MelhorParEstavelDeCadaPropoente(propoem, recebem);
            for (var p = 0; p < n; p++)
                Assert.True(c.ParDoPropoente(p) == melhor[p],
                    $"o propoente {p} levou {c.ParDoPropoente(p)} e o melhor estavel dele e {melhor[p]} (instancia {semente})");
        }
    }

    /// <summary>
    /// A outra face do mesmo teorema, e a menos divulgada: o mesmo casamento da a
    /// CADA recebedor o PIOR par que ele teria em qualquer casamento estavel.
    ///
    /// Quem propoe nao ganha um pouco, ganha o maximo; quem recebe nao perde um
    /// pouco, perde o maximo. Isso e o que faz a escolha de quem propoe, num
    /// sistema real de alocacao, ser uma decisao de politica e nao de engenharia.
    /// </summary>
    [Fact]
    public void QuemRecebeLevaOPiorParEstavelPossivel()
    {
        const int n = 5;
        for (var semente = 1; semente <= 600; semente++)
        {
            var (propoem, recebem) = Preferencias.Sorteadas(n, semente);
            var (c, _) = GaleShapley.Resolver(propoem, recebem);
            var pior = ForcaBruta.PiorParEstavelDeCadaRecebedor(propoem, recebem);
            for (var r = 0; r < n; r++)
                Assert.True(c.ParDoRecebedor(r) == pior[r],
                    $"o recebedor {r} levou {c.ParDoRecebedor(r)} e o pior estavel dele e {pior[r]} (instancia {semente})");
        }
    }

    /// <summary>
    /// Trocar os papeis troca o resultado a favor de quem passou a propor, e o
    /// teste mede isso: a soma dos postos do lado que propoe nunca piora quando
    /// ele e quem propoe. Nunca, em instancia nenhuma.
    /// </summary>
    [Fact]
    public void TrocarOsPapeisTrocaQuemLevaVantagem()
    {
        const int n = 6;
        var diferentes = 0;
        for (var semente = 1; semente <= 500; semente++)
        {
            var (propoem, recebem) = Preferencias.Sorteadas(n, semente);
            var (a, _) = GaleShapley.Resolver(propoem, recebem);
            var (b, _) = GaleShapley.ResolverTrocandoOsPapeis(propoem, recebem);

            Assert.True(Estabilidade.Estavel(b, propoem, recebem), "o invertido saiu instavel");

            var ladoEsquerdoQuandoPropoe = Estabilidade.SomaDosPostos(a, propoem, true);
            var ladoEsquerdoQuandoRecebe = Estabilidade.SomaDosPostos(b, propoem, true);
            var ladoDireitoQuandoRecebe = Estabilidade.SomaDosPostos(a, recebem, false);
            var ladoDireitoQuandoPropoe = Estabilidade.SomaDosPostos(b, recebem, false);

            Assert.True(ladoEsquerdoQuandoPropoe <= ladoEsquerdoQuandoRecebe);
            Assert.True(ladoDireitoQuandoPropoe <= ladoDireitoQuandoRecebe);
            if (a.Chave() != b.Chave()) diferentes++;
        }
        Assert.True(diferentes > 0, "nenhuma instancia deu resultados diferentes; o teste nao exercitou nada");
    }

    /// <summary>
    /// Com todos preferindo a mesma ordem, existe exatamente UM casamento estavel
    /// e e o que casa i com i. Nesse caso nao ha vies nenhum: os dois sentidos do
    /// Gale-Shapley dao o mesmo.
    /// </summary>
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(5)]
    [InlineData(7)]
    public void ComPreferenciasIdenticasSoHaUmEstavel(int n)
    {
        var iguais = Preferencias.Identicas(n);
        var estaveis = ForcaBruta.Estaveis(iguais, iguais);
        Assert.Single(estaveis);

        var (c, _) = GaleShapley.Resolver(iguais, iguais);
        for (var i = 0; i < n; i++) Assert.Equal(i, c.ParDoPropoente(i));

        var (invertido, _) = GaleShapley.ResolverTrocandoOsPapeis(iguais, iguais);
        Assert.Equal(c.Chave(), invertido.Chave());
    }

    /// <summary>
    /// O limite de n^2 propostas nao e frouxo: o algoritmo nunca passa dele, e em
    /// instancia construida ele chega perto. Ninguem propoe duas vezes a mesma
    /// pessoa, que e de onde o limite sai.
    /// </summary>
    [Fact]
    public void NuncaPassaDeNAoQuadradoPropostas()
    {
        foreach (var n in new[] { 4, 6, 8, 20, 50 })
        {
            for (var semente = 1; semente <= 50; semente++)
            {
                var (propoem, recebem) = Preferencias.Sorteadas(n, semente);
                var (_, propostas) = GaleShapley.Resolver(propoem, recebem);
                Assert.InRange(propostas, n, (long)n * n);
            }
        }
    }
}
