using System;
using UnityEngine.UIElements;

namespace Silentor.CheatPanel
{
    public abstract class CheatFieldWrapperBase
    {
        public abstract  VisualElement GetField();
        public abstract Object        GetBoxedFieldValue( );
        public abstract void          RefreshFieldUI( );
    }
}