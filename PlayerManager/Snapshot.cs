using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.Localization;
using TShockAPI;
using TShockAPI.DB;

namespace Plugin
{
    public class Snapshot
    {
        private static BConfig _config;

        private static string save_dir = Path.Combine(TShock.SavePath, "PlayerManager");
        private static string config_file = Path.Combine(TShock.SavePath, "PlayerManager", "BagSnapshot.json");

        public static void load()
        {
            _config = BConfig.Load(config_file);
        }
        private static void save()
        {
            if (!Directory.Exists(save_dir))
                Directory.CreateDirectory(save_dir);
            File.WriteAllText(config_file, JsonConvert.SerializeObject(_config, Formatting.Indented));
        }

        #region Bag Snapshot
        public static void BagSnapshot(CommandArgs args)
        {
            if (args.Parameters.Count<string>() == 0)
            {
                args.Player.SendErrorMessage("语法错误，/bs help 可查询帮助信息");
                return;
            }

            if (args.Parameters[0].ToLowerInvariant()=="help")
            {
                args.Player.SendInfoMessage("==背包快照==");
                args.Player.SendInfoMessage("/bs look <玩家名>, 查看快照");
                args.Player.SendInfoMessage("/bs add <玩家名>, 添加快照");
                args.Player.SendInfoMessage("/bs del <玩家名>, 删除快照");
                args.Player.SendInfoMessage("/bs recover <玩家名>, 恢复快照");
                args.Player.SendInfoMessage("/bs addall, 添加快照-当前所有在线玩家");
                args.Player.SendInfoMessage("/bs recoverall, 恢复快照-当前所有在线玩家");
                args.Player.SendInfoMessage("/savemybag, 添加自己到快照");
                args.Player.SendInfoMessage("/lookmybag, 查看自己的快照");
                args.Player.SendInfoMessage("/pm help, 玩家管理");
                return;
            }

            if (!Main.ServerSideCharacter)
			{
				args.Player.SendErrorMessage("强制开荒 未开启！");
				return;
			}

            // ----------
            //  多玩家
            if(args.Parameters.Count > 0)
            {
                switch (args.Parameters[0].ToLowerInvariant())
                {
                    // 添加记录 当前在线的所有玩家
                    case "aa":
                    case "addall":
                        AddAll(args.Player);
                        break;

                    // 恢复记录，当前在线的所有玩家
                    case "ra":
                    case "recoverall":
                        RecoverAll(args.Player);
                        break;
                }
            }


            // ----------
            // 指定玩家
            if(args.Parameters.Count > 1)
            {
                var username = args.Parameters[1];
                var found = Util.GetPlayer(args.Player, username);
                var userid = found.ID;
                if( !found.valid )
                    return;

                switch (args.Parameters[0].ToLowerInvariant())
                {
                    // 查看记录
                    case "look":
                        if ( _config.records.ContainsKey(username) )
                        {
                            var record = _config.records[username];
                            if( record.Bak!="" )
                                Look.LookInventoryStr(args.Player, record.Bak);
                            else
                                args.Player.SendErrorMessage("背包快照：查看失败，本地没有记录！（" + username+ "）");
                        } else {
                            args.Player.SendErrorMessage("背包快照：查看失败，本地没有记录！（" + username+ "）");
                        }
                        break;


                    //  删除
                    case "del":
                    case "remove":
                        if ( _config.records.ContainsKey(username) )
                        {
                            _config.records.Remove(username);
                            save();
                            args.Player.SendSuccessMessage("背包快照：移除成功！（" + username+ "）");
                        } else {
                            args.Player.SendErrorMessage("背包快照：移除失败，本地没有记录！（" + username+ "）");
                        }
                        break;


                    // 添加记录
                    case "a":
                    case "add":
                        Add(args.Player, found);
                        break;


                    // 恢复记录
                    case "r":
                    case "recover":
                        Recover(args.Player, found);
                        break;
                }
            }

        }
        #endregion

        #region all
        // 添加全部
        private static void AddAll(TSPlayer op)
        {
            Boolean hasPlayer = false;
            foreach (var p in TShock.Players)
            {
                if( p!=null ){
                    hasPlayer = true;

                    var username = p.Name;
                    var found = Util.GetPlayer(op, username);
                    var userid = found.ID;
                    if( !found.valid )
                        continue;
                    Add(op, found);
                }
            }
            if( !hasPlayer )
            {
                op.SendErrorMessage("没有玩家在线");
            }
        }

