using Rests;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Terraria;
using TShockAPI;

namespace PlayerManager
{
    /// <summary>
    /// RestApi
    /// </summary>
    public class Rest
    {
        #region /pm/look
        [Route("/pm/look")]
        [Description("查看玩家")]
        [Permission("playermanager")]
        [Noun("name", true, "玩家名", typeof(string))]
        [Token]
        public static object Look(RestRequestArgs args)
        {
            if (string.IsNullOrWhiteSpace(args.Parameters["name"]))
                return RestMissingParam("name");

            string name = args.Parameters["name"];
            FoundPlayer found = Utils.GetPlayer(name, out string errorStr);
            if (!found.valid)
                return RestError(errorStr);

            if (found.online)
            {
                // 在线
                Player plr = found.plr.TPlayer;

                #region 背包
                NetItem[] invs = new NetItem[NetItem.MaxInventory];
                Item[] inventory = plr.inventory;
                Item[] armor = plr.armor;
                Item[] dye = plr.dye;
                Item[] miscEqups = plr.miscEquips;
                Item[] miscDyes = plr.miscDyes;
                Item[] piggy = plr.bank.item;
                Item[] safe = plr.bank2.item;
                Item[] forge = plr.bank3.item;
                Item[] voidVault = plr.bank4.item;
                Item trash = plr.trashItem;
                Item[] loadout1Armor = plr.Loadouts[0].Armor;
                Item[] loadout1Dye = plr.Loadouts[0].Dye;
                Item[] loadout2Armor = plr.Loadouts[1].Armor;
                Item[] loadout2Dye = plr.Loadouts[1].Dye;
                Item[] loadout3Armor = plr.Loadouts[2].Armor;
                Item[] loadout3Dye = plr.Loadouts[2].Dye;

                for (int i = 0; i < NetItem.MaxInventory; i++)
                {
                    if (i < NetItem.InventoryIndex.Item2)
                    {
                        //0-58
                        invs[i] = (NetItem)inventory[i];
                    }
                    else if (i < NetItem.ArmorIndex.Item2)
                    {
                        //59-78
                        var index = i - NetItem.ArmorIndex.Item1;
                        invs[i] = (NetItem)armor[index];
                    }
                    else if (i < NetItem.DyeIndex.Item2)
                    {
                        //79-88
                        var index = i - NetItem.DyeIndex.Item1;
                        invs[i] = (NetItem)dye[index];
                    }
                    else if (i < NetItem.MiscEquipIndex.Item2)
                    {
                        //89-93
                        var index = i - NetItem.MiscEquipIndex.Item1;
                        invs[i] = (NetItem)miscEqups[index];
                    }
                    else if (i < NetItem.MiscDyeIndex.Item2)
                    {
                        //93-98
                        var index = i - NetItem.MiscDyeIndex.Item1;
                        invs[i] = (NetItem)miscDyes[index];
                    }
                    else if (i < NetItem.PiggyIndex.Item2)
                    {
                        //98-138
                        var index = i - NetItem.PiggyIndex.Item1;
                        invs[i] = (NetItem)piggy[index];
                    }
                    else if (i < NetItem.SafeIndex.Item2)
                    {
                        //138-178
                        var index = i - NetItem.SafeIndex.Item1;
                        invs[i] = (NetItem)safe[index];
                    }
                    else if (i < NetItem.TrashIndex.Item2)
                    {
                        //179-219
                        invs[i] = (NetItem)trash;
                    }
                    else if (i < NetItem.ForgeIndex.Item2)
                    {
                        //220
                        var index = i - NetItem.ForgeIndex.Item1;
                        invs[i] = (NetItem)forge[index];
                    }
                    else if (i < NetItem.VoidIndex.Item2)
                    {
                        //220
                        var index = i - NetItem.VoidIndex.Item1;
                        invs[i] = (NetItem)voidVault[index];
                    }
                    else if (i < NetItem.Loadout1Armor.Item2)
                    {
                        var index = i - NetItem.Loadout1Armor.Item1;
                        invs[i] = (NetItem)loadout1Armor[index];
                    }
                    else if (i < NetItem.Loadout1Dye.Item2)
                    {
                        var index = i - NetItem.Loadout1Dye.Item1;
                        invs[i] = (NetItem)loadout1Dye[index];
                    }
                    else if (i < NetItem.Loadout2Armor.Item2)
                    {
                        var index = i - NetItem.Loadout2Armor.Item1;
                        invs[i] = (NetItem)loadout2Armor[index];
                    }
                    else if (i < NetItem.Loadout2Dye.Item2)
                    {
                        var index = i - NetItem.Loadout2Dye.Item1;
                        invs[i] = (NetItem)loadout2Dye[index];
                    }
                    else if (i < NetItem.Loadout3Armor.Item2)
                    {
                        var index = i - NetItem.Loadout3Armor.Item1;
                        invs[i] = (NetItem)loadout3Armor[index];
                    }
                    else if (i < NetItem.Loadout3Dye.Item2)
                    {
                        var index = i - NetItem.Loadout3Dye.Item1;
                        invs[i] = (NetItem)loadout3Dye[index];
                    }
                }
                #endregion

                return new RestObject
                {
                    {"name", plr.name },
                    {"online", 1 },

                    { "health", plr.statLife },
                    { "maxHealth", plr.statLifeMax },
                    { "mana", plr.statMana },
                    { "maxMana", plr.statManaMax },
                    { "inventory", string.Join("~", invs) },   // 背包
                    { "extraSlot", plr.extraAccessory ? 1:0 }, // 3335 恶魔之心
                    { "spawnX", plr.SpawnX },
                    { "spawnY", plr.SpawnY },
                    { "skinVariant", plr.skinVariant },
                    { "hair", plr.hair },
                    { "hairDye", plr.hairDye },
                    { "hairColor", TShock.Utils.EncodeColor(plr.hairColor) },
                    { "pantsColor", TShock.Utils.EncodeColor(plr.pantsColor) },
                    { "shirtColor", TShock.Utils.EncodeColor(plr.shirtColor) },
                    { "underShirtColor", TShock.Utils.EncodeColor(plr.underShirtColor) },
                    { "shoeColor", TShock.Utils.EncodeColor(plr.shoeColor) },
                    { "hideVisuals", TShock.Utils.EncodeBoolArray(plr.hideVisibleAccessory) },
                    { "skinColor", TShock.Utils.EncodeColor(plr.skinColor) },
                    { "eyeColor", TShock.Utils.EncodeColor(plr.eyeColor) },
                    { "questsCompleted", plr.anglerQuestsFinished },
                    { "usingBiomeTorches", plr.UsingBiomeTorches },
                    { "happyFunTorchTime", plr.happyFunTorchTime },
                    { "unlockedBiomeTorches", plr.unlockedBiomeTorches }, // 5043 火把神徽章
                    { "currentLoadoutIndex", plr.CurrentLoadoutIndex },
                    { "ateArtisanBread", plr.ateArtisanBread ? 1:0 },
                    { "usedAegisCrystal", plr.usedAegisCrystal ? 1:0 },
                    { "usedAegisFruit", plr.usedAegisFruit ? 1:0 },
                    { "usedArcaneCrystal", plr.usedArcaneCrystal ? 1:0 },
                    { "usedGalaxyPearl", plr.usedGalaxyPearl ? 1:0 },
                    { "usedGummyWorm", plr.usedGummyWorm ? 1:0 },
                    { "usedAmbrosia", plr.usedAmbrosia ? 1:0 },
                    { "unlockedSuperCart", plr.unlockedSuperCart ? 1:0 },
                    { "enabledSuperCart", plr.enabledSuperCart ? 1 : 0 },
                };
            }
            else
            {
                // 离线
                var data = TShock.CharacterDB.GetPlayerData(new TSPlayer(-1), found.ID);
                if (data == null)
                    return RestError("读取离线玩家数据出错！");

                return new RestObject
                {
                    { "name", name },
                    { "online", 0 },

                    { "health", data.health },
                    { "maxHealth", data.maxHealth },
                    { "mana", data.mana },
                    { "maxMana", data.maxMana },
                    { "inventory", string.Join("~", data.inventory) },   // 背包
                    { "extraSlot", data.extraSlot }, // 3335 恶魔之心
                    { "spawnX", data.spawnX },
                    { "spawnY", data.spawnY },
                    { "skinVariant", data.skinVariant },
                    { "hair", data.hair },
                    { "hairDye", data.hairDye },
                    { "hairColor", TShock.Utils.EncodeColor(data.hairColor) },
                    { "pantsColor", TShock.Utils.EncodeColor(data.pantsColor) },
                    { "shirtColor", TShock.Utils.EncodeColor(data.shirtColor) },
                    { "underShirtColor", TShock.Utils.EncodeColor(data.underShirtColor) },
                    { "shoeColor", TShock.Utils.EncodeColor(data.shoeColor) },
                    { "hideVisuals", TShock.Utils.EncodeBoolArray(data.hideVisuals) },
                    { "skinColor", TShock.Utils.EncodeColor(data.skinColor) },
                    { "eyeColor", TShock.Utils.EncodeColor(data.eyeColor) },
                    { "questsCompleted", data.questsCompleted },
                    { "usingBiomeTorches", data.usingBiomeTorches },
                    { "happyFunTorchTime", data.happyFunTorchTime },
                    { "unlockedBiomeTorches", data.unlockedBiomeTorches }, // 5043 火把神徽章
                    { "currentLoadoutIndex", data.currentLoadoutIndex },
                    { "ateArtisanBread", data.ateArtisanBread },
                    { "usedAegisCrystal", data.usedAegisCrystal },
                    { "usedAegisFruit", data.usedAegisFruit },
                    { "usedArcaneCrystal", data.usedArcaneCrystal },
                    { "usedGalaxyPearl", data.usedGalaxyPearl },
                    { "usedGummyWorm", data.usedGummyWorm },
                    { "usedAmbrosia", data.usedAmbrosia },
                    { "unlockedSuperCart", data.unlockedSuperCart },
                    { "enabledSuperCart", data.enabledSuperCart },
                };
            }
        }
        #endregion


