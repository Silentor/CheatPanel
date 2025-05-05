using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using Object = System.Object;
using Screen = UnityEngine.Device.Screen;
using SystemInfo = UnityEngine.Device.SystemInfo;
using Application = UnityEngine.Device.Application;
using Debug = UnityEngine.Debug;


namespace Silentor.CheatPanel
{
    public class FpsMeter : IDisposable
    {
        public float    AverageFrameTime => _averageFrame;
        public int      AverageFPS => _averageFrame > 0 ? (int)Math.Round(1 / _averageFrame) : 0;
        public float    SlowestFrameTime => _slowestFrame;
        public int      SlowestFPS => _slowestFrame > 0 ? (int)Math.Round(1 / _slowestFrame) : 0;
        public float    Percentile99FrameTime => _percentile99Frame;
        public int      Percentile99FPS       => _percentile99Frame > 0 ? (int)Math.Round(1 / _percentile99Frame) : 0;
        public float    Percentile95FrameTime => _percentile95Frame;
        public int      Percentile95FPS       => _percentile95Frame > 0 ? (int)Math.Round(1 / _percentile95Frame) : 0;
        public float    Percentile90FrameTime => _percentile90Frame;
        public int      Percentile90FPS       => _percentile90Frame > 0 ? (int)Math.Round(1 / _percentile90Frame) : 0;


        public FpsMeter( )
        {
            _deltaTimes = new NativeList<Single>(Allocator.Persistent );
        }

        public void StartMeter( )
        {
            _deltaTimes.Clear();
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
                    BurstHelper.GetStats( in _deltaTimes, out _averageFrame, out _slowestFrame, out _percentile99Frame, out _percentile95Frame, out _percentile90Frame );
                    _deltaTimes.Clear();
                }

                var dt = Time.unscaledDeltaTime;
                _deltaTimes.Add( dt );
                _measureTime += dt;

                await Awaitable.NextFrameAsync( cancel );
            }
        }

        private float _measureTime;
        private float _slowestFrame;
        private float _averageFrame;
        private float _percentile99Frame;
        private float _percentile95Frame;
        private float _percentile90Frame;
        private          CancellationTokenSource _cancel;
        private         NativeList<float> _deltaTimes;

        public void Dispose( )
        {
            StopMeter();
            _cancel?.Dispose();
            _deltaTimes.Dispose();
        }

        [BurstCompile]
        private static class BurstHelper
        {
            [BurstCompile]
            public static void GetStats( in NativeList<float> deltaTimes, out float frameAverage, out float frameMin, out float frame99, out float frame95, out float frame90 )
            {
                if( deltaTimes.Length == 0 )
                {
                    frameMin = 0;
                    frame99  = 0;
                    frame95  = 0;
                    frame90  = 0;
                    frameAverage = 0;
                    return;
                }
            
                deltaTimes.Sort();

                var percentiles = new float3( 0.99f, 0.95f, 0.9f );
                var length = new int3( deltaTimes.Length - 1 );
                var indices = math.round( percentiles * length );

                frameMin = deltaTimes[deltaTimes.Length - 1];
                frame99 = deltaTimes[(int)indices.x];
                frame95  = deltaTimes[(int)indices.y];
                frame90  = deltaTimes[(int)indices.z];

                float average = 0;
                for ( int i = 0; i < deltaTimes.Length; i++ )
                {
                    average += deltaTimes[i];
                }
                average /= deltaTimes.Length;
                frameAverage = average;
            }

        }
    }
}