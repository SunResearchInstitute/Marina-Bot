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
            BotToken = "INSERT TOKEN HERE";
            TwitchName = "";
            Suggestions_ChannelId = 0L;
            Suggestions_GuildId = 0L;
            GitKey = "";
            Debug_GuildId = 0L;
        }

        public string BotToken { get; set; }
        public string TwitchName { get; set; }
        public ulong Suggestions_ChannelId { get; set; }
        public ulong Suggestions_GuildId { get; set; }
        public string GitKey { get; set; }
        public ulong Debug_GuildId { get; set; }
    }
}
