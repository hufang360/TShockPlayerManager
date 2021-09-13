using System.Collections.Generic;
using System;
using System.Data;
using System.Linq;
using Terraria;
using TShockAPI;
using TShockAPI.DB;

namespace Plugin
{
    public class Snapshot
    {

        public static void PlayerSnapshot(CommandArgs args)
        {
            if (args.Parameters.Count<string>() == 0)
            {
                args.Player.SendErrorMessage("语法错误，/ps help 可查询帮助信息");
                return;
            }

            if (args.Parameters[0].ToLowerInvariant()=="help")
            {
                args.Player.SendInfoMessage("/ps look <玩家名>, 查看快照");
                args.Player.SendInfoMessage("/ps add <玩家名>, 添加快照");
                args.Player.SendInfoMessage("/ps del <玩家名>, 删除快照");
                args.Player.SendInfoMessage("/ps recover <玩家名>, 恢复快照");
                args.Player.SendInfoMessage("/pm help, 玩家管理1");
                return;
            }
            
            if (!Main.ServerSideCharacter)
			{
				args.Player.SendErrorMessage("强制开荒 未开启.");
				return;
			}

            var username = args.Parameters[1];
            TSPlayer player = args.Player;
            IDbConnection db = TShock.CharacterDB.database;
            bool online = true;
            int userid = 0;

            //List<TSPlayer> players = TShock.Utils.FindPlayer(username);
            List<TSPlayer> players = new List<TSPlayer>();
            foreach (var p in TShock.Players)
            {
                if (p ==null)
                    continue;
                if (p.Name == username) {
                    players.Append(p);
                }
            }

            if (players.Count < 1) //if player not found online
            {
                online = false;
                player.SendWarningMessage("Player not found online. Searching database...");
            }
            else if (players.Count > 1)
            {
                string list = string.Join(", ", players.Select(p => p.Name));
                player.SendErrorMessage("Multiple players found: " + list);
                return;
            }

            if (!online)
            {
                if (TShock.UserAccounts.GetUserAccountByName(username) == null)
                {
                    player.SendErrorMessage("Username \"{0}\" not found in database.", username);
                    return;
                }
                else
                {
                    userid = TShock.UserAccounts.GetUserAccountByName(username).ID;
                }
            }
            switch (args.Parameters[0].ToLowerInvariant())
            {
                default:
                    args.Player.SendErrorMessage("语法错误！");
                    break;

                // 帮助
                case "help":
                    return;

                case "look":
                    // LookSnapshot(args);
                    break;
                
                case "add":

                    // ing
                    var reader = db.QueryReader("select Inventory from tsCharacter where Account=@", userid);
                    Console.WriteLine(reader.Get<string>("Inventory"));
                    break;
                
                case "del":
                case "remove":
                    break;
                
                case "recover":
                case "r":
                    Recover(args);
                    break;
            }
        }

        private static void Add(CommandArgs args)
        {
            if (args.Parameters.Count<string>() == 1)
            {
                args.Player.SendErrorMessage("需输入玩家名");
                return;
            }
            var username = args.Parameters[1];
            TSPlayer player = args.Player;
            IDbConnection db = TShock.CharacterDB.database;
            bool online = true;
            int userid = 0;
        }

        private static void Recover(CommandArgs args)
        {
            if (args.Parameters.Count<string>() == 1)
            {
                args.Player.SendErrorMessage("需输入玩家名");
                return;
            }

            

            // try
            // {
            //     if (online)
            //     {
            //         // ResetInventory(players[0]);
            //         player.SendSuccessMessage(players[0].Name + "'s inventory has been reset!");
            //         players[0].SendInfoMessage("Your inventory has been reset!");
            //     }
            //     else
            //     {
            //         var inventory = new StringBuilder();
            //         for (int i = 0; i < NetItem.MaxInventory; i++)
            //         {
            //             if (i > 0)
            //             {
            //                 inventory.Append("~");
            //             }
            //             if (i < TShock.ServerSideCharacterConfig.Settings.StartingInventory.Count)
            //             {
            //                 var item = TShock.ServerSideCharacterConfig.Settings.StartingInventory[i];
            //                 inventory.Append(item.NetId).Append(',').Append(item.Stack).Append(',').Append(item.PrefixId);
            //             }
            //             else
            //             {
            //                 inventory.Append("0,0,0");
            //             }
            //         }
            //         string initialItems = inventory.ToString();
            //         db.Query("UPDATE tsCharacter SET Inventory = @0 WHERE Account = @1;", initialItems, userid);
            //         player.SendSuccessMessage(username + "'s inventory has been reset!");
            //     }
            // }
            // catch (Exception ex)
            // {
            //     player.SendErrorMessage("An error occurred while resetting!");
            //     TShock.Log.ConsoleError(ex.ToString());
            // }

        }
        private static void LookSnapshot(CommandArgs args)
        {
            if (args.Parameters.Count<string>() == 1)
            {
                args.Player.SendErrorMessage("需输入玩家名");
                return;
            }
        }

    }
}