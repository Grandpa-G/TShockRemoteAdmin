using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;
using TShockAPI;
using TShockAPI.DB;
using System.Net;
using MySql.Data.MySqlClient;

namespace extraAdminREST.DB
{
    public class ExtendedLog
    {
        private IDbConnection database;

        public ExtendedLog(IDbConnection db)
        {
            database = db;

            var table = new SqlTable("ExtendedLog",
                                     new SqlColumn("recId", MySqlDbType.Int32) { Primary = true, AutoIncrement = true },
                                     new SqlColumn("Log", MySqlDbType.Text)                                     
                );
            var creator = new SqlTableCreator(db,
                                              db.GetSqlType() == SqlType.Sqlite
                                                ? (IQueryBuilder)new SqliteQueryCreator()
                                                : new MysqlQueryCreator());
            try
            {
                creator.EnsureExists(table);
            }
            catch (DllNotFoundException)
            {
                System.Console.WriteLine("Possible problem with your database - is Sqlite3.dll present?");
                throw new Exception("Could not find a database library (probably Sqlite3.dll)");
            }
        }

    }
    public class SSCTransfer
    {
        		private IDbConnection database;

         public SSCTransfer(IDbConnection db)
		{
			database = db;

			var table = new SqlTable("SSCTransferLog",
			                         new SqlColumn("recId", MySqlDbType.Int32) {Primary = true, AutoIncrement = true},
			                         new SqlColumn("TimeStamp", MySqlDbType.Text),
			                         new SqlColumn("Account", MySqlDbType.Int32),
                                     new SqlColumn("User", MySqlDbType.Text),
                                     new SqlColumn("OldInventory", MySqlDbType.Text),
			                         new SqlColumn("NewInventory", MySqlDbType.Text),
                                     new SqlColumn("ChangedBy", MySqlDbType.Text)
				);
			var creator = new SqlTableCreator(db,
			                                  db.GetSqlType() == SqlType.Sqlite
			                                  	? (IQueryBuilder) new SqliteQueryCreator()
			                                  	: new MysqlQueryCreator());
			try
			{
				creator.EnsureExists(table);
			}
			catch (DllNotFoundException)
			{
				System.Console.WriteLine("Possible problem with your database - is Sqlite3.dll present?");
				throw new Exception("Could not find a database library (probably Sqlite3.dll)");
			}
		}

         /// <summary>
		/// Gets a list of SSCInventoryLog entries.
		/// </summary>
         public List<SSCInventory> GetSSCInventoryLog()
		{
            List<SSCInventory> SSCInventorylist = new List<SSCInventory>();
			try
			{
                using (var reader = database.QueryReader("SELECT * FROM SSCTransferLog order by timestamp DESC limit 20"))
				{
					while (reader.Read())
					{
                        SSCInventorylist.Add(new SSCInventory(reader.Get<Int32>("Account"), reader.Get<string>("TimeStamp"), reader.Get<string>("User"), reader.Get<string>("OldInventory"), reader.Get<string>("NewInventory"), reader.Get<string>("ChangedBy")));
                     }
                    return SSCInventorylist;
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex.ToString());
				Console.WriteLine(ex.StackTrace);
			}
			return null;
		}

		/// <summary>
		/// Gets a ban by name.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="casesensitive">Whether to check with case sensitivity.</param>
		/// <returns>The ban.</returns>
         public SSCInventory GetSSCInventoryByName(string name, bool casesensitive = true)
		{
			try
			{
				var namecol = casesensitive ? "Name" : "UPPER(Name)";
				if (!casesensitive)
					name = name.ToUpper();
                using (var reader = database.QueryReader("SELECT * FROM SSCTransferLog WHERE " + namecol + "=@0", name))
				{
					if (reader.Read())
                        return new SSCInventory(reader.Get<Int32>("Account"), reader.Get<string>("TimeStamp"), reader.Get<string>("User"), reader.Get<string>("OldInventory"), reader.Get<string>("NewInventory"), reader.Get<string>("ChangedBy"));
                }
			}
			catch (Exception ex)
			{
				Log.Error(ex.ToString());
			}
			return null;
		}

#if COMPAT_SIGS
		[Obsolete("This method is for signature compatibility for external code only")]
		public bool AddBan(string ip, string name, string reason)
		{
			return AddBan(ip, name, "", reason, false, "", "");
		}
#endif

        public bool ModifySSCInventory(Int32 account, string user, string newInventory, string changedBy, bool exceptions)
		{
            String oldInventory = "";
            try
            {
                using (var reader = database.QueryReader("SELECT * from tsCharacter WHERE Account=@0", account))
                {
                    if (reader.Read())
                    {
                          oldInventory = reader.Get<string>("Inventory");
                     }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }

            try
            {
                using (var reader = database.QueryReader("UPDATE tsCharacter set inventory = @1 WHERE Account=@0", account, newInventory))
                {
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
            try
			{
                return database.Query("INSERT INTO SSCTransferLog (Account, TimeStamp, User, OldInventory, NewInventory, ChangedBy) VALUES (@0, @1, @2, @3, @4, @5);", account, DateTime.UtcNow.ToString("s"), user, oldInventory, newInventory, changedBy) != 0;
			}
			catch (Exception ex)
			{
				if (exceptions)
					throw ex;
				Log.Error(ex.ToString());
			}
			return false;
		}

#if COMPAT_SIGS
		[Obsolete("This method is for signature compatibility for external code only")]
		public bool RemoveBan(string ip)
		{
			return RemoveBan(ip, false, true, false);
		}
#endif
        public bool ClearSSCInventory()
		{
			try
			{
                return database.Query("DELETE FROM SSCTransferLog") != 0;
			}
			catch (Exception ex)
			{
				Log.Error(ex.ToString());
			}
			return false;
		}
	}

	public class SSCInventory
	{
        public Int32 Account { get; set; }

        public string TimeStamp { get; set; }

        public string User { get; set; }

        public string OldInventory { get; set; }

        public string NewInventory { get; set; }

        public string ChangedBy { get; set; }

        public SSCInventory(Int32 account, string timestamp, string user, string oldinventory, string newinventory, string changedby)
		{
			Account = account;
            TimeStamp = timestamp;
			User = user;
            OldInventory = oldinventory;
            NewInventory = newinventory;
            ChangedBy = changedby;
		}

        public SSCInventory()
		{
           Account = 0;
            TimeStamp = string.Empty;
            User = string.Empty;
            OldInventory = "";
            NewInventory = "";
            ChangedBy = string.Empty;
        }
	}
}