using System;
using System.Collections.Generic;
using Terraria;
using TShockAPI;

namespace PlayerManager
{
    public class Look
    {
        #region 查看玩家背包
        public static void LookBag(CommandArgs args)
        {
            if (args.Parameters.Count == 0 && !args.Player.RealPlayer)
                args.Player.SendErrorMessage("请输入玩家名，/lookbag <玩家名>");
            else
                LookPlayer(args);
        }
        #endregion



        private static bool ChatItemIsIcon;
        public static void LookPlayer(CommandArgs args)
        {
            string name;

            if (args.Parameters.Count == 0)
            {
                if (!args.Player.RealPlayer)
                {
                    args.Player.SendErrorMessage("请输入玩家名，/pm look <玩家名>");
                    return;
                }
                else
                {
                    name = args.Player.Name;
                }
            }
            else
            {
                name = string.Join("", args.Parameters);
            }


            // 控制台显示 物品名称
            // 4.4.0 -1.4.1.2   [i:4444]
            // 4.5.0 -1.4.2.2   [女巫扫帚]
            ChatItemIsIcon = TShock.VersionNum.CompareTo(new Version(4, 5, 0, 0)) >= 0;
            //Console.WriteLine($"ChatItemIsIcon:");


            FoundPlayer found = Utils.GetPlayer(name, out string errMsg);
            if (!found.valid)
            {
                args.Player.SendErrorMessage(errMsg);
                return;
            }

            if (found.online)
                ShowPlayer(args.Player, found.plr.TPlayer);
            else
                ShowDBPlayer(args.Player, found.Name, found.ID);
        }

