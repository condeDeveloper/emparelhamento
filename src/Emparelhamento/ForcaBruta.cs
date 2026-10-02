namespace Conde.Emparelhamento;

/// <summary>
/// O juiz: enumera as n! atribuicoes possiveis e separa as estaveis pela
/// DEFINICAO, procurando par bloqueador.
///
/// Nenhum algoritmo de casamento e usado aqui. Isso importa porque o que se quer
/// verificar sao os teoremas de Gale-Shapley, e um juiz que tambem fosse um
/// algoritmo de casamento poderia errar junto.
///
/// Com o conjunto COMPLETO dos casamentos estaveis em maos, teoremas que
/// normalmente se aceitam de boa fe viram afirmacoes conferiveis:
///
/// - o casamento do Gale-Shapley esta no conjunto;
/// - ele e o MELHOR do conjunto para todo propoente, ao mesmo tempo;
/// - ele e o PIOR do conjunto para todo recebedor, ao mesmo tempo;
/// - o conjunto nunca e vazio, qualquer que seja a instancia.
/// </summary>
public static class ForcaBruta
{
    /// <summary>Acima disso o n! deixa de ser razoavel.</summary>
    public const int MaximoDeTamanho = 8;

    /// <summary>Todas as n! atribuicoes, estaveis ou nao.</summary>
    public static IEnumerable<Casamento> Todos(int n)
    {
        if (n < 0) throw new ArgumentOutOfRangeException(nameof(n));
        if (n > MaximoDeTamanho) throw new ArgumentOutOfRangeException(nameof(n), $"{n}! e demais; o juiz vai ate {MaximoDeTamanho}");
        var atual = new int[n];
        var usado = new bool[n];
        foreach (var _ in Gerar(0)) yield return Casamento.De(atual);

        IEnumerable<bool> Gerar(int p)
        {
            if (p == n) { yield return true; yield break; }
            for (var r = 0; r < n; r++)
            {
                if (usado[r]) continue;
                usado[r] = true; atual[p] = r;
                foreach (var x in Gerar(p + 1)) yield return x;
                usado[r] = false;
            }
        }
    }

    /// <summary>Todos os casamentos ESTAVEIS, pela definicao.</summary>
    public static List<Casamento> Estaveis(Preferencias propoem, Preferencias recebem)
    {
        ArgumentNullException.ThrowIfNull(propoem);
        var saida = new List<Casamento>();
        foreach (var c in Todos(propoem.Tamanho))
            if (Estabilidade.Estavel(c, propoem, recebem)) saida.Add(c);
        return saida;
    }

    /// <summary>
    /// Para cada propoente, o MELHOR par que ele consegue em algum casamento
    /// estavel. O teorema diz que existe um unico casamento estavel que da a
    /// todos eles esse melhor par ao mesmo tempo, o que nao e obvio: nada na
    /// definicao impede que o melhor de um exija o pior de outro.
    /// </summary>
    public static int[] MelhorParEstavelDeCadaPropoente(Preferencias propoem, Preferencias recebem)
    {
        var estaveis = Estaveis(propoem, recebem);
        var n = propoem.Tamanho;
        var melhor = new int[n];
        for (var p = 0; p < n; p++)
        {
            var melhorPosto = int.MaxValue;
            foreach (var c in estaveis)
            {
                var posto = propoem.Posto(p, c.ParDoPropoente(p));
                if (posto >= melhorPosto) continue;
                melhorPosto = posto;
                melhor[p] = c.ParDoPropoente(p);
            }
        }
        return melhor;
    }

    /// <summary>Para cada recebedor, o PIOR par que ele leva em algum casamento estavel.</summary>
    public static int[] PiorParEstavelDeCadaRecebedor(Preferencias propoem, Preferencias recebem)
    {
        var estaveis = Estaveis(propoem, recebem);
        var n = propoem.Tamanho;
        var pior = new int[n];
        for (var r = 0; r < n; r++)
        {
            var piorPosto = -1;
            foreach (var c in estaveis)
            {
                var posto = recebem.Posto(r, c.ParDoRecebedor(r));
                if (posto <= piorPosto) continue;
                piorPosto = posto;
                pior[r] = c.ParDoRecebedor(r);
            }
        }
        return pior;
    }

    /// <summary>n!, para o teste conferir que o dominio e o que eu digo que e.</summary>
    public static long Fatorial(int n)
    {
        long f = 1;
        for (var k = 2; k <= n; k++) f *= k;
        return f;
    }
}
