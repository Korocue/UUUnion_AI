THunt-Dev/ChatRoom/Log-36.md

# Log-36.md
- Branch: b62
- Topic: IMGUI debug overlay / runtime debug visualization
- Outcome: GameDebugOverlay を MonoBehaviour として登録、F1切替の IMGUI デバッグ表示が正常動作
- Keywords: unity, imgui, debug, overlay, ongui, runtime

# Q44
無事に登録完了しました。
希望のImGUIでのデバッグ機能が実現出来ました。

これにてチャットクローズします。
/THunt-Framework/Core/AGENTS.mdの「チャットクローズ」の項目に従い
ヘッダー案をお願いします。

## A44
```
# Log-62.md
- Branch: T-Hunt-Game/b62
- Topic: IMGUI debug overlay / runtime debug visualization
- Outcome: GameDebugOverlay を MonoBehaviour として登録、F1切替の IMGUI デバッグ表示が正常動作
- Keywords: unity, imgui, debug, overlay, ongui, runtime
```

# M43
feat : debug / add GameDebug overlay

# M42
割愛。monobehaviorが付くクラス名とファイル名は一致させなくてはいけない。
オブジェクトへのファイルのアタッチに成功した。
以上をルールとして_AGENTS.mdに追加。

# Q41
元々はDebugじゃないらしい
どういう想定でCodexが作ったかちんぷんかんぷん
この指示に従いたいだけなんだけど

## A41
状況はシンプルで、Codexの「作り方」がUnityの“アタッチ前提ルール”と噛み合ってないだけ。

あなたが従いたい指示はこれでしょ：

GameDebugOverlay を任意の GameObject にアタッチ

OnGUIで表示、F1でトグル

GameDebug は値レジストリ（ログ溜める側）

InspPopPresenter から GameDebugSet(...) で登録

この設計自体は普通にアリ。壊れてるのは Unityがアタッチできる形になってない点。

何が起きてるか（ちんぷんかんぷんの正体）

UnityがInspectorでスクリプトを付けるときは、基本こういう制約がある：

アタッチできるのは public class X : MonoBehaviour の“具象クラス”だけ

abstract / static / generic はアタッチ不可

さらに実務上は
**「1ファイル1MonoBehaviour」かつ「ファイル名＝クラス名」**にしておくのが安全

Codexはたぶん Debug.cs の中に

GameDebug（static/非MonoBehaviour）

GameDebugOverlay（MonoBehaviour）
みたいに 複数クラスを同居させて、しかも場合によっては

abstract class Debug : MonoBehaviour
みたいな“ベースクラス”を置いちゃって、Unityが「Debugを付けようとして失敗」してる。

さらに最悪なのは Debug という名前。UnityEngine.Debug と衝突して未来が燃える。

いまやるべき修正（最短で「指示どおりに動く」形）

やることは ファイル分割＋名前整備。これでアタッチ問題は消える。

1) Debug.cs を分割する

(A) GameDebug.cs（アタッチしない、レジストリ側）

public static class GameDebug（または public sealed class GameDebug でstaticメソッドでもOK）

MonoBehaviourにしない

(B) GameDebugOverlay.cs（アタッチする、表示側）

public class GameDebugOverlay : MonoBehaviour

F1トグル、OnGUI表示

アタッチするのは GameDebugOverlay.cs だけ。

2) “Debug” というクラス名があるなら消す（超重要）

もし class Debug : MonoBehaviour がいるなら、それが元凶。

abstract を外す、でもいいけど

名前が地雷なので、根本的に Debug という名前をやめる方が安全

おすすめ命名：

GameDebugOverlay

DebugOverlayIMGUI

DevDebugOverlay

3) ファイル名とMonoBehaviour名を一致させる

例えば：

GameDebugOverlay.cs の中はこれだけにする：

using UnityEngine;

public class GameDebugOverlay : MonoBehaviour
{
    void OnGUI()
    {
        // 表示
    }
}


これで Inspector から確実に付けられる。

あなたがやりたい「指示に従う」ための最短手順

Assets/Script/Core/Debug/Debug.cs を開く

中に MonoBehaviour が何個あるか確認

Debug というMonoBehaviour（特にabstract）がいたら撤去 or リネーム

GameDebugOverlay を 単独ファイル GameDebugOverlay.cs にする

Unityに戻って、任意のGameObjectに GameDebugOverlay をアタッチ

ついでに：スクショで見えてる「Debugが2つ」問題

あなたのプロジェクトツリー、Core/Debug 配下に Debugが二重に見えてる。
同名クラスや同名ファイルがあると、Unityのスクリプト解決がぐちゃる典型パターン。

対策：

同名クラスを1つにする

ファイル名・クラス名を全部ユニークにする

Debug を名乗らない

