using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Poyo.FaceUtil.Editor
{
    internal static class FaceUtilSenderLauncher
    {
        /// <summary>パッケージ内の送り手 exe の絶対パス。パッケージが見つからなければ null。</summary>
        internal static string SenderExePath
        {
            get
            {
                var info = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(FaceUtilSenderLauncher).Assembly);
                return info == null
                    ? null
                    : Path.GetFullPath(Path.Combine(info.resolvedPath, FaceUtilInfo.SenderExeRelativePath));
            }
        }

        /// <summary>送り手を起動する。mapPath が null なら引数なし。起動できたら true。</summary>
        internal static bool Launch(string mapPath)
        {
            if (!IsWindows)
            {
                return false;
            }

            var exe = SenderExePath;
            if (exe == null || !File.Exists(exe))
            {
                EditorUtility.DisplayDialog(
                    FaceUtilInfo.DisplayName,
                    "送り手アプリが見つかりません。\n" + (exe ?? "(パッケージが見つかりません)"),
                    "OK");
                return false;
            }

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = exe,
                    Arguments = mapPath == null ? string.Empty : "\"" + mapPath + "\"",
                    UseShellExecute = true,
                    WorkingDirectory = Path.GetDirectoryName(exe),
                });
                return true;
            }
            catch (Exception e) when (e is Win32Exception || e is InvalidOperationException || e is IOException)
            {
                Debug.LogError("[Poyo's Face Util] 送り手を起動できませんでした: " + e.Message);
                return false;
            }
        }

        /// <summary>マップのフォルダをエクスプローラーで開く（無ければ作る）。</summary>
        internal static void OpenMapFolder()
        {
            try
            {
                Directory.CreateDirectory(FaceUtilMapWriter.MapFolder);
                Process.Start(new ProcessStartInfo
                {
                    FileName = FaceUtilMapWriter.MapFolder,
                    UseShellExecute = true,
                });
            }
            catch (Exception e) when (e is Win32Exception || e is InvalidOperationException || e is IOException || e is UnauthorizedAccessException)
            {
                Debug.LogError("[Poyo's Face Util] マップのフォルダを開けませんでした: " + e.Message);
            }
        }

        [MenuItem(FaceUtilInfo.OpenSenderMenuPath, false, 1000)]
        private static void OpenSenderMenu() => Launch(null);

        [MenuItem(FaceUtilInfo.OpenSenderMenuPath, true)]
        private static bool ValidateOpenSender() => IsWindows;

        [MenuItem(FaceUtilInfo.OpenMapFolderMenuPath, false, 1001)]
        private static void OpenMapFolderMenu() => OpenMapFolder();

        [MenuItem(FaceUtilInfo.OpenMapFolderMenuPath, true)]
        private static bool ValidateOpenMapFolder() => IsWindows;

        private static bool IsWindows => Application.platform == RuntimePlatform.WindowsEditor;
    }
}
