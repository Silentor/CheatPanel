
using System;
using UnityEngine;

namespace Silentor.CheatPanel
{
    public class Settings
    {
        private static readonly String SettingsKey = $"{Application.identifier}.CheatPanelSettings";

        private SettingsDTO _settingsInstance;

        public SettingsDTO GetSettings()
        {
            var settings = PlayerPrefs.GetString(SettingsKey, string.Empty);
            if ( string.IsNullOrEmpty( settings ) )
            {
                _settingsInstance = new SettingsDTO();
                return _settingsInstance;
            }

            try
            {
                _settingsInstance = JsonUtility.FromJson<SettingsDTO>(settings);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load settings: {e}. Using default settings.");
                _settingsInstance = new SettingsDTO();
            }

            return _settingsInstance;
        }

        public void UpdateSettings( )
        {
            var jsonSettings = JsonUtility.ToJson(_settingsInstance);
            PlayerPrefs.SetString(SettingsKey, jsonSettings);
            PlayerPrefs.Save();
        }

    }

    [Serializable]
    public class SettingsDTO
    {
        public bool IsMaximized;
        public int FPSHistogrammMode = (int)FpsMeter.EFPSStats.Worst;
        public bool UpdateFPSHistogramm = true;

        public TEnum GetEnumValueSafe<TEnum>( int value ) where TEnum : Enum
        {
            if ( Enum.IsDefined( typeof(TEnum), value ) )
                return (TEnum)Enum.ToObject( typeof(TEnum), value );

            return (TEnum)Enum.ToObject( typeof(TEnum), Enum.GetValues( typeof(TEnum) ).GetValue( 0 ) );
        } 

        public FpsMeter.EFPSStats GetFPSHistogrammMode( )
        {
            return GetEnumValueSafe<FpsMeter.EFPSStats>( FPSHistogrammMode );
        }


    }
}