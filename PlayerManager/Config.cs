using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using TShockAPI;

namespace Plugin
{
    public class Config
    {
        public bool LockHPEnable  = false;
        public int LockHPSecond = 1;
        public int LockHPValue = 1;

        // 随机物品
        public bool RandItemEnable = false;
        public List<MItem> RandItem1 = new List<MItem>();
        public List<MItem> RandItem2 = new List<MItem>();
        public List<MItem> RandItem3 = new List<MItem>();
        public List<MItem> RandItem4 = new List<MItem>();
        public List<MItem> RandItem5 = new List<MItem>();
        public List<MItem> RandItem6 = new List<MItem>();
        public List<MItem> RandItem7 = new List<MItem>();
        public List<MItem> RandItem8 = new List<MItem>();
        public List<MItem> RandItem9 = new List<MItem>();
        public List<MItem> RandItem10 = new List<MItem>();

        public static Config Load(string path)
        {
            if (File.Exists(path))
            {
                return JsonConvert.DeserializeObject<Config>(File.ReadAllText(path));
            }
            else
            {
                var c = new Config();
                c.InitDefault();
                File.WriteAllText(path, JsonConvert.SerializeObject(c, Formatting.Indented));
                return c;
            }
        }

        public void InitDefault()
        {
            RandItem1.Add( new MItem(3093, 5, 0, "草药袋") );
            RandItem1.Add( new MItem(4345, 10, 0, "蠕虫罐头") );
            RandItem1.Add( new MItem(1774, 5, 0, "礼袋") );
            RandItem1.Add( new MItem(1869, 5, 0, "礼物") );
            RandItem1.Add( new MItem(4731, 1, 0, "泰拉马桶") );

            RandItem2.Add( new MItem(3085, 1, 0, "金锁盒") );
            RandItem2.Add( new MItem(327, 2, 0, "金钥匙") );
            RandItem2.Add( new MItem(4879, 1, 0, "黑曜石锁盒") );
            RandItem2.Add( new MItem(329, 1, 0, "暗影钥匙") );
            RandItem2.Add( new MItem(236, 1, 0, "金匣") );
            RandItem2.Add( new MItem(2334, 2, 0, "木匣") );
            RandItem2.Add( new MItem(4877, 1, 0, "黑曜石匣") );
            RandItem2.Add( new MItem(4731, 1, 0, "泰拉马桶") );

            RandItem3.Add( new MItem(3318, 1, 0, "宝藏袋-史莱姆王") );
            RandItem3.Add( new MItem(3319, 1, 0, "宝藏袋-克苏鲁之眼") );
            RandItem3.Add( new MItem(3320, 1, 0, "宝藏袋-世界吞噬者") );
            RandItem3.Add( new MItem(3321, 1, 0, "宝藏袋-克苏鲁之脑") );
            RandItem3.Add( new MItem(3322, 1, 0, "宝藏袋-蜂王") );
            RandItem3.Add( new MItem(3323, 1, 0, "宝藏袋-骷髅王") );
            RandItem3.Add( new MItem(3324, 1, 0, "宝藏袋-血肉墙") );
            RandItem3.Add( new MItem(4731, 1, 0, "泰拉马桶") );

            RandItem4.Add( new MItem(1326, 1, 0, "混沌传送仗") );
            RandItem4.Add( new MItem(989, 1, 0, "附魔剑") );
            RandItem4.Add( new MItem(3352, 1, 0, "时尚剪刀") );
            RandItem4.Add( new MItem(486, 1, 0, "标尺") );
            RandItem4.Add( new MItem(4818, 1, 0, "战斗扳手") );
            RandItem4.Add( new MItem(3019, 1, 0, "地狱之翼弓") );
            RandItem4.Add( new MItem(2888, 1, 0, "蜂膝弓") );
            RandItem4.Add( new MItem(96, 1, 0, "火枪") );
            RandItem4.Add( new MItem(800, 1, 0, "夺命枪") );
            RandItem4.Add( new MItem(964, 1, 0, "三发猎枪") );
            RandItem4.Add( new MItem(744, 1, 0, "钻石法杖") );
            RandItem4.Add( new MItem(64, 1, 0, "魔刺") );
            RandItem4.Add( new MItem(1256, 1, 0, "猩红魔杖") );
            RandItem4.Add( new MItem(165, 1, 0, "水矢") );
            RandItem4.Add( new MItem(4281, 1, 0, "雀杖") );
            RandItem4.Add( new MItem(5069, 1, 0, "小雪怪法杖") );
            RandItem4.Add( new MItem(4731, 1, 0, "泰拉马桶") );

            RandItem5.Add( new MItem(285, 1, 0, "金属带扣") );
            RandItem5.Add( new MItem(212, 1, 0, "疾风脚镯") );
            RandItem5.Add( new MItem(158, 1, 0, "幸运马掌") );
            RandItem5.Add( new MItem(4978, 1, 0, "雏翼") );
            RandItem5.Add( new MItem(159, 1, 0, "闪亮红气球") );
            RandItem5.Add( new MItem(1724, 1, 0, "罐中臭屁") );
            RandItem5.Add( new MItem(857, 1, 0, "沙暴瓶") );
            RandItem5.Add( new MItem(863, 1, 0, "水上漂靴") );
            RandItem5.Add( new MItem(950, 1, 0, "溜冰鞋") );
            RandItem5.Add( new MItem(4731, 1, 0, "泰拉马桶") );

            RandItem6.Add( new MItem(3118, 1, 0, "生命体分析机") );
            RandItem6.Add( new MItem(3099, 1, 0, "秒表") );
            RandItem6.Add( new MItem(3119, 1, 0, "每秒伤害计数器") );
            RandItem6.Add( new MItem(3120, 1, 0, "渔民袖珍宝典") );
            RandItem6.Add( new MItem(3037, 1, 0, "天气收音机") );
            RandItem6.Add( new MItem(3096, 1, 0, "六分仪") );
            RandItem6.Add( new MItem(3084, 1, 0, "雷达") );
            RandItem6.Add( new MItem(4731, 1, 0, "泰拉马桶") );

            RandItem7.Add( new MItem(906, 1, 0, "熔岩护身符") );
            RandItem7.Add( new MItem(211, 1, 0, "猛爪手套") );
            RandItem7.Add( new MItem(891, 1, 0, "邪眼") );
            RandItem7.Add( new MItem(1323, 1, 0, "黑曜石玫瑰") );
            RandItem7.Add( new MItem(3212, 1, 0, "鲨牙项链") );
            RandItem7.Add( new MItem(1864, 1, 0, "甲虫莎草纸") );
            RandItem7.Add( new MItem(938, 1, 0, "圣骑士护盾") );
            RandItem7.Add( new MItem(1253, 1, 0, "冰冻海龟壳") );
            RandItem7.Add( new MItem(1321, 1, 0, "魔法箭袋") );
            RandItem7.Add( new MItem(1322, 1, 0, "岩浆石") );
            RandItem7.Add( new MItem(4731, 1, 0, "泰拉马桶") );

            RandItem8.Add( new MItem(4056, 1, 0, "远古凿子") );
            RandItem8.Add( new MItem(407, 1, 0, "工具腰带") );
            RandItem8.Add( new MItem(1923, 1, 0, "工具箱") );
            RandItem8.Add( new MItem(2214, 1, 0, "砖层") );
            RandItem8.Add( new MItem(2215, 1, 0, "加长握爪") );
            RandItem8.Add( new MItem(2217, 1, 0, "水泥搅拌机") );
            RandItem8.Add( new MItem(2373, 1, 0, "优质钓鱼线") );
            RandItem8.Add( new MItem(2374, 1, 0, "渔夫耳环") );
            RandItem8.Add( new MItem(2375, 1, 0, "钓具箱") );
            RandItem8.Add( new MItem(4881, 1, 0, "防熔岩钓钩") );
            RandItem8.Add( new MItem(4442, 1, 0, "甲虫钓竿") );
            RandItem8.Add( new MItem(2296, 1, 0, "冤大头钓竿") );
            RandItem8.Add( new MItem(2422, 1, 0, "熔线钓钩") );
            RandItem8.Add( new MItem(3183, 1, 0, "金虫网") );
            RandItem8.Add( new MItem(4731, 1, 0, "泰拉马桶") );

            RandItem9.Add( new MItem(3060, 1, 0, "骨头拨浪鼓") );
            RandItem9.Add( new MItem(1181, 1, 0, "奇异发光蘑菇") );
            RandItem9.Add( new MItem(603, 1, 0, "胡萝卜") );
            RandItem9.Add( new MItem(1172, 1, 0, "蜥蜴蛋") );
            RandItem9.Add( new MItem(4701, 1, 0, "泥芽") );
            RandItem9.Add( new MItem(4366, 1, 0, "桉树汁") );
            RandItem9.Add( new MItem(1927, 1, 0, "狗哨") );
            RandItem9.Add( new MItem(4605, 1, 0, "眩晕花蜜") );
            RandItem9.Add( new MItem(669, 1, 0, "鱼") );
            RandItem9.Add( new MItem(4731, 1, 0, "泰拉马桶") );

            RandItem10.Add( new MItem(2428, 1, 0, "绒毛胡萝卜") );
            RandItem10.Add( new MItem(2428, 1, 0, "绒毛胡萝卜") );
            RandItem10.Add( new MItem(2428, 1, 0, "绒毛胡萝卜") );
            RandItem10.Add( new MItem(2502, 1, 0, "涂蜜护目镜") );
            RandItem10.Add( new MItem(2502, 1, 0, "涂蜜护目镜") );
            RandItem10.Add( new MItem(2430, 1, 0, "粘鞍") );
            RandItem10.Add( new MItem(2430, 1, 0, "粘鞍") );
            RandItem10.Add( new MItem(4791, 1, 0, "蹦蹦跷") );
            RandItem10.Add( new MItem(4791, 1, 0, "蹦蹦跷") );
            RandItem10.Add( new MItem(4716, 1, 0, "贝壳哨") );
            RandItem10.Add( new MItem(4716, 1, 0, "贝壳哨") );
            RandItem10.Add( new MItem(4786, 1, 0, "皇家鎏金鞍") );
            RandItem10.Add( new MItem(4786, 1, 0, "皇家鎏金鞍") );
            RandItem10.Add( new MItem(4264, 1, 0, "高尔夫球车钥匙") );
            RandItem10.Add( new MItem(4264, 1, 0, "高尔夫球车钥匙") );
            RandItem10.Add( new MItem(1914, 1, 0, "驯鹿铃铛") );
            RandItem10.Add( new MItem(2771, 1, 0, "扰脑器") );
            RandItem10.Add( new MItem(4796, 1, 0, "暗黑魔法师巨著") );
            RandItem10.Add( new MItem(4731, 1, 0, "泰拉马桶") );
        }

