# Poyo's Face Util

VRChat にいるアバターの表情（ブレンドシェイプ）を、VRChat の外から OSC で 1 本ずつ動かすツールです。アバターに組み込む受け手（Unity パッケージ）と、スライダーで操作する Windows の送り手アプリでできています。

使い方: <https://ghp.maaaaa.net/Face-Util/>

## 動作環境

| 項目 | 要件 |
|---|---|
| Unity | 2022.3 |
| VRChat SDK - Avatars | 3.7.0 以降 |
| Modular Avatar | 1.17.0 以上、2.0.0 未満 |
| NDMF | 1.14.0 以上、2.0.0 未満 |
| 送り手 | Windows 10 / 11、Microsoft Edge WebView2 ランタイム（Windows 11 には入っています） |

AAO Avatar Optimizer と Avatar Blink Fix が入っていれば連携します。

## インストール

1. VCC / ALCOM に `https://vpm.maaaaa.net/index.json` を追加します。
2. プロジェクトの Manage Project で「Poyo's Face Util」を追加します。

VRChat SDK・Modular Avatar・NDMF も自動で追加されます。送り手のアプリもパッケージに入っています。

VCC を使わない場合は、[リリースページ](https://github.com/Poyotoron/Face-Util/releases)の `.unitypackage` をインポートし、依存パッケージを別に入れてください。`.unitypackage` には送り手が入っていないので、同じリリースにある `FaceUtilSender.exe` を使ってください。

## 使い方

1. Hierarchy でアバターを右クリックし、`Poyo's Face Util` → `Setup` を選びます。
2. できた `Face Util` オブジェクトの Inspector で、動かすブレンドシェイプを選びます。
3. アバターをアップロードします。
4. VRChat の Action Menu で、Options → OSC → `Enabled` をオンにします。
5. Inspector の `送り手を起動` を押します。
6. 送り手のスライダーを動かすと、アバターの表情が変わります。

## 注意

- ON の間は、登録したブレンドシェイプがすべて送り手の値になります。体型の調整など、表情に使わないブレンドシェイプは Inspector でオフにしてください。
- 動かすブレンドシェイプを変えたら、アップロードし直してください。
- 登録数が多いと、同期パラメータを多く使います。255 本までは 18 bit です。
- パラメータを追加したアバターを上書きでアップロードして OSC が効かないときは、Action Menu の Options → OSC → `Reset Config` を実行してください。

## 送り手のビルド

Rust と `tauri-cli` 2 系を用意し、`Sender~/src-tauri` で次を実行します。

```powershell
cargo tauri build --no-bundle
```

できた `target/release/face-util-sender.exe` を `Bin~/FaceUtilSender.exe` に置きます。

## ライセンス

MIT License（[LICENSE](LICENSE)）
