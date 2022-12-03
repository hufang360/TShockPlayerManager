using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Terraria;
using Terraria.Localization;
using TShockAPI;
using TShockAPI.DB;

namespace PlayerManager
{
    /// <summary>
    /// 角色数据
    /// </summary>
    public class CharacterData
    {
        public string inventoryStr;
        public string name;
        public int id;
        public Dictionary<int, int> buffs;

        public int health;
        public int maxHealth;
        public int mana;
        public int maxMana;
        public int spawnX = -1;
        public int spawnY = -1;
        public int? extraSlot;
        public int? skinVariant;
        public int? hair;
        public byte hairDye;
        public Color? hairColor;
        public Color? pantsColor;
        public Color? shirtColor;
        public Color? underShirtColor;
        public Color? shoeColor;
        public Color? skinColor;
        public Color? eyeColor;
        public bool[] hideVisuals;
        public int questsCompleted;
        public int usingBiomeTorches;
        public int happyFunTorchTime;
        public int unlockedBiomeTorches;
        public int currentLoadoutIndex;
        public int ateArtisanBread;
        public int usedAegisCrystal;
        public int usedAegisFruit;
        public int usedArcaneCrystal;
        public int usedGalaxyPearl;
        public int usedGummyWorm;
        public int usedAmbrosia;
        public int unlockedSuperCart;
        public int enabledSuperCart;

        /// <summary>
        /// 恢复类型（0=全部，1=仅背包，2=仅皮肤, 3=除背包和皮肤之外的属性，4=仅buff）
        /// </summary>
        public int RecoverType = 0;

        /// <summary>
        /// 读取ssc初始状态
        /// </summary>
        public void SeedInitialData(string userName)
        {
            name = userName;
            health = TShock.ServerSideCharacterConfig.Settings.StartingHealth;
            mana = TShock.ServerSideCharacterConfig.Settings.StartingMana;
            maxHealth = health;
            maxMana = mana;
            spawnX = -1;
            spawnY = -1;

            extraSlot = 0;
            questsCompleted = 0;
            usingBiomeTorches = 0;
            happyFunTorchTime = 0;
            unlockedBiomeTorches = 0;
            currentLoadoutIndex = 0;
            ateArtisanBread = 0;
            usedAegisCrystal = 0;
            usedAegisFruit = 0;
            usedArcaneCrystal = 0;
            usedGalaxyPearl = 0;
            usedGummyWorm = 0;
            usedAmbrosia = 0;
            unlockedSuperCart = 0;
            enabledSuperCart = 0;

            var items = new List<NetItem>(TShock.ServerSideCharacterConfig.Settings.StartingInventory);
            if (items.Count < NetItem.MaxInventory)
                items.AddRange(new NetItem[NetItem.MaxInventory - items.Count]);
            inventoryStr = string.Join("~", items.Take(NetItem.MaxInventory));

            buffs = new();
        }

        /// <summary>
        /// 读取玩家加入时的数据（主要是皮肤）
        /// </summary>
        /// <param name="playerData"></param>
        public void AddDataWhenJoined(PlayerData playerData)
        {
            skinVariant = playerData.skinVariant;
            hair = playerData.hair;
            hairDye = playerData.hairDye;
            hairColor = playerData.hairColor;
            pantsColor = playerData.pantsColor;
            shirtColor = playerData.shirtColor;
            underShirtColor = playerData.underShirtColor;
            shoeColor = playerData.shoeColor;
            hideVisuals = playerData.hideVisuals;
            skinColor = playerData.skinColor;
            eyeColor = playerData.eyeColor;

            buffs = new();
        }

        /// <summary>
        /// 从数据库中读取角色数据
        /// </summary>
        public void Copy(IDbConnection db, string userName)
        {
            name = userName;
            id = Utils.GetAccountID(db, userName);
            try
            {
                using var reader = db.QueryReader("SELECT * FROM tsCharacter WHERE Account=@0", id);
                if (reader.Read())
                {
                    health = reader.Get<int>("Health");
                    maxHealth = reader.Get<int>("MaxHealth");
                    mana = reader.Get<int>("Mana");
                    maxMana = reader.Get<int>("MaxMana");
                    spawnX = reader.Get<int>("spawnX");
                    spawnY = reader.Get<int>("spawnY");
                    extraSlot = reader.Get<int>("extraSlot");
                    skinVariant = reader.Get<int?>("skinVariant");
                    hair = reader.Get<int?>("hair");
                    hairDye = (byte)reader.Get<int>("hairDye");
                    hairColor = TShock.Utils.DecodeColor(reader.Get<int?>("hairColor"));
                    pantsColor = TShock.Utils.DecodeColor(reader.Get<int?>("pantsColor"));
                    shirtColor = TShock.Utils.DecodeColor(reader.Get<int?>("shirtColor"));
                    underShirtColor = TShock.Utils.DecodeColor(reader.Get<int?>("underShirtColor"));
                    shoeColor = TShock.Utils.DecodeColor(reader.Get<int?>("shoeColor"));
                    hideVisuals = TShock.Utils.DecodeBoolArray(reader.Get<int?>("hideVisuals"));
                    skinColor = TShock.Utils.DecodeColor(reader.Get<int?>("skinColor"));
                    eyeColor = TShock.Utils.DecodeColor(reader.Get<int?>("eyeColor"));
                    questsCompleted = reader.Get<int>("questsCompleted");
                    usingBiomeTorches = reader.Get<int>("usingBiomeTorches");
                    happyFunTorchTime = reader.Get<int>("happyFunTorchTime");
                    unlockedBiomeTorches = reader.Get<int>("unlockedBiomeTorches");
                    currentLoadoutIndex = reader.Get<int>("currentLoadoutIndex");
                    ateArtisanBread = reader.Get<int>("ateArtisanBread");
                    usedAegisCrystal = reader.Get<int>("usedAegisCrystal");
                    usedAegisFruit = reader.Get<int>("usedAegisFruit");
                    usedArcaneCrystal = reader.Get<int>("usedArcaneCrystal");
                    usedGalaxyPearl = reader.Get<int>("usedGalaxyPearl");
                    usedGummyWorm = reader.Get<int>("usedGummyWorm");
                    usedAmbrosia = reader.Get<int>("usedAmbrosia");
                    unlockedSuperCart = reader.Get<int>("unlockedSuperCart");
                    enabledSuperCart = reader.Get<int>("enabledSuperCart");

                    inventoryStr = reader.Get<string>("Inventory");
                    buffs = new();
                }
            }
            catch (Exception ex)
            {
                TShock.Log.Error(ex.ToString());
            }
        }

