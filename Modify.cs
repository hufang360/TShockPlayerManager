using System;
using System.Data;
using Terraria;
using Terraria.Localization;
using TShockAPI;
using TShockAPI.DB;

namespace Plugin
{
    public class Modify
    {
        #region modify hp mana
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
            if (  !int.TryParse(value, out hp) ){
                hp = 0;
            }
            if (hp<1){
                args.Player.SendSuccessMessage($"输入的生命值无效, {value}");
                return;
            }

            FoundPlayer plr  = Util.GetPlayer(args.Player, name);
            if( plr.ID==-1 )
                return;

            if( plr.online )
            {
                plr.plr.TPlayer.statLife = hp;
                NetMessage.SendData((int)PacketTypes.PlayerHp, -1, -1, NetworkText.Empty, plr.plr.Index, 0f, 0f, 0f, 0);
                args.Player.SendSuccessMessage($"{plr.Name} 的生命值已修改为 {hp}");
            } else {
                bool success = StatsDB(args.Player, plr.ID, hp, 1);
                if( success ){
                    args.Player.SendSuccessMessage($"{plr.Name} 的生命值已修改为 {hp}");
                }
            }
        }

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
            if (  !int.TryParse(value, out maxhp) ){
                maxhp = 0;
            }
            if (maxhp<100){
                args.Player.SendErrorMessage("生命上限，不能低于100!");
                return;
            }

            FoundPlayer plr  = Util.GetPlayer(args.Player, name);
            if( plr.ID==-1 )
                return;

            if( plr.online )
            {
                plr.plr.TPlayer.statLifeMax = maxhp;
                NetMessage.SendData((int)PacketTypes.PlayerHp, -1, -1, NetworkText.Empty, plr.plr.Index, 0f, 0f, 0f, 0);
                args.Player.SendSuccessMessage($"{plr.Name} 的生命上限已修改为 {maxhp}");
            } else {
                bool success = StatsDB(args.Player, plr.ID, maxhp, 2);
                if( success ){
                    args.Player.SendSuccessMessage($"{plr.Name} 的生命上限已修改为 {maxhp}");
                }
            }
        }
        public static void ModifyMana(CommandArgs args)
        {
            if (args.Parameters.Count < 3)
            {
                args.Player.SendInfoMessage("语法错误，/pm mana <玩家名> <魔力值>");
                return;
            }
            string name = args.Parameters[1];
            string value = args.Parameters[2];
            int mana = 0;
            if (  !int.TryParse(value, out mana) ){
                mana = 0;
            }
            if (mana<1){
                args.Player.SendSuccessMessage($"输入的魔力值无效, {value}");
                return;
            }

            FoundPlayer plr  = Util.GetPlayer(args.Player, name);
            if( plr.ID==-1 )
                return;

            if( plr.online )
            {
                plr.plr.TPlayer.statMana = mana;
                NetMessage.SendData((int)PacketTypes.PlayerMana, -1, -1, NetworkText.Empty, plr.plr.Index, 0f, 0f, 0f, 0);
                args.Player.SendSuccessMessage($"{plr.Name} 的魔力值已修改为 {mana}");
            } else {
                bool success = StatsDB(args.Player, plr.ID, mana, 3);
                if( success ){
                    args.Player.SendSuccessMessage($"{plr.Name} 的魔力值已修改为 {mana}");
                }
            }
        }

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
            if (  !int.TryParse(value, out maxMana) ){
                maxMana = 0;
            }
            if (maxMana<20){
                args.Player.SendErrorMessage("魔力上限，不能低于20!");
                return;
            }

            FoundPlayer plr  = Util.GetPlayer(args.Player, name);
            if( plr.ID==-1 )
                return;
                
            if( plr.online )
            {
                plr.plr.TPlayer.statManaMax = maxMana;
                NetMessage.SendData((int)PacketTypes.PlayerMana, -1, -1, NetworkText.Empty, plr.plr.Index, 0f, 0f, 0f, 0);
                args.Player.SendSuccessMessage($"{plr.Name} 的魔力上限已修改为 {maxMana}");
            } else {
                bool success = StatsDB(args.Player, plr.ID, maxMana, 4);
                if( success ){
                    args.Player.SendSuccessMessage($"{plr.Name} 的魔力上限已修改为 {maxMana}");
                }
            }
        }

        private static bool StatsDB(TSPlayer op, int id, int vaule, int type){
            var data = TShock.CharacterDB.GetPlayerData(new TSPlayer(-1), id);
            if (data != null)
            {
                IDbConnection db = TShock.CharacterDB.database;

                switch (type)
                {
                    case 1:
                        data.health = vaule;
                        break;
                    case 2:
                        data.maxHealth = vaule;
                        break;
                    case 3:
                        data.mana = vaule;
                        break;
                    case 4:
                        data.maxMana = vaule;
                        break;

                    default:
                        op.SendErrorMessage("type 传值错误");
                        return false;
                }
                try
                {
                    db.Query("UPDATE tsCharacter SET Health = @0, MaxHealth = @1, Mana = @2, MaxMana = @3 WHERE Account = @4;", data.health, data.maxHealth, data.mana, data.maxMana, id);
                    // args.Player.SendSuccessMessage(plrDB.Name + "'s stats have been reset!");
                    return true;
                }
                catch (Exception ex)
                {
                    op.SendErrorMessage("An error occurred while resetting!");
                    TShock.Log.ConsoleError(ex.ToString());
                    return false;
                }
            }
            return false;
        }
        #endregion
    }
}