using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Unity.Properties;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;
using Object = System.Object;

namespace Silentor.CheatPanel
{
    /// <summary>
    /// Wrapper for cheat instance
    /// </summary>
    public abstract class Cheat : IDisposable
    {
        public readonly String Name;
        public readonly String TabName;
        public readonly String GroupName;
        
        public readonly  ICheats    CheatObject;

        public static Cheat CreateCheat( MemberInfo cheatMember, ICheats cheatObject, CheatPanel cheatPanel )
        {
            if( cheatMember is PropertyInfo cheatProperty )
            {
                var rangeAttr = cheatProperty.GetCustomAttribute<RangeAttribute>();
                if( rangeAttr == null )
                {
                    return new CheatPropertyField( cheatProperty, cheatObject );
                }
                else
                {
                    return new CheatPropertySlider( cheatProperty, rangeAttr, cheatObject );
                }
            }
            else if( cheatMember is MethodInfo cheatMethod )
            {
                if( cheatMethod.GetParameters().Length == 0 )
                    return new CheatMethod( cheatMethod, cheatObject, cheatPanel );
                else
                    return new CheatMethodParams( cheatMethod, cheatObject, cheatPanel );
            }

            return null;
        }

        protected Cheat(   MemberInfo cheatMember, ICheats cheatObject )
        {
            _cheatMember = cheatMember;
            CheatObject  = cheatObject;

            Attr = cheatMember.GetCustomAttribute<CheatAttribute>( );
            if ( Attr != null )
            {
                TabName   = Attr.TabName;
                GroupName = Attr.GroupName;
                Name      = Attr.CheatName;
            }
            Name ??= cheatMember.Name;
        }

        public          VisualElement GetUI()
        {
            return _ui ??= PrepareUI( ) ?? new VisualElement();
        }

        public void InvalidateUI( )
        {
            _ui = null;
        }

        public static bool IsValidCheat( MemberInfo memberInfo )
        {
            if ( typeof(ICheats).IsAssignableFrom( memberInfo.DeclaringType ) )
            {
                if ( memberInfo is MethodInfo methodInfo )
                {
                    if ( !methodInfo.IsSpecialName && (methodInfo.IsPublic || methodInfo.GetCustomAttribute<CheatAttribute>() != null ) )
                    {
                        return true;
                    }
                }
                else if ( memberInfo is PropertyInfo propertyInfo)
                {
                    if ( (propertyInfo.GetGetMethod() != null || propertyInfo.GetSetMethod() != null) || propertyInfo.GetCustomAttribute<CheatAttribute>() != null )
                    {
                        var propType = propertyInfo.PropertyType;
                        if( propType.IsPrimitive || propType == typeof( string ) || propType == typeof(Vector2) || propType == typeof(Vector3) || propType == typeof(Vector4)
                            || propType == typeof(Vector3Int) || propType == typeof(Vector2Int) || propType == typeof(Rect) || propType == typeof(Bounds)
                            || propType == typeof(RectInt) || propType == typeof(BoundsInt) || propType.IsEnum || propType == typeof(Color))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        protected readonly  CheatAttribute    Attr;
        protected           Boolean IsDisposed;

        private            VisualElement     _ui;
        private            bool              _isRefreshFieldExceptionReported;
        private readonly   CheatPanel        _cheatPanel;
        private readonly   MemberInfo        _cheatMember;


        protected abstract VisualElement GenerateUI( );

        protected abstract void RefreshUI( );

        protected abstract RefreshUITiming GetRefreshUITiming( );

        private VisualElement PrepareUI( )
        {
            VisualElement ui = null;
            try
            {
                ui = GenerateUI();
            }
            catch ( Exception e )
            {
                Debug.LogError( $"[Cheat]-[PrepareUI] Exception {e.GetType().Name} generating UI for cheat {_cheatMember}. Cheat will be discarded: {e}" );
            }

            if( ui == null )
                return null;
            
            var refreshTime = GetRefreshUITiming();
            if( refreshTime.Type == ERefreshUIType.OneTime )
            {
                RefreshUI();
            }
            else if( refreshTime.Type == ERefreshUIType.Loop )
            {
                RefreshUILoop( refreshTime.Time );
            }

            return ui;
        }

        private async void RefreshUILoop( float refreshTime )
        {
            while ( !IsDisposed )
            {
                try
                {
                    RefreshUI();
                    _isRefreshFieldExceptionReported = false;
                }
                catch ( Exception e )
                {
                    if( !_isRefreshFieldExceptionReported )
                    {
                        _isRefreshFieldExceptionReported = true;
                        Debug.LogError( $"[Cheat]-[RefreshUILoop] Cheat {ToString()} refresh UI exception: {e}" );
                    }
                }

                if ( refreshTime > 0 )
                    await Task.Delay( TimeSpan.FromSeconds( refreshTime ), CancellationToken.None);
                else
                    await Awaitable.NextFrameAsync( CancellationToken.None );
            }
        }

        private void FieldFocusIn( FocusInEvent evt )
        {
            Debug.Log( $"{Name} has focus" );
        }

        private void FieldFocusOut( FocusOutEvent focusOutEvent )
        {
            Debug.Log( $"{Name} no focus" );
        }

        public enum ERefreshUIType
        {
            Loop,
            OneTime,
            None
        }

        public readonly struct RefreshUITiming
        {
            public readonly ERefreshUIType Type;
            public readonly float          Time;

            public static readonly RefreshUITiming Never      = new(ERefreshUIType.None, 0);         //Cheat UI should not be refreshed
            public static readonly RefreshUITiming OneTime    = new(ERefreshUIType.OneTime, 0);     //Refresh one time at the generate UI stage
            public static readonly RefreshUITiming EveryFrame = new(ERefreshUIType.Loop, 0);        //Refresh cheat UI every frame
            public static readonly RefreshUITiming PerSecond  = new(ERefreshUIType.Loop, 1);    //Refresh cheat UI every second
            public static          RefreshUITiming Loop(float time) => new(ERefreshUIType.Loop, time);

            private RefreshUITiming( ERefreshUIType mode, float time ) 
            {
                Type = mode;
                Time = Math.Max( time, 0f );
            }
        }

        public void Dispose( )
        {
            // TODO release managed resources here
            IsDisposed = true;
        }
    }
}