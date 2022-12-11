using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using TShockAPI;
using TShockAPI.DB;

namespace PlayerManager
{
    /// <summary>
    /// 工具类
    /// </summary>
    public class Utils
    {
        /// <summary>
        /// 保存目录
        /// </summary>
        public static string SaveDir;

        /// <summary>
        /// 自动备份目录
        /// </summary>
        public static string BackupsDir;

        /// <summary>
        /// 配置文件
        /// </summary>
        public static Config Config;

        /// <summary>
        /// 创建保存目录
        /// </summary>
        public static void CreateSaveDir() { if (!Directory.Exists(SaveDir)) Directory.CreateDirectory(SaveDir); }

        /// <summary>
        /// 创建自动备份保存目录
        /// </summary>
        public static void CreateBackupsDir() { if (!Directory.Exists(BackupsDir)) Directory.CreateDirectory(BackupsDir); }

        /// <summary>
        /// 用当前的时间创建保存目录
        /// </summary>
        /// <returns></returns>
        public static string CreateTimeDir()
        {
            string dir = Path.Combine(SaveDir, GetTimeStr());
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            return dir;
        }

        /// <summary>
        /// 获得时间字符串（当前时间）
        /// </summary>
        public static string GetTimeStr() { return GetTimeStr(DateTime.Now); }

        /// <summary>
        /// 获得时间字符串
        /// </summary>
        public static string GetTimeStr(DateTime dt) { return string.Format("{0:yyyyMMdd-HHmm-ss}", dt); }

        /// <summary>
        /// SSC是否使用.sqlite文件
        /// </summary>
        public static bool IsSqlite()
        {
            return TShock.Config.Settings.StorageType.ToLower() == "sqlite";
        }

        /// <summary>
        /// 备份文件
        /// </summary>
        public static void BackFile(string file)
        {
            if (File.Exists(file))
            {
                var info = new FileInfo(file);
                var dir = info.DirectoryName;
                var name = info.Name.Replace(info.Extension, "");
                var ext = info.Extension;
                var timeStr = GetTimeStr(info.CreationTime);
                string bakFile = Path.Combine(dir, $"{name}_{timeStr}{ext}");
                File.Copy(file, bakFile, true);
            }
        }

        /// <summary>
        /// 查找玩家
        /// </summary>
        public static FoundPlayer GetPlayer(string name, out string errMsg)
        {
            errMsg = "";
            var found = new FoundPlayer();

            List<TSPlayer> players = TShock.Players.Where(p => p != null && p.Active && p.Name == name).ToList();
            if (players.Count > 0)
            {
                // 在线玩家
                if (players.Count == 1)
                    found.SetOnline(players[0]);
                else
                    errMsg = $"找到多个匹配项-无法判断哪个是正确的：\n{string.Join(", ", players.Select(p => p.Name))}";
            }
            else
            {
                // 离线玩家
                var offline = GetAccount(name);
                if (offline.Count == 0)
                    errMsg = $"找不到名为 {name} 的玩家!";
                else if (offline.Count == 1)
                    found.SetOffline(offline.First().Key, offline.First().Value);
                else
                    errMsg = $"找到多个匹配项-无法判断哪个是正确的：\n{string.Join(", ", offline.Values)}";
            }
            return found;
        }

        /// <summary>
        /// 获取用户（ssc）
        /// </summary>
        public static Dictionary<int, string> GetAccount(string name)
        {
            return GetAccount(TShock.CharacterDB.database, name);
        }

        /// <summary>
        /// 获取用户（指定数据库）
        /// </summary>
        public static Dictionary<int, string> GetAccount(IDbConnection db, string name)
        {
            var dict = new Dictionary<int, string>();
            try
            {
                using var reader = db.QueryReader("SELECT * FROM Users WHERE Username = @0", name);
                while (reader.Read())
                {
                    dict.Add(reader.Get<int>("ID"), reader.Get<string>("Username"));
                }
            }
            catch (Exception ex)
            {
                TShock.Log.Error(ex.ToString());
            }
            return dict;
        }

