using UnityEngine;

// このファイルは MixPop のポンプ入力（右ボタン＋ドラッグ）を担当する。
// 仕様: Assets/Script/Core/MixCore.md
public sealed class MixPopInput : MonoBehaviour
{
    public bool IsPumping { get; private set; }
    public float DragPixels { get; private set; }

    private float _startY;

    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            IsPumping = true;
            _startY = Input.mousePosition.y;
        }
        else if (Input.GetMouseButtonUp(1))
        {
            IsPumping = false;
            DragPixels = 0f;
        }

        if (IsPumping)
        {
            var currentY = Input.mousePosition.y;
            DragPixels = Mathf.Max(0f, _startY - currentY);
        }
    }
}
