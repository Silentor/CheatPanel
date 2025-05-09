using System;
using UnityEngine;
using UnityEngine.UIElements;
using Object = System.Object;

namespace Silentor.CheatPanel.UI
{
    public class LogItemElement : VisualElement
    {
        private readonly Label _timeLabel;
        private readonly Label _messageLabel;
        private readonly Label _stackLabel;
        private LogType _logType;
        private readonly Label _threadLabel;

        public const string LogItemUssClassName = "log-item";
        public const string MainLineUssClassName = LogItemUssClassName + "__main";
        public const string TimeUssClassName = LogItemUssClassName + "__time";
        public const string MessageUssClassName = LogItemUssClassName + "__message";
        public const string ThreadUssClassName = LogItemUssClassName + "__thread";
        public const string StackUssClassName = LogItemUssClassName + "__stack";
        public const string StackExpandedUssClassName = StackUssClassName + "--expanded";
        public const string ErrorLogItemUssClassName = LogItemUssClassName + "--error";
        public const string WarningLogItemUssClassName = LogItemUssClassName + "--warning";
        public const string InfoLogItemUssClassName = LogItemUssClassName + "--info";

        public LogItemElement( )
        {
            AddToClassList( LogItemUssClassName );
            var mainLineContainer = new VisualElement();
            mainLineContainer.AddToClassList( MainLineUssClassName );
            Add( mainLineContainer );
            _timeLabel = new Label();
            _timeLabel.AddToClassList( TimeUssClassName );
            mainLineContainer.Add( _timeLabel );
            _threadLabel = new Label();
            _threadLabel.AddToClassList( ThreadUssClassName );
            mainLineContainer.Add( _threadLabel );
            _messageLabel = new Label();
            _messageLabel.AddToClassList( MessageUssClassName );
            mainLineContainer.Add( _messageLabel );
            _stackLabel = new Label();
            _stackLabel.AddToClassList( StackUssClassName );
            Add( _stackLabel );

            var openStackManip = new Clickable( ( ) =>
            {
                _stackLabel.ToggleInClassList( StackExpandedUssClassName );
            } );
            openStackManip.activators.Add( new ManipulatorActivationFilter(){button = MouseButton.LeftMouse, clickCount = 2} );
            mainLineContainer.AddManipulator( openStackManip );
        }

        public void SetLogItem( String time, String threadId, String message, String stackTrace, LogType logType )
        {
            _timeLabel.text = time;
            if ( !String.IsNullOrEmpty( threadId ) )
            {
                _threadLabel.text = threadId;
                _threadLabel.style.display = DisplayStyle.Flex;
            }
            _messageLabel.text = message;
            _stackLabel.text = stackTrace;

            var logTypeStyle = logType switch
            {
                LogType.Error => ErrorLogItemUssClassName,
                LogType.Exception => ErrorLogItemUssClassName,
                LogType.Assert => ErrorLogItemUssClassName,
                LogType.Warning => WarningLogItemUssClassName,
                LogType.Log => InfoLogItemUssClassName,
                _ => throw new ArgumentOutOfRangeException( nameof(logType), logType, null )
            };
            _logType = logType;

            _timeLabel.AddToClassList( logTypeStyle );
            _threadLabel.AddToClassList( logTypeStyle );
            _messageLabel.AddToClassList( logTypeStyle );
            _stackLabel.AddToClassList( logTypeStyle );
        }

        public void Recycle( )
        {
            var logTypeStyle = _logType switch
                               {
                                       LogType.Error   => ErrorLogItemUssClassName,
                                       LogType.Exception => ErrorLogItemUssClassName,
                                       LogType.Assert    => ErrorLogItemUssClassName,
                                       LogType.Warning => WarningLogItemUssClassName,
                                       LogType.Log     => InfoLogItemUssClassName,
                                       _               => throw new ArgumentOutOfRangeException( nameof(_logType), _logType, null )
                               };

            _timeLabel.RemoveFromClassList( logTypeStyle );
            _threadLabel.style.display = StyleKeyword.Null;
            _messageLabel.RemoveFromClassList( logTypeStyle );
            _stackLabel.RemoveFromClassList( logTypeStyle );
            _stackLabel.RemoveFromClassList( StackExpandedUssClassName );
        }
    }
}
