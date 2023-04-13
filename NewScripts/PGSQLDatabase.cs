using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Npgsql;

namespace PBA
{
    public class PGSQLDatabase
    {
        private Dictionary<string, Type> dbUsersValues = new Dictionary<string, Type>();
        private Dictionary<string, Type> dbTeamsValues = new Dictionary<string, Type>();
        private Dictionary<string, Type> dbTournamentsValues = new Dictionary<string, Type>();
        private Dictionary<string, Type> dbBattlesValues = new Dictionary<string, Type>();
        private Dictionary<string, Type> dbFightersValues = new Dictionary<string, Type>();
        private Dictionary<string, Type> dbUserPokesValues = new Dictionary<string, Type>();
        private string connString;

        public PGSQLDatabase(string server, string port, string db, string userid, string password)
        {
            connString = $"Server={server};Port={port};Database={db};User Id={userid};Password={password}";
            DatabaseInitialize();
        }

        public async Task<List<string[]>> DatabaseReader(string query)
        {
            List<string[]> response = new List<string[]>();

            try
            {
                NpgsqlConnection conn = new NpgsqlConnection(connString);
                await conn.OpenAsync();


                string sql = query;
                
                using var cmd = new NpgsqlCommand(sql, conn);

                using NpgsqlDataReader rdr = cmd.ExecuteReader();

                int cols = rdr.FieldCount;
                List<string[]> vals = new List<string[]>();
                while (rdr.Read())
                {
                    string[] rowvals = new string[cols];
                    for (int i = 0; i < cols; i++)
                    {
                        rowvals[i] = rdr.GetValue(i).ToString();
                    }
                    vals.Add(rowvals);
                }
                response = vals;
                await conn.CloseAsync();

            }
            catch (Exception ex)
            {
                if (ex is Npgsql.PostgresException)
                {
                    System.Diagnostics.Debug.WriteLine("PGAdmin Error: " + ex.Message);
                }
                else if(ex is Npgsql.NpgsqlException)
                {
                    System.Diagnostics.Debug.WriteLine("PGAdmin Connection Error: " + ex.Message);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("PGAdmin Error: " + ex.Message);
                }
            }

            return response;
        }
        public async Task<List<string[]>> DatabaseReader(string tableName, string[] columns, int keyrow)
        {
            List<string[]> response = new List<string[]>();

            try
            {
                NpgsqlConnection conn = new NpgsqlConnection(connString);
                await conn.OpenAsync();
                

                string sql = "";
                string sqlSelect = "";
                string sqlWhere = "_id = " + keyrow;

                if (columns[0] != "*")
                {
                    for (int i = 0; i < columns.Length; i++)
                    {
                        sqlSelect += columns[i];
                        if (i < columns.Length - 1)
                        {
                            sqlSelect += ", ";
                        }
                    }
                }
                else
                {
                    sqlSelect = "*";
                }

                sql = "SELECT " + sqlSelect + " FROM " + tableName + " WHERE " + sqlWhere;

                using var cmd = new NpgsqlCommand(sql, conn);

                using NpgsqlDataReader rdr = cmd.ExecuteReader();
                int cols = rdr.FieldCount;

                while (rdr.Read())
                {
                    string[] vals = new string[cols];
                    for (int i = 0; i < cols; i++)
                    {
                        vals[i] = rdr.GetValue(i).ToString();
                    }
                    response.Add(vals);
                }

                await conn.CloseAsync();

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("PGAdmin Connection Error: " + ex.Message);
            }

            return response;
        }

        /// <summary>
        /// Adds a new user to the users database
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public async Task DatabaseAddNewUser(string username)
        {
            try
            {
                NpgsqlConnection conn = new NpgsqlConnection(connString);
                await conn.OpenAsync();

                string sql = "INSERT INTO users(username, balance, joindate) VALUES(@username, @balance, @joindate)";

                using var cmd = new NpgsqlCommand(sql, conn);

                cmd.Parameters.AddWithValue("username", username);
                cmd.Parameters.AddWithValue("balance", 200);
                cmd.Parameters.AddWithValue("joindate", DateTime.Today);
                await cmd.PrepareAsync();
                await cmd.ExecuteNonQueryAsync();

                await conn.CloseAsync();

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("PGAdmin Connection Error: " + ex.Message);
            }
        }

        /// <summary>
        /// Checks for new or existing user
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public async Task NewUserCheck(string username)
        {
            List<string[]> check = await DatabaseReader($"SELECT * FROM users WHERE username = '{username}'");

            if (check.Count == 0)
            {
                await DatabaseAddNewUser(username);
            }

        }

