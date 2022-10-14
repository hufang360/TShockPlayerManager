using System.Collections.Generic;
using System;
using System.Data;
using System.Linq;
using TShockAPI;
using TShockAPI.DB;

namespace Plugin
{
    public class Util
    {
        public static FoundPlayer GetPlayer(TSPlayer op, String name)
        {
            var found = new FoundPlayer();

            var players = GetPlr(name);
             if (players.Count>1 && players[0].Name!=name)
            {
                op.SendMultipleMatchError(players);
            }
            else if (players.Any())
            {
                // plr = players[0];
                found.SetOnline(players[0]);
            }
            else
            {
                // 离线玩家
                var offline = GetUserAccounts(name);
                if (offline.Count ==0)
                {
                    op.SendErrorMessage($"找不到名为 {name} 的玩家!");
                }
                else if (offline.Count > 1 && offline[0].Name!=name)
                {
                    op.SendMultipleMatchError(offline);
                }
                else if (offline.Any())
                {
                    found.SetOffline(offline[0].ID, offline[0].Name);
                }
            }

            return found;
        }

        private static List<TSPlayer> GetPlr (string name)
        {
            // TSPlayer.FindByNameOrID($"tsn:{name}");
            var found = new List<TSPlayer>();
			foreach (TSPlayer player in TShock.Players)
			{
				if (player != null && player.Name==name)
                {
                    found.Add(player);
				}
			}
			return found;
        }

        private static List<UserAccountLite> GetUserAccounts (string name)
        {
            List<UserAccountLite> found = new List<UserAccountLite>();
            try
            {
                IDbConnection db = TShock.CharacterDB.database;
                using (var reader = db.QueryReader("SELECT * FROM Users WHERE Username = @0", name))
                {
                    while (reader.Read())
                    {
                        found.Add(LoadUserAccountFromResult(reader));
                    }
                }
                // return found;
            }
            catch (Exception ex)
            {
                TShock.Log.Error(ex.ToString());
            }
            return found;
        }

        private static UserAccountLite LoadUserAccountFromResult(QueryResult result)
		{
            UserAccountLite account = new UserAccountLite();
			account.ID = result.Get<int>("ID");
			account.Name = result.Get<string>("Username");
			return account;
		}

        public static string CFlag(bool foo, string fstr) { return foo ? $"[c/96FF96:✔{fstr}]" : $"-{fstr}"; }
        public static string CFlag(string fstr, bool foo) { return foo ? $"{fstr}✓" : $"{fstr}-"; }

        public static void Log(string msg) { TShock.Log.ConsoleInfo("[pm]" + msg); }
    }

    public class UserAccountLite
    {
        // TShock.UserAccounts.GetUserAccountsByName(name);
        public int ID = -1;
        public string Name = "";

        // QueryResult result)
		// {
		// 	account.ID = result.Get<int>("ID");
		// 	account.Group = result.Get<string>("Usergroup");
		// 	account.Password = result.Get<string>("Password");
		// 	account.UUID = result.Get<string>("UUID");
		// 	account.Name = result.Get<string>("Username");
		// 	account.Registered = result.Get<string>("Registered");
		// 	account.LastAccessed = result.Get<string>("LastAccessed");
		// 	account.KnownIps = result.Get<string>("KnownIps");
		// 	return account;
		// }
    }

    public class FoundPlayer
    {   
        // id从1开始，-1表示非ssc玩家
        // 此id，仅对db数据才有效
        public int ID = -1;
        public string Name = "";

        public bool online = false;

        public TSPlayer plr = null;

        public bool valid = false;

        public void SetOnline(TSPlayer p)
        {
            valid = true;
            online = true;
            
            plr = p;
            ID = p.Index;
            Name = p.Name;
        }
        public void SetOffline(int id, string name)
        {
            valid = true;
            online = false;

            ID = id;
            Name = name;

        }
    }
}