using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Assertions;
using Debug = UnityEngine.Debug;

namespace Silentor.CheatPanel.DevProject
{
    public class MetaGameTestStage : MonoBehaviour
    {
        private void Awake( )
        {
            Application.targetFrameRate = 30;
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        async void Start()
        {
            //StartCoroutine( CallAwaitable() );

            

            //Debug.unityLogger.filterLogType

            var cheats = FindAnyObjectByType<CheatPanel>();

            await Awaitable.WaitForSecondsAsync( 1, destroyCancellationToken );

            //cheats.AddCheats( new LogMessagesCheats( this ) );
            cheats.AddCheats( new MethodCheats( ) );

            //await Awaitable.WaitForSecondsAsync( 3, destroyCancellationToken );

            //Debug.unityLogger.filterLogType = LogType.Exception;

            //cheats.AddCheats( new MetaGameCheats() );

            //gameObject.AddComponent<HeavyLoadCheats>();
            //cheats.AddCheats(  GetComponent<HeavyLoadCheats>() );

            //Bench();

            //StartCoroutine( LogCoroutine() );
        }

        // IEnumerator CallAwaitable( )
        // {
        //     Debug.Log( "Start call awaitable" );
        //
        //     var mi         = this.GetType().GetMethod( nameof(TestAwaitable) );
        //     var getAwaiter = mi.ReturnType.GetMethod( "GetAwaiter" );
        //
        //     if( getAwaiter != null )
        //     {
        //         var awaitable  = mi.Invoke( this, null );
        //         var awaiter = getAwaiter.Invoke( awaitable, null );
        //         var isCompletedProp = awaiter.GetType().GetProperty( "IsCompleted" );
        //
        //         while( !((Boolean)isCompletedProp.GetValue( awaiter )))
        //         {
        //             yield return null;
        //         }
        //
        //         var resultMethod = awaiter.GetType().GetMethod( "GetResult" );
        //         var isResultPresent = resultMethod.ReturnType != typeof(void);
        //         if ( isResultPresent )
        //         {
        //             var result = resultMethod.Invoke( awaiter, null );
        //             Debug.Log( result );
        //         }
        //         else
        //         {
        //             Debug.Log( "No result" );
        //         }
        //     }
        // }

        // public async UniTask<String> TestAwaitable( )
        // {
        //     //await   Task.Delay( 1000 );
        //     await Awaitable.WaitForSecondsAsync( 1 );
        //     return "Unitask result";
        // }

        IEnumerator LogCoroutine( )
        {
            while ( true )
            {
                Debug.Log( "test " + Time.time );
                yield return new WaitForSecondsRealtime( 1 );
            }
        } 

        private void Bench( )
        {
            var cheats = new PropertyCheats();

            var speedprop      = cheats.GetType().GetProperty( "Speed" );
            var speed1         = (float)speedprop.GetValue( cheats );
            var speedGetmethod = speedprop.GetGetMethod();
            var speed2         = (float)speedGetmethod.Invoke( cheats, null );
            var delUntyped     = Delegate.CreateDelegate( typeof(Func<float>), cheats, speedGetmethod );
            var delTyped       = (Func<float>)delUntyped;
            //var speed3         = delUntyped.;
            var speed4         = delTyped();

            Assert.IsTrue( speed1 == speed2 );
            Assert.IsTrue( speed2 == speed4 );

            var timer = Stopwatch.StartNew();
            for ( int i = 0; i < 100000; i++ )
            {
               speed1 += (float)speedprop.GetValue( cheats );
            }
            timer.Stop();
            UnityEngine.Debug.Log( timer.ElapsedMilliseconds );
            timer.Restart();
            for ( int i = 0; i < 100000; i++ )
            {
                speed2 += (float)speedGetmethod.Invoke( cheats, null );
            }
            timer.Stop();
            UnityEngine.Debug.Log( timer.ElapsedMilliseconds );
            timer.Restart();
            for ( int i = 0; i < 100000; i++ )
            {
                speed4 += delTyped();
            }
            timer.Stop();
            UnityEngine.Debug.Log( timer.ElapsedMilliseconds );

            Assert.IsTrue( speed1 == speed2 );
            Assert.IsTrue( speed2 == speed4 );
        }


        
    }
}
