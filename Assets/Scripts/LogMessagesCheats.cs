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
    }
}