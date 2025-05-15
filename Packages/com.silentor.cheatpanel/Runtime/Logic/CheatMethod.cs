using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Silentor.CheatPanel.Binders;
using Silentor.CheatPanel.Utils;
using UnityEngine;
using UnityEngine.UIElements;
using Object = System.Object;

namespace Silentor.CheatPanel
{
    /// <summary>
    /// Simple sync cheat button without params
    /// </summary>
    public class CheatMethod : Cheat
    {
        private readonly MethodInfo                          _methodInfo;
        private readonly ICheats                             _cheatObject;
        private readonly CheatPanel                          _cheatPanel;

        private VisualElement   _cheatContainer;
        private Button _cheatBtn;
        private Button _cancelBtn;
        private          IReadOnlyList<CheatControlBinderBase> _paramsWrappers;
        private          Object[]                            _params;

        private Boolean _isAnyParams;
        private Boolean _isResultPresent;

        //Awaitable method support
        private Boolean _isAwaitable;
        private Boolean _isCancellable;
        private MethodInfo _getAwaiterMethod;
        private PropertyInfo _isCompletedProperty;
        private MethodInfo _getResultMethod;
        private CancellationTokenSource _cts;
        private int _cancellationTokenParamIndex;


        public CheatMethod( MethodInfo methodInfo, ICheats cheatObject, CheatPanel cheatPanel ) : base( methodInfo, cheatObject )
        {
            _methodInfo  = methodInfo;
            _cheatObject = cheatObject;
            _cheatPanel  = cheatPanel;
        }

        public override String ToString( )
        {
            return $"{_cheatObject.GetType().Name}.{Name}()";
        }

        protected override VisualElement GenerateUI( )
        {
            _cheatContainer = new VisualElement( );
            _cheatBtn       = new Button( );
            _cheatBtn.text = Name;
            _cheatContainer.Add( _cheatBtn );

            _isAnyParams = _methodInfo.GetParameters().Length != 0;
            _isResultPresent = _methodInfo.ReturnType != typeof( void );
            (_isAwaitable, _isCancellable) = IsAwaitable( _methodInfo );

            if ( _isAnyParams )
            {
                _paramsWrappers = GetParamWrappers( );
                foreach ( var w in _paramsWrappers )
                    _cheatContainer.Add( w.GetControl() );
            }
            else
                _params = Array.Empty<Object>();

            if ( _isCancellable )
            {
                _cancelBtn = GenerateCancelButton();
                _cheatContainer.Add( _cancelBtn );  //Invisible by default
            }

            _cheatBtn.AddToClassList( "CheatBtn" );
            _cheatContainer.AddToClassList( "CheatLine" );

            _cheatBtn.clicked += ( ) =>
            {
                if ( _isAnyParams )
                {
                    if ( _params == null )
                        _params = new Object[_paramsWrappers.Count];
                    for ( int i = 0; i < _params.Length; i++ )
                        if( _paramsWrappers[i] != null )
                            _params[ i ] = _paramsWrappers[ i ].GetBoxedControlValue( );
                }

                if ( _isAwaitable )
                {
                    ProcessAsyncCheatCall( _params, CancellationToken.None );
                }
                else
                {
                    var result = _methodInfo.Invoke( _cheatObject, _params );
                    if ( result != null )                        
                        _cheatPanel.ShowResult( result.ToString() );
                }
            };

            return _cheatContainer;
        }

        protected override void RefreshUI( )
        {
            //No need
            throw new System.NotImplementedException();
        }

        private Button GenerateCancelButton( )
        {
            var cancelBtn = new Button( );
            cancelBtn.text                  = String.Empty;
            cancelBtn.style.backgroundImage = new StyleBackground( Resources.CancelIcon );
            cancelBtn.style.display = DisplayStyle.None;
            cancelBtn.AddToClassList( "CheatBtn" );
            cancelBtn.clicked += ( ) =>
            {
                if ( _cts != null )
                {
                    _cts.Cancel( );
                    //_cheatBtn.SetEnabled( true );
                }
            };
            return cancelBtn;
        }

        protected override RefreshUITiming GetRefreshUITiming( )
        {
            return RefreshUITiming.Never;
        }

