using MySql.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;


namespace TaskApp.Models
{
    public static class CommitCollection
    {

        public static void AddCommit(Commit commit)
        {
            MySqlConnectionStringBuilder connectionStringBuilder = new MySqlConnectionStringBuilder();
            connectionStringBuilder.Server = "localhost";
            connectionStringBuilder.UserID = "root";
            connectionStringBuilder.Password = "mysql";
            connectionStringBuilder.Database = "restoreddb";
            MySqlConnection connection = new MySqlConnection(connectionStringBuilder.ToString());
            string name = commit.Author.Name;
            string username = commit.Author.Username;
            string email = commit.Author.Email;
            int messageid = Convert.ToInt32(commit.GetTaskIdFromMessage());
            string addquery = $"INSERT INTO commitinfo(uname, username, email, taskid) VALUES ('{name}', '{username}', '{email}', {messageid})";
            try
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand(addquery, connection);
                command.ExecuteNonQuery();
                connection.Close();
            }
            catch
            {
                throw new Exception("error");
            }
        }

          public static IEnumerable<Commit> GetCommits()
          {
              return new List<Commit>();
          }
          
    }
}
