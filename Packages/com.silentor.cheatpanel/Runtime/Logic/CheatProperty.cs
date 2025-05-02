using System;
using System.Reflection;
using System.Threading;
using UnityEngine;

namespace Silentor.CheatPanel
{
    public class CheatProperty
    {
        public    static Cheat CreatePropertyCheat( PropertyInfo cheatProperty, ICheats cheatObject, CancellationToken cancel )
        {
            var rangeAttr = cheatProperty.GetCustomAttribute<RangeAttribute>();
            if( rangeAttr == null )
            {
                return new CheatPropertyField( cheatProperty, cheatObject, cancel );
            }
            else
            {
                return new CheatPropertySlider( cheatProperty, rangeAttr, cheatObject, cancel );
            }

            return null;
        }
    }
}