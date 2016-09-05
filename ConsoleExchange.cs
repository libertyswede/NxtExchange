using System;
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
                Console.WriteLine("3) Quit");
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
    }
}