        /// <summary>
        /// 从玩家对象中读取角色数据
        /// </summary>
        public void CopyFormPlayer(Player player)
        {
            name = player.name;

            health = player.statLife;
            maxHealth = player.statLifeMax;
            mana = player.statMana;
            maxMana = player.statManaMax;
            spawnX = player.SpawnX;
            spawnY = player.SpawnY;
            extraSlot = player.extraAccessory ? 1 : 0;
            skinVariant = player.skinVariant;
            hair = player.hair;
            hairDye = player.hairDye;
            hairColor = player.hairColor;
            pantsColor = player.pantsColor;
            shirtColor = player.shirtColor;
            underShirtColor = player.underShirtColor;
            shoeColor = player.shoeColor;
            hideVisuals = player.hideVisibleAccessory;
            skinColor = player.skinColor;
            eyeColor = player.eyeColor;
            questsCompleted = player.anglerQuestsFinished;
            usingBiomeTorches = player.UsingBiomeTorches ? 1 : 0;
            happyFunTorchTime = player.happyFunTorchTime ? 1 : 0;
            unlockedBiomeTorches = player.unlockedBiomeTorches ? 1 : 0;
            currentLoadoutIndex = player.CurrentLoadoutIndex;
            ateArtisanBread = player.ateArtisanBread ? 1 : 0;
            usedAegisCrystal = player.usedAegisCrystal ? 1 : 0;
            usedAegisFruit = player.usedAegisFruit ? 1 : 0;
            usedArcaneCrystal = player.usedArcaneCrystal ? 1 : 0;
            usedGalaxyPearl = player.usedGalaxyPearl ? 1 : 0;
            usedGummyWorm = player.usedGummyWorm ? 1 : 0;
            usedAmbrosia = player.usedAmbrosia ? 1 : 0;
            unlockedSuperCart = player.unlockedSuperCart ? 1 : 0;
            enabledSuperCart = player.enabledSuperCart ? 1 : 0;

            // buff
            buffs = new();
            for (int i = 0; i < Player.maxBuffs; i++)
            {
                int buffID = player.buffType[i];
                int buffTime = player.buffTime[i];
                if (buffID == 0 || buffs.ContainsKey(buffID))
                    continue;
                buffs.Add(buffID, buffTime);
            }

            // 背包
            List<NetItem> items = new();
            items.AddRange(new NetItem[NetItem.MaxInventory]);

            Item[] inventory = player.inventory;
            Item[] armor = player.armor;
            Item[] dye = player.dye;
            Item[] miscEqups = player.miscEquips;
            Item[] miscDyes = player.miscDyes;
            Item[] piggy = player.bank.item;
            Item[] safe = player.bank2.item;
            Item[] forge = player.bank3.item;
            Item[] voidVault = player.bank4.item;
            Item trash = player.trashItem;
            Item[] loadout1Armor = player.Loadouts[0].Armor;
            Item[] loadout1Dye = player.Loadouts[0].Dye;
            Item[] loadout2Armor = player.Loadouts[1].Armor;
            Item[] loadout2Dye = player.Loadouts[1].Dye;
            Item[] loadout3Armor = player.Loadouts[2].Armor;
            Item[] loadout3Dye = player.Loadouts[2].Dye;

            for (int i = 0; i < NetItem.MaxInventory; i++)
            {
                if (i < NetItem.InventoryIndex.Item2)
                {
                    //0-58
                    items[i] = (NetItem)inventory[i];
                }
                else if (i < NetItem.ArmorIndex.Item2)
                {
                    //59-78
                    var index = i - NetItem.ArmorIndex.Item1;
                    items[i] = (NetItem)armor[index];
                }
                else if (i < NetItem.DyeIndex.Item2)
                {
                    //79-88
                    var index = i - NetItem.DyeIndex.Item1;
                    items[i] = (NetItem)dye[index];
                }
                else if (i < NetItem.MiscEquipIndex.Item2)
                {
                    //89-93
                    var index = i - NetItem.MiscEquipIndex.Item1;
                    items[i] = (NetItem)miscEqups[index];
                }
                else if (i < NetItem.MiscDyeIndex.Item2)
                {
                    //93-98
                    var index = i - NetItem.MiscDyeIndex.Item1;
                    items[i] = (NetItem)miscDyes[index];
                }
                else if (i < NetItem.PiggyIndex.Item2)
                {
                    //98-138
                    var index = i - NetItem.PiggyIndex.Item1;
                    items[i] = (NetItem)piggy[index];
                }
                else if (i < NetItem.SafeIndex.Item2)
                {
                    //138-178
                    var index = i - NetItem.SafeIndex.Item1;
                    items[i] = (NetItem)safe[index];
                }
                else if (i < NetItem.TrashIndex.Item2)
                {
                    //179-219
                    items[i] = (NetItem)trash;
                }
                else if (i < NetItem.ForgeIndex.Item2)
                {
                    //220
                    var index = i - NetItem.ForgeIndex.Item1;
                    items[i] = (NetItem)forge[index];
                }
                else if (i < NetItem.VoidIndex.Item2)
                {
                    //220
                    var index = i - NetItem.VoidIndex.Item1;
                    items[i] = (NetItem)voidVault[index];
                }
                else if (i < NetItem.Loadout1Armor.Item2)
                {
                    var index = i - NetItem.Loadout1Armor.Item1;
                    items[i] = (NetItem)loadout1Armor[index];
                }
                else if (i < NetItem.Loadout1Dye.Item2)
                {
                    var index = i - NetItem.Loadout1Dye.Item1;
                    items[i] = (NetItem)loadout1Dye[index];
                }
                else if (i < NetItem.Loadout2Armor.Item2)
                {
                    var index = i - NetItem.Loadout2Armor.Item1;
                    items[i] = (NetItem)loadout2Armor[index];
                }
                else if (i < NetItem.Loadout2Dye.Item2)
                {
                    var index = i - NetItem.Loadout2Dye.Item1;
                    items[i] = (NetItem)loadout2Dye[index];
                }
                else if (i < NetItem.Loadout3Armor.Item2)
                {
                    var index = i - NetItem.Loadout3Armor.Item1;
                    items[i] = (NetItem)loadout3Armor[index];
                }
                else if (i < NetItem.Loadout3Dye.Item2)
                {
                    var index = i - NetItem.Loadout3Dye.Item1;
                    items[i] = (NetItem)loadout3Dye[index];
                }
            }

            inventoryStr = string.Join("~", items.Take(NetItem.MaxInventory));
        }


