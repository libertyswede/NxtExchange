using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.PlatformAbstractions;
using NxtLib;

namespace NxtExchange
{
    public class Program
    {
        // TODO:
        // * Migrate sqlite wallet to EF7
        // * Encrypt wallet. 
        //   Need to decide whether to use DataProtection (new dotnet core stuff) described here: https://docs.asp.net/en/latest/security/data-protection/index.html
        //   or just to use AES-256-CBC encryption and let user provide a key.
        // * Make NxtConnector thread safe (if needed?)

        public static void Main(string[] args)
        {
            var configSettings = ReadConfig();
            var walletFile = configSettings.Single(c => c.Key == "walletFile").Value;
            var nxtServerAddress = configSettings.Single(c => c.Key == "nxtServerAddress").Value;
            var confirmations = int.Parse(configSettings.Single(c => c.Key == "confirmations").Value);

            var connector = new NxtConnector(new ServiceFactory(nxtServerAddress), walletFile, confirmations);
            var exchange = new ConsoleExchange(connector);
            Task.Run(() => exchange.Run()).Wait();
        }

        private static IEnumerable<IConfigurationSection> ReadConfig()
        {
            var configBuilder = new ConfigurationBuilder();
            configBuilder.SetBasePath(PlatformServices.Default.Application.ApplicationBasePath);
            configBuilder.AddJsonFile("config.json");
            configBuilder.AddJsonFile("config-Development.json", true);
            var configRoot = configBuilder.Build();
            var configSettings = configRoot.GetChildren();
            return configSettings;
        }
    }
}
