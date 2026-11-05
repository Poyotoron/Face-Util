# OSC の仕様

送り手を使わずに、自分で作ったツールから Face Util を操作するときの情報です。

## アドレス

送り先は VRChat の OSC の受信ポート（既定は `127.0.0.1:9000`）です。

| アドレス | 型 | 値 |
|---|---|---|
| `/avatar/parameters/FaceUtil/Enabled` | Bool | ON・OFF |
| `/avatar/parameters/FaceUtil/LipSync` | Bool | ON の間も口パクを動かすか |
| `/avatar/parameters/FaceUtil/Index` | Int32 | 0〜255。1 つ目の枠の番号です。0 はどれも動かしません |
| `/avatar/parameters/FaceUtil/Value` | Int32 | 0〜255。ウェイトの 0〜100 にあたります |
| `/avatar/parameters/FaceUtil/Index2`、`Index3` … | Int32 | 2 つ目からの枠の番号です。登録数が 255 を超えるアバターだけにあります |
| `/avatar/parameters/FaceUtil/Value2`、`Value3` … | Int32 | 2 つ目からの枠の値です |

ウェイトは `値 × 100 ÷ 255` です。ウェイト `w` を送るときは、`round(w × 255 ÷ 100)` を送ります。

## 送り方

1 本のブレンドシェイプを動かすときは、`Index` を送ってから `Value` を送ります。`Value` を先に送ると、切り替える前の番号のブレンドシェイプに値が入ります。

- 同じ番号の値を変えるときも、毎回 `Index` と `Value` を組で送ってください。アバターの `Index` は、メニューでオフにしたときやアバターを読み込み直したときに 0 に戻ります。
- 別の番号に切り替えるときは、前に送ってから時間をあけてください（送り手の既定は 250 ミリ秒）。同期は決まった間隔でしか行われないため、すぐに切り替えると、ほかのプレイヤーに届かない値が出ます。
- 同じ番号の値を変えるだけなら、50 ミリ秒以上あければ足ります。
- 255 本ごとに 1 つの枠です。枠ごとの送り方は同じで、別の枠には同時に送れます。

## マップ

マップは `%LOCALAPPDATA%\FaceUtil\Maps\マップ名.json`（UTF-8）です。パラメータの名前は、アドレスを決め打ちせずにマップの `parameters` から読んでください。

| キー | 内容 |
|---|---|
| `format` / `version` | `face-util-map` / `1` |
| `mapName` | マップ名 |
| `avatars[]` | このマップを使うアバター（`name`、`blueprintId`） |
| `parameters.enabled` / `parameters.lipSync` | ON・OFF と口パクのパラメータ名 |
| `parameters.channels[]` | 枠ごとの `index` と `value` のパラメータ名 |
| `slots[].channel` / `slots[].index` | そのブレンドシェイプの枠（1 から）と番号（1〜255） |
| `slots[].mesh` / `slots[].blendShape` | アバターのルートからのメッシュのパスと、ブレンドシェイプの名前 |
| `slots[].defaultWeight` | 既定の値（0〜100） |
| `slots[].group` | 区切りのグループの名前。最初の区切りより前は空です |

## 表情ファイル

`format` が `face-util-expression` の JSON です。`blendShapes[]` に、`mesh`、`blendShape`、`weight`（0〜100）が入ります。
