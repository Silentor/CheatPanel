using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.Jobs;
using UnityEngine;
using Object = System.Object;

namespace Silentor.CheatPanel.DevProject
{
    public class LogMessagesCheats: ICheats
    {
        private readonly MonoBehaviour _host;
        private Coroutine _floodCoroutine;

        public LogMessagesCheats( MonoBehaviour host )
        {
            _host = host;
        }

        public void Log( )
        {
            Debug.Log( "Log message" );
        }

        public void Warning( )
        {
            Debug.LogWarning( "Warn message" );
        }

        public void Error( )
        {
            Debug.LogError( "Error message" );
        }

        public void ThrowException( )
        {
            throw new Exception( "Throw Exception!!!" );
        }

        public void LogException( )
        {
            Debug.LogException( new Exception("Log exception") );
        }

        public void Assert( )
        {
            Debug.Assert( false, "Assert" );
        }

        public void LogFromAnotherThread( )
        {
            Thread thread = new Thread( () =>
            {
                Debug.Log( "Log from another thread" );
            } );
            thread.Start();
        }

        public void LogFromAnotherThreadWithException( )
        {
            Thread thread = new Thread( () =>
            {
                throw new Exception( "Log from another thread with exception" );
            } );
            thread.Start();
        }

        public void LogFromAwaitableBackgroundThread( )
        {
            Log();
            async Awaitable Log( )
            {
                await Awaitable.BackgroundThreadAsync();
                Debug.Log( "Log from Awaitable.BackgroundThreadAsync()" );
            }
        }

        public void LogFromJob( )
        {
            DoneJobOnThread();

            async Awaitable DoneJobOnThread( )
            {
                var handle = new TestJob().Schedule();
                await Awaitable.NextFrameAsync(  );
                handle.Complete();
            }
        }

        public struct TestJob : IJob
        {
            public void Execute( )
            {
                Debug.Log( "Log from IJob" );
            }
        }

        public void FloodLogs( )
        {
            _floodCoroutine = _host.StartCoroutine( FloodLogsCoroutine() );
        }

        public void StopFloodLogs( )
        {
            _host.StopCoroutine( _floodCoroutine );
        }

        private IEnumerator FloodLogsCoroutine( )
        {
            var counter = 0;
            while ( true )
            {
                Debug.Log( $"Flood log {counter++}" );
                yield return new WaitForSeconds( 0.1f );
            }
        }
    }
}