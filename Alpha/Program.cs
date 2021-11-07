using System;
using System.Text; 
using System.IO;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Alpha
{
    public class Program
    {
        const string url = "https://api.bitbucket.org/2.0/users";
        static void Main(string[] args)
        {
            try
            {
                var seconds = ValidateTempLog();
                if(seconds > 59)
                {
                    Console.WriteLine("Add the path, and then press Enter: ");
                    string path = Console.ReadLine();

                    //1 - Receive a mandatory parameter with a full path to a file
                    path = ValidadePath(path);

                    //2 - Process the file, storing in a variable each username line contained in the file
                    var listname = ProcessFile(path);

                    //3 - Retrieve from the bitbucket api information for each user
                    //4 - Log the response from the API to a log file in the same folder as the executable
                    Console.WriteLine("Sending names for web api...");
                    foreach(string name in listname)
                    { 
                        Console.WriteLine("Await 5 seconds for next");
                        System.Threading.Thread.Sleep(5000);
                        Send(name).GetAwaiter().GetResult(); 
                    }
                }
                else
                {
                    Console.WriteLine(seconds);
                    Console.WriteLine("Wait at least 60 seconds to run the app again. Thank you");
                }

            }
            catch (IOException e)
            {
                TextWriter errorWriter = Console.Error;
                errorWriter.WriteLine(e.Message);
            }
            catch (Exception e)
            {
                TextWriter errorWriter = Console.Error;
                errorWriter.WriteLine(e.Message);
            }
            finally
            {
                //5 - Exit the application
                // Recover the standard output stream so that a
                // completion message can be displayed.
                Console.WriteLine("Await 5 seconds for exit");
                System.Threading.Thread.Sleep(5000);
                var standardOutput = new StreamWriter(Console.OpenStandardOutput());
                standardOutput.AutoFlush = true;
                Console.WriteLine("Open file Log. Finish");
                Console.SetOut(standardOutput);
            }
        }

        private static double ValidateTempLog()
        {
            DateTime lastTime;
            using (var reader = new StreamReader("log"))
            {
               var str=  reader.ReadToEnd();
               if(!string.IsNullOrEmpty(str))
               {
                int x = str.LastIndexOf('\n');
                string lastline = str.Substring(x-19);
                lastTime = string.IsNullOrEmpty(lastline) ? DateTime.Now.AddSeconds(-60): Convert.ToDateTime(lastline);
               }else{
                   lastTime = DateTime.Now.AddSeconds(-60);
               }

               reader.Close();
            }
            var now = DateTime.Now;
            return (now < lastTime) ? (lastTime - now).TotalSeconds : (now - lastTime).TotalSeconds; 
        }

        private static async Task Send(string name)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri($"{url}/{name}");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            try
            {   Console.WriteLine($"Sending name for: {client.BaseAddress}");
                CreateLog($"Request: {client.BaseAddress}");
                HttpResponseMessage response = await client.GetAsync(client.BaseAddress);
                if(response.IsSuccessStatusCode){
                    Console.WriteLine($"Ok, name sent {name}");
                    Console.WriteLine("------------------------------------------------------");
                }else{
                    Console.WriteLine($"Error, name not sent {name}");
                    Console.WriteLine("------------------------------------------------------");
                }
                CreateLog($"Response: {response.ToString()}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                CreateLog(e.Message);
            }
        }
        private static List<string> ProcessFile(string path)
        {
            List<string> lista = new List<string>();
            using (var reader = new StreamReader(path))
            {
                // Redirect standard input from the console to the input file.
                Console.SetIn(reader);
                string line;
                while ((line = Console.ReadLine()) != null)
                {
                    if(!string.IsNullOrEmpty(line))
                    {
                        Console.WriteLine(line);
                        lista.Add(line);
                    }
                }
                reader.Close();
            }
            Console.WriteLine("There were {0} lines.", lista.Count);  
            return lista;
        }
        private static string ValidadePath(string path)
        { 
            //Validade path and file
            while (string.IsNullOrEmpty(path) || string.IsNullOrWhiteSpace(path))
            {
                Console.WriteLine("This is not valid input. Please enter an path value: ");
                Console.WriteLine("Waiting parameter...");
                path = Console.ReadLine();
            }
            return path;
        }
        private static void CreateLog(string parameter)
        {
            // Escreve o arquivo e caso ele não exista o mesmo é criado.
            using (var log = new StreamWriter("log", true, Encoding.ASCII))
            {
                log.WriteLine(parameter);
                log.WriteLine("-----------------------------------------------");
                log.WriteLine(DateTime.Now);
                log.Close();
            }
        }

    
    }
}