using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Silentor.CheatPanel.Utils;
using UnityEngine.UIElements;
using Object = System.Object;

namespace Silentor.CheatPanel
{
    /// <summary>
    /// Simple sync cheat button without params
    /// </summary>
    public class CheatMethodParams : Cheat
    {
        private readonly MethodInfo                  _methodInfo;
        private readonly ICheats                     _cheatObject;
        private readonly CheatPanel                  _cheatPanel;
        private          IReadOnlyList<CheatFieldWrapperBase> _paramsWrappers;
        private          Object[]                    _params;

        public CheatMethodParams( MethodInfo methodInfo, ICheats cheatObject, CheatPanel cheatPanel, CancellationToken cancel ) : base( methodInfo, cheatObject, cancel )
        {
            _methodInfo      = methodInfo;
            _cheatObject     = cheatObject;
            _cheatPanel = cheatPanel;
        }

        protected override VisualElement GenerateUI( )
        {
            var container = new VisualElement( );
            var btn = new Button( );
            btn.text = Name;
            btn.clicked += ( ) =>
            {
                if( _params == null )
                    _params = new Object[_paramsWrappers.Count];
                for ( int i = 0; i < _params.Length; i++ )                    
                    _params[i] = _paramsWrappers[i].GetBoxedFieldValue( );
                var result = _methodInfo.Invoke( _cheatObject, _params );
                if( result != null )
                {
                    _cheatPanel.ShowResult( result.ToString() );
                }
            };

            container.Add( btn );
            _paramsWrappers = GetParamWrappers( );
            foreach ( var w in _paramsWrappers )                
                container.Add( w.GetField() );
            btn.AddToClassList( "CheatBtn" );
            container.AddToClassList( "CheatLine" );

            return container;
        }

        protected override void RefreshUI( )
        {
            //No need
            throw new System.NotImplementedException();
        }

        protected override RefreshUITiming GetRefreshUITiming( )
        {
            return RefreshUITiming.Never;
        }

        private IReadOnlyList<CheatFieldWrapperBase> GetParamWrappers( )
        {
            var result = new List<CheatFieldWrapperBase>();
            foreach ( var par in _methodInfo.GetParameters() )
            {
                if( par.ParameterType == typeof(int) )
                {
                    var field = new IntegerField( null );
                    field.AddToClassList( "CheatTextBox" );
                    result.Add( PrepareWrapper( field, par.HasDefaultValue ? (int)par.DefaultValue : 0 ) );
                }
                else if( par.ParameterType == typeof(uint) )
                {
                    var field = new UnsignedIntegerField( null );
                    field.AddToClassList( "CheatTextBox" );
                    result.Add( PrepareWrapper( field, par.HasDefaultValue ? (uint)par.DefaultValue : 0 ) );
                }
                else if( par.ParameterType == typeof(long) )
                {
                    var field = new LongField( null );
                    field.AddToClassList( "CheatTextBox" );
                    result.Add( PrepareWrapper( field, par.HasDefaultValue ? (long)par.DefaultValue : 0 ) );
                }
                else if( par.ParameterType == typeof(ulong) )
                {
                    var field = new UnsignedLongField( null );
                    field.AddToClassList( "CheatTextBox" );
                    result.Add( PrepareWrapper( field, par.HasDefaultValue ? (ulong)par.DefaultValue : 0 ) );
                }
                else if( par.ParameterType == typeof(byte) )
                {
                    var field = new IntegerField( null );
                    field.AddToClassList( "CheatTextBox" );
                    result.Add( PrepareWrapper( field, par.HasDefaultValue ? (byte)par.DefaultValue : 0, i => i.ClampToUInt8() ) );
                }
                else if( par.ParameterType == typeof(sbyte) )
                {
                    var field = new IntegerField( null );
                    field.AddToClassList( "CheatTextBox" );
                    result.Add( PrepareWrapper( field, par.HasDefaultValue ? (sbyte)par.DefaultValue : 0, i => i.ClampToInt8() ) );
                }
                else if( par.ParameterType == typeof(ushort) )
                {
                    var field = new IntegerField( null );
                    field.AddToClassList( "CheatTextBox" );
                    result.Add( PrepareWrapper( field, par.HasDefaultValue ? (ushort)par.DefaultValue : 0, i => i.ClampToUInt16() ) );
                }
                else if( par.ParameterType == typeof(short) )
                {
                    var field = new IntegerField( null );
                    field.AddToClassList( "CheatTextBox" );
                    result.Add( PrepareWrapper( field, par.HasDefaultValue ? (short)par.DefaultValue : 0, i => i.ClampToInt16() ) );
                }
                else if( par.ParameterType == typeof( float ) )
                {
                    var field = new FloatField( null );
                    field.AddToClassList( "CheatTextBox" );
                    result.Add( PrepareWrapper( field, par.HasDefaultValue ? (float)par.DefaultValue : 0 ) );
                }
                else if( par.ParameterType == typeof( double ) )
                {
                    var field = new DoubleField( null );
                    field.AddToClassList( "CheatTextBox" );
                    result.Add( PrepareWrapper( field, par.HasDefaultValue ? (double)par.DefaultValue : 0 ) );
                }
                else if( par.ParameterType == typeof( string ) )
                {
                    var field = new TextField( null );
                    field.AddToClassList( "CheatTextBox" );
                    result.Add( PrepareWrapper( field, par.HasDefaultValue ? (String)par.DefaultValue : String.Empty ) );
                }
                else if( par.ParameterType == typeof( bool ) )
                {
                    var field = new Toggle( null );
                    field.AddToClassList( "CheatToggle" );
                    result.Add( PrepareWrapper( field, par.HasDefaultValue ? (Boolean)par.DefaultValue : false ) );
                }
                else if( par.ParameterType.IsEnum )
                {
                    var value = (Enum)(par.HasDefaultValue ? par.DefaultValue : par.ParameterType.GetEnumValues().GetValue(0));
                    var field = new EnumField( null, value );
                    field.AddToClassList( "CheatEnum" );
                    result.Add( PrepareWrapper( field, value ) );
                }
                else
                {
                    //Unsupported type
                    throw new InvalidOperationException( $"Unsupported cheat method parameter type: {par.ParameterType}" );
                }
            }

            return result;
        }

        private CheatFieldWrapperBase PrepareWrapper<T>( BaseField<T> field, T value )
        {
            var wrapper = new ParameterFieldSimpleWrapper<T>( field, value );
            return wrapper;
        }

        private CheatFieldWrapperBase PrepareWrapper<TField, TParam>( BaseField<TField> field, TField value, Func<TField, TParam> fieldToParam )
        {
            var wrapper = new ParameterFieldWrapper<TField, TParam>( field, value, fieldToParam );
            return wrapper;
        }
    }
}