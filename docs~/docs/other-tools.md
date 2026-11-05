# ほかのツールとの併用

次のツールが入っていれば、自動で連携します。どれも無くても使えます。

## AAO Avatar Optimizer

AAO Avatar Optimizer の Freeze BlendShapes で固定するブレンドシェイプは、ビルドのときにメッシュに焼き込まれてなくなるため、動かせません。Face Util はこれを見つけて、動かす対象から外します。

- 受け手の一覧に `（固定）` と出て、チェックは操作できません。
- 登録数、マップ、送り手のスライダーにも入りません。
- Freeze BlendShapes から外すと、元のオン・オフで一覧に戻ります。固定するものを変えたときは、Face Util の Inspector を開き直すと表示が変わります。

## Avatar Blink Fix

Avatar Blink Fix は、ビルドのときに顔のメッシュを直します。Face Util はその後に組み込むので、直したあとの顔で表情を動かせます。

## Modular Avatar などでビルドのときに値を変えている場合

Modular Avatar の Shape Changer などでブレンドシェイプの値をビルドのときに変えていても、Face Util はその値を既定の値として扱えません。Face Util のほうが先に組み込まれるためです。Face Util を ON にすると、そのブレンドシェイプは変える前の値になります。

表情として動かす必要がなければ、そのブレンドシェイプは受け手の一覧でオフにしてください。
