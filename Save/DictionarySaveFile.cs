using System.Collections.Generic;

namespace Marina.Save
{
    public class DictionarySaveFile<T, K> : SaveFile<Dictionary<T, K>>
    {
        public DictionarySaveFile(string name) : base(name) { }
    }
}
