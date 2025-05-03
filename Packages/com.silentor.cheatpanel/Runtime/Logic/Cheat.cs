using System;
using System.Reflection;
using System.Threading;
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
    public abstract class Cheat
    {
        public readonly String Name;
        public readonly String TabName;
        public readonly String GroupName;
        
        public readonly  ICheats    CheatObject;
        //public readonly  MemberInfo MemberInfo;

        public static Cheat CreateCheat( MemberInfo cheatMember, ICheats cheatObject, CheatPanel cheatPanel, CancellationToken cancel )
        {
            if( cheatMember is PropertyInfo cheatProperty )
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
            }
            else if( cheatMember is MethodInfo cheatMethod )
            {
                if( cheatMethod.GetParameters().Length == 0 )
                    return new CheatMethod( cheatMethod, cheatObject, cheatPanel, cancel );
                else
                    return new CheatMethodParams( cheatMethod, cheatObject, cheatPanel, cancel );
            }

            return null;
        }

        protected Cheat(   MemberInfo cheatMember, ICheats cheatObject, CancellationToken cancel )
        {
            _cheatMember = cheatMember;
            CheatObject  = cheatObject;
            _cancel      = cancel;

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
                        if( propertyInfo.PropertyType.IsPrimitive || propertyInfo.PropertyType == typeof( string ) )
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        protected readonly CheatAttribute    Attr;
        private            VisualElement     _ui;
        private readonly   CancellationToken _cancel;
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
                RefreshUILoop( refreshTime.Time, _cancel );
            }

            return ui;
        }

        // protected VisualElement GenerateUI( )
        // {
        //     var cheatName = Name;
        //
        //     if ( MemberInfo is MethodInfo cheatMethod )
        //     {
        //         var paramz = cheatMethod.GetParameters( );
        //         if( paramz.Length == 0 )                //Simple one btn cheat
        //         {
        //             var cheatBtn  = new Button( );
        //             cheatBtn.AddToClassList( "CheatBtn" );
        //             cheatBtn.text = cheatName;
        //
        //             cheatBtn.clicked += ( ) => ExecuteMethodCheat( null );
        //             return cheatBtn;
        //         }
        //         else if( paramz.Length == 1 )           
        //         {
        //             //var paramType = paramz[0].ParameterType;
        //             var paramValuesAttr = paramz[0].GetCustomAttribute<CheatValueAttribute>( );
        //             if( paramValuesAttr != null )                   //Bunch of buttons for every param value
        //             {
        //                 var container = new VisualElement();
        //                 container.AddToClassList( "CheatLine" );
        //                 var label = new Label( cheatName );
        //                 label.AddToClassList( "CheatLabel" );
        //                 container.Add( label );
        //                 foreach ( var paramVal in paramValuesAttr.Values )
        //                 {
        //                     var paramBtn = new Button( );
        //                     paramBtn.AddToClassList( "CheatBtn" );
        //                     paramBtn.text = paramVal.ToString();
        //                     paramBtn.clicked += ( ) => ExecuteMethodCheat( new []{paramVal} );
        //                     container.Add( paramBtn );
        //                 }
        //                 return container;
        //             }
        //             else                                        //Text field for param value (similar to cheat property set only)
        //             {
        //                 return GenerateValueField( cheatName, cheatMethod );
        //             }
        //         }
        //     }
        //     else if ( MemberInfo is PropertyInfo cheatProp )
        //     {
        //         _cheatProp = cheatProp;
        //         var propType = cheatProp.PropertyType;
        //         if ( propType == typeof(bool) )
        //         {
        //             var field = new Toggle( cheatName );
        //             field.AddToClassList( "CheatToggle" );
        //             field.AddToClassList( "CheatLine" );
        //
        //             if ( cheatProp.CanWrite )                        
        //                 field.RegisterValueChangedCallback( evt => UpdateProperty( evt.newValue ) );
        //             else
        //                 field.SetEnabled( false );
        //
        //             if ( cheatProp.CanRead ) //Property cheats value should be refreshed if changed externally
        //                 RefreshUIFromCheatProperty( () => field.SetValueWithoutNotify( (bool)cheatProp.GetValue( CheatObject, null )), _cancel );
        //
        //             return field;
        //         }
        //         else if ( propType.IsPrimitive || propType == typeof(string))
        //         {
        //             var              rangeAttr = cheatProp.GetCustomAttribute<RangeAttribute>( );
        //             if( rangeAttr != null )
        //             {
        //                 return GenerateSliderField( cheatName, cheatProp, rangeAttr );
        //             }
        //             else
        //             {
        //                 return GenerateValueField( cheatName, cheatProp );
        //             }
        //         }
        //     }
        //
        //     return null;
        // }
        //
        // private void UpdateProperty( System.Object newValue )
        // {
        //     _cheatProp.SetValue( CheatObject, newValue, null );
        // }
        //
        // protected void SetRefreshUILogic( Action refreshCheatLogic )
        // {
        //     RefreshUILogic = refreshCheatLogic;
        // }

        private async Awaitable RefreshUILoop( float refreshTime, CancellationToken cancel )
        {
            while ( !cancel.IsCancellationRequested )
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
                        Debug.LogError( $"[Cheat]-[RefreshUILoop] Cheat {ToString()} refresh UI exception: {e.Message}" );
                    }
                }
                
                await (refreshTime > 0 ? Awaitable.WaitForSecondsAsync( refreshTime, cancel ) : Awaitable.NextFrameAsync( cancel ));
            }
        }

        // private VisualElement GenerateSliderField( String cheatName, PropertyInfo cheatProp, RangeAttribute range )
        // {
        //     if( cheatProp.PropertyType == typeof(int) || cheatProp.PropertyType == typeof(uint)
        //        || cheatProp.PropertyType == typeof(long)  || cheatProp.PropertyType == typeof(ulong)
        //        || cheatProp.PropertyType == typeof(byte)  || cheatProp.PropertyType == typeof(sbyte)
        //        || cheatProp.PropertyType == typeof(short) || cheatProp.PropertyType == typeof(ushort) )
        //     {
        //         var field = new SliderInt( cheatName, (int)range.min, (int)range.max );
        //         field.AddToClassList( "CheatSlider" );
        //         field.AddToClassList( "CheatLine" );
        //         if ( cheatProp.CanWrite )                        
        //             field.RegisterValueChangedCallback( evt => UpdateProperty( Convert.ChangeType(evt.newValue, cheatProp.PropertyType) ) );
        //         else
        //             field.SetEnabled( false );
        //         if ( cheatProp.CanRead ) //Property cheats value should be refreshed if changed externally
        //             RefreshUIFromCheatProperty( () => field.SetValueWithoutNotify( Convert.ToInt32(cheatProp.GetValue( CheatObject, null ))), _cancel );
        //         return field;
        //     }
        //     else if( cheatProp.PropertyType == typeof(float) || cheatProp.PropertyType == typeof(double) )
        //     {
        //         var field = new Slider( cheatName, range.min, range.max );
        //         field.AddToClassList( "CheatSlider" );
        //         field.AddToClassList( "CheatLine" );
        //         if ( cheatProp.CanWrite )                        
        //             field.RegisterValueChangedCallback( evt => UpdateProperty( evt.newValue ) );
        //         else
        //             field.SetEnabled( false );
        //         if ( cheatProp.CanRead ) //Property cheats value should be refreshed if changed externally
        //             RefreshUIFromCheatProperty( () => field.SetValueWithoutNotify( Convert.ToSingle( cheatProp.GetValue( CheatObject, null ))), _cancel );
        //         return field;
        //     }
        //
        //     return null;
        // }

        // private VisualElement GenerateValueField( String cheatName, MethodInfo cheatMethod_param )
        // {
        //     Assert.IsTrue( cheatMethod_param.GetParameters().Length == 0 );
        //     Action<Object> setter    = value => cheatMethod_param.Invoke( CheatObject, new []{value} );
        //     var            paramType = cheatMethod_param.GetParameters()[0].ParameterType;
        //     return GenerateValueField( cheatName, paramType, null, setter );
        // }
        //
        // private VisualElement GenerateValueField( String cheatName, PropertyInfo cheatProp )
        // {
        //     Func<Object> getter = cheatProp.CanRead ? () => cheatProp.GetValue( CheatObject, null ) : null;
        //     Action<Object> setter = cheatProp.CanWrite ? (value) => cheatProp.SetValue( CheatObject, value, null ) : null;
        //     return GenerateValueField( cheatName, cheatProp.PropertyType, getter, setter );
        // }

        // private VisualElement GenerateValueField( String cheatName, Type type, Func<Object> getValue, Action<Object> setValue )
        // {
        //     if( type == typeof(float) )
        //         return PrepareTextValueField( new FloatField( cheatName ) );
        //     else if( type == typeof(int) )
        //         return PrepareTextValueField( new IntegerField( cheatName ) );
        //     else if( type == typeof(double) )
        //         return PrepareTextValueField( new DoubleField( cheatName ) );
        //     else if( type == typeof(uint) )
        //         return PrepareTextValueField( new UnsignedIntegerField( cheatName ) );
        //     else if( type == typeof(long) )
        //         return PrepareTextValueField( new LongField( cheatName ) );
        //     else if( type == typeof(ulong) )
        //         return PrepareTextValueField( new UnsignedLongField( cheatName ) );
        //     else if( type == typeof(byte) )
        //         return PrepareSubIntegerField<byte>( new IntegerField( cheatName ), Byte.MinValue, Byte.MaxValue );
        //     else if( type == typeof(sbyte) )
        //         return PrepareSubIntegerField<sbyte>( new IntegerField( cheatName ), SByte.MinValue, SByte.MaxValue );
        //     else if( type == typeof(short) )
        //         return PrepareSubIntegerField<short>( new IntegerField( cheatName ), Int16.MinValue, Int16.MaxValue );
        //     else if( type == typeof(ushort) )
        //         return PrepareSubIntegerField<ushort>( new IntegerField( cheatName ), UInt16.MinValue, UInt16.MaxValue );
        //     else if( type == typeof(string) )
        //         return PrepareTextValueField( new TextField( cheatName ) );
        //
        //     return null;
        //     
        //     TextInputBaseField<T> PrepareTextValueField<T>( TextInputBaseField<T> field )
        //     {
        //         field.AddToClassList( "CheatTextBox" );
        //         field.AddToClassList( "CheatLine" );
        //         field.isDelayed = true;
        //         field.RegisterCallback<FocusInEvent>( FieldFocusIn );
        //         field.RegisterCallback<FocusOutEvent>( FieldFocusOut );
        //         if ( setValue != null )                        
        //             field.RegisterValueChangedCallback( evt => setValue( evt.newValue ) );
        //         else
        //             field.SetEnabled( false );
        //
        //         if ( getValue != null ) //Property cheats value should be refreshed if changed externally
        //             RefreshUIFromCheatProperty( () => field.SetValueWithoutNotify( (T)getValue()), _cancel );
        //
        //         return field;
        //     }
        //
        //     IntegerField PrepareSubIntegerField<T>( IntegerField field, int min, int max ) where T : IConvertible
        //     {
        //         field.AddToClassList( "CheatTextBox" );
        //         field.AddToClassList( "CheatLine" );
        //         field.isDelayed = true;
        //         if ( setValue != null )
        //         {
        //             field.RegisterValueChangedCallback( evt =>
        //             {
        //                 var clampedValue = Math.Clamp( evt.newValue, min, max );
        //                 setValue( Convert.ChangeType( clampedValue, typeof(T) ) );
        //             } );
        //         }
        //         else
        //             field.SetEnabled( false );
        //
        //         if ( getValue != null ) //Property cheats value should be refreshed if changed externally
        //             RefreshUIFromCheatProperty( () => field.SetValueWithoutNotify( Convert.ToInt32( getValue())), _cancel );
        //
        //         return field;
        //     }
        // }

        // private void ExecuteMethodCheat( Object[] paramz )
        // {
        //     var methodInfo = (MethodInfo)MemberInfo;
        //     var result = methodInfo.Invoke( CheatObject, paramz );
        //     if( result != null )
        //     {
        //         _cheatPanel.ShowResult( result.ToString() );
        //     }
        // }

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
    }
}