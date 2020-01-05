using System.Collections.Generic;

namespace Marina.Save
{
    public class ListSaveFile<T> : SaveFile<List<T>>
    {
        public ListSaveFile(string name) : base(name) { }
    }
}
