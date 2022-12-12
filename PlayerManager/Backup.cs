using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Terraria;
using Terraria.IO;
using TerrariaApi.Server;
using TShockAPI;

namespace PlayerManager
{
    /// <summary>
    /// 备份 和 恢复
    /// </summary>
    public class Backup
    {
        #region 自动备份
        public static TerrariaPlugin RegObj;
        static bool hasUpdate = false;

        /// <summary>
        /// 销毁
        /// </summary>
        public static void Dispose()
        {
            RemoveHook();
            RegObj = null;
        }

        /// <summary>
        /// 自动备份
        /// </summary>
        public static void ReloadJson()
        {
            Utils.Config = Config.Load(Path.Combine(Utils.SaveDir, "config.json"));
            ReflushHook();
        }

        /// <summary>
        /// 保存配置文件
        /// </summary>
        public static void SaveJson()
        {
            var f = Path.Combine(Utils.SaveDir, "config.json");
            File.WriteAllText(f, JsonConvert.SerializeObject(Utils.Config, Formatting.Indented));
        }

        /// <summary>
        /// 刷新钩子的情况
        /// </summary>
        static void ReflushHook()
        {
            if (Utils.Config.Interval == 0)
                RemoveHook();
            else if (Utils.Config.Interval > 0)
                AddHook();
        }
        /// <summary>
        /// 添加钩子
        /// </summary>
        static void AddHook()
        {
            if (!hasUpdate && Utils.IsSqlite())
            {
                hasUpdate = true;
                ServerApi.Hooks.GameUpdate.Register(RegObj, OnUpdate);
            }
        }
        /// <summary>
        /// 移除钩子
        /// </summary>
        static void RemoveHook()
        {
            if (hasUpdate)
            {
                hasUpdate = false;
                ServerApi.Hooks.GameUpdate.Deregister(RegObj, OnUpdate);
            }
        }

        /// <summary>
        /// 游戏嘀嗒
        /// </summary>
        static void OnUpdate(EventArgs args)
        {
            DoAutoBackup();
        }
        #endregion

        #region 自动备份第二部分
        private static DateTime lastbackup = DateTime.Now;
        /// <summary>
        /// 自动备份
        /// </summary>
        public static async void DoAutoBackup()
        {
            var Interval = Utils.Config.Interval;
            if (Interval > 0 && ((DateTime.Now - lastbackup).TotalMinutes >= Interval))
            {
                lastbackup = DateTime.Now;
                Save();
                await DoAutoBack();
                await DeleteOld();
            }
        }

        /// <summary>
        /// 执行自动备份
        /// </summary>
        static async Task DoAutoBack()
        {
            await Task.Run(() =>
            {
                Utils.CreateBackupsDir();
                string file = $"{Utils.GetTimeStr()}.sqlite";
                string sqlFile = Path.Combine(Utils.BackupsDir, file);
                File.Copy("tshock/tshock.sqlite", sqlFile, true);
                if (Utils.Config.ShowSaveMessages)
                    Utils.Log($"SSC已备份");
            });
        }

        /// <summary>
        /// 删除过时的自动备份的文件
        /// </summary>
        static async Task DeleteOld()
        {
            await Task.Run(() =>
            {
                if (Utils.Config.KeepFor <= 0)
                    return;
                foreach (var fi in new DirectoryInfo(Utils.BackupsDir).GetFiles("*.sqlite"))
                {
                    if ((DateTime.Now - fi.CreationTime).TotalMinutes > Utils.Config.KeepFor)
                    {
                        fi.Delete();
                    }
                }
            });
        }

