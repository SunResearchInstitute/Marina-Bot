using Marina.Utils;
using Newtonsoft.Json;

namespace Marina.Save
{
    public abstract class SaveFile<T> : ISaveFile where T : new()
    {
        public T Data;

        public SaveFile(string name)
        {
            FileInfo = SaveDirectory.GetFile(name);
            Data = FileInfo.Exists ? JsonConvert.DeserializeObject<T>(FileInfo.ReadAllText()) : new T();
        }

        public override void Write() => FileInfo.WriteAllText(JsonConvert.SerializeObject(Data, Formatting.Indented));
    }
}