        // 恢复全部
        private static void RecoverAll(TSPlayer op)
        {
            Boolean hasPlayer = false;
            foreach (var p in TShock.Players)
            {
                if( p!=null ){
                    hasPlayer = true;

                    var username = p.Name;
                    var found = Util.GetPlayer(op, username);
                    var userid = found.ID;
                    if( !found.valid )
                        continue;
                    Recover(op, found);
                }
            }
            if( !hasPlayer )
            {
                op.SendErrorMessage("没有玩家在线");
            }
        }
        #endregion


        #region 普通用户
        public static void SaveMyBag(CommandArgs args)
        {
            if(!args.Player.RealPlayer)
            {
                args.Player.SendErrorMessage("需在游戏内使用该指令。");
                return;
            }

            var found = new FoundPlayer();
            found.SetOnline(args.Player);
            Add(args.Player, found);
        }

        public static void LookMyBag(CommandArgs args)
        {
            if(!args.Player.RealPlayer)
            {
                args.Player.SendErrorMessage("需在游戏内使用该指令。");
                return;
            }

            var username = args.Player.Name;
            if ( _config.records.ContainsKey(username) )
            {
                var record = _config.records[username];
                if( record.Bak!="" )
                    Look.LookInventoryStr(args.Player, record.Bak);
                else
                    args.Player.SendErrorMessage("背包快照：查看失败，本地记录为空！");
            } else {
                args.Player.SendErrorMessage("背包快照：查看失败，本地没有记录！");
            }
        }
        #endregion

        #region 添加
        private static void Add(TSPlayer op, FoundPlayer found)
        {
            var invstr = "";
            var username = found.Name;
            var userid = found.ID;
            var record = new BConfigSub();

            if ( found.online )
            {
                invstr = String.Join("~", found.plr.PlayerData.inventory);
                // op.SendInfoMessage("online");
            }
            else
            {
                // op.SendInfoMessage("db");
                var db = TShock.CharacterDB.database;
                using( var reader = db.QueryReader("SELECT * FROM tsCharacter WHERE Account=@0", userid) )
                {
                    if( reader.Read() ){
                        invstr = reader.Get<string>("Inventory");
                    } else {
                        op.SendErrorMessage("背包快照：添加失败！（" + username+ "）");
                    }
                }
            }

            if (invstr!=""){
                if ( _config.records.ContainsKey(username) )
                {
                    record = _config.records[username];
                    // record.name = username;

                    // 保留最近3条
                    record.Bak3 = record.Bak2;
                    record.Bak2 = record.Bak;
                    record.Bak = invstr;
                } else {
                    record.name = username;
                    record.Bak = invstr;

                    _config.records.Add(username, record);
                }
                save();
                op.SendSuccessMessage("背包快照：已添加！（" + username+ "）");
            }
        }
        #endregion

        #region 恢复
        private static void Recover(TSPlayer op, FoundPlayer found)
        {
            var username = found.Name;
            var userid = found.ID;

            if ( !_config.records.ContainsKey(username) )
            {
                op.SendErrorMessage("背包快照：恢复失败，本地无记录！（" + username+ "）");
                return;
            }

            var record = _config.records[username];
            var invstr = record.Bak;
            var invarr = invstr.Split('~');
            if( invarr.Length !=260 ){
                op.SendErrorMessage("背包快照：快照配置有误！（" + username+ "）");
                return;
            }

            if( found.online )
            {
                // 在线
                RecoverInventory(found.plr, invarr);
                op.SendSuccessMessage("背包快照：背包已恢复！（" + username+ "）");
            } else {
                // 离线
                try
                {
                    var db = TShock.CharacterDB.database;
                    db.Query("UPDATE tsCharacter SET Inventory = @0 WHERE Account = @1;", invstr, userid);
                    op.SendSuccessMessage("背包快照：背包已恢复！（" + username+ "）（该玩家已离线）");
                }
                catch (Exception ex)
                {
                    op.SendErrorMessage("背包快照：恢复时遇到错误!");
                    TShock.Log.ConsoleError(ex.ToString());
                }
            }
        }

