using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace Plugin
{
    [ApiVersion(2, 1)]
    public class Plugin : TerrariaPlugin
    {

        #region Plugin Info
        public override string Author => "hufang360";
        public override string Description => "玩家管理";
        public override string Name => "PlayerManager";
        public override Version Version => Assembly.GetExecutingAssembly().GetName().Version;
        #endregion

        public static readonly string save_dir = Path.Combine(TShock.SavePath, "PlayerManager");


        public Plugin(Main game) : base(game)
        {
        }


        #region Initialize/Dispose
        public override void Initialize()
        {
            Commands.ChatCommands.Add(new Command(new List<string>() { "playermanager" }, PlayerManager, "playermanager", "pm") { HelpText = "玩家管理" });
            Commands.ChatCommands.Add(new Command(new List<string>() { "lookbag" }, LookBag, "lookbag", "lb") { HelpText = "查看玩家背包" });
            Commands.ChatCommands.Add(new Command(new List<string>() { "playersnapshot" }, Snapshot.PlayerSnapshot, "playersnapshot", "ps") { HelpText = "存档快照" });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
        #endregion


        private void PlayerManager(CommandArgs args)
        {
            if (args.Parameters.Count<string>() == 0)
            {
                args.Player.SendErrorMessage("语法错误，/pm help 可查询帮助信息");
                return;
            }

            switch (args.Parameters[0].ToLowerInvariant())
            {
                default:
                    args.Player.SendErrorMessage("语法错误！");
                    break;

                // 帮助
                case "help":
                    args.Player.SendInfoMessage("/pm look <玩家名>, 查看玩家");
                    args.Player.SendInfoMessage("/pm hp <玩家名> <生命值>, 修改生命值");
                    args.Player.SendInfoMessage("/pm maxhp <玩家名> <生命上限>, 修改生命上限");
                    args.Player.SendInfoMessage("/pm mana <玩家名> <魔力上限>, 修改魔力值");
                    args.Player.SendInfoMessage("/pm maxmana <玩家名> <魔力上限>, 修改魔力上限");
                    args.Player.SendInfoMessage("/pm export, 导出玩家存档");
                    args.Player.SendInfoMessage("/pm export [玩家名], 导出单个玩家存档");
                    args.Player.SendInfoMessage("/ps help, 存档快照功能");
                    return;

                // 查看玩家背包
                case "look":
                    var name = args.Parameters.Count<string>() > 1 ?  args.Parameters[1] : "";
                    Look.LookPlayer(args, name);
                    break;

                // 导出
                case "export":
                    if (!Directory.Exists(save_dir)){
                        Directory.CreateDirectory(save_dir);
                    }
                    #pragma warning disable 4014
                    if (args.Parameters.Count()>1){
                        ExportPlayer.Export(args);
                    } else {
                        ExportPlayer.ExportAll(args);
                    }
                    #pragma warning restore 4014
                    break;


                //  生命
                case "hp":
                    Modify.ModifyHP(args);
                    break;
                case "maxhp":
                    Modify.ModifyMaxHP(args);
                    break;

                //  魔力
                case "mana":
                    Modify.ModifyMana(args);
                    break;
                case "maxmana":
                    Modify.ModifyMaxMana(args);
                    break;
            }
        }

        private void LookBag(CommandArgs args)
        {
            var name = args.Parameters.Count<string>() > 0 ?  args.Parameters[0] : "";
            Look.LookPlayer(args, name);
        }

    }
}
