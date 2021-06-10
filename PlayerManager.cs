using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using Newtonsoft.Json;

namespace Plugin
{
    [ApiVersion(2, 1)]
    public class Plugin : TerrariaPlugin
    {
        public override string Author => "hufang360";

        public override string Description => "角色管理";

        public override string Name => "PlayerManager";

        public override Version Version => Assembly.GetExecutingAssembly().GetName().Version;

        public static readonly string save_dir = Path.Combine(TShock.SavePath, "PlayerManager");

        public static readonly string config_path = Path.Combine(save_dir, "config.json");

        private static Config _config;

        public Plugin(Main game) : base(game)
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                GetDataHandlers.PlayerSpawn -= OnRespawn;
                ServerApi.Hooks.GameUpdate.Deregister(this, OnUpdate);
            }
            base.Dispose(disposing);
        }

        public override void Initialize()
        {
            if (!Directory.Exists(save_dir))
                Directory.CreateDirectory(save_dir);
            LoadConfig();

            Commands.ChatCommands.Add(new Command(new List<string>() { "playermanage" }, PlayerManage, "playermanage", "pm") { HelpText = "角色管理" });
            GetDataHandlers.PlayerSpawn += OnRespawn;
            ServerApi.Hooks.GameUpdate.Register(this, OnUpdate);
        }

        private void PlayerManage(CommandArgs args)
        {
            if (args.Parameters.Count<string>() == 0)
            {
                args.Player.SendErrorMessage("语法错误，/pm help 可查询相关用法");
                return;
            }


            switch (args.Parameters[0].ToLowerInvariant())
            {
                default:
                    args.Player.SendErrorMessage("语法不正确！");
                    break;

                // 帮助
                case "help":
                    args.Player.SendInfoMessage("/pm export [玩家名], 导出所有玩家的在线存档，可指定只导出某一个玩家");
                    args.Player.SendInfoMessage("/pm look <玩家名>, 查看某一个玩家的背包信息");
                    args.Player.SendInfoMessage("/pm onehp, 开/关 1血锁定");
                    return;

                // 查看玩家背包
                case "look":
                    LookPlayer(args);
                    break;

                //  1血锁定
                case "onehp":
                    LockOneHP(args);
                    break;

                // 导出
                case "export":
                    ExportPlayer.Export(args);
                    break;
            }

        }

        private void  LoadConfig()
        {
            _config = Config.Load(config_path);
            update_total = 60* _config.LockHPSecond;
            File.WriteAllText(config_path, JsonConvert.SerializeObject(_config, Formatting.Indented));
        }

        private void LockOneHP(CommandArgs args)
        {
            _config.LockHPEnable = !_config.LockHPEnable;
            File.WriteAllText(config_path, JsonConvert.SerializeObject(_config, Formatting.Indented));
            if (_config.LockHPEnable){
                args.Player.SendInfoMessage("1血锁定 已开启");
            } else {
                args.Player.SendInfoMessage("1血锁定 已关闭");
            }
        }

        private int update_count = 0;
        // 每隔30秒重置所有玩家生命值
        private int update_total = 60*10;
        private void OnUpdate(EventArgs args)
        {
            if(!_config.LockHPEnable){
                return;
            }
            if(  update_count < update_total ){
                update_count ++;
                return;
            }else{
                update_count = 0;
            }

			foreach (TSPlayer plr in TShock.Players)
			{
				if (plr != null)
				{
                    if (plr.TPlayer.dead)
                    {
                        continue;
                    }

                    plr.TPlayer.statLife = _config.LockHP;
                    plr.TPlayer.statLifeMax = _config.LockHP;
                    // https://github.com/Brycey92/PlayerInfo/blob/terraria-1.4/PlayerInfo/PlayerInfo.cs#L117
                    // PlayerHp
                    NetMessage.SendData(16, -1, -1, null, plr.Index, 0f, 0f, 0f, 0); // Sends Health Packet
				}
			}
        }

        private void OnRespawn(object o, GetDataHandlers.SpawnEventArgs args)
		{
            if(!_config.LockHPEnable){
                return;
            }

            List<TSPlayer> players = TSPlayer.FindByNameOrID(Main.player[args.PlayerId].name);
            if (players.Count == 0)
			{
				args.Player.SendErrorMessage("无效的玩家!");
			}
			else if (players.Count > 1)
			{
				args.Player.SendMultipleMatchError(players.Select(p => p.Name));
			}
			else
			{
				var plr = players[0];
                plr.TPlayer.statLife = _config.LockHP;
                plr.TPlayer.statLifeMax = _config.LockHP;
                // https://github.com/Brycey92/PlayerInfo/blob/terraria-1.4/PlayerInfo/PlayerInfo.cs#L117
                // PlayerHp
                NetMessage.SendData(16, -1, -1, null, plr.Index, 0f, 0f, 0f, 0); // Sends Health Packet
            }

		}

        private void LookPlayer(CommandArgs args)
        {
            if (args.Parameters.Count<string>() == 1)
            {
                args.Player.SendErrorMessage("语法错误，需输入要查看的玩家名");
                return;
            }

            var name = args.Parameters[1];
            var list = TSPlayer.FindByNameOrID(name);
            if (list.Count > 1)
            {
                args.Player.SendMultipleMatchError(list);
            }
            else if (list.Any())
            {
                ShowPlayer(list[0].TPlayer, args);

            }
            else
            {
                var offlinelist = TShock.UserAccounts.GetUserAccountsByName(name);
                if (offlinelist.Count > 1)
                {
                    args.Player.SendMultipleMatchError(offlinelist);
                }
                else if (offlinelist.Any())
                {
                    ShowDBPlayer(offlinelist[0], args);
                }
            }
        }


        private void ShowPlayer(Player plr, CommandArgs args)
        {
            args.Player.SendInfoMessage("玩家：{0}", plr.name);
            args.Player.SendInfoMessage("生命：{0}/{1}", plr.statLife, plr.statLifeMax);
            args.Player.SendInfoMessage("魔力：{0}/{1}", plr.statMana, plr.statManaMax);

            // accessories
            // misc
            List<string> inventory = new List<string>();
            List<string> assist = new List<string>();
            List<string> armor = new List<string>();
            List<string> vanity = new List<string>();
            List<string> dye = new List<string>();
            List<string> miscEquips = new List<string>();
            List<string> miscDyes = new List<string>();
            List<string> bank = new List<string>();
            List<string> bank2 = new List<string>();
            List<string> bank3 = new List<string>();
            List<string> bank4 = new List<string>();

            String s;
            String trashDesc = GetItemDesc(plr.trashItem);
            for (int i = 0; i < 59; i++)
            {
                s = GetItemDesc(plr.inventory[i]);
                if (i < 50)
                {
                    if (s != "") inventory.Add(s);
                }
                else if (i >= 50 && i < 59)
                {
                    if (s != "") assist.Add(s);
                }
            }

            for (int i = 0; i < plr.armor.Length; i++)
            {
                s = GetItemDesc(plr.armor[i]);
                if (i < 10)
                {
                    if (s != "") armor.Add(s);
                }
                else
                {
                    if (s != "") vanity.Add(s);
                }
            }
            for (int i = 0; i < plr.dye.Length; i++)
            {
                s = GetItemDesc(plr.dye[i]);
                if (s != "") dye.Add(s);
            }

            for (int i = 0; i < plr.miscEquips.Length; i++)
            {
                s = GetItemDesc(plr.miscEquips[i]);
                if (s != "") miscEquips.Add(s);
            }

            for (int i = 0; i < plr.miscDyes.Length; i++)
            {
                s = GetItemDesc(plr.miscDyes[i]);
                if (s != "") miscDyes.Add(s);
            }

            for (int i = 0; i < plr.bank.item.Length; i++)
            {
                s = GetItemDesc(plr.bank.item[i]);
                if (s != "") bank.Add(s);
            }

            for (int i = 0; i < plr.bank2.item.Length; i++)
            {
                s = GetItemDesc(plr.bank2.item[i]);
                if (s != "") bank2.Add(s);
            }

            for (int i = 0; i < plr.bank3.item.Length; i++)
            {
                s = GetItemDesc(plr.bank3.item[i]);
                if (s != "") bank3.Add(s);
            }

            for (int i = 0; i < plr.bank4.item.Length; i++)
            {
                s = GetItemDesc(plr.bank4.item[i]);
                if (s != "") bank4.Add(s);
            }

            if (inventory.Count != 0) SendMultipleMessage(args, "●背包：", inventory);
            if (assist.Count != 0) SendMultipleMessage(args, "●钱币、弹药：", assist);
            if (trashDesc != "") args.Player.SendInfoMessage("●垃圾桶：", trashDesc);
            if (armor.Count != 0) SendMultipleMessage(args, "●装备栏：", armor);
            if (vanity.Count != 0) SendMultipleMessage(args, "●社交栏：", vanity);
            if (dye.Count != 0) SendMultipleMessage(args, "●染料1：", dye);
            if (miscEquips.Count != 0) SendMultipleMessage(args, "●工具栏：", miscEquips);
            if (miscDyes.Count != 0) SendMultipleMessage(args, "●染料2：", miscDyes);
            if (bank.Count != 0) SendMultipleMessage(args, "●储蓄罐：", bank);
            if (bank2.Count != 0) SendMultipleMessage(args, "●保险箱：", bank2);
            if (bank3.Count != 0) SendMultipleMessage(args, "●护卫熔炉：", bank3);
            if (bank4.Count != 0) SendMultipleMessage(args, "●虚空保险箱：", bank4);
        }

        public void SendMultipleMessage(CommandArgs args, String header,  List<String> matches)
		{
            matches[0] = header + matches[0];
			var lines = PaginationTools.BuildLinesFromTerms(matches.ToArray());
			lines.ForEach(args.Player.SendInfoMessage);
		}

        private string GetItemDesc(Item item)
        {
            string s = item.Name;
            var prefixName = TShock.Utils.GetPrefixById(item.prefix);
            if (prefixName != "")
                s = String.Format("{0}·{1}", prefixName, s);

            if (item.stack > 1)
                s = String.Format("{0}x{1}", s, item.stack);

            return s;
        }


        private void ShowDBPlayer(TShockAPI.DB.UserAccount dbplr, CommandArgs args)
        {
            var name = dbplr.Name;
            var data = TShock.CharacterDB.GetPlayerData(new TSPlayer(-1), dbplr.ID);
            if (data != null)
            {
                if (data.hideVisuals == null)
                {
                    args.Player.SendErrorMessage($"玩家 {name} 的数据不完整, 无法查看.");
                    return;
                }
                args.Player.SendInfoMessage("玩家：{0}", name);
                args.Player.SendInfoMessage("生命：{0}/{1}", data.health, data.maxHealth);
                args.Player.SendInfoMessage("魔力：{0}/{1}", data.mana, data.maxMana);


                // accessories
                // misc
                List<string> inventory = new List<string>();
                List<string> assist = new List<string>();
                List<string> armor = new List<string>();
                List<string> vanity = new List<string>();
                List<string> dye = new List<string>();
                List<string> miscEquips = new List<string>();
                List<string> miscDyes = new List<string>();
                List<string> bank = new List<string>();
                List<string> bank2 = new List<string>();
                List<string> bank3 = new List<string>();
                List<string> bank4 = new List<string>();

                String s;
                String trashDesc = "";
                for (int i = 0; i < 260; i++)
                {
                    s = GetNetItemDesc(data.inventory[i]);
                    if (i < 50)
                    {
                        if (s != "") inventory.Add(s);
                    }
                    else if (i >= 50 && i < 59)
                    {
                        if (s != "") assist.Add(s);
                    }
                    else if (i >= 59 && i < 68)
                    {
                        if (s != "") armor.Add(s);
                    }
                    else if (i >= 69 && i < 78)
                    {
                        if (s != "") vanity.Add(s);
                    }
                    else if (i >= 79 && i < 89)
                    {
                        if (s != "") dye.Add(s);
                    }
                    else if (i >= 89 && i < 94)
                    {
                        if (s != "") miscEquips.Add(s);
                    }
                    else if (i >= 94 && i < 99)
                    {
                        if (s != "") miscDyes.Add(s);
                    }
                    else if (i >= 99 && i < 139)
                    {
                        if (s != "") bank.Add(s);
                    }
                    else if (i >= 139 && i < 179)
                    {
                        if (s != "") bank2.Add(s);
                    }
                    else if (i >= 180 && i < 220)
                    {
                        if (s != "") bank3.Add(s);
                    }
                    else if (i >= 220 && i < 260)
                    {
                        if (s != "") bank4.Add(s);
                    }
                    else if (i == 179)
                    {
                        if (s != "") trashDesc = s;
                    }
                }

                if (inventory.Count != 0) SendMultipleMessage(args, "●背包：", inventory);
                if (assist.Count != 0) SendMultipleMessage(args, "●钱币、弹药：", assist);
                if (trashDesc != "") args.Player.SendInfoMessage("●垃圾桶：", trashDesc);
                if (armor.Count != 0) SendMultipleMessage(args, "●装备栏：", armor);
                if (vanity.Count != 0) SendMultipleMessage(args, "●社交栏：", vanity);
                if (dye.Count != 0) SendMultipleMessage(args, "●染料1：", dye);
                if (miscEquips.Count != 0) SendMultipleMessage(args, "●工具栏：", miscEquips);
                if (miscDyes.Count != 0) SendMultipleMessage(args, "●染料2：", miscDyes);
                if (bank.Count != 0) SendMultipleMessage(args, "●储蓄罐：", bank);
                if (bank2.Count != 0) SendMultipleMessage(args, "●保险箱：", bank2);
                if (bank3.Count != 0) SendMultipleMessage(args, "●护卫熔炉：", bank3);
                if (bank4.Count != 0) SendMultipleMessage(args, "●虚空保险箱：", bank4);

            }
            else
            {
                args.Player.SendErrorMessage($"未找到名称中包含 {name} 的玩家.");
            }
        }


        // 获得物品描述
        private string GetNetItemDesc(NetItem netItem)
        {
            String itemNameOrId = netItem.NetId.ToString();
            Item item;
            List<Item> matchedItems = TShock.Utils.GetItemByIdOrName(itemNameOrId);
            if (matchedItems.Count == 0)
            {
                return "";
            }
            else if (matchedItems.Count > 1)
            {
                // args.Player.SendMultipleMatchError(matchedItems.Select(i => $"{i.Name}({i.netID})"));
                return "";
            }
            else
            {
                item = matchedItems[0];
                string s = item.Name;
                var prefixName = TShock.Utils.GetPrefixById(netItem.PrefixId);
                if (prefixName != "")
                    s = String.Format("{0}·{1}", prefixName, s);

                if (netItem.Stack > 1)
                    s = String.Format("{0}x{1}", s, netItem.Stack);

                return s;
            }
        }


    }
}
