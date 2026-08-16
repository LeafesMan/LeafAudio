using System;
using PrimeTween;

namespace LeafAudio
{
    public static class Extensions
    {
        public static Tween OnComplete(this Tween tween, PlaybackHandle target, Action<PlaybackHandle> onComplete)
        {
            var tweenTarget = target.manager.GetAudioTweenTarget();
            tweenTarget.tween = tween;
            tweenTarget.playback = target;
            tweenTarget.onComplete = onComplete;
            return tween.OnComplete(target: tweenTarget, t => t.onComplete(t.playback));
        }
    }
}
