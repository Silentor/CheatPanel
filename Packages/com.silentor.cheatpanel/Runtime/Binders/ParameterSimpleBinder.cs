using System;
using UnityEngine.UIElements;

namespace Silentor.CheatPanel.Binders
{
    /// <summary>
    /// Binder for some value and UI control. Used for cheat method parameters
    /// </summary>
    public class ParameterSimpleBinder<T> : CheatFieldBinderBase
    {
        private readonly BaseField<T> _field;

        public ParameterSimpleBinder( BaseField<T> field )
        {
            _field = field;
        }

        public ParameterSimpleBinder( BaseField<T> field, T defaultValue )
        {
            _field = field;
            _field.SetValueWithoutNotify( defaultValue );
        }

        public override VisualElement GetField( )
        {
            return _field;
        }

        public override Object GetBoxedFieldValue( )
        {
            return _field.value;
        }

        public override void RefreshFieldUI( )
        {
            //Not applicable
            throw new NotImplementedException();
        }
    }
}