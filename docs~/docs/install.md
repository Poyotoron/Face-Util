# インストール

## 動作環境

| 項目 | 要件 |
|---|---|
| Unity | 2022.3 |
| VRChat SDK - Avatars | 3.7.0 以降 |
| Modular Avatar | 1.17.0 以上、2.0.0 未満 |
| NDMF | 1.14.0 以上、2.0.0 未満 |
| 送り手 | Windows 10 / 11、Microsoft Edge WebView2 ランタイム（Windows 11 には入っています） |

次のツールが入っていれば連携します。無くても使えます（[ほかのツールとの併用](other-tools.md)）。

| ツール | 連携 |
|---|---|
| AAO Avatar Optimizer | Freeze BlendShapes で固定するブレンドシェイプを、操作の対象から外します |
| Avatar Blink Fix | Avatar Blink Fix が顔のメッシュを直したあとに組み込みます |

## VCC / ALCOM から入れる

1. VCC または ALCOM に `https://vpm.maaaaa.net/index.json` を追加します。
2. プロジェクトの Manage Project を開き、「Poyo's Face Util」を追加します。

VRChat SDK・Modular Avatar・NDMF も自動で追加されます。送り手のアプリ（`FaceUtilSender.exe`）もパッケージに入っています。

## .unitypackage から入れる

[リリースページ](https://github.com/Poyotoron/Face-Util/releases)から `.unitypackage` をダウンロードし、Unity にインポートします。

- VRChat SDK・Modular Avatar・NDMF は自動では入りません。先に入れてください。
- `.unitypackage` には送り手が入っていません。同じリリースにある `FaceUtilSender.exe` をダウンロードして使ってください。

## 送り手の起動

送り手はインストールせずに使えるアプリです。

| 起動のしかた | 開くマップ |
|---|---|
| 受け手の Inspector の `送り手を起動` | そのアバターのマップ |
| メニューの `Tools > Poyo's Face Util > Open Sender` | 前に開いたマップ |
| `FaceUtilSender.exe` を直接開く | 前に開いたマップ |

VCC から入れたときは、exe はパッケージの中の `Bin~/FaceUtilSender.exe` にあります。Unity を開かずに使うときは、この exe のショートカットを作っておくと便利です。
