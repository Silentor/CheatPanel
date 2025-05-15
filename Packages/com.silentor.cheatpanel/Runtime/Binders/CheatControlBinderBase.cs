using System;
using UnityEngine.UIElements;

namespace Silentor.CheatPanel.Binders
{
    /// <summary>
    /// Binder for UI control and some cheat element
    /// </summary>
    public abstract class CheatControlBinderBase
    {
        public abstract  VisualElement GetControl();
        public abstract Object        GetBoxedControlValue( );
        public abstract void          RefreshControl( );
    }
}