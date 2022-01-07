using System;

namespace Marina.Commands.Attributes
{
    public class ManualOptionalParameterAttribute : Attribute
    {
        public ManualOptionalParameterAttribute(string defaultValue)
        {
            ManualDefaultValue = defaultValue;
        }

        public string ManualDefaultValue { get; }
        public override bool IsDefaultAttribute() => false;
    }
}