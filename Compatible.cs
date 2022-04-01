using System;
using System.Reflection;
using Terraria;
using TShockAPI;


namespace Plugin
{
    class Compatible
    {
        public static void ShowDataUnlockedBiomeTorches(TSPlayer op, PlayerData data)
        {
            // 1.4.0.5 ssc无 unlockedBiomeTorches 属性
            try
            {
                int result = (int)typeof(PlayerData).InvokeMember("unlockedBiomeTorches", BindingFlags.Public | BindingFlags.GetProperty, null, null, null);
                op.SendInfoMessage("生态火把：{0}", result == 1 ? "已激活" : "未激活");
            }
            #pragma warning disable 0168
            catch (MissingMethodException e)
            {
                // Console.WriteLine("如果是 TShock4.4.0_Pre12_Terraria1.4.0.5，请忽略此条信息\n {0}", e.Message);
            }
            #pragma warning restore 0168
        }

        public static void ModifyDataBiomeTorches(Player op, PlayerData data)
        {
            // 1.4.0.5 ssc无 unlockedBiomeTorches 属性
            // op.unlockedBiomeTorches = data.unlockedBiomeTorches==1;
            try
            {
                int result = (int)typeof(PlayerData).InvokeMember("unlockedBiomeTorches", BindingFlags.Public | BindingFlags.GetProperty, null, data, new object[] { });
                op.unlockedBiomeTorches = result == 1;
            }
            #pragma warning disable 0168
            catch (MissingMethodException e) { }
            #pragma warning restore 0168


            // 1.4.0.5 ssc无 usingBiomeTorches 属性
            // op.UsingBiomeTorches = data.usingBiomeTorches==1;
            try
            {
                int result = (int)typeof(PlayerData).InvokeMember("usingBiomeTorches", BindingFlags.Public | BindingFlags.GetProperty, null, data, new object[] { });
                op.UsingBiomeTorches = result == 1;
            }
            #pragma warning disable 0168
            catch (MissingMethodException e) { }
            #pragma warning restore 0168
        }



        // public static Boolean RequireLogin
        // {
        //     // 1.4.0.5
        //     // get { return TShock.Config.RequireLogin; }

        //     get { return TShock.Config.Settings.RequireLogin; }
        // }
    }
}