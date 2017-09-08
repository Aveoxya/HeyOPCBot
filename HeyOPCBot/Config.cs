using System;
using Newtonsoft.Json;
using System.IO;

namespace HeyOPCBot
{
    public class Config
    {
        [JsonIgnore]
        private static readonly string appdir = AppDomain.CurrentDomain.BaseDirectory;
        public string Prefix { get; set; }
        public string Token { get; set; }
        public Config()
        {
            Prefix = "";
            Token = "";
        }

        public void Save(string dir = "configuration.json") { File.WriteAllText(Path.Combine(appdir, dir), JsonConvert.SerializeObject(this, Formatting.Indented)); }

        public static Config Load(string dir = "configuration.json")
        {
            return JsonConvert.DeserializeObject<Config>(File.ReadAllText(Path.Combine(appdir, dir)));
        }
    }
}
