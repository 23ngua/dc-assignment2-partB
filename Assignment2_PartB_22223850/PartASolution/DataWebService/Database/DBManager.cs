using System.Data.SQLite;

namespace DataWebService.Database
{
    public class DBManager
    {
        //  TABLE INIT
        public static bool CreateTables()
        {
            try
            {
                using var connection = DBConnection.Open();
                using var command = connection.CreateCommand();
                {
                    // CREATE USER TABLE
                    command.CommandText = @"
                        CREATE TABLE IF NOT EXISTS UserTable (
                            userID INTEGER PRIMARY KEY AUTOINCREMENT,
                            username TEXT NOT NULL UNIQUE,
                            fName TEXT NOT NULL,
                            lName TEXT NOT NULL,
                            email TEXT NOT NULL UNIQUE,
                            address TEXT NOT NULL,
                            phone TEXT NOT NULL,
                            picture TEXT,
                            password TEXT NOT NULL,
                            role INTEGER NOT NULL,
                            activeUser NUMERIC DEFAULT 1
                        );";
                    command.ExecuteNonQuery();
                    Console.WriteLine("User Table Created Successfully");

                    // CREATE ACCOUNT TABLE
                    command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS AccountTable (
                            accNum INTEGER PRIMARY KEY AUTOINCREMENT,
                            accName TEXT NOT NULL,
                            ownerID INTEGER NOT NULL,
                            balance INTEGER NOT NULL,
                            activeAcc NUMERIC DEFAULT 1,
                            FOREIGN KEY(ownerID) REFERENCES UserTable(userID)
                        );";
                    command.ExecuteNonQuery();
                    Console.WriteLine("Account Table Created Successfully");

                    // CREATE TRANSACTION TABLE
                    command.CommandText = @"
                        CREATE TABLE IF NOT EXISTS TransactionTable (
                            transactionID INTEGER PRIMARY KEY AUTOINCREMENT,
                            type INTEGER NOT NULL,
                            fromAcc INTEGER,
                            toAcc INTEGER,
                            description TEXT,
                            amount INTEGER NOT NULL CHECK(amount > 0),
                            timeStamp TEXT NOT NULL,
                            FOREIGN KEY(fromAcc) REFERENCES AccountTable(accNum),
                            FOREIGN KEY(toAcc) REFERENCES AccountTable(accNum)
                        );";
                    command.ExecuteNonQuery();
                    Console.WriteLine("Transaction Table Created Successfully");

                    // CREATE AUDIT TABLE
                    command.CommandText = @"
                        CREATE TABLE IF NOT EXISTS AuditTable (
                            auditID INTEGER PRIMARY KEY AUTOINCREMENT,
                            adminUsername TEXT NOT NULL,
                            action TEXT NOT NULL,
                            target TEXT NOT NULL,
                            details TEXT NOT NULL,
                            timestamp TEXT NOT NULL
                        );";
                    command.ExecuteNonQuery();
                    Console.WriteLine("Audit Table Created Successfully");
                }
                Console.WriteLine("Tables Created Successfully");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            return false;
        }

        public static void DBInitialize()
        {
            if (CreateTables())
            {
                DBSeed.Seed();
            }
        }
    }
}