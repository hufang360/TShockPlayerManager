using System;
using System.Collections.Generic;
using System.Data;
using Terraria;
using Terraria.Localization;
using TShockAPI;
using TShockAPI.DB;

namespace PlayerManager
{
    /// <summary>
    /// 属性修改
    /// </summary>
    public class Modify
    {
        #region 修改生命值 和 魔力值
        /// <summary>
        /// 修改生命值
        /// </summary>
        public static void ModifyHP(CommandArgs args)
        {
            if (args.Parameters.Count < 3)
            {
                args.Player.SendInfoMessage("语法错误，/pm hp <玩家名> <生命值>");
                return;
            }
            string name = args.Parameters[1];
            string value = args.Parameters[2];
            int hp = 0;
            if (!int.TryParse(value, out hp))
            {
                hp = 0;
            }
            if (hp < 1)
            {
                args.Player.SendSuccessMessage($"输入的生命值无效, {value}");
                return;
            }

            FoundPlayer found = Utils.GetPlayer(name, out string errMsg);
            if (!found.valid)
            {
                args.Player.SendErrorMessage(errMsg);
                return;
            }

            if (found.online)
            {
                found.plr.TPlayer.statLife = hp;
                NetMessage.SendData((int)PacketTypes.PlayerHp, -1, -1, NetworkText.Empty, found.plr.Index, 0f, 0f, 0f, 0);
                args.Player.SendSuccessMessage($"在线玩家 {found.Name} 的生命值已修改为 {hp}");
            }
            else
            {
                bool success = StatsDB(args.Player, found.ID, hp, 1);
                if (success)
                {
                    args.Player.SendSuccessMessage($"离线玩家 {found.Name} 的生命值已修改为 {hp}");
                }
            }
        }

        /// <summary>
        /// 修改生命上限
        /// </summary>
        public static void ModifyMaxHP(CommandArgs args)
        {
            if (args.Parameters.Count < 3)
            {
                args.Player.SendInfoMessage("语法错误，/pm maxhp <玩家名> <生命上限>");
                return;
            }
            string name = args.Parameters[1];
            string value = args.Parameters[2];
            int maxhp = 0;
            if (!int.TryParse(value, out maxhp))
            {
                maxhp = 0;
            }
            if (maxhp < 100)
            {
                args.Player.SendErrorMessage("生命上限，不能低于100!");
                return;
            }

            FoundPlayer found = Utils.GetPlayer(name, out string errMsg);
            if (!found.valid)
            {
                args.Player.SendErrorMessage(errMsg);
                return;
            }

            if (found.online)
            {
                found.plr.TPlayer.statLifeMax = maxhp;
                NetMessage.SendData((int)PacketTypes.PlayerHp, -1, -1, NetworkText.Empty, found.plr.Index, 0f, 0f, 0f, 0);
                args.Player.SendSuccessMessage($"在线玩家 {found.Name} 的生命上限已修改为 {maxhp}");
            }
            else
            {
                bool success = StatsDB(args.Player, found.ID, maxhp, 2);
                if (success)
                {
                    args.Player.SendSuccessMessage($"离线玩家 {found.Name} 的生命上限已修改为 {maxhp}");
                }
            }
        }

        /// <summary>
        /// 修改魔力值
        /// </summary>
        public static void ModifyMana(CommandArgs args)
        {
            if (args.Parameters.Count < 3)
            {
                args.Player.SendInfoMessage("语法错误，/pm mana <玩家名> <魔力值>");
                return;
            }
            string name = args.Parameters[1];
            string value = args.Parameters[2];
            int mana;
            if (!int.TryParse(value, out mana))
            {
                mana = 0;
            }
            if (mana < 1)
            {
                args.Player.SendSuccessMessage($"输入的魔力值无效, {value}");
                return;
            }

            FoundPlayer found = Utils.GetPlayer(name, out string errMsg);
            if (!found.valid)
            {
                args.Player.SendErrorMessage(errMsg);
                return;
            }

            if (found.online)
            {
                found.plr.TPlayer.statMana = mana;
                NetMessage.SendData((int)PacketTypes.PlayerMana, -1, -1, NetworkText.Empty, found.plr.Index, 0f, 0f, 0f, 0);
                args.Player.SendSuccessMessage($"在线玩家 {found.Name} 的魔力值已修改为 {mana}");
            }
            else
            {
                bool success = StatsDB(args.Player, found.ID, mana, 3);
                if (success)
                {
                    args.Player.SendSuccessMessage($"离线玩家 {found.Name} 的魔力值已修改为 {mana}");
                }
            }
        }

