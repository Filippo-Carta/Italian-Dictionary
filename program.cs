using System;
using System.Net.Http;
using HtmlAgilityPack;
using Newtonsoft.Json;

class Program
{
    static async System.Threading.Tasks.Task Main(string[] args)
    {
        while (true) // Ciclo infinito per continuare fino a che l'utente non decide di uscire
        {
            Console.WriteLine("\nInserisci una parola da cercare:\n");
            string input = Console.ReadLine();
            Console.Clear();

            // Converte la prima lettera in maiuscolo e il resto in minuscolo
            string output = char.ToUpper(input[0]) + input.Substring(1).ToLower();
            string searchUrl = $"https://www.treccani.it/enciclopedia/ricerca/{output}/?search={output}";

            // Crea un'istanza di HttpClient per effettuare la richiesta
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    
                    // Aggiungi l'intestazione per sembrare una richiesta fatta da un browser
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");

                    // Effettua la richiesta GET
                    var response = await client.GetStringAsync(searchUrl);

                    // Carica il contenuto HTML nella HtmlDocument
                    var htmlDoc = new HtmlDocument();
                    htmlDoc.LoadHtml(response);

                    // Cerca tutti gli <script> nella pagina
                    HtmlNodeCollection scriptNodes = htmlDoc.DocumentNode.SelectNodes("//script");

                    if (scriptNodes != null)
                    {
                        foreach (var script in scriptNodes)
                        {
                            string scriptContent = script.InnerText;

                            // Verifica se il contenuto del <script> contiene la parola "description"
                            if (scriptContent.Contains("\"description\"") && scriptContent.Contains("{"))
                            {
                                // Trova l'inizio e la fine del JSON (racchiuso tra parentesi graffe)
                                int start = scriptContent.IndexOf("{");
                                int end = scriptContent.LastIndexOf("}") + 1;

                                if (start != -1 && end != -1)
                                {
                                    string jsonContent = scriptContent.Substring(start, end - start);

                                    // Deserializza il JSON in un oggetto C#
                                    dynamic jsonObject = JsonConvert.DeserializeObject(jsonContent);

                                    // Estrai la description dal JSON
                                    string description = jsonObject.props.pageProps.data.description;

                                    // Stampa la description
                                    Console.WriteLine($"Descrizione da treccani.it per {output}: \n\n{description}");
                                    Console.WriteLine($"\n\nPer più significati visita {searchUrl}");

                                    // Chiedi se l'utente vuole continuare
                                    Console.WriteLine("\nContinuare a navigare? [y/n]");
                                    string esc_or_stay = Console.ReadLine();
                                    Console.Clear();
                                    // Se l'utente sceglie "n", esci dal ciclo e termina il programma
                                    if (esc_or_stay.ToLower() != "y")
                                    {
                                        
                                        Environment.Exit(0); // Uscita immediata
                                    }
                                    break; // Uscita dal ciclo per evitare più ricerche nello stesso ciclo
                                }
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Nessun script trovato.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Si è verificato un errore: " + ex.Message);
                }
            }
        }
    }
}
