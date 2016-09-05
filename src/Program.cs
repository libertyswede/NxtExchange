using System.Threading.Tasks;
using NxtLib;

namespace NxtExchange
{
    public class Program
    {
        private const string walletfile = @"c:\temp\nxtexchange\nxtwallet.db";
        private const string nxtServerAddress = "https://node1.ardorcrypto.com/nxt"; //NxtLib.Local.Constants.DefaultNxtUrl;

        // TODO:
        // * Migrate sqlite wallet to EF7
        // * Encrypt wallet. 
        //   Need to decide whether to use DataProtection (new dotnet core stuff) described here: https://docs.asp.net/en/latest/security/data-protection/index.html
        //   or just to use AES-256-CBC encryption and let user provide a key.
        // * Make NxtConnector thread safe (if needed?)

        public static void Main(string[] args)
        {
            var connector = new NxtConnector(new ServiceFactory(nxtServerAddress), walletfile);
            var exchange = new ConsoleExchange(connector);
            Task.Run(() => exchange.Run()).Wait();
        }
    }
}