        #region /pm/export
        [Route("/pm/export")]
        [Description("导出玩家存档")]
        [Permission("playermanager")]
        [Noun("name", true, "玩家名", typeof(string))]
        [Noun("base64", true, "是否将玩家存档转成base64", typeof(bool))]
        [Noun("hostpath", true, "是否返回服务器所在路径", typeof(bool))]
        [Token]
        public static object Export(RestRequestArgs args)
        {
            if (string.IsNullOrWhiteSpace(args.Parameters["name"]))
                return RestMissingParam("name");

            string name = args.Parameters["name"];
            List<string> names;
            string path;
            string errorStr;
            if (name == "*")
            {
                // 导出全部玩家
                var resul1 = ExportPlayer.ExportAll();
                errorStr = string.Join("\n", resul1["error"]);
                path = resul1["path"][0];
                names = resul1["names"];

                // 未导出任何存档
                if (names.Count == 0)
                    return RestError(errorStr);
            }
            else
            {
                // 导出单个玩家
                var result2 = ExportPlayer.Export(name);
                errorStr = result2["error"];
                path = result2["path"];
                names = new List<string>() { name };

                // 导出失败
                if (!string.IsNullOrEmpty(errorStr))
                    return RestError(errorStr);
            }

            var respone = new RestObject
            {
                { "names", names },
                { "path", path }
            };

            // 完整路径
            if (bool.TryParse(args.Parameters["hostpath"], out _))
            {
                respone.Add("hostpath", Directory.GetCurrentDirectory());
            }

            // 转成base64
            if (bool.TryParse(args.Parameters["base64"], out _))
            {
                List<string> b64s = new();
                foreach (var n in names)
                {
                    byte[] buffer = File.ReadAllBytes(Path.Combine(path, n + ".plr"));
                    b64s.Add(Convert.ToBase64String(buffer));
                }
                respone.Add("base64s", b64s);
            }

            return respone;
        }
        #endregion



        #region common
        /// <summary>
        /// 缺少参数
        /// </summary>
        public static RestObject RestInvalidParam(string var)
        {
            return RestError($"未找到参数 {var}，或者值为空！");
        }
        /// <summary>
        /// 参数无效
        /// </summary>
        public static RestObject RestMissingParam(string var)
        {
            return RestError($"未找到参数 {var}，或者值无效！");
        }

        /// <summary>
        /// 返回错误信息
        /// </summary>
        public static RestObject RestError(string message, string status = "400")
        {
            return new RestObject(status) { Error = message };
        }
        #endregion
    }
}