using System;
using System.Collections.Generic;
using System.Threading;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using Object = System.Object;


namespace Silentor.CheatPanel
{
    public class FpsMeter : IDisposable
    {
        public float    AverageFrameTime => CurrentStats.AverageFrameTime;
        public int      AverageFPS => AverageFrameTime > 0 ? (int)Math.Round(1 / AverageFrameTime) : 0;
        public float    WorstFrameTime => CurrentStats.WorstFrameTime;
        public int      WorstFPS => WorstFrameTime > 0 ? (int)Math.Round(1 / WorstFrameTime) : 0;
        public float    Percentile99FrameTime => CurrentStats.Percentile99FrameTime;
        public int      Percentile99FPS       => Percentile99FrameTime > 0 ? (int)Math.Round(1 / Percentile99FrameTime) : 0;
        public float    Percentile95FrameTime => CurrentStats.Percentile95FrameTime;
        public int      Percentile95FPS       => Percentile95FrameTime > 0 ? (int)Math.Round(1 / Percentile95FrameTime) : 0;
        public float    Percentile90FrameTime => CurrentStats.Percentile90FrameTime;
        public int      Percentile90FPS       => Percentile90FrameTime > 0 ? (int)Math.Round(1 / Percentile90FrameTime) : 0;

        public Stats CurrentStats { get; private set; }

        public IReadOnlyCollection<Stats> LastStats => _lastStats; 

        public Int32 LastStatsCapacity => _lastStatsCapacity;

        /// <summary>
        /// When stats updated
        /// </summary>
        public event Action<FpsMeter> Updated; 

        public FpsMeter( )
        {
            _deltaTimes = new NativeList<Single>(Allocator.Persistent );
            _lastStats = new Queue<Stats>( _lastStatsCapacity );
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

        private async void Update( CancellationToken cancel )
        {
            while ( !cancel.IsCancellationRequested )
            {
                if ( (int)_measureTime > 0 )
                {
                    _measureTime -= (int)_measureTime;
                    BurstHelper.GetStats( in _deltaTimes, out var averageFrame, out var worstFrame, out var percentile99Frame, out var percentile95Frame, out var percentile90Frame );
                    CurrentStats = new Stats(averageFrame, worstFrame, percentile99Frame, percentile95Frame, percentile90Frame);
                    if( LastStats.Count > 60 )                        
                        _lastStats.Dequeue();
                    _lastStats.Enqueue( CurrentStats );
                    _deltaTimes.Clear();
                    Updated?.Invoke( this );
                }

                var dt = Time.unscaledDeltaTime;
                _deltaTimes.Add( dt );
                _measureTime += dt;

                await Awaitable.NextFrameAsync( cancel );
            }
        }

        private float _measureTime;
        private          CancellationTokenSource _cancel;
        private         NativeList<float> _deltaTimes;
        private Int32   _lastStatsCapacity = 60;
        private readonly Queue<Stats> _lastStats;

        public void Dispose( )
        {
            StopMeter();
            _cancel?.Dispose();
            _deltaTimes.Dispose();
        }

        public readonly struct Stats
        {
            public float AverageFrameTime { get; }
            public float WorstFrameTime { get; }
            public float Percentile99FrameTime { get; }
            public float Percentile95FrameTime { get; }
            public float Percentile90FrameTime { get; }

            public Stats(Single averageFrameTime, Single worstFrameTime, Single percentile99FrameTime, Single percentile95FrameTime, Single percentile90FrameTime )
            {
                AverageFrameTime = averageFrameTime;
                WorstFrameTime = worstFrameTime;
                Percentile99FrameTime = percentile99FrameTime;
                Percentile95FrameTime = percentile95FrameTime;
                Percentile90FrameTime = percentile90FrameTime;
            }

            public float GetStat( EFPSStats stat )
            {
                return stat switch
                {
                    EFPSStats.Average => AverageFrameTime,
                    EFPSStats.Worst   => WorstFrameTime,
                    EFPSStats.Percent99 => Percentile99FrameTime,
                    EFPSStats.Percent95 => Percentile95FrameTime,
                    EFPSStats.Percent90 => Percentile90FrameTime,
                    _ => 0
                };
            }
        } 

        public enum EFPSStats
        {
            Worst,
            Percent99,
            Percent95,
            Percent90,
            Average,
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