        private IReadOnlyList<CheatControlBinderBase> GetParamWrappers( )
        {
            var result = new List<CheatControlBinderBase>();
            foreach ( var par in _methodInfo.GetParameters() )
            {
                if ( par.ParameterType == typeof(int) )
                {
                    var field = new IntegerField( null );
                    result.Add( PrepareWrapper( field, par.HasDefaultValue ? (int)par.DefaultValue : 0 ) );
                }
                else if ( par.ParameterType == typeof(uint) )
                {
                    var field = new UnsignedIntegerField( null );
                    result.Add( PrepareWrapper( field, par.HasDefaultValue ? (uint)par.DefaultValue : 0 ) );
                }
                else if ( par.ParameterType == typeof(long) )
                {
                    var field = new LongField( null );
                    result.Add( PrepareWrapper( field, par.HasDefaultValue ? (long)par.DefaultValue : 0 ) );
                }
                else if ( par.ParameterType == typeof(ulong) )
                {
                    var field = new UnsignedLongField( null );
                    result.Add( PrepareWrapper( field, par.HasDefaultValue ? (ulong)par.DefaultValue : 0 ) );
                }
                else if ( par.ParameterType == typeof(byte) )
                {
                    var field = new IntegerField( null );
                    result.Add( PrepareWrapper( field, par.HasDefaultValue ? (byte)par.DefaultValue : 0, i => i.ClampToByte() ) );
                }
                else if ( par.ParameterType == typeof(sbyte) )
                {
                    var field = new IntegerField( null );
                    result.Add( PrepareWrapper( field, par.HasDefaultValue ? (sbyte)par.DefaultValue : 0, i => i.ClampToSByte() ) );
                }
                else if ( par.ParameterType == typeof(ushort) )
                {
                    var field = new IntegerField( null );
                    result.Add( PrepareWrapper( field, par.HasDefaultValue ? (ushort)par.DefaultValue : 0, i => i.ClampToUInt16() ) );
                }
                else if ( par.ParameterType == typeof(short) )
                {
                    var field = new IntegerField( null );
                    result.Add( PrepareWrapper( field, par.HasDefaultValue ? (short)par.DefaultValue : 0, i => i.ClampToInt16() ) );
                }
                else if ( par.ParameterType == typeof( float ) )
                {
                    var field = new FloatField( null );
                    result.Add( PrepareWrapper( field, par.HasDefaultValue ? (float)par.DefaultValue : 0 ) );
                }
                else if ( par.ParameterType == typeof( double ) )
                {
                    var field = new DoubleField( null );
                    result.Add( PrepareWrapper( field, par.HasDefaultValue ? (double)par.DefaultValue : 0 ) );
                }
                else if ( par.ParameterType == typeof( string ) )
                {
                    var field = new TextField( null );
                    result.Add( PrepareWrapper( field, par.HasDefaultValue ? (String)par.DefaultValue : String.Empty ) );
                }
                else if ( par.ParameterType == typeof( bool ) )
                {
                    var field = new Toggle( null );
                    result.Add( PrepareWrapper( field, par.HasDefaultValue ? (Boolean)par.DefaultValue : false ) );
                }
                else if ( par.ParameterType.IsEnum )
                {
                    var value = (Enum)(par.HasDefaultValue ? par.DefaultValue : par.ParameterType.GetEnumValues().GetValue( 0 ));
                    var field = new EnumField( null, value );
                    result.Add( PrepareWrapper( field, value ) );
                }
                else
                {
                    //Unsupported type, use default value as parameter
                    result.Add( new ConstantBinder( null ) );
                }
            }

            return result;
        }

        private async void ProcessAsyncCheatCall( Object[] paramz, CancellationToken cancel )
        {
            _cheatBtn.SetEnabled( false );
            if ( _isCancellable )
            {
                _cts = new CancellationTokenSource();
                paramz[ _cancellationTokenParamIndex ] = _cts.Token;
                _cancelBtn.style.display = DisplayStyle.Flex;
            }

            try
            {
                var awaitable = _methodInfo.Invoke( _cheatObject, paramz );
                var awaiter   = _getAwaiterMethod.Invoke( awaitable, null );

                //Wait for completion
                while ( !(Boolean)_isCompletedProperty.GetValue( awaiter, null ) )
                {
                    await Awaitable.NextFrameAsync( cancel );
                }

                if ( _isResultPresent )
                {
                    var result = _getResultMethod.Invoke( awaiter, null );
                    if( result != null )
                        _cheatPanel.ShowResult( result.ToString() );
                    else
                        _cheatPanel.ShowResult( "null" );
                }
                else
                {
                    _getResultMethod.Invoke( awaiter, null );           //Just want to check for exception/cancellation
                }
            }
            catch (TargetInvocationException e )    
            {
                var innerException = e.InnerException;
                if( innerException is OperationCanceledException )
                {}                                              //Its ok
                else
                {
                    var errorMsg = $"Cheat method {this} call failed: {innerException.Message}";
                    _cheatPanel.ShowResult( errorMsg );
                }
            }
            finally
            {
                _cheatBtn.SetEnabled( true );
                if ( _isCancellable )
                {
                    _cts.Dispose();
                    _cancelBtn.style.display = DisplayStyle.None;
                }
            }
        }

        private (Boolean, Boolean) IsAwaitable( MethodInfo mi )
        {
            var returnType = mi.ReturnType;
            _getAwaiterMethod = returnType.GetMethod( "GetAwaiter" );
            if ( _getAwaiterMethod != null )
            {
                var awaiterType = _getAwaiterMethod.ReturnType;
                var isCompletedProperty = awaiterType.GetProperty( "IsCompleted" );
                var getResultMethod = awaiterType.GetMethod( "GetResult" );
                if ( isCompletedProperty != null && getResultMethod != null )
                {
                    _isCompletedProperty = isCompletedProperty;
                    _getResultMethod = getResultMethod;
                    _isResultPresent = getResultMethod.ReturnType != typeof( void );
                    
                    var isCancellable = false;
                    var paramz = mi.GetParameters( );
                    if( paramz.Count( p => p.ParameterType == typeof(CancellationToken) ) == 1 )
                    {
                        _cancellationTokenParamIndex = Array.FindIndex( paramz, p => p.ParameterType == typeof(CancellationToken) );
                        isCancellable = true;
                    }

                    return (true, isCancellable);
                }
            }

            return (false, false);
        }

        private CheatControlBinderBase PrepareWrapper<T>( BaseField<T> field, T value )
        {
            var wrapper = new ParameterSimpleBinder<T>( field, value );
            return wrapper;
        }

        private CheatControlBinderBase PrepareWrapper<TField, TParam>( BaseField<TField> field, TField value, Func<TField, TParam> fieldToParam )
        {
            var wrapper = new ParameterBinder<TField, TParam>( field, value, fieldToParam );
            return wrapper;
        }

        private static class Resources
        {
            public static readonly Sprite CancelIcon = UnityEngine.Resources.Load<Sprite>( "cancel" );
        }
    }
}