using UnityEngine;

namespace Silentor.CheatPanel.DevProject
{
    public class MetaGameTestStage : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            var cheats = FindAnyObjectByType<CheatPanel>();
            cheats.AddCheats( new MetaGameCheats() );
        }

       
    }
}
