using UnityEngine.UIElements;

namespace Silentor.CheatPanel
{
    internal static class Resources
    {
        public   static readonly VisualTreeAsset CheatGroup = UnityEngine.Resources.Load<VisualTreeAsset>( "CheatGroup" );
        public   static readonly VisualTreeAsset CheatPanelMax = UnityEngine.Resources.Load<VisualTreeAsset>( "CheatPanelMax" );
        public   static readonly VisualTreeAsset CheatPanelMin = UnityEngine.Resources.Load<VisualTreeAsset>( "CheatPanelMin" );
        public   static readonly VisualTreeAsset SystemTab = UnityEngine.Resources.Load<VisualTreeAsset>( "SystemTab" );
    }
}