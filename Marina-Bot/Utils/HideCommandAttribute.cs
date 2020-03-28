using System;

namespace Marina.Utils
{
    public class HideCommandAttribute : Attribute
    {
        public override bool IsDefaultAttribute() => false;
    }
}