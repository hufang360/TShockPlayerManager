using Rests;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;


namespace PlayerManager
{
    [ApiVersion(2, 1)]
    public class Plugin : TerrariaPlugin
    {
        public override string Description => "玩家管理";
        public override string Name => "PlayerManager";
        public override string Author => "hufang360";
        public override Version Version => Assembly.GetExecutingAssembly().GetName().Version;

        readonly string PermissionManager = "playermanager";
        readonly string PermissionLookbag = "lookbag";

        public Plugin(Main game) : base(game)
        {
        }

        /// <summary>
        /////// 初始化
        /// </summary>
        public override void Initialize()
        {
            foreach (Command c in Commands.ChatCommands)
            {
                if (c.Names.Contains("pm"))
                    c.Names.Remove("pm");
            }

            Commands.ChatCommands.Add(new Command(PermissionManager, PMCommand, "playermanager", "pm", "ppm") { HelpText = "玩家管理" });
            Commands.ChatCommands.Add(new Command(PermissionLookbag, Look.LookBag, "lookbag", "lb") { HelpText = "查看背包" });
            //Commands.ChatCommands.Add(new Command("deathrank", Rank.Manage, "deathrank", "dr") { HelpText = "死亡榜" });

            // RestApi
            TShock.RestApi.Register(new SecureRestCommand("/pm/look", Rest.Look, PermissionManager));
            TShock.RestApi.Register(new SecureRestCommand("/pm/export", Rest.Export, PermissionManager));

            // 工作目录
            Utils.SaveDir = Path.Combine(TShock.SavePath, "PlayerManager");
            Utils.BackupsDir = Path.Combine(TShock.SavePath, "PlayerManager", "backups");
            Utils.CreateSaveDir();

            Backup.RegObj = this;
            Backup.ReloadJson();
        }


        /// <summary>
        /// 释放
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Backup.Dispose();
            }
            base.Dispose(disposing);
        }



        #region PMCommand
        private async void PMCommand(CommandArgs args)
        {
            #region 帮助
            void Help()
            {
                List<string> lines = new()
                {
                    "/lookbag <玩家名>, 查看玩家（给普通玩家的）",
                    "/pm look <玩家名>, 查看玩家",
                    "/pm export <玩家名>, 导出指定玩家",
                    "/pm exportall, 导出全部玩家",

                    "/pm hp <玩家名> <生命值>, 修改生命值",
                    "/pm maxhp <玩家名> <生命上限>, 修改生命上限",
                    "/pm mana <玩家名> <魔力上限>, 修改魔力值",
                    "/pm maxmana <玩家名> <魔力上限>, 修改魔力上限",

                    "/pm backup help, 备份",
                    "/pm recover help, 恢复",
                    "/pm list help, 列表",
                    "/pm reload, 重载配置",

                    "/pm enhance help, 永久增强",
                    "/pm quest <玩家名> <次数>, 修改渔夫任务完成次数",
                };

                if (!PaginationTools.TryParsePageNumber(args.Parameters, 1, args.Player, out int pageNumber)) return;
                PaginationTools.SendPage(args.Player, pageNumber, lines, new PaginationTools.Settings
                {
                    HeaderFormat = $"{Utils.Highlight("/p")}layer{Utils.Highlight("m")}" + "anager 指令用法 ({0}/{1}):",
                    FooterFormat = $"输入{Utils.Highlight("/pm help {{0}}")}查看更多".SFormat(Commands.Specifier)
                });
            }
            #endregion

            if (args.Parameters.Count == 0)
            {
                Help();
                return;
            }

            switch (args.Parameters[0].ToLowerInvariant())
            {
                // 帮助
                default: case "help": case "h": Help(); return;

                // 导出指定玩家
                case "export":
                case "e":
                    args.Parameters.RemoveAt(0);
                    if (args.Parameters.Count == 0)
                    {
                        args.Player.SendInfoMessage("请输入玩家名！");
                        return;
                    }
                    var result = await ExportPlayer.AsyncExport(string.Join("", args.Parameters));
                    if (!string.IsNullOrEmpty(result.Item1)) args.Player.SendSuccessMessage(result.Item1);
                    if (!string.IsNullOrEmpty(result.Item2)) args.Player.SendErrorMessage(result.Item2);
                    break;

                // 导出全部玩家
                case "exportall":
                case "ea":
                    await ExportPlayer.AsyncExportAll(args.Player);
                    break;

                // 查看玩家背包
                case "look":
                case "l":
                    args.Parameters.RemoveAt(0);
                    var flag = args.Player.RealPlayer;
                    Look.LookPlayer(args, flag);
                    break;

                // 立即保存SSC
                case "save":
                case "s":
                    Backup.Save();
                    args.Player.SendSuccessMessage("SSC已保存！");
                    break;

                // 备份 / 恢复 / 列表
                case "backup": case "back": case "b": Backup.BackCommand(args); break;
                case "recover": case "r": Backup.RecoverCommand(args); break;
                case "list": case "ls": Backup.ListCommand(args); break;


                // 重载配置文件
                case "reload":
                    Backup.ReloadJson();
                    args.Player.SendSuccessMessage("已重新加载配置文件");
                    break;

                // 生命 / 魔力 / 增强
                case "hp": Modify.ModifyHP(args); break;
                case "mana": Modify.ModifyMana(args); break;
                case "maxhp": case "mh": Modify.ModifyMaxHP(args); break;
                case "maxmana": case "mm": Modify.ModifyMaxMana(args); break;
                case "enhance": case "en": Modify.ModifyEnhance(args); break;
                case "quest": case "q": Modify.ModifyQuest(args); break;
            }
        }
        #endregion

    }
}