        // 恢复背包
		private static void RecoverInventory(TSPlayer player, string [] invarr)
		{
            // 参考
            // PlayerData.cs
            // PlayerData.restoreCharacter
			for (int i = 0; i < NetItem.MaxInventory; i++)
			{
				if (i < NetItem.InventorySlots)
				{
                    // 主背包
					player.TPlayer.inventory[i] = NetItem2Item(invarr[i]);
				}
				else if (i < NetItem.InventorySlots + NetItem.ArmorSlots)
				{
                    // 盔甲 和 饰品槽
					var index = i - NetItem.InventorySlots;
					player.TPlayer.armor[index] = NetItem2Item(invarr[i]);
				}
				else if (i < NetItem.InventorySlots + NetItem.ArmorSlots + NetItem.DyeSlots)
				{
                    // 染料
					var index = i - (NetItem.InventorySlots + NetItem.ArmorSlots);
					player.TPlayer.dye[index] = NetItem2Item(invarr[i]);
				}
				else if (i < NetItem.InventorySlots + NetItem.ArmorSlots + NetItem.DyeSlots + NetItem.MiscEquipSlots)
				{
                    //工具栏
					var index = i - (NetItem.InventorySlots + NetItem.ArmorSlots + NetItem.DyeSlots);
					player.TPlayer.miscEquips[index] = NetItem2Item(invarr[i]);
				}
				else if (i < NetItem.InventorySlots + NetItem.ArmorSlots + NetItem.DyeSlots + NetItem.MiscEquipSlots + NetItem.MiscDyeSlots)
				{
                    //工具栏-染料
					var index = i - (NetItem.InventorySlots + NetItem.ArmorSlots + NetItem.DyeSlots + NetItem.MiscEquipSlots);
					player.TPlayer.miscDyes[index] = NetItem2Item(invarr[i]);
				}
                else if(i==179)
                {
                    // 回收站
					player.TPlayer.trashItem = NetItem2Item(invarr[i]);
                }
				else if (i < NetItem.InventorySlots + NetItem.ArmorSlots + NetItem.DyeSlots + NetItem.MiscEquipSlots + NetItem.MiscDyeSlots + NetItem.PiggySlots) //piggy Bank
				{
					//var index = i - (NetItem.InventorySlots + NetItem.ArmorSlots + NetItem.DyeSlots + NetItem.MiscEquipSlots + NetItem.MiscDyeSlots);
					//player.TPlayer.bank.item[index].netDefaults(0);
				}
				else if (i < NetItem.InventorySlots + NetItem.ArmorSlots + NetItem.DyeSlots + NetItem.MiscEquipSlots + NetItem.MiscDyeSlots + NetItem.PiggySlots + NetItem.SafeSlots) //safe Bank
				{
					//var index = i - (NetItem.InventorySlots + NetItem.ArmorSlots + NetItem.DyeSlots + NetItem.MiscEquipSlots + NetItem.MiscDyeSlots + NetItem.PiggySlots);
					//player.TPlayer.bank2.item[index].netDefaults(0);
				}
				else if (i < NetItem.InventorySlots + NetItem.ArmorSlots + NetItem.DyeSlots + NetItem.MiscEquipSlots + NetItem.MiscDyeSlots + NetItem.PiggySlots + NetItem.SafeSlots + NetItem.ForgeSlots) //Defender's Forge
				{
					//var index = i - (NetItem.InventorySlots + NetItem.ArmorSlots + NetItem.DyeSlots + NetItem.MiscEquipSlots + NetItem.MiscDyeSlots + NetItem.PiggySlots + NetItem.SafeSlots);
					//player.TPlayer.bank3.item[index].netDefaults(0);
				}
				else if (i < NetItem.InventorySlots + NetItem.ArmorSlots + NetItem.DyeSlots + NetItem.MiscEquipSlots + NetItem.MiscDyeSlots + NetItem.PiggySlots + NetItem.SafeSlots + NetItem.ForgeSlots+NetItem.VoidSlots) //Void bank
				{
					//var index = i - (NetItem.InventorySlots + NetItem.ArmorSlots + NetItem.DyeSlots + NetItem.MiscEquipSlots + NetItem.MiscDyeSlots + NetItem.PiggySlots + NetItem.SafeSlots + NetItem.ForgeSlots);
					//player.TPlayer.bank3.item[index].netDefaults(0);
				}
				else
				{
                    // Console.WriteLine("有漏网之鱼："+i);
				}
			}

			for (int k = 0; k < NetItem.PiggySlots; k++)
			{
                // 储蓄罐
				player.TPlayer.bank.item[k] = NetItem2Item(invarr[99+k]);
                // Console.WriteLine("test:PiggySlots"+NetItem.PiggySlots+""+k);
			}
			for (int k = 0; k < NetItem.SafeSlots; k++)
			{
                // 保险箱
				player.TPlayer.bank2.item[k] = NetItem2Item(invarr[139+k]);
                // Console.WriteLine("test:SafeSlots"+NetItem.SafeSlots+""+k);
			}
			for (int k = 0; k < NetItem.ForgeSlots; k++)
			{
                // 护卫熔炉
				player.TPlayer.bank3.item[k] = NetItem2Item(invarr[180+k]);
                // Console.WriteLine("test:ForgeSlots"+NetItem.ForgeSlots+""+k);
			}
			for (int k = 0; k < NetItem.VoidSlots; k++)
			{
                // 虚空袋
				player.TPlayer.bank4.item[k] = NetItem2Item(invarr[220+k]);
                // Console.WriteLine("test:VoidSlots"+NetItem.VoidSlots+""+k);
			}

            //  ==========
            // 消息处理

			float slot = 0f;
			for (int k = 0; k < NetItem.InventorySlots; k++)
			{
				NetMessage.SendData(5, -1, -1, NetworkText.FromLiteral(Main.player[player.Index].inventory[k].Name), player.Index, slot, (float)Main.player[player.Index].inventory[k].prefix);
				slot++;
			}
			for (int k = 0; k < NetItem.ArmorSlots; k++)
			{
				NetMessage.SendData(5, -1, -1, NetworkText.FromLiteral(Main.player[player.Index].armor[k].Name), player.Index, slot, (float)Main.player[player.Index].armor[k].prefix);
				slot++;
			}
			for (int k = 0; k < NetItem.DyeSlots; k++)
			{
				NetMessage.SendData(5, -1, -1, NetworkText.FromLiteral(Main.player[player.Index].dye[k].Name), player.Index, slot, (float)Main.player[player.Index].dye[k].prefix);
				slot++;
			}
			for (int k = 0; k < NetItem.MiscEquipSlots; k++)
			{
				NetMessage.SendData(5, -1, -1, NetworkText.FromLiteral(Main.player[player.Index].miscEquips[k].Name), player.Index, slot, (float)Main.player[player.Index].miscEquips[k].prefix);
				slot++;
			}
			for (int k = 0; k < NetItem.MiscDyeSlots; k++)
			{
				NetMessage.SendData(5, -1, -1, NetworkText.FromLiteral(Main.player[player.Index].miscDyes[k].Name), player.Index, slot, (float)Main.player[player.Index].miscDyes[k].prefix);
				slot++;
			}
			for (int k = 0; k < NetItem.PiggySlots; k++)
			{
				NetMessage.SendData(5, -1, -1, NetworkText.FromLiteral(Main.player[player.Index].bank.item[k].Name), player.Index, slot, (float)Main.player[player.Index].bank.item[k].prefix);
				slot++;
			}
			for (int k = 0; k < NetItem.SafeSlots; k++)
			{
				NetMessage.SendData(5, -1, -1, NetworkText.FromLiteral(Main.player[player.Index].bank2.item[k].Name), player.Index, slot, (float)Main.player[player.Index].bank2.item[k].prefix);
				slot++;
			}
			NetMessage.SendData(5, -1, -1, NetworkText.FromLiteral(Main.player[player.Index].trashItem.Name), player.Index, slot++, (float)Main.player[player.Index].trashItem.prefix);
			for (int k = 0; k < NetItem.ForgeSlots; k++)
			{
				NetMessage.SendData(5, -1, -1, NetworkText.FromLiteral(Main.player[player.Index].bank3.item[k].Name), player.Index, slot, (float)Main.player[player.Index].bank3.item[k].prefix);
				slot++;
			}
			for (int k = 0; k < NetItem.VoidSlots; k++)
			{
				NetMessage.SendData(5, -1, -1, NetworkText.FromLiteral(Main.player[player.Index].bank4.item[k].Name), player.Index, slot, (float)Main.player[player.Index].bank4.item[k].prefix);
				slot++;
			}

			for (int k = 0; k < Player.maxBuffs; k++)
			{
                // buff
				player.TPlayer.buffType[k] = 0;
			}
			NetMessage.SendData((int)PacketTypes.PlayerInfo, -1, -1, NetworkText.FromLiteral(player.Name), player.Index, 0f, 0f, 0f, 0);
			NetMessage.SendData((int)PacketTypes.PlayerMana, -1, -1, NetworkText.Empty, player.Index, 0f, 0f, 0f, 0);
			NetMessage.SendData((int)PacketTypes.PlayerHp, -1, -1, NetworkText.Empty, player.Index, 0f, 0f, 0f, 0);
			NetMessage.SendData((int)PacketTypes.PlayerBuff, -1, -1, NetworkText.Empty, player.Index, 0f, 0f, 0f, 0);
            //  ==========

			slot = 0f;
			for (int k = 0; k < NetItem.InventorySlots; k++)
			{
				NetMessage.SendData(5, player.Index, -1, NetworkText.FromLiteral(Main.player[player.Index].inventory[k].Name), player.Index, slot, (float)Main.player[player.Index].inventory[k].prefix);
				slot++;
			}
			for (int k = 0; k < NetItem.ArmorSlots; k++)
			{
				NetMessage.SendData(5, player.Index, -1, NetworkText.FromLiteral(Main.player[player.Index].armor[k].Name), player.Index, slot, (float)Main.player[player.Index].armor[k].prefix);
				slot++;
			}
			for (int k = 0; k < NetItem.DyeSlots; k++)
			{
				NetMessage.SendData(5, player.Index, -1, NetworkText.FromLiteral(Main.player[player.Index].dye[k].Name), player.Index, slot, (float)Main.player[player.Index].dye[k].prefix);
				slot++;
			}
			for (int k = 0; k < NetItem.MiscEquipSlots; k++)
			{
				NetMessage.SendData(5, player.Index, -1, NetworkText.FromLiteral(Main.player[player.Index].miscEquips[k].Name), player.Index, slot, (float)Main.player[player.Index].miscEquips[k].prefix);
				slot++;
			}
			for (int k = 0; k < NetItem.MiscDyeSlots; k++)
			{
				NetMessage.SendData(5, player.Index, -1, NetworkText.FromLiteral(Main.player[player.Index].miscDyes[k].Name), player.Index, slot, (float)Main.player[player.Index].miscDyes[k].prefix);
				slot++;
			}
			for (int k = 0; k < NetItem.PiggySlots; k++)
			{
				NetMessage.SendData(5, player.Index, -1, NetworkText.FromLiteral(Main.player[player.Index].bank.item[k].Name), player.Index, slot, (float)Main.player[player.Index].bank.item[k].prefix);
				slot++;
			}
			for (int k = 0; k < NetItem.SafeSlots; k++)
			{
				NetMessage.SendData(5, player.Index, -1, NetworkText.FromLiteral(Main.player[player.Index].bank2.item[k].Name), player.Index, slot, (float)Main.player[player.Index].bank2.item[k].prefix);
				slot++;
			}
			NetMessage.SendData(5, player.Index, -1, NetworkText.FromLiteral(Main.player[player.Index].trashItem.Name), player.Index, slot++, (float)Main.player[player.Index].trashItem.prefix);
			for (int k = 0; k < NetItem.ForgeSlots; k++)
			{
				NetMessage.SendData(5, player.Index, -1, NetworkText.FromLiteral(Main.player[player.Index].bank3.item[k].Name), player.Index, slot, (float)Main.player[player.Index].bank3.item[k].prefix);
				slot++;
			}
			for (int k = 0; k < NetItem.VoidSlots; k++)
			{
				NetMessage.SendData(5, player.Index, -1, NetworkText.FromLiteral(Main.player[player.Index].bank4.item[k].Name), player.Index, slot, (float)Main.player[player.Index].bank4.item[k].prefix);
				slot++;
			}

			for (int k = 0; k < Player.maxBuffs; k++)
			{
				player.TPlayer.buffType[k] = 0;
			}

			NetMessage.SendData((int)PacketTypes.PlayerInfo, player.Index, -1, NetworkText.FromLiteral(player.Name), player.Index, 0f, 0f, 0f, 0);
			NetMessage.SendData((int)PacketTypes.PlayerMana, player.Index, -1, NetworkText.Empty, player.Index, 0f, 0f, 0f, 0);
			NetMessage.SendData((int)PacketTypes.PlayerHp, player.Index, -1, NetworkText.Empty, player.Index, 0f, 0f, 0f, 0);
			NetMessage.SendData((int)PacketTypes.PlayerBuff, player.Index, -1, NetworkText.Empty, player.Index, 0f, 0f, 0f, 0);

			NetMessage.SendData(39, player.Index, -1, NetworkText.Empty, 400);
		}

        private static Item NetItem2Item(string invstr)
        {
            var item = NetItem.Parse(invstr);
            Item give;
            give = TShock.Utils.GetItemById(item.NetId);
            give.stack = item.Stack;
            give.prefix = item.PrefixId;

            return give;
        }
        #endregion

    }

    class BConfig
    {
         public Dictionary<string, BConfigSub> records = new Dictionary<string, BConfigSub>();

        public static BConfig Load(string path)
        {
            if (File.Exists(path))
                return JsonConvert.DeserializeObject<BConfig>(File.ReadAllText(path));
            else
                return new BConfig() {};
        }
    }

    class BConfigSub{
        public string name = "";
        public string Bak="";
        public string Bak2="";
        public string Bak3="";
    }

}