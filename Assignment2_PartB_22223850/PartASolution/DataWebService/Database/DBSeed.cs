using Bogus;
using DataWebService.Models.Entities;
using DataWebService.Models.Enums;
using Microsoft.AspNetCore.Identity;
using System.Data.SQLite;

namespace DataWebService.Database
{
    /* Fills empty database with generated data.
     * Using Bogus, the mock library helps generate random values for users, accts, and transactions
     * A total of 20 Users, 30 Accts, and 500 Transactions are made, these can be changed in the static variables
    */
    internal static class DBSeed
    {
        private const int RoleCustomer = 0;
        private const int RoleAdmin = 1;
        private const int NumOfUsers = 20;
        private const int NumOfAccts = 30;
        private const int NumOfTrnsc = 500;

        private const string AdminPassword = "Admin123!";
        private const string CustomerPassword = "Password1!";

        private static readonly string[] AccountNames = { "Everyday", "Savings", "Holiday Fund", "Bills", "Emergency Fund" };

        public static void Seed()
        {
            using var conn = DBConnection.Open();

            // chack and only seed an empty database
            using (var check = conn.CreateCommand())
            {
                check.CommandText = "SELECT COUNT(*) FROM UserTable";
                if (Convert.ToInt32(check.ExecuteScalar()) > 0)
                {
                    Console.WriteLine("Seed skipped: users already exist");
                    return;
                }
            }

            // set a seed value, this can be changed to randomise new data
            Randomizer.Seed = new Random(8675309);
            var faker = new Faker("en_AU"); // set locale
            using var tx = conn.BeginTransaction();

            try
            {
                /* USERS */
                var hasher = new PasswordHasher<User>();
                string adminHash = hasher.HashPassword(new User(), AdminPassword); // PBKDF2 hash (random 16 byte salt also stored inside string)

                var usernames = new HashSet<string> { "admin" };
                var customers = new List<int>();

                InsertUser(conn, tx, "admin", "Admin", "McAdmin", "admin@dcbank.com", "1 Admin St, Perth WA 6000", "0400111222", RoleAdmin, adminHash);


                while (customers.Count < NumOfUsers - 1)
                {
                    string first = faker.Name.FirstName();
                    string last = faker.Name.LastName();
                    string username = faker.Internet.UserName(first, last).ToLower();
                    // try insert user to hashset and if its a duplicate user, skip them
                    if (!usernames.Add(username))
                    {
                        continue;
                    }

                    string address = $"{faker.Address.StreetAddress()}," +
                                     $" {faker.Address.City()} " +
                                     $"{faker.Address.StateAbbr()} " +
                                     $"{faker.Address.ZipCode()}";
                    string phone = faker.Phone.PhoneNumber("04########");

                    string customerHash = hasher.HashPassword(new User(), CustomerPassword); // new random salt for every customer
                    InsertUser(conn, tx, username, first, last, $"{username}@test.com", address, phone, RoleCustomer, customerHash);
                    customers.Add((int)conn.LastInsertRowId); // add userID to list
                }

                /* ACCOUNTS */
                var accounts = new List<int>();
                var balances = new Dictionary<int, long>();

                // one acc for every customer
                for (int i = 0; i < customers.Count; i++)
                {
                    int ownerID = customers[i];
                    InsertAccount(conn, tx, ownerID, faker.PickRandom(AccountNames));
                    int accNum = (int)conn.LastInsertRowId;
                    accounts.Add(accNum);
                    balances[accNum] = 0;
                }

                // randomise rest of accs to users
                for (int i = accounts.Count; i < NumOfAccts; i++)
                {
                    int ownerID = faker.PickRandom(customers);
                    InsertAccount(conn, tx, ownerID, faker.PickRandom(AccountNames));
                    int accNum = (int)conn.LastInsertRowId;
                    accounts.Add(accNum);
                    balances[accNum] = 0;
                }

                /* TRANSACTIONS */
                // time format is YYYY-MM-DD HH:MM:SS 
                DateTime now = DateTime.UtcNow;
                DateTime firstDay = now.AddDays(-90); // first transaction start 90 days ago
                DateTime openingEnd = firstDay.AddDays(1);

                // 1. opening deposit for every account
                foreach (int acc in accounts)
                {
                    long amount = faker.Random.Long(50_000, 1_000_000); // $500 - $10,000
                    balances[acc] += amount;
                    InsertTransaction(conn, tx, TransactionType.Deposit, null, acc, amount, faker.Date.Between(firstDay, firstDay.AddDays(1))); // one day set for first round of deposits
                }

                // 2. other transaction types, random time given for each
                int remaining = NumOfTrnsc - accounts.Count;
                var times = new List<DateTime>();
                for (int i = 0; i < remaining; i++)
                {
                    times.Add(faker.Date.Between(firstDay.AddDays(1), now));
                }
                times.Sort();

                foreach (DateTime when in times)
                {
                    int D20 = faker.Random.Int(1, 20);
                    int srcAcc = faker.PickRandom(accounts);
                    long maxSpend = Math.Min(balances[srcAcc], 50_000);

                    // deposit (25% chance)
                    if (D20 <= 5 || maxSpend < 100)
                    {
                        long amount = faker.Random.Long(1_000, 100_000);
                        balances[srcAcc] += amount;
                        InsertTransaction(conn, tx, TransactionType.Deposit, null, srcAcc, amount, when);
                    }
                    // withdrawal (45% chance)
                    else if (D20 <= 14)
                    {
                        long amount = faker.Random.Long(100, maxSpend);
                        balances[srcAcc] -= amount;
                        InsertTransaction(conn, tx, TransactionType.Withdrawal, srcAcc, null, amount, when);
                    }
                    // transfers (30% chance)
                    else
                    {
                        int dest = faker.PickRandom(accounts.Where(a => a != srcAcc).ToList());
                        long amount = faker.Random.Long(100, maxSpend);
                        balances[srcAcc] -= amount;
                        balances[dest] += amount;
                        InsertTransaction(conn, tx, TransactionType.Transfer, srcAcc, dest, amount, when);
                    }
                }

                /* FINAL ADJUSTMENT */
                foreach (int acc in accounts)
                {
                    bool active = !faker.Random.Bool(0.05f);
                    using var cmd = new SQLiteCommand(
                        "UPDATE AccountTable " +
                        "SET balance = @balance, activeAcc = @active " +
                        "WHERE accNum = @acc",
                        conn, tx);
                    cmd.Parameters.AddWithValue("@balance", balances[acc]);
                    cmd.Parameters.AddWithValue("@active", active ? 1 : 0);
                    cmd.Parameters.AddWithValue("@acc", acc);
                    cmd.ExecuteNonQuery();
                }

                tx.Commit();
                Console.WriteLine($"Database seeded: {NumOfUsers} users, {NumOfAccts} accounts, {NumOfTrnsc} transactions");
            }
            catch (Exception ex)
            {
                tx.Rollback();
                Console.WriteLine("Seeding failed, rolled back: " + ex.Message);
                throw;
            }
        }


