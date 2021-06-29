using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.IO;
using TShockAPI;

namespace Plugin
{
    public class ExportPlayer
    {
        private static readonly string save_dir = Path.Combine(TShock.SavePath, "PlayerManager");

        public static async Task Export(CommandArgs args)
        {
            await Task.Run(() =>
            {
                if (args.Parameters.Count<string>() == 1)
                {
                    // 导出全部
                    args.Player.SendInfoMessage("语法错误，请输入玩家名！");
                }
                else
                {
                    // 导出单个
                    var name = args.Parameters[1];
                    var list = TSPlayer.FindByNameOrID(name);
                    string path = Path.Combine(save_dir, name + ".plr");
                    if (list.Count > 1) args.Player.SendMultipleMatchError(list);
                    else if (list.Any())
                    {
                        if (ExportOne(list[0].TPlayer).Result) args.Player.SendSuccessMessage($"已导出玩家 {list[0].Name} 的存档至 {path}.");
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
                                if (ExportOne(ModifyData(name, data)).Result) args.Player.SendSuccessMessage($"已导出玩家 {name} 的存档至 {path}.");
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
                }
            });
        }

         public static async Task ExportAll(CommandArgs args)
        {
            await Task.Run(() =>
            {
                    int successcount = 0;
                    int faildcount = 0;
                    var savedlist = new List<string>();
                    TShock.Players.Where(p => p != null && p.SaveServerCharacter()).ForEach(plr =>
                    {
                        savedlist.Add(plr.Name);
                        if (ExportOne(plr.TPlayer).Result) { args.Player.SendSuccessMessage($"已导出 {plr.Name} 的在线存档."); successcount++; }
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
                                if (ExportOne(ModifyData(acc.Name, data)).Result) { args.Player.SendSuccessMessage($"已导出 {acc.Name} 的存档."); successcount++; }
                                else { args.Player.SendErrorMessage($"导出 {acc.Name} 的存档时发生错误."); faildcount++; }
                            }
                            else args.Player.SendInfoMessage($"玩家 {acc.Name} 的数据不完整, 已跳过.");
                        }
                    });
                    args.Player.SendInfoMessage($"操作完成. 成功: {successcount}, 失败: {faildcount}.");
            });
        }


        private static async Task<bool> ExportOne(Player plr)
        {
            return await Task.Run(() =>
            {
                string path = Path.Combine(save_dir, plr.name + ".plr");
                try
                {
                    string folder = save_dir;
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


        private static Player ModifyData(string name, PlayerData data)
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


    }
}
