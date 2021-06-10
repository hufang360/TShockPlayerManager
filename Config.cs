using System.IO;
using Newtonsoft.Json;

namespace Plugin
{
    public class Config
    {
        public bool LockHPEnable  = false;
        public int LockHPSecond = 1;
        public int LockHP = 1;

        public static Config Load(string path) {
            if (File.Exists(path))
            {
                return JsonConvert.DeserializeObject<Config>(File.ReadAllText(path));
            }
            else
            {
                return new Config();
            }
        }
    }
}