        /// <summary>
        /// 获得符合要求的文件
        /// </summary>
        /// <param name="isDescend">是否进行倒序排序</param>
        /// <returns></returns>
        public static Dictionary<string, FileInfo> GetAutoBackupFiles(bool isDescend = false)
        {
            Dictionary<string, FileInfo> dict = new();
            foreach (var f in new DirectoryInfo(Utils.BackupsDir).GetFiles("*.sqlite"))
            {
                string k = f.Name.Replace(".sqlite", "");
                if (dict.ContainsKey(k)) continue;
                dict.Add(k, f);
            }

            // 排序
            if (!isDescend)
                dict = dict.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
            else
                dict = dict.OrderByDescending(x => x.Key).ToDictionary(x => x.Key, x => x.Value);

            return dict;
        }

        /// <summary>
        /// 获得plr文件
        /// </summary>
        /// <param name="isDescend">是否进行倒序排序</param>
        public static Dictionary<string, FileInfo> GetPlrFiles(bool isDescend = false)
        {
            Dictionary<string, FileInfo> dict = new();
            foreach (var f in new DirectoryInfo(Utils.SaveDir).GetFiles("*.plr"))
            {
                if (dict.ContainsKey(f.Name)) continue;
                dict.Add(f.Name, f);
            }

            // 排序
            if (!isDescend)
                dict = dict.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
            else
                dict = dict.OrderByDescending(x => x.Key).ToDictionary(x => x.Key, x => x.Value);

            return dict;
        }
        #endregion

        #region 手动备份
        public async static void BackCommand(CommandArgs args)
        {
            string note = "";
            var _config = Utils.Config;
            args.Parameters.RemoveAt(0);
            if (args.Parameters.Count > 0)
            {
                switch (args.Parameters[0].ToLowerInvariant())
                {
                    case "help":
                    case "h":
                        args.Player.SendInfoMessage($"{Utils.Highlight("/pm b")}ackup 指令用法:" +
                            $"\n/pm b [备注], 创建备份点（手动备份）" +
                            $"\n/pm b status, 查看自动备份状态" +
                            $"\n/pm b <on/off>, 开启/关闭 自动备份" +
                            $"\n/pm b interval <分钟数>, 设置自动备份间隔" +
                            $"\n/pm b keepfor <分钟数>, 设置备份文件的期限" +
                            $"\n/pm b keep <日期.sqlite>, 保留一个自动备份的文件");
                        return;

                    // 状态
                    case "status":
                    case "s":
                        args.Player.SendInfoMessage($"自动备份：{(_config.Interval == 0 ? "已关闭" : "已开启")}" +
                            $"\n备份频率：每{Utils.Highlight(_config.Interval)}分钟备份一次" +
                            $"\n文件期限：仅{Utils.Highlight(_config.KeepFor)}分钟内" +
                            $"\n备份路径：{Utils.BackupsDir}" +
                            $"\n{(Utils.IsSqlite() ? "" : "未使用sqlite作为ssc数据库，将无法使用备份功能！")}");
                        return;

                    // 开启
                    case "on":
                        if (_config.Interval > 0)
                        {
                            args.Player.SendInfoMessage($"自动备份已经开启过了，每{_config.Interval}分钟备份一次");
                        }
                        else
                        {
                            _config.Interval = 2;
                            SaveJson();
                            ReflushHook();
                            args.Player.SendInfoMessage("自动备份已开启，每2分钟备份一次");
                        }
                        return;

                    // 关闭
                    case "off":
                        if (_config.Interval > 0)
                        {
                            _config.Interval = 0;
                            SaveJson();
                            ReflushHook();
                            args.Player.SendInfoMessage($"自动备份已关闭");
                        }
                        else
                        {
                            args.Player.SendInfoMessage("自动备份已经是关闭状态");
                        }
                        return;

                    // 设置备份间隔
                    case "interval":
                        if (args.Parameters.Count == 1)
                        {
                            args.Player.SendInfoMessage("请输入备份频率");
                            return;
                        }
                        if (!int.TryParse(args.Parameters[1], out int interval))
                        {
                            args.Player.SendInfoMessage("请输入有效的数值");
                            return;
                        }
                        _config.Interval = interval;
                        SaveJson();
                        ReflushHook();
                        args.Player.SendInfoMessage($"已设置成每{interval}分钟备份一次");
                        return;

                    // 设置备份间隔
                    case "keepfor":
                        if (args.Parameters.Count == 1)
                        {
                            args.Player.SendInfoMessage("请输入备份期限！");
                            return;
                        }
                        if (!int.TryParse(args.Parameters[1], out int keepfor))
                        {
                            args.Player.SendInfoMessage("请输入有效的数值");
                            return;
                        }
                        _config.KeepFor = keepfor;
                        SaveJson();
                        ReflushHook();
                        args.Player.SendInfoMessage($"已设置成保留最近{keepfor}分钟的文件");
                        return;

                    // 保留自动备份的文件
                    case "keep":
                        if (args.Parameters.Count == 1)
                        {
                            args.Player.SendInfoMessage("请输入要保留的文件名");
                            return;
                        }
                        string fileName = args.Parameters[1];
                        if (!File.Exists(Path.Combine(Utils.BackupsDir, fileName)))
                        {
                            args.Player.SendInfoMessage($"{Utils.BackupsDir} 目录下找不到 {fileName}");
                            return;
                        }
                        await KeepABFile(args.Player, fileName);
                        return;

                    default: note = args.Parameters[0]; break;
                }
            }

            if (!Utils.IsSqlite())
            {
                args.Player.SendErrorMessage("SSC未保存在tshock.sqlite，无法备份！");
                return;
            }

            // 保存并备份
            await DoManualBack(args.Player, note);
        }

