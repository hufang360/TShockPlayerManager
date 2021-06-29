using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using Terraria.Localization;
using TShockAPI.DB;
using System.Data;

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
            if (!Directory.Exists(save_dir))
                Directory.CreateDirectory(save_dir);

            Commands.ChatCommands.Add(new Command(new List<string>() { "playermanager" }, PlayerManager, "playermanager", "pm") { HelpText = "玩家管理" });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
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
                    args.Player.SendInfoMessage("/pm exportall, 导出所有玩家的人物存档");
                    args.Player.SendInfoMessage("/pm export [玩家名], 导出单个玩家的人物存档");
                    args.Player.SendInfoMessage("/pm look <玩家名>, 查看玩家");
                    args.Player.SendInfoMessage("/pm hp <玩家名> <生命值>, 修改生命值");
                    args.Player.SendInfoMessage("/pm maxhp <玩家名> <生命上限>, 修改生命上限");
                    args.Player.SendInfoMessage("/pm mana <玩家名> <魔力上限>, 修改魔力值");
                    args.Player.SendInfoMessage("/pm maxmana <玩家名> <魔力上限>, 修改魔力上限");
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

                // 导出全部
                case "exportall":
                    #pragma warning disable 4014
                    ExportPlayer.ExportAll(args);
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
            string name = args.Parameters[1];
            string value = args.Parameters[2];
            int hp = 0;
            if (  !int.TryParse(value, out hp) ){
                hp = 0;
            }
            if (hp<1){
                args.Player.SendSuccessMessage($"输入的生命值无效, {value}");
                return;
            }


            TSPlayer plr  = null;
            TShockAPI.DB.UserAccount plrDB = null;
            GetPlayer(args, name, out plr, out plrDB);
            if (plr != null)
            {
                plr.TPlayer.statLife = hp;
                NetMessage.SendData((int)PacketTypes.PlayerHp, -1, -1, NetworkText.Empty, plr.Index, 0f, 0f, 0f, 0);
                args.Player.SendSuccessMessage($"{plr.Name} 的生命值已修改为 {hp}");
            }
            if(plrDB!=null){
                bool success = StatsDB(hp, 1, plrDB, args);
                if( success ){
                    args.Player.SendSuccessMessage($"{plrDB.Name} 的生命值已修改为 {hp}");
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
            string name = args.Parameters[1];
            string value = args.Parameters[2];
            int maxhp = 0;
            if (  !int.TryParse(value, out maxhp) ){
                maxhp = 0;
            }
            if (maxhp<100){
                args.Player.SendErrorMessage("生命上限，不能低于100!");
                return;
            }

            TSPlayer plr  = null;
            TShockAPI.DB.UserAccount plrDB = null;
            GetPlayer(args, name, out plr, out plrDB);
            if (plr != null)
            {
                plr.TPlayer.statLifeMax = maxhp;
                NetMessage.SendData((int)PacketTypes.PlayerHp, -1, -1, NetworkText.Empty, plr.Index, 0f, 0f, 0f, 0);
                args.Player.SendSuccessMessage($"{plr.Name} 的生命上限已修改为 {maxhp}");
            }
            if(plrDB!=null){
                bool success = StatsDB(maxhp, 2, plrDB, args);
                if( success ){
                    args.Player.SendSuccessMessage($"{plrDB.Name} 的生命上限已修改为 {maxhp}");
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
            string name = args.Parameters[1];
            string value = args.Parameters[2];
            int mana = 0;
            if (  !int.TryParse(value, out mana) ){
                mana = 0;
            }
            if (mana<1){
                args.Player.SendSuccessMessage($"输入的魔力值无效, {value}");
                return;
            }

            TSPlayer plr  = null;
            TShockAPI.DB.UserAccount plrDB = null;
            GetPlayer(args, name, out plr, out plrDB);
            if (plr != null)
            {
                plr.TPlayer.statMana = mana;
                NetMessage.SendData((int)PacketTypes.PlayerMana, -1, -1, NetworkText.Empty, plr.Index, 0f, 0f, 0f, 0);
                args.Player.SendSuccessMessage($"{plr.Name} 的魔力值已修改为 {mana}");
            }
            if(plrDB!=null){
                bool success = StatsDB(mana, 3, plrDB, args);
                if( success ){
                    args.Player.SendSuccessMessage($"{plrDB.Name} 的魔力值已修改为 {mana}");
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
            string name = args.Parameters[1];
            string value = args.Parameters[2];
            int maxMana = 0;
            if (  !int.TryParse(value, out maxMana) ){
                maxMana = 0;
            }
            if (maxMana<20){
                args.Player.SendErrorMessage("魔力上限，不能低于20!");
                return;
            }

            TSPlayer plr  = null;
            TShockAPI.DB.UserAccount plrDB = null;
            GetPlayer(args, name, out plr, out plrDB);
            if (plr != null)
            {
                plr.TPlayer.statManaMax = maxMana;
                NetMessage.SendData((int)PacketTypes.PlayerMana, -1, -1, NetworkText.Empty, plr.Index, 0f, 0f, 0f, 0);
                args.Player.SendSuccessMessage($"{plr.Name} 的魔力上限已修改为 {maxMana}");
            }
            if(plrDB!=null){
                bool success = StatsDB(maxMana, 4, plrDB, args);
                if( success ){
                    args.Player.SendSuccessMessage($"{plrDB.Name} 的魔力上限已修改为 {maxMana}");
                }
            }
        }
        private bool StatsDB(int vaule, int type, TShockAPI.DB.UserAccount plrDB, CommandArgs args){
            var data = TShock.CharacterDB.GetPlayerData(new TSPlayer(-1), plrDB.ID);
            if (data != null)
            {
                IDbConnection db = TShock.CharacterDB.database;

                switch (type)
                {
                    case 1:
                    data.health = vaule;
                    break;
                    case 2:
                    data.maxHealth = vaule;
                    break;
                    case 3:
                    data.mana = vaule;
                    break;
                    case 4:
                    data.maxMana = vaule;
                    break;

                    default:
                    args.Player.SendErrorMessage("type 传值错误");
                    return false;
                }
                args.Player.SendSuccessMessage("测试是否写入数据库");
                try
                {
                    db.Query("UPDATE tsCharacter SET Health = @0, MaxHealth = @1, Mana = @2, MaxMana = @3 WHERE Account = @4;", data.health, data.maxHealth, data.mana, data.maxMana, plrDB.ID);
                    // args.Player.SendSuccessMessage(plrDB.Name + "'s stats have been reset!");
                    return true;
                }
                catch (Exception ex)
                {
                    args.Player.SendErrorMessage("An error occurred while resetting!");
                    TShock.Log.ConsoleError(ex.ToString());
                    return false;
                }
            }
            return false;
        }

        private void GetPlayer(CommandArgs args, String name, out TSPlayer plr, out TShockAPI.DB.UserAccount plrDB){
            plr = null;
            plrDB = null;
            var players = TSPlayer.FindByNameOrID($"tsn:{name}");
             if (players.Count > 1)
            {
                if(players[0].TPlayer.name==name){
                    plr = players[0];
                } else {
                    args.Player.SendMultipleMatchError(players);
                }
            }
            else if (players.Any())
            {
                plr = players[0];
            }else{
                // 离线玩家
                var offlinePlayers = TShock.UserAccounts.GetUserAccountsByName(name);
                if (offlinePlayers.Count ==0)
                {
                    args.Player.SendErrorMessage($"找不到名为 {name} 的玩家!");
                }
                else if (offlinePlayers.Count > 1 && offlinePlayers[0].Name!=name)
                {
                    args.Player.SendMultipleMatchError(offlinePlayers);
                }
                else if (offlinePlayers.Any())
                {
                    plrDB = offlinePlayers[0];
                }
            }
        }
        #endregion


        # region look player
        bool ChatItemIsIcon;
        private void LookPlayer(CommandArgs args)
        {
            if (args.Parameters.Count<string>() == 1)
            {
                args.Player.SendErrorMessage("需输入要查看的玩家名");
                return;
            }

            // 控制台显示 物品名称
            // 4.4.0 -1.4.1.2   [i:4444]
            // 4.5.0 -1.4.2.2   [女巫扫帚]
            if ( TShock.VersionNum.CompareTo(new Version(4,5,0,0)) !=-1 )
            {
                ChatItemIsIcon = true;
            } else {
                ChatItemIsIcon = false;
            }

            var name = args.Parameters[1];
            TSPlayer plr  = null;
            TShockAPI.DB.UserAccount plrDB = null;
            GetPlayer(args, name, out plr, out plrDB);
            if (plr !=null){
                ShowPlayer(plr.TPlayer, args);
            }
            if(plrDB != null){
                ShowDBPlayer(plrDB, args);
            }
        }


        private void ShowPlayer(Player plr, CommandArgs args)
        {
            args.Player.SendInfoMessage("玩家：{0}", plr.name);
            args.Player.SendInfoMessage("生命：{0}/{1}", plr.statLife, plr.statLifeMax);
            args.Player.SendInfoMessage("魔力：{0}/{1}", plr.statMana, plr.statManaMax);
            args.Player.SendInfoMessage("渔夫任务：{0} 次", plr.anglerQuestsFinished);
            args.Player.SendInfoMessage("生态火把：{0}", plr.UsingBiomeTorches ? "已使用 火把神徽章":"未使用 火把神徽章");
            args.Player.SendInfoMessage("饰品槽：{0}", plr.extraAccessory ? "已使用 恶魔之心":"未使用 恶魔之心" );

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
                args.Player.SendInfoMessage("渔夫任务：{0} 次", data.questsCompleted );
                args.Player.SendInfoMessage("生态火把：{0}", data.unlockedBiomeTorches==1 ? "已使用 火把神徽章":"未使用 火把神徽章");
                args.Player.SendInfoMessage("饰品槽：{0}", data.extraSlot==1 ? "已使用 恶魔之心":"未使用 恶魔之心" );

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
