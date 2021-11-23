using System.IO;

namespace Marina.Save.Types
{
    public class ConfigFile : LibSave.Types.SaveFile<Config>
    {
        public ConfigFile(FileInfo filePath) : base(filePath, new Config(), true) { }
    }

    public class Config
    {
        public Config()
        {
            Token = "INSERT TOKEN HERE";
            TwitchName = "";
            ChannelId = 0L;
            GuildId = 0L;
            GitKey = "";
        }

        public string Token { get; set; }
        public string TwitchName { get; set; }
        public ulong ChannelId { get; set; }
        public ulong GuildId { get; set; }
        public string GitKey { get; set; }
    }
}
