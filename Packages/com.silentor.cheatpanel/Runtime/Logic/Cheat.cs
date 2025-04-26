using System;
using System.Reflection;
using UnityEngine.UIElements;

namespace Silentor.CheatPanel
{
    /// <summary>
    /// Wrapper for cheat instance
    /// </summary>
    public class Cheat
    {
        public readonly string Name;
        public readonly String TabName;
        public readonly String GroupName;
        
        public readonly ICheats       CheatObject;
        public readonly MemberInfo    MemberInfo;

        public Cheat(ICheats cheatObject, MemberInfo memberInfo )
        {
            CheatObject = cheatObject;
            MemberInfo  = memberInfo;
            var cheatAttribute = memberInfo.GetCustomAttribute<CheatAttribute>( );
            if ( cheatAttribute != null )
            {
                TabName   = cheatAttribute.TabName;
                GroupName = cheatAttribute.GroupName;
                Name      = cheatAttribute.CheatName;
            }
            Name ??= memberInfo.Name;
        }

        public          VisualElement GetUI()
        {
            return _ui ??= GenerateUI( );
        }

        public void InvalidateUI( )
        {
            _ui = null;
        }

        public static bool IsValidCheat( MemberInfo memberInfo )
        {
            if ( typeof(ICheats).IsAssignableFrom( memberInfo.DeclaringType ) )
            {
                if ( memberInfo is MethodInfo methodInfo && (methodInfo.IsPublic || methodInfo.GetCustomAttribute<CheatAttribute>() != null ) )
                {
                    var parameters = methodInfo.GetParameters( );
                    if ( parameters.Length == 0 )
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private         VisualElement _ui;

        private VisualElement GenerateUI( )
        {
            var cheatName = Name;

            if ( MemberInfo is MethodInfo cheatMethod )
            {
                var cheatBtn  = new Button( );
                cheatBtn.AddToClassList( "CheatBtn" );
                cheatBtn.text = cheatName;

                cheatBtn.clicked += ( ) =>
                {
                    var parameters = cheatMethod.GetParameters( );
                    if ( parameters.Length == 0 )
                    {
                        cheatMethod.Invoke( CheatObject, null );
                    }
                    else
                    {
                        //todo add UI for parameters
                        throw new NotImplementedException();
                    }
                };

                return cheatBtn;
            }

            throw new NotImplementedException( $"Cheat {Name} is not a method" );
        }
    }
}