        /// <summary>
        /// 还原至离线玩家
        /// </summary>
        public void ToDatabase(string userName)
        {
            IDbConnection db = TShock.CharacterDB.database;
            int _accountID = Utils.GetAccountID(db, userName);
            try
            {
                if (RecoverType == 1)
                {
                    // 仅背包
                    db.Query("UPDATE tsCharacter SET Inventory = @1 WHERE Account = @0;", _accountID, inventoryStr);
                }
                else if (RecoverType == 2)
                {
                    // 仅皮肤
                    db.Query("UPDATE tsCharacter SET skinVariant = @1, hair = @2, hairDye = @3, hairColor = @4, pantsColor = @5, shirtColor = @6, underShirtColor = @7, shoeColor = @8, hideVisuals = @9, skinColor = @10, eyeColor = @11 WHERE Account = @0;",
                        _accountID,
                        skinVariant,
                        hair,
                        hairDye,
                        TShock.Utils.EncodeColor(hairColor),
                        TShock.Utils.EncodeColor(pantsColor),
                        TShock.Utils.EncodeColor(shirtColor),
                        TShock.Utils.EncodeColor(underShirtColor),
                        TShock.Utils.EncodeColor(shoeColor),
                        TShock.Utils.EncodeBoolArray(hideVisuals),
                        TShock.Utils.EncodeColor(skinColor),
                        TShock.Utils.EncodeColor(eyeColor)
                    );
                }
                else if (RecoverType == 4)
                {
                    // 仅属性（除背包和皮肤外）
                    db.Query("UPDATE tsCharacter SET Health = @1, MaxHealth = @2, Mana = @3, MaxMana = @4, extraSlot = @5, spawnX = @6, spawnY = @7, questsCompleted = @8, usingBiomeTorches = @9, happyFunTorchTime = @10, unlockedBiomeTorches = @11, ateArtisanBread = @12, usedAegisCrystal = @13, usedAegisFruit = @14, usedArcaneCrystal = @15, usedGalaxyPearl = @16, usedGummyWorm = @17, usedAmbrosia = @18, unlockedSuperCart = @19 WHERE Account = @0;",
                        _accountID,
                        health,
                        maxHealth,
                        mana,
                        maxMana,
                        extraSlot,
                        spawnX,
                        spawnY,
                        questsCompleted,
                        usingBiomeTorches,
                        happyFunTorchTime,
                        unlockedBiomeTorches,
                        ateArtisanBread,
                        usedAegisCrystal,
                        usedAegisFruit,
                        usedArcaneCrystal,
                        usedGalaxyPearl,
                        usedGummyWorm,
                        usedAmbrosia,
                        unlockedSuperCart
                    );
                }
                else
                {
                    // 恢复全部
                    db.Query("UPDATE tsCharacter SET Health = @1, MaxHealth = @2, Mana = @3, MaxMana = @4, Inventory = @5, extraSlot = @6, spawnX = @7, spawnY = @8, skinVariant = @9, hair = @10, hairDye = @11, hairColor = @12, pantsColor = @13, shirtColor = @14, underShirtColor = @15, shoeColor = @16, hideVisuals = @17, skinColor = @18, eyeColor = @19, questsCompleted = @20, usingBiomeTorches = @21, happyFunTorchTime = @22, unlockedBiomeTorches = @23, currentLoadoutIndex = @24, ateArtisanBread = @25, usedAegisCrystal = @26, usedAegisFruit = @27, usedArcaneCrystal = @28, usedGalaxyPearl = @29, usedGummyWorm = @30, usedAmbrosia = @31, unlockedSuperCart = @32, enabledSuperCart = @33 WHERE Account = @0;",
                        _accountID,
                        health,
                        maxHealth,
                        mana,
                        maxMana,
                        inventoryStr,
                        extraSlot,
                        spawnX,
                        spawnY,
                        skinVariant,
                        hair,
                        hairDye,
                        TShock.Utils.EncodeColor(hairColor),
                        TShock.Utils.EncodeColor(pantsColor),
                        TShock.Utils.EncodeColor(shirtColor),
                        TShock.Utils.EncodeColor(underShirtColor),
                        TShock.Utils.EncodeColor(shoeColor),
                        TShock.Utils.EncodeBoolArray(hideVisuals),
                        TShock.Utils.EncodeColor(skinColor),
                        TShock.Utils.EncodeColor(eyeColor),
                        questsCompleted,
                        usingBiomeTorches,
                        happyFunTorchTime,
                        unlockedBiomeTorches,
                        currentLoadoutIndex,
                        ateArtisanBread,
                        usedAegisCrystal,
                        usedAegisFruit,
                        usedArcaneCrystal,
                        usedGalaxyPearl,
                        usedGummyWorm,
                        usedAmbrosia,
                        unlockedSuperCart,
                        enabledSuperCart
                    );
                }
            }
            catch (Exception ex)
            {
                TShock.Log.ConsoleError(ex.ToString());
            }
        }

