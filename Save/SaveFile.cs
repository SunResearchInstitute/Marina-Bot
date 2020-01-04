using Marina.Utils;
using Newtonsoft.Json;

namespace Marina.Save
{
    public abstract class SaveFile<T> : ISaveFile
    {
        public T Data;

        protected SaveFile(string name)
        {
            Name = name;
            FileInfo = SaveDirectory.GetFile($"{name}.{Extension}");
            if (FileInfo.Exists) Data = JsonConvert.DeserializeObject<T>(FileInfo.ReadAllText());
        }
    }
}
