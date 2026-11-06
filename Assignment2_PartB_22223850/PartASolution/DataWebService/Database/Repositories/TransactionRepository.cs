using DataWebService.Controllers;
using DataWebService.Models.Entities;
using DataWebService.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using System.Data.SQLite;

namespace DataWebService.Database.Repositories
{
    public static class TransactionRepository
    {
        /* INSERT QUERY */
        public static bool InsertTransaction(Transaction transaction)   // Return bool if successful
        {
            try
            {
                using (SQLiteConnection conn = DBConnection.Open())
                {
                    using (SQLiteCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"INSERT INTO TransactionTable
                            (type, fromAcc, toAcc, description, amount, timeStamp) 
                            VALUES (@type, @fromAcc, @toAcc, @description, @amount, @timeStamp);";
                        cmd.Parameters.AddWithValue("@type", (int)transaction.Type);
                        cmd.Parameters.AddWithValue("@fromAcc", (object?)transaction.FromAcc ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@toAcc", (object?)transaction.ToAcc ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@description", (object?)transaction.Description ?? DBNull.Value);      // use DBNull instead of null to explicitly store a null value object,
                        cmd.Parameters.AddWithValue("@amount", transaction.Amount);                                         // matters for deposits and withdraws, so that a c# null doesnt get
                        cmd.Parameters.AddWithValue("@timeStamp", DateTime.UtcNow.ToString(DBConnection.TimeFormat)); // flagged as an error

                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (SQLiteException ex)
            {
                Console.WriteLine("Insert Transaction error: " + ex.Message);
                return false;
            }
        }

        /* SEARCH QUERY */
        public static Transaction? GetTransactionFromTransactionID(int transactionID)
        {
            Transaction? transaction = null;

            using (SQLiteConnection conn = DBConnection.Open())
            {
                using (SQLiteCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT * FROM TransactionTable WHERE transactionID = @ID;";
                    cmd.Parameters.AddWithValue("@ID", transactionID);

                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            transaction = ReadTransaction(reader);
                        }
                    }
                }
            }
            return transaction;
        }

        public static List<Transaction?> GetAllTransactionsFromAccID(int accID)
        {
            List<Transaction?> transactionList = new List<Transaction?>();

            using (SQLiteConnection conn = DBConnection.Open())
            {
                using (SQLiteCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT * FROM TransactionTable WHERE fromacc = @accID;";
                    cmd.Parameters.AddWithValue("@accID", accID);

                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            transactionList.Add(ReadTransaction(reader));
                        }
                    }
                }
            }
            return transactionList;
        }

        public static List<Transaction?> GetAllTransactionsFromUserID(int userID) // Return List<Transaction>
        {
            List<Transaction?> transactionList = new List<Transaction?>();

            using (SQLiteConnection conn = DBConnection.Open())
            {
                using (SQLiteCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT 
                            t.transactionID,
                            t.type,
                            t.fromAcc,
                            t.toAcc,
                            t.description,
                            t.amount,
                            t.timeStamp
                        FROM
                            TransactionTable AS t INNER JOIN AccountTable AS a
                                ON t.fromAcc = a.accNum
                                WHERE a.ownerID = @userID
                        ORDER BY t.timeStamp DESC;";
                    cmd.Parameters.AddWithValue("@userID", userID);

                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            transactionList.Add(ReadTransaction(reader));
                        }
                    }
                }
            }
            return transactionList;
        }

        // Gets balance of account for updating balance when transaction occurs
        public static long getBalanceOfAcc(int accNum)
        {
            try
            {
                using (SQLiteConnection conn = DBConnection.Open())
                {
                    using (SQLiteCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"
                        SELECT balance FROM AccountTable WHERE accNum = @accNum;";
                        cmd.Parameters.AddWithValue("@accNum", accNum);

                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return Convert.ToInt32(reader["balance"]);
                            }
                        }
                    }
                }
            }
            catch (SQLiteException ex)
            {
                Console.WriteLine("Get Balance Error: " + ex.Message);
            }
            return -1;
        }

        /* HELPERS */

        // builds a User from the reader's current row
        private static Transaction ReadTransaction(SQLiteDataReader reader)
        {
            Transaction transaction = new Transaction();

            transaction.TransactionID = Convert.ToInt32(reader["transactionID"]);
            transaction.Type = (TransactionType)Convert.ToInt32(reader["type"]);
            transaction.FromAcc = reader["fromAcc"] == DBNull.Value ? null : Convert.ToInt32(reader["fromAcc"]);
            transaction.ToAcc = reader["toAcc"] == DBNull.Value ? null : Convert.ToInt32(reader["toAcc"]);
            transaction.Description = reader["description"] == DBNull.Value ? null : Text(reader, "description");
            transaction.Amount = Convert.ToInt32(reader["amount"]);
            transaction.Timestamp = DateTime.ParseExact(Text(reader, "timeStamp"), DBConnection.TimeFormat, null);

            return transaction;
        }
        private static string Text(SQLiteDataReader reader, string column)
        {
            return reader[column].ToString() ?? "";
        }
    }
}