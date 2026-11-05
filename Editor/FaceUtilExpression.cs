using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Poyo.FaceUtil.Editor
{
    [Serializable]
    internal sealed class FaceUtilExpressionFile
    {
        public string format;
        public int version;
        public string mapName;
        public string savedAt;
        public List<FaceUtilExpressionEntry> blendShapes;
    }

    [Serializable]
    internal sealed class FaceUtilExpressionEntry
    {
        public string mesh;
        public string blendShape;
        public float weight;
    }

    internal static class FaceUtilExpression
    {
        internal const string Format = "face-util-expression";
        internal const string LegacyFormat = "kaotsuki-expression";
        internal static string LegacyFolder { get; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Kaotsuki", "Expressions");

        internal static void EnsureFolder()
        {
            if (Directory.Exists(Folder)) return;
            try
            {
                Directory.CreateDirectory(Folder);
                if (!Directory.Exists(LegacyFolder)) return;
                // 表情は利用者が作ったものなので、複製して旧フォルダを残す。
                int count = 0;
                foreach (var file in Directory.GetFiles(LegacyFolder, "*.json", SearchOption.TopDirectoryOnly))
                {
                    File.Copy(file, Path.Combine(Folder, Path.GetFileName(file)), false);
                    count++;
                }
                if (count > 0)
                    Debug.Log($"[Poyo's Face Util] 以前の名前（Kaotsuki）で保存した表情を {count} 件、{Folder} に複製しました。");
            }
            catch (IOException ex) { Debug.LogException(ex); }
            catch (UnauthorizedAccessException ex) { Debug.LogException(ex); }
        }
        /// <summary>%LOCALAPPDATA%\FaceUtil\Expressions</summary>
        internal static string Folder { get; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FaceUtil", "Expressions");

        /// <summary>表情ファイルを読み、使えるエントリだけを返す。形式が違えば null。</summary>
        internal static List<FaceUtilExpressionEntry> Load(string path)
        {
            FaceUtilExpressionFile file;
            try
            {
                var text = File.ReadAllText(path, Encoding.UTF8).TrimStart('\uFEFF');
                file = JsonUtility.FromJson<FaceUtilExpressionFile>(text);
            }
            catch (IOException)
            {
                return null;
            }
            catch (UnauthorizedAccessException)
            {
                return null;
            }
            catch (ArgumentException)
            {
                return null;
            }

            if (file == null || (file.format != Format && file.format != LegacyFormat) || file.version != 1 || file.blendShapes == null)
            {
                return null;
            }

            return file.blendShapes.FindAll(entry => entry != null &&
                !string.IsNullOrEmpty(entry.mesh) && !string.IsNullOrEmpty(entry.blendShape) &&
                !float.IsNaN(entry.weight) && !float.IsInfinity(entry.weight) && entry.weight >= 0f && entry.weight <= 100f);
        }

        /// <summary>スロットごとのウェイトを照合で決める。</summary>
        internal static float[] Match(IReadOnlyList<FaceUtilSlot> slots, List<FaceUtilExpressionEntry> entries,
            out int applied, out int unmatched)
        {
            var weights = new float[slots.Count];
            var used = new HashSet<int>();
            for (var i = 0; i < slots.Count; i++)
            {
                var slot = slots[i];
                var match = entries.FindIndex(entry =>
                    string.Equals(entry.mesh, slot.Path, StringComparison.Ordinal) &&
                    string.Equals(entry.blendShape, slot.BlendShape, StringComparison.Ordinal));

                // NOTE: 別のアバターでは顔のメッシュのパスが違うことがある。同じ名前が 1 つだけなら同じブレンドシェイプとみなす。
                if (match < 0)
                {
                    var unique = -1;
                    for (var entryIndex = 0; entryIndex < entries.Count; entryIndex++)
                    {
                        if (!string.Equals(entries[entryIndex].blendShape, slot.BlendShape, StringComparison.Ordinal))
                        {
                            continue;
                        }

                        if (unique >= 0)
                        {
                            unique = -1;
                            break;
                        }

                        unique = entryIndex;
                    }

                    match = unique;
                }

                if (match >= 0)
                {
                    weights[i] = entries[match].weight;
                    used.Add(match);
                }
                else
                {
                    weights[i] = slot.DefaultWeight;
                }
            }

            applied = used.Count;
            unmatched = entries.Count - applied;
            return weights;
        }
    }
}
