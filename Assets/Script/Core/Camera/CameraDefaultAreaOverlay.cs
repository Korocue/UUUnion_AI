using UnityEngine;

// このファイルはデフォルト表示エリアを背景の四角形で表示する。
// 仕様: Assets/Script/Core/Camera/Camera.md
public sealed class CameraDefaultAreaOverlay : MonoBehaviour
{
    [Header("Bindings")]
    [SerializeField] private Camera targetCamera;

    [Header("Layout")]
    [SerializeField] private float heightScale = 2f;   // height = OrthographicSize * heightScale
    [SerializeField] private float depthOffset = 0f;
    [SerializeField] private float lineWidth = 0.02f;

    [Header("Colors")]
    [SerializeField] private Color fillColor = new Color(1.0f, 1.0f, 1.0f, 0.1f);
    [SerializeField] private Color lineColor = Color.white;

    private Transform _root;
    private Transform _fill;
    private LineRenderer _line;
    private bool _logged;

    private void OnEnable()
    {
        if (targetCamera == null)
        {
            targetCamera = GetComponent<Camera>();
        }

        EnsureObjects();
        UpdateFrame();
    }

    private void LateUpdate()
    {
        // 初期状態で固定する。
        if (!_logged)
        {
            LogMaterialState();
            _logged = true;
        }
    }

    private void EnsureObjects()
    {
        if (_root != null)
        {
            return;
        }

        _root = new GameObject("CameraDefaultArea").transform;

        var fill = GameObject.CreatePrimitive(PrimitiveType.Cube);
        fill.name = "AreaFill";
        fill.transform.SetParent(_root, false);
        fill.transform.localPosition = Vector3.zero;
        _fill = fill.transform;

        var renderer = fill.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            var shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
            {
                shader = Shader.Find("Universal Render Pipeline/Lit");
            }

            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            var material = new Material(shader);
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", fillColor);
            }
            else
            {
                material.color = fillColor;
            }

            if (shader != null && shader.name.StartsWith("Universal Render Pipeline"))
            {
                // URP の Unlit/Lit を透明設定にする。
                if (material.HasProperty("_Surface"))
                {
                    material.SetFloat("_Surface", 1f);
                }

                if (material.HasProperty("_Blend"))
                {
                    material.SetFloat("_Blend", 0f);
                }

                if (material.HasProperty("_SrcBlend"))
                {
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                }

                if (material.HasProperty("_DstBlend"))
                {
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                }

                if (material.HasProperty("_AlphaClip"))
                {
                    material.SetFloat("_AlphaClip", 0f);
                }

                if (material.HasProperty("_Cull"))
                {
                    material.SetFloat("_Cull", 2f);
                }

                if (material.HasProperty("_ZWrite"))
                {
                    material.SetFloat("_ZWrite", 0f);
                }

                material.SetOverrideTag("RenderType", "Transparent");
                material.DisableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.EnableKeyword("_ALPHABLEND_ON");
                material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            }
            else if (shader != null && shader.name == "Standard")
            {
                // Standardシェーダーを透明描画に切り替える。
                material.SetFloat("_Mode", 3f);
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.EnableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            }

            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        var lineObject = new GameObject("AreaLine");
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

    private void LogMaterialState()
    {
        var renderer = _fill != null ? _fill.GetComponent<MeshRenderer>() : null;
        if (renderer == null)
        {
            Debug.LogWarning("CameraDefaultAreaOverlay: MeshRenderer not found.");
            return;
        }

        var material = renderer.sharedMaterial;
        if (material == null)
        {
            Debug.LogWarning("CameraDefaultAreaOverlay: Material not found.");
            return;
        }

        var shaderName = material.shader != null ? material.shader.name : "null";
        var color = material.HasProperty("_BaseColor") ? material.GetColor("_BaseColor") : material.color;
        var surface = material.HasProperty("_Surface") ? material.GetFloat("_Surface") : -1f;
        var blend = material.HasProperty("_Blend") ? material.GetFloat("_Blend") : -1f;
        var zwrite = material.HasProperty("_ZWrite") ? material.GetFloat("_ZWrite") : -1f;
        Debug.Log($"CameraDefaultAreaOverlay: shader={shaderName}, color={color}, surface={surface}, blend={blend}, zwrite={zwrite}, queue={material.renderQueue}");
    }

    private void UpdateFrame()
    {
        var height = targetCamera.orthographicSize * heightScale;
        var width = height * targetCamera.aspect;
        _root.position = new Vector3(0f, 0f, depthOffset);
        _root.rotation = Quaternion.identity;
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