        /// <summary>
        /// 还原至在线玩家
        /// </summary>
        public void ToPlayer(TSPlayer player)
        {
            if (RecoverType == 0 || RecoverType == 4)
            {
                // 恢复属性
                player.TPlayer.statLife = health;
                player.TPlayer.statLifeMax = maxHealth;
                player.TPlayer.statMana = maxMana;
                player.TPlayer.statManaMax = maxMana;
                player.TPlayer.SpawnX = spawnX;
                player.TPlayer.SpawnY = spawnY;
                player.sX = spawnX;
                player.sY = spawnY;
                player.TPlayer.anglerQuestsFinished = questsCompleted;
                player.TPlayer.UsingBiomeTorches = usingBiomeTorches == 1;
                player.TPlayer.happyFunTorchTime = happyFunTorchTime == 1;
                player.TPlayer.unlockedBiomeTorches = unlockedBiomeTorches == 1;
                player.TPlayer.CurrentLoadoutIndex = currentLoadoutIndex;
                player.TPlayer.ateArtisanBread = ateArtisanBread == 1;
                player.TPlayer.usedAegisCrystal = usedAegisCrystal == 1;
                player.TPlayer.usedAegisFruit = usedAegisFruit == 1;
                player.TPlayer.usedArcaneCrystal = usedArcaneCrystal == 1;
                player.TPlayer.usedGalaxyPearl = usedGalaxyPearl == 1;
                player.TPlayer.usedGummyWorm = usedGummyWorm == 1;
                player.TPlayer.usedAmbrosia = usedAmbrosia == 1;
                player.TPlayer.unlockedSuperCart = unlockedSuperCart == 1;
                player.TPlayer.enabledSuperCart = enabledSuperCart == 1;
            }

            if (RecoverType == 0 || RecoverType == 2)
            {
                // 恢复皮肤
                player.TPlayer.hairDye = hairDye;
                if (extraSlot != null)
                    player.TPlayer.extraAccessory = extraSlot.Value == 1;
                if (skinVariant != null)
                    player.TPlayer.skinVariant = skinVariant.Value;
                if (hair != null)
                    player.TPlayer.hair = hair.Value;
                if (hairColor != null)
                    player.TPlayer.hairColor = hairColor.Value;
                if (pantsColor != null)
                    player.TPlayer.pantsColor = pantsColor.Value;
                if (shirtColor != null)
                    player.TPlayer.shirtColor = shirtColor.Value;
                if (underShirtColor != null)
                    player.TPlayer.underShirtColor = underShirtColor.Value;
                if (shoeColor != null)
                    player.TPlayer.shoeColor = shoeColor.Value;
                if (skinColor != null)
                    player.TPlayer.skinColor = skinColor.Value;
                if (eyeColor != null)
                    player.TPlayer.eyeColor = eyeColor.Value;

                if (hideVisuals != null)
                    player.TPlayer.hideVisibleAccessory = hideVisuals;
                else
                    player.TPlayer.hideVisibleAccessory = new bool[player.TPlayer.hideVisibleAccessory.Length];
            }


            // 恢复背包
            if (RecoverType == 0 || RecoverType == 1)
            {
                // 恢复背包
                ToPlayerInventory(player);
                // 4 PlayerInfo
                // 5 PlayerSlot
                // 16 PlayerHp
                // 42 PlayerMana
                // 50 PlayerBuff
                // 76 NumberOfAnglerQuestsCompleted
                // 39 RemoveItemOwner
            }

            // 仅buff
            if( RecoverType==3 )
            {
                ToPlayerBuff(player);
            }


            // 补发消息（不恢复背包的话会跳过一些消息）
            if (RecoverType == 2)
            {
                // 仅皮肤
                NetMessage.SendData(4, -1, -1, NetworkText.FromLiteral(player.Name), player.Index, 0f, 0f, 0f, 0);
                NetMessage.SendData(4, player.Index, -1, NetworkText.FromLiteral(player.Name), player.Index, 0f, 0f, 0f, 0);
            }
            else if (RecoverType == 4)
            {
                // 一些属性
                NetMessage.SendData(4, -1, -1, NetworkText.FromLiteral(player.Name), player.Index, 0f, 0f, 0f, 0);
                NetMessage.SendData(42, -1, -1, NetworkText.Empty, player.Index, 0f, 0f, 0f, 0);
                NetMessage.SendData(16, -1, -1, NetworkText.Empty, player.Index, 0f, 0f, 0f, 0);

                NetMessage.SendData(4, player.Index, -1, NetworkText.FromLiteral(player.Name), player.Index, 0f, 0f, 0f, 0);
                NetMessage.SendData(42, player.Index, -1, NetworkText.Empty, player.Index, 0f, 0f, 0f, 0);
                NetMessage.SendData(16, player.Index, -1, NetworkText.Empty, player.Index, 0f, 0f, 0f, 0);

                // 恢复buff
                ToPlayerBuff(player);

                NetMessage.SendData(76, player.Index, -1, NetworkText.Empty, player.Index);
                NetMessage.SendData(76, -1, -1, NetworkText.Empty, player.Index);
            }
        }

