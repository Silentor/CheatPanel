using System;
using UnityEngine.UIElements;

namespace Silentor.CheatPanel.Binders
{
    public class ConstantBinder : CheatControlBinderBase
    {
        private readonly Object _value;
        private readonly VisualElement _noneField;

        public ConstantBinder( Object value )
        {
            _value = value;
            _noneField = new VisualElement();
            _noneField.style.display = DisplayStyle.None;
        }

        public override VisualElement GetControl( )
        {
            return _noneField;
        }

        public override Object GetBoxedControlValue( )
        {
            return _value;
        }

        public override void RefreshControl( )
        {
            throw new NotImplementedException();
        }
    }
}