        /// <summary>
        /// 保留自动备份的文件
        /// </summary>
        static async Task KeepABFile(TSPlayer op, string rawFileName)
        {
            await Task.Run(() =>
            {
                Dictionary<int, FileInfo> dict = GetManualBackupFiles();
                int num = dict.Any() ? dict.Last().Key + 1 : 1;
                string file = $"{num}_{rawFileName}";
                string rawFile = Path.Combine(Utils.BackupsDir, rawFileName);
                string sqlFile = Path.Combine(Utils.SaveDir, $"{num}_{rawFileName}");
                File.Copy("tshock/tshock.sqlite", sqlFile, true);
                op.SendSuccessMessage($"已将 {rawFileName} 转成 第{num}个备份点！({file})");
            });
        }

        /// <summary>
        /// 执行手动备份
        /// </summary>
        static async Task DoManualBack(TSPlayer op, string note)
        {
            await Task.Run(() =>
            {
                Save();
                Utils.CreateSaveDir();
                Dictionary<int, FileInfo> dict = GetManualBackupFiles();
                int num = dict.Any() ? dict.Last().Key + 1 : 1;
                if (string.IsNullOrEmpty(note))
                {
                    note = Utils.GetTimeStr();
                }
                string file = $"{num}_{note}.sqlite";
                string sqlFile = Path.Combine(Utils.SaveDir, file);
                File.Copy("tshock/tshock.sqlite", sqlFile, true);
                op.SendSuccessMessage($"第{num}个备份点已创建！({file})");
            });
        }


        /// <summary>
        /// 获得符合要求的文件
        /// </summary>
        /// <param name="isDescend">是否进行倒序排序</param>
        /// <returns></returns>
        public static Dictionary<int, FileInfo> GetManualBackupFiles(bool isDescend = false)
        {
            Dictionary<int, FileInfo> dict = new();
            foreach (var f in new DirectoryInfo(Utils.SaveDir).GetFiles("*.sqlite"))
            {
                if (int.TryParse(f.Name.Split("_")[0], out int num))
                {
                    if (dict.ContainsKey(num)) continue;
                    dict.Add(num, f);
                }
            }
            // 排序
            if (!isDescend)
                dict = dict.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
            else
                dict = dict.OrderByDescending(x => x.Key).ToDictionary(x => x.Key, x => x.Value);

            // 删除不连续的
            //num = 1;
            //List<int> keys = new();
            //foreach (int k in dict.Keys)
            //{
            //    if (k == num)
            //        num++;
            //    else
            //        keys.Add(k);
            //}
            //foreach (int k in keys)
            //{
            //    dict.Remove(k);
            //}

            return dict;
        }
        #endregion




