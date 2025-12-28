/Assets/Script/Core/InspCore.md

# インスピレーションの設定
Assets\Script\Old\Uni_Mxt_Old.cs.disabledを参考に
右クリックで1%mixを増加させるプログラムを作ってください。

# 目的
Unityプロジェクトに Insp（メイン値） と Mix（燃料） を実装する。
右クリックでMixが増え、Mixは 約0.2秒 の時間スケールで Insp に吸収（変換）される。
InspPop（丸いUI）は最終的に重要UIだが、現時点では 膨らむ表示 だけ作る。

# 用語
Insp: ゲームのメイン値（蓄積される）
Mix: Inspに変換される燃料
InspPop: Inspを表示・操作する丸いUI（この段階では表示だけ）

# 制約（重要）
CoreはUnity依存禁止（UnityEngine参照禁止、MonoBehaviour禁止）
Mix→Insp変換は1関数1箇所に固定（他でInsp/Mixを書き換えない）
InspPopは表示と入力だけ（ゲーム数値ロジックを持たない）
InspPopの面積はInspに応じて決定される
0.2秒吸収は指数減衰（dt耐性）で実装
  tau = 0.2
  alpha = 1 - exp(-dt / tau)
  moved = Mix * alpha
  Mix -= moved; Insp += moved;

# 実装物（ファイル構成）

## Assets/Script/Core/InspCore.cs（純C#）
public double Insp { get; }
public double Mix { get; }
public void AddMix(double amount)（右クリックで使う）
public double Tick(double dt)（毎フレーム呼ぶ。戻り値は今回変換したmoved量）

## Assets/Script/UI/InspPop/InspPopPresenter.cs（MonoBehaviour）
InspCore を保持して Tick(Time.deltaTime) を呼ぶ
右クリックで AddMix(mixPerClick) を呼ぶ
表示対象 Transform inspPop のスケールを Insp で更新
テスト用ログを1秒に1回出す（Insp/Mix/movedを表示）

## Assets/Script/UI/InspPop/InspPopView.cs
Presenterから渡された値で見た目更新だけする

# 完成条件（Acceptance Criteria）
再生中、右クリックでMixが増える（ログで確認可能）
Mixは放置すると約0.2秒スケールで0に近づき、同量がInspに増える（ログで確認可能）
Inspが増えると InspPop の丸が膨らむ（Transform.localScale）
- localScaleの初期値は0
CoreはUnity依存ゼロでコンパイル可能

# 注意
旧 Uni_Mxt_Old.cs の変数群（dMix, ddMixSukkiri等）は移植しない
必要なら後で「追加状態」として段階的に導入する