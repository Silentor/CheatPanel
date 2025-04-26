using System;
using UnityEngine.Scripting;
using UnityEngine.UI;

namespace Silentor.CheatPanel
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
    public class CheatAttribute : Attribute
    {
        public String CheatName;
        public String GroupName;
        public String TabName;

        public CheatAttribute( )
        {
            CheatName = null;
            GroupName = null;
            TabName   = null;
        }

        public CheatAttribute( String groupName )
        {
            GroupName = groupName;
        }    
    }
}