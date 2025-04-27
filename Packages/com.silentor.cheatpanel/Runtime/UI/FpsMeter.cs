using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Object = System.Object;

namespace Silentor.CheatPanel
{
    public class FpsMeter
    {
        public float AverageFrameTime => _averageFrame;
        public int   AverageFPS => _averageFrame > 0 ? (int)Math.Round(1 / _averageFrame) : 0;
        public float SlowestFrameTime => _slowestFrame;
        public int  SlowestFPS => _slowestFrame > 0 ? (int)Math.Round(1 / _slowestFrame) : 0;

        public void StartMeter( )
        {
            _measureTime = Time.unscaledTime;
            _cancel      = new CancellationTokenSource();
            Update( _cancel.Token );
        }

        public void StopMeter( )
        {
            _cancel?.Cancel();
        }

        private async Awaitable Update( CancellationToken cancel )
        {
            while ( !cancel.IsCancellationRequested )
            {
                if ( (int)_measureTime > 0 )
                {
                    _measureTime -= (int)_measureTime;
                    var slowestFrame = 0f;
                    var averageFrame = 0f;
                    if ( _deltaTimes.Count > 0 )
                    {
                        foreach ( var dT in _deltaTimes )
                        {
                            averageFrame += dT;
                            if ( slowestFrame < dT )
                                slowestFrame = dT;
                        }
                        averageFrame /= _deltaTimes.Count;
                        _deltaTimes.Clear();
                    }

                    _averageFrame = averageFrame;
                    _slowestFrame = slowestFrame;
                }

                var dt = Time.unscaledDeltaTime;
                _deltaTimes.Add( dt );
                _measureTime += dt;

                await Awaitable.NextFrameAsync( cancel );
            }
        }

        private          float                  _measureTime;
        private float _slowestFrame;
        private float _averageFrame;
        private          CancellationTokenSource _cancel;
        private readonly List<float>             _deltaTimes = new ( 30 );
    }
}