これで「Codexの指示に従って、GameDebugOverlayをアタッチして動かす」が成立する。
“抽象クラスをアタッチしようとして死ぬ”のは、Unityあるあるの初見殺し。人間の人生みたいに。

# Q40
Codexが作ってくれたんだけどアタッチ出来ない。

# CA39
【北船#27】Orchestrator.md の指示（b62・デバッグ機能・最小変更）に従い、IMGUIデバッグオーバーレイを追加しました。

Debug.cs (line 1) を新規追加（GameDebug 値レジストリ + GameDebugOverlay OnGUI表示、F1で表示切替）
InspPopPresenter.cs (line 1) で Insp/Mix を GameDebug.Set(...) に登録するよう連携（表示は GameDebugOverlay 側に委譲）
使い方: シーン内の任意の GameObject に GameDebugOverlay をアタッチしてください（再生中に F1 で表示切替）。

コミットメッセージ案: feat : debug / add IMGUI debug overlay and registry

# Q38
デバッグを実装する場合、構成はどうすればいいだろう？

## A38
結論：**「Debugは“機能”じゃなくて“横断インフラ”」**だから、Core直下に独立モジュールとして切るのが一番揉めない。今の構成（Core / UI\InspPop / Old）を見る限り、こう置くのが自然。

推奨フォルダ構成（T-Hunt向け・育つやつ）
Assets/
  Script/
    Core/
      Debug/
        DebugConfig.asset         // ON/OFF, 表示行数, フィルタ等
        DebugBoot.cs              // 自動起動（Development/Editorのみ）
        DebugBus.cs               // メッセージ投げ口（チャンネル/レベル）
        DebugRingBuffer.cs        // 最新N行保持
        DebugOverlayIMGUI.cs      // 画面オーバーレイ（OnGUI + DPIスケール）
        DebugHudStats.cs          // FPS/メモリ/座標など
        DebugCommands.cs          // 任意：コンソールコマンド

置き場所の理由

UI/ に入れると「ゲームUIと依存が絡む」→破滅しがち

Old/ は参照用の墓場でOK（実装は新設）

Core/Debug/ は “どのアプリでも使う共通インフラ” として一番分かりやすい

依存ルール（これ守るだけで地獄回避）

Debugは依存していい：UnityEngine / UnityEngine.UI（必要なら）/ TMP（必要なら）
Debugが依存しちゃダメ：ゲーム固有のPresenter/Model/Scene、THunt固有のロジック

要するに、

ゲーム → Debugにメッセージを投げる（OK）

Debug → ゲームの中身を覗きに行く（なるべく禁止）

起動方式（手で置かない。勝手に生える）
DebugBoot（自動起動）

RuntimeInitializeOnLoadMethod で生成

UNITY_EDITOR or DEVELOPMENT_BUILD のときだけ有効

DebugConfig.asset があれば設定適用、なければデフォルト

これで「シーンにオブジェクト置き忘れた」が消える。

API設計（ゲーム側が使う入口を1本にする）

ゲーム側はこれだけ叩けばいい形にする：

Dbg.Log("Net", "Lobby created");
Dbg.Warn("EOS", "Audio input device missing");
Dbg.Err("Save", "Failed to write file");


内部では

DebugBus が受けて

RingBuffer に積んで

Overlay が描画

UI方式（IMGUI/TMP）を差し替えてもゲーム側コードは変わらない。

表示レイヤの分離（ここが超重要）

収集：Application.logMessageReceived ＋ Dbg.*

保持：リングバッファ（最新N行）

描画：IMGUIオーバーレイ（まずこれ）

拡張：必要になったら Canvas/TMP に差し替え

“今はIMGUI採用したい”って話と相性がいい。

既存の Insp とどう住み分ける？

スクショだと InspCore と UI\InspPop があるから、たぶん「インスペクタ風UI」系だよね。

Insp*：ユーザー操作/調整/入力ポップアップ（プロダクト機能寄り）

Core/Debug：開発者の観測装置（デバッグ専用）

混ぜると「デバッグが本番機能に昇格して消せない」事故になる。

最低限の実装順（迷わないための順番）

DebugBoot（自動生成）

DebugBus（チャンネル＋レベル）

RingBuffer（最新N行）

OverlayIMGUI（DPIスケール＋F1で表示切替）

StatsHud（FPS/メモリ/座標）

ここまでで「スクショのそれっぽいやつ」になる。

この構成にすると、UUUMaster_Old.cs.disabled の“画面テキスト表示”も、UI層を差し替えるだけで復活できる。つまり過去資産も無駄にならない。人間の過去はだいたい無駄だけど、コードは回収できる。

# Q37
https://zenn.dev/nekoxsgames/articles/cc9859fd7da253
こういうのかな？
それなら一番採用したい。

## A37
そう、それ。まさに **Unityの“よくあるデバッグオーバーレイ”**の入り口。
記事は OnGUI() の IMGUI を GUI.matrix でスケールして、高解像度でも文字が読めるようにするって内容だね。 
Zenn

