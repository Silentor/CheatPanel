using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

namespace Silentor.CheatPanel.UI
{
    /// <summary>
    /// Custom control with completely custom drawing: histogram of FPS values
    /// </summary>
    [UxmlElement]
    public partial class FpsHistogram : VisualElement
    {
        static readonly CustomStyleProperty<Color> S_ExcellentFrameColor = new ("--excellent-frame-color"); //Within 10% of target FPS
        static readonly CustomStyleProperty<Color> S_MediocreFrameColor = new ("--mediocre-frame-color"); //Within 20% of target FPS
        static readonly CustomStyleProperty<Color> S_BadFrameColor = new ("--bad-frame-color"); //Outside of mediocre range
        static readonly CustomStyleProperty<Color> S_TopLineColor = new ("--top-line-color"); 

        private IReadOnlyCollection<FpsMeter.Stats> _stats;
        private float _targetFrameTime;
        private FpsMeter.EFPSStats _stat;
        private Color _excellentFrameColor = Color.green;
        private Color _mediocreFrameColor = Color.yellow;
        private Color _badFrameColor = Color.red;
        private Color _topLineColor = Color.gray;
        private Int32 _capacity;

        public FpsHistogram( )
        {
            generateVisualContent += OnGenerateVisualContent;
            RegisterCallback<CustomStyleResolvedEvent>(OnStylesResolved);
        }

        private void OnStylesResolved(CustomStyleResolvedEvent evt )
        {
            if ( evt.customStyle.TryGetValue( S_ExcellentFrameColor, out var excellent ) )
                _excellentFrameColor = excellent;
            if ( evt.customStyle.TryGetValue( S_MediocreFrameColor, out var mediocre ) )
                _mediocreFrameColor = mediocre;
            if ( evt.customStyle.TryGetValue( S_BadFrameColor, out var bad ) )
                _badFrameColor = bad;
            if ( evt.customStyle.TryGetValue( S_TopLineColor, out var topLine ) )
                _topLineColor = topLine;
        }

        public void SetFPS( IReadOnlyCollection<FpsMeter.Stats> stats, int capacity, float targetFrameTime, FpsMeter.EFPSStats statToDisplay)
        {
            _stats = stats;
            _capacity = capacity;
            _targetFrameTime = targetFrameTime;
            _stat = statToDisplay;
            MarkDirtyRepaint();
        }

        private void OnGenerateVisualContent(MeshGenerationContext ctx )
        {
            if ( _stats == null || _stats.Count == 0 )
                return;

            var rect = contentRect;
            var painter = ctx.painter2D;

            var barWidth = rect.width /_capacity;
            var targetFPS = 1 / _targetFrameTime;
            var excellentFPS = targetFPS - targetFPS * 0.1f;
            var mediocreFPS  = targetFPS - targetFPS * 0.2f;

            painter.strokeColor = _topLineColor;
            painter.BeginPath();
            painter.MoveTo( rect.min );
            painter.LineTo( new Vector2(rect.max.x, rect.min.y) );      //Top line
            painter.Stroke();
            
            painter.lineJoin = LineJoin.Bevel;

            var i = _capacity - _stats.Count;
            foreach ( var stat in _stats )
            {
                var barXMin   = rect.min.x + i * barWidth;
                var barXMax   = barXMin    + barWidth;
                var frameTime = stat.GetStat( _stat );
                var barFPS    = ( frameTime > 0 ? 1 / frameTime : 0 );
                if ( barFPS == 0 )
                {
                    i++;
                    continue;
                }

                Color barColor;
                if ( barFPS > excellentFPS )
                    barColor = _excellentFrameColor;
                else if ( barFPS > mediocreFPS )
                    barColor = _mediocreFrameColor;
                else
                    barColor = _badFrameColor;

                painter.strokeColor = barColor;
                painter.fillColor = barColor;

                var barHeight = rect.yMax - math.saturate( barFPS / targetFPS ) * rect.height;
                painter.BeginPath();
                painter.MoveTo( new Vector2( barXMin, rect.max.y ) );
                painter.LineTo( new Vector2( barXMin, barHeight ) );
                painter.LineTo( new Vector2( barXMax, barHeight ) );
                painter.LineTo( new Vector2( barXMax, rect.max.y ) );
                painter.ClosePath();
                painter.Fill(  );
                i++;
            }
        }
    }
}