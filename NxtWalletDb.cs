using System.Collections.Generic;
using System.IO;
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

        public void Init(NxtAccount mainAccount, ulong lastBlockId)
        {
            if (IsInitialized())
            {
                return;
            }

            using (var dbConnection = OpenNewDbConnection())
            {
                const string createAccountSql = "CREATE TABLE account (id INTEGER PRIMARY KEY, secret_phrase TEXT, address TEXT, main_account INTEGER)";
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
                
                AddAccount(mainAccount, dbConnection);
            }
        }

        public NxtAccount GetMainAccount()
        {
            var sql = "SELECT id, secret_phrase, address, main_account FROM account WHERE main_account = 1";
            using (var dbConnection = OpenNewDbConnection())
            using (var command = new SqliteCommand(sql, dbConnection))
            using (var reader = command.ExecuteReader())
            {
                reader.Read();
                var account = ParseAccount(reader);
                return account;
            }
        }

        private static NxtAccount ParseAccount(SqliteDataReader reader)
        {
            var account = new NxtAccount
            {
                Id = (long)reader["id"],
                IsMainAccount = (long)reader["main_account"] == 1,
                SecretPhrase = reader["secret_phrase"].ToString(),
                Address = reader["address"].ToString()
            };
            return account;
        }

        public string GetSecretPhrase(long accountId)
        {
            var sql = $"SELECT secret_phrase FROM account WHERE id = {accountId}";
            using (var dbConnection = OpenNewDbConnection())
            using (var command = new SqliteCommand(sql, dbConnection))
            {
                var secretPhrase = command.ExecuteScalar().ToString();
                return secretPhrase;
            }
        }

        public ulong GetLastBlockId()
        {
            using (var dbConnection = OpenNewDbConnection())
            {
                return GetLastBlockId(dbConnection);
            }
        }

        private ulong GetLastBlockId(SqliteConnection dbConnection)
        {
            var sql = $"SELECT last_id FROM block";
            using (var command = new SqliteCommand(sql, dbConnection))
            {
                var lastBlockId = (ulong)(long)command.ExecuteScalar();
                return lastBlockId;
            }
        }

        public List<NxtAddress> GetAllDepositAddresses()
        {
            const string sql = "SELECT id, address FROM account WHERE main_account = 0";
            var addresses = new List<NxtAddress>();

            using (var dbConnection = OpenNewDbConnection())
            using (var command = new SqliteCommand(sql, dbConnection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var address = new NxtAddress
                    {
                        Id = (long)reader["id"],
                        Address = reader["address"].ToString()
                    };
                    addresses.Add(address);
                }
            }
            return addresses;
        }

        public List<NxtAccount> GetAllDepositAccounts()
        {
            var accounts = new List<NxtAccount>();
            var sql = "SELECT id, secret_phrase, address, main_account FROM account WHERE main_account = 0";
            using (var dbConnection = OpenNewDbConnection())
            using (var command = new SqliteCommand(sql, dbConnection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var account = ParseAccount(reader);
                    accounts.Add(account);
                }
            }
            return accounts;
        }

        public void UpdateLastBlockId(ulong lastBlockId)
        {
            using (var dbConnection = OpenNewDbConnection())
            {
                var sql = $"UPDATE block SET last_id = {(long)lastBlockId}";
                using (var command = new SqliteCommand(sql, dbConnection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public void AddAccount(NxtAccount account)
        {
            using (var dbConnection = OpenNewDbConnection())
            {
                AddAccount(account, dbConnection);
            }
        }

        private void AddAccount(NxtAccount account, SqliteConnection dbConnection)
        {
            var isMainAccount = account.IsMainAccount ? "1" : "0";
            var sql = $"INSERT INTO account (secret_phrase, address, main_account) VALUES ('{account.SecretPhrase}', '{account.Address}', {isMainAccount})";
            using (var command = new SqliteCommand(sql, dbConnection))
            {
                command.ExecuteNonQuery();
            }

            using (var command = new SqliteCommand("SELECT last_insert_rowid()", dbConnection))
            {
                account.Id = (long)command.ExecuteScalar();
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