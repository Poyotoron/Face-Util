using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Poyo.FaceUtil.Editor
{
    // NOTE: 既定値のままのスロットも含めて、登録済みの全スロットを書く。Write Defaults OFF の環境では、
    //       書かなかったブレンドシェイプが前の表情のまま残るため。
    // NOTE: パスは表情ファイルではなく、このアバターのスロット表のものを使う。別のアバターで保存した表情でも動くようにするため。
    internal static class FaceUtilExpressionClipWriter
    {
        /// <summary>表情ファイルを選ばせ、このアバター用の AnimationClip を保存する。ダイアログを出したら true。</summary>
        internal static bool Run(FaceUtilReceiver receiver, Transform avatarRoot)
        {
            try
            {
                FaceUtilExpression.EnsureFolder();
                var expressionPath = EditorUtility.OpenFilePanel("表情ファイルを選ぶ", FaceUtilExpression.Folder, "json");
                if (string.IsNullOrEmpty(expressionPath))
                {
                    return true;
                }

                var entries = FaceUtilExpression.Load(expressionPath);
                if (entries == null)
                {
                    EditorUtility.DisplayDialog(FaceUtilInfo.DisplayName, "対応していないファイルです。", "OK");
                    return true;
                }

                var table = FaceUtilSlotTable.Build(receiver, avatarRoot);
                if (table.Slots.Count == 0)
                {
                    EditorUtility.DisplayDialog(FaceUtilInfo.DisplayName, "操作できるブレンドシェイプがありません。", "OK");
                    return true;
                }

                var weights = FaceUtilExpression.Match(table.Slots, entries, out var applied, out var unmatched);
                var path = EditorUtility.SaveFilePanelInProject("AnimationClip を保存",
                    Path.GetFileNameWithoutExtension(expressionPath), "anim", "保存先を選んでください。");
                if (string.IsNullOrEmpty(path))
                {
                    return true;
                }

                var bindings = new EditorCurveBinding[table.Slots.Count];
                var curves = new AnimationCurve[table.Slots.Count];
                for (var i = 0; i < table.Slots.Count; i++)
                {
                    var slot = table.Slots[i];
                    bindings[i] = EditorCurveBinding.FloatCurve(slot.Path, typeof(SkinnedMeshRenderer), "blendShape." + slot.BlendShape);
                    curves[i] = new AnimationCurve(new Keyframe(0f, weights[i]));
                }

                var existing = AssetDatabase.LoadMainAssetAtPath(path);
                var clip = existing as AnimationClip;
                if (existing != null && clip == null)
                {
                    EditorUtility.DisplayDialog(FaceUtilInfo.DisplayName, "そのパスには AnimationClip 以外のアセットがあります。", "OK");
                    return true;
                }

                if (clip != null)
                {
                    // NOTE: 既存のクリップを作り直すと GUID が変わり、参照している Animator が外れるため、中身だけを書き換える。
                    clip.ClearCurves();
                }
                else
                {
                    clip = new AnimationClip { name = Path.GetFileNameWithoutExtension(path) };
                }

                AnimationUtility.SetEditorCurves(clip, bindings, curves);
                if (existing == null)
                {
                    AssetDatabase.CreateAsset(clip, path);
                }

                EditorUtility.SetDirty(clip);
                AssetDatabase.SaveAssets();
                EditorGUIUtility.PingObject(clip);
                Debug.Log("[Poyo's Face Util] AnimationClip を作りました: " + path + "（" + applied + " 件を適用" +
                          (unmatched > 0 ? "、" + unmatched + " 件はこのアバターにありません" : "") + "）");
                return true;
            }
            catch (Exception error)
            {
                Debug.LogException(error);
                EditorUtility.DisplayDialog(FaceUtilInfo.DisplayName, "AnimationClip を作れませんでした。\n" + error.Message, "OK");
                return true;
            }
        }
    }
}
