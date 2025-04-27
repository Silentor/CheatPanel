using System;
using System.Reflection;
using System.Threading;
using UnityEngine;
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
        
        public readonly  ICheats           CheatObject;
        public readonly  MemberInfo        MemberInfo;
        

        public Cheat(ICheats cheatObject, MemberInfo memberInfo, CancellationToken cancel )
        {
            CheatObject  = cheatObject;
            MemberInfo   = memberInfo;
            _cancel = cancel;
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
                else if ( memberInfo is PropertyInfo propertyInfo &&
                          ( propertyInfo.GetMethod.IsPublic || propertyInfo.GetCustomAttribute<CheatAttribute>() != null ) )
                {
                    if( propertyInfo.PropertyType.IsPrimitive )
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private          VisualElement     _ui;
        private readonly CancellationToken _cancel;

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
            else if ( MemberInfo is PropertyInfo cheatProp )
            {
                var propType = cheatProp.PropertyType;
                if ( propType == typeof(bool) )
                {
                    var toggle = new Toggle( );
                    toggle.AddToClassList( "CheatToggle" );
                    toggle.text = cheatName;
                    toggle.value = (bool)cheatProp.GetValue( CheatObject, null );

                    toggle.RegisterValueChangedCallback( evt =>
                    {
                        cheatProp.SetValue( CheatObject, evt.newValue, null );
                    } );

                    //Property cheats value should be refreshed if changed externally
                    RefreshCheatValue( () => toggle.value = (bool)cheatProp.GetValue( CheatObject, null ), _cancel );

                    return toggle;
                }
            }

            throw new NotImplementedException( $"Cheat {Name} is not a method" );
        }

        private async Awaitable RefreshCheatValue( Action refreshCheatLogic, CancellationToken cancel )
        {
            while ( !cancel.IsCancellationRequested )
            {
                refreshCheatLogic();
                await Awaitable.WaitForSecondsAsync( 1000, cancel );
            }
        }
    }
}