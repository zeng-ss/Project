using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using DG.Tweening;

[CreateAssetMenu(fileName = "EaseSetting", menuName = "DoTween/Create Ease Setting")]
public class EaseSetting : ScriptableObject
{
    public List<Ease> easies = new List<Ease>();
}

public static class EaseSettingExtension
{
    private static string _easeSettingPath = "Assets/Editor/Ease/EaseSetting.asset";

    public static void ApplyEase(this Tween tween, int index)
    {
#if UNITY_EDITOR
        EaseSetting _easeSetting = AssetDatabase.LoadAssetAtPath<EaseSetting>(_easeSettingPath);
        tween.SetEase(_easeSetting.easies[index]);
#endif
    }

    public static void ApplyEase(this Sequence sequence, int index)
    {
#if UNITY_EDITOR
        EaseSetting _easeSetting = AssetDatabase.LoadAssetAtPath<EaseSetting>(_easeSettingPath);
        sequence.SetEase(_easeSetting.easies[index]);
#endif
    }
}