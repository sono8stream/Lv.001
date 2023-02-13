using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// 【暫定】Assembly Infoを切り分けてifdefを削除
#if UNITY_EDITOR
[CustomEditor(typeof(FloatingJoystick))]
public class FloatingJoystickEditor : JoystickEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (background != null)
        {
            RectTransform backgroundRect = (RectTransform)background.objectReferenceValue;
            backgroundRect.anchorMax = Vector2.zero;
            backgroundRect.anchorMin = Vector2.zero;
            backgroundRect.pivot = center;
        }
    }
}
#endif