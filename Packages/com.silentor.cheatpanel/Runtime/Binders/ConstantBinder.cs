using System;
using UnityEngine.UIElements;

namespace Silentor.CheatPanel.Binders
{
    public class ConstantBinder : CheatFieldBinderBase
    {
        private readonly Object _value;
        private readonly VisualElement _noneField;

        public ConstantBinder( Object value )
        {
            _value = value;
            _noneField = new VisualElement();
            _noneField.style.display = DisplayStyle.None;
        }

        public override VisualElement GetField( )
        {
            return _noneField;
        }

        public override Object GetBoxedFieldValue( )
        {
            return _value;
        }

        public override void RefreshFieldUI( )
        {
            throw new NotImplementedException();
        }
    }
}