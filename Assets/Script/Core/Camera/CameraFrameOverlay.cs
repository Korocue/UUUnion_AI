using UnityEngine;

// このファイルはデフォルト表示エリアを背景の四角形で表示する。
// 仕様: Assets/Script/Core/Camera/Camera.md
public sealed class CameraFrameOverlay : MonoBehaviour
{
    [Header("Bindings")]
    [SerializeField] private Camera targetCamera;

    [Header("Layout")]
    [SerializeField] private float heightScale = 5f;   // height = OrthographicSize * heightScale
    [SerializeField] private float depthOffset = 1f;
    [SerializeField] private float lineWidth = 0.02f;

    [Header("Colors")]
    [SerializeField] private Color fillColor = new Color(0.6f, 0.8f, 1f, 0.15f);
    [SerializeField] private Color lineColor = new Color(0.5f, 0.8f, 1f, 1f);

    private Transform _root;
    private Transform _fill;
    private LineRenderer _line;

    private void OnEnable()
    {
        if (targetCamera == null)
        {
            targetCamera = GetComponent<Camera>();
        }

        EnsureObjects();
    }

    private void LateUpdate()
    {
        if (targetCamera == null)
        {
            return;
        }

        UpdateFrame();
    }

    private void EnsureObjects()
    {
        if (_root != null)
        {
            return;
        }

        _root = new GameObject("CameraFrame").transform;
        _root.SetParent(transform, false);

        var fill = GameObject.CreatePrimitive(PrimitiveType.Cube);
        fill.name = "FrameFill";
        fill.transform.SetParent(_root, false);
        fill.transform.localPosition = Vector3.zero;
        _fill = fill.transform;

        var renderer = fill.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            var shader = Shader.Find("Unlit/Color");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            var material = new Material(shader) { color = fillColor };
            renderer.sharedMaterial = material;
        }

        var lineObject = new GameObject("FrameLine");
        lineObject.transform.SetParent(_root, false);
        _line = lineObject.AddComponent<LineRenderer>();
        _line.positionCount = 5;
        _line.useWorldSpace = false;
        _line.startWidth = lineWidth;
        _line.endWidth = lineWidth;
        _line.startColor = lineColor;
        _line.endColor = lineColor;
        _line.material = new Material(Shader.Find("Unlit/Color")) { color = lineColor };
    }

    private void UpdateFrame()
    {
        var height = targetCamera.orthographicSize * heightScale;
        var width = height * targetCamera.aspect;
        var z = targetCamera.nearClipPlane + depthOffset;

        _root.localPosition = new Vector3(0f, 0f, z);
        _fill.localScale = new Vector3(width, height, 0.01f);

        var halfW = width * 0.5f;
        var halfH = height * 0.5f;
        _line.SetPosition(0, new Vector3(-halfW, -halfH, 0f));
        _line.SetPosition(1, new Vector3(halfW, -halfH, 0f));
        _line.SetPosition(2, new Vector3(halfW, halfH, 0f));
        _line.SetPosition(3, new Vector3(-halfW, halfH, 0f));
        _line.SetPosition(4, new Vector3(-halfW, -halfH, 0f));
    }
}
