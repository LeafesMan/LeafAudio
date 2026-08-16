using System;
using PrimeTween;
namespace LeafAudio
{
    public static class PlaybackHandleExtensions
    {
        #region Tween Support
        public static Tween TweenVolume(this PlaybackHandle playbackHandle, float endValue, float duration, Easing ease = default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false, Action<PlaybackHandle> onComplete = null) => TweenVolume(playbackHandle, new TweenSettings<float>(playbackHandle.Volume, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime), onComplete);
        public static Tween TweenVolume(this PlaybackHandle playbackHandle, float startValue, float endValue, float duration, Easing ease = default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false, Action<PlaybackHandle> onComplete = null) => TweenVolume(playbackHandle, new TweenSettings<float>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime), onComplete);
        public static Tween TweenVolume(this PlaybackHandle playbackHandle, TweenSettings<float> tweenSettings, Action<PlaybackHandle> onComplete = null)
        {
            var tweenTarget = AudioTweenPool.RentTarget();
            var tween = Tween.Custom(target: tweenTarget, tweenSettings, (t, newValue) => { t.playback.Volume = newValue; });

            // Apply OnComplete
            if (onComplete != null) tween.OnComplete(target: tweenTarget, t => t.onComplete(t.playback));

            // Cache values for tween target
            tweenTarget.playback = playbackHandle;
            tweenTarget.onComplete = onComplete;
            tweenTarget.tween = tween;

            return tween;
        }

        public static Tween TweenPitch(this PlaybackHandle playbackHandle, float endValue, float duration, Easing ease = default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false, Action<PlaybackHandle> onComplete = null) => TweenPitch(playbackHandle, new TweenSettings<float>(playbackHandle.Pitch, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime), onComplete);
        public static Tween TweenPitch(this PlaybackHandle playbackHandle, float startValue, float endValue, float duration, Easing ease = default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false, Action<PlaybackHandle> onComplete = null) => TweenPitch(playbackHandle, new TweenSettings<float>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime), onComplete);
        public static Tween TweenPitch(this PlaybackHandle playbackHandle, TweenSettings<float> tweenSettings, Action<PlaybackHandle> onComplete = null)
        {
            var tweenTarget = AudioTweenPool.RentTarget();
            var tween = Tween.Custom(target: tweenTarget, tweenSettings, (t, newValue) => { t.playback.Pitch = newValue; });

            // Apply OnComplete
            if (onComplete != null) tween.OnComplete(target: tweenTarget, t => t.onComplete(t.playback));

            // Cache Values for Tween Target
            tweenTarget.playback = playbackHandle;
            tweenTarget.onComplete = onComplete;
            tweenTarget.tween = tween;

            return tween;
        }
        #endregion
    }
}