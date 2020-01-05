using System.Collections.Generic;

namespace Marina.Save.Types
{
    public class ListSaveFile<T> : SaveFile<List<T>>
    {
        public ListSaveFile(string name) : base(name) { }
    }
}
