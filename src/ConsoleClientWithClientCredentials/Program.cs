using ConsoleClientWithClientCredentials.Models;
using IdentityModel.Client;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleClientWithClientCredentials
{
    public class Program
    {
        public static async Task Main()
        {
            Console.WriteLine("                         ");
            Console.WriteLine("             ██╗░░██╗██████╗░");
            Console.WriteLine("             ██║░██╔╝╚════██╗");
            Console.WriteLine("             █████═╝░░░███╔═╝");
            Console.WriteLine("             ██╔═██╗░██╔══╝░░");
            Console.WriteLine("             ██║░╚██╗███████╗");
            Console.WriteLine("             ╚═╝░░╚═╝╚══════╝");
            Console.WriteLine("+-------------------------------------+");
            Console.WriteLine("|  Welcome to K2 Identity Provider    |");
            Console.WriteLine("+-------------------------------------+");
            Console.WriteLine();
            Thread.Sleep(1000);

            await SignIn();

            Console.WriteLine();
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }

        private static async Task SignIn()
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var scimConfig = new ScimConfig();
            configuration.Bind("scim", scimConfig);

            var scimClient = new HttpClient()
            {
                BaseAddress = new Uri($"{scimConfig.BaseUrl.Trim('/')}/tokens/{scimConfig.Token}/")
            };

            var clientConfig = new ClientConfig();
            configuration.Bind("client", clientConfig);

            var idtsClient = new HttpClient();
            var disco = await idtsClient.GetDiscoveryDocumentAsync(clientConfig.Authority);

            if (disco.IsError) 
                throw new Exception(disco.Error);

            TokenResponse tokenResponse = await idtsClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientId = clientConfig.ClientId,
                ClientSecret = clientConfig.ClientSecret,
                Scope = clientConfig.Scope
            });

            if (tokenResponse.IsError)
            {
                Console.WriteLine(tokenResponse.Error);
                return;
            }

            Console.WriteLine(tokenResponse.Json);
            Console.WriteLine();

            // call api
            scimClient.SetBearerToken(tokenResponse.AccessToken);

            HttpResponseMessage response = await scimClient.GetAsync("scim/v2/users");

            Console.WriteLine($"{response.RequestMessage.Method} {response.RequestMessage.RequestUri}");
            Console.WriteLine($"{(int)response.StatusCode} {response.ReasonPhrase}");

            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                var obj = JToken.Parse(content);

                Console.WriteLine(obj.ToString(Newtonsoft.Json.Formatting.Indented));
            }

        }
    }
}
