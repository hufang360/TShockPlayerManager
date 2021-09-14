using TShockAPI;
using Terraria;


namespace Plugin
{
    class Compatible
    {
        public static void ShowDataUnlockedBiomeTorches(TSPlayer op, PlayerData data){
            // 1.4.0.5 ssc无 unlockedBiomeTorches 属性
            op.SendInfoMessage("生态火把：{0}", data.unlockedBiomeTorches==1 ? "已激活":"未激活" );
        }

        public static void ModifyDataBiomeTorches(Player op, PlayerData data){
            // 1.4.0.5 ssc无 unlockedBiomeTorches 属性
            // 1.4.0.5 ssc无 usingBiomeTorches 属性
            op.unlockedBiomeTorches = data.unlockedBiomeTorches==1;
            op.UsingBiomeTorches = data.usingBiomeTorches==1;
        }



        // public static Boolean RequireLogin
        // {
        //     // 1.4.0.5
        //     // get { return TShock.Config.RequireLogin; }

        //     get { return TShock.Config.Settings.RequireLogin; }
        // }
    }
}