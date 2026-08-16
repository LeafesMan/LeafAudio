using System;
using PrimeTween;
namespace LeafAudio
{
    internal class AudioTweenTarget
    {
        public PlaybackHandle playback;
        public Tween tween;
        public Action<PlaybackHandle> onComplete;
    }
}