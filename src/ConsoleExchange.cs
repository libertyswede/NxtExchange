using System;
using System.Linq;
using System.Threading.Tasks;

namespace NxtExchange
{
    public class ConsoleExchange
    {
        private readonly NxtConnector connector;

        public ConsoleExchange(NxtConnector connector)
        {
            this.connector = connector;
        }

        public async Task Run()
        {
            Console.WriteLine("Welcome to the Nxt Exchange Integration Program!");
            await WriteMenu();
        }

        private async Task WriteMenu()
        {
            var done = false;
            while (!done)
            {
                Console.WriteLine();
                Console.WriteLine("1) Check For Incoming Transactions");
                Console.WriteLine("2) Add Account");
                Console.WriteLine("3) List Accounts");
                Console.WriteLine("4) Send Money");
                Console.WriteLine("5) Quit");
                Console.Write("> ");

                var value = int.Parse(Console.ReadLine());
                Console.WriteLine();

                switch (value)
                {
                    case 1:
                        await WriteCheckIncomingTransactions();
                        break;
                    case 2:
                        await WriteAddAccount();
                        break;
                    case 3:
                        await WriteListAccounts();
                        break;
                    case 4:
                        await WriteSendMoney();
                        break;
                    default:
                        done = true;
                        break;
                }
            }
            Console.WriteLine("Cya!");
        }

        private async Task WriteCheckIncomingTransactions()
        {
            await connector.CheckIncomingTransactions();
        }

        private async Task WriteAddAccount()
        {
            var account = await connector.AddAccount();
            Console.WriteLine($"Account {account.Address} added");
        }

        private async Task WriteListAccounts()
        {
            var accounts = await connector.GetAccounts();
            foreach (var account in accounts.Where(a => a.BalanceNqt > 0))
            {
                Console.WriteLine($"Address: {account.Address}, Balance: {account.BalanceNqt}, Id: {account.Id}");
            }
            if (accounts.Any(a => a.BalanceNqt == 0))
            {
                Console.WriteLine($"Plus {accounts.Count(a => a.BalanceNqt == 0)} with 0 balance.");
            }
        }

        private async Task WriteSendMoney()
        {
            Console.Write("Enter Account ID to send from: ");
            var accountId = long.Parse(Console.ReadLine());
            Console.Write("Enter Recipient address: ");
            var recipient = Console.ReadLine();
            Console.Write("Enter number of NQT to send: ");
            var amountNqt = long.Parse(Console.ReadLine());
            Console.Write("Enter message (optional): ");
            var message = Console.ReadLine();
            Console.Write("Enter recipient public key (optional): ");
            var recipientPublicKey = Console.ReadLine();
            await connector.SendMoney(accountId, recipient, amountNqt, message, recipientPublicKey);
        }
    }
}