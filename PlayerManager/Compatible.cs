using Terraria;
using TShockAPI;


namespace Plugin
{
    class Compatible
    {
        public static void ShowPlrUnlockedBiomeTorches(TSPlayer op, Player plr)
        {
            // 1.4.0.5 无法获取 UsingBiomeTorches 属性
            op.SendInfoMessage("生态火把：{0}", plr.UsingBiomeTorches ? "已激活" : "未激活");
        }

        public static void ShowDataUnlockedBiomeTorches(TSPlayer op, PlayerData data)
        {
            // 1.4.0.5 ssc无 unlockedBiomeTorches 属性
            op.SendInfoMessage("生态火把：{0}", data.unlockedBiomeTorches == 1 ? "已激活" : "未激活");
        }

        public static void ModifyDataBiomeTorches(Player op, PlayerData data)
        {
            // 1.4.0.5 ssc无 unlockedBiomeTorches 属性
            op.unlockedBiomeTorches = data.unlockedBiomeTorches == 1;
        }

    }
}