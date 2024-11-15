using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Vibrator
{
#if UNITY_ANDROID && !UNITY_EDITOR
    public static AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
    public static AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
    public static AndroidJavaObject vibrator = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");
#else
    public static AndroidJavaClass unityPlayer;
    public static AndroidJavaObject currentActivity;
    public static AndroidJavaObject vibrator;
#endif

    public static void Vibrate()
    {
        if (IsMobile())
            vibrator.Call("vibrate");
    }

    public static void Vibrate(long milliseconds = 250)
    {
#if (UNITY_ANDROID) && !UNITY_EDITOR
        if (IsMobile())
            vibrator.Call("vibrate", milliseconds);
#elif (UNITY_IOS) && !UNITY_EDITOR
            Handheld.Vibrate();
#endif
    }

    public static void Vibrate(long[] pattern, int repeat)
    {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
        if (IsMobile())
            vibrator.Call("vibrate", pattern, repeat);
#else
#endif
    }

    public static bool HasVibrator()
    {
        return IsMobile();
    }

    public static void Cancel()
    {
        if (IsMobile())
            vibrator.Call("cancel");
    }

    private static bool IsMobile()
    {
#if UNITY_ANDROID || UNITY_IOS
        return true;
#else
        return false;
#endif
    }
}