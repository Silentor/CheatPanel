using System;
using UnityEngine.UIElements;

namespace Silentor.CheatPanel.UI
{
    [UxmlElement]
    public partial class CheatGroupControl: VisualElement
    {
        public static readonly string cheatGroupUssClassName = "cheat-group";
        public static readonly string headerUssClassName = "cheat-group__header";
        public static readonly string delimUssClassName = "cheat-group__delim";
        public static readonly string delimLeftUssClassName = "cheat-group__delim--left";
        public static readonly string captionUssClassName = "cheat-group__caption";
        public static readonly string contentUssClassName = "cheat-group__content";

        private readonly Label _captionLabel;
        private readonly VisualElement _content;

        public CheatGroupControl( ) : this( "Cheat Group" )
        {

        } 

        public CheatGroupControl( String caption ) : base()
        {
            AddToClassList( cheatGroupUssClassName );
            var header = new VisualElement();
            var leftLine = new VisualElement();
            var rightLine = new VisualElement();
            _captionLabel = new Label();
            _content = new VisualElement();
            header.AddToClassList( headerUssClassName );
            leftLine.AddToClassList( delimUssClassName );
            leftLine.AddToClassList( delimLeftUssClassName );
            rightLine.AddToClassList( delimUssClassName );
            _captionLabel.AddToClassList( captionUssClassName );
            _content.AddToClassList( contentUssClassName );
            Add( header );
            header.Add( leftLine );
            header.Add( _captionLabel );
            header.Add( rightLine );
            Add( _content );

            Caption = caption;
        }

        [UxmlAttribute]
        public String Caption
        {
            get => _captionLabel.text;
            set
            {
                _captionLabel.text = value;
                _captionLabel.style.display = String.IsNullOrEmpty( value ) ? DisplayStyle.None : DisplayStyle.Flex;
            }
        }

        public VisualElement Content => _content;
    }
}