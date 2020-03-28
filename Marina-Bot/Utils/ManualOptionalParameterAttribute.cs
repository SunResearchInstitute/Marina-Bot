using System;

namespace Marina.Utils
{
    public class ManualOptionalParameterAttribute : Attribute
    {
        public string ManualDefaultValue { get; }
        public override bool IsDefaultAttribute() => false;

        public ManualOptionalParameterAttribute(string defaultValue)
        {
            ManualDefaultValue = defaultValue;
        }
    }
}