        /* --- helpers --- */

        private static void InsertUser(SQLiteConnection conn, SQLiteTransaction tx, string username, string fName, string lName, string email, string address, string phone, int role, string passwordHash)
        {
            using var cmd = new SQLiteCommand(@"
                INSERT INTO UserTable (username, fName, lName, email, address, phone, picture, password, role, activeUser)
                VALUES (@username, @fName, @lName, @email, @address, @phone, NULL, @password, @role, 1)",
                conn, tx);
            cmd.Parameters.AddWithValue("@username", username);
            cmd.Parameters.AddWithValue("@fName", fName);
            cmd.Parameters.AddWithValue("@lName", lName);
            cmd.Parameters.AddWithValue("@email", email);
            cmd.Parameters.AddWithValue("@address", address);
            cmd.Parameters.AddWithValue("@phone", phone);
            cmd.Parameters.AddWithValue("@password", passwordHash);
            cmd.Parameters.AddWithValue("@role", role);
            cmd.ExecuteNonQuery();
        }

        private static void InsertAccount(SQLiteConnection conn, SQLiteTransaction tx, int ownerID, string accName)
        {
            using var cmd = new SQLiteCommand(@"
                INSERT INTO AccountTable (accName, ownerID, balance, activeAcc)
                VALUES (@accName, @ownerID, 0, 1)",
                conn, tx);
            cmd.Parameters.AddWithValue("@accName", accName);
            cmd.Parameters.AddWithValue("@ownerID", ownerID);
            cmd.ExecuteNonQuery();
        }

        private static void InsertTransaction(SQLiteConnection conn, SQLiteTransaction tx, TransactionType type, int? fromAcc, int? toAcc, long amount, DateTime when)
        {
            using var cmd = new SQLiteCommand(@"
                INSERT INTO TransactionTable (type, fromAcc, toAcc, amount, timeStamp)
                VALUES (@type, @from, @to, @amount, @ts)",
                conn, tx);
            cmd.Parameters.AddWithValue("@type", (int)type);
            cmd.Parameters.AddWithValue("@from", (object?)fromAcc ?? DBNull.Value); // use DBNull instead of null to explicitly store a null value object,
            cmd.Parameters.AddWithValue("@to", (object?)toAcc ?? DBNull.Value);     // matters for deposits and withdraws, so that a c# null doesnt get
            cmd.Parameters.AddWithValue("@amount", amount);                         // flagged as an error
            cmd.Parameters.AddWithValue("@ts", when.ToString(DBConnection.TimeFormat));
            cmd.ExecuteNonQuery();
        }
    }
}