using System;
using UnityEngine.Scripting;
using UnityEngine.UI;

namespace Silentor.CheatPanel
{
    [Preserve]
    [RequireAttributeUsages]
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field )]
    public class CheatAttribute : Attribute
    {
        public String CheatName;
        public String GroupName;
        public String TabName;

        public Cheat.RefreshUITiming RefreshTime;

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