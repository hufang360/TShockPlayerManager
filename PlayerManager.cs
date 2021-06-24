﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using Terraria.Localization;

namespace Plugin
{
    [ApiVersion(2, 1)]
    public class Plugin : TerrariaPlugin
    {

        #region Plugin Info
        public override string Author => "hufang360";
        public override string Description => "角色管理";
        public override string Name => "PlayerManager";
        public override Version Version => Assembly.GetExecutingAssembly().GetName().Version;
        #endregion

        public static readonly string save_dir = Path.Combine(TShock.SavePath, "PlayerManager");
        public static readonly string config_path = Path.Combine(save_dir, "config.json");

        private static Config _config;
        private List<NetItem> startingInventory = new List<NetItem>();
        private int update_count = 0;
        private int update_total = 60 * 10;


        public Plugin(Main game) : base(game)
        {
        }


        #region Initialize/Dispose
        public override void Initialize()
        {
            if (!Directory.Exists(save_dir))
                Directory.CreateDirectory(save_dir);
            LoadConfig();

            Commands.ChatCommands.Add(new Command(new List<string>() { "playermanager" }, PlayerManager, "playermanager", "pm") { HelpText = "角色管理" });
            Commands.ChatCommands.Add(new Command(new List<string>() { "playermanager.lockhp" }, LockHP, "lockhp") { HelpText = "锁血" });
            Commands.ChatCommands.Add(new Command(new List<string>() { "playermanager.startinventory" }, StartInventory, "startinventory", "si") { HelpText = "初始背包管理" });

            GetDataHandlers.PlayerSpawn += OnRespawn;
            ServerApi.Hooks.GameUpdate.Register(this, OnUpdate);
            ServerApi.Hooks.ServerJoin.Register(this, OnServerJoin, 421);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                GetDataHandlers.PlayerSpawn -= OnRespawn;
                ServerApi.Hooks.GameUpdate.Deregister(this, OnUpdate);
                ServerApi.Hooks.ServerJoin.Deregister(this, OnServerJoin);
            }
            base.Dispose(disposing);
        }
        #endregion


        private void LoadConfig()
        {
            startingInventory = TShock.ServerSideCharacterConfig.Settings.StartingInventory;

            _config = Config.Load(config_path);
            update_total = 60 * _config.LockHPSecond;
        }

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
                    args.Player.SendInfoMessage("/pm export [玩家名], 导出所有玩家的在线存档，可指定只导出某一个玩家");
                    args.Player.SendInfoMessage("/pm hp <玩家名> <生命值>, 修改单个玩家的生命值");
                    args.Player.SendInfoMessage("/pm maxhp <玩家名> <生命上限>, 修改单个玩家的生命上限");
                    args.Player.SendInfoMessage("/pm look <玩家名>, 查看某一个玩家的背包信息");
                    args.Player.SendInfoMessage("/lockhp help, 血量锁定 功能");
                    args.Player.SendInfoMessage("/si help, 初始背包和新手礼包 功能");
                    return;

                // 查看玩家背包
                case "look":
                    LookPlayer(args);
                    break;

                // 导出
                case "export":
                    #pragma warning disable 4014
                    ExportPlayer.Export(args);
                    #pragma warning restore 4014
                    break;

