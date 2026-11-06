using DataWebService.Models.Entities;
using System.Data.SQLite;

namespace DataWebService.Database.Repositories
{
    public static class AuditRepository
    {
        // Insert new audit record
        public static bool InsertAudit(Audit audit)
        {
            using (SQLiteConnection conn = DBConnection.Open())
            {
                using (SQLiteCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText =
                        @"INSERT INTO AuditTable
                        (adminUsername, action, target, details, timestamp)
                        VALUES
                        (@adminUsername, @action, @target, @details, @timestamp);";

                    cmd.Parameters.AddWithValue("@adminUsername", audit.AdminUsername);
                    cmd.Parameters.AddWithValue("@action", audit.Action);
                    cmd.Parameters.AddWithValue("@target", audit.Target);
                    cmd.Parameters.AddWithValue("@details", audit.Details);
                    cmd.Parameters.AddWithValue("@timestamp", audit.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"));

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        // Retrieve all audit records
        public static List<Audit> GetAllAudits()
        {
            List<Audit> audits = new List<Audit>();

            using (SQLiteConnection conn = DBConnection.Open())
            {
                using (SQLiteCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText =
                        @"SELECT auditID,
                                 adminUsername,
                                 action,
                                 target,
                                 details,
                                 timestamp
                        FROM AuditTable
                        ORDER BY timestamp DESC;";

                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Audit audit = new Audit
                            {
                                AuditID = Convert.ToInt32(reader["auditID"]),
                                AdminUsername = reader["adminUsername"].ToString() ?? "",
                                Action = reader["action"].ToString() ?? "",
                                Target = reader["target"].ToString() ?? "",
                                Details = reader["details"].ToString() ?? "",
                                Timestamp = DateTime.Parse(reader["timestamp"].ToString() ?? "")
                            };

                            audits.Add(audit);
                        }
                    }
                }
            }

            return audits;
        }
    }
}