        /// <summary>
        /// Updates new balance for user in DB after fight ends.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="balance"></param>
        /// <returns></returns>
        public async Task DatabaseUpdateUserBalance(string username, int balance)
        {
            try
            {
                NpgsqlConnection conn = new NpgsqlConnection(connString);
                await conn.OpenAsync();

                string sql = "UPDATE users SET balance = @balance WHERE username = @username";

                using var cmd = new NpgsqlCommand(sql, conn);

                cmd.Parameters.AddWithValue("username", username);
                cmd.Parameters.AddWithValue("balance", balance);
                await cmd.PrepareAsync();
                await cmd.ExecuteNonQueryAsync();

                await conn.CloseAsync();

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("PGAdmin Connection Error: " + ex.Message);
            }
        }
        
        private void DatabaseInitialize()
        {
            try
            {
                NpgsqlConnection conn = new NpgsqlConnection(connString);
                conn.Open();

                string sql1 = "SELECT * FROM users WHERE userid = 1";
                string sql2 = "SELECT * FROM teams WHERE teamid = 1";
                string sql3 = "SELECT * FROM tournaments WHERE tournamentid = 1";
                string sql4 = "SELECT * FROM battles WHERE battleid = 1";
                string sql5 = "SELECT * FROM fighers WHERE fighterid = 1";
                string sql6 = "SELECT * FROM userpokes WHERE userid = 1";
                using var cmd1 = new NpgsqlCommand(sql1, conn);
                using NpgsqlDataReader rdr1 = cmd1.ExecuteReader();
                using var cmd2 = new NpgsqlCommand(sql2, conn);
                using NpgsqlDataReader rdr2 = cmd2.ExecuteReader();
                using var cmd3 = new NpgsqlCommand(sql3, conn);
                using NpgsqlDataReader rdr3 = cmd3.ExecuteReader();
                using var cmd4 = new NpgsqlCommand(sql4, conn);
                using NpgsqlDataReader rdr4 = cmd4.ExecuteReader();
                using var cmd5 = new NpgsqlCommand(sql5, conn);
                using NpgsqlDataReader rdr5 = cmd5.ExecuteReader();
                using var cmd6 = new NpgsqlCommand(sql6, conn);
                using NpgsqlDataReader rdr6 = cmd6.ExecuteReader();

                string[] userColNames = new string[rdr1.FieldCount];
                string[] teamColNames = new string[rdr2.FieldCount];
                string[] tourColNames = new string[rdr3.FieldCount];
                string[] battColNames = new string[rdr4.FieldCount];
                string[] fighterColNames = new string[rdr5.FieldCount];
                string[] userpokesColNames = new string[rdr6.FieldCount];

                for (int i = 0; i < rdr1.FieldCount; i++)
                {
                    userColNames[i] = rdr1.GetName(i);
                }
                for (int i = 0; i < rdr2.FieldCount; i++)
                {
                    teamColNames[i] = rdr2.GetName(i);
                }
                for (int i = 0; i < rdr3.FieldCount; i++)
                {
                    tourColNames[i] = rdr3.GetName(i);
                }
                for (int i = 0; i < rdr4.FieldCount; i++)
                {
                    battColNames[i] = rdr4.GetName(i);
                }
                for (int i = 0; i < rdr5.FieldCount; i++)
                {
                    fighterColNames[i] = rdr5.GetName(i);
                }
                for (int i = 0; i < rdr6.FieldCount; i++)
                {
                    userpokesColNames[i] = rdr6.GetName(i);
                }

                while (rdr1.Read())
                {
                    for (int i = 0; i < userColNames.Length; i++)
                    {
                        dbUsersValues.Add(userColNames[i], rdr1.GetType());
                    }
                }
                while (rdr2.Read())
                {
                    for (int i = 0; i < teamColNames.Length; i++)
                    {
                        dbTeamsValues.Add(teamColNames[i], rdr2.GetType());
                    }
                }
                while (rdr3.Read())
                {
                    for (int i = 0; i < tourColNames.Length; i++)
                    {
                        dbTournamentsValues.Add(tourColNames[i], rdr3.GetType());
                    }
                }
                while (rdr4.Read())
                {
                    for (int i = 0; i < battColNames.Length; i++)
                    {
                        dbBattlesValues.Add(battColNames[i], rdr4.GetType());
                    }
                }
                while (rdr5.Read())
                {
                    for (int i = 0; i < fighterColNames.Length; i++)
                    {
                        dbFightersValues.Add(fighterColNames[i], rdr5.GetType());
                    }
                }
                while (rdr6.Read())
                {
                    for (int i = 0; i < userpokesColNames.Length; i++)
                    {
                        dbUserPokesValues.Add(userpokesColNames[i], rdr6.GetType());
                    }
                }

                conn.Close();

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("PGAdmin Connection Error: " + ex.Message);
            }
        }

        private Type DatabaseGetColumnType(Dictionary<string, Type> table, string colName)
        {
            return table[colName];
        }
    }
}
