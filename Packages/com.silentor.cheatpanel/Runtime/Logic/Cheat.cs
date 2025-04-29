using System;
using System.Reflection;
using System.Threading;
using UnityEngine;
using UnityEngine.UIElements;
using Object = System.Object;

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
            return _ui ??= GenerateUI( ) ?? new VisualElement();
        }

        public void InvalidateUI( )
        {
            _ui = null;
        }

        public static bool IsValidCheat( MemberInfo memberInfo )
        {
            if ( typeof(ICheats).IsAssignableFrom( memberInfo.DeclaringType ) )
            {
                if ( memberInfo is MethodInfo methodInfo && !methodInfo.IsSpecialName && (methodInfo.IsPublic || methodInfo.GetCustomAttribute<CheatAttribute>() != null ) )
                {
                    var parameters = methodInfo.GetParameters( );
                    if ( parameters.Length == 0 )
                    {
                        return true;
                    }
                }
                else if ( memberInfo is PropertyInfo propertyInfo &&
                          ( (propertyInfo.GetGetMethod() != null || propertyInfo.GetSetMethod() != null) || propertyInfo.GetCustomAttribute<CheatAttribute>() != null ))
                {
                    if( propertyInfo.PropertyType.IsPrimitive || propertyInfo.PropertyType == typeof( string ) )
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private          VisualElement     _ui;
        private readonly CancellationToken _cancel;
        private          PropertyInfo      _cheatProp;
        private          bool              _isRefreshFieldExceptionReported;

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
                _cheatProp = cheatProp;
                var propType = cheatProp.PropertyType;
                if ( propType == typeof(bool) )
                {
                    var field = new Toggle( cheatName );
                    field.AddToClassList( "CheatToggle" );
                    field.AddToClassList( "CheatLine" );

                    if ( cheatProp.CanWrite )                        
                        field.RegisterValueChangedCallback( evt => UpdateProperty( evt.newValue ) );
                    else
                        field.SetEnabled( false );

                    if ( cheatProp.CanRead ) //Property cheats value should be refreshed if changed externally
                        RefreshFieldValue( () => field.SetValueWithoutNotify( (bool)cheatProp.GetValue( CheatObject, null )), _cancel );

                    return field;
                }
                else if ( propType.IsPrimitive || propType == typeof(string))
                {
                    var              rangeAttr = cheatProp.GetCustomAttribute<RangeAttribute>( );
                    if( rangeAttr != null )
                    {
                        return GenerateSliderField( cheatName, cheatProp, rangeAttr );
                    }
                    else
                    {
                        return GenerateValueField( cheatName, cheatProp );
                    }
                }
            }

            throw new NotImplementedException( $"Cheat {Name} type {MemberInfo} is not supported" );
        }

        private void UpdateProperty( System.Object newValue )
        {
            _cheatProp.SetValue( CheatObject, newValue, null );
        }

        private async Awaitable RefreshFieldValue( Action refreshCheatLogic, CancellationToken cancel )
        {
            while ( !cancel.IsCancellationRequested )
            {
                try
                {
                    refreshCheatLogic();
                    _isRefreshFieldExceptionReported = false;
                }
                catch ( Exception e )
                {
                    if( !_isRefreshFieldExceptionReported )
                    {
                        _isRefreshFieldExceptionReported = true;
                        if( _cheatProp != null )
                            Debug.LogError( $"Cheat {Name} prop ({_cheatProp}) = {_cheatProp.GetValue( CheatObject, null )} refresh field exception: {e.Message}" );
                        else
                            Debug.LogError( $"Cheat {Name} refresh field exception: {e.Message}" );
                    }
                }
                
                await Awaitable.WaitForSecondsAsync( 1f, cancel );
            }
        }

        private VisualElement GenerateSliderField( String cheatName, PropertyInfo cheatProp, RangeAttribute range )
        {
            if( cheatProp.PropertyType == typeof(int) || cheatProp.PropertyType == typeof(uint)
               || cheatProp.PropertyType == typeof(long)  || cheatProp.PropertyType == typeof(ulong)
               || cheatProp.PropertyType == typeof(byte)  || cheatProp.PropertyType == typeof(sbyte)
               || cheatProp.PropertyType == typeof(short) || cheatProp.PropertyType == typeof(ushort) )
            {
                var field = new SliderInt( cheatName, (int)range.min, (int)range.max );
                field.AddToClassList( "CheatSlider" );
                field.AddToClassList( "CheatLine" );
                if ( cheatProp.CanWrite )                        
                    field.RegisterValueChangedCallback( evt => UpdateProperty( Convert.ChangeType(evt.newValue, cheatProp.PropertyType) ) );
                else
                    field.SetEnabled( false );
                if ( cheatProp.CanRead ) //Property cheats value should be refreshed if changed externally
                    RefreshFieldValue( () => field.SetValueWithoutNotify( Convert.ToInt32(cheatProp.GetValue( CheatObject, null ))), _cancel );
                return field;
            }
            else if( cheatProp.PropertyType == typeof(float) || cheatProp.PropertyType == typeof(double) )
            {
                var field = new Slider( cheatName, range.min, range.max );
                field.AddToClassList( "CheatSlider" );
                field.AddToClassList( "CheatLine" );
                if ( cheatProp.CanWrite )                        
                    field.RegisterValueChangedCallback( evt => UpdateProperty( evt.newValue ) );
                else
                    field.SetEnabled( false );
                if ( cheatProp.CanRead ) //Property cheats value should be refreshed if changed externally
                    RefreshFieldValue( () => field.SetValueWithoutNotify( Convert.ToSingle( cheatProp.GetValue( CheatObject, null ))), _cancel );
                return field;
            }

            return null;
        }
        
        private VisualElement GenerateValueField( String cheatName, PropertyInfo cheatProp )
        {
            if( cheatProp.PropertyType == typeof(float) )
                return PrepareTextValueField( new FloatField( cheatName ) );
            else if( cheatProp.PropertyType == typeof(int) )
                return PrepareTextValueField( new IntegerField( cheatName ) );
            else if( cheatProp.PropertyType == typeof(double) )
                return PrepareTextValueField( new DoubleField( cheatName ) );
            else if( cheatProp.PropertyType == typeof(uint) )
                return PrepareTextValueField( new UnsignedIntegerField( cheatName ) );
            else if( cheatProp.PropertyType == typeof(long) )
                return PrepareTextValueField( new LongField( cheatName ) );
            else if( cheatProp.PropertyType == typeof(ulong) )
                return PrepareTextValueField( new UnsignedLongField( cheatName ) );
            else if( cheatProp.PropertyType == typeof(byte) )
                return PrepareSubIntegerField<byte>( new IntegerField( cheatName ), Byte.MinValue, Byte.MaxValue );
            else if( cheatProp.PropertyType == typeof(sbyte) )
                return PrepareSubIntegerField<sbyte>( new IntegerField( cheatName ), SByte.MinValue, SByte.MaxValue );
            else if( cheatProp.PropertyType == typeof(short) )
                return PrepareSubIntegerField<short>( new IntegerField( cheatName ), Int16.MinValue, Int16.MaxValue );
            else if( cheatProp.PropertyType == typeof(ushort) )
                return PrepareSubIntegerField<ushort>( new IntegerField( cheatName ), UInt16.MinValue, UInt16.MaxValue );
            else if( cheatProp.PropertyType == typeof(string) )
                return PrepareTextValueField( new TextField( cheatName ) );

            return null;
            
            TextInputBaseField<T> PrepareTextValueField<T>( TextInputBaseField<T> field )
            {
                field.AddToClassList( "CheatTextBox" );
                field.AddToClassList( "CheatLine" );
                field.isDelayed = true;
                field.RegisterCallback<FocusInEvent>( FieldFocusIn );
                field.RegisterCallback<FocusOutEvent>( FieldFocusOut );
                if ( cheatProp.CanWrite )                        
                    field.RegisterValueChangedCallback( evt => UpdateProperty( evt.newValue ) );
                else
                    field.SetEnabled( false );

                if ( cheatProp.CanRead ) //Property cheats value should be refreshed if changed externally
                    RefreshFieldValue( () => field.SetValueWithoutNotify( (T)cheatProp.GetValue( CheatObject, null )), _cancel );

                return field;
            }

            IntegerField PrepareSubIntegerField<T>( IntegerField field, int min, int max )
            {
                field.AddToClassList( "CheatTextBox" );
                field.AddToClassList( "CheatLine" );
                field.isDelayed = true;
                if ( cheatProp.CanWrite )                        
                    field.RegisterValueChangedCallback( evt => UpdateProperty( Convert.ChangeType(Math.Clamp(evt.newValue, min, max), typeof(T)) ) );
                else
                    field.SetEnabled( false );

                if ( cheatProp.CanRead ) //Property cheats value should be refreshed if changed externally
                    RefreshFieldValue( () => field.SetValueWithoutNotify( Convert.ToInt32( cheatProp.GetValue( CheatObject, null ))), _cancel );

                return field;
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
    }
}