        /// <summary>
        /// 查看玩家信息（在线）
        /// </summary>
        static void ShowPlayer(TSPlayer op, Player plr)
        {
            op.SendInfoMessage("玩家：{0}", plr.name);
            op.SendInfoMessage("生命：{0}/{1}", plr.statLife, plr.statLifeMax);
            op.SendInfoMessage("魔力：{0}/{1}", plr.statMana, plr.statManaMax);
            if (plr.anglerQuestsFinished > 0) op.SendInfoMessage("渔夫任务：{0} 次", plr.anglerQuestsFinished);
            //if (plr.numberOfDeathsPVE > 0 || plr.numberOfDeathsPVP > 0)
            //{
            //    string[] f = new string[] {
            //        plr.numberOfDeathsPVE > 0 ? $"被杀死了{plr.numberOfDeathsPVE}次" : "",
            //        plr.numberOfDeathsPVP > 0 ? $"被其它玩家杀死了{plr.numberOfDeathsPVP}次" : "",
            //    };
            //    op.SendInfoMessage($"死亡统计：{string.Join(", ", f)}");
            //}

            List<string> enhance = new();
            if (plr.extraAccessory) enhance.Add("[i:3335]"); // 3335 恶魔之心
            if (plr.unlockedBiomeTorches) enhance.Add("[i:5043]"); // 5043 火把神徽章
            if (plr.ateArtisanBread) enhance.Add("[i:5326]"); // 5326	工匠面包
            if (plr.usedAegisCrystal) enhance.Add("[i:5337]");    // 5337 生命水晶	永久强化生命再生 
            if (plr.usedAegisFruit) enhance.Add("[i:5338]");  // 5338 埃癸斯果	永久提高防御力 
            if (plr.usedArcaneCrystal) enhance.Add("[i:5339]"); // 5339 奥术水晶	永久提高魔力再生 
            if (plr.usedGalaxyPearl) enhance.Add("[i:5340]"); // 5340	银河珍珠	永久增加运气 
            if (plr.usedGummyWorm) enhance.Add("[i:5341]"); // 5341	黏性蠕虫	永久提高钓鱼技能  
            if (plr.usedAmbrosia) enhance.Add("[i:5342]"); // 5342	珍馐	 永久提高采矿和建造速度 
            if (plr.unlockedSuperCart) enhance.Add("[i:5289]"); // 5289	矿车升级包
            if (enhance.Count != 0) SendMultipleMessage(op, "永久增强：", enhance);

            #region 读取格子数据
            // accessories
            // misc
            List<string> inventory = new();
            List<string> assist = new();
            List<string> armor = new();
            List<string> vanity = new();
            List<string> dye = new();
            List<string> miscEquips = new();
            List<string> miscDyes = new();
            List<string> bank = new();
            List<string> bank2 = new();
            List<string> bank3 = new();
            List<string> bank4 = new();
            List<string> armor1 = new();
            List<string> armor2 = new();
            List<string> armor3 = new();
            List<string> vanity1 = new();
            List<string> vanity2 = new();
            List<string> vanity3 = new();
            List<string> dye1 = new();
            List<string> dye2 = new();
            List<string> dye3 = new();

            string s;
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

            // 装备（loadout）
            for (int i = 0; i < plr.Loadouts.Length; i++)
            {
                Item[] items = plr.Loadouts[i].Armor;
                // 装备 和 时装
                for (int j = 0; j < items.Length; j++)
                {
                    s = GetItemDesc(items[j]);
                    if (!string.IsNullOrEmpty(s))
                    {
                        if (i == 0)
                        {
                            if (j < 10)
                                armor1.Add(s);
                            else
                                vanity1.Add(s);
                        }
                        else if (i == 1)
                        {
                            if (j < 10)
                                armor2.Add(s);
                            else
                                vanity2.Add(s);
                        }
                        else if (i == 2)
                        {
                            if (j < 10)
                                armor3.Add(s);
                            else
                                vanity3.Add(s);
                        }
                    }
                }

                // 染料
                items = plr.Loadouts[i].Dye;
                for (int j = 0; j < items.Length; j++)
                {
                    s = GetItemDesc(items[j]);
                    if (!string.IsNullOrEmpty(s))
                    {
                        if (i == 0) dye1.Add(s);
                        else if (i == 1) dye2.Add(s);
                        else if (i == 2) dye3.Add(s);
                    }
                }
            }
            #endregion

            List<string> trash = new();
            s = GetItemDesc(plr.trashItem);
            if (s != "") trash.Add(s);

            if (inventory.Count != 0) SendMultipleMessage(op, "背包：", inventory);
            if (trash.Count != 0) SendMultipleMessage(op, "垃圾桶：", trash);
            if (assist.Count != 0) SendMultipleMessage(op, "钱弹：", assist);

            int num = plr.CurrentLoadoutIndex + 1;
            if (armor.Count != 0) SendMultipleMessage(op, $">装备{num}：", armor);
            if (vanity.Count != 0) SendMultipleMessage(op, $">时装{num}：", vanity);
            if (dye.Count != 0) SendMultipleMessage(op, $">染料{num}：", dye);

            if (armor1.Count != 0) SendMultipleMessage(op, "装备1：", armor1);
            if (vanity1.Count != 0) SendMultipleMessage(op, "时装1：", vanity1);
            if (dye1.Count != 0) SendMultipleMessage(op, "染料1：", dye1);

            if (armor2.Count != 0) SendMultipleMessage(op, "装备2：", armor2);
            if (vanity2.Count != 0) SendMultipleMessage(op, "时装2：", vanity2);
            if (dye2.Count != 0) SendMultipleMessage(op, "染料2：", dye2);

            if (armor3.Count != 0) SendMultipleMessage(op, "装备3：", armor3);
            if (vanity3.Count != 0) SendMultipleMessage(op, "时装3：", vanity3);
            if (dye3.Count != 0) SendMultipleMessage(op, "染料3：", dye3);

            if (miscEquips.Count != 0) SendMultipleMessage(op, "工具栏：", miscEquips);
            if (miscDyes.Count != 0) SendMultipleMessage(op, "工具栏染料：", miscDyes);
            if (bank.Count != 0) SendMultipleMessage(op, "储蓄罐：", bank);
            if (bank2.Count != 0) SendMultipleMessage(op, "保险箱：", bank2);
            if (bank3.Count != 0) SendMultipleMessage(op, "护卫熔炉：", bank3);
            if (bank4.Count != 0) SendMultipleMessage(op, "虚空保险箱：", bank4);
            op.SendInfoMessage("*游戏内按↑↓键可滚动查看");
        }

        /// <summary>
        /// 获得物品描述
        /// </summary>
        private static string GetItemDesc(Item item)
        {
            if (item.netID == 0)
                return "";

            if (ChatItemIsIcon)
            {
                if (item.stack > 1)
                {
                    return $"[i/s{item.stack}:{item.netID}]";
                }
                else
                {
                    if (item.prefix.Equals(0))
                        return $"[i:{item.netID}]";
                    else
                        return $"[i/p{item.prefix}:{item.netID}]";
                }
            }

            string s;
            s = item.Name;
            string prefixName = TShock.Utils.GetPrefixById(item.prefix);
            if (prefixName != "")
                s = string.Format("{0}·{1}", prefixName, s);

            if (item.stack > 1)
                s = string.Format("{0}x{1}", s, item.stack);

            return s;
        }


        /// <summary>
        /// 查看玩家信息（离线）
        /// </summary>
        static void ShowDBPlayer(TSPlayer op, String username, int userid)
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
                op.SendInfoMessage("玩家：{0}（已离线）", username);
                op.SendInfoMessage("生命：{0}/{1}", data.health, data.maxHealth);
                op.SendInfoMessage("魔力：{0}/{1}", data.mana, data.maxMana);

                if (data.questsCompleted > 0) op.SendInfoMessage("渔夫任务：{0} 次", data.questsCompleted);
                //if (data.numberOfDeathsPVE > 0 || data.numberOfDeathsPVP > 0)
                //{
                //    string[] f = new string[] {
                //        data.numberOfDeathsPVE > 0 ? $"被杀死了{data.numberOfDeathsPVE}次" : "",
                //        data.numberOfDeathsPVP > 0 ? $"被其它玩家杀死了{data.numberOfDeathsPVP}次" : "",
                //    };
                //    op.SendInfoMessage($"死亡统计：{string.Join(", ", f)}");
                //}

                List<string> enhance = new();
                if (data.extraSlot == 1) enhance.Add("[i:3335]"); // 3335 恶魔之心
                if (data.unlockedBiomeTorches == 1) enhance.Add("[i:5043]"); // 5043 火把神徽章
                if (data.ateArtisanBread == 1) enhance.Add("[i:5326]"); // 5326	工匠面包
                if (data.usedAegisCrystal == 1) enhance.Add("[i:5337]");    // 5337 生命水晶	永久强化生命再生 
                if (data.usedAegisFruit == 1) enhance.Add("[i:5338]");  // 5338 埃癸斯果	永久提高防御力 
                if (data.usedArcaneCrystal == 1) enhance.Add("[i:5339]"); // 5339 奥术水晶	永久提高魔力再生 
                if (data.usedGalaxyPearl == 1) enhance.Add("[i:5340]"); // 5340	银河珍珠	永久增加运气 
                if (data.usedGummyWorm == 1) enhance.Add("[i:5341]"); // 5341	黏性蠕虫	永久提高钓鱼技能  
                if (data.usedAmbrosia == 1) enhance.Add("[i:5342]"); // 5342	珍馐	永久提高采矿和建造速度 
                if (data.unlockedSuperCart == 1) enhance.Add("[i:5289]"); // 5289	矿车升级包
                if (enhance.Count != 0) SendMultipleMessage(op, "永久增强：", enhance);

                #region 读取格子
                // accessories
                // misc
                List<string> inventory = new();
                List<string> assist = new();
                List<string> armor = new();
                List<string> vanity = new();
                List<string> dye = new();
                List<string> miscEquips = new();
                List<string> miscDyes = new();
                List<string> bank = new();
                List<string> bank2 = new();
                List<string> bank3 = new();
                List<string> bank4 = new();
                List<string> trash = new();

                List<string> armor1 = new();
                List<string> armor2 = new();
                List<string> armor3 = new();
                List<string> vanity1 = new();
                List<string> vanity2 = new();
                List<string> vanity3 = new();
                List<string> dye1 = new();
                List<string> dye2 = new();
                List<string> dye3 = new();

                string s;
                for (int i = 0; i < NetItem.MaxInventory; i++)
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
                    else if (i >= 69 && i < 79)
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
                    else if (i == 179)
                    {
                        if (s != "") trash.Add(s);
                    }
                    else if (i >= 180 && i < 220)
                    {
                        if (s != "") bank3.Add(s);
                    }
                    else if (i >= 220 && i < 260)
                    {
                        if (s != "") bank4.Add(s);
                    }

                    // loadout
                    else if (i >= 260 && i < 270)
                    {
                        if (s != "") armor1.Add(s);
                    }
                    else if (i >= 270 && i < 280)
                    {
                        if (s != "") vanity1.Add(s);
                    }
                    else if (i >= 280 && i < 290)
                    {
                        if (s != "") dye1.Add(s);
                    }


                    else if (i >= 290 && i < 300)
                    {
                        if (s != "") armor2.Add(s);
                    }
                    else if (i >= 300 && i < 310)
                    {
                        if (s != "") vanity2.Add(s);
                    }
                    else if (i >= 310 && i < 320)
                    {
                        if (s != "") dye2.Add(s);
                    }

                    else if (i >= 320 && i < 330)
                    {
                        if (s != "") armor3.Add(s);
                    }
                    else if (i >= 330 && i < 340)
                    {
                        if (s != "") vanity3.Add(s);
                    }
                    else if (i >= 340 && i < 350)
                    {
                        if (s != "") dye3.Add(s);
                    }

                }
                #endregion

                if (inventory.Count != 0) SendMultipleMessage(op, "背包：", inventory);
                if (trash.Count != 0) SendMultipleMessage(op, "垃圾桶：", trash);
                if (assist.Count != 0) SendMultipleMessage(op, "钱币弹药：", assist);


                int num = data.currentLoadoutIndex + 1;
                if (armor.Count != 0) SendMultipleMessage(op, $">装备{num}：", armor);
                if (vanity.Count != 0) SendMultipleMessage(op, $">时装{num}：", vanity);
                if (dye.Count != 0) SendMultipleMessage(op, $">染料{num}：", dye);

                if (armor1.Count != 0) SendMultipleMessage(op, "装备1：", armor1);
                if (vanity1.Count != 0) SendMultipleMessage(op, "时装1：", vanity1);
                if (dye1.Count != 0) SendMultipleMessage(op, "染料1：", dye1);

                if (armor2.Count != 0) SendMultipleMessage(op, "装备2：", armor2);
                if (vanity2.Count != 0) SendMultipleMessage(op, "时装2：", vanity2);
                if (dye2.Count != 0) SendMultipleMessage(op, "染料2：", dye2);

                if (armor3.Count != 0) SendMultipleMessage(op, "装备3：", armor3);
                if (vanity3.Count != 0) SendMultipleMessage(op, "时装3：", vanity3);
                if (dye3.Count != 0) SendMultipleMessage(op, "染料3：", dye3);

                if (miscEquips.Count != 0) SendMultipleMessage(op, "工具栏：", miscEquips);
                if (miscDyes.Count != 0) SendMultipleMessage(op, "染料2：", miscDyes);
                if (bank.Count != 0) SendMultipleMessage(op, "储蓄罐：", bank);
                if (bank2.Count != 0) SendMultipleMessage(op, "保险箱：", bank2);
                if (bank3.Count != 0) SendMultipleMessage(op, "护卫熔炉：", bank3);
                if (bank4.Count != 0) SendMultipleMessage(op, "虚空保险箱：", bank4);
                op.SendInfoMessage("*游戏内按↑↓键可滚动查看");
            }
            else
            {
                op.SendErrorMessage($"未找到名称中包含 {name} 的玩家.");
            }
        }

        // 获得物品描述
        private static string GetNetItemDesc(NetItem netItem)
        {
            if (netItem.NetId == 0) return "";

            if (ChatItemIsIcon)
            {
                // https://terraria.fandom.com/wiki/Chat
                // [i:29]   数量
                // [i/s10:29]   数量
                // [i/p57:4]    词缀
                if (netItem.Stack > 1)
                {
                    return $"[i/s{netItem.Stack}:{netItem.NetId}]";
                }
                else
                {
                    if (netItem.PrefixId.Equals(0))
                        return $"[i:{netItem.NetId}]";
                    else
                        return $"[i/p{netItem.PrefixId}:{netItem.NetId}]";
                }
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
                string s = item.Name;
                var prefixName = TShock.Utils.GetPrefixById(netItem.PrefixId);
                if (prefixName != "")
                    s = string.Format("{0}·{1}", prefixName, s);

                if (netItem.Stack > 1)
                    s = string.Format("{0}x{1}", s, netItem.Stack);

                return s;
            }
        }

        public static void SendMultipleMessage(TSPlayer op, string header, List<string> matches)
        {
            // if( ChatItemIsIcon ){
            //if (matches.Count <= 10)
            //{
            //    matches[0] = header + matches[0];
            //}
            //else
            //{
            //    op.SendInfoMessage(header);
            //}
            matches[0] = header + matches[0];
            // 一行显示10个物品
            var s = "";
            var count = 0;
            for (int i = 0; i < matches.Count; i++)
            {
                s += matches[i] + " ";
                count++;
                if (count >= 10)
                {
                    op.SendInfoMessage(s);
                    count = 0;
                    s = "";
                }
            }
            if (s != "")
            {
                op.SendInfoMessage(s);
            }

            // } else {
            //     matches[0] = header + matches[0];
            //     var lines = PaginationTools.BuildLinesFromTerms(matches.ToArray());
            //     lines.ForEach(op.SendInfoMessage);
            // }
        }


        public static void LookInventoryStr(TSPlayer op, string invstr)
        {
            var invarr = invstr.Split('~');
            if (invarr.Length != 260)
            {
                op.SendErrorMessage("角色快照：本地快照配置有误！");
                return;
            }

            // accessories
            // misc
            List<string> inventory = new();
            List<string> assist = new();
            List<string> armor = new();
            List<string> vanity = new();
            List<string> dye = new();
            List<string> miscEquips = new();
            List<string> miscDyes = new();
            List<string> bank = new();
            List<string> bank2 = new();
            List<string> bank3 = new();
            List<string> bank4 = new();
            List<string> trash = new();

            String s;
            for (int i = 0; i < 260; i++)
            {
                s = GetNetItemDesc(NetItem.Parse(invarr[i]));
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
            if (trash.Count != 0) SendMultipleMessage(op, "●垃圾桶：", trash);
            if (assist.Count != 0) SendMultipleMessage(op, "●钱弹：", assist);
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