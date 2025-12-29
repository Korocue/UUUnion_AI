using System.Collections.Generic;
using UnityEngine;

// ゲーム内の主要な値を IMGUI で表示するためのレジストリとオーバーレイ。
// 仕様: Assets/Script/Core/Debug/Debug.md
public static class GameDebug
{
    private static readonly Dictionary<string, string> Values = new();
    private static readonly List<string> Order = new();

    public static void Set(string key, string value)
    {
        if (!Values.ContainsKey(key))
        {
            Order.Add(key);
        }

        Values[key] = value;
    }

    public static IReadOnlyList<string> Keys => Order;

    public static bool TryGet(string key, out string value) => Values.TryGetValue(key, out value);
}

// OnGUI（IMGUI）のオーバーレイ。主要な値は GameDebug.Set(...) で登録する。
public sealed class GameDebugOverlay : MonoBehaviour
{
    [Header("Behavior")]
    [SerializeField] private bool visible = true;
    [SerializeField] private KeyCode toggleKey = KeyCode.F1;

    [Header("Layout")]
    [SerializeField] private float x = 10f;
    [SerializeField] private float y = 10f;
    [SerializeField] private float width = 260f;
    [SerializeField] private float height = 180f;

    private GUIStyle _labelStyle;

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            visible = !visible;
        }
    }

    private void OnGUI()
    {
        if (!visible)
        {
            return;
        }

        if (_labelStyle == null)
        {
            _labelStyle = new GUIStyle(GUI.skin.label)
            {
                padding = new RectOffset(0, 0, 0, 0),
                margin = new RectOffset(0, 0, 0, 0),
                fontSize = 12,
                fixedHeight = 14
            };
        }

        var fullHeight = Mathf.Max(0f, Screen.height - y * 2f);
        var areaHeight = Mathf.Max(height, fullHeight);
        GUILayout.BeginArea(new Rect(x, y, width, areaHeight), GUI.skin.box);
        GUILayout.Label("Debug", _labelStyle);

        foreach (var key in GameDebug.Keys)
        {
            if (GameDebug.TryGet(key, out var value))
            {
                GUILayout.Label($"{key}: {value}", _labelStyle);
            }
        }

        GUILayout.EndArea();
    }
}
