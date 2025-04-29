using System;

namespace Silentor.CheatPanel
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class CheatValueAttribute: Attribute
    {
        public readonly Object[] Values;

        public CheatValueAttribute( params Object[] values )
        {
            Values = values;
        }
    }
}