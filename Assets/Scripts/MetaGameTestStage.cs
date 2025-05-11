using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Assertions;
using Debug = UnityEngine.Debug;
using Object = System.Object;

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
           

            var cheats = FindAnyObjectByType<CheatPanel>();

            await Awaitable.WaitForSecondsAsync( 1, destroyCancellationToken );

            cheats.AddCheats( new LogMessagesCheats( this ) );

            //cheats.AddCheats( new MetaGameCheats() );

            //gameObject.AddComponent<HeavyLoadCheats>();
            //cheats.AddCheats(  GetComponent<HeavyLoadCheats>() );

            //Bench();

            //StartCoroutine( LogCoroutine() );
        }

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
            var cheats = new MetaGameCheats();

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