        /// <summary>
        /// 获取用户id
        /// </summary>
        public static int GetAccountID(IDbConnection db, string name)
        {
            try
            {
                using var reader = db.QueryReader("SELECT * FROM Users WHERE Username = @0", name);
                if (reader.Read())
                {
                    return reader.Get<int>("ID");
                }
            }
            catch (Exception ex)
            {
                TShock.Log.Error(ex.ToString());
            }
            return -1;
        }

        /// <summary>
        /// 获取用户列表（指定数据库）
        /// </summary>
        public static Dictionary<int, CharacterData> GetAccountList(IDbConnection db)
        {
            // 列出id和玩家名
            var dict = new Dictionary<int, CharacterData>();
            try
            {
                using var reader = db.QueryReader("SELECT * FROM Users");
                while (reader.Read())
                {
                    var data = new CharacterData
                    {
                        id = reader.Get<int>("ID"),
                        name = reader.Get<string>("Username")
                    };
                    dict.Add(data.id, data);
                }
            }
            catch (Exception ex)
            {
                TShock.Log.Error(ex.ToString());
            }

            // 补齐生命和魔力
            try
            {
                using var reader = db.QueryReader("SELECT * FROM tsCharacter");
                while (reader.Read())
                {
                    var id = reader.Get<int>("Account");
                    if (dict.ContainsKey(id))
                    {
                        dict[id].health = reader.Get<int>("Health");
                        dict[id].maxHealth = reader.Get<int>("MaxHealth");
                        dict[id].mana = reader.Get<int>("Mana");
                        dict[id].maxMana = reader.Get<int>("MaxMana");
                    }
                }
            }
            catch (Exception ex)
            {
                TShock.Log.Error(ex.ToString());
            }

            return dict;
        }

        /// <summary>
        /// 将字符串换行
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="column">列数，1行显示多个</param>
        /// <returns></returns>
        public static List<string> WarpLines(List<string> lines, int column = 10)
        {
            List<string> li1 = new();
            List<string> li2 = new();
            foreach (var line in lines)
            {
                if (li2.Count % column == 0)
                {
                    if (li2.Count > 0)
                    {
                        li1.Add(string.Join(" ", li2));
                        li2.Clear();
                    }
                }
                li2.Add(line);
            }
            if (li2.Any())
            {
                li1.Add(string.Join(" ", li2));
            }
            return li1;
        }


        /// <summary>
        /// 高亮显示文本
        /// </summary>
        public static string Highlight(object msg) { return $"[c/96FF0A:{msg}]"; }

        /// <summary>
        /// 打勾显示
        /// </summary>
        public static string CFlag(bool foo, string fstr) { return foo ? $"[c/96FF96:✔]{fstr}" : $"-{fstr}"; }
        public static string CFlag(string fstr, bool foo) { return foo ? $"{fstr}✓" : $"{fstr}-"; }

        /// <summary>
        /// 输出日志
        /// </summary>
        public static void Log(string msg) { TShock.Log.ConsoleInfo($"[pm]{msg}"); }
        public static void Log(object obj) { TShock.Log.ConsoleInfo($"[pm]{obj}"); }
    }

    //public class UserAccountLite
    //{
    //    // TShock.UserAccounts.GetUserAccountsByName(name);
    //    public int ID = -1;
    //    public string Name = "";

    //    // QueryResult result)
    //    // {
    //    // 	account.ID = result.Get<int>("ID");
    //    // 	account.Group = result.Get<string>("Usergroup");
    //    // 	account.Password = result.Get<string>("Password");
    //    // 	account.UUID = result.Get<string>("UUID");
    //    // 	account.Name = result.Get<string>("Username");
    //    // 	account.Registered = result.Get<string>("Registered");
    //    // 	account.LastAccessed = result.Get<string>("LastAccessed");
    //    // 	account.KnownIps = result.Get<string>("KnownIps");
    //    // 	return account;
    //    // }
    //}

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