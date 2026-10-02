namespace Conde.Emparelhamento;

/// <summary>
/// O algoritmo hungaro (Kuhn, 1955; Munkres, 1957): atribuicao de custo minimo.
///
/// Dada uma matriz n por n de custos, achar a permutacao que minimiza a soma. Por
/// forca bruta sao n! permutacoes; com 20 tarefas isso e 2,4 quintilhoes. O
/// hungaro resolve em O(n^3).
///
/// O nome e uma homenagem de Kuhn a Konig e Egervary, cujos trabalhos de 1931 ele
/// usou. Decadas depois descobriu-se que Jacobi ja tinha o metodo no seculo XIX,
/// em latim, num manuscrito publicado postumamente.
///
/// A ideia e dual e vale entender porque ela explica o algoritmo inteiro: manter
/// um POTENCIAL em cada linha e coluna com u[i] + v[j] &lt;= custo[i][j] sempre, e
/// so usar arestas em que vale a igualdade. Essas arestas formam um subgrafo, e
/// se existir emparelhamento perfeito nele, ele e otimo, porque a soma dos
/// potenciais e um limite inferior para qualquer atribuicao. Quando nao existe,
/// ajustam-se os potenciais de modo a trazer a proxima aresta mais barata para a
/// igualdade, sem nunca quebrar a desigualdade.
///
/// A implementacao abaixo e a versao por linhas com vetor de folga, que e a que
/// fica O(n^3) sem precisar montar o subgrafo explicitamente.
/// </summary>
public static class Hungaro
{
    /// <summary>
    /// A atribuicao de custo minimo. Devolve, para cada linha, a coluna
    /// escolhida, e o custo total.
    /// </summary>
    public static (int[] Colunas, long Custo) Resolver(long[,] custo)
    {
        ArgumentNullException.ThrowIfNull(custo);
        var n = custo.GetLength(0);
        if (custo.GetLength(1) != n) throw new ArgumentException("a matriz precisa ser quadrada", nameof(custo));
        if (n == 0) return ([], 0);

        // Indices base 1 por dentro: a posicao zero e a sentinela da busca, que e
        // o que deixa o laco sem caso especial para "ainda nao casou ninguem".
        var u = new long[n + 1];
        var v = new long[n + 1];
        var par = new int[n + 1];     // par[j] = linha casada com a coluna j
        var veioDe = new int[n + 1];

        for (var i = 1; i <= n; i++)
        {
            par[0] = i;
            var coluna = 0;
            var folga = new long[n + 1];
            Array.Fill(folga, long.MaxValue);
            var usada = new bool[n + 1];

            do
            {
                usada[coluna] = true;
                var linha = par[coluna];
                var melhor = long.MaxValue;
                var proxima = 0;

                for (var j = 1; j <= n; j++)
                {
                    if (usada[j]) continue;
                    var diferenca = custo[linha - 1, j - 1] - u[linha] - v[j];
                    if (diferenca < folga[j]) { folga[j] = diferenca; veioDe[j] = coluna; }
                    if (folga[j] < melhor) { melhor = folga[j]; proxima = j; }
                }

                // Ajusta os potenciais: as colunas ja usadas sobem, as livres
                // descem, e a desigualdade u + v <= custo continua valendo.
                for (var j = 0; j <= n; j++)
                {
                    if (usada[j]) { u[par[j]] += melhor; v[j] -= melhor; }
                    else folga[j] -= melhor;
                }
                coluna = proxima;
            } while (par[coluna] != 0);

            // Desfaz o caminho alternante, casando ao longo dele.
            do
            {
                var anterior = veioDe[coluna];
                par[coluna] = par[anterior];
                coluna = anterior;
            } while (coluna != 0);
        }

        var colunas = new int[n];
        long total = 0;
        for (var j = 1; j <= n; j++)
        {
            colunas[par[j] - 1] = j - 1;
            total += custo[par[j] - 1, j - 1];
        }
        return (colunas, total);
    }

    /// <summary>
    /// O juiz: enumera as n! permutacoes e fica com a mais barata. Sem potencial,
    /// sem caminho alternante, sem nada.
    /// </summary>
    public static long MelhorPorForcaBruta(long[,] custo)
    {
        ArgumentNullException.ThrowIfNull(custo);
        var n = custo.GetLength(0);
        if (n > 8) throw new ArgumentException("n! cresce rapido; o juiz vai ate 8", nameof(custo));
        var colunas = new int[n];
        var usada = new bool[n];
        var melhor = long.MaxValue;
        Buscar(0, 0);
        return n == 0 ? 0 : melhor;

        void Buscar(int linha, long acumulado)
        {
            if (linha == n) { melhor = Math.Min(melhor, acumulado); return; }
            for (var j = 0; j < n; j++)
            {
                if (usada[j]) continue;
                usada[j] = true; colunas[linha] = j;
                Buscar(linha + 1, acumulado + custo[linha, j]);
                usada[j] = false;
            }
        }
    }

    /// <summary>
    /// Maximizar em vez de minimizar: troca o sinal dos custos. Parece detalhe e
    /// e onde a conta vira o contrario sem avisar, porque o algoritmo continua
    /// devolvendo uma atribuicao valida.
    /// </summary>
    public static (int[] Colunas, long Valor) Maximizar(long[,] ganho)
    {
        ArgumentNullException.ThrowIfNull(ganho);
        var n = ganho.GetLength(0);
        var negativo = new long[n, n];
        for (var i = 0; i < n; i++)
            for (var j = 0; j < n; j++) negativo[i, j] = -ganho[i, j];
        var (colunas, custo) = Resolver(negativo);
        return (colunas, -custo);
    }
}
