namespace Conde.Emparelhamento;

/// <summary>
/// Gale e Shapley, 1962: aceitacao diferida.
///
/// Quem esta sozinho propoe ao proximo da lista que ainda nao recusou. Quem
/// recebe compara com o compromisso atual e fica com o melhor, dispensando o
/// outro. Repete ate ninguem estar sozinho.
///
/// O "diferida" do nome e a peca inteira: quem recebe NUNCA aceita em definitivo,
/// so segura a melhor oferta ate aqui. E por isso que trocar nao desfaz nada do
/// que foi feito: cada troca melhora o lado que recebe, e cada recusa faz o lado
/// que propoe descer uma posicao na propria lista. Os dois lados so andam num
/// sentido, e e dai que vem a terminacao.
///
/// O limite e n^2 propostas, porque ninguem propoe duas vezes a mesma pessoa, e
/// com n propoentes e n nomes cada isso fecha. Nao e um limite frouxo: existem
/// instancias que chegam perto dele.
/// </summary>
public static class GaleShapley
{
    /// <summary>O casamento estavel, e quantas propostas foram feitas.</summary>
    public static (Casamento Casamento, long Propostas) Resolver(Preferencias propoem, Preferencias recebem)
    {
        ArgumentNullException.ThrowIfNull(propoem);
        ArgumentNullException.ThrowIfNull(recebem);
        if (propoem.Tamanho != recebem.Tamanho)
            throw new ArgumentException("os dois lados precisam ter o mesmo tamanho", nameof(recebem));

        var n = propoem.Tamanho;
        var casamento = new Casamento(n);
        // Ate onde cada propoente ja desceu na propria lista. So anda para frente.
        var proximo = new int[n];
        var sozinhos = new Stack<int>();
        for (var p = n - 1; p >= 0; p--) sozinhos.Push(p);
        long propostas = 0;

        while (sozinhos.Count > 0)
        {
            var p = sozinhos.Pop();
            if (!casamento.PropoenteSozinho(p)) continue;
            if (proximo[p] >= n)
                throw new InvalidOperationException($"{p} esgotou a lista; com listas completas isso nao acontece");

            var r = propoem.Preferido(p, proximo[p]++);
            propostas++;

            var atual = casamento.ParDoRecebedor(r);
            if (atual == Casamento.Sozinho)
            {
                casamento.Casar(p, r);
            }
            else if (recebem.Prefere(r, p, atual))
            {
                casamento.Casar(p, r);   // desfaz o anterior dos dois lados
                sozinhos.Push(atual);
            }
            else
            {
                sozinhos.Push(p);        // recusado: tenta o proximo da lista
            }
        }
        return (casamento, propostas);
    }

    /// <summary>
    /// O mesmo algoritmo com os papeis trocados: quem recebia passa a propor.
    ///
    /// Isso nao e um detalhe de uso, e a demonstracao do vies. O resultado tende a
    /// ser outro casamento, e quando e outro, cada lado prefere aquele em que ELE
    /// propoe. Quem propoe tem vantagem, sempre.
    /// </summary>
    public static (Casamento Casamento, long Propostas) ResolverTrocandoOsPapeis(Preferencias propoem, Preferencias recebem)
    {
        var (invertido, propostas) = Resolver(recebem, propoem);
        var certo = new Casamento(invertido.Tamanho);
        for (var r = 0; r < invertido.Tamanho; r++)
        {
            var p = invertido.ParDoPropoente(r);
            if (p != Casamento.Sozinho) certo.Casar(p, r);
        }
        return (certo, propostas);
    }
}
