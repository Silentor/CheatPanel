using System;
using System.Reflection;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace Silentor.CheatPanel.Binders
{
    /// <summary>
    /// Binder enum cheat field <-> UI control. Works with boxing.
    /// </summary>
    public class FieldEnumBinder : CheatControlBinderBase
    {
        private readonly EnumField _control;
        private readonly FieldInfo _field;
        private readonly ICheats _cheatObject;

        public FieldEnumBinder( EnumField control, FieldInfo field, ICheats cheatObject )
        {
            _control            = control;
            _field         = field;
            _cheatObject = cheatObject;

            if ( !field.IsInitOnly && !field.IsLiteral )
                _control.RegisterValueChangedCallback( OnControlChanged );
            else
                _control.SetEnabled( false );
        }

        private void OnControlChanged( ChangeEvent<Enum> changeEvent )
        {
            _field.SetValue( _cheatObject, changeEvent.newValue );
        }

        public override VisualElement GetControl( )
        {
            return _control;
        }

        public override Object GetBoxedControlValue( )
        {
            return _control.value;
        }

        public override void RefreshControl( )
        {
            var boxedValue = _field.GetValue( _cheatObject );
            _control.SetValueWithoutNotify( (Enum)boxedValue );
        }
    }
    
}