THunt-Dev/ChatRoom/Pending.md

# A36(デバッグ方法)
できる。というか ログだけ って、困ったときに一番見づらい地獄ルートに自分から突っ込んでるだけ。

Unityで「画面にリアルタイム表示」する定番はだいたいこのへん。用途ごとに強みが違う。

1) いちばん手軽: OnGUI（IMGUI）でオーバーレイ表示

メリット: 実装が速い / PrefabもCanvasも不要 / その場で出せる
デメリット: いまどき感はない / レイアウト凝ると面倒

“いま見たい”だけならこれが最短。

例（リングバッファで最新N行を表示）:

using System.Collections.Generic;
using System.Text;
using UnityEngine;

public sealed class DebugOverlayIMGUI : MonoBehaviour
{
    [SerializeField] int maxLines = 30;
    [SerializeField] int fontSize = 14;
    [SerializeField] KeyCode toggleKey = KeyCode.F1;

    readonly Queue<string> _lines = new();
    bool _visible = true;
    Vector2 _scroll;

    void Awake()
    {
        Application.logMessageReceived += OnLog;
    }

    void OnDestroy()
    {
        Application.logMessageReceived -= OnLog;
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey)) _visible = !_visible;
    }

    void OnLog(string condition, string stackTrace, LogType type)
    {
        // スパム防止に必要なら type フィルタや stackTrace 抑制を追加
        Enqueue($"{System.DateTime.Now:HH:mm:ss.fff} [{type}] {condition}");
    }

    public void Enqueue(string msg)
    {
        _lines.Enqueue(msg);
        while (_lines.Count > maxLines) _lines.Dequeue();
    }

    void OnGUI()
    {
        if (!_visible) return;

        var style = new GUIStyle(GUI.skin.label) { fontSize = fontSize, richText = true };
        GUI.Box(new Rect(10, 10, Screen.width * 0.6f, Screen.height * 0.35f), "Debug Overlay");

        var view = new Rect(20, 40, Screen.width * 0.6f - 20, Screen.height * 0.35f - 50);
        var content = new Rect(0, 0, view.width - 20, _lines.Count * (fontSize + 4));

        _scroll = GUI.BeginScrollView(view, _scroll, content);
        var sb = new StringBuilder();
        foreach (var l in _lines) sb.AppendLine(l);
        GUI.Label(new Rect(0, 0, content.width, content.height), sb.ToString(), style);
        GUI.EndScrollView();
    }
}

2) “Unityっぽい”UIでやる: Canvas + TextMeshPro（あなたの既存方式の強化版）

メリット: 見た目調整しやすい / スクロール・色分け・フィルタが簡単
デメリット: UIの用意が必要 / 実装がちょい増える

強化ポイントはこれだけ押さえればいい：

リングバッファ（最新N行）

LogType別に色分け（Error赤、Warning黄、Log白）

トグル表示（F1など）

Development Build限定（本番で出すと恥ずかしい）

Unityの「よくある汎用デバッグUI」は、だいたいこの方針で作られてる。

3) さらに実用: “ゲーム内コンソール”方式（コマンド入力 + ログビュー）

メリット: 実運用に強い（QA・デバッグが楽） / 変数いじれる / チートコマンドも統合可
デメリット: ちゃんと作ると時間が溶ける（いつものUnity）

最低限の機能セット:

ログ表示（フィルタ: Log/Warning/Error）

検索

クリア

コマンド入力（例: god on, spawn cube 10, set timescale 0.2）

オートコンプリート（余裕があれば）

この形式、Unity製ゲームでも割と見かける。「~（チルダ）でコンソールが開く」やつ。

4) “いかにも開発者向け”: Stats HUD（FPS/メモリ/ネット/プレイヤー座標）

スクショに出てるやつ（FPSやメモリ、Pingっぽい表示）まさにこれ。

表示したい定番:

FPS（1/Time.unscaledDeltaTime）

フレーム時間

GC/ヒープ使用量（Profiler系、ただしビルドで制限あり）

プレイヤー座標

現在シーン、プレイ状態、サーバー/ロビー情報（今回のEOS/Steamworksっぽい文脈だと超有効）

ログより“状態”が見えるから、原因切り分けが速い。

5) UI Toolkit（UITK）で作る（将来寄り）

メリット: 大規模UIに強い / 画面解像度対応しやすい
デメリット: いまこの瞬間に欲しいなら過剰装備

T-Huntみたいに「ツールとして長く育てる」ならアリ。

6) 既製品を使って人生を節約する（おすすめ）

“面白い方法”としては、ここが一番コスパいい。

SRDebugger（有料寄りだけど強い。変数インスペクタ、コンソール、オーバーレイなど）

In-game Debug Console系（無料/有料いろいろ。ログビュー＋コマンド入力）

Runtime Inspector系（実行中にGameObjectや値を触れる）

自作は学びにはなるけど、デバッグUIは育つほど沼。

実装のコツ（地味に重要）

Development Build / Editor のときだけ有効化

#if UNITY_EDITOR || DEVELOPMENT_BUILD

ログ全量を垂れ流さない

重要カテゴリだけ（Network/AI/UI/Inputなど）に絞れるようにする

“チャネル”概念

DebugBus.Push("Net", "...") みたいに分類して、表示側でフィルタ

スパム防止

同一メッセージ連打をまとめる（x42 表示）

どれを採用すべきか（結論）

まず成果が欲しい: OnGUIオーバーレイ

ちゃんと運用したい: Canvas+TMPでログビュー + Stats HUD

開発チームごっこを始めたい: ゲーム内コンソール + コマンド

時間を守りたい: SRDebugger系の導入

ログに埋もれて「何が起きてるんだ…」ってやるより、画面に出して殴ったほうが早い。人類は進歩しないけど、デバッグは進歩できる。

# Codexに配置だけ見せたいなら…
すぐ使えるコマンド（君の手元で）

Codexに“配置”を見せたいなら、まずこれを貼ればいい（ログじゃなく構造だけ吐く）:

Windows (PowerShell)

cd <UnityProjectRoot>
tree /F /A Assets

出力結果をそのままCodexに渡すと、「どのフォルダに何があるか」を一発で把握する。

以上。見えるけど、見える範囲を勘違いすると刺さる。