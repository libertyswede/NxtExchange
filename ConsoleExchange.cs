using System;
using System.Threading.Tasks;
using NxtLib;
using NxtLib.Blocks;
using NxtLib.ServerInfo;

namespace NxtExchange
{
    public class ConsoleExchange
    {
        private readonly NxtWalletDb wallet;
        private readonly string nxtServerUri;

        public ConsoleExchange(string nxtServerUri, string walletfile, string mainAccountSecretPhrase)
        {
            this.nxtServerUri = nxtServerUri;
            wallet = InitWalletFile(walletfile, mainAccountSecretPhrase);
        }

        private NxtWalletDb InitWalletFile(string walletfile, string mainAccountSecretPhrase)
        {
            var wallet = new NxtWalletDb(walletfile);

            if (!wallet.FileExists())
            {
                var accountService = new NxtLib.Local.LocalAccountService();
                var account = accountService.GetAccount(NxtLib.Accounts.AccountIdLocator.BySecretPhrase(mainAccountSecretPhrase));
                var mainAccount = new NxtAccount
                {
                    Address = account.AccountRs,
                    SecretPhrase = mainAccountSecretPhrase,
                    IsMainAccount = true
                };
                wallet.InitNewDb(mainAccount, NxtLib.Local.Constants.GenesisBlockId);
            }
            return wallet;
        }

        public async Task Run()
        {
            Console.WriteLine("Boot phase");

            var lastBlockId = wallet.GetLastBlockId();
            int lastBlockHeight = 0;
            var blockService = new BlockService(nxtServerUri);
            try
            {
                var block = await blockService.GetBlock(BlockLocator.ByBlockId(lastBlockId));
                lastBlockHeight = block.Height;
                Console.WriteLine($"Last block id {lastBlockId} is on height {lastBlockHeight}");
            }
            catch (NxtException e)
            {
                if (e.Message == "Unknown block")
                {
                    Console.WriteLine($"Fork detected, unable to find block with id: {lastBlockId}");
                    Environment.Exit(-1);
                }
                else
                {
                    throw;
                }
            }

            var serverInfoService = new ServerInfoService(nxtServerUri);
            var blockchainStatus = await serverInfoService.GetBlockchainStatus();
            var currentBlockHeight = blockchainStatus.NumberOfBlocks - 1;
            var blocksToProcess = currentBlockHeight - lastBlockHeight;
            Console.WriteLine($"Current block height is: {currentBlockHeight} ({blocksToProcess} blocks to process)");

            Console.WriteLine("Catch up phase");
            // while current height - 10, check each block for incoming tx
            // Update DB with new block id when done

            Console.WriteLine("Running phase");
            // Sleep
            // Check getBlockchainStatus to see if new block
            // Update DB with new block id
            Console.WriteLine("Work complete!");
        }
    }
}