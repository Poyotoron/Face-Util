using System;
using System.Collections.Generic;
using nadena.dev.modular_avatar.core;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using Object = UnityEngine.Object;

namespace Poyo.FaceUtil.Editor
{
    internal static class FaceUtilSetupMenu
    {
        [MenuItem(FaceUtilInfo.SetupMenuPath, false, FaceUtilInfo.SetupMenuPriority)]
        private static void Setup(MenuCommand command)
        {
            if (IsDuplicateInvocation(command))
            {
                return;
            }

            var roots = new List<Transform>();
            var seen = new HashSet<Transform>();
            foreach (var selected in Selection.gameObjects)
            {
                var descriptor = selected.GetComponentInParent<VRCAvatarDescriptor>(true);
                if (descriptor != null && seen.Add(descriptor.transform))
                {
                    roots.Add(descriptor.transform);
                }
            }

            GameObject lastCreated = null;
            foreach (var root in roots)
            {
                var existing = root.GetComponentsInChildren<FaceUtilReceiver>(true);
                if (existing.Length > 0)
                {
                    Selection.activeGameObject = existing[0].gameObject;
                    EditorUtility.DisplayDialog(FaceUtilInfo.DisplayName, "このアバターにはすでに Poyo's Face Util があります。", "OK");
                    continue;
                }

                var go = new GameObject("Face Util");
                go.transform.SetParent(root, false);
                var receiver = go.AddComponent<FaceUtilReceiver>();
                // NOTE: 素体の Prefab で Setup すると、その Variant（別衣装）にも同じマップ名が継承され、1 つのマップを共有できる。
                receiver.mapName = FaceUtilMapWriter.SanitizeName(root.name);
                // NOTE: 通常の Modular Avatar の手順で、利用者がメニューの置き場所を変えられるようにする。
                go.AddComponent<ModularAvatarMenuInstaller>();
                var item = go.AddComponent<ModularAvatarMenuItem>();
                FaceUtilMenuUpgrader.BuildSubMenu(item, "Face Util Setup");

                var body = FindBody(root);
                if (body != null)
                {
                    receiver.meshes.Add(new FaceUtilMeshEntry { renderer = body });
                }
                else
                {
                    Debug.LogWarning(
                        "[Poyo's Face Util] Body メッシュが見つかりませんでした。Inspector でメッシュを追加してください。",
                        go);
                }

                Undo.RegisterCreatedObjectUndo(go, "Face Util Setup");
                lastCreated = go;
            }

            if (lastCreated != null)
            {
                Selection.activeGameObject = lastCreated;
            }
        }

        [MenuItem(FaceUtilInfo.SetupMenuPath, true)]
        private static bool ValidateSetup()
        {
            return Selection.activeGameObject != null &&
                   Selection.activeGameObject.GetComponentInParent<VRCAvatarDescriptor>(true) != null;
        }

        private static bool IsDuplicateInvocation(MenuCommand command)
        {
            Object[] objects = Selection.objects;
            return objects.Length > 1 && command.context != null && command.context != objects[0];
        }

        private static SkinnedMeshRenderer FindBody(Transform root)
        {
            for (var i = 0; i < root.childCount; i++)
            {
                var child = root.GetChild(i);
                if (string.Equals(child.name, "Body", StringComparison.Ordinal))
                {
                    var renderer = child.GetComponent<SkinnedMeshRenderer>();
                    if (renderer != null)
                    {
                        return renderer;
                    }
                }
            }

            foreach (var renderer in root.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                if (string.Equals(renderer.name, "Body", StringComparison.Ordinal))
                {
                    return renderer;
                }
            }

            return null;
        }
    }
}
