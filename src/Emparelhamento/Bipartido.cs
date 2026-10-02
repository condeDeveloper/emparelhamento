namespace Conde.Emparelhamento;

/// <summary>
/// Um grafo bipartido simples: dois lados e as arestas entre eles.
///
/// Aqui nao ha preferencia nenhuma. A pergunta e outra, e mais crua: quantos
/// pares cabem ao mesmo tempo. E o problema que aparece em escala de operacao,
/// nao em teoria dos jogos: tarefas e maquinas, entregas e motoboys, cadeiras e
/// candidatos.
/// </summary>
public sealed class Bipartido
{
    private readonly List<int>[] _vizinhos;

    public Bipartido(int esquerda, int direita)
    {
        if (esquerda < 0) throw new ArgumentOutOfRangeException(nameof(esquerda));
        if (direita < 0) throw new ArgumentOutOfRangeException(nameof(direita));
        Esquerda = esquerda;
        Direita = direita;
        _vizinhos = new List<int>[esquerda];
        for (var a = 0; a < esquerda; a++) _vizinhos[a] = [];
    }

    public int Esquerda { get; }
    public int Direita { get; }

    public int Arestas
    {
        get
        {
            var n = 0;
            foreach (var lista in _vizinhos) n += lista.Count;
            return n;
        }
    }

    public void Ligar(int a, int b)
    {
        if (a < 0 || a >= Esquerda) throw new ArgumentOutOfRangeException(nameof(a));
        if (b < 0 || b >= Direita) throw new ArgumentOutOfRangeException(nameof(b));
        if (_vizinhos[a].Contains(b)) return;
        _vizinhos[a].Add(b);
    }

    public IReadOnlyList<int> Vizinhos(int a) => _vizinhos[a];

    public bool Ligados(int a, int b) => _vizinhos[a].Contains(b);

    /// <summary>Todas as arestas, como pares.</summary>
    public IEnumerable<(int A, int B)> TodasAsArestas()
    {
        for (var a = 0; a < Esquerda; a++)
            foreach (var b in _vizinhos[a]) yield return (a, b);
    }

    public static Bipartido Sorteado(int esquerda, int direita, double densidade, int semente)
    {
        if (densidade is < 0 or > 1) throw new ArgumentOutOfRangeException(nameof(densidade));
        var r = new Random(semente);
        var g = new Bipartido(esquerda, direita);
        for (var a = 0; a < esquerda; a++)
            for (var b = 0; b < direita; b++)
                if (r.NextDouble() < densidade) g.Ligar(a, b);
        return g;
    }

    /// <summary>Todo mundo ligado a todo mundo.</summary>
    public static Bipartido Completo(int esquerda, int direita)
    {
        var g = new Bipartido(esquerda, direita);
        for (var a = 0; a < esquerda; a++)
            for (var b = 0; b < direita; b++) g.Ligar(a, b);
        return g;
    }
}
