using UnityEngine;

// このファイルは MixPop のポンプ入力（右ボタン＋ドラッグ）を担当する。
// 仕様: Assets/Script/Core/MixCore.md
public sealed class MixPopInput : MonoBehaviour
{
    public bool IsPumping { get; private set; }
    public float DragPixels { get; private set; }
    public float ClickY { get; private set; }
    public float CurrentY { get; private set; }
    public float DragPixelsPerHalfRadius => dragPixelsPerHalfRadius;
    public float DeltaY { get; private set; }

    private float _clickY;
    private float _lastY;

    [SerializeField] private float dragPixelsPerHalfRadius = 100f; // 100pxで面積が50%になる基準距離。

    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            IsPumping = true;
            _clickY = Input.mousePosition.y;
            _lastY = _clickY;
            ClickY = _clickY;
            CurrentY = _clickY;
            DragPixels = 0f;
            DeltaY = 0f;
        }
        else if (Input.GetMouseButtonUp(1))
        {
            IsPumping = false;
            DragPixels = 0f;
            DeltaY = 0f;
        }

        if (IsPumping)
        {
            var currentY = Input.mousePosition.y;
            CurrentY = currentY;
            DeltaY = currentY - _lastY;
            _lastY = currentY;
        }
    }

    // DeltaY を取得する（上方向は正、下方向は負）。
    public float GetDeltaY() => DeltaY;
}
