# うにうにうにおんストーリー

Unity製プロジェクトです。現在、`b60` までの内容を AI を利用して移植・再構築中です。

## 動作環境
- Unity: `6000.0.21f1`

## 起動手順
- Unity Hub でこのリポジトリ（プロジェクトフォルダ）を追加して開いてください。
- 主要シーン例: `Assets/Union.unity`

## リポジトリ運用（重要）
- このプロジェクトは T-Hunt フレームワークに準拠します。
- Codex/AI を含む作業ガイドは、`THunt-Framework/Core/AGENTS.md` を前提に運用します。
- 今後、リポジトリ直下の運用ガイドは `AGENTS.md` から `_AGENTS.md` へ変更予定です（`/THunt-Framework/` 内の `AGENTS.md` と区別するため）。
  - 変更後は、リポジトリ直下では `AGENTS.md` の代わりに `_AGENTS.md` に従ってください。

## 備考
- `Library/`, `Temp/`, `Obj/` などの生成物はリポジトリ管理対象外です（`.gitignore` 前提）。
