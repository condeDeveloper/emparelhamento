using System.Diagnostics;
using System.Globalization;
using Conde.Emparelhamento;

// A ferramenta RODA para registrar numeros. Nao ha asseveracao de tempo: os
// numeros que entram no README saem daqui, nunca da minha cabeca.

// Com InvariantGlobalization a cultura pt-BR nao existe e lancaria. O formato vai
// montado na mao, que e o padrao ja adotado nos outros projetos.
var br = new NumberFormatInfo { NumberDecimalSeparator = ",", NumberGroupSeparator = "." };
string N(long x) => x.ToString("N0", br);
string D(double x, int casas = 2) => x.ToString("N" + casas, br);

var comando = args.Length > 0 ? args[0] : "tudo";
switch (comando)
{
    case "vies": Vies(); break;
    case "quantos": Quantos(); break;
    case "fases": Fases(); break;
    case "hungaro": Hungaro(); break;
    default: Vies(); Console.WriteLine(); Quantos(); Console.WriteLine(); Fases(); Console.WriteLine(); Hungaro(); break;
}

return;

// O vies de quem propoe, em numero. A soma dos postos mede o quao bem cada lado
// saiu: zero seria todo mundo com a primeira opcao.
void Vies()
{
    Console.WriteLine("o vies de quem propoe (500 instancias por tamanho)");
    Console.WriteLine();
    Console.WriteLine("    n   posto medio de quem propoe   de quem recebe   razao");
    foreach (var n in new[] { 5, 10, 50, 200, 1000 })
    {
        const int amostras = 500;
        long somaPropoe = 0, somaRecebe = 0;
        for (var s = 1; s <= amostras; s++)
        {
            var (propoem, recebem) = Preferencias.Sorteadas(n, s);
            var (c, _) = GaleShapley.Resolver(propoem, recebem);
            somaPropoe += Estabilidade.SomaDosPostos(c, propoem, true);
            somaRecebe += Estabilidade.SomaDosPostos(c, recebem, false);
        }
        var mediaP = (double)somaPropoe / amostras / n;
        var mediaR = (double)somaRecebe / amostras / n;
        Console.WriteLine($"  {N(n),3}   {D(mediaP),26}   {D(mediaR),14}   {D(mediaR / Math.Max(mediaP, 1e-9), 1),5}x");
    }
    Console.WriteLine();
    Console.WriteLine("  O posto e base zero: 0 e a primeira opcao. Quem propoe fica perto de ln(n);");
    Console.WriteLine("  quem recebe fica perto de n/ln(n). A razao cresce com n.");
}

// Quantos casamentos estaveis existem. A pergunta importa porque o Gale-Shapley
// devolve um extremo do conjunto, e saber o tamanho do conjunto diz o quanto de
// escolha esta sendo feita em silencio.
void Quantos()
{
    Console.WriteLine("quantos casamentos estaveis existem (enumeracao das n!)");
    Console.WriteLine();
    Console.WriteLine("    n   instancias   media   maximo   so um");
    foreach (var n in new[] { 3, 4, 5, 6, 7 })
    {
        const int amostras = 300;
        long soma = 0;
        var maximo = 0;
        var soUm = 0;
        for (var s = 1; s <= amostras; s++)
        {
            var (propoem, recebem) = Preferencias.Sorteadas(n, s);
            var quantos = ForcaBruta.Estaveis(propoem, recebem).Count;
            soma += quantos;
            maximo = Math.Max(maximo, quantos);
            if (quantos == 1) soUm++;
        }
        Console.WriteLine($"  {N(n),3}   {N(amostras),10}   {D((double)soma / amostras),5}   {N(maximo),6}   {D(100.0 * soUm / amostras, 1),4}%");
    }
}

// Fases do Hopcroft-Karp contra buscas do metodo de um caminho por vez. O ganho
// nao e constante: ele acompanha a raiz do numero de vertices.
void Fases()
{
    Console.WriteLine("hopcroft-karp: fases contra buscas do metodo simples");
    Console.WriteLine();
    Console.WriteLine("        n   arestas    pares   fases   buscas   raiz(n)");
    foreach (var n in new[] { 100, 400, 1600, 6400 })
    {
        var g = Bipartido.Sorteado(n, n, 4.0 / n, 1);
        var (hk, fases) = HopcroftKarp.Maximo(g);
        var (simples, buscas) = Berge.Maximo(g);
        if (hk.Pares != simples.Pares)
            throw new InvalidOperationException("os dois discordaram; isso e defeito, nao medida");
        Console.WriteLine($"  {N(n),7}   {N(g.Arestas),7}   {N(hk.Pares),6}   {N(fases),5}   {N(buscas),6}   {D(Math.Sqrt(n), 1),7}");
    }
}

// O hungaro contra a enumeracao: onde a forca bruta para de ser possivel.
void Hungaro()
{
    Console.WriteLine("hungaro contra a enumeracao das n! permutacoes");
    Console.WriteLine();
    Console.WriteLine("    n   permutacoes       hungaro ms   forca bruta ms");
    var aleatorio = new Random(1955);

    // Aquece o JIT antes de medir. Sem isso a primeira linha da tabela mede a
    // compilacao e nao o algoritmo, e sai dez vezes maior que a segunda, o que e
    // uma mentira involuntaria mas uma mentira.
    var aquecimento = new long[6, 6];
    for (var i = 0; i < 6; i++)
        for (var j = 0; j < 6; j++) aquecimento[i, j] = (i * 7 + j * 13) % 11;
    for (var k = 0; k < 200; k++) Conde.Emparelhamento.Hungaro.Resolver(aquecimento);
    Conde.Emparelhamento.Hungaro.MelhorPorForcaBruta(aquecimento);

    foreach (var n in new[] { 6, 8, 10, 11 })
    {
        var custo = new long[n, n];
        for (var i = 0; i < n; i++)
            for (var j = 0; j < n; j++) custo[i, j] = aleatorio.Next(0, 1000);

        var relogio = Stopwatch.StartNew();
        var (_, total) = Conde.Emparelhamento.Hungaro.Resolver(custo);
        relogio.Stop();
        var msHungaro = relogio.Elapsed.TotalMilliseconds;

        long permutacoes = 1;
        for (var k = 2; k <= n; k++) permutacoes *= k;

        if (n <= 8)
        {
            relogio.Restart();
            var bruto = Conde.Emparelhamento.Hungaro.MelhorPorForcaBruta(custo);
            relogio.Stop();
            if (bruto != total) throw new InvalidOperationException("discordaram; isso e defeito");
            Console.WriteLine($"  {N(n),3}   {N(permutacoes),11}   {D(msHungaro, 3),14}   {D(relogio.Elapsed.TotalMilliseconds, 3),14}");
        }
        else
        {
            Console.WriteLine($"  {N(n),3}   {N(permutacoes),11}   {D(msHungaro, 3),14}   {"(fora do juiz)",14}");
        }
    }
}