        /// <summary>
        /// 修改魔力上限
        /// </summary>
        public static void ModifyMaxMana(CommandArgs args)
        {
            if (args.Parameters.Count < 3)
            {
                args.Player.SendInfoMessage("语法错误，/pm maxmana <玩家名> <魔力上限>");
                return;
            }
            string name = args.Parameters[1];
            string value = args.Parameters[2];
            int maxMana = 0;
            if (!int.TryParse(value, out maxMana))
            {
                maxMana = 0;
            }
            if (maxMana < 20)
            {
                args.Player.SendErrorMessage("魔力上限，不能低于20!");
                return;
            }

            FoundPlayer found = Utils.GetPlayer(name, out string errMsg);
            if (!found.valid)
            {
                args.Player.SendErrorMessage(errMsg);
                return;
            }

            if (found.online)
            {
                found.plr.TPlayer.statManaMax = maxMana;
                NetMessage.SendData((int)PacketTypes.PlayerMana, -1, -1, NetworkText.Empty, found.plr.Index, 0f, 0f, 0f, 0);
                args.Player.SendSuccessMessage($"在线玩家 {found.Name} 的魔力上限已修改为 {maxMana}");
            }
            else
            {
                bool success = StatsDB(args.Player, found.ID, maxMana, 4);
                if (success)
                {
                    args.Player.SendSuccessMessage($"离线玩家 {found.Name} 的魔力上限已修改为 {maxMana}");
                }
            }
        }

