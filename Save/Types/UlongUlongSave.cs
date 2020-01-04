using Marina.Utils;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace Marina.Save.Types
{
    public class UlongUlongSave : SaveFile<Dictionary<ulong, ulong>>
    {
        public UlongUlongSave(string name) : base(name)
        {
            if (Data == null) Data = new Dictionary<ulong, ulong>();
        }

        public override string Extension => "ulongulong";

        public override void Write() => FileInfo.WriteAllText(JsonConvert.SerializeObject(Data, Formatting.Indented));
    }
}