        // public static void fixedNetID( Config _config)
        // {
        //     _config.RandItem1.ForEach( i => i.fixNetID() );
        //     _config.RandItem2.ForEach( i => i.fixNetID() );
        //     _config.RandItem3.ForEach( i => i.fixNetID() );
        //     _config.RandItem4.ForEach( i => i.fixNetID() );
        //     _config.RandItem5.ForEach( i => i.fixNetID() );
        // }

    }



    public class MItem
    {
        public string netName;
        public int netID;
        public byte prefix;
        public int stack;

        public MItem(int _netID, int _stack = 1, byte _prefix = 0, string _netName="" )
        {
            netID = _netID;
            stack = _stack;
            prefix = _prefix;
            netName = _netName;
        }

        public  bool fixNetID()
        {
            if(netID==0 && netName!=""){
                var items = TShock.Utils.GetItemByIdOrName(netName);
                TShock.Log.ConsoleError($"[playerManager]: 查找 {netName}  的物品id");
                if (items.Count == 1)
                {
                    netID = items[0].netID;
                    TShock.Log.ConsoleError($"[playerManager]: 物品id={items[0].netID} ");
                    return true;
                } else {
                    TShock.Log.ConsoleError($"[playerManager]:  {netName}不存在 ！");
                }
            }

            return false;
        }

    }



}