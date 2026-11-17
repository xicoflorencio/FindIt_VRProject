using Google.XR.Cardboard;
using UnityEngine;

/// <summary>
/// Keeps Cardboard's display calibration in sync with the Android screen.
/// This project is designed for a phone placed horizontally in a Cardboard viewer.
/// </summary>
[DefaultExecutionOrder(-10000)]
public sealed class CardboardRuntime : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Create()
    {
        if (!Application.isMobilePlatform)
            return;

        var runtimeObject = new GameObject(nameof(CardboardRuntime));
        DontDestroyOnLoad(runtimeObject);
        runtimeObject.AddComponent<CardboardRuntime>();
    }

    private void Awake()
    {
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    private void Update()
    {
        Api.UpdateScreenParams();

        if (Api.IsGearButtonPressed)
            Api.ScanDeviceParams();

        if (Api.IsTriggerHeldPressed)
            Api.Recenter();

        if (Api.IsCloseButtonPressed)
            Application.Quit();
    }
}
