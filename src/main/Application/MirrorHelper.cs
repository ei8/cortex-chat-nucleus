using ei8.Cortex.Chat.Common;

namespace ei8.Cortex.Chat.Nucleus.Application
{
    public static class MirrorHelper
    {
        public static MirrorStates GetState(string mirrorUrl) =>
            string.IsNullOrWhiteSpace(mirrorUrl) ?
                Common.MirrorStates.None :
                Common.MirrorStates.Remote;
    }
}
