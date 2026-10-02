namespace Conde.Emparelhamento;

/// <summary>
/// Um casamento: cada um de um lado ligado a exatamente um do outro.
///
/// A estrutura guarda os dois sentidos e mantem os dois coerentes por construcao.
/// Guardar so um sentido e tentador e e onde o defeito nasce: a operacao que
/// troca um par precisa desfazer o anterior dos DOIS envolvidos, e com um vetor
/// so metade disso fica implicita.
/// </summary>
public sealed class Casamento
{
    private readonly int[] _parDoPropoente;
    private readonly int[] _parDoRecebedor;

    public const int Sozinho = -1;

    public Casamento(int tamanho)
    {
        if (tamanho < 0) throw new ArgumentOutOfRangeException(nameof(tamanho));
        Tamanho = tamanho;
        _parDoPropoente = new int[tamanho];
        _parDoRecebedor = new int[tamanho];
        Array.Fill(_parDoPropoente, Sozinho);
        Array.Fill(_parDoRecebedor, Sozinho);
    }

    public int Tamanho { get; }

    public int ParDoPropoente(int p) => _parDoPropoente[p];
    public int ParDoRecebedor(int r) => _parDoRecebedor[r];

    public bool PropoenteSozinho(int p) => _parDoPropoente[p] == Sozinho;
    public bool RecebedorSozinho(int r) => _parDoRecebedor[r] == Sozinho;

    /// <summary>Quantos pares existem.</summary>
    public int Pares
    {
        get
        {
            var n = 0;
            foreach (var x in _parDoPropoente) if (x != Sozinho) n++;
            return n;
        }
    }

    /// <summary>Todo mundo tem par.</summary>
    public bool Completo => Pares == Tamanho;

    /// <summary>
    /// Casa os dois, desfazendo os compromissos anteriores de cada um. Os dois
    /// sentidos saem coerentes, sempre.
    /// </summary>
    public void Casar(int propoente, int recebedor)
    {
        var antigoDoP = _parDoPropoente[propoente];
        if (antigoDoP != Sozinho) _parDoRecebedor[antigoDoP] = Sozinho;
        var antigoDoR = _parDoRecebedor[recebedor];
        if (antigoDoR != Sozinho) _parDoPropoente[antigoDoR] = Sozinho;
        _parDoPropoente[propoente] = recebedor;
        _parDoRecebedor[recebedor] = propoente;
    }

    public void Separar(int propoente)
    {
        var r = _parDoPropoente[propoente];
        if (r == Sozinho) return;
        _parDoPropoente[propoente] = Sozinho;
        _parDoRecebedor[r] = Sozinho;
    }

    /// <summary>
    /// Confere que os dois sentidos concordam. E a invariante da estrutura, e ela
    /// e conferida nos testes depois de cada algoritmo: um casamento incoerente
    /// continua respondendo a todas as perguntas, so que errado.
    /// </summary>
    public bool Coerente()
    {
        for (var p = 0; p < Tamanho; p++)
        {
            var r = _parDoPropoente[p];
            if (r == Sozinho) continue;
            if (_parDoRecebedor[r] != p) return false;
        }
        for (var r = 0; r < Tamanho; r++)
        {
            var p = _parDoRecebedor[r];
            if (p == Sozinho) continue;
            if (_parDoPropoente[p] != r) return false;
        }
        return true;
    }

    /// <summary>Monta a partir do vetor de pares do lado que propoe.</summary>
    public static Casamento De(IReadOnlyList<int> paresDoPropoente)
    {
        ArgumentNullException.ThrowIfNull(paresDoPropoente);
        var c = new Casamento(paresDoPropoente.Count);
        for (var p = 0; p < paresDoPropoente.Count; p++)
            if (paresDoPropoente[p] != Sozinho) c.Casar(p, paresDoPropoente[p]);
        return c;
    }

    /// <summary>Uma chave estavel para comparar e guardar em conjunto.</summary>
    public string Chave() => string.Join(",", _parDoPropoente);

    public override string ToString() => Chave();
}
