using System.Collections.Generic;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using Object = UnityEngine.Object;

namespace Poyo.FaceUtil.Editor
{
    internal enum FaceUtilTrackingKind
    {
        None,
        LipSync,
        Eyelid,
    }

    /// <summary>VRChat が動かす口パク・まばたきのブレンドシェイプ。</summary>
    internal sealed class FaceUtilTrackingShapes
    {
        internal const string CopyPrefix = "FaceUtil/";

        private readonly Dictionary<SkinnedMeshRenderer, Dictionary<string, FaceUtilTrackingKind>> _shapes =
            new Dictionary<SkinnedMeshRenderer, Dictionary<string, FaceUtilTrackingKind>>();

        internal static FaceUtilTrackingShapes Resolve(Transform avatarRoot)
        {
            var result = new FaceUtilTrackingShapes();
            var descriptor = avatarRoot == null ? null : avatarRoot.GetComponent<VRCAvatarDescriptor>();
            if (descriptor == null)
            {
                return result;
            }

            var visemeMesh = descriptor.VisemeSkinnedMesh;
            if (descriptor.lipSync == VRC.SDKBase.VRC_AvatarDescriptor.LipSyncStyle.VisemeBlendShape &&
                descriptor.VisemeBlendShapes != null)
            {
                foreach (var name in descriptor.VisemeBlendShapes)
                {
                    result.Add(visemeMesh, name, FaceUtilTrackingKind.LipSync);
                }
            }
            else if (descriptor.lipSync == VRC.SDKBase.VRC_AvatarDescriptor.LipSyncStyle.JawFlapBlendShape)
            {
                result.Add(visemeMesh, descriptor.MouthOpenBlendShapeName, FaceUtilTrackingKind.LipSync);
            }

            var eyes = descriptor.customEyeLookSettings;
            if (descriptor.enableEyeLook &&
                eyes.eyelidType == VRCAvatarDescriptor.EyelidType.Blendshapes &&
                eyes.eyelidsSkinnedMesh != null && eyes.eyelidsSkinnedMesh.sharedMesh != null &&
                eyes.eyelidsBlendshapes != null)
            {
                var mesh = eyes.eyelidsSkinnedMesh.sharedMesh;
                foreach (var index in eyes.eyelidsBlendshapes)
                {
                    if (index >= 0 && index < mesh.blendShapeCount)
                    {
                        result.Add(eyes.eyelidsSkinnedMesh, mesh.GetBlendShapeName(index), FaceUtilTrackingKind.Eyelid);
                    }
                }
            }

            return result;
        }

        private void Add(SkinnedMeshRenderer renderer, string name, FaceUtilTrackingKind kind)
        {
            if (renderer == null || renderer.sharedMesh == null || string.IsNullOrEmpty(name))
            {
                return;
            }

            if (!_shapes.TryGetValue(renderer, out var names))
            {
                names = new Dictionary<string, FaceUtilTrackingKind>();
                _shapes.Add(renderer, names);
            }

            // NOTE: 両方に使われる名前は、口パクとして表示し、複製も 1 つだけ作る。
            if (!names.ContainsKey(name))
            {
                names.Add(name, kind);
            }
        }

        internal FaceUtilTrackingKind KindOf(SkinnedMeshRenderer renderer, string blendShape)
        {
            return renderer != null && blendShape != null && _shapes.TryGetValue(renderer, out var names) &&
                   names.TryGetValue(blendShape, out var kind)
                ? kind
                : FaceUtilTrackingKind.None;
        }

        /// <summary>ビルド中のメッシュへ口パク・まばたきの複製を足す。</summary>
        internal static void CreateCopies(FaceUtilSlotTable table)
        {
            // NOTE: FX のクリップで動かすブレンドシェイプは、レイヤーの重みが 0 でも Animator が毎フレーム書き込む。
            //       口パク・まばたきのブレンドシェイプをそのまま動かすと、Face Util が OFF の間も VRChat の口パク・まばたきが止まるので、
            //       複製を作ってそちらだけを動かす。元のメッシュアセットは変更しない。
            var renderers = new List<SkinnedMeshRenderer>();
            var slotsByRenderer = new Dictionary<SkinnedMeshRenderer, List<FaceUtilSlot>>();
            foreach (var slot in table.Slots)
            {
                if (slot.Tracking == FaceUtilTrackingKind.None)
                {
                    continue;
                }

                if (!slotsByRenderer.TryGetValue(slot.Renderer, out var slots))
                {
                    slots = new List<FaceUtilSlot>();
                    slotsByRenderer.Add(slot.Renderer, slots);
                    renderers.Add(slot.Renderer);
                }

                slots.Add(slot);
            }

            foreach (var renderer in renderers)
            {
                var original = renderer.sharedMesh;
                var weights = new float[original.blendShapeCount];
                for (var i = 0; i < weights.Length; i++)
                {
                    weights[i] = renderer.GetBlendShapeWeight(i);
                }

                var copy = Object.Instantiate(original);
                copy.name = original.name + " (Face Util)";
                var dv = new Vector3[original.vertexCount];
                var dn = new Vector3[original.vertexCount];
                var dt = new Vector3[original.vertexCount];
                foreach (var slot in slotsByRenderer[renderer])
                {
                    var source = original.GetBlendShapeIndex(slot.BlendShape);
                    if (source < 0)
                    {
                        continue;
                    }

                    var name = CopyPrefix + slot.BlendShape;
                    var suffix = 2;
                    while (copy.GetBlendShapeIndex(name) >= 0)
                    {
                        name = CopyPrefix + slot.BlendShape + " " + suffix++;
                    }

                    for (var frame = 0; frame < original.GetBlendShapeFrameCount(source); frame++)
                    {
                        original.GetBlendShapeFrameVertices(source, frame, dv, dn, dt);
                        copy.AddBlendShapeFrame(name, original.GetBlendShapeFrameWeight(source, frame), dv, dn, dt);
                    }

                    slot.AnimatedBlendShape = name;
                }

                renderer.sharedMesh = copy;
                for (var i = 0; i < weights.Length; i++)
                {
                    renderer.SetBlendShapeWeight(i, weights[i]);
                }

                for (var i = weights.Length; i < copy.blendShapeCount; i++)
                {
                    renderer.SetBlendShapeWeight(i, 0f);
                }
            }
        }
    }
}
