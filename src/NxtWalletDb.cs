using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace NxtExchange
{
    public class NxtWalletDb
    {
        private readonly string filepath;

        public NxtWalletDb(string filePath)
        {
            this.filepath = filePath;
        }

        public bool IsInitialized()
        {
            return File.Exists(filepath);
        }

        public void Init(ulong lastBlockId)
        {
            if (IsInitialized())
            {
                return;
            }

            var folder = Path.GetDirectoryName(filepath);
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            using (var dbConnection = OpenNewDbConnection())
            {
                const string createAccountSql = "CREATE TABLE account (id INTEGER PRIMARY KEY, secret_phrase TEXT, address TEXT, balance_nqt INTEGER)";
                using (var command = new SqliteCommand(createAccountSql, dbConnection))
                {
                    command.ExecuteNonQuery();
                }

                const string createBlockSql = "CREATE TABLE block (last_id INTEGER)";
                using (var command = new SqliteCommand(createBlockSql, dbConnection))
                {
                    command.ExecuteNonQuery();
                }
                var insertBlockSql = $"INSERT INTO block (last_id) VALUES ({(long)lastBlockId})";
                using (var command = new SqliteCommand(insertBlockSql, dbConnection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public async Task<string> GetSecretPhrase(long accountId)
        {
            var sql = $"SELECT secret_phrase FROM account WHERE id = {accountId}";
            using (var dbConnection = OpenNewDbConnection())
            using (var command = new SqliteCommand(sql, dbConnection))
            {
                var secretPhrase = (await command.ExecuteScalarAsync()).ToString();
                return secretPhrase;
            }
        }

        public async Task<ulong> GetLastBlockId()
        {
            using (var dbConnection = OpenNewDbConnection())
            {
                var sql = $"SELECT last_id FROM block";
                using (var command = new SqliteCommand(sql, dbConnection))
                {
                    var lastBlockId = (ulong)(long) await command.ExecuteScalarAsync();
                    return lastBlockId;
                }
            }
        }

        public async Task<List<NxtAccount>> GetAllDepositAccounts()
        {
            var accounts = new List<NxtAccount>();
            var sql = "SELECT id, secret_phrase, address, balance_nqt FROM account";
            using (var dbConnection = OpenNewDbConnection())
            using (var command = new SqliteCommand(sql, dbConnection))
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var account = ParseAccount(reader);
                    accounts.Add(account);
                }
            }
            return accounts;
        }

        public async Task<NxtAccount> GetAccount(long accountId)
        {
            var sql = $"SELECT id, secret_phrase, address, balance_nqt FROM account WHERE id = {accountId}";
            using (var dbConnection = OpenNewDbConnection())
            using (var command = new SqliteCommand(sql, dbConnection))
            using (var reader = await command.ExecuteReaderAsync())
            {
                await reader.ReadAsync();
                var account = ParseAccount(reader);
                return account;
            }
        }

        private static NxtAccount ParseAccount(SqliteDataReader reader)
        {
            var account = new NxtAccount
            {
                Id = (long)reader["id"],
                BalanceNqt = (long)reader["balance_nqt"],
                SecretPhrase = reader["secret_phrase"].ToString(),
                Address = reader["address"].ToString()
            };
            return account;
        }

        public async Task UpdateLastBlockId(ulong lastBlockId)
        {
            using (var dbConnection = OpenNewDbConnection())
            {
                var sql = $"UPDATE block SET last_id = {(long)lastBlockId}";
                using (var command = new SqliteCommand(sql, dbConnection))
                {
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task AddAccount(NxtAccount account)
        {
            using (var dbConnection = OpenNewDbConnection())
            {
                var sql = $"INSERT INTO account (secret_phrase, address, balance_nqt) VALUES ('{account.SecretPhrase}', '{account.Address}', {account.BalanceNqt})";
                using (var command = new SqliteCommand(sql, dbConnection))
                {
                    await command.ExecuteNonQueryAsync();
                }

                using (var command = new SqliteCommand("SELECT last_insert_rowid()", dbConnection))
                {
                    account.Id = (long) await command.ExecuteScalarAsync();
                }
            }
        }

        public async Task UpdateAccountBalance(long accountId, long balanceNqt)
        {
            using (var dbConnection = OpenNewDbConnection())
            {
                var sql = $"UPDATE account SET balance_nqt = {balanceNqt} WHERE id = {accountId}";
                using (var command = new SqliteCommand(sql, dbConnection))
                {
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        private SqliteConnection OpenNewDbConnection()
        {
            var dbConnection = new SqliteConnection($"Data Source={filepath}");
            dbConnection.Open();
            return dbConnection;
        }
    }
}