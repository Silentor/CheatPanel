using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using UnityEngine;
using UnityEngine.UIElements;
using Object = System.Object;

namespace Silentor.CheatPanel
{
    /// <summary>
    /// Simple sync cheat button without params
    /// </summary>
    public class CheatMethodParams : Cheat
    {
        private readonly MethodInfo _methodInfo;
        private readonly ICheats    _cheatObject;
        private readonly CheatPanel _cheatPanel;

        public CheatMethodParams( MethodInfo methodInfo, ICheats cheatObject, CheatPanel cheatPanel, CancellationToken cancel ) : base( methodInfo, cheatObject, cancel )
        {
            _methodInfo      = methodInfo;
            _cheatObject     = cheatObject;
            _cheatPanel = cheatPanel;
        }

        protected override VisualElement GenerateUI( )
        {
            var container = new VisualElement( );
            var paramsElements = GetParamsElements( );
            var btn = new Button( );
            btn.text = Name;
            btn.clicked += ( ) =>
            {
                var result = _methodInfo.Invoke( _cheatObject, null );
                if( result != null )
                {
                    _cheatPanel.ShowResult( result.ToString() );
                }
            };

            container.Add( btn );
            foreach ( var element in paramsElements )                
                container.Add( element );
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

        private IReadOnlyList<VisualElement> GetParamsElements( )
        {
            var result = new List<VisualElement>();
            foreach ( var par in _methodInfo.GetParameters() )
            {
                if( par.ParameterType == typeof(int) )
                {
                    var field = new IntegerField( null );
                    field.AddToClassList( "CheatTextBox" );
                    if( par.HasDefaultValue )
                        field.value = (int)par.DefaultValue;
                    result.Add( field );
                }
                else if( par.ParameterType == typeof( float ) )
                {
                    var field = new FloatField( null );
                    field.AddToClassList( "CheatTextBox" );
                    if( par.HasDefaultValue )
                        field.value = (float)par.DefaultValue;
                    result.Add( field );
                }
                else if( par.ParameterType == typeof( string ) )
                {
                    var field = new TextField( null );
                    field.AddToClassList( "CheatTextBox" );
                    if( par.HasDefaultValue )
                        field.value = (string)par.DefaultValue;
                    result.Add( field );
                }
                else if( par.ParameterType == typeof( bool ) )
                {
                    var field = new Toggle( null );
                    field.AddToClassList( "CheatToggle" );
                    if( par.HasDefaultValue )
                        field.value = (bool)par.DefaultValue;
                    result.Add( field );
                }
                else if( par.ParameterType.IsEnum )
                {
                    var defaultValue = (Enum)(par.HasDefaultValue ? par.DefaultValue : par.ParameterType.GetEnumValues().GetValue(0));
                    var field        = new EnumField( null, defaultValue );
                    field.AddToClassList( "CheatTextBox" );
                    result.Add( field );
                }
                else
                {
                    //Unsupported type
                    Debug.LogError( $"Unsupported parameter type: {par.ParameterType}" );
                }
            }

            return result;
        }

        //private IReadOnlyList<Object> GetParameters( IReadOnlyList<VisualElement> ){}

        public struct ElementWithValue
        {
            public VisualElement Element;
            public Object        GetValue( )
            {
                if( Element is IntegerField intField )
                    return intField.value;
                else if( Element is FloatField floatField )
                    return floatField.value;
                else if( Element is TextField textField )
                    return textField.value;
                else if( Element is Toggle toggle )
                    return toggle.value;
                else if( Element is EnumField enumField )
                    return enumField.value;
                else
                    throw new NotSupportedException( $"Unsupported element type: {Element.GetType()}" );
            }
        }
    }
}