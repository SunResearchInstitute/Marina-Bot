using System;

namespace Marina.Attributes
{
    public class HideCommandAttribute : Attribute
    {
        public override bool IsDefaultAttribute() => false;
    }
}