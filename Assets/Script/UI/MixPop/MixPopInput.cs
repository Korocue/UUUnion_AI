using UnityEngine;

// このファイルは MixPop のポンプ入力（右ボタン＋ドラッグ）を担当する。
// 仕様: Assets/Script/Core/MixCore.md
public sealed class MixPopInput : MonoBehaviour
{
    public bool IsPumping { get; private set; }
    public float DragPixels { get; private set; }
    public float ClickY { get; private set; }
    public float AnchorY { get; private set; }
    public float CurrentY { get; private set; }
    public float DragPixelsPerHalfRadius => dragPixelsPerHalfRadius;

    private float _clickY;
    private float _startY;
    private float _maxDownFromClick;
    private float _maxUpFromClick;
    private float _pumpLevel;

    [SerializeField] private float anchorFollowRate = 0.5f; // 下方向の移動量に対する基準点の追従率。
    [SerializeField] private float dragPixelsPerHalfRadius = 100f; // 100pxで半径が50%になる基準距離。

    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            IsPumping = true;
            _clickY = Input.mousePosition.y;
            _startY = _clickY;
            _maxDownFromClick = 0f;
            _maxUpFromClick = 0f;
            ClickY = _clickY;
            AnchorY = _startY;
            CurrentY = _clickY;
        }
        else if (Input.GetMouseButtonUp(1))
        {
            IsPumping = false;
            DragPixels = 0f;
        }

        if (IsPumping)
        {
            var currentY = Input.mousePosition.y;
            CurrentY = currentY;
            var downFromClick = _clickY - currentY;
            if (downFromClick > _maxDownFromClick)
            {
                _maxDownFromClick = downFromClick;
            }

            var upFromClick = currentY - _clickY;
            if (upFromClick > _maxUpFromClick && _pumpLevel >= 0.9f)
            {
                _maxUpFromClick = upFromClick;
            }

            var followRate = Mathf.Clamp01(anchorFollowRate);
            var downFollow = _maxDownFromClick * followRate;
            var upFollow = _maxUpFromClick * followRate;
            _startY = _clickY + (upFollow - downFollow);

            DragPixels = Mathf.Max(0f, currentY - _startY);
            AnchorY = _startY;
        }
    }

    // 上方向補正の判定は PumpLevel によって行う。
    public void SetPumpLevel(float pumpLevel)
    {
        _pumpLevel = Mathf.Clamp01(pumpLevel);
    }
}
