using System.Collections.Generic;
using UnityEngine;

namespace Poyo.FaceUtil
{
    /// <summary>アバターの表情を OSC から操作するための設定。アップロード時には取り除かれる。</summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Poyo's Face Util/Face Util Receiver")]
    public sealed class FaceUtilReceiver : MonoBehaviour, VRC.SDKBase.IEditorOnly
    {
        public List<FaceUtilMeshEntry> meshes = new List<FaceUtilMeshEntry>();

        // ON の間、まばたき・視線を止める。
        public bool overrideEyes = true;

        // マップ名。空ならアバター名を使う。同じマップ名のアバターは、送り手で 1 つのマップを共有する。
        public string mapName = string.Empty;
    }
}