で、「それなら一番採用したい」なら、方針はこうするのが一番ラクで強い。

採用案：IMGUIデバッグHUD（GUI.matrixスケール付き）

ポイントは3つだけ。

GUI.matrix を DPI からスケール計算して適用（記事の核） 
Zenn

ログをリングバッファで溜めて表示（流量制御しないと死ぬ）

F1とかで表示切替（プレイ中ずっと邪魔）

そのまま貼って使える最小実装（1ファイル）：

using System.Collections.Generic;
using System.Text;
using UnityEngine;

public sealed class DebugHudIMGUI : MonoBehaviour
{
    [Header("Toggle")]
    [SerializeField] KeyCode toggleKey = KeyCode.F1;
    [SerializeField] bool visible = true;

    [Header("Scale (DPI)")]
    [SerializeField] bool useDpiScale = true;
    [SerializeField] float dpiBase = 96f;      // 記事の基準
    [SerializeField] float dpiMultiplier = 0.8f; // 記事の 0.8 相当
    [SerializeField] float fallbackScale = 1.25f; // DPI取れない環境用

    [Header("Layout")]
    [SerializeField] int maxLines = 40;
    [SerializeField] int fontSize = 14;
    [SerializeField] float widthRatio = 0.62f;
    [SerializeField] float heightRatio = 0.38f;

    readonly Queue<string> _lines = new();
    Vector2 _scroll;

    void Awake()
    {
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
        // 本番ビルドで出したくなければここで無効化
        enabled = false;
        return;
#endif
        Application.logMessageReceived += OnLog;
    }

    void OnDestroy()
    {
        Application.logMessageReceived -= OnLog;
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey)) visible = !visible;
    }

    void OnLog(string condition, string stackTrace, LogType type)
    {
        // スタックトレースまで出すと画面が即死するので基本は抑制
        Enqueue($"{System.DateTime.Now:HH:mm:ss.fff} [{type}] {condition}");
    }

    public void Enqueue(string msg)
    {
        _lines.Enqueue(msg);
        while (_lines.Count > maxLines) _lines.Dequeue();
    }

    float CalcScale()
    {
        if (!useDpiScale) return 1f;

        float dpi = Screen.dpi;
        // dpi が 0 や変な値の端末があるので保険
        if (dpi <= 0f || dpi > 1000f) return fallbackScale;

        // 記事の式: (Screen.dpi / 96f) * 0.8f :contentReference[oaicite:2]{index=2}
        return (dpi / dpiBase) * dpiMultiplier;
    }

    void OnGUI()
    {
        if (!visible) return;

        float scale = CalcScale();
        var prev = GUI.matrix;
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scale, scale, 1f));

        float w = (Screen.width * widthRatio) / scale;
        float h = (Screen.height * heightRatio) / scale;

        GUI.Box(new Rect(10, 10, w, h), $"Debug HUD  (scale={scale:0.00}, dpi={Screen.dpi:0})");

        var style = new GUIStyle(GUI.skin.label) { fontSize = fontSize, richText = false };
        var view = new Rect(20, 40, w - 30, h - 50);

        // 行数ぶんの高さをざっくり確保
        float contentH = Mathf.Max(0, _lines.Count * (fontSize + 4));
        var content = new Rect(0, 0, view.width - 20, contentH);

        _scroll = GUI.BeginScrollView(view, _scroll, content);

        var sb = new StringBuilder();
        foreach (var l in _lines) sb.AppendLine(l);
        GUI.Label(new Rect(0, 0, content.width, content.height), sb.ToString(), style);

        GUI.EndScrollView();

        GUI.matrix = prev;
    }
}

使い方（雑に強い）

空のGameObject作って DebugHudIMGUI を付ける

Playすると画面左上に出る

F1で表示ON/OFF

Debug.Log/Warning/Error が勝手に流れる

追加で自前メッセージ出したければ Enqueue("...") を呼ぶ

これを“スクショのやつ”っぽくしたいなら

次に足すと効くのはこの2つだけ：

Stats行（FPS / メモリ / プレイヤー座標）を上部に固定表示

カテゴリ（Net/AI/UI…）のフィルタ（キーで切替）

ログは無限に出せるけど、人間の脳は無限に読めないからな。

# Q36
デバッグメッセージについて
現在はログに表示する形式だけど、これを画面にリアルタイムで表示する方式は取れないかな？

UUUMaster_Old.cs.disabledでは画面上のテキストオブジェクトに
メッセージを表示する手法を取ってる。

よくUnity製のゲームでなんか汎用っぽいインターフェースが出たりするけど
デバッグ面で何か参考になる情報はないかな？

他に何か面白い方法があれば教えて欲しい。

## A36
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


# End Of File