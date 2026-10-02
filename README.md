# emparelhamento

Casamento estável, emparelhamento máximo e atribuição de custo mínimo do zero em
C# e .NET 8: Gale-Shapley, Hopcroft-Karp e o algoritmo húngaro. O juiz é a
enumeração das n! atribuições.

```
$ dotnet medidor.dll vies

    n   posto medio de quem propoe   de quem recebe   razao
    5                         0,78             1,39     1,8x
   10                         1,42             2,66     1,9x
   50                         3,13            10,96     3,5x
  200                         4,78            34,35     7,2x
1.000                         6,40           136,47    21,3x
```

## O juiz tem o conjunto completo, e por isso os teoremas viram afirmações

A força bruta enumera as **n! atribuições** e separa as estáveis pela definição,
procurando par bloqueador entre os n² pares. Ela não sabe o que é Gale-Shapley.

Com o conjunto **inteiro** dos casamentos estáveis em mãos, coisas que normalmente
se aceitam de boa fé viram conferíveis em 600 instâncias cada:

- o conjunto nunca é vazio, qualquer que seja a instância;
- o resultado do Gale-Shapley está nele;
- ele dá a **cada** proponente o melhor par que esse proponente consegue em
  **qualquer** casamento estável, e dá todos ao mesmo tempo;
- o mesmo casamento dá a **cada** recebedor o **pior** par que ele teria em
  qualquer casamento estável.

O terceiro item não é óbvio e é por isso que ele precisa ser conferido em vez de
assumido: nada na definição impede que o melhor par de um exija o pior par de
outro. O juiz calcula, proponente por proponente, o melhor par entre todos os
casamentos estáveis, e o resultado tem de bater nos n **ao mesmo tempo**.

## Quem propõe não ganha um pouco: ganha o máximo

A tabela do topo mede o viés. O posto é base zero, então 0 é a primeira opção.
Com mil participantes, quem propõe fica com a 6ª opção em média e quem recebe com
a 136ª. **Vinte e uma vezes pior.**

Os dois números seguem o que a teoria prevê: quem propõe fica perto de ln(n)
(ln 1000 ≈ 6,9) e quem recebe perto de n/ln(n) (≈ 145). E a razão entre os dois
**cresce com n**, o que significa que o viés não é um detalhe que some na escala:
ele piora.

Isso faz da escolha de quem propõe uma decisão de política, não de engenharia. Em
alocação de residência médica, de vaga em escola ou de servidor em região, quem
está do lado que propõe leva sistematicamente o melhor resultado estável
disponível, e quem recebe leva o pior.

## Quantas opções estavam sendo descartadas em silêncio

```
$ dotnet medidor.dll quantos

    n   instancias   media   maximo   so um
    3          300    1,30        3   73,0%
    4          300    1,53        5   58,0%
    5          300    1,77        5   46,3%
    6          300    1,95        6   38,0%
    7          300    2,08        6   35,0%
```

O Gale-Shapley devolve **um extremo** do conjunto. Saber o tamanho do conjunto diz
o quanto de escolha está sendo feita sem que ninguém decida: com 7 participantes,
em 65% das instâncias existe mais de um casamento estável, e o algoritmo escolhe
aquele que beneficia um lado específico.

## Berge é o critério de parada, e ele é conferido nos dois sentidos

Berge, 1957: um emparelhamento é máximo **se e somente se** não existe caminho
aumentante. Sem esse teorema um algoritmo guloso não tem como saber se o que achou
é o máximo, e a resposta passaria a depender da ordem em que as arestas foram
olhadas.

O teste exercita as duas direções, que é o que um teste de "se e somente se"
precisa fazer: depois de terminar não pode existir caminho aumentante, e ao
desfazer um par qualquer tem de existir.

```
$ dotnet medidor.dll fases

        n   arestas    pares   fases   buscas   raiz(n)
      100       426       96       3      100      10,0
      400     1.652      389       6      400      20,0
    1.600     6.423    1.564       5    1.600      40,0
    6.400    25.311    6.251       8    6.400      80,0
```

Com 6.400 vértices de cada lado, o Hopcroft-Karp usa **8 fases** e o método de um
caminho por vez faz 6.400 buscas. O limite teórico é √n = 80, e ele gastou 8: o
limite é de pior caso, e grafo esparso sorteado está longe do pior caso.

A diferença entre os dois é uma condição de nível de uma linha. Sem ela o
algoritmo continua **correto** e vira o de um caminho por vez — que é o jeito mais
silencioso de perder a complexidade.

## O húngaro, e onde a força bruta para de existir

```
$ dotnet medidor.dll hungaro

    n   permutacoes       hungaro ms   forca bruta ms
    6           720            0,020            0,491
    8        40.320            0,039           21,149
   10     3.628.800            0,030   (fora do juiz)
   11    39.916.800            0,057   (fora do juiz)
```

Em n = 8 são 40.320 permutações: 21 ms contra 0,039 ms, **540 vezes**. Em n = 11
já são 39,9 milhões de permutações e o húngaro continua em 0,057 ms.

A primeira versão desta tabela tinha a linha de n = 6 marcando 5,4 ms, dez vezes
mais que n = 8. Era o JIT compilando dentro da medição. Está aquecido agora, e vale
dizer em voz alta: uma tabela com um número desses passa a mentir sem que ninguém
tenha mentido.

O algoritmo é dual e é isso que o explica: mantém um potencial por linha e por
coluna com `u[i] + v[j] ≤ custo[i][j]` **sempre**, e só usa as arestas em que vale
a igualdade. A soma dos potenciais é um limite inferior para qualquer atribuição,
então um emparelhamento perfeito dentro dessas arestas é ótimo por construção. O
teste confere a consequência disso: somar uma constante à matriz inteira soma
n vezes a constante ao total e **não muda a escolha**, porque o que conta são as
diferenças.

## As peças

| arquivo | o que faz |
|---|---|
| `Preferencias` | listas e postos; o posto é o que mantém o Gale-Shapley em O(n²) |
| `Casamento` | os dois sentidos, sempre coerentes |
| `Estabilidade` | a definição ao pé da letra: procura par bloqueador |
| `GaleShapley` | aceitação diferida, e a versão com os papéis trocados |
| `ForcaBruta` | o juiz: as n! atribuições e o conjunto completo dos estáveis |
| `Bipartido` | grafo sem preferência nenhuma |
| `Berge` | caminho aumentante, e o critério de parada como pergunta |
| `HopcroftKarp` | fases de caminhos mais curtos disjuntos, e König |
| `Hungaro` | atribuição de custo mínimo, com os potenciais duais |

São 25 testes.

## Como rodar

```
dotnet test testes/Emparelhamento.Testes/Emparelhamento.Testes.csproj -c Release
dotnet run --project ferramentas/Medidor/Medidor.csproj -c Release -- vies
dotnet run --project ferramentas/Medidor/Medidor.csproj -c Release -- quantos
dotnet run --project ferramentas/Medidor/Medidor.csproj -c Release -- fases
dotnet run --project ferramentas/Medidor/Medidor.csproj -c Release -- hungaro
```

## Licença

MIT.
