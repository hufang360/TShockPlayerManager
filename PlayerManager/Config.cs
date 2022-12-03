using Newtonsoft.Json;
using System.IO;

namespace PlayerManager
{
    /// <summary>
    /// 配置文件
    /// </summary>
    public class Config
    {
        // 自动备份的时间间隔（分钟），0表示关闭自动备份
        public int Interval = 0;

        // 备份文件保留时长（分钟）
        public int KeepFor = 240;

        // 是否显示自动备份通知
        public bool ShowSaveMessages = true;

        /// <summary>
        /// 加载配置文件
        /// </summary>
        public static Config Load(string path)
        {
            if (File.Exists(path))
            {
                return JsonConvert.DeserializeObject<Config>(File.ReadAllText(path));
            }
            else
            {
                var c = new Config();
                File.WriteAllText(path, JsonConvert.SerializeObject(c, Formatting.Indented));
                return c;
            }
        }
    }

}