        /// <summary>
        /// 修改db（基础属性）
        /// </summary>
        private static bool StatsDB(TSPlayer op, int id, int vaule, int type)
        {
            var data = TShock.CharacterDB.GetPlayerData(new TSPlayer(-1), id);
            if (data != null)
            {
                IDbConnection db = TShock.CharacterDB.database;

                switch (type)
                {
                    case 1: data.health = vaule; break;
                    case 2: data.maxHealth = vaule; break;
                    case 3: data.mana = vaule; break;
                    case 4: data.maxMana = vaule; break;

                    default:
                        op.SendErrorMessage("type 传值错误");
                        return false;
                }
                try
                {
                    db.Query("UPDATE tsCharacter SET Health = @0, MaxHealth = @1, Mana = @2, MaxMana = @3 WHERE Account = @4;", data.health, data.maxHealth, data.mana, data.maxMana, id);
                    return true;
                }
                catch (Exception ex)
                {
                    TShock.Log.ConsoleError(ex.ToString());
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// 钓鱼任务次数
        /// </summary>
        public static void ModifyQuest(CommandArgs args)
        {
            if (args.Parameters.Count < 3)
            {
                args.Player.SendInfoMessage("语法错误，/pm quest <玩家名> <次数>");
                return;
            }

            string name = args.Parameters[1];
            string value = args.Parameters[2];
            if (!int.TryParse(value, out int num))
            {
                num = -1;
            }
            if (num < 0)
            {
                args.Player.SendErrorMessage("输入的渔夫任务次数无效！");
                return;
            }

            FoundPlayer found = Utils.GetPlayer(name, out string errMsg);
            if (!found.valid)
            {
                args.Player.SendErrorMessage(errMsg);
                return;
            }

            if (found.online)
            {
                // 在线
                found.plr.TPlayer.anglerQuestsFinished = num;
                NetMessage.SendData((int)PacketTypes.AnglerQuest, -1, -1, NetworkText.Empty, found.plr.Index, 0f, 0f, 0f, 0);
                args.Player.SendSuccessMessage($"在线玩家 {found.Name} 的渔夫任务次数已修改为 {num}");
            }
            else
            {
                // 离线
                IDbConnection db = TShock.CharacterDB.database;
                try
                {
                    db.Query("UPDATE tsCharacter SET questsCompleted = @0 WHERE Account = @1;", num, found.ID);
                    args.Player.SendSuccessMessage($"离线线玩家 {found.Name} 的渔夫任务次数已修改为 {num}");
                }
                catch (Exception ex)
                {
                    TShock.Log.ConsoleError(ex.ToString());
                }
            }
        }
        #endregion


        #region 开关增强属性
        /// <summary>
        /// 开关增强属性
        /// </summary>
        public static void ModifyEnhance(CommandArgs args)
        {
            Dictionary<int, string> dict = new()
            {
                {3335, "恶魔之心"},
                {5043, "火把神徽章"},
                {5326, "工匠面包"},
                {5337, "生命水晶"},
                {5338, "埃癸斯果"},
                {5339, "奥术水晶"},
                {5340, "银河珍珠"},
                {5341, "黏性蠕虫"},
                {5342, "珍馐"},
                {5289, "矿车升级包"},
            };

            void Help()
            {
                var lines = new List<string>();
                foreach (var obj in dict)
                {
                    lines.Add($"{(lines.Count % 5 == 0 ? "\n" : "")}[i:{obj.Key}]{obj.Value}({obj.Key})");
                }
                args.Player.SendInfoMessage($"{Utils.Highlight("/pm en")}hance 指令用法：" +
                    "\n/pm en <玩家名> <物品名称>, 开启/关闭 增强属性" +
                    $"\n永久增强类的物品有:{string.Join(", ", lines)}");
            }

            args.Parameters.RemoveAt(0);
            if (args.Parameters.Count < 2)
            {
                Help();
                return;
            }

            switch (args.Parameters[0].ToLowerInvariant())
            {
                case "help": case "h": Help(); return;
            }

            string plrName = args.Parameters[0];
            string itemName = args.Parameters[1];

            // 查找玩家
            FoundPlayer found = Utils.GetPlayer(plrName, out string errMsg);
            if (!found.valid)
            {
                args.Player.SendErrorMessage(errMsg);
                return;
            }

            // 匹配物品
            if (int.TryParse(itemName, out int id) && !dict.ContainsKey(id))
                id = 0;
            if (id == 0)
            {
                foreach (var obj in dict)
                {
                    if (obj.Value == itemName)
                    {
                        id = obj.Key;
                        break;
                    }
                }
            }
            if (id == 0)
            {
                args.Player.SendErrorMessage("输入的物品名称无效！");
                return;
            }


            // 修改数据
            string BDesc(bool _value) { return _value ? "已开启" : "已清除"; }
            bool IntDesc(int? _value) { return _value == 1; }

            bool status = false;
            IDbConnection db = TShock.CharacterDB.database;
            CharacterData data = new();
            // 读取离线玩家数据
            if (!found.online) data.Copy(db, found.Name);

            switch (id)
            {
                // 恶魔之心
                case 3335:
                    if (found.online)
                    {
                        found.plr.TPlayer.extraAccessory = !found.plr.TPlayer.extraAccessory;
                        status = found.plr.TPlayer.extraAccessory;
                    }
                    else
                    {
                        data.extraSlot = data.extraSlot == 1 ? 0 : 1;
                        status = IntDesc(data.extraSlot);
                    }
                    break;

                // 火把神徽章
                case 5043:
                    if (found.online)
                    {
                        found.plr.TPlayer.unlockedBiomeTorches = !found.plr.TPlayer.unlockedBiomeTorches;
                        status = found.plr.TPlayer.unlockedBiomeTorches;
                    }
                    else
                    {
                        data.unlockedBiomeTorches = data.unlockedBiomeTorches == 1 ? 0 : 1;
                        status = IntDesc(data.unlockedBiomeTorches);
                    }
                    break;

                // 工匠面包
                case 5326:
                    if (found.online)
                    {
                        found.plr.TPlayer.ateArtisanBread = !found.plr.TPlayer.ateArtisanBread;
                        status = found.plr.TPlayer.ateArtisanBread;
                    }
                    else
                    {
                        data.ateArtisanBread = data.ateArtisanBread == 1 ? 0 : 1;
                        status = IntDesc(data.ateArtisanBread);
                    }
                    break;

                // 生命水晶	
                case 5337:
                    if (found.online)
                    {
                        found.plr.TPlayer.usedAegisCrystal = !found.plr.TPlayer.usedAegisCrystal;
                        status = found.plr.TPlayer.usedAegisCrystal;
                    }
                    else
                    {
                        data.usedAegisCrystal = data.usedAegisCrystal == 1 ? 0 : 1;
                        status = IntDesc(data.usedAegisCrystal);
                    }
                    break;

                // 埃癸斯果
                case 5338:
                    if (found.online)
                    {
                        found.plr.TPlayer.usedAegisFruit = !found.plr.TPlayer.usedAegisFruit;
                        status = found.plr.TPlayer.usedAegisFruit;
                    }
                    else
                    {
                        data.usedAegisFruit = data.usedAegisFruit == 1 ? 0 : 1;
                        status = IntDesc(data.usedAegisFruit);
                    }
                    break;

                // 奥术水晶
                case 5339:
                    if (found.online)
                    {
                        found.plr.TPlayer.usedArcaneCrystal = !found.plr.TPlayer.usedArcaneCrystal;
                        status = found.plr.TPlayer.usedArcaneCrystal;
                    }
                    else
                    {
                        data.usedArcaneCrystal = data.usedArcaneCrystal == 1 ? 0 : 1;
                        status = IntDesc(data.usedArcaneCrystal);
                    }
                    break;

                // 银河珍珠
                case 5340:
                    if (found.online)
                    {
                        found.plr.TPlayer.usedGalaxyPearl = !found.plr.TPlayer.usedGalaxyPearl;
                        status = found.plr.TPlayer.usedGalaxyPearl;
                    }
                    else
                    {
                        data.usedGalaxyPearl = data.usedGalaxyPearl == 1 ? 0 : 1;
                        status = IntDesc(data.usedGalaxyPearl);
                    }
                    break;

                // 黏性蠕虫
                case 5341:
                    if (found.online)
                    {
                        found.plr.TPlayer.usedGummyWorm = !found.plr.TPlayer.usedGummyWorm;
                        status = found.plr.TPlayer.usedGummyWorm;
                    }
                    else
                    {
                        data.usedGummyWorm = data.usedGummyWorm == 1 ? 0 : 1;
                        status = IntDesc(data.usedGummyWorm);
                    }
                    break;

                // 珍馐
                case 5342:
                    if (found.online)
                    {
                        found.plr.TPlayer.usedAmbrosia = !found.plr.TPlayer.usedAmbrosia;
                        status = found.plr.TPlayer.usedAmbrosia;
                    }
                    else
                    {
                        data.usedAmbrosia = data.usedAmbrosia == 1 ? 0 : 1;
                        status = IntDesc(data.usedAmbrosia);
                    }
                    break;

                // 矿车升级包
                case 5289:
                    if (found.online)
                    {
                        found.plr.TPlayer.unlockedSuperCart = !found.plr.TPlayer.unlockedSuperCart;
                        status = found.plr.TPlayer.unlockedSuperCart;
                    }
                    else
                    {
                        data.unlockedSuperCart = data.unlockedSuperCart == 1 ? 0 : 1;
                        status = IntDesc(data.unlockedSuperCart);
                    }
                    break;
            }

            if (found.online)
            {
                // 发消息同步状态
                NetMessage.SendData(4, -1, -1, NetworkText.FromLiteral(found.plr.Name), found.plr.Index, 0f, 0f, 0f, 0);
                NetMessage.SendData(4, found.plr.Index, -1, NetworkText.FromLiteral(found.plr.Name), found.plr.Index, 0f, 0f, 0f, 0);
                args.Player.SendSuccessMessage($"已将在线玩家 {Utils.Highlight(found.Name)} 的[i:{id}]{dict[id]}属性设置为 {BDesc(status)}");
            }
            else
            {
                // 写入数据库
                try
                {
                    db.Query("UPDATE tsCharacter SET extraSlot = @1, unlockedBiomeTorches = @2, ateArtisanBread = @3, usedAegisCrystal = @4, usedAegisFruit = @5, usedArcaneCrystal = @6, usedGalaxyPearl = @7, usedGummyWorm = @8, usedAmbrosia = @9, unlockedSuperCart = @10 WHERE Account = @0;",
                        found.ID,
                        data.extraSlot,
                        data.unlockedBiomeTorches,
                        data.ateArtisanBread,
                        data.usedAegisCrystal,
                        data.usedAegisFruit,
                        data.usedArcaneCrystal,
                        data.usedGalaxyPearl,
                        data.usedGummyWorm,
                        data.usedAmbrosia,
                        data.unlockedSuperCart
                    );
                    args.Player.SendSuccessMessage($"已将离线玩家 {Utils.Highlight(found.Name)} 的[i:{id}]{dict[id]}属性设置为 {BDesc(status)}");
                }
                catch (Exception ex)
                {
                    TShock.Log.ConsoleError(ex.ToString());
                }
            }
        }
        #endregion



    }
}