        #region 恢复
        public async static void RecoverCommand(CommandArgs args)
        {
            void Help()
            {
                args.Player.SendInfoMessage($"{Utils.Highlight("/pm r")}ecover 指令用法" +
                    $"\n/pm r <玩家名> 0, 恢复至初始状态" +
                    "\n/pm r <玩家名> <备份点>, 恢复至备份点（手动备份）" +
                    "\n/pm r <玩家名> <日期.sqlite>, 恢复至数据库文件（自动备份）" +
                    "\n/pm r <玩家名> <xx.plr>, 恢复至plr文件" +
                    "\n/pm r <玩家名> <目标> [仅背包/仅皮肤/仅buff/仅属性], 按选项进行恢复，选项可以用1/2/3/4代替" +
                    $"\n输入{Utils.Highlight("/pm list")}查看可用的恢复目标");
            }

            // 帮助
            args.Parameters.RemoveAt(0);
            if (args.Parameters.Count < 2)
            {
                Help();
                return;
            }
            switch (args.Parameters[0].ToLowerInvariant())
            {
                case "help": case "h": Help(); return;
            }

            // 恢复选项
            int opt = 0;
            if (args.Parameters.Count > 2)
            {
                switch (args.Parameters[2].ToLowerInvariant())
                {
                    default: args.Player.SendErrorMessage("恢复选项输入错误！"); return;

                    case "1": case "仅背包": opt = 1; break;
                    case "2": case "仅皮肤": opt = 2; break;
                    case "3": case "仅buff": opt = 3; break;
                    case "4": case "仅属性": opt = 4; break;
                }
            }

            // 恢复
            string userName = args.Parameters[0];
            string param = args.Parameters[1];
            await DoRecover(args.Player, userName, param, opt);
        }

        /// <summary>
        /// 恢复备份
        /// </summary>
        static async Task DoRecover(TSPlayer op, string userName, string param, int opt)
        {
            await Task.Run(() =>
            {
                // 玩家不存在
                FoundPlayer found = Utils.GetPlayer(userName, out string errMsg);
                if (!found.valid)
                {
                    op.SendErrorMessage(errMsg);
                    return;
                }

                if (!found.online && opt == 3)
                {
                    op.SendErrorMessage($"{userName}已离线，无法对离线玩家进行buff恢复");
                    return;
                }

                CharacterData data = new()
                {
                    RecoverType = opt
                };

                string optDesc = data.GetRecoverTypeDesc();
                if (!string.IsNullOrEmpty(optDesc))
                    optDesc = $"({optDesc})";

                // 恢复至初始SSC状态 ==========
                if (param == "0")
                {
                    data.SeedInitialData(userName);
                    if (found.online)
                    {
                        data.AddDataWhenJoined(found.plr.DataWhenJoined);
                        data.ToPlayer(found.plr);
                        op.SendSuccessMessage($"已将在线玩家 {userName}，恢复至初始状态{optDesc}。");
                    }
                    else
                    {
                        data.ToDatabase(userName);
                        op.SendSuccessMessage($"已将离线玩家 {userName}，恢复至初始状态{optDesc}。");
                    }
                    return;
                }

                // 恢复至plr ==========
                if (param.Contains(".plr"))
                {
                    string plrFile = Path.Combine(Utils.SaveDir, param);
                    if (!File.Exists(plrFile))
                    {
                        op.SendErrorMessage($"{plrFile} 文件不存在！");
                        return;
                    }

                    PlayerFileData playerFileData = Player.LoadPlayer(plrFile, false);
                    data.CopyFormPlayer(playerFileData.Player);
                    if (found.online)
                    {
                        data.ToPlayer(found.plr);
                        op.SendSuccessMessage($"已将在线玩家 {userName}，恢复至{param}{optDesc}。");
                    }
                    else
                    {
                        data.ToDatabase(userName);
                        op.SendSuccessMessage($"已将离线玩家 {userName}，恢复至{param}{optDesc}。");
                    }
                    return;
                }


                // 恢复至手动备份/自动备份
                string backupFile;
                string sqlFile;
                if (param.Contains(".sqlite"))
                {
                    // 恢复至自动备份文件
                    backupFile = param;
                    sqlFile = Path.Combine(Utils.BackupsDir, param);
                    if (!File.Exists(sqlFile))
                    {
                        op.SendErrorMessage($"{Utils.BackupsDir} 目录下找不到 {param} 文件");
                        return;
                    }
                }
                else
                {
                    // 恢复至备份点 ==========
                    if (!int.TryParse(param, out int backupNum) && backupNum == 0)
                    {
                        op.SendErrorMessage("备份点输入错误！（注意备份点是数字）");
                        return;
                    }

                    // 无备份点
                    Dictionary<int, FileInfo> dict = GetManualBackupFiles();
                    if (dict.Count == 0)
                    {
                        op.SendErrorMessage($"本地没有备份点，请检查 {Utils.SaveDir} 目录下是否有 {Utils.Highlight("序号_备注.sqlite")} 格式的文件！");
                        return;
                    }
                    if (!dict.ContainsKey(backupNum))
                    {
                        op.SendErrorMessage($"找不到备份点，输入{Utils.Highlight("/pm backup list")}查看可用的备份点！");
                        return;
                    }
                    backupFile = dict[backupNum].Name;
                    sqlFile = Path.Combine(Utils.SaveDir, backupFile);
                }

                using IDbConnection backupDB = new SqliteConnection(string.Format("Data Source={0}", sqlFile));

                // 找不到玩家
                int backupAccountID = Utils.GetAccountID(backupDB, userName);
                if (backupAccountID == -1)
                {
                    op.SendErrorMessage($"{backupFile} 里没有名为 {userName} 的玩家！");
                    return;
                }

                // 恢复数据
                data.Copy(backupDB, userName);
                if (found.online)
                {
                    data.ToPlayer(found.plr);
                    op.SendSuccessMessage($"已将在线玩家 {userName}，恢复至{backupFile}{optDesc}。");
                }
                else
                {
                    data.ToDatabase(userName);
                    op.SendSuccessMessage($"已将离线玩家 {userName}，恢复至{backupFile}{optDesc}。");
                }
                backupDB.Close();
                backupDB.Dispose();
            });
        }
        #endregion

