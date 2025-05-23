﻿using System;
using System.Reflection;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace Silentor.CheatPanel.Binders
{
    /// <summary>
    /// Binder enum cheat property <-> UI control. Works with boxing.
    /// </summary>
    /// <typeparam name="TField"></typeparam>
    public class PropertyEnumBinder : CheatControlBinderBase
    {
        private readonly EnumField _control;
        private readonly PropertyInfo      _property;
        private readonly ICheats _cheatObject;

        public PropertyEnumBinder( EnumField control, PropertyInfo property, ICheats cheatObject )
        {
            Assert.IsTrue( property.CanRead || property.CanWrite );

            _control            = control;
            _property         = property;
            _cheatObject = cheatObject;

            if ( property.CanWrite )
                _control.RegisterValueChangedCallback( OnControlChanged );
            else
                _control.SetEnabled( false );
        }

        private void OnControlChanged( ChangeEvent<Enum> changeEvent )
        {
            _property.SetValue( _cheatObject, changeEvent.newValue );
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
            if ( _property.CanRead )
            {
                var boxedValue = _property.GetValue( _cheatObject );
                _control.SetValueWithoutNotify( (Enum)boxedValue );
            }
        }
    }
    
}