using DataWebService.Models.Entities;
using DataWebService.Models.Enums;
using System.Data.SQLite;

namespace DataWebService.Database.Repositories
{
    public static class UserRepository
    {
        /* INSERT QUERY */
        public static bool InsertUser(User user)
        {
            try
            {
                using (SQLiteConnection conn = DBConnection.Open())
                {
                    using (SQLiteCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = (@"
                            INSERT INTO UserTable (username, fName, lName, email, address, phone, picture, password, role, activeUser)
                            VALUES (@username, @fName, @lName, @email, @address, @phone, NULL, @password, @role, 1)");
                        cmd.Parameters.AddWithValue("@username", user.Username);
                        cmd.Parameters.AddWithValue("@fName", user.FName);
                        cmd.Parameters.AddWithValue("@lName", user.LName);
                        cmd.Parameters.AddWithValue("@email", user.Email);
                        cmd.Parameters.AddWithValue("@address", user.Address);
                        cmd.Parameters.AddWithValue("@phone", user.PhoneNum);
                        cmd.Parameters.AddWithValue("@picture", (object?)user.Picture ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@password", user.Password);
                        cmd.Parameters.AddWithValue("@role", (int)user.Role);

                        if (cmd.ExecuteNonQuery() == 0)
                        {
                            return false;
                        }

                        user.UserID = (int)conn.LastInsertRowId;
                        return true;
                    }
                }
            }
            catch (SQLiteException ex)
            {
                Console.WriteLine("InsertUser error: " + ex.Message); // catches duplicate username or emails
                return false;
            }
        }

        /* SEARCH QUERIES */

        public static User? GetUserFromUsername(string userName)
        {
            User? user = null;

            using (SQLiteConnection conn = DBConnection.Open())
            {
                using (SQLiteCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM UserTable WHERE username = @userName;";
                    cmd.Parameters.AddWithValue("@userName", userName);

                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            user = ReadUser(reader);
                        }
                    }
                }
            }
            return user;
        }
        // ADMIN STUFF
        public static User? GetUserFromEmail(string email)
        {
            User? user = null;

            using (SQLiteConnection conn = DBConnection.Open())
            {
                using (SQLiteCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM UserTable WHERE email = @email;";
                    cmd.Parameters.AddWithValue("@email", email);

                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            user = ReadUser(reader);
                        }
                    }
                }
            }
            return user;
        }

        public static User? GetUserFromUserID(int userId)
        {
            User? user = null;

            using (SQLiteConnection conn = DBConnection.Open())
            {
                using (SQLiteCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM UserTable WHERE userID = @userID;";
                    cmd.Parameters.AddWithValue("@userID", userId);

                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            user = ReadUser(reader);
                        }
                    }
                }
            }

            return user;
        }

        /* UPDATE QUERIES */

        public static bool UpdateUser(User user)
        {
            try
            {
                using (SQLiteConnection conn = DBConnection.Open())
                {
                    using (SQLiteCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"
                            UPDATE UserTable
                            SET fName = @fName, lName = @lName, email = @email, address = @address, phone = @phone, picture = @picture
                            WHERE userID = @userID;";

                        cmd.Parameters.AddWithValue("@fName", user.FName);
                        cmd.Parameters.AddWithValue("@lName", user.LName);
                        cmd.Parameters.AddWithValue("@email", user.Email);
                        cmd.Parameters.AddWithValue("@address", user.Address);
                        cmd.Parameters.AddWithValue("@phone", user.PhoneNum);
                        cmd.Parameters.AddWithValue("@picture", (object?)user.Picture ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@userID", user.UserID);

                        if (cmd.ExecuteNonQuery() == 0)
                        {
                            return false;
                        }
                        return true;
                    }
                }
            }
            catch (SQLiteException ex)
            {
                Console.WriteLine("Update error: " + ex.Message);
                return false;
            }
        }
        //public static bool UpdatePicture(string userName, string filePath)
        //{
        //    using var conn = DBConnection.Open();
        //    using var cmd = conn.CreateCommand();
        //    // 
        //    cmd.CommandText = @"
        //        UPDATE UserTable SET picture = @filePath WHERE userName = @userName;";
        //    cmd.ExecuteNonQuery();
        //    Console.WriteLine("");

        //    throw new NotImplementedException();
        //}

        //public static bool UpdateEmail(string userName, string newEmail)
        //{
        //    using var conn = DBConnection.Open();
        //    using var cmd = conn.CreateCommand();
        //    // 
        //    cmd.CommandText = @"
        //        UPDATE UserTable SET email = @newEmail WHERE userName = @userName;";
        //    cmd.ExecuteNonQuery();
        //    Console.WriteLine("");

        //    throw new NotImplementedException();
        //}
        //public static bool UpdatePhone(string userName, string phoneNumber)
        //{
        //    using var conn = DBConnection.Open();
        //    using var cmd = conn.CreateCommand();
        //    // 
        //    cmd.CommandText = @"
        //        UPDATE UserTable SET phone = @phoneNumber WHERE userName = @userName;";
        //    cmd.ExecuteNonQuery();
        //    Console.WriteLine("");

        //    throw new NotImplementedException();
        //}
        public static bool UpdatePassword(string userName, string newPassword)
        {
            try
            {
                using (SQLiteConnection conn = DBConnection.Open())
                {
                    using (SQLiteCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"
                            UPDATE UserTable SET password = @newPassword WHERE username = @userName;";
                        cmd.Parameters.AddWithValue("@newPassword", newPassword);
                        cmd.Parameters.AddWithValue("@userName", userName);

                        if (cmd.ExecuteNonQuery() == 0)
                        {
                            return false;
                        }
                        return true;
                    }
                }
            }
            catch (SQLiteException ex)
            {
                Console.WriteLine("Update password error: " + ex.Message);
                return false;
            }
        }

        public static bool Deactivate(string userName)   // Maybe make it a toggle if want (currently only deactivates)
        {
            try
            {
                using (SQLiteConnection conn = DBConnection.Open())
                {
                    using (SQLiteCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"
                            UPDATE UserTable SET activeUser = 0 WHERE userName = @userName;";

                        cmd.Parameters.AddWithValue("@userName", userName);

                        if (cmd.ExecuteNonQuery() == 0)
                        {
                            return false;
                        }
                        return true;
                    }
                }
            }
            catch (SQLiteException ex)
            {
                Console.WriteLine("Deactivate User error: " + ex.Message);
                return false;
            }
        }

        /* HELPERS */

        // builds a User from the reader's current row
        private static User ReadUser(SQLiteDataReader reader)
        {
            User user = new User();

            user.UserID = Convert.ToInt32(reader["userID"]);
            user.Username = Text(reader, "username");
            user.FName = Text(reader, "fName");
            user.LName = Text(reader, "lName");
            user.Email = Text(reader, "email");
            user.Address = Text(reader, "address");
            user.PhoneNum = Text(reader, "phone");
            user.Picture = reader["picture"] == DBNull.Value ? null : Text(reader, "picture");
            user.Password = Text(reader, "password");
            user.Role = (UserRole)Convert.ToInt32(reader["role"]);
            user.Active = Convert.ToInt32(reader["activeUser"]) == 1;

            return user;
        }

        private static string Text(SQLiteDataReader reader, string column)
        {
            return reader[column].ToString() ?? "";
        }

    }
}