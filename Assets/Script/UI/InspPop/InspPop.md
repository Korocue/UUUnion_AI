/Assets/Script/UI/InspPop/InspPop.md

# ミクスチャ状態表示（InspPop）
ミクスチャの内部状態を表示する
円を描画するコードを書いて下さい。

## InspPop
目的: プレイヤー操作（入力）＋メイン値表示（Insp/Mix）
構成: InspPopPresenter が InspCore を保持して Tick(dt) と AddMix() を呼ぶ（Core更新の窓口）。Viewは見た目更新だけ。
制約: UIはゲーム数値ロジックを持たない（Coreは純C#、Unity依存禁止）。

## GamanPop
目的: 表示専用（入力なし）。「がまん値」を InspPopの内側の円として重ねて見せる。
表現: 表示される部分の面積がGaman値に一致するようにする（InspPopを隠す分は差し引き）。
データ: GamanCore 等で算出された gamanValue を受けて描画するだけ（がまん計算・EMA・しきい値計算はUIの外）。
- GamanPopはInspPopの上に重なるように、スプライトのオーダーを調整してください
- GamanPopはInspPopと重なった分、InspPopを膨らませます。

## 依存ルール
Core（InspCore/GamanCore等）→ Unity依存禁止
UI（InspPop/GamanPop）→ 値を受け取って表示するだけ
入力は InspPop側に集約（GamanPopは触らない）