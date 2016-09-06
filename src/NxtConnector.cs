using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NxtLib;
using NxtLib.Accounts;
using NxtLib.Blocks;
using NxtLib.Local;
using NxtLib.ServerInfo;

namespace NxtExchange
{
    public class NxtConnector
    {
        private readonly int confirmations;
        private readonly NxtWalletDb wallet;
        private readonly IBlockService blockService;
        private readonly IServerInfoService serverInfoService;

        public NxtConnector(IServiceFactory serviceFactory, string walletfile, int confirmations)
        {
            blockService = serviceFactory.CreateBlockService();
            serverInfoService = serviceFactory.CreateServerInfoService();
            this.confirmations = confirmations;

            wallet = InitWallet(walletfile);
        }

        private NxtWalletDb InitWallet(string walletfile)
        {
            var wallet = new NxtWalletDb(walletfile);

            if (wallet.IsInitialized())
            {
                return wallet;
            }

            var blockchainStatus = serverInfoService.GetBlockchainStatus().Result;
            wallet.Init(blockchainStatus.LastBlockId);
            return wallet;
        }

        public async Task CheckIncomingTransactions()
        {
            var lastBlockId = await wallet.GetLastBlockId();
            int lastBlockHeight = 0;
            try
            {
                var block = await blockService.GetBlock(BlockLocator.ByBlockId(lastBlockId));
                lastBlockHeight = block.Height;
                Console.WriteLine($"Last known block id {lastBlockId} is on height {lastBlockHeight}");
            }
            catch (NxtException e)
            {
                if (e.Message == "Unknown block")
                {
                    Console.WriteLine($"Fork detected, unable to find block with id: {lastBlockId}. Manual rollback is needed!");
                    Environment.Exit(-1);
                }
                else
                {
                    throw;
                }
            }

            var blockchainStatus = await serverInfoService.GetBlockchainStatus();
            var currentBlockHeight = blockchainStatus.NumberOfBlocks - 1;
            var blocksToProcess = Math.Max(0, currentBlockHeight - lastBlockHeight - confirmations);
            Console.WriteLine($"Current block height is: {currentBlockHeight} ({blocksToProcess} block(s) to process)");
            var depositAccounts = await wallet.GetAllDepositAccounts();
            var depositAddresses = new HashSet<string>(depositAccounts.Select(a => a.Address));
            while (currentBlockHeight > lastBlockHeight + confirmations)
            {
                lastBlockHeight++;
                Console.WriteLine($"Processing block @ height {lastBlockHeight}");

                var block = await blockService.GetBlockIncludeTransactions(BlockLocator.ByHeight(lastBlockHeight), true);
                var nxtTransactions = block.Transactions.Where(t => depositAddresses.Contains(t.RecipientRs) && !t.Phased)
                    .Union(block.ExecutedPhasedTransactions.Where(t => depositAddresses.Contains(t.RecipientRs)))
                    .Where(t => t.Amount.Nqt > 0);

                foreach (var transaction in nxtTransactions)
                {
                    Console.WriteLine($"Incoming {transaction.Amount.Nxt} NXT to {transaction.RecipientRs}");
                    var account = depositAccounts.Single(d => d.Address == transaction.RecipientRs);
                    account.BalanceNqt += transaction.Amount.Nqt;
                    await wallet.UpdateAccountBalance(account.Id, account.BalanceNqt);
                }
                await wallet.UpdateLastBlockId(block.BlockId);
            }
        }

        public async Task<NxtAccount> AddAccount()
        {
            var localPasswordGenerator = new LocalPasswordGenerator();
            var localAccountService = new LocalAccountService();

            var secretPhrase = localPasswordGenerator.GeneratePassword();
            var accountWithPublicKey = localAccountService.GetAccount(AccountIdLocator.BySecretPhrase(secretPhrase));

            var account = new NxtAccount
            {
                Address = accountWithPublicKey.AccountRs,
                SecretPhrase = secretPhrase,
                BalanceNqt = 0
            };

            await wallet.AddAccount(account);

            return account;
        }
    }
}