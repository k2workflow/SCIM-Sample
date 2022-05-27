using IdentityModel.OidcClient;
using Serilog;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Serilog.Sinks.SystemConsole.Themes;
using Microsoft.Extensions.Configuration;
using IdentityModel.Client;
using Newtonsoft.Json.Linq;
using ConsoleClientWithBrowser.Models;

namespace ConsoleClientWithBrowser
{
    public class Program
    {
        private static OidcClient s_oidcClient;
        private static HttpClient s_apiClient;

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
            Console.WriteLine("");
            Console.WriteLine("Press any key to sign in...");
        
            Console.ReadKey();

            await SignIn();
        }

        private static async Task SignIn()
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var scimConfig = new ScimConfig();
            configuration.Bind("scim", scimConfig);

            s_apiClient = new HttpClient()
            {
                BaseAddress = new Uri($"{scimConfig.BaseUrl.Trim('/')}/tokens/{scimConfig.Token}/")
            };

            var clientConfig = new ClientConfig();
            configuration.Bind("client", clientConfig);

            var browser = new SystemBrowser(5678);
            string redirectUri = string.Format($"http://127.0.0.1:5678");

            var options = new OidcClientOptions
            {
                Authority = clientConfig.Authority,
                ClientId = clientConfig.ClientId,
                RedirectUri = redirectUri,
                Scope = clientConfig.Scope,
                FilterClaims = false,
                Browser = browser,
                IdentityTokenValidator = new JwtHandlerIdentityTokenValidator(),
                RefreshTokenInnerHttpHandler = new SocketsHttpHandler()
            };

            Serilog.Core.Logger serilog = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message}{NewLine}{Exception}{NewLine}", theme: AnsiConsoleTheme.Code)
                .CreateLogger();

            options.LoggerFactory.AddSerilog(serilog);

            s_oidcClient = new OidcClient(options);
            LoginResult result = await s_oidcClient.LoginAsync(new LoginRequest());

            ShowResult(result);
            await NextSteps(result);
        }

        private static void ShowResult(LoginResult result)
        {
            if (result.IsError)
            {
                Console.WriteLine("\n\nError:\n{0}", result.Error);
                return;
            }

            Console.WriteLine("\n\nClaims:");
            foreach (System.Security.Claims.Claim claim in result.User.Claims)
            {
                Console.WriteLine("{0}: {1}", claim.Type, claim.Value);
            }

            Dictionary<string, JsonElement> values = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(result.TokenResponse.Raw);

            Console.WriteLine($"token response...");
            foreach (KeyValuePair<string, JsonElement> item in values)
            {
                Console.WriteLine($"{item.Key}: {item.Value}");
            }
        }

        private static async Task NextSteps(LoginResult result)
        {
            var currentAccessToken = result.AccessToken;
            var currentRefreshToken = result.RefreshToken;

            var menu = "- [G]et SCIM Users\n";
            if (currentRefreshToken != null) menu += "- [R]efresh token\n";
            menu += "- [E]xit\n";
            menu += "Press key: ";

            while (true)
            {
                Console.WriteLine("\n\n");

                Console.Write(menu);
                ConsoleKeyInfo key = Console.ReadKey();

                if (key.Key == ConsoleKey.E) return;
                if (key.Key == ConsoleKey.G) await CallApi(currentAccessToken);
                if (key.Key == ConsoleKey.R)
                {
                    IdentityModel.OidcClient.Results.RefreshTokenResult refreshResult = await s_oidcClient.RefreshTokenAsync(currentRefreshToken);
                    if (refreshResult.IsError)
                    {
                        Console.WriteLine($"Error: {refreshResult.Error}");
                    }
                    else
                    {
                        currentRefreshToken = refreshResult.RefreshToken;
                        currentAccessToken = refreshResult.AccessToken;

                        Console.WriteLine("\n\n");
                        Console.WriteLine($"Access token:   {currentAccessToken}");
                        Console.WriteLine($"Refresh token:  {currentRefreshToken ?? "none"}");
                    }
                }
            }
        }

        private static async Task CallApi(string currentAccessToken)
        {
            s_apiClient.SetBearerToken(currentAccessToken);
            HttpResponseMessage response = await s_apiClient.GetAsync("scim/v2/users");

            Console.WriteLine("\n\n");
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
