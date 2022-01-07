using System;

namespace Marina.Commands.Attributes
{
    public class HideCommandAttribute : Attribute
    {
        public override bool IsDefaultAttribute() => false;
    }
}