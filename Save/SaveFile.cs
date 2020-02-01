using Marina.Utils;
using Newtonsoft.Json;

namespace Marina.Save
{
    public abstract class SaveFile<T> : ISaveFile where T : new()
    {
        protected T _data;

        public SaveFile(string name)
        {
            FileInfo = SaveDirectory.GetFile(name);
            _data = FileInfo.Exists ? JsonConvert.DeserializeObject<T>(FileInfo.ReadAllText()) : new T();
        }

        public override void Write() => FileInfo.WriteAllText(JsonConvert.SerializeObject(_data, Formatting.Indented));
    }
}
