namespace Conde.Emparelhamento;

/// <summary>
/// As listas de preferencia dos dois lados.
///
/// Cada participante ordena TODOS os do outro lado, do mais desejado ao menos. A
/// lista completa e exigencia do problema classico, nao comodidade: com listas
/// incompletas a existencia de casamento estavel continua garantida, mas o
/// casamento deixa de ser perfeito e varios enunciados mudam de forma.
///
/// Alem da lista, guarda-se o POSTO: dado um participante e um candidato, em que
/// posicao da lista ele esta. A lista responde "quem e o k-esimo" e o posto
/// responde "em que lugar esta fulano", e a segunda pergunta e a que o algoritmo
/// faz o tempo todo. Sem o posto, comparar dois candidatos custaria uma varredura
/// e o Gale-Shapley sairia de O(n^2) para O(n^3) sem que nada quebrasse.
/// </summary>
public sealed class Preferencias
{
    private readonly int[][] _lista;
    private readonly int[][] _posto;

    public Preferencias(int[][] listas)
    {
        ArgumentNullException.ThrowIfNull(listas);
        var n = listas.Length;
        _lista = new int[n][];
        _posto = new int[n][];
        for (var i = 0; i < n; i++)
        {
            var linha = listas[i];
            ArgumentNullException.ThrowIfNull(linha);
            if (linha.Length != n)
                throw new ArgumentException($"a lista de {i} tem {linha.Length} nomes e deveria ter {n}", nameof(listas));
            _lista[i] = (int[])linha.Clone();
            _posto[i] = new int[n];
            Array.Fill(_posto[i], -1);
            foreach (var quem in linha)
            {
                if (quem < 0 || quem >= n) throw new ArgumentException($"{quem} fora da faixa na lista de {i}", nameof(listas));
                if (_posto[i][quem] != -1) throw new ArgumentException($"{quem} aparece duas vezes na lista de {i}", nameof(listas));
                _posto[i][quem] = Array.IndexOf(linha, quem);
            }
            for (var k = 0; k < n; k++)
                if (_posto[i][k] == -1) throw new ArgumentException($"a lista de {i} nao cita {k}", nameof(listas));
        }
        Tamanho = n;
    }

    /// <summary>Quantos participantes tem cada lado.</summary>
    public int Tamanho { get; }

    /// <summary>O k-esimo preferido de quem (k comeca em zero).</summary>
    public int Preferido(int quem, int k) => _lista[quem][k];

    /// <summary>A lista inteira de quem, em ordem.</summary>
    public IReadOnlyList<int> Lista(int quem) => _lista[quem];

    /// <summary>Em que posicao da lista de quem esta o candidato. Menor e melhor.</summary>
    public int Posto(int quem, int candidato) => _posto[quem][candidato];

    /// <summary>Verdadeiro quando quem prefere a a b.</summary>
    public bool Prefere(int quem, int a, int b) => _posto[quem][a] < _posto[quem][b];

    /// <summary>Todos preferindo na mesma ordem: 0, 1, 2... Util como caso de borda.</summary>
    public static Preferencias Identicas(int n)
    {
        var listas = new int[n][];
        var ordem = new int[n];
        for (var k = 0; k < n; k++) ordem[k] = k;
        for (var i = 0; i < n; i++) listas[i] = (int[])ordem.Clone();
        return new Preferencias(listas);
    }

    /// <summary>Listas sorteadas, para medir e para a comparacao exaustiva.</summary>
    public static (Preferencias Propoem, Preferencias Recebem) Sorteadas(int n, int semente)
    {
        var r = new Random(semente);
        return (Sortear(n, r), Sortear(n, r));
    }

    private static Preferencias Sortear(int n, Random r)
    {
        var listas = new int[n][];
        for (var i = 0; i < n; i++)
        {
            var ordem = new int[n];
            for (var k = 0; k < n; k++) ordem[k] = k;
            for (var k = n - 1; k > 0; k--)
            {
                var j = r.Next(k + 1);
                (ordem[k], ordem[j]) = (ordem[j], ordem[k]);
            }
            listas[i] = ordem;
        }
        return new Preferencias(listas);
    }
}
