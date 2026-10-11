using System;
using UnityEngine;

namespace TerraTactix.Core
{
    public static class HapticFeedback
    {
        static float next;
        static bool logged;

        public static void Pulse(bool strong)
        {
            if (Time.unscaledTime < next)
                return;
            next = Time.unscaledTime + (strong ? 0.12f : 0.07f);

#if UNITY_ANDROID && !UNITY_EDITOR
            AndroidPulse(strong ? 28L : 14L, strong ? 160 : 60);
#elif UNITY_IOS && !UNITY_EDITOR
            Handheld.Vibrate();
#endif
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        static void AndroidPulse(long milliseconds, int amplitude)
        {
            try
            {
                using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                using (var vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator"))
                {
                    if (vibrator == null)
                        return;

                    using (var version = new AndroidJavaClass("android.os.Build$VERSION"))
                    {
                        var sdk = version.GetStatic<int>("SDK_INT");
                        if (sdk >= 26)
                        {
                            using (var effectClass = new AndroidJavaClass("android.os.VibrationEffect"))
                            using (var effect = effectClass.CallStatic<AndroidJavaObject>("createOneShot", milliseconds, amplitude))
                                vibrator.Call("vibrate", effect);
                            return;
                        }
                    }

                    Handheld.Vibrate();
                }
            }
            catch (Exception exception)
            {
                if (logged)
                    return;
                logged = true;
                Debug.LogWarning("HapticFeedback: titreşim kullanılamadı. " + exception.Message);
            }
        }
#endif
    }
}
