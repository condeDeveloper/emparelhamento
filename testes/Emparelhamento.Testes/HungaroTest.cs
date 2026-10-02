using Conde.Emparelhamento;
using Xunit;

namespace Conde.Emparelhamento.Testes;

/// <summary>
/// O algoritmo hungaro contra a enumeracao das n! permutacoes.
/// </summary>
public class HungaroTest
{
    /// <summary>
    /// 2.000 matrizes sorteadas de ate 7 por 7, cada uma com ate 5.040
    /// permutacoes enumeradas. O custo devolvido tem de bater com o minimo, e a
    /// atribuicao devolvida tem de ser uma permutacao de verdade: um custo certo
    /// com duas linhas na mesma coluna seria inutil.
    /// </summary>
    [Fact]
    public void OCustoBateComAEnumeracaoDePermutacoes()
    {
        var aleatorio = new Random(1955);
        for (var caso = 0; caso < 2000; caso++)
        {
            var n = 1 + aleatorio.Next(7);
            var custo = new long[n, n];
            for (var i = 0; i < n; i++)
                for (var j = 0; j < n; j++) custo[i, j] = aleatorio.Next(0, 50);

            var (colunas, total) = Hungaro.Resolver(custo);
            Assert.Equal(Hungaro.MelhorPorForcaBruta(custo), total);

            Assert.Equal(n, colunas.Distinct().Count());
            long conferindo = 0;
            for (var i = 0; i < n; i++) conferindo += custo[i, colunas[i]];
            Assert.Equal(total, conferindo);
        }
    }

    /// <summary>
    /// Custos negativos tem de funcionar igual: o algoritmo trabalha com
    /// diferencas, nao com valores absolutos. Somar uma constante a TODA a matriz
    /// soma n vezes essa constante ao total e nao muda a escolha, e essa
    /// invariante e o que mostra que nao ha hipotese escondida de nao negatividade.
    /// </summary>
    [Fact]
    public void SomarConstanteNaoMudaAEscolha()
    {
        var aleatorio = new Random(7);
        for (var caso = 0; caso < 500; caso++)
        {
            const int n = 5;
            var custo = new long[n, n];
            for (var i = 0; i < n; i++)
                for (var j = 0; j < n; j++) custo[i, j] = aleatorio.Next(-40, 40);

            var (colunas, total) = Hungaro.Resolver(custo);
            Assert.Equal(Hungaro.MelhorPorForcaBruta(custo), total);

            const long constante = 1000;
            var deslocado = new long[n, n];
            for (var i = 0; i < n; i++)
                for (var j = 0; j < n; j++) deslocado[i, j] = custo[i, j] + constante;

            var (outrasColunas, outroTotal) = Hungaro.Resolver(deslocado);
            Assert.Equal(total + n * constante, outroTotal);
            long mesmoCusto = 0;
            for (var i = 0; i < n; i++) mesmoCusto += custo[i, outrasColunas[i]];
            Assert.Equal(total, mesmoCusto);
            _ = colunas;
        }
    }

    /// <summary>
    /// Maximizar e minimizar o negativo, e trocar isso de lugar e o erro que
    /// devolve uma atribuicao perfeitamente valida e perfeitamente errada. O juiz
    /// aqui e a enumeracao de novo, procurando o MAIOR.
    /// </summary>
    [Fact]
    public void MaximizarBateComOMaiorDaEnumeracao()
    {
        var aleatorio = new Random(11);
        for (var caso = 0; caso < 500; caso++)
        {
            const int n = 6;
            var ganho = new long[n, n];
            var negativo = new long[n, n];
            for (var i = 0; i < n; i++)
                for (var j = 0; j < n; j++) { ganho[i, j] = aleatorio.Next(0, 80); negativo[i, j] = -ganho[i, j]; }

            var (_, valor) = Hungaro.Maximizar(ganho);
            Assert.Equal(-Hungaro.MelhorPorForcaBruta(negativo), valor);
        }
    }

    /// <summary>
    /// Matriz em que uma linha e muito mais barata em TODAS as colunas: a escolha
    /// dela nao pode depender disso, porque o que conta sao as diferencas. Caso
    /// construido para pegar implementacao que escolhe por minimo de linha.
    /// </summary>
    [Fact]
    public void UmaLinhaBarataNaoDecideSozinha()
    {
        long[,] custo =
        {
            { 1, 2, 3 },
            { 100, 200, 300 },
            { 1000, 2000, 3000 },
        };
        var (colunas, total) = Hungaro.Resolver(custo);
        Assert.Equal(Hungaro.MelhorPorForcaBruta(custo), total);
        // O otimo e a diagonal inversa: 3 + 200 + 1000 = 1203.
        Assert.Equal(1203, total);
        Assert.Equal([2, 1, 0], colunas);
    }

    /// <summary>
    /// Matriz de uma linha so, e matriz vazia. Os dois casos de borda em que um
    /// laco base um costuma estourar.
    /// </summary>
    [Fact]
    public void OsCasosDeBordaNaoEstouram()
    {
        var (colunas, total) = Hungaro.Resolver(new long[,] { { 42 } });
        Assert.Equal([0], colunas);
        Assert.Equal(42, total);

        var (vazio, zero) = Hungaro.Resolver(new long[0, 0]);
        Assert.Empty(vazio);
        Assert.Equal(0, zero);
    }

    /// <summary>
    /// A matriz precisa ser quadrada. Com matriz retangular o problema continua
    /// fazendo sentido, mas exige preencher com custo zero, e fazer isso em
    /// silencio esconderia do usuario o que o algoritmo esta resolvendo.
    /// </summary>
    [Fact]
    public void MatrizRetangularNaoPassaCalada()
    {
        Assert.Throws<ArgumentException>(() => Hungaro.Resolver(new long[2, 3]));
    }

    /// <summary>
    /// Em n = 8 a enumeracao ja sao 40.320 permutacoes e o hungaro responde na
    /// hora. A tabela do README sai desta diferenca.
    /// </summary>
    [Fact]
    public void ConcordaNoMaiorTamanhoQueOJuizAlcanca()
    {
        var aleatorio = new Random(8);
        for (var caso = 0; caso < 60; caso++)
        {
            const int n = 8;
            var custo = new long[n, n];
            for (var i = 0; i < n; i++)
                for (var j = 0; j < n; j++) custo[i, j] = aleatorio.Next(0, 100);
            var (_, total) = Hungaro.Resolver(custo);
            Assert.Equal(Hungaro.MelhorPorForcaBruta(custo), total);
        }
    }
}
