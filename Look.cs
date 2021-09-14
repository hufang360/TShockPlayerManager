using System.Collections.Generic;
using System;
using Terraria;
using TShockAPI;

namespace Plugin
{
    public class Look
    {
        private static bool ChatItemIsIcon;
        public static void LookPlayer(CommandArgs args, string name)
        {
            if( name == "" )
            {
                args.Player.SendErrorMessage("输入要查看的玩家名");
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
            FoundPlayer found  = Util.GetPlayer(args.Player, name);
            if( !found.valid )
                return;

            if( found.online )
            {
                ShowPlayer(args.Player, found.plr.TPlayer);
            } else {
                ShowDBPlayer(args.Player, found.Name, found.ID);
            }
        }

        private static void ShowPlayer(TSPlayer op, Player plr)
        {
           op.SendInfoMessage("玩家：{0}", plr.name);
           op.SendInfoMessage("生命：{0}/{1}", plr.statLife, plr.statLifeMax);
           op.SendInfoMessage("魔力：{0}/{1}", plr.statMana, plr.statManaMax);
           op.SendInfoMessage("渔夫任务完成数：{0} 次", plr.anglerQuestsFinished);
           op.SendInfoMessage("生态火把：{0}", plr.UsingBiomeTorches ? "已激活":"未激活" );
           op.SendInfoMessage("额外饰品栏：{0}", plr.extraAccessory ? "已激活":"未激活" );
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

            if (inventory.Count != 0) SendMultipleMessage(op, "●背包：", inventory);
            if (assist.Count != 0) SendMultipleMessage(op, "●钱币、弹药：", assist);
            if (trash.Count != 0) SendMultipleMessage(op, "●垃圾桶：", trash);
            if (armor.Count != 0) SendMultipleMessage(op, "●装备栏：", armor);
            if (vanity.Count != 0) SendMultipleMessage(op, "●社交栏：", vanity);
            if (dye.Count != 0) SendMultipleMessage(op, "●染料1：", dye);
            if (miscEquips.Count != 0) SendMultipleMessage(op, "●工具栏：", miscEquips);
            if (miscDyes.Count != 0) SendMultipleMessage(op, "●染料2：", miscDyes);
            if (bank.Count != 0) SendMultipleMessage(op, "●储蓄罐：", bank);
            if (bank2.Count != 0) SendMultipleMessage(op, "●保险箱：", bank2);
            if (bank3.Count != 0) SendMultipleMessage(op, "●护卫熔炉：", bank3);
            if (bank4.Count != 0) SendMultipleMessage(op, "●虚空保险箱：", bank4);
        }

        private  static string GetItemDesc(Item item)
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


        private  static void ShowDBPlayer(TSPlayer op, String username, int userid)
        {
            var name = username;
            var data = TShock.CharacterDB.GetPlayerData(new TSPlayer(-1), userid);
            if (data != null)
            {
                // if (data.hideVisuals == null)
                // {
                //     args.Player.SendErrorMessage($"玩家 {name} 的数据不完整, 无法查看.");
                //     return;
                // }
                op.SendInfoMessage("玩家：{0}", name);
                op.SendInfoMessage("生命：{0}/{1}", data.health, data.maxHealth);
                op.SendInfoMessage("魔力：{0}/{1}", data.mana, data.maxMana);
                op.SendInfoMessage("渔夫任务完成数：{0} 次", data.questsCompleted );
                op.SendInfoMessage("生态火把：{0}", data.unlockedBiomeTorches==1 ? "已激活":"未激活" );
                op.SendInfoMessage("额外饰品栏：{0}", data.extraSlot==1 ? "已激活":"未激活" );

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

                if (inventory.Count != 0) SendMultipleMessage(op, "●背包：", inventory);
                if (assist.Count != 0) SendMultipleMessage(op, "●钱币、弹药：", assist);
                if (trash.Count != 0) SendMultipleMessage(op, "●垃圾桶：", trash);
                if (armor.Count != 0) SendMultipleMessage(op, "●装备栏：", armor);
                if (vanity.Count != 0) SendMultipleMessage(op, "●社交栏：", vanity);
                if (dye.Count != 0) SendMultipleMessage(op, "●染料1：", dye);
                if (miscEquips.Count != 0) SendMultipleMessage(op, "●工具栏：", miscEquips);
                if (miscDyes.Count != 0) SendMultipleMessage(op, "●染料2：", miscDyes);
                if (bank.Count != 0) SendMultipleMessage(op, "●储蓄罐：", bank);
                if (bank2.Count != 0) SendMultipleMessage(op, "●保险箱：", bank2);
                if (bank3.Count != 0) SendMultipleMessage(op, "●护卫熔炉：", bank3);
                if (bank4.Count != 0) SendMultipleMessage(op, "●虚空保险箱：", bank4);

            }
            else
            {
                op.SendErrorMessage($"未找到名称中包含 {name} 的玩家.");
            }
        }

        // 获得物品描述
        private  static string GetNetItemDesc(NetItem netItem)
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
                var prefixName = TShock.Utils.GetPrefixById(netItem.PrefixId);
                if (prefixName != "")
                    s = String.Format("{0}·{1}", prefixName, s);

                if (netItem.Stack > 1)
                    s = String.Format("{0}x{1}", s, netItem.Stack);

                return s;
            }
        }

        public  static  void SendMultipleMessage(TSPlayer op, String header, List<String> matches)
        {
            if( ChatItemIsIcon ){
                if( matches.Count<=10 ){
                    matches[0] = header + matches[0];
                } else {
                    op.SendInfoMessage(header);
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
                        op.SendInfoMessage(s);
                        count = 0;
                        s = "";
                    }
                }
                if(  s!="" ){
                    op.SendInfoMessage(s);
                }

            } else {
                matches[0] = header + matches[0];
                var lines = PaginationTools.BuildLinesFromTerms(matches.ToArray());
                lines.ForEach(op.SendInfoMessage);
            }
        }


        public static void LookInventoryStr(TSPlayer op, string invstr)
        {
            var invarr = invstr.Split('~');
            if( invarr.Length !=260 ){
                op.SendErrorMessage("角色快照：本地快照配置有误！");
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
                s = GetNetItemDesc( NetItem.Parse(invarr[i]));
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

            if (inventory.Count != 0) SendMultipleMessage(op, "●背包：", inventory);
            if (assist.Count != 0) SendMultipleMessage(op, "●钱币、弹药：", assist);
            if (trash.Count != 0) SendMultipleMessage(op, "●垃圾桶：", trash);
            if (armor.Count != 0) SendMultipleMessage(op, "●装备栏：", armor);
            if (vanity.Count != 0) SendMultipleMessage(op, "●社交栏：", vanity);
            if (dye.Count != 0) SendMultipleMessage(op, "●染料1：", dye);
            if (miscEquips.Count != 0) SendMultipleMessage(op, "●工具栏：", miscEquips);
            if (miscDyes.Count != 0) SendMultipleMessage(op, "●染料2：", miscDyes);
            if (bank.Count != 0) SendMultipleMessage(op, "●储蓄罐：", bank);
            if (bank2.Count != 0) SendMultipleMessage(op, "●保险箱：", bank2);
            if (bank3.Count != 0) SendMultipleMessage(op, "●护卫熔炉：", bank3);
            if (bank4.Count != 0) SendMultipleMessage(op, "●虚空保险箱：", bank4);
        }
    }
}