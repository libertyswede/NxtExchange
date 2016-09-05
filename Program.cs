using System.Threading.Tasks;

namespace NxtExchange
{
    public class Program
    {
        private const string walletfile = @"c:\temp\nxtexchange\nxtwallet.db";
        private const string mainAccountSecretPhrase = "abc123";
        private const string nxtServerAddress = "https://node1.ardorcrypto.com/nxt"; //NxtLib.Local.Constants.DefaultNxtUrl;

        // TODO:
        // * Migrate sqlite wallet to EF7
        // * Use async/await
        // * Encrypt wallet. 
        //   Need to decide whether to use DataProtection (new dotnet core stuff) described here: https://docs.asp.net/en/latest/security/data-protection/index.html
        //   or just to use AES-256-CBC encryption and let user provide a key.

        public static void Main(string[] args)
        {
            var exchange = new ConsoleExchange(nxtServerAddress, walletfile, mainAccountSecretPhrase);
            Task.Run(() => exchange.Run()).Wait();
        }
    }
}
