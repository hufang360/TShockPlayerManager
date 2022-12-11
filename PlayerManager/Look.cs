using System.Collections.Generic;
using System.Linq;
using Terraria;
using TShockAPI;

namespace PlayerManager
{
    /// <summary>
    /// 查看玩家
    /// </summary>
    public class Look
    {
        #region 查看玩家背包
        public static void LookBag(CommandArgs args)
        {
            if (args.Parameters.Count == 0 && !args.Player.RealPlayer)
                args.Player.SendErrorMessage("请输入玩家名，/lookbag <玩家名>");
            else
                LookPlayer(args, true);
        }
        #endregion


        /// <summary>
        /// 查看玩家
        /// </summary>
        /// <param name="args"></param>
        /// <param name="isFlag">true=使用tag标签进行展示，false=读取物品名进行展示</param>
        public static void LookPlayer(CommandArgs args, bool isFlag)
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


            FoundPlayer found = Utils.GetPlayer(name, out string errMsg);
            if (!found.valid)
            {
                args.Player.SendErrorMessage(errMsg);
                return;
            }

            List<string> msgs = new();
            if (found.online)
            {
                ShowPlayer(found.plr.TPlayer, isFlag, ref msgs);
            }
            else
            {
                ShowDBPlayer(found.Name, found.ID, isFlag, ref msgs, ref errMsg);
                if (!string.IsNullOrEmpty(errMsg))
                {
                    args.Player.SendErrorMessage(errMsg);
                    return;
                }
            }
            if (!isFlag)
            {
                foreach (var s in msgs)
                {
                    args.Player.SendInfoMessage(s);
                }
            }
            else
            {

                args.Player.SendInfoMessage(string.Join("\n", msgs));
            }
        }

        /// <summary>
        /// 查看玩家信息（在线）
        /// <param name="isFlag">true=使用tag标签进行展示，false=读取物品名进行展示</param>
        /// </summary>
        static void ShowPlayer(Player plr, bool isFlag, ref List<string> msgs)
        {
            msgs.Add($"玩家：{plr.name}");
            msgs.Add($"生命：{plr.statLife}/{plr.statLifeMax}");
            msgs.Add($"魔力：{plr.statMana}/{plr.statManaMax}");
            if (plr.anglerQuestsFinished > 0)
                msgs.Add($"渔夫任务：{plr.anglerQuestsFinished} 次");
            //if (plr.numberOfDeathsPVE > 0 || plr.numberOfDeathsPVP > 0)
            //{
            //    string[] f = new string[] {
            //        plr.numberOfDeathsPVE > 0 ? $"被杀死了{plr.numberOfDeathsPVE}次" : "",
            //        plr.numberOfDeathsPVP > 0 ? $"被其它玩家杀死了{plr.numberOfDeathsPVP}次" : "",
            //    };
            //    op.SendInfoMessage($"死亡统计：{string.Join(", ", f)}");
            //}

            List<string> enhance = new();
            if (plr.extraAccessory) enhance.Add(GetItemDesc(3335, isFlag)); // 3335 恶魔之心
            if (plr.unlockedBiomeTorches) enhance.Add(GetItemDesc(5043, isFlag)); // 5043 火把神徽章
            if (plr.ateArtisanBread) enhance.Add(GetItemDesc(5326, isFlag)); // 5326	工匠面包
            if (plr.usedAegisCrystal) enhance.Add(GetItemDesc(5337, isFlag));    // 5337 生命水晶	永久强化生命再生 
            if (plr.usedAegisFruit) enhance.Add(GetItemDesc(5338, isFlag));  // 5338 埃癸斯果	永久提高防御力 
            if (plr.usedArcaneCrystal) enhance.Add(GetItemDesc(5339, isFlag)); // 5339 奥术水晶	永久提高魔力再生 
            if (plr.usedGalaxyPearl) enhance.Add(GetItemDesc(5340, isFlag)); // 5340	银河珍珠	永久增加运气 
            if (plr.usedGummyWorm) enhance.Add(GetItemDesc(5341, isFlag)); // 5341	黏性蠕虫	永久提高钓鱼技能  
            if (plr.usedAmbrosia) enhance.Add(GetItemDesc(5342, isFlag)); // 5342	珍馐	 永久提高采矿和建造速度 
            if (plr.unlockedSuperCart) enhance.Add(GetItemDesc(5289, isFlag)); // 5289	矿车升级包
            if (enhance.Count != 0) NewWarp(ref msgs, "永久增强：", enhance);

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
                s = GetItemDesc(plr.inventory[i], isFlag);
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
                s = GetItemDesc(plr.armor[i], isFlag);
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
                s = GetItemDesc(plr.dye[i], isFlag);
                if (s != "") dye.Add(s);
            }

            for (int i = 0; i < plr.miscEquips.Length; i++)
            {
                s = GetItemDesc(plr.miscEquips[i]);
                if (s != "") miscEquips.Add(s);
            }

            for (int i = 0; i < plr.miscDyes.Length; i++)
            {
                s = GetItemDesc(plr.miscDyes[i], isFlag);
                if (s != "") miscDyes.Add(s);
            }

            for (int i = 0; i < plr.bank.item.Length; i++)
            {
                s = GetItemDesc(plr.bank.item[i], isFlag);
                if (s != "") bank.Add(s);
            }

            for (int i = 0; i < plr.bank2.item.Length; i++)
            {
                s = GetItemDesc(plr.bank2.item[i], isFlag);
                if (s != "") bank2.Add(s);
            }

            for (int i = 0; i < plr.bank3.item.Length; i++)
            {
                s = GetItemDesc(plr.bank3.item[i], isFlag);
                if (s != "") bank3.Add(s);
            }

            for (int i = 0; i < plr.bank4.item.Length; i++)
            {
                s = GetItemDesc(plr.bank4.item[i], isFlag);
                if (s != "") bank4.Add(s);
            }

            // 装备（loadout）
            for (int i = 0; i < plr.Loadouts.Length; i++)
            {
                Item[] items = plr.Loadouts[i].Armor;
                // 装备 和 时装
                for (int j = 0; j < items.Length; j++)
                {
                    s = GetItemDesc(items[j], isFlag);
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
                    s = GetItemDesc(items[j], isFlag);
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
            s = GetItemDesc(plr.trashItem, isFlag);
            if (s != "") trash.Add(s);

            if (inventory.Count != 0) NewWarp(ref msgs, "背包：", inventory);
            if (trash.Count != 0) NewWarp(ref msgs, "垃圾桶：", trash);
            if (assist.Count != 0) NewWarp(ref msgs, "钱币弹药：", assist);

            int num = plr.CurrentLoadoutIndex + 1;
            if (armor.Count != 0) NewWarp(ref msgs, $">装备{num}：", armor);
            if (vanity.Count != 0) NewWarp(ref msgs, $">时装{num}：", vanity);
            if (dye.Count != 0) NewWarp(ref msgs, $">染料{num}：", dye);

            if (armor1.Count != 0) NewWarp(ref msgs, "装备1：", armor1);
            if (vanity1.Count != 0) NewWarp(ref msgs, "时装1：", vanity1);
            if (dye1.Count != 0) NewWarp(ref msgs, "染料1：", dye1);

            if (armor2.Count != 0) NewWarp(ref msgs, "装备2：", armor2);
            if (vanity2.Count != 0) NewWarp(ref msgs, "时装2：", vanity2);
            if (dye2.Count != 0) NewWarp(ref msgs, "染料2：", dye2);

            if (armor3.Count != 0) NewWarp(ref msgs, "装备3：", armor3);
            if (vanity3.Count != 0) NewWarp(ref msgs, "时装3：", vanity3);
            if (dye3.Count != 0) NewWarp(ref msgs, "染料3：", dye3);

            if (miscEquips.Count != 0) NewWarp(ref msgs, "工具栏：", miscEquips);
            if (miscDyes.Count != 0) NewWarp(ref msgs, "工具栏染料：", miscDyes);
            if (bank.Count != 0) NewWarp(ref msgs, "储蓄罐：", bank);
            if (bank2.Count != 0) NewWarp(ref msgs, "保险箱：", bank2);
            if (bank3.Count != 0) NewWarp(ref msgs, "护卫熔炉：", bank3);
            if (bank4.Count != 0) NewWarp(ref msgs, "虚空保险箱：", bank4);
            if (msgs.Count > 10)
                msgs.Add("*游戏内按↑↓键可滚动查看");
        }

        /// <summary>
        /// 查看玩家信息（离线）
        /// <param name="isFlag">true=使用tag标签进行展示，false=读取物品名进行展示</param>
        /// </summary>
        static void ShowDBPlayer(string username, int userid, bool isFlag, ref List<string> msgs, ref string errMsg)
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
                msgs.Add($"玩家：{username}（已离线）");
                msgs.Add($"生命：{data.health}/{data.maxHealth}");
                msgs.Add($"魔力：{data.mana}/{data.maxMana}");

                if (data.questsCompleted > 0) msgs.Add($"渔夫任务：{data.questsCompleted} 次");
                //if (data.numberOfDeathsPVE > 0 || data.numberOfDeathsPVP > 0)
                //{
                //    string[] f = new string[] {
                //        data.numberOfDeathsPVE > 0 ? $"被杀死了{data.numberOfDeathsPVE}次" : "",
                //        data.numberOfDeathsPVP > 0 ? $"被其它玩家杀死了{data.numberOfDeathsPVP}次" : "",
                //    };
                //    op.SendInfoMessage($"死亡统计：{string.Join(", ", f)}");
                //}

                List<string> enhance = new();
                if (data.extraSlot == 1) enhance.Add(GetItemDesc(3335, isFlag)); // 3335 恶魔之心
                if (data.unlockedBiomeTorches == 1) enhance.Add(GetItemDesc(5043, isFlag)); // 5043 火把神徽章
                if (data.ateArtisanBread == 1) enhance.Add(GetItemDesc(5326, isFlag)); // 5326	工匠面包
                if (data.usedAegisCrystal == 1) enhance.Add(GetItemDesc(5337, isFlag));    // 5337 生命水晶	永久强化生命再生 
                if (data.usedAegisFruit == 1) enhance.Add(GetItemDesc(5338, isFlag));  // 5338 埃癸斯果	永久提高防御力 
                if (data.usedArcaneCrystal == 1) enhance.Add(GetItemDesc(5339, isFlag)); // 5339 奥术水晶	永久提高魔力再生 
                if (data.usedGalaxyPearl == 1) enhance.Add(GetItemDesc(5340, isFlag)); // 5340	银河珍珠	永久增加运气 
                if (data.usedGummyWorm == 1) enhance.Add(GetItemDesc(5341, isFlag)); // 5341	黏性蠕虫	永久提高钓鱼技能  
                if (data.usedAmbrosia == 1) enhance.Add(GetItemDesc(5342, isFlag)); // 5342	珍馐	永久提高采矿和建造速度 
                if (data.unlockedSuperCart == 1) enhance.Add(GetItemDesc(5289, isFlag)); // 5289	矿车升级包
                if (enhance.Count != 0) NewWarp(ref msgs, "永久增强：", enhance);

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
                    s = GetNetItemDesc(data.inventory[i], isFlag);
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

                if (inventory.Count != 0) NewWarp(ref msgs, "背包：", inventory);
                if (trash.Count != 0) NewWarp(ref msgs, "垃圾桶：", trash);
                if (assist.Count != 0) NewWarp(ref msgs, "钱币弹药：", assist);


                int num = data.currentLoadoutIndex + 1;
                if (armor.Count != 0) NewWarp(ref msgs, $">装备{num}：", armor);
                if (vanity.Count != 0) NewWarp(ref msgs, $">时装{num}：", vanity);
                if (dye.Count != 0) NewWarp(ref msgs, $">染料{num}：", dye);

                if (armor1.Count != 0) NewWarp(ref msgs, "装备1：", armor1);
                if (vanity1.Count != 0) NewWarp(ref msgs, "时装1：", vanity1);
                if (dye1.Count != 0) NewWarp(ref msgs, "染料1：", dye1);

                if (armor2.Count != 0) NewWarp(ref msgs, "装备2：", armor2);
                if (vanity2.Count != 0) NewWarp(ref msgs, "时装2：", vanity2);
                if (dye2.Count != 0) NewWarp(ref msgs, "染料2：", dye2);

                if (armor3.Count != 0) NewWarp(ref msgs, "装备3：", armor3);
                if (vanity3.Count != 0) NewWarp(ref msgs, "时装3：", vanity3);
                if (dye3.Count != 0) NewWarp(ref msgs, "染料3：", dye3);

                if (miscEquips.Count != 0) NewWarp(ref msgs, "工具栏：", miscEquips);
                if (miscDyes.Count != 0) NewWarp(ref msgs, "染料2：", miscDyes);
                if (bank.Count != 0) NewWarp(ref msgs, "储蓄罐：", bank);
                if (bank2.Count != 0) NewWarp(ref msgs, "保险箱：", bank2);
                if (bank3.Count != 0) NewWarp(ref msgs, "护卫熔炉：", bank3);
                if (bank4.Count != 0) NewWarp(ref msgs, "虚空保险箱：", bank4);
                if (msgs.Count > 10)
                    msgs.Add("*游戏内按↑↓键可滚动查看");
            }
            else
            {
                errMsg = $"未找到名称中包含 {name} 的玩家.";
            }
        }


        /// <summary>
        /// 添加新的标题行
        /// </summary>
        /// <param name="rawLines"></param>
        /// <param name="header"></param>
        /// <param name="newLines"></param>
        static void NewWarp(ref List<string> rawLines, string header, List<string> newLines)
        {
            var li = Utils.WarpLines(newLines, 10);
            if (li.Any())
            {
                li[0] = $"{header}{li[0]}";
            }
            else
            {
                li.Add(header);
            }
            foreach (var s in li)
            {
                rawLines.Add(s);
            }
        }

        /// <summary>
        /// 获得物品描述
        /// <param name="isFlag">true=使用tag标签进行展示，false=读取物品名进行展示</param>
        /// </summary>
        static string GetItemDesc(Item item, bool isFlag = true)
        {
            if (item.netID == 0)
                return "";

            return GetItemDesc(item.netID, item.Name, item.stack, item.prefix, isFlag);
        }


        /// <summary>
        /// 获得物品描述
        /// </summary>
        /// <param name="isFlag">true=使用tag标签进行展示，false=读取物品名进行展示</param>
        private static string GetNetItemDesc(NetItem netItem, bool isFlag)
        {
            if (netItem.NetId == 0)
                return "";

            string name = !isFlag ? Lang.GetItemNameValue(netItem.NetId) : "";
            return GetItemDesc(netItem.NetId, name, netItem.Stack, netItem.PrefixId, isFlag);
        }

        /// <summary>
        /// 获得物品描述
        /// </summary>
        /// <param name="isFlag">true=使用tag标签进行展示，false=读取物品名进行展示</param>
        /// <returns></returns>
        static string GetItemDesc(int id, string name, int stack, byte prefix, bool isFlag)
        {
            if (isFlag)
            {
                // https://terraria.fandom.com/wiki/Chat
                // [i:29]   数量
                // [i/s10:29]   数量
                // [i/p57:4]    词缀
                // 控制台显示 物品名称
                // 4.4.0 -1.4.1.2   [i:4444]
                // 4.5.0 -1.4.2.2   [女巫扫帚]
                //ChatItemIsIcon = TShock.VersionNum.CompareTo(new Version(4, 5, 0, 0)) >= 0;
                //Console.WriteLine($"ChatItemIsIcon:");
                if (stack > 1)
                {
                    return $"[i/s{stack}:{id}]";
                }
                else
                {
                    if (prefix.Equals(0))
                        return $"[i:{id}]";
                    else
                        return $"[i/p{prefix}:{id}]";
                }
            }
            else
            {
                string s = name;
                string prefixName = TShock.Utils.GetPrefixById(prefix);
                if (prefixName != "")
                    s = $"{prefixName} {s}";

                if (stack > 1)
                    s = $"{s} ({stack})";

                return $"[{s}]";
            }
        }

        /// <summary>
        /// 获得物品描述
        /// </summary>
        /// <param name="isFlag">true=使用tag标签进行展示，false=读取物品名进行展示</param>
        static string GetItemDesc(int id, bool isFlag = true)
        {
            if (isFlag)
                return $"[i:{id}]";
            else
                return $"[{Lang.GetItemNameValue(id)}]";
        }


        //public static void SendMultipleMessage(TSPlayer op, string header, List<string> matches)
        //{
        //    // if( ChatItemIsIcon ){
        //    //if (matches.Count <= 10)
        //    //{
        //    //    matches[0] = header + matches[0];
        //    //}
        //    //else
        //    //{
        //    //    op.SendInfoMessage(header);
        //    //}
        //    matches[0] = header + matches[0];
        //    // 一行显示10个物品
        //    var s = "";
        //    var count = 0;
        //    for (int i = 0; i < matches.Count; i++)
        //    {
        //        s += matches[i] + " ";
        //        count++;
        //        if (count >= 10)
        //        {
        //            op.SendInfoMessage(s);
        //            count = 0;
        //            s = "";
        //        }
        //    }
        //    if (s != "")
        //    {
        //        op.SendInfoMessage(s);
        //    }

        //    // } else {
        //    //     matches[0] = header + matches[0];
        //    //     var lines = PaginationTools.BuildLinesFromTerms(matches.ToArray());
        //    //     lines.ForEach(op.SendInfoMessage);
        //    // }
        //}

    }
}