using System.IO;
using nadena.dev.modular_avatar.core;
using nadena.dev.ndmf;
using nadena.dev.ndmf.fluent;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

[assembly: ExportsPlugin(typeof(Poyo.FaceUtil.Editor.FaceUtilPlugin))]

namespace Poyo.FaceUtil.Editor
{
    public sealed class FaceUtilPlugin : Plugin<FaceUtilPlugin>
    {
        public override string QualifiedName => FaceUtilInfo.PackageId;
        public override string DisplayName => FaceUtilInfo.DisplayName;

        protected override void Configure()
        {
            // NOTE: スロットの既定値と口パク・まばたきの複製は、この時点のメッシュから作る。ビルド中に顔のメッシュを作り替える
            //       ツールより前に走ると、ON にしたとき古い既定値や修正前の形を書いて顔が変わるため、Generating のツールより後の
            //       Transforming で、Avatar Blink Fix の後に走らせる。入っていないプラグインへの順序指定は NDMF が無視する。
            // NOTE: 生成した Modular Avatar のコンポーネントを Modular Avatar 自身に処理させるため、Modular Avatar より先に走らせる。
            InPhase(BuildPhase.Transforming)
                .AfterPlugin(FaceUtilInfo.BlinkFixPluginId)
                .AfterPlugin(FaceUtilInfo.BlinkFixBakePluginId)
                .BeforePlugin(FaceUtilInfo.ModularAvatarPluginId)
                .Run("Generate receiver", Generate);
        }

        private static void Generate(BuildContext ctx)
        {
            var receivers = ctx.AvatarRootObject.GetComponentsInChildren<FaceUtilReceiver>(true);
            if (receivers.Length == 0)
            {
                return;
            }

            if (receivers.Length > 1)
            {
                FaceUtilErrors.Report(ErrorSeverity.Error, FaceUtilErrors.Multiple);
                return;
            }

            var receiver = receivers[0];
            var table = FaceUtilSlotTable.Build(receiver, ctx.AvatarRootTransform);
            if (table.SkippedReasons.Count > 0)
            {
                FaceUtilErrors.Report(
                    ErrorSeverity.NonFatal,
                    FaceUtilErrors.Skipped,
                    string.Join("\n", table.SkippedReasons));
            }

            if (table.TruncatedCount > 0)
            {
                FaceUtilErrors.Report(ErrorSeverity.NonFatal, FaceUtilErrors.Truncated, table.TruncatedCount);
            }

            if (table.Slots.Count == 0)
            {
                FaceUtilErrors.Report(ErrorSeverity.NonFatal, FaceUtilErrors.NoSlots);
                return;
            }

            FaceUtilTrackingShapes.CreateCopies(table);
            var controller = FaceUtilAnimatorBuilder.Build(table, receiver.overrideEyes);
            var merge = receiver.gameObject.AddComponent<ModularAvatarMergeAnimator>();
            merge.animator = controller;
            merge.layerType = VRCAvatarDescriptor.AnimLayerType.FX;
            merge.pathMode = MergeAnimatorPathMode.Absolute;
            merge.matchAvatarWriteDefaults = false;
            // NOTE: 他の Merge Animator より後ろに並べ、ON の間は登録済みの表情を確実に上書きする。
            merge.layerPriority = 10000;
            merge.deleteAttachedAnimator = false;
            merge.mergeAnimatorMode = MergeAnimatorMode.Append;

            var parameters = receiver.gameObject.AddComponent<ModularAvatarParameters>();
            parameters.parameters.Add(CreateParameter(FaceUtilInfo.ParamEnabled, ParameterSyncType.Bool));
            parameters.parameters.Add(CreateParameter(FaceUtilInfo.ParamLipSync, ParameterSyncType.Bool));
            for (var channel = 1; channel <= table.ChannelCount; channel++)
            {
                parameters.parameters.Add(CreateParameter(FaceUtilInfo.IndexParam(channel), ParameterSyncType.Int));
                parameters.parameters.Add(CreateParameter(FaceUtilInfo.ValueParam(channel), ParameterSyncType.Int));
            }

            try
            {
                var result = FaceUtilMapWriter.Write(table, receiver, ctx.AvatarRootObject);
                Debug.Log("[Poyo's Face Util] マップを書き出しました: " + result.Path);
                if (result.RemovedOtherAvatars)
                {
                    FaceUtilErrors.Report(ErrorSeverity.NonFatal, FaceUtilErrors.MapShared, result.MapName);
                }
            }
            catch (IOException e)
            {
                FaceUtilErrors.Report(ErrorSeverity.NonFatal, FaceUtilErrors.MapWrite, e.Message);
            }
            catch (System.UnauthorizedAccessException e)
            {
                FaceUtilErrors.Report(ErrorSeverity.NonFatal, FaceUtilErrors.MapWrite, e.Message);
            }
        }

        private static ParameterConfig CreateParameter(string name, ParameterSyncType type)
        {
            return new ParameterConfig
            {
                nameOrPrefix = name,
                syncType = type,
                defaultValue = 0f,
                saved = false,
                localOnly = false,
                hasExplicitDefaultValue = true,
            };
        }
    }
}
