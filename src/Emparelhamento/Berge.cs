namespace Conde.Emparelhamento;

/// <summary>
/// Berge, 1957: um emparelhamento e MAXIMO se e so se nao existe caminho
/// aumentante.
///
/// Um caminho aumentante comeca e termina em vertices livres e alterna aresta de
/// fora, aresta de dentro, aresta de fora. Como ele comeca e termina fora, tem
/// uma aresta de fora a mais que de dentro: inverter o papel de todas aumenta o
/// emparelhamento em exatamente um.
///
/// Esse teorema e o que da o criterio de PARADA. Sem ele, um algoritmo guloso nao
/// tem como saber se o que achou e o maximo, e a resposta ficaria dependendo da
/// ordem em que as arestas foram olhadas. E ele tambem da a receita: enquanto
/// houver caminho aumentante, melhore; quando nao houver, acabou, e nao e preciso
/// olhar mais nada.
/// </summary>
public static class Berge
{
    /// <summary>
    /// Procura um caminho aumentante a partir de um vertice livre da esquerda, e
    /// se achar, ja inverte. Devolve verdadeiro quando o emparelhamento cresceu.
    ///
    /// Esta e a busca "hungara" classica, em profundidade. O vetor de visitados e
    /// por CHAMADA e nao global: um vertice da direita descartado numa tentativa
    /// pode servir em outra.
    /// </summary>
    public static bool Aumentar(Bipartido g, int a, int[] parDaDireita, bool[] visitado)
    {
        ArgumentNullException.ThrowIfNull(g);
        ArgumentNullException.ThrowIfNull(parDaDireita);
        ArgumentNullException.ThrowIfNull(visitado);
        foreach (var b in g.Vizinhos(a))
        {
            if (visitado[b]) continue;
            visitado[b] = true;
            if (parDaDireita[b] == Casamento.Sozinho || Aumentar(g, parDaDireita[b], parDaDireita, visitado))
            {
                parDaDireita[b] = a;
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Emparelhamento maximo pelo metodo do caminho aumentante, um de cada vez.
    /// Custa O(V*E), que e a versao simples do Hopcroft-Karp.
    /// </summary>
    public static (Casamento Casamento, long Buscas) Maximo(Bipartido g)
    {
        ArgumentNullException.ThrowIfNull(g);
        var parDaDireita = new int[g.Direita];
        Array.Fill(parDaDireita, Casamento.Sozinho);
        long buscas = 0;

        for (var a = 0; a < g.Esquerda; a++)
        {
            var visitado = new bool[g.Direita];
            buscas++;
            Aumentar(g, a, parDaDireita, visitado);
        }
        return (Montar(g, parDaDireita), buscas);
    }

    /// <summary>
    /// Procura um caminho aumentante SEM alterar nada. E o criterio de Berge
    /// usado como pergunta: existe ou nao existe.
    ///
    /// Serve para o teste conferir que o algoritmo parou no lugar certo, e nao
    /// so que ele parou.
    /// </summary>
    public static bool ExisteCaminhoAumentante(Bipartido g, Casamento c)
    {
        ArgumentNullException.ThrowIfNull(g);
        ArgumentNullException.ThrowIfNull(c);
        var parDaDireita = new int[g.Direita];
        Array.Fill(parDaDireita, Casamento.Sozinho);
        for (var b = 0; b < g.Direita; b++) parDaDireita[b] = c.ParDoRecebedor(b);

        for (var a = 0; a < g.Esquerda; a++)
        {
            if (!c.PropoenteSozinho(a)) continue;
            var copia = (int[])parDaDireita.Clone();
            var visitado = new bool[g.Direita];
            if (Aumentar(g, a, copia, visitado)) return true;
        }
        return false;
    }

    internal static Casamento Montar(Bipartido g, int[] parDaDireita)
    {
        var c = new Casamento(Math.Max(g.Esquerda, g.Direita));
        for (var b = 0; b < g.Direita; b++)
            if (parDaDireita[b] != Casamento.Sozinho) c.Casar(parDaDireita[b], b);
        return c;
    }
}
