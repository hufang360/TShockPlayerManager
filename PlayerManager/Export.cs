using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.IO;
using TShockAPI;


namespace Plugin
{
    public class ExportPlayer
    {
        private static readonly string save_dir = Path.Combine(TShock.SavePath, "PlayerManager");

        private static string GenTimeDir()
        {
            DateTime dt = DateTime.Now;
            string timeStr = string.Format("{0:yyyy-MMdd-HHmm-ss}", dt);
            string plr_dir = Path.Combine(save_dir, timeStr);

            if (!Directory.Exists(plr_dir))
                Directory.CreateDirectory(plr_dir);

            return plr_dir;
        }

        public static async Task Export(TSPlayer op, string name)
        {
            await Task.Run(() =>
            {
                var list = TSPlayer.FindByNameOrID(name);
                string path = Path.Combine(GenTimeDir(), name + ".plr");

                if (list.Count > 1)
                {
                    op.SendMultipleMatchError(list);

                }
                else if (list.Any())
                {
                    if (ExportOne(list[0].TPlayer, path).Result)
                        op.SendSuccessMessage($"已导出玩家 {list[0].Name} 的存档至 {path}.");
                    else
                        op.SendErrorMessage($"导出失败.");

                }
                else
                {
                    var offlinelist = TShock.UserAccounts.GetUserAccountsByName(name);
                    if (offlinelist.Count > 1)
                    {
                        op.SendMultipleMatchError(offlinelist);

                    }
                    else if (offlinelist.Any())
                    {
                        name = offlinelist[0].Name;
                        op.SendInfoMessage($"玩家 {name} 未在线, 将导出离线存档...");
                        var data = TShock.CharacterDB.GetPlayerData(new TSPlayer(-1), offlinelist[0].ID);
                        if (data != null)
                        {
                            if (data.hideVisuals == null)
                            {
                                op.SendErrorMessage($"玩家 {name} 的数据不完整, 无法导出.");
                                return;
                            }
                            if (ExportOne(ModifyData(name, data), path).Result)
                                op.SendSuccessMessage($"已导出玩家 {name} 的存档至 {path}.");
                            else
                                op.SendErrorMessage($"导出失败.");

                        }
                        else
                        {
                            op.SendErrorMessage($"未能从数据库中获取到玩家数据.");
                        }

                    }
                    else
                    {
                        op.SendErrorMessage($"未找到名称中包含 {name} 的玩家.");
                    }
                }

            });
        }

        public static async Task ExportAll(TSPlayer op)
        {
            await Task.Run(() =>
            {
                int successcount = 0;
                int faildcount = 0;
                string plr_dir = GenTimeDir();

                // 在线存档
                var savedlist = new List<string>();
                TShock.Players.Where(p => p != null && p.SaveServerCharacter()).ForEach(plr =>
                {
                    savedlist.Add(plr.Name);
                    string path1 = Path.Combine(plr_dir, plr.Name + ".plr");
                    if (ExportOne(plr.TPlayer, path1).Result)
                    {
                        op.SendSuccessMessage($"已导出 {plr.Name} 的在线存档.");
                        successcount++;
                    }
                    else
                    {
                        op.SendErrorMessage($"导出 {plr.Name} 的在线存档时发生错误.");
                        faildcount++;
                    }
                });

                // 离线存档
                var allaccount = TShock.UserAccounts.GetUserAccounts();
                allaccount.Where(acc => acc != null && !savedlist.Contains(acc.Name)).ForEach(acc =>
                {
                    var data = TShock.CharacterDB.GetPlayerData(new TSPlayer(-1), acc.ID);
                    if (data != null)
                    {
                        if (data.hideVisuals != null)
                        {
                            string path2 = Path.Combine(plr_dir, acc.Name + ".plr");
                            if (ExportOne(ModifyData(acc.Name, data), path2).Result)
                            {
                                op.SendSuccessMessage($"已导出 {acc.Name} 的存档.");
                                successcount++;
                            }
                            else
                            {
                                op.SendErrorMessage($"导出 {acc.Name} 的存档时发生错误.");
                                faildcount++;
                            }
                        }
                        else
                        {
                            op.SendInfoMessage($"玩家 {acc.Name} 的数据不完整, 已跳过.");
                        }
                    }
                });
                op.SendInfoMessage($"操作完成. 成功: {successcount}, 失败: {faildcount}.");

            });
        }


