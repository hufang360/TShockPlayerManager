using System;
using System.Collections.Generic;
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
        public override string Description => "玩家管理";
        public override string Name => "PlayerManager";
        public override string Author => "hufang360";
        public override Version Version => Assembly.GetExecutingAssembly().GetName().Version;
        #endregion


        public Plugin(Main game) : base(game)
        {
        }


        #region Initialize/Dispose
        public override void Initialize()
        {
            Commands.ChatCommands.Add(new Command(new List<string>() { "playermanager" }, PlayerManager, "playermanager", "ppm") { HelpText = "玩家管理" });
            Commands.ChatCommands.Add(new Command(new List<string>() { "lookbag" }, LookBag, "lookbag", "lb") { HelpText = "查看背包" });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
        #endregion


        #region PlayerManager
        private void PlayerManager(CommandArgs args)
        {
            if (args.Parameters.Count<string>() == 0)
            {
                args.Player.SendErrorMessage("语法错误，/ppm help 可查询帮助信息");
                return;
            }

            switch (args.Parameters[0].ToLowerInvariant())
            {
                default:
                    args.Player.SendErrorMessage("语法错误！");
                    break;

                // 帮助
                case "help":
                case "h":
                    args.Player.SendInfoMessage("/ppm look <玩家名>, 查看玩家");
                    args.Player.SendInfoMessage("/lookbg <玩家名>, 查看背包（普通用户）");
                    args.Player.SendInfoMessage("/ppm maxhp <玩家名> <生命上限>, 修改生命上限");
                    args.Player.SendInfoMessage("/ppm maxmana <玩家名> <魔力上限>, 修改魔力上限");
                    args.Player.SendInfoMessage("/ppm hp <玩家名> <生命值>, 修改生命值");
                    args.Player.SendInfoMessage("/ppm mana <玩家名> <魔力上限>, 修改魔力值");
                    args.Player.SendInfoMessage("/ppm export [玩家名], 导出某个玩家存档");
                    args.Player.SendInfoMessage("/ppm exportall, 导出玩家存档");
                    return;

                // 查看玩家背包
                case "look":
                case "l":
                    string name = "";
                    args.Parameters.RemoveAt(0);
                    if( args.Parameters.Count==0 ){
                        if( !args.Player.RealPlayer ){
                            args.Player.SendErrorMessage("请输入玩家名，/ppm look <玩家名>");
                            return;
                        } else {
                            name = args.Player.Name;
                        }
                    } else {
                        name = string.Join("", args.Parameters);
                    }
                    Look.LookPlayer(args, name);
                    break;

                // 导出
                case "export":
                case "e":
                    args.Parameters.RemoveAt(0);
                    if (args.Parameters.Count == 0)
                    {
                        args.Player.SendInfoMessage("请输入玩家名！");
                        return;
                    }
                    #pragma warning disable 4014
                    ExportPlayer.Export(args.Player, string.Join("", args.Parameters));
                    #pragma warning restore 4014
                    break;
                
                // 导出全部
                case "exportall":
                case "ea":
                    #pragma warning disable 4014
                    ExportPlayer.ExportAll(args.Player);
                    #pragma warning restore 4014
                    break;


                //  生命
                case "hp":
                    Modify.ModifyHP(args);
                    break;
                case "maxhp":
                case "mh":
                    Modify.ModifyMaxHP(args);
                    break;

                //  魔力
                case "mana":
                    Modify.ModifyMana(args);
                    break;
                case "maxmana":
                case "mm":
                    Modify.ModifyMaxMana(args);
                    break;
            }
        }
        #endregion
        

        #region lookbag
        private void LookBag(CommandArgs args)
        {
            string name = "";
            if( args.Parameters.Count==0 ){
                if( !args.Player.RealPlayer ){
                    args.Player.SendErrorMessage("请输入玩家名，/lookbag <玩家名>");
                    return;
                } else {
                    name = args.Player.Name;
                }
            } else {
                name = string.Join("", args.Parameters);
            }
            Look.LookPlayer(args, name);
        }
        #endregion

        
    }
}
