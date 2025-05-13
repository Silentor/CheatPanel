using System;
using Unity.Collections.LowLevel.Unsafe;

namespace Silentor.CheatPanel.UI
{
    /// <summary>
    /// To be backend property for UI binding
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SmartProperty<T> where T : IEquatable<T>
    {
        public T Value
        {
            get => _getter();
            set
            {
                _lastValue = value;
                _setter( value );
            }
        }

        public bool CheckExternalChanges( )
        {
            var currentValue = _getter();
            if ( _lastValue.Equals( currentValue ) )
                return false;

            _lastValue = currentValue;
            return true;
        }

        public SmartProperty( Func<T> getter, Action<T> setter )
        {
            _getter = getter;
            _setter = setter;
            _lastValue = _getter();
        }

        private readonly Func<T>   _getter;
        private readonly Action<T> _setter;
        private T              _lastValue;
    }

    public class SmartPropertyEnum<T> where T : struct, IConvertible
    {
        public T Value
        {
            get => _getter();
            set
            {
                _lastValue = value;
                _setter( value );
            }
        }

        public bool CheckExternalChanges( )
        {
            var currentValue = _getter();

            if( UnsafeUtility.EnumEquals( currentValue, _lastValue ))
                return false;

            _lastValue = currentValue;
            return true;
        }

        public SmartPropertyEnum( Func<T> getter, Action<T> setter )
        {
            _getter    = getter;
            _setter    = setter;
            _lastValue = _getter();
        }

        private readonly Func<T>   _getter;
        private readonly Action<T> _setter;
        private          T         _lastValue;
    }
}