        private static async Task<bool> ExportOne(Player player, string path)
        {
            return await Task.Run(() =>
            {
                try
                {
                    RijndaelManaged rijndaelManaged = new RijndaelManaged();
                    using (Stream stream = new FileStream(path, FileMode.Create))
                    {
                        using (CryptoStream cryptoStream = new CryptoStream(stream, rijndaelManaged.CreateEncryptor(Player.ENCRYPTION_KEY, Player.ENCRYPTION_KEY), CryptoStreamMode.Write))
                        {
                            PlayerFileData playerFileData = new PlayerFileData
                            {
                                Metadata = FileMetadata.FromCurrentSettings(FileType.Player),
                                Player = player,
                                _isCloudSave = false,
                                _path = path
                            };
                            Main.LocalFavoriteData.ClearEntry(playerFileData);
                            using (BinaryWriter binaryWriter = new BinaryWriter(cryptoStream))
                            {
                                binaryWriter.Write(230);
                                playerFileData.Metadata.Write(binaryWriter);
                                binaryWriter.Write(player.name);
                                binaryWriter.Write(player.difficulty);
                                binaryWriter.Write(playerFileData.GetPlayTime().Ticks);
                                binaryWriter.Write(player.hair);
                                binaryWriter.Write(player.hairDye);
                                BitsByte bitsByte = 0;
                                for (int i = 0; i < 8; i++)
                                {
                                    bitsByte[i] = player.hideVisibleAccessory[i];
                                }
                                binaryWriter.Write(bitsByte);
                                bitsByte = 0;
                                for (int j = 0; j < 2; j++)
                                {
                                    bitsByte[j] = player.hideVisibleAccessory[j + 8];
                                }
                                binaryWriter.Write(bitsByte);
                                binaryWriter.Write(player.hideMisc);
                                binaryWriter.Write((byte)player.skinVariant);
                                binaryWriter.Write(player.statLife);
                                binaryWriter.Write(player.statLifeMax);
                                binaryWriter.Write(player.statMana);
                                binaryWriter.Write(player.statManaMax);
                                binaryWriter.Write(player.extraAccessory);
                                binaryWriter.Write(player.unlockedBiomeTorches);
                                binaryWriter.Write(player.UsingBiomeTorches);
                                binaryWriter.Write(player.downedDD2EventAnyDifficulty);
                                binaryWriter.Write(player.taxMoney);
                                binaryWriter.Write(player.hairColor.R);
                                binaryWriter.Write(player.hairColor.G);
                                binaryWriter.Write(player.hairColor.B);
                                binaryWriter.Write(player.skinColor.R);
                                binaryWriter.Write(player.skinColor.G);
                                binaryWriter.Write(player.skinColor.B);
                                binaryWriter.Write(player.eyeColor.R);
                                binaryWriter.Write(player.eyeColor.G);
                                binaryWriter.Write(player.eyeColor.B);
                                binaryWriter.Write(player.shirtColor.R);
                                binaryWriter.Write(player.shirtColor.G);
                                binaryWriter.Write(player.shirtColor.B);
                                binaryWriter.Write(player.underShirtColor.R);
                                binaryWriter.Write(player.underShirtColor.G);
                                binaryWriter.Write(player.underShirtColor.B);
                                binaryWriter.Write(player.pantsColor.R);
                                binaryWriter.Write(player.pantsColor.G);
                                binaryWriter.Write(player.pantsColor.B);
                                binaryWriter.Write(player.shoeColor.R);
                                binaryWriter.Write(player.shoeColor.G);
                                binaryWriter.Write(player.shoeColor.B);
                                for (int k = 0; k < player.armor.Length; k++)
                                {
                                    binaryWriter.Write(player.armor[k].netID);
                                    binaryWriter.Write(player.armor[k].prefix);
                                }
                                for (int l = 0; l < player.dye.Length; l++)
                                {
                                    binaryWriter.Write(player.dye[l].netID);
                                    binaryWriter.Write(player.dye[l].prefix);
                                }
                                for (int m = 0; m < 58; m++)
                                {
                                    binaryWriter.Write(player.inventory[m].netID);
                                    binaryWriter.Write(player.inventory[m].stack);
                                    binaryWriter.Write(player.inventory[m].prefix);
                                    binaryWriter.Write(player.inventory[m].favorited);
                                }
                                for (int n = 0; n < player.miscEquips.Length; n++)
                                {
                                    binaryWriter.Write(player.miscEquips[n].netID);
                                    binaryWriter.Write(player.miscEquips[n].prefix);
                                    binaryWriter.Write(player.miscDyes[n].netID);
                                    binaryWriter.Write(player.miscDyes[n].prefix);
                                }
                                for (int num = 0; num < 40; num++)
                                {
                                    binaryWriter.Write(player.bank.item[num].netID);
                                    binaryWriter.Write(player.bank.item[num].stack);
                                    binaryWriter.Write(player.bank.item[num].prefix);
                                }
                                for (int num2 = 0; num2 < 40; num2++)
                                {
                                    binaryWriter.Write(player.bank2.item[num2].netID);
                                    binaryWriter.Write(player.bank2.item[num2].stack);
                                    binaryWriter.Write(player.bank2.item[num2].prefix);
                                }
                                for (int num3 = 0; num3 < 40; num3++)
                                {
                                    binaryWriter.Write(player.bank3.item[num3].netID);
                                    binaryWriter.Write(player.bank3.item[num3].stack);
                                    binaryWriter.Write(player.bank3.item[num3].prefix);
                                }
                                for (int num4 = 0; num4 < 40; num4++)
                                {
                                    binaryWriter.Write(player.bank4.item[num4].netID);
                                    binaryWriter.Write(player.bank4.item[num4].stack);
                                    binaryWriter.Write(player.bank4.item[num4].prefix);
                                }
                                binaryWriter.Write(player.voidVaultInfo);
                                for (int num5 = 0; num5 < 22; num5++)
                                {
                                    if (Main.buffNoSave[player.buffType[num5]])
                                    {
                                        binaryWriter.Write(0);
                                        binaryWriter.Write(0);
                                    }
                                    else
                                    {
                                        binaryWriter.Write(player.buffType[num5]);
                                        binaryWriter.Write(player.buffTime[num5]);
                                    }
                                }
                                for (int num6 = 0; num6 < 200; num6++)
                                {
                                    if (player.spN[num6] == null)
                                    {
                                        binaryWriter.Write(-1);
                                        break;
                                    }
                                    binaryWriter.Write(player.spX[num6]);
                                    binaryWriter.Write(player.spY[num6]);
                                    binaryWriter.Write(player.spI[num6]);
                                    binaryWriter.Write(player.spN[num6]);
                                }
                                binaryWriter.Write(player.hbLocked);
                                for (int num7 = 0; num7 < player.hideInfo.Length; num7++)
                                {
                                    binaryWriter.Write(player.hideInfo[num7]);
                                }
                                binaryWriter.Write(player.anglerQuestsFinished);
                                for (int num8 = 0; num8 < player.DpadRadial.Bindings.Length; num8++)
                                {
                                    binaryWriter.Write(player.DpadRadial.Bindings[num8]);
                                }
                                for (int num9 = 0; num9 < player.builderAccStatus.Length; num9++)
                                {
                                    binaryWriter.Write(player.builderAccStatus[num9]);
                                }
                                binaryWriter.Write(player.bartenderQuestLog);
                                binaryWriter.Write(player.dead);
                                if (player.dead)
                                {
                                    binaryWriter.Write(player.respawnTimer);
                                }
                                long value = DateTime.UtcNow.ToBinary();
                                binaryWriter.Write(value);
                                binaryWriter.Write(player.golferScoreAccumulated);
                                SaveSacrifice(binaryWriter);
                                // player.creativeTracker.Save(binaryWriter);
                                player.SaveTemporaryItemSlotContents(binaryWriter);
                                CreativePowerManager.Instance.SaveToPlayer(player, binaryWriter);
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

        public static void SaveSacrifice(BinaryWriter writer)
        {

            Dictionary<int, int> dictionary = TShock.ResearchDatastore.GetSacrificedItems();
            writer.Write(dictionary.Count);
            foreach (KeyValuePair<int, int> item in dictionary)
            {
                writer.Write(ContentSamples.ItemPersistentIdsByNetIds[item.Key]);
                writer.Write(item.Value);
            }
        }


        private static Player ModifyData(string name, PlayerData data)
        {
            Player player = new Player();
            if (data != null)
            {
                player.name = name;
                player.SpawnX = data.spawnX;
                player.SpawnY = data.spawnY;

                player.hideVisibleAccessory = data.hideVisuals;
                player.skinVariant = data.skinVariant ?? default;
                player.statLife = data.health;
                player.statLifeMax = data.maxHealth;
                player.statMana = data.mana;
                player.statManaMax = data.maxMana;
                player.extraAccessory = data.extraSlot == 1;

                player.difficulty = (byte)Main.GameModeInfo.Id;

                // 火把神
                player.unlockedBiomeTorches = data.unlockedBiomeTorches == 1;

                player.hairColor = data.hairColor ?? default;
                player.skinColor = data.skinColor ?? default;
                player.eyeColor = data.eyeColor ?? default;
                player.shirtColor = data.shirtColor ?? default;
                player.underShirtColor = data.underShirtColor ?? default;
                player.pantsColor = data.pantsColor ?? default;
                player.shoeColor = data.shoeColor ?? default;

                player.hair = data.hair ?? default;
                player.hairDye = data.hairDye;

                player.anglerQuestsFinished = data.questsCompleted;

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
                    if (i < 59) player.inventory[i] = NetItem2Item(data.inventory[i]);
                    else if (i >= 59 && i < 79) player.armor[i - 59] = NetItem2Item(data.inventory[i]);
                    else if (i >= 79 && i < 89) player.dye[i - 79] = NetItem2Item(data.inventory[i]);
                    else if (i >= 89 && i < 94) player.miscEquips[i - 89] = NetItem2Item(data.inventory[i]);
                    else if (i >= 94 && i < 99) player.miscDyes[i - 94] = NetItem2Item(data.inventory[i]);
                    else if (i >= 99 && i < 139) player.bank.item[i - 99] = NetItem2Item(data.inventory[i]);
                    else if (i >= 139 && i < 179) player.bank2.item[i - 139] = NetItem2Item(data.inventory[i]);
                    else if (i == 179) player.trashItem = NetItem2Item(data.inventory[i]);
                    else if (i >= 180 && i < 220) player.bank3.item[i - 180] = NetItem2Item(data.inventory[i]);
                    else if (i >= 220 && i < 260) player.bank4.item[i - 220] = NetItem2Item(data.inventory[i]);
                }
            }
            return player;
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
