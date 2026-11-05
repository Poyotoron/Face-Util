using UnityEditor;
using UnityEngine;

namespace Poyo.FaceUtil.Editor
{
    internal static class FaceUtilLegacy
    {
#if FACE_UTIL_HAS_LEGACY
        [InitializeOnLoadMethod]
        private static void Notify()
        {
            const string key = "net.maaaaa.face-util.legacyNoticeShown";
            if (Application.isBatchMode || SessionState.GetBool(key, false)) return;
            SessionState.SetBool(key, true);
            Debug.LogWarning("[Poyo's Face Util] 旧名の Kaotsuki（net.maaaaa.kaotsuki）が同じプロジェクトに入っています。Poyo's Face Util は Kaotsuki の新しい名前です。VCC などのパッケージ管理ツールで Kaotsuki を削除してください。");
        }
#endif
    }
}
