using System;
using UnityEngine;

namespace Silentor.CheatPanel.DevProject
{
    public class LogMessagesCheats: ICheats
    {
        public void Log( )
        {
            Debug.Log( "Log message" );
        }

        public void Warning( )
        {
            Debug.LogWarning( "Log message" );
        }

        public void Error( )
        {
            Debug.LogError( "Log message" );
        }

        public void ThrowException( )
        {
            throw new Exception( "Log message" );
        }

        public void LogException( )
        {
            Debug.LogException( new Exception("Some exception") );
        }

        public void Assert( )
        {
            Debug.Assert( false, "Log message" );
        }
    }
}