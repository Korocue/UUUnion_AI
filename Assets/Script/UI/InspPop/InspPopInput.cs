using System;
using UnityEngine;

// このファイルは InspPop の入力（右クリック検知）だけを担当する。
// 仕様: Assets/Script/Core/InspCore.md の「右クリックで AddMix」をPresenterへ渡すための補助。
public sealed class InspPopInput : MonoBehaviour
{
    public event Action RightClicked;

    private void Update()
    {
        // 右クリック（マウス右ボタン）を通知する。
        if (Input.GetMouseButtonDown(1))
        {
            RightClicked?.Invoke();
        }
    }
}
