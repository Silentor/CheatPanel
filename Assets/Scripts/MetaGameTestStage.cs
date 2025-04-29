using UnityEngine;

namespace Silentor.CheatPanel.DevProject
{
    public class MetaGameTestStage : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        async void Start()
        {
            var cheats = FindAnyObjectByType<CheatPanel>();

            await Awaitable.WaitForSecondsAsync( 1, destroyCancellationToken );

            cheats.AddCheats( new MetaGameCheats() );
        }

       
    }
}