                case "hp":
                    ModifyHP(args);
                    break;
                case "maxhp":
                    ModifyMaxHP(args);
                    break;
                case "mana":
                    ModifyMana(args);
                    break;
                case "maxMana":
                    ModifyMaxMana(args);
                    break;
            }
        }

        #region modify hp mana
        private void ModifyHP(CommandArgs args)
        {
            if (args.Parameters.Count < 3)
            {
                args.Player.SendInfoMessage("语法错误，/pm hp <玩家名> <生命值>");
                return;
            }
            string pname = args.Parameters[1];
            string pvalue = args.Parameters[2];
            List<TSPlayer> players = TSPlayer.FindByNameOrID(pname);
            if (players.Count == 0)
            {
                args.Player.SendErrorMessage($"找不到名为 {pname} 的玩家!");
            }
            else if (players.Count > 1)
            {
                args.Player.SendMultipleMatchError(players.Select(p => p.Name));
            }
            else
            {
                var plr = players[0];
                int hp = 0;
                if ( int.TryParse(pvalue, out hp) ){
                    if(hp>0){
                        plr.TPlayer.statLife = hp;
                        NetMessage.SendData((int)PacketTypes.PlayerHp, -1, -1, NetworkText.Empty, plr.Index, 0f, 0f, 0f, 0);
                        args.Player.SendSuccessMessage($"{plr.Name} 的生命值已修改为 {hp}");
                    } else {
                        args.Player.SendSuccessMessage($"输入的生命值无效, {pvalue}");
                    }
                } else {
                    args.Player.SendSuccessMessage($"输入的生命值无效, {pvalue}");
                }
            }
        }

        private void ModifyMaxHP(CommandArgs args)
        {
            if (args.Parameters.Count < 3)
            {
                args.Player.SendInfoMessage("语法错误，/pm maxhp <玩家名> <生命上限>");
                return;
            }
            string pname = args.Parameters[1];
            string pvalue = args.Parameters[2];
            List<TSPlayer> players = TSPlayer.FindByNameOrID(pname);
            if (players.Count == 0)
            {
                args.Player.SendErrorMessage($"找不到名为 {pname} 的玩家!");
            }
            else if (players.Count > 1)
            {
                args.Player.SendMultipleMatchError(players.Select(p => p.Name));
            }
            else
            {
                var plr = players[0];
                int maxhp = 0;
                if ( int.TryParse(pvalue, out maxhp)){
                    if (maxhp<100){
                        args.Player.SendErrorMessage("最大生命值，不能低于100!");
                    } else {
                        plr.TPlayer.statLifeMax = maxhp;
                        NetMessage.SendData((int)PacketTypes.PlayerHp, -1, -1, NetworkText.Empty, plr.Index, 0f, 0f, 0f, 0);
                        args.Player.SendSuccessMessage($"{plr.Name} 的最大生命值已修改为 {maxhp}");
                    }
                } else {
                    args.Player.SendSuccessMessage($"输入的最大生命值无效, {pvalue}");
                }
            }
        }
        private void ModifyMana(CommandArgs args)
        {
            if (args.Parameters.Count < 3)
            {
                args.Player.SendInfoMessage("语法错误，/pm mana <玩家名> <魔力值>");
                return;
            }
            string pname = args.Parameters[1];
            string pvalue = args.Parameters[2];
            List<TSPlayer> players = TSPlayer.FindByNameOrID(pname);
            if (players.Count == 0)
            {
                args.Player.SendErrorMessage($"找不到名为 {pname} 的玩家!");
            }
            else if (players.Count > 1)
            {
                args.Player.SendMultipleMatchError(players.Select(p => p.Name));
            }
            else
            {
                var plr = players[0];
                int mana = 0;
                if ( int.TryParse(pvalue, out mana) ){
                    if(mana>0){
                        plr.TPlayer.statMana = mana;
                        NetMessage.SendData((int)PacketTypes.PlayerMana, -1, -1, NetworkText.Empty, plr.Index, 0f, 0f, 0f, 0);
                        args.Player.SendSuccessMessage($"{plr.Name} 的魔力值已修改为 {mana}");
                    } else {
                        args.Player.SendSuccessMessage($"输入的魔力值无效, {pvalue}");
                    }
                } else {
                    args.Player.SendSuccessMessage($"输入的魔力值无效, {pvalue}");
                }
            }
        }

        private void ModifyMaxMana(CommandArgs args)
        {
            if (args.Parameters.Count < 3)
            {
                args.Player.SendInfoMessage("语法错误，/pm maxhp <玩家名> <生命上限>");
                return;
            }
            string pname = args.Parameters[1];
            string pvalue = args.Parameters[2];
            List<TSPlayer> players = TSPlayer.FindByNameOrID(pname);
            if (players.Count == 0)
            {
                args.Player.SendErrorMessage($"找不到名为 {pname} 的玩家!");
            }
            else if (players.Count > 1)
            {
                args.Player.SendMultipleMatchError(players.Select(p => p.Name));
            }
            else
            {
                var plr = players[0];
                int mana = 0;
                if ( int.TryParse(pvalue, out mana)){
                    if (mana<20){
                        args.Player.SendErrorMessage("最大魔力值不能低于20!");
                    } else {
                        plr.TPlayer.statManaMax = mana;
                        NetMessage.SendData((int)PacketTypes.PlayerMana, -1, -1, NetworkText.Empty, plr.Index, 0f, 0f, 0f, 0);
                        args.Player.SendSuccessMessage($"{plr.Name} 的最大魔力值已修改为 {mana}");
                    }
                } else {
                    args.Player.SendSuccessMessage($"输入的最大魔力值无效, {pvalue}");
                }
            }
        }
        #endregion


        # region lock hp
        private void LockHP(CommandArgs args)
        {
            if (args.Parameters.Count == 0)
            {
                args.Player.SendInfoMessage("语法错误，输入 /lockhp help 显示帮助信息");
                return;
            }

            var choice = args.Parameters[0].ToLowerInvariant();
            switch (choice)
            {
                // 帮助
                case "help":
                    args.Player.SendInfoMessage("/lockhp info, 显示功能信息");
                    args.Player.SendInfoMessage("/lockhp true, 开启 血量锁定");
                    args.Player.SendInfoMessage("/lockhp false, 关闭 血量锁定");
                    args.Player.SendInfoMessage("/lockhp <血量>, 设置 锁血值");
                    break;

                case "info":
                    args.Player.SendInfoMessage("-----[血量锁定]-----");
                    if (_config.LockHPEnable)
                        args.Player.SendInfoMessage("功能: 已打开");
                    else
                        args.Player.SendInfoMessage("功能: 已关闭");
                    args.Player.SendInfoMessage("锁血值：{0}", _config.LockHPValue);
                    args.Player.SendInfoMessage("频率：{0}", _config.LockHPSecond);
                    break;

                case "true":
                    _config.LockHPEnable = true;
                    args.Player.SendInfoMessage("血量锁定 已打开");
                    File.WriteAllText(config_path, JsonConvert.SerializeObject(_config, Formatting.Indented));
                    break;

                case "false":
                    _config.LockHPEnable = false;
                    args.Player.SendInfoMessage("血量锁定 已关闭");
                    File.WriteAllText(config_path, JsonConvert.SerializeObject(_config, Formatting.Indented));
                    break;

                default:
                    int num;
                    if (int.TryParse(choice, out num))
                    {
                        if ( num<1){
                            args.Player.SendErrorMessage("锁血值 不能低于1");
                        } else{
                            _config.LockHPValue = num;
                            args.Player.SendSuccessMessage("锁血值 已改成 {0}", num);
                            File.WriteAllText(config_path, JsonConvert.SerializeObject(_config, Formatting.Indented));
                        }
                    }
                    else
                    {
                        args.Player.SendErrorMessage("语法错误");
                    }
                    break;
            }

        }
        private void OnRespawn(object o, GetDataHandlers.SpawnEventArgs args)
        {
            if (!_config.LockHPEnable)
                return;

            var plr = args.Player;
            if( plr.TPlayer.statLife > _config.LockHPValue && !plr.Dead ){
                plr.TPlayer.statLife = _config.LockHPValue;
                NetMessage.SendData((int)PacketTypes.PlayerHp, -1, -1, NetworkText.Empty, plr.Index, 0f, 0f, 0f, 0);
            }
        }
        private void OnUpdate(EventArgs args)
        {
            if (!_config.LockHPEnable)
                return;

            if (update_count < update_total)
            {
                update_count++;
                return;
            }
            else
            {
                update_count = 0;
            }

            foreach (TSPlayer plr in TShock.Players)
            {
                if (plr != null && !plr.Dead)
                {
                    if (plr.TPlayer.statLife > _config.LockHPValue){
                        plr.TPlayer.statLife = _config.LockHPValue;
                        NetMessage.SendData((int)PacketTypes.PlayerHp, -1, -1, NetworkText.Empty, plr.Index, 0f, 0f, 0f, 0);
                    }
                }
            }
        }
        # endregion


        #  region 初始背包管理
        private void StartInventory(CommandArgs args)
        {
            if (args.Parameters.Count ==0)
            {
                args.Player.SendInfoMessage("语法错误，输入 /si help 显示帮助信息");
                return;
            }

            var choice = args.Parameters[0].ToLowerInvariant();
            switch (choice)
            {
                default:
                    args.Player.SendErrorMessage("语法错误！");
                    break;

                // 帮助
                case "help":
                    args.Player.SendInfoMessage("/si info, 显示信息");
                    // args.Player.SendInfoMessage("/si add <物品id/名称> [堆叠数量] [词缀] , 添加物品，堆叠数量和词缀信息可不提供");
                    // args.Player.SendInfoMessage("/si del <物品id/名称>, 移除物品");
                    args.Player.SendInfoMessage("/si gif true, 开启 新手礼包功能");
                    args.Player.SendInfoMessage("/si gif false, 关闭 新手礼包功能");
                    break;

                case "info":
                    args.Player.SendInfoMessage("-----[新手礼包]-----");
                    if (_config.RandItemEnable)
                        args.Player.SendInfoMessage("功能：已打开");
                    else
                        args.Player.SendInfoMessage("功能：已关闭");
                    break;

                case "gift":
                    Gift(args);
                    break;
            }
        }

        // 新手礼包
        private void Gift(CommandArgs args)
        {
            if (args.Parameters.Count < 2)
            {
                args.Player.SendInfoMessage("语法错误，输入 /si help 显示帮助信息");
                return;
            }

            var choice = args.Parameters[1].ToLowerInvariant();
            switch (choice)
            {
                case "true":
                    _config.RandItemEnable = true;
                    RandomInventory();
                    args.Player.SendInfoMessage("新手礼包 已打开");
                    File.WriteAllText(config_path, JsonConvert.SerializeObject(_config, Formatting.Indented));
                    break;

                case "false":
                    _config.RandItemEnable = false;
                    TShock.ServerSideCharacterConfig.Settings.StartingInventory = startingInventory;
                    args.Player.SendInfoMessage("新手礼包 已关闭");
                    File.WriteAllText(config_path, JsonConvert.SerializeObject(_config, Formatting.Indented));
                    break;

                case "add":
                    break;

                case "del":
                    break;
            }
        }

        private void RandomInventory()
        {
            Random rd = new Random();
            List<MItem> inve1 = new List<MItem>();
            List<MItem> inve2 = new List<MItem>();

            inve2 = _config.RandItem1;
            if (inve2.Count > 0)
                inve1.Add(inve2[rd.Next(inve2.Count)]);

            inve2 = _config.RandItem2;
            if (inve2.Count > 0)
                inve1.Add(inve2[rd.Next(inve2.Count)]);

            inve2 = _config.RandItem3;
            if (inve2.Count > 0)
                inve1.Add(inve2[rd.Next(inve2.Count)]);

            inve2 = _config.RandItem4;
            if (inve2.Count > 0)
                inve1.Add(inve2[rd.Next(inve2.Count)]);

            inve2 = _config.RandItem5;
            if (inve2.Count > 0)
                inve1.Add(inve2[rd.Next(inve2.Count)]);

            inve2 = _config.RandItem6;
            if (inve2.Count > 0)
                inve1.Add(inve2[rd.Next(inve2.Count)]);

            inve2 = _config.RandItem7;
            if (inve2.Count > 0)
                inve1.Add(inve2[rd.Next(inve2.Count)]);

            inve2 = _config.RandItem8;
            if (inve2.Count > 0)
                inve1.Add(inve2[rd.Next(inve2.Count)]);

            inve2 = _config.RandItem9;
            if (inve2.Count > 0)
                inve1.Add(inve2[rd.Next(inve2.Count)]);

            inve2 = _config.RandItem10;
            if (inve2.Count > 0)
                inve1.Add(inve2[rd.Next(inve2.Count)]);


            List<NetItem> inve = new List<NetItem>();
            startingInventory.ForEach(i => inve.Add(i));
            inve1.ForEach( i => i.fixNetID() );
            inve1.ForEach( i => inve.Add(new NetItem(i.netID, i.stack, i.prefix)) );

            TShock.ServerSideCharacterConfig.Settings.StartingInventory = inve;
            TShock.Log.ConsoleInfo($"新手礼包 包含 {inve1.Count} 个物品");
        }
        private void OnServerJoin(JoinEventArgs args)
        {
            if (!_config.RandItemEnable)
                return;

            RandomInventory();
        }
        # endregion

        # region event


        # endregion


        # region look player
        bool ChatItemIsIcon;
        private void LookPlayer(CommandArgs args)
        {
            if (args.Parameters.Count<string>() == 1)
            {
                args.Player.SendErrorMessage("语法错误，需输入要查看的玩家名");
                return;
            }
            if ( TShock.VersionNum.CompareTo(new Version(4,5,3,0)) !=-1 )
            {
                ChatItemIsIcon = true;
            } else {
                ChatItemIsIcon = false;
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
            
            List<String> trash = new List<string>();
            s = GetItemDesc(plr.trashItem);
             if (s != "") trash.Add(s);

            if (inventory.Count != 0) SendMultipleMessage(args, "●背包：", inventory);
            if (assist.Count != 0) SendMultipleMessage(args, "●钱币、弹药：", assist);
            if (trash.Count != 0) SendMultipleMessage(args, "●垃圾桶：", trash);
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

        private string GetItemDesc(Item item)
        {
            string s = "";
            if (item.netID==0){
                return s;
            }

            if( ChatItemIsIcon ){
                // https://terraria.fandom.com/wiki/Chat
                // [i/s10:29]   数量
                // [i/p57:4]    词缀
                if(item.stack>1){
                    s = $"[i/s{item.stack}:{item.netID}]";
                }else {
                    s = $"[i/p{item.prefix}:{item.netID}]";
                }
                return s;
            }

            s = item.Name;
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
                List<string> trash = new List<string>();

                String s;
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
                    else if (i >= 59 && i < 69)
                    {
                        if (s != "") armor.Add(s);
                    }
                    else if (i >= 69 && i < 78)
                    {
                        if (s != "") vanity.Add(s);
                    }
                    else if (i >= 78 && i < 89)
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
                        if (s != "") trash.Add(s);
                    }
                }

                if (inventory.Count != 0) SendMultipleMessage(args, "●背包：", inventory);
                if (assist.Count != 0) SendMultipleMessage(args, "●钱币、弹药：", assist);
                if (trash.Count != 0) SendMultipleMessage(args, "●垃圾桶：", trash);
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
            string s = "";

            if (netItem.NetId==0){
                return s;
            }

            if( ChatItemIsIcon ){
                // https://terraria.fandom.com/wiki/Chat
                // [i/s10:29]   数量
                // [i/p57:4]    词缀
                if(netItem.Stack>1){
                    s = $"[i/s{netItem.Stack}:{netItem.NetId}]";
                }else {
                    s = $"[i/p{netItem.PrefixId}:{netItem.NetId}]";
                }
                return s;
            }

            Item item;
            String itemNameOrId = netItem.NetId.ToString();
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
                s = item.Name;
                var prefixName = TShock.Utils.GetPrefixById(item.prefix);
                if (prefixName != "")
                    s = String.Format("{0}·{1}", prefixName, s);

                if (item.stack > 1)
                    s = String.Format("{0}x{1}", s, item.stack);

                return s;
            }
        }

        public void SendMultipleMessage(CommandArgs args, String header, List<String> matches)
        {
            if( ChatItemIsIcon ){
                if( matches.Count<=10 ){
                    matches[0] = header + matches[0];
                } else {
                    args.Player.SendInfoMessage(header);
                }
                // 一行显示10个物品
                var s = "";
                var count=0;
                for (int i = 0; i < matches.Count; i++)
                {
                    s += matches[i] + " ";
                    count ++;
                    if (count>=10)
                    {
                        args.Player.SendInfoMessage(s);
                        count = 0;
                        s = "";
                    }
                }
                if(  s!="" ){
                    args.Player.SendInfoMessage(s);
                }

            } else {
                matches[0] = header + matches[0];
                var lines = PaginationTools.BuildLinesFromTerms(matches.ToArray());
                lines.ForEach(args.Player.SendInfoMessage);
            }
        }
        # endregion


    }
}
