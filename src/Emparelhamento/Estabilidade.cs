namespace Conde.Emparelhamento;

/// <summary>
/// Um par bloqueador: duas pessoas que nao estao juntas e que preferem uma a
/// outra aos pares que tem.
/// </summary>
public readonly record struct ParBloqueador(int Propoente, int Recebedor)
{
    public override string ToString() => $"({Propoente}, {Recebedor})";
}

/// <summary>
/// A definicao de estabilidade, aplicada ao pe da letra.
///
/// Um casamento e estavel quando nao existe par bloqueador. A definicao e
/// NEGATIVA, e isso importa para o teste: verificar estabilidade e procurar um
/// contraexemplo entre os n^2 pares, e nao seguir um algoritmo. Por isso esta
/// classe serve de juiz do Gale-Shapley sem saber nada sobre ele.
///
/// "Estavel" nao quer dizer justo, nem bom, nem otimo. Quer dizer so que ninguem
/// tem incentivo para romper por conta propria. Um casamento em que todos ficam
/// com a ultima opcao pode ser perfeitamente estavel.
/// </summary>
public static class Estabilidade
{
    /// <summary>
    /// O primeiro par bloqueador que existir, ou nulo. Varre os n^2 pares.
    /// </summary>
    public static ParBloqueador? Bloqueador(Casamento c, Preferencias propoem, Preferencias recebem)
    {
        ArgumentNullException.ThrowIfNull(c);
        ArgumentNullException.ThrowIfNull(propoem);
        ArgumentNullException.ThrowIfNull(recebem);
        var n = c.Tamanho;

        for (var p = 0; p < n; p++)
        {
            var parDeP = c.ParDoPropoente(p);
            for (var r = 0; r < n; r++)
            {
                if (parDeP == r) continue;
                // p prefere r ao par que tem (ou nao tem par nenhum)?
                var pQuer = parDeP == Casamento.Sozinho || propoem.Prefere(p, r, parDeP);
                if (!pQuer) continue;
                var parDeR = c.ParDoRecebedor(r);
                var rQuer = parDeR == Casamento.Sozinho || recebem.Prefere(r, p, parDeR);
                if (rQuer) return new ParBloqueador(p, r);
            }
        }
        return null;
    }

    /// <summary>Estavel quando nao ha par bloqueador nenhum.</summary>
    public static bool Estavel(Casamento c, Preferencias propoem, Preferencias recebem) =>
        Bloqueador(c, propoem, recebem) is null;

    /// <summary>
    /// Todos os pares bloqueadores. Serve para medir o quao longe da estabilidade
    /// um casamento esta, o que e mais informativo que um sim ou nao.
    /// </summary>
    public static List<ParBloqueador> TodosOsBloqueadores(Casamento c, Preferencias propoem, Preferencias recebem)
    {
        ArgumentNullException.ThrowIfNull(c);
        var saida = new List<ParBloqueador>();
        var n = c.Tamanho;
        for (var p = 0; p < n; p++)
        {
            var parDeP = c.ParDoPropoente(p);
            for (var r = 0; r < n; r++)
            {
                if (parDeP == r) continue;
                var pQuer = parDeP == Casamento.Sozinho || propoem.Prefere(p, r, parDeP);
                if (!pQuer) continue;
                var parDeR = c.ParDoRecebedor(r);
                var rQuer = parDeR == Casamento.Sozinho || recebem.Prefere(r, p, parDeR);
                if (rQuer) saida.Add(new ParBloqueador(p, r));
            }
        }
        return saida;
    }

    /// <summary>
    /// A soma dos postos de um lado. Quanto menor, melhor para esse lado. E o
    /// numero que mostra, sem retorica, quem leva vantagem no Gale-Shapley.
    /// </summary>
    public static long SomaDosPostos(Casamento c, Preferencias quemAvalia, bool ladoQuePropoe)
    {
        ArgumentNullException.ThrowIfNull(c);
        ArgumentNullException.ThrowIfNull(quemAvalia);
        long soma = 0;
        for (var i = 0; i < c.Tamanho; i++)
        {
            var par = ladoQuePropoe ? c.ParDoPropoente(i) : c.ParDoRecebedor(i);
            if (par == Casamento.Sozinho) continue;
            soma += quemAvalia.Posto(i, par);
        }
        return soma;
    }
}
