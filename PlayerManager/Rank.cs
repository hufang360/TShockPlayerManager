using System.Collections.Generic;
using System.Linq;
using TShockAPI;

namespace PlayerManager
{
    public class Rank
    {
        public static void Manage(CommandArgs args)
        {
            TSPlayer op = args.Player;
            void Help()
            {
                op.SendInfoMessage("/[c/96FF0A:d]eath[c/96FF0A:r]ank 指令用法：");
                op.SendInfoMessage("/dr pve, 死亡榜");
                op.SendInfoMessage("/dr pvp, 玩家对决榜");
            }

            if (args.Parameters.Count == 0)
            {
                Help();
                return;
            }

            bool isPVE = true;
            switch (args.Parameters[0].ToLowerInvariant())
            {
                case "pve": isPVE = true; break;
                case "pvp": isPVE = false; break;

                case "help":
                default:
                    Help();
                    return;
            }


            // 查找玩家数据
            List<RankData> datas = new();

            // 在线存档
            var savedlist = new List<string>();
            TShock.Players.Where(p => p != null && p.SaveServerCharacter()).ForEach(plr =>
            {
                savedlist.Add(plr.Name);
                datas.Add(new RankData(plr.Name, plr.TPlayer.numberOfDeathsPVE, plr.TPlayer.numberOfDeathsPVP));
            });

            // 离线存档
            var allaccount = TShock.UserAccounts.GetUserAccounts();
            allaccount.Where(acc => acc != null && !savedlist.Contains(acc.Name)).ForEach(acc =>
            {
                PlayerData data = TShock.CharacterDB.GetPlayerData(new TSPlayer(-1), acc.ID);
                if (data != null && data.hideVisuals != null)
                {
                    //datas.Add(new RankData(acc.Name, data.numberOfDeathsPVE, data.numberOfDeathsPVP));
                }
            });

            List<RankData> lists;
            List<string> lines = new();
            if (isPVE)
            {
                if (!PaginationTools.TryParsePageNumber(args.Parameters, 1, op, out int pageNumber)) return;

                lists = datas.Where(obj => obj.pve > 0).OrderByDescending(obj => obj.pve).ToList();
                if (lists.Count == 0)
                {
                    args.Player.SendInfoMessage("暂无 死亡榜 数据");
                    return;
                }

                for (int i = 0; i < lists.Count; i++)
                {
                    lines.Add($"第{i + 1}名, {lists[i].name}, {lists[i].pve}次");
                }

                PaginationTools.SendPage(op, pageNumber, lines, new PaginationTools.Settings
                {
                    HeaderFormat = "[c/96FF0A:『死亡榜』({0}/{1})：]",
                    FooterFormat = "输入 /dr pve {{0}} 查看更多".SFormat(Commands.Specifier)
                });
            }
            else
            {
                if (!PaginationTools.TryParsePageNumber(args.Parameters, 1, op, out int pageNumber)) return;

                lists = datas.Where(obj => obj.pvp > 0).OrderByDescending(obj => obj.pvp).ToList();
                if (lists.Count == 0)
                {
                    args.Player.SendInfoMessage("暂无 玩家对决榜 数据");
                    return;
                }

                for (int i = 0; i < lists.Count; i++)
                {
                    lines.Add($"第{i + 1}名, {lists[i].name}, {lists[i].pvp}次");
                }
                PaginationTools.SendPage(op, pageNumber, lines, new PaginationTools.Settings
                {
                    HeaderFormat = "[c/96FF0A:『玩家对决榜』({0}/{1})：]",
                    FooterFormat = "输入 /dr pvp {{0}} 查看更多".SFormat(Commands.Specifier)
                });
            }
        }
    }


    public class RankData
    {
        public string name = "";

        public int pve = 0;

        public int pvp = 0;

        public RankData(string _name, int _pve, int _pvp)
        {
            name = _name;
            pve = _pve > 0 ? _pve : 0;
            pvp = _pvp > 0 ? _pvp : 0;
        }
    }

}