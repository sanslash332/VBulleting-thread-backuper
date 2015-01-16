using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;


namespace Safari_thread_backuper
{
    class Program
    {
        static void Main(string[] args)
        {

            string webDomain=null;
            string subforum=null;
            string numbers = "";
            int pages = 0;
            if (File.Exists("input.txt") == true)
            {
                StreamReader lecotr = new StreamReader(new FileStream("input.txt", FileMode.Open));
                webDomain = lecotr.ReadLine();
                subforum = lecotr.ReadLine();
                lecotr.Dispose();
                lecotr.Close();


            }


            WebClient web = new WebClient();
            Console.WriteLine("ingrese la url del foro a respaldar, incluyendo el / final de la dirección. Sólo la dirección principal, no la del subforo");

            if (webDomain == null)
            {

                webDomain = Console.ReadLine();

            }
            else
            {
                Console.WriteLine("cargado desde archivo input: " + webDomain);

            }


           
            Console.WriteLine(" ahora escriba la extención hacia el subforo, si tiene varias páginas, incluya [%] en el lugar correspondiente al número de la página");

            if (subforum == null)
            {

                subforum = Console.ReadLine();
            }
            else
            {
                Console.WriteLine("cargado desde archivo input: " + subforum);

            }

            
            if (subforum.Contains("[%]") == true)
            {

                Console.WriteLine("ingrese cuantos elementos quiere revisar del objeto");
                numbers=Console.ReadLine();
                pages = Int32.Parse(numbers);
            }



            Console.WriteLine("la descarga se está realizando de algún elemento diferente a temas? s/n");
            string noSonTemas = Console.ReadLine();


            Console.WriteLine("precione enter para iniciar la descarga de temas de la dirección: " + webDomain + subforum);
            Console.WriteLine("modo bruto!");
            Console.ReadLine();

            string oldPage = "";
            string currentPage = "hola";

            int threadNum = 0;
            int numPage = 0;
            string contenido = "";
            long escritas = 0;
            int subpages = 2;

            if (noSonTemas != "s")
            {
                Console.WriteLine("ingrese cantidad de páginas a revisar para los temas");
                string pags = Console.ReadLine();
                subpages = Int32.Parse(pags);

            }

            for (int i = 0; i <= pages ; i++)
            {
                Console.WriteLine("objeto " + i.ToString());

                try
                {

                    for (int j = 1; j <= subpages ; j++)
                    {

                        string subpage = subforum.Replace("[%]", i.ToString());

                        string urlToDownload = webDomain + subpage;

                        if (noSonTemas != "s")
                        {
                            urlToDownload += "&page=" + j.ToString();
                        }

                        contenido = web.DownloadString(urlToDownload);
                        escribirLog("bajando: " + urlToDownload);
                        currentPage = i.ToString() + " - " + getWebName(contenido);

                        if (oldPage == currentPage)
                        {
                            break;

                        }

                        oldPage = currentPage;
                        escritas++;
                        escribirLog("número " + escritas.ToString());

                        guardarTema(currentPage, contenido);

                        Console.WriteLine("guardada " + escritas.ToString() + " tema " + currentPage);


                      



                    }
                }
                catch (Exception e)
                {
                    escribirLog("error en " + webDomain + subforum  + " elemento " + i.ToString() + ", siendo " + e.Message);
                }

                }






            Console.WriteLine("terminado, se guardaron " + escritas.ToString());

            Console.ReadLine();
            Environment.Exit(1);

            if (pages == 0)
            {

                try
                {
                    Console.WriteLine(webDomain + subforum);

                    
                    contenido = web.DownloadString(webDomain + subforum);
 



                    string titulo = getWebName(contenido);

                    guardarTema(titulo, contenido);

                    extraerTemas(contenido, webDomain, web);

                }
                catch (Exception e)
                {
                    Console.WriteLine("hubo un error " + e.Message);

                }
            }
            else
            {
                for (int i = 1; i <= pages; i++)
                {
                    string newSubForum = subforum.Replace("[%]", i.ToString());
                    try
                    {
                        Console.WriteLine(webDomain + newSubForum);

                        contenido = web.DownloadString(webDomain + newSubForum);


                        string titulo = getWebName(contenido);


                        guardarTema(titulo, contenido);

                        extraerTemas(contenido, webDomain, web);

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("hubo un error " + e.Message);

                    }


                    
                }
                            }


           Console.WriteLine("precione enter para salir");

           Console.ReadLine();


        }

        public static  string getWebName(string webp)
        {
            
            string[] cortado = webp.Split(new string[]{"title","Title","TITLE"}, StringSplitOptions.None);
            string titulo= "";

            foreach (string cap in cortado)
            {
                if (cap.Length <= 150)
                {
                    if (cap.Contains('\n') == true)
                    {
                        continue;
                    }

                    titulo = cap;
                    break;
                }

            }

            titulo = titulo.Replace('<', ' ').Replace('>', ' ').Replace('"',' ').Replace('/',' ').Replace('\\',' ').Replace('*',' ').Replace(':',' ');
            //Console.WriteLine("el titulo es " + titulo);
            
            return titulo;

        }


        public static void extraerTemas(string subforo, string webDomain, WebClient navigator)
        {
            string[] links = subforo.Split(new string[] {"href"}, StringSplitOptions.None);
            List<string> visitados = new List<string>();

            for (int i = 1; i < links.Length; i++)
            {
                if (links[i].Contains("showthread") == false)
                {
                    continue;

                }

                if (links[i].Contains("page") == false)
                {
                    string link = webDomain + links[i].Split('"')[1];
                    string content = navigator.DownloadString(link);
                    guardarTema(getWebName(content), content);
                    escribirLog("navegado link: " + link);

                }
                else
                {

                }
                //end for
            }

        }

        public static void guardarTema(string titulo, string pagina)
        {
            if (File.Exists(@"downloadPages\" + titulo + ".html") == true)
            {
                escribirLog("ya existe el archivo " + titulo + ".html");

                return;
                File.Delete(@"downloadPages\" + titulo + ".html");
                
            }

            StreamWriter escritor = new StreamWriter(new FileStream(@"downloadPages\" + titulo + ".html", FileMode.OpenOrCreate), Encoding.Unicode);

            escritor.Write(pagina);

            escritor.Dispose();
            escritor.Close();
            escribirLog("guardado el archivo " + titulo + ".html");

        }

        public static void escribirLog(string texto)
        {
            StreamWriter log = new StreamWriter(new FileStream("log.log", FileMode.Append));
           
            log.WriteLine(texto);
            log.Dispose();

            log.Close();

        }



    }
}
