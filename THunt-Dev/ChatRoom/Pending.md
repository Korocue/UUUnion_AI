THunt-Dev/ChatRoom/Pending.md

# Codexに配置だけ見せたいなら…
すぐ使えるコマンド（君の手元で）

Codexに“配置”を見せたいなら、まずこれを貼ればいい（ログじゃなく構造だけ吐く）:

Windows (PowerShell)

cd <UnityProjectRoot>
tree /F /A Assets

出力結果をそのままCodexに渡すと、「どのフォルダに何があるか」を一発で把握する。

以上。見えるけど、見える範囲を勘違いすると刺さる。