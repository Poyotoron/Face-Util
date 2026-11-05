namespace Poyo.FaceUtil.Editor
{
    /// <summary>パッケージ共通の定数。</summary>
    internal static class FaceUtilInfo
    {
        internal const string PackageId = "net.maaaaa.face-util";
        internal const string DisplayName = "Poyo's Face Util";
        internal const string ModularAvatarPluginId = "nadena.dev.modular-avatar";
        internal const string BlinkFixPluginId = "dev.lemoneru.avatar-blink-fix";
        internal const string BlinkFixBakePluginId = "dev.lemoneru.avatar-blink-fix.redefine";

        internal const string ParamEnabled = "FaceUtil/Enabled";
        internal const string ParamLipSync = "FaceUtil/LipSync";
        internal const string ParamIndex = "FaceUtil/Index";
        internal const string ParamValue = "FaceUtil/Value";

        internal const string ParamOne = "FaceUtil/One";
        internal const string ParamSlotPrefix = "FaceUtil/Slot/";

        internal const int SlotsPerChannel = 255;
        // NOTE: 同期パラメータの上限 256 bit に、Enabled と LipSync（2 bit）と Index + Value（16 bit）の組が収まる最大数。
        internal const int MaxChannels = 15;
        internal const int FixedSyncBits = 2;
        internal const int MaxSlots = SlotsPerChannel * MaxChannels;
        internal const int ValueMax = 255;

        internal const string SetupMenuPath = "GameObject/Poyo's Face Util/Setup";
        internal const int SetupMenuPriority = 30;
        internal const string OpenSenderMenuPath = "Tools/Poyo's Face Util/Open Sender";
        internal const string OpenMapFolderMenuPath = "Tools/Poyo's Face Util/Open Map Folder";

        internal const string SenderExeRelativePath = "Bin~/FaceUtilSender.exe";

        /// <summary>枠の Index パラメータ名（1 → FaceUtil/Index、2 → FaceUtil/Index2 …）。</summary>
        // NOTE: 255 件以下のアバターとの互換性を保つため、1 枠目には番号を付けない。
        internal static string IndexParam(int channel) => channel == 1 ? ParamIndex : ParamIndex + channel;

        internal static string ValueParam(int channel) => channel == 1 ? ParamValue : ParamValue + channel;

        /// <summary>スロット番号から内部パラメータ名を作る（例: 7 → FaceUtil/Slot/0007）。</summary>
        internal static string SlotParam(int number) => ParamSlotPrefix + number.ToString("0000");
    }
}
