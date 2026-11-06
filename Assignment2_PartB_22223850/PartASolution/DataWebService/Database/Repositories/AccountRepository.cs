using DataWebService.Models.Entities;
using System.Data.SQLite;
using System.Security.Principal;
using System.Text;

namespace DataWebService.Database.Repositories
{
    public static class AccountRepository
    {
        /* INSERT QUERY */
        public static bool InsertAccount(Account acc)   // Return bool if successful
        {
            try
            {
                using (SQLiteConnection conn = DBConnection.Open())
                {
                    using (SQLiteCommand cmd = conn.CreateCommand())
                    {
                        // Add Account Entry
                        cmd.CommandText = @"
                            INSERT INTO AccountTable (accName, ownerID, balance)
                            VALUES (@acc.accName, @acc.ownerID, @acc.balance);";
                        cmd.Parameters.AddWithValue("@accName", acc.AccName);
                        cmd.Parameters.AddWithValue("@ownerID", acc.OwnerID);
                        cmd.Parameters.AddWithValue("@balance", acc.Balance);

                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (SQLiteException ex)
            {
                Console.WriteLine("InsertAccount error: " + ex.Message); // catches duplicate username or emails
                return false;
            }
        }

        /* SEARCH QUERY */
        public static Account? GetAccountFromAccNum(int accNum) // Return Account that holds temp data for use
        {
            Account? account = null;

            using (SQLiteConnection conn = DBConnection.Open())
            {
                using (SQLiteCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM AccountTable WHERE accNum = @accNum;";
                    cmd.Parameters.AddWithValue("@accNum", accNum);

                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            account = ReadAccount(reader);
                        }
                    }
                }
            }
            return account;
        }

        public static List<Account?> GetAllAccountsFromOwnerID(int ownerID)
        {
            List<Account?> accountList = new List<Account?>();

            using (SQLiteConnection conn = DBConnection.Open())
            {
                using (SQLiteCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT * FROM AccountTable WHERE ownerID = @ownerID;";
                    cmd.Parameters.AddWithValue("@ownerID", ownerID);

                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            accountList.Add(ReadAccount(reader));
                        }
                    }
                }
            }
            return accountList;
        }

        /* UPDATE QUERIES */    // Return bool For successful update
        public static bool UpdateAccountName(int accNum, string name)
        {
            try
            {
                using (SQLiteConnection conn = DBConnection.Open())
                {
                    using (SQLiteCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE AccountTable SET accName = @name WHERE accNum = @accNum;";
                        cmd.Parameters.AddWithValue("@name", name);
                        cmd.Parameters.AddWithValue("@accNum", accNum);

                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (SQLiteException ex)
            {
                Console.WriteLine("Update Account Name error: " + ex.Message);
                return false;
            }
        }
        public static bool UpdateBalance(int accNum, long newBal)
        {
            try
            {
                using (SQLiteConnection conn = DBConnection.Open())
                {
                    using (SQLiteCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE AccountTable SET balance = @newBal WHERE accNum = @accNum;";
                        cmd.Parameters.AddWithValue("@newBal", newBal);
                        cmd.Parameters.AddWithValue("@accNum", accNum);

                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (SQLiteException ex)
            {
                Console.WriteLine("Update Balance error: " + ex.Message);
                return false;
            }
        }
        public static bool Deactivate(int accNum)
        {
            try
            {
                using (SQLiteConnection conn = DBConnection.Open())
                {
                    using (SQLiteCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE AccountTable SET activeAcc = '0' WHERE accNum = @accNum;";
                        cmd.Parameters.AddWithValue("@accNum", accNum);

                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (SQLiteException ex)
            {
                Console.WriteLine("Deactivate Account error: " + ex.Message);
                return false;
            }
        }
        /* HELPERS */

        // builds an Account from the reader's current row
        private static Account ReadAccount(SQLiteDataReader reader)
        {
            Account account = new Account();

            account.AccNum = Convert.ToInt32(reader["accNum"]);
            account.AccName = Text(reader, "accName");
            account.OwnerID = Convert.ToInt32(reader["ownerID"]);
            account.Balance = Convert.ToInt64(reader["balance"]);
            account.Active = Convert.ToInt32(reader["activeAcc"]) == 1;

            return account;
        }

        private static string Text(SQLiteDataReader reader, string column)
        {
            return reader[column].ToString() ?? "";
        }
        public static bool accountExists(int? accNum)    // Helper for balance handling
        {
            if (accNum == null)
            {
                return false;
            }
            if (GetAccountFromAccNum((int)accNum) == null)
            {
                return false;
            }
            return true;
        }
    }
}