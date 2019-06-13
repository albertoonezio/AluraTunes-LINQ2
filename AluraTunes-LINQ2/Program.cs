using AluraTunes_LINQ2.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AluraTunes_LINQ2
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var contexto = new AluraTunesEntities())
            {
                const int TAMANHO_PAGINA = 10;
                var numeroNotasFiscais = contexto.NotaFiscais.Count();
                var numeroPagina = Math.Ceiling((decimal)numeroNotasFiscais / TAMANHO_PAGINA);

                for (int i = 1; i <= numeroPagina; i++)
                {
                    ImprimirPagina(contexto, TAMANHO_PAGINA, i);
                }
            }

            Console.WriteLine();
            Console.WriteLine("Mostrar as notas ficais acima da média");

            using (var contexto = new AluraTunesEntities())
            {
                decimal queryMedia = contexto.NotaFiscais.Average(n => n.Total);
                var query = from nf in contexto.NotaFiscais
                            where nf.Total > queryMedia
                            orderby nf.Total descending
                            select new
                            {
                                Numero = nf.NotaFiscalId,
                                Data = nf.DataNotaFiscal,
                                Cliente = nf.Cliente.PrimeiroNome + " " + nf.Cliente.Sobrenome,
                                Total = nf.Total
                            };

                foreach (var notaFiscal in query)
                {
                    Console.WriteLine("{0}\t{1}\t{2}\t{3}", notaFiscal.Numero, notaFiscal.Data, notaFiscal.Cliente.PadRight(20), notaFiscal.Total);
                }
            }

            Console.WriteLine();
            Console.WriteLine("Produto mais vendido");

            using (var contexto = new AluraTunesEntities())
            {
                var faixasQuery = from f in contexto.Faixas
                                  where f.ItemNotaFiscals.Count() > 0
                                  let TotalDeVendas = f.ItemNotaFiscals.Sum(nf => nf.Quantidade * nf.PrecoUnitario)
                                  orderby TotalDeVendas descending
                                  select new
                                  {
                                      f.FaixaId,
                                      f.Nome,
                                      Total = TotalDeVendas
                                  };

                var produtoMaisVendido = faixasQuery.First();

                Console.WriteLine("{0}\t{1}\t{2}", produtoMaisVendido.FaixaId, produtoMaisVendido.Nome.PadRight(20), produtoMaisVendido.Total);

                Console.WriteLine();

                var query = from inf in contexto.ItemNotaFiscais
                            where inf.FaixaId == produtoMaisVendido.FaixaId
                            select new
                            {
                                nomeCliente = inf.NotaFiscal.Cliente.PrimeiroNome + " " + inf.NotaFiscal.Cliente.Sobrenome
                            };

                foreach (var cliente in query)
                {
                    Console.WriteLine(cliente.nomeCliente);
                }
            }

            Console.WriteLine();
            Console.WriteLine("Função para mostra que um item comprado pode comprar também");

            var nomeDaMusica = "Smells Like Teen Spirit";

            using (var contexto = new AluraTunesEntities())
            {
                var faixaIds = contexto.Faixas.Where(f => f.Nome == nomeDaMusica).Select(f => f.FaixaId);

                var query = from comprouItem in contexto.ItemNotaFiscais
                            join comprouTambem in contexto.ItemNotaFiscais
                                on comprouItem.NotaFiscalId equals comprouTambem.NotaFiscalId
                            where faixaIds.Contains(comprouItem.FaixaId)
                                && comprouItem.FaixaId != comprouTambem.FaixaId
                            select comprouTambem;

                foreach (var item in query)
                {
                    Console.WriteLine("{0}\t{1}", item.ItemNotaFiscalId, item.Faixa.Nome);
                }
            }

            Console.WriteLine();
            Console.WriteLine("Lista de aniversariantes do Mês");

            using (var contexto = new AluraTunesEntities())
            {
                var mesAniversario = 1;

                while (mesAniversario <= 12)
                {
                    Console.WriteLine("Mês: {0}", mesAniversario);

                    var lista = (from f in contexto.Funcionarios
                                where f.DataNascimento.Value.Month == mesAniversario
                                orderby f.DataNascimento.Value.Month, f.DataNascimento.Value.Day
                                select f).ToList();

                    mesAniversario += 1;

                    foreach (var func in lista)
                    {
                        Console.WriteLine("{0:dd/MM}\t{1} {2}", func.DataNascimento, func.PrimeiroNome, func.Sobrenome);
                    }
                }
            }

            Console.ReadKey();
        }

        private static void ImprimirPagina(AluraTunesEntities contexto, int TAMANHO_PAGINA, int numeroPagina)
        {
            var query = from nf in contexto.NotaFiscais
                        orderby nf.NotaFiscalId
                        select new
                        {
                            Numero = nf.NotaFiscalId,
                            Data = nf.DataNotaFiscal,
                            Cliente = nf.Cliente.PrimeiroNome + " " + nf.Cliente.Sobrenome,
                            Total = nf.Total
                        };
            int numeroDePulos = (numeroPagina - 1) * TAMANHO_PAGINA;

            query = query.Skip(numeroDePulos);

            query = query.Take(TAMANHO_PAGINA);

            Console.WriteLine();
            Console.WriteLine("Número da Página: {0}", numeroPagina);

            foreach (var item in query)
            {
                Console.WriteLine("{0}\t{1}\t{2}\t{3}", item.Numero, item.Data, item.Cliente.PadRight(20), item.Total);
            }
        }

        class Genero
        {
            public int Id { get; set; }
            public string Nome { get; set; }
        }

        class Musica
        {
            public int Id { get; set; }
            public string Nome { get; set; }
            public int GeneroId { get; set; }
        }
    }
}
