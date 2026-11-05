using System;
using System.Collections.Generic;

namespace Poyo.FaceUtil.Editor
{
    [Serializable]
    internal sealed class FaceUtilMap
    {
        public string format = "face-util-map";
        public int version = 1;
        public string mapName;
        public string avatarName;
        public string blueprintId;
        public List<FaceUtilMapAvatar> avatars = new List<FaceUtilMapAvatar>();
        public string generatedAt;
        public FaceUtilMapParameters parameters = new FaceUtilMapParameters();
        public int valueMax = FaceUtilInfo.ValueMax;
        public List<FaceUtilMapSlot> slots = new List<FaceUtilMapSlot>();
    }

    [Serializable]
    internal sealed class FaceUtilMapAvatar
    {
        public string name;
        public string blueprintId;
    }

    [Serializable]
    internal sealed class FaceUtilMapParameters
    {
        public string enabled = FaceUtilInfo.ParamEnabled;
        public string lipSync = FaceUtilInfo.ParamLipSync;
        public List<FaceUtilMapChannel> channels = new List<FaceUtilMapChannel>();
    }

    [Serializable]
    internal sealed class FaceUtilMapChannel
    {
        public string index;
        public string value;
    }

    [Serializable]
    internal sealed class FaceUtilMapSlot
    {
        public int channel;
        public int index;
        public string mesh;
        public string blendShape;
        public float defaultWeight;
        public string group;
    }
}