        #region 用户列表
        /// <summary>
        /// 显示用户列表
        /// </summary>
        public static void ListCommand(CommandArgs args)
        {
            void Help()
            {
                args.Player.SendInfoMessage($"{Utils.Highlight("/pm list")} 指令用法:" +
                    $"\n/pm list 0, 查看玩家列表（当前服务器）" +
                    $"\n/pm list <备份点>, 查看备份点里的玩家列表（手动备份）" +
                    $"\n/pm list <日期.sqlite>, 查看数据库文件里的玩家列表（自动备份）" +
                    $"\n/pm list backup, 列出备份点（手动备份）" +
                    $"\n/pm list {Utils.Highlight("a")}uto{Utils.Highlight("b")}ackup, 列出数据库文件（自动备份）" +
                    $"\n/pm list plr, 列出plr文件");
            }

            // 帮助
            args.Parameters.RemoveAt(0);
            if (args.Parameters.Count == 0)
            {
                Help();
                return;
            }

            string DataToString(CharacterData ch)
            {
                return $"ID:{ch.id}, 玩家:{ch.name}, 生命:{ch.health}/{ch.maxHealth}, 魔力:{ch.mana}/{ch.maxMana}";
            }
            string titleStr = "";
            List<string> lines = new();

            int backID = -2;
            string prefix = "";
            string sqlFile = "";
            string cmdStr = "/pm list " + string.Join(" ", args.Parameters[0]);

            switch (args.Parameters[0].ToLowerInvariant())
            {
                case "help": case "h": Help(); return;

                // 列出手动备份的文件
                case "backup":
                case "back":
                case "b":
                    titleStr = "可用的备份点";
                    foreach (var obj in GetManualBackupFiles(true))
                    {
                        lines.Add($"备份点:{obj.Key}, 文件名:{obj.Value.Name}");
                    }
                    lines.Add($"提示：要恢复至备份点1，只需输入{Utils.Highlight("/pm r 玩家名 1")}");
                    break;

                // 列出手动备份的文件
                case "autobackup":
                case "ab":
                    titleStr = "自动备份的文件";
                    foreach (var obj in GetAutoBackupFiles(true))
                    {
                        lines.Add($"{obj.Value.Name}");
                    }
                    break;

                // 列出plr文件
                case "plr":
                case "p":
                    titleStr = "可用的plr文件";
                    foreach (var obj in GetPlrFiles())
                    {
                        lines.Add($"{obj.Value.Name}");
                    }
                    break;


                // 查看 手动备份/自动备份 文件里的用户
                default:
                    if (int.TryParse(args.Parameters[0], out backID))
                    {
                        if (backID > 0)
                        {

                            Dictionary<int, FileInfo> fDict = GetManualBackupFiles();
                            if (!fDict.ContainsKey(backID))
                            {
                                args.Player.SendErrorMessage($"找不到备份点{backID}，输入{Utils.Highlight("/pm backup list")}查看可用的备份点！");
                                return;
                            }

                            sqlFile = Path.Combine(Utils.SaveDir, fDict[backID].Name);
                            prefix = $"{Utils.Highlight(fDict[backID].Name)}";
                            break;
                        }
                        else if (backID == 0)
                        {
                            prefix = "服务器";
                            break;
                        }
                        else
                        {
                            args.Player.SendErrorMessage("输入的备份点无效！");
                            return;
                        }
                    }


                    if (args.Parameters[0].Contains(".sqlite"))
                    {
                        // 自动备份
                        sqlFile = Path.Combine(Utils.BackupsDir, args.Parameters[0]);
                        if (!File.Exists(sqlFile))
                        {
                            args.Player.SendErrorMessage($"{Utils.BackupsDir} 目录下找不到 {args.Parameters[0]} 数据库文件");
                            return;
                        }

                        backID = -1;
                        prefix = $"{Utils.Highlight(args.Parameters[0])}";
                    }
                    break;
            }

            // 列出数据库玩家
            if (backID > -2)
            {
                Dictionary<int, CharacterData> dict = new();
                if (backID == 0)
                {
                    // 当前ssc
                    IDbConnection db1 = TShock.CharacterDB.database;
                    dict = Utils.GetAccountList(db1);
                }
                else
                {
                    // 备份点
                    using IDbConnection db2 = new SqliteConnection(string.Format("Data Source={0}", sqlFile));
                    dict = Utils.GetAccountList(db2);
                }

                if (!dict.Any())
                {
                    args.Player.SendErrorMessage($"{prefix}没有玩家数据！");
                    return;
                }

                foreach (var f in dict)
                {
                    lines.Add($"{DataToString(f.Value)}");
                }

                titleStr = $"{prefix}里的玩家列表";
            }


            var fStr = $"{cmdStr}" + " {{0}}";
            if (!PaginationTools.TryParsePageNumber(args.Parameters, 1, args.Player, out int pageNumber)) return;
            PaginationTools.SendPage(args.Player, pageNumber, lines, new PaginationTools.Settings
            {
                HeaderFormat = $"{titleStr}" + " ({0}/{1}):",
                FooterFormat = $"输入{Utils.Highlight(fStr)}查看更多".SFormat(Commands.Specifier)
            });
        }
        #endregion

        /// <summary>
        /// 保存ssc
        /// </summary>
        public static void Save()
        {
            foreach (var p in TShock.Players.Where(p => p != null && p.Active))
            {
                p.SaveServerCharacter();
            }
        }


    }
}