        /// <summary>
        /// 恢复在线玩家的背包
        /// </summary>
        /// <param name="player"></param>
        private void ToPlayerInventory(TSPlayer player)
        {
            List<NetItem> inventory = inventoryStr.Split('~').Select(NetItem.Parse).ToList();
            if (inventory.Count < NetItem.MaxInventory)
            {
                inventory.InsertRange(67, new NetItem[2]);
                inventory.InsertRange(77, new NetItem[2]);
                inventory.InsertRange(87, new NetItem[2]);
                inventory.AddRange(new NetItem[NetItem.MaxInventory - inventory.Count]);
            }
            for (int i = 0; i < NetItem.MaxInventory; i++)
            {
                if (i < NetItem.InventoryIndex.Item2)
                {
                    //0-58
                    player.TPlayer.inventory[i].netDefaults(inventory[i].NetId);

                    if (player.TPlayer.inventory[i].netID != 0)
                    {
                        player.TPlayer.inventory[i].stack = inventory[i].Stack;
                        player.TPlayer.inventory[i].prefix = inventory[i].PrefixId;
                    }
                }
                else if (i < NetItem.ArmorIndex.Item2)
                {
                    //59-78
                    var index = i - NetItem.ArmorIndex.Item1;
                    player.TPlayer.armor[index].netDefaults(inventory[i].NetId);

                    if (player.TPlayer.armor[index].netID != 0)
                    {
                        player.TPlayer.armor[index].stack = inventory[i].Stack;
                        player.TPlayer.armor[index].prefix = inventory[i].PrefixId;
                    }
                }
                else if (i < NetItem.DyeIndex.Item2)
                {
                    //79-88
                    var index = i - NetItem.DyeIndex.Item1;
                    player.TPlayer.dye[index].netDefaults(inventory[i].NetId);

                    if (player.TPlayer.dye[index].netID != 0)
                    {
                        player.TPlayer.dye[index].stack = inventory[i].Stack;
                        player.TPlayer.dye[index].prefix = inventory[i].PrefixId;
                    }
                }
                else if (i < NetItem.MiscEquipIndex.Item2)
                {
                    //89-93
                    var index = i - NetItem.MiscEquipIndex.Item1;
                    player.TPlayer.miscEquips[index].netDefaults(inventory[i].NetId);

                    if (player.TPlayer.miscEquips[index].netID != 0)
                    {
                        player.TPlayer.miscEquips[index].stack = inventory[i].Stack;
                        player.TPlayer.miscEquips[index].prefix = inventory[i].PrefixId;
                    }
                }
                else if (i < NetItem.MiscDyeIndex.Item2)
                {
                    //93-98
                    var index = i - NetItem.MiscDyeIndex.Item1;
                    player.TPlayer.miscDyes[index].netDefaults(inventory[i].NetId);

                    if (player.TPlayer.miscDyes[index].netID != 0)
                    {
                        player.TPlayer.miscDyes[index].stack = inventory[i].Stack;
                        player.TPlayer.miscDyes[index].prefix = inventory[i].PrefixId;
                    }
                }
                else if (i < NetItem.PiggyIndex.Item2)
                {
                    //98-138
                    var index = i - NetItem.PiggyIndex.Item1;
                    player.TPlayer.bank.item[index].netDefaults(inventory[i].NetId);

                    if (player.TPlayer.bank.item[index].netID != 0)
                    {
                        player.TPlayer.bank.item[index].stack = inventory[i].Stack;
                        player.TPlayer.bank.item[index].prefix = inventory[i].PrefixId;
                    }
                }
                else if (i < NetItem.SafeIndex.Item2)
                {
                    //138-178
                    var index = i - NetItem.SafeIndex.Item1;
                    player.TPlayer.bank2.item[index].netDefaults(inventory[i].NetId);

                    if (player.TPlayer.bank2.item[index].netID != 0)
                    {
                        player.TPlayer.bank2.item[index].stack = inventory[i].Stack;
                        player.TPlayer.bank2.item[index].prefix = inventory[i].PrefixId;
                    }
                }
                else if (i < NetItem.TrashIndex.Item2)
                {
                    //179-219
                    var index = i - NetItem.TrashIndex.Item1;
                    player.TPlayer.trashItem.netDefaults(inventory[i].NetId);

                    if (player.TPlayer.trashItem.netID != 0)
                    {
                        player.TPlayer.trashItem.stack = inventory[i].Stack;
                        player.TPlayer.trashItem.prefix = inventory[i].PrefixId;
                    }
                }
                else if (i < NetItem.ForgeIndex.Item2)
                {
                    //220
                    var index = i - NetItem.ForgeIndex.Item1;
                    player.TPlayer.bank3.item[index].netDefaults(inventory[i].NetId);

                    if (player.TPlayer.bank3.item[index].netID != 0)
                    {
                        player.TPlayer.bank3.item[index].stack = inventory[i].Stack;
                        player.TPlayer.bank3.item[index].Prefix(inventory[i].PrefixId);
                    }
                }
                else if (i < NetItem.VoidIndex.Item2)
                {
                    //260
                    var index = i - NetItem.VoidIndex.Item1;
                    player.TPlayer.bank4.item[index].netDefaults(inventory[i].NetId);

                    if (player.TPlayer.bank4.item[index].netID != 0)
                    {
                        player.TPlayer.bank4.item[index].stack = inventory[i].Stack;
                        player.TPlayer.bank4.item[index].Prefix(inventory[i].PrefixId);
                    }
                }
                else if (i < NetItem.Loadout1Armor.Item2)
                {
                    var index = i - NetItem.Loadout1Armor.Item1;
                    player.TPlayer.Loadouts[0].Armor[index].netDefaults(inventory[i].NetId);

                    if (player.TPlayer.Loadouts[0].Armor[index].netID != 0)
                    {
                        player.TPlayer.Loadouts[0].Armor[index].stack = inventory[i].Stack;
                        player.TPlayer.Loadouts[0].Armor[index].Prefix(inventory[i].PrefixId);
                    }
                }
                else if (i < NetItem.Loadout1Dye.Item2)
                {
                    var index = i - NetItem.Loadout1Dye.Item1;
                    player.TPlayer.Loadouts[0].Dye[index].netDefaults(inventory[i].NetId);

                    if (player.TPlayer.Loadouts[0].Dye[index].netID != 0)
                    {
                        player.TPlayer.Loadouts[0].Dye[index].stack = inventory[i].Stack;
                        player.TPlayer.Loadouts[0].Dye[index].Prefix(inventory[i].PrefixId);
                    }
                }
                else if (i < NetItem.Loadout2Armor.Item2)
                {
                    var index = i - NetItem.Loadout2Armor.Item1;
                    player.TPlayer.Loadouts[1].Armor[index].netDefaults(inventory[i].NetId);

                    if (player.TPlayer.Loadouts[1].Armor[index].netID != 0)
                    {
                        player.TPlayer.Loadouts[1].Armor[index].stack = inventory[i].Stack;
                        player.TPlayer.Loadouts[1].Armor[index].Prefix(inventory[i].PrefixId);
                    }
                }
                else if (i < NetItem.Loadout2Dye.Item2)
                {
                    var index = i - NetItem.Loadout2Dye.Item1;
                    player.TPlayer.Loadouts[1].Dye[index].netDefaults(inventory[i].NetId);

                    if (player.TPlayer.Loadouts[1].Dye[index].netID != 0)
                    {
                        player.TPlayer.Loadouts[1].Dye[index].stack = inventory[i].Stack;
                        player.TPlayer.Loadouts[1].Dye[index].Prefix(inventory[i].PrefixId);
                    }
                }
                else if (i < NetItem.Loadout3Armor.Item2)
                {
                    var index = i - NetItem.Loadout3Armor.Item1;
                    player.TPlayer.Loadouts[2].Armor[index].netDefaults(inventory[i].NetId);

                    if (player.TPlayer.Loadouts[2].Armor[index].netID != 0)
                    {
                        player.TPlayer.Loadouts[2].Armor[index].stack = inventory[i].Stack;
                        player.TPlayer.Loadouts[2].Armor[index].Prefix(inventory[i].PrefixId);
                    }
                }
                else if (i < NetItem.Loadout3Dye.Item2)
                {
                    var index = i - NetItem.Loadout3Dye.Item1;
                    player.TPlayer.Loadouts[2].Dye[index].netDefaults(inventory[i].NetId);

                    if (player.TPlayer.Loadouts[2].Dye[index].netID != 0)
                    {
                        player.TPlayer.Loadouts[2].Dye[index].stack = inventory[i].Stack;
                        player.TPlayer.Loadouts[2].Dye[index].Prefix(inventory[i].PrefixId);
                    }
                }
            }


            NetMessage.SendData((int)PacketTypes.SyncLoadout, remoteClient: player.Index, number: player.Index, number2: player.TPlayer.CurrentLoadoutIndex);
            NetMessage.SendData((int)PacketTypes.SyncLoadout, ignoreClient: player.Index, number: player.Index, number2: player.TPlayer.CurrentLoadoutIndex);

            float slot = 0f;
            for (int k = 0; k < NetItem.InventorySlots; k++)
            {
                NetMessage.SendData(5, -1, -1, NetworkText.FromLiteral(Main.player[player.Index].inventory[k].Name), player.Index, slot, Main.player[player.Index].inventory[k].prefix);
                slot++;
            }
            for (int k = 0; k < NetItem.ArmorSlots; k++)
            {
                NetMessage.SendData(5, -1, -1, NetworkText.FromLiteral(Main.player[player.Index].armor[k].Name), player.Index, slot, Main.player[player.Index].armor[k].prefix);
                slot++;
            }
            for (int k = 0; k < NetItem.DyeSlots; k++)
            {
                NetMessage.SendData(5, -1, -1, NetworkText.FromLiteral(Main.player[player.Index].dye[k].Name), player.Index, slot, Main.player[player.Index].dye[k].prefix);
                slot++;
            }
            for (int k = 0; k < NetItem.MiscEquipSlots; k++)
            {
                NetMessage.SendData(5, -1, -1, NetworkText.FromLiteral(Main.player[player.Index].miscEquips[k].Name), player.Index, slot, Main.player[player.Index].miscEquips[k].prefix);
                slot++;
            }
            for (int k = 0; k < NetItem.MiscDyeSlots; k++)
            {
                NetMessage.SendData(5, -1, -1, NetworkText.FromLiteral(Main.player[player.Index].miscDyes[k].Name), player.Index, slot, Main.player[player.Index].miscDyes[k].prefix);
                slot++;
            }
            for (int k = 0; k < NetItem.PiggySlots; k++)
            {
                NetMessage.SendData(5, -1, -1, NetworkText.FromLiteral(Main.player[player.Index].bank.item[k].Name), player.Index, slot, Main.player[player.Index].bank.item[k].prefix);
                slot++;
            }
            for (int k = 0; k < NetItem.SafeSlots; k++)
            {
                NetMessage.SendData(5, -1, -1, NetworkText.FromLiteral(Main.player[player.Index].bank2.item[k].Name), player.Index, slot, Main.player[player.Index].bank2.item[k].prefix);
                slot++;
            }
            NetMessage.SendData(5, -1, -1, NetworkText.FromLiteral(Main.player[player.Index].trashItem.Name), player.Index, slot++, Main.player[player.Index].trashItem.prefix);
            for (int k = 0; k < NetItem.ForgeSlots; k++)
            {
                NetMessage.SendData(5, -1, -1, NetworkText.FromLiteral(Main.player[player.Index].bank3.item[k].Name), player.Index, slot, Main.player[player.Index].bank3.item[k].prefix);
                slot++;
            }
            for (int k = 0; k < NetItem.VoidSlots; k++)
            {
                NetMessage.SendData(5, -1, -1, NetworkText.FromLiteral(Main.player[player.Index].bank4.item[k].Name), player.Index, slot, Main.player[player.Index].bank4.item[k].prefix);
                slot++;
            }
            for (int k = 0; k < NetItem.LoadoutArmorSlots; k++)
            {
                NetMessage.SendData(5, -1, -1, NetworkText.FromLiteral(Main.player[player.Index].Loadouts[0].Armor[k].Name), player.Index, slot, Main.player[player.Index].Loadouts[0].Armor[k].prefix);
                slot++;
            }
            for (int k = 0; k < NetItem.LoadoutDyeSlots; k++)
            {
                NetMessage.SendData(5, -1, -1, NetworkText.FromLiteral(Main.player[player.Index].Loadouts[0].Dye[k].Name), player.Index, slot, Main.player[player.Index].Loadouts[0].Dye[k].prefix);
                slot++;
            }
            for (int k = 0; k < NetItem.LoadoutArmorSlots; k++)
            {
                NetMessage.SendData(5, -1, -1, NetworkText.FromLiteral(Main.player[player.Index].Loadouts[1].Armor[k].Name), player.Index, slot, Main.player[player.Index].Loadouts[1].Armor[k].prefix);
                slot++;
            }
            for (int k = 0; k < NetItem.LoadoutDyeSlots; k++)
            {
                NetMessage.SendData(5, -1, -1, NetworkText.FromLiteral(Main.player[player.Index].Loadouts[1].Dye[k].Name), player.Index, slot, Main.player[player.Index].Loadouts[1].Dye[k].prefix);
                slot++;
            }
            for (int k = 0; k < NetItem.LoadoutArmorSlots; k++)
            {
                NetMessage.SendData(5, -1, -1, NetworkText.FromLiteral(Main.player[player.Index].Loadouts[2].Armor[k].Name), player.Index, slot, Main.player[player.Index].Loadouts[2].Armor[k].prefix);
                slot++;
            }
            for (int k = 0; k < NetItem.LoadoutDyeSlots; k++)
            {
                NetMessage.SendData(5, -1, -1, NetworkText.FromLiteral(Main.player[player.Index].Loadouts[1].Dye[k].Name), player.Index, slot, Main.player[player.Index].Loadouts[2].Dye[k].prefix);
                slot++;
            }

            NetMessage.SendData(4, -1, -1, NetworkText.FromLiteral(player.Name), player.Index, 0f, 0f, 0f, 0);
            NetMessage.SendData(42, -1, -1, NetworkText.Empty, player.Index, 0f, 0f, 0f, 0);
            NetMessage.SendData(16, -1, -1, NetworkText.Empty, player.Index, 0f, 0f, 0f, 0);

            slot = 0f;
            for (int k = 0; k < NetItem.InventorySlots; k++)
            {
                NetMessage.SendData(5, player.Index, -1, NetworkText.FromLiteral(Main.player[player.Index].inventory[k].Name), player.Index, slot, Main.player[player.Index].inventory[k].prefix);
                slot++;
            }
            for (int k = 0; k < NetItem.ArmorSlots; k++)
            {
                NetMessage.SendData(5, player.Index, -1, NetworkText.FromLiteral(Main.player[player.Index].armor[k].Name), player.Index, slot, Main.player[player.Index].armor[k].prefix);
                slot++;
            }
            for (int k = 0; k < NetItem.DyeSlots; k++)
            {
                NetMessage.SendData(5, player.Index, -1, NetworkText.FromLiteral(Main.player[player.Index].dye[k].Name), player.Index, slot, Main.player[player.Index].dye[k].prefix);
                slot++;
            }
            for (int k = 0; k < NetItem.MiscEquipSlots; k++)
            {
                NetMessage.SendData(5, player.Index, -1, NetworkText.FromLiteral(Main.player[player.Index].miscEquips[k].Name), player.Index, slot, Main.player[player.Index].miscEquips[k].prefix);
                slot++;
            }
            for (int k = 0; k < NetItem.MiscDyeSlots; k++)
            {
                NetMessage.SendData(5, player.Index, -1, NetworkText.FromLiteral(Main.player[player.Index].miscDyes[k].Name), player.Index, slot, Main.player[player.Index].miscDyes[k].prefix);
                slot++;
            }
            for (int k = 0; k < NetItem.PiggySlots; k++)
            {
                NetMessage.SendData(5, player.Index, -1, NetworkText.FromLiteral(Main.player[player.Index].bank.item[k].Name), player.Index, slot, Main.player[player.Index].bank.item[k].prefix);
                slot++;
            }
            for (int k = 0; k < NetItem.SafeSlots; k++)
            {
                NetMessage.SendData(5, player.Index, -1, NetworkText.FromLiteral(Main.player[player.Index].bank2.item[k].Name), player.Index, slot, Main.player[player.Index].bank2.item[k].prefix);
                slot++;
            }
            NetMessage.SendData(5, player.Index, -1, NetworkText.FromLiteral(Main.player[player.Index].trashItem.Name), player.Index, slot++, Main.player[player.Index].trashItem.prefix);
            for (int k = 0; k < NetItem.ForgeSlots; k++)
            {
                NetMessage.SendData(5, player.Index, -1, NetworkText.FromLiteral(Main.player[player.Index].bank3.item[k].Name), player.Index, slot, Main.player[player.Index].bank3.item[k].prefix);
                slot++;
            }
            for (int k = 0; k < NetItem.VoidSlots; k++)
            {
                NetMessage.SendData(5, player.Index, -1, NetworkText.FromLiteral(Main.player[player.Index].bank4.item[k].Name), player.Index, slot, Main.player[player.Index].bank4.item[k].prefix);
                slot++;
            }
            for (int k = 0; k < NetItem.LoadoutArmorSlots; k++)
            {
                NetMessage.SendData(5, player.Index, -1, NetworkText.FromLiteral(Main.player[player.Index].Loadouts[0].Armor[k].Name), player.Index, slot, Main.player[player.Index].Loadouts[0].Armor[k].prefix);
                slot++;
            }
            for (int k = 0; k < NetItem.LoadoutDyeSlots; k++)
            {
                NetMessage.SendData(5, player.Index, -1, NetworkText.FromLiteral(Main.player[player.Index].Loadouts[0].Dye[k].Name), player.Index, slot, Main.player[player.Index].Loadouts[0].Dye[k].prefix);
                slot++;
            }
            for (int k = 0; k < NetItem.LoadoutArmorSlots; k++)
            {
                NetMessage.SendData(5, player.Index, -1, NetworkText.FromLiteral(Main.player[player.Index].Loadouts[1].Armor[k].Name), player.Index, slot, Main.player[player.Index].Loadouts[1].Armor[k].prefix);
                slot++;
            }
            for (int k = 0; k < NetItem.LoadoutDyeSlots; k++)
            {
                NetMessage.SendData(5, player.Index, -1, NetworkText.FromLiteral(Main.player[player.Index].Loadouts[1].Dye[k].Name), player.Index, slot, Main.player[player.Index].Loadouts[1].Dye[k].prefix);
                slot++;
            }
            for (int k = 0; k < NetItem.LoadoutArmorSlots; k++)
            {
                NetMessage.SendData(5, player.Index, -1, NetworkText.FromLiteral(Main.player[player.Index].Loadouts[2].Armor[k].Name), player.Index, slot, Main.player[player.Index].Loadouts[2].Armor[k].prefix);
                slot++;
            }
            for (int k = 0; k < NetItem.LoadoutDyeSlots; k++)
            {
                NetMessage.SendData(5, player.Index, -1, NetworkText.FromLiteral(Main.player[player.Index].Loadouts[2].Dye[k].Name), player.Index, slot, Main.player[player.Index].Loadouts[2].Dye[k].prefix);
                slot++;
            }

            NetMessage.SendData(4, player.Index, -1, NetworkText.FromLiteral(player.Name), player.Index, 0f, 0f, 0f, 0);
            NetMessage.SendData(42, player.Index, -1, NetworkText.Empty, player.Index, 0f, 0f, 0f, 0);
            NetMessage.SendData(16, player.Index, -1, NetworkText.Empty, player.Index, 0f, 0f, 0f, 0);

            // 恢复buff
            ToPlayerBuff(player);

            NetMessage.SendData(76, player.Index, -1, NetworkText.Empty, player.Index);
            NetMessage.SendData(76, -1, -1, NetworkText.Empty, player.Index);

            NetMessage.SendData(39, player.Index, -1, NetworkText.Empty, 400);

            // 4 PlayerInfo
            // 5 PlayerSlot
            // 16 PlayerHp
            // 42 PlayerMana
            // 50 PlayerBuff
            // 76 NumberOfAnglerQuestsCompleted
            // 39 RemoveItemOwner

            // 55 PlayerAddBuff
        }

        // 恢复buff到玩家
        void ToPlayerBuff(TSPlayer player)
        {
            // 50 PlayerBuff
            // 55 PlayerAddBuff
            for (int k = 0; k < Player.maxBuffs; k++)
            {
                player.TPlayer.buffType[k] = 0;
                player.TPlayer.buffTime[k] = 0;
            }
            NetMessage.SendData(50, -1, -1, NetworkText.Empty, player.Index, 0f, 0f, 0f, 0);
            NetMessage.SendData(50, player.Index, -1, NetworkText.Empty, player.Index, 0f, 0f, 0f, 0);

            // 恢复buff
            foreach (var obj in buffs)
            {
                player.SetBuff(obj.Key, obj.Value);
            }
        }

        /// <summary>
        /// 获得恢复选项说明
        /// </summary>
        public string GetRecoverTypeDesc()
        {
            return RecoverType switch
            {
                1 => "仅背包",
                2 => "仅皮肤",
                3 => "仅buff",
                4 => "仅属性",
                _ => "",
            };
        }


    }
}
