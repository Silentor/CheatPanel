using System;
using UnityEngine.UIElements;

namespace Silentor.CheatPanel.Binders
{
    /// <summary>
    /// Binder for UI control and some cheat element
    /// </summary>
    public abstract class CheatFieldBinderBase
    {
        public abstract  VisualElement GetField();
        public abstract Object        GetBoxedFieldValue( );
        public abstract void          RefreshFieldUI( );
    }
}