using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.IO;
using TerrariaApi.Server;
using TShockAPI;


namespace Plugin
{
    [ApiVersion(2, 1)]
    public class Plugin : TerrariaPlugin
    {
        public override string Author => "hufang360";

        public override string Description => "角色管理";

        public override string Name => "PlayerManage";

        public override Version Version => Assembly.GetExecutingAssembly().GetName().Version;

        public Plugin(Main game) : base(game)
        {
        }

        public override void Initialize()
        {
            Commands.ChatCommands.Add(new Command(new List<string>() { "hf.playermanage" }, PlayerManage, "playermanage", "pm") { HelpText = "角色管理" });
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
                // 帮助
                case "help":
                    args.Player.SendInfoMessage("/pm export [玩家名], 导出所有玩家的在线存档，可指定只导出某一个玩家");
                    args.Player.SendInfoMessage("/pm look <玩家名>, 查看某一个玩家的背包信息");
                    return;

                default:
                    args.Player.SendErrorMessage("语法不正确！");
                    break;

                // 查看玩家背包
                case "look":
                    LookPlayer(args);
                    break;

                // 导出
                case "export":
                    ExportPlayer(args);
                    break;
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

                // if (inventory.Count != 0){
                //     args.Player.SendInfoMessage("背包：");
                //     args.Player.SendMultipleMatchError(inventory);
                // }
                // if (assist.Count != 0) args.Player.SendInfoMessage("钱币、弹药：{0}", String.Join(", ", assist));
                // if (trashDesc != "") args.Player.SendInfoMessage("垃圾桶：{0}", trashDesc);
                // if (armor.Count != 0) args.Player.SendInfoMessage("装备栏：{0}", String.Join(", ", armor));
                // if (vanity.Count != 0) args.Player.SendInfoMessage("社交栏：{0}", String.Join(", ", vanity));
                // if (dye.Count != 0) args.Player.SendInfoMessage("染料1：{0}", String.Join(", ", dye));
                // if (miscEquips.Count != 0) args.Player.SendInfoMessage("工具栏：{0}", String.Join(", ", miscEquips));
                // if (miscDyes.Count != 0) args.Player.SendInfoMessage("染料2：{0}", String.Join(", ", miscDyes));
                // if (bank.Count != 0) args.Player.SendInfoMessage("储蓄罐：{0}", String.Join(", ", bank));
                // if (bank2.Count != 0) args.Player.SendInfoMessage("保险箱：{0}", String.Join(", ", bank2));
                // if (bank3.Count != 0) args.Player.SendInfoMessage("护卫熔炉：{0}", String.Join(", ", bank3));
                // if (bank4.Count != 0) args.Player.SendInfoMessage("虚空保险箱：{0}", String.Join(", ", bank4));

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
                return "不存在";
            }
            else if (matchedItems.Count > 1)
            {
                // args.Player.SendMultipleMatchError(matchedItems.Select(i => $"{i.Name}({i.netID})"));
                return "找到多个";
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


        private async Task ExportPlayer(CommandArgs args)
        {
            await Task.Run(() =>
            {
                // 导出全部
                if (args.Parameters.Count<string>() == 1)
                {
                    int successcount = 0;
                    int faildcount = 0;
                    var savedlist = new List<string>();
                    TShock.Players.Where(p => p != null && p.SaveServerCharacter()).ForEach(plr =>
                    {
                        savedlist.Add(plr.Name);
                        if (Export(plr.TPlayer).Result) { args.Player.SendSuccessMessage($"已导出 {plr.Name} 的在线存档."); successcount++; }
                        else { args.Player.SendErrorMessage($"导出 {plr.Name} 的在线存档时发生错误."); faildcount++; }
                    });
                    var allaccount = TShock.UserAccounts.GetUserAccounts();
                    allaccount.Where(acc => acc != null && !savedlist.Contains(acc.Name)).ForEach(acc =>
                    {
                        var data = TShock.CharacterDB.GetPlayerData(new TSPlayer(-1), acc.ID);
                        if (data != null)
                        {
                            if (data.hideVisuals != null)
                            {
                                if (Export(ModifyData(acc.Name, data)).Result) { args.Player.SendSuccessMessage($"已导出 {acc.Name} 的存档."); successcount++; }
                                else { args.Player.SendErrorMessage($"导出 {acc.Name} 的存档时发生错误."); faildcount++; }
                            }
                            else args.Player.SendInfoMessage($"玩家 {acc.Name} 的数据不完整, 已跳过.");
                        }
                    });
                    args.Player.SendInfoMessage($"操作完成. 成功: {successcount}, 失败: {faildcount}.");

                    return;
                }


                // 导出单个
                var name = args.Parameters[1];
                var list = TSPlayer.FindByNameOrID(name);
                string path = Path.Combine(TShock.SavePath, "PlayerExport", name + ".plr");
                if (list.Count > 1) args.Player.SendMultipleMatchError(list);
                else if (list.Any())
                {
                    if (Export(list[0].TPlayer).Result) args.Player.SendSuccessMessage($"已导出玩家 {list[0].Name} 的存档至 {path}.");
                    else args.Player.SendErrorMessage($"导出失败.");
                }
                else
                {
                    var offlinelist = TShock.UserAccounts.GetUserAccountsByName(name);
                    if (offlinelist.Count > 1) args.Player.SendMultipleMatchError(offlinelist);
                    else if (offlinelist.Any())
                    {
                        name = offlinelist[0].Name;
                        args.Player.SendInfoMessage($"玩家 {name} 未在线, 将导出离线存档...");
                        var data = TShock.CharacterDB.GetPlayerData(new TSPlayer(-1), offlinelist[0].ID);
                        if (data != null)
                        {
                            if (data.hideVisuals == null)
                            {
                                args.Player.SendErrorMessage($"玩家 {name} 的数据不完整, 无法导出.");
                                return;
                            }
                            if (Export(ModifyData(name, data)).Result) args.Player.SendSuccessMessage($"已导出玩家 {name} 的存档至 {path}.");
                            else args.Player.SendErrorMessage($"导出失败.");
                        }
                        else
                        {
                            args.Player.SendErrorMessage($"未能从数据库中获取到玩家数据.");
                        }
                    }
                    else
                    {
                        args.Player.SendErrorMessage($"未找到名称中包含 {name} 的玩家.");
                    }
                }


            });
        }

        public static Player ModifyData(string name, PlayerData data)
        {
            var plr = new Player();
            if (data != null)
            {
                plr.name = name;
                plr.statLife = data.health;
                plr.statLifeMax = data.maxHealth;
                plr.statMana = data.mana;
                plr.statManaMax = data.maxMana;
                plr.SpawnX = data.spawnX;
                plr.SpawnY = data.spawnY;
                plr.skinVariant = data.skinVariant ?? default;
                plr.hair = data.hair ?? default;
                plr.hairDye = data.hairDye;
                plr.hairColor = data.hairColor ?? default;
                plr.pantsColor = data.pantsColor ?? default;
                plr.underShirtColor = data.underShirtColor ?? default;
                plr.shoeColor = data.shoeColor ?? default;
                plr.hideVisibleAccessory = data.hideVisuals;
                plr.skinColor = data.skinColor ?? default;
                plr.eyeColor = data.eyeColor ?? default;
                for (int i = 0; i < 260; i++)
                {
                    //  0~49 背包   5*10
                    //  50、51、52、53 钱
                    //  54、55、56、57 弹药
                    // 59 ~68  饰品栏
                    // 69 ~78  社交栏
                    // 79 ~88  染料1
                    // 89 ~93  宠物、照明、矿车、坐骑、钩爪
                    // 94 ~98  染料2
                    // 99~138 储蓄罐
                    // 139~178 保险箱（商人）
                    // 179 垃圾桶
                    // 180~219 护卫熔炉
                    // 220~259 虚空保险箱
                    if (i < 59) plr.inventory[i] = NetItem2Item(data.inventory[i]);
                    else if (i >= 59 && i < 79) plr.armor[i - 59] = NetItem2Item(data.inventory[i]);
                    else if (i >= 79 && i < 89) plr.dye[i - 79] = NetItem2Item(data.inventory[i]);
                    else if (i >= 89 && i < 94) plr.miscEquips[i - 89] = NetItem2Item(data.inventory[i]);
                    else if (i >= 94 && i < 99) plr.miscDyes[i - 94] = NetItem2Item(data.inventory[i]);
                    else if (i >= 99 && i < 139) plr.bank.item[i - 99] = NetItem2Item(data.inventory[i]);
                    else if (i >= 139 && i < 179) plr.bank2.item[i - 139] = NetItem2Item(data.inventory[i]);
                    else if (i == 179) plr.trashItem = NetItem2Item(data.inventory[i]);
                    else if (i >= 180 && i < 220) plr.bank3.item[i - 180] = NetItem2Item(data.inventory[i]);
                    else if (i >= 220 && i < 260) plr.bank4.item[i - 220] = NetItem2Item(data.inventory[i]);
                }
            }
            return plr;
        }
        public static Item NetItem2Item(NetItem item)
        {
            var i = new Item();
            i.SetDefaults(item.NetId);
            i.stack = item.Stack;
            i.prefix = item.PrefixId;
            return i;
        }


        public static async Task<bool> Export(Player plr)
        {
            return await Task.Run(() =>
            {
                string path = Path.Combine(TShock.SavePath, "PlayerExport", plr.name + ".plr");
                try
                {
                    string folder = Path.Combine(TShock.SavePath, "PlayerExport").ToString();
                    if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                    if (File.Exists(path))
                    {
                        File.Copy(path, path + ".bak", true);
                    }
                    RijndaelManaged rijndaelManaged = new RijndaelManaged();
                    using (Stream stream = new FileStream(path, FileMode.Create))
                    {
                        using (CryptoStream cryptoStream = new CryptoStream(stream, rijndaelManaged.CreateEncryptor(Player.ENCRYPTION_KEY, Player.ENCRYPTION_KEY), CryptoStreamMode.Write))
                        {
                            PlayerFileData playerFileData = new PlayerFileData
                            {
                                Metadata = FileMetadata.FromCurrentSettings(FileType.Player),
                                Player = plr,
                                _isCloudSave = false,
                                _path = path
                            };
                            Main.LocalFavoriteData.ClearEntry(playerFileData);
                            using (BinaryWriter binaryWriter = new BinaryWriter(cryptoStream))
                            {
                                binaryWriter.Write(230);
                                playerFileData.Metadata.Write(binaryWriter);
                                binaryWriter.Write(plr.name);
                                binaryWriter.Write(plr.difficulty);
                                binaryWriter.Write(playerFileData.GetPlayTime().Ticks);
                                binaryWriter.Write(plr.hair);
                                binaryWriter.Write(plr.hairDye);
                                BitsByte bb = 0;
                                for (int i = 0; i < 8; i++)
                                {
                                    bb[i] = plr.hideVisibleAccessory[i];
                                }
                                binaryWriter.Write(bb);
                                bb = 0;
                                for (int j = 0; j < 2; j++)
                                {
                                    bb[j] = plr.hideVisibleAccessory[j + 8];
                                }
                                binaryWriter.Write(bb);
                                binaryWriter.Write(plr.hideMisc);
                                binaryWriter.Write((byte)plr.skinVariant);
                                binaryWriter.Write(plr.statLife);
                                binaryWriter.Write(plr.statLifeMax);
                                binaryWriter.Write(plr.statMana);
                                binaryWriter.Write(plr.statManaMax);
                                binaryWriter.Write(plr.extraAccessory);
                                binaryWriter.Write(plr.unlockedBiomeTorches);
                                binaryWriter.Write(plr.UsingBiomeTorches);
                                binaryWriter.Write(plr.downedDD2EventAnyDifficulty);
                                binaryWriter.Write(plr.taxMoney);
                                binaryWriter.Write(plr.hairColor.R);
                                binaryWriter.Write(plr.hairColor.G);
                                binaryWriter.Write(plr.hairColor.B);
                                binaryWriter.Write(plr.skinColor.R);
                                binaryWriter.Write(plr.skinColor.G);
                                binaryWriter.Write(plr.skinColor.B);
                                binaryWriter.Write(plr.eyeColor.R);
                                binaryWriter.Write(plr.eyeColor.G);
                                binaryWriter.Write(plr.eyeColor.B);
                                binaryWriter.Write(plr.shirtColor.R);
                                binaryWriter.Write(plr.shirtColor.G);
                                binaryWriter.Write(plr.shirtColor.B);
                                binaryWriter.Write(plr.underShirtColor.R);
                                binaryWriter.Write(plr.underShirtColor.G);
                                binaryWriter.Write(plr.underShirtColor.B);
                                binaryWriter.Write(plr.pantsColor.R);
                                binaryWriter.Write(plr.pantsColor.G);
                                binaryWriter.Write(plr.pantsColor.B);
                                binaryWriter.Write(plr.shoeColor.R);
                                binaryWriter.Write(plr.shoeColor.G);
                                binaryWriter.Write(plr.shoeColor.B);
                                for (int k = 0; k < plr.armor.Length; k++)
                                {
                                    binaryWriter.Write(plr.armor[k].netID);
                                    binaryWriter.Write(plr.armor[k].prefix);
                                }
                                for (int l = 0; l < plr.dye.Length; l++)
                                {
                                    binaryWriter.Write(plr.dye[l].netID);
                                    binaryWriter.Write(plr.dye[l].prefix);
                                }
                                for (int m = 0; m < 58; m++)
                                {
                                    binaryWriter.Write(plr.inventory[m].netID);
                                    binaryWriter.Write(plr.inventory[m].stack);
                                    binaryWriter.Write(plr.inventory[m].prefix);
                                    binaryWriter.Write(plr.inventory[m].favorited);
                                }
                                for (int n = 0; n < plr.miscEquips.Length; n++)
                                {
                                    binaryWriter.Write(plr.miscEquips[n].netID);
                                    binaryWriter.Write(plr.miscEquips[n].prefix);
                                    binaryWriter.Write(plr.miscDyes[n].netID);
                                    binaryWriter.Write(plr.miscDyes[n].prefix);
                                }
                                for (int num = 0; num < 40; num++)
                                {
                                    binaryWriter.Write(plr.bank.item[num].netID);
                                    binaryWriter.Write(plr.bank.item[num].stack);
                                    binaryWriter.Write(plr.bank.item[num].prefix);
                                }
                                for (int num2 = 0; num2 < 40; num2++)
                                {
                                    binaryWriter.Write(plr.bank2.item[num2].netID);
                                    binaryWriter.Write(plr.bank2.item[num2].stack);
                                    binaryWriter.Write(plr.bank2.item[num2].prefix);
                                }
                                for (int num3 = 0; num3 < 40; num3++)
                                {
                                    binaryWriter.Write(plr.bank3.item[num3].netID);
                                    binaryWriter.Write(plr.bank3.item[num3].stack);
                                    binaryWriter.Write(plr.bank3.item[num3].prefix);
                                }
                                for (int num4 = 0; num4 < 40; num4++)
                                {
                                    binaryWriter.Write(plr.bank4.item[num4].netID);
                                    binaryWriter.Write(plr.bank4.item[num4].stack);
                                    binaryWriter.Write(plr.bank4.item[num4].prefix);
                                }
                                binaryWriter.Write(plr.voidVaultInfo);
                                for (int num5 = 0; num5 < 22; num5++)
                                {
                                    if (Main.buffNoSave[plr.buffType[num5]])
                                    {
                                        binaryWriter.Write(0);
                                        binaryWriter.Write(0);
                                    }
                                    else
                                    {
                                        binaryWriter.Write(plr.buffType[num5]);
                                        binaryWriter.Write(plr.buffTime[num5]);
                                    }
                                }
                                for (int num6 = 0; num6 < 200; num6++)
                                {
                                    if (plr.spN[num6] == null)
                                    {
                                        binaryWriter.Write(-1);
                                        break;
                                    }
                                    binaryWriter.Write(plr.spX[num6]);
                                    binaryWriter.Write(plr.spY[num6]);
                                    binaryWriter.Write(plr.spI[num6]);
                                    binaryWriter.Write(plr.spN[num6]);
                                }
                                binaryWriter.Write(plr.hbLocked);
                                for (int num7 = 0; num7 < plr.hideInfo.Length; num7++)
                                {
                                    binaryWriter.Write(plr.hideInfo[num7]);
                                }
                                binaryWriter.Write(plr.anglerQuestsFinished);
                                for (int num8 = 0; num8 < plr.DpadRadial.Bindings.Length; num8++)
                                {
                                    binaryWriter.Write(plr.DpadRadial.Bindings[num8]);
                                }
                                for (int num9 = 0; num9 < plr.builderAccStatus.Length; num9++)
                                {
                                    binaryWriter.Write(plr.builderAccStatus[num9]);
                                }
                                binaryWriter.Write(plr.bartenderQuestLog);
                                binaryWriter.Write(plr.dead);
                                if (plr.dead)
                                {
                                    binaryWriter.Write(plr.respawnTimer);
                                }
                                long value = DateTime.UtcNow.ToBinary();
                                binaryWriter.Write(value);
                                binaryWriter.Write(plr.golferScoreAccumulated);
                                plr.creativeTracker.Save(binaryWriter);
                                plr.SaveTemporaryItemSlotContents(binaryWriter);
                                CreativePowerManager.Instance.SaveToPlayer(plr, binaryWriter);
                                binaryWriter.Flush();
                                cryptoStream.FlushFinalBlock();
                                stream.Flush();

                            }
                        }
                    }
                    return true;
                }
                catch (Exception ex) { File.Delete(path); TShock.Log.ConsoleError(ex.Message); }
                return false;
            });
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
            base.Dispose(disposing);
        }
    }
}
