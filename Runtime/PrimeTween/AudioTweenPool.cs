using System.Collections.Generic;
using PrimeTween;
using UnityEngine;

namespace LeafAudio
{
    /// <summary>
    /// Pools TweenTargets for reuse to allow zero-allocation Tweens on PlaybackHandles
    /// </summary>
    internal static class AudioTweenPool
    {

        static List<AudioTweenTarget> freeTweenTargets = new();
        static List<AudioTweenTarget> usedTweenTargets = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Setup()
        {
            Tween.Delay(float.PositiveInfinity).OnUpdate(freeTweenTargets, (f, t) =>
            {
                // Free TweenTargets if the Tween isDone
                for (int i = usedTweenTargets.Count - 1; i >= 0; i--)
                {
                    if (!usedTweenTargets[i].tween.isAlive)
                    {
                        // Add to free and swap remove from used
                        freeTweenTargets.Add(usedTweenTargets[i]);
                        int lastIndex = usedTweenTargets.Count - 1;
                        usedTweenTargets[i] = usedTweenTargets[lastIndex];
                        usedTweenTargets.RemoveAt(lastIndex);
                    }
                }
            });
        }

        /// <summary>
        /// Returns a target for a tween from the pool
        /// * Note you must set a tween and this target will be freed back to the pool once said tween is dead
        /// </summary>
        /// <returns></returns>
        internal static AudioTweenTarget RentTarget()
        {
            AudioTweenTarget toReturn;
            if (freeTweenTargets.Count > 0)
            {
                int toUse = freeTweenTargets.Count - 1;
                toReturn = freeTweenTargets[toUse];
                freeTweenTargets.RemoveAt(toUse);
            }
            else toReturn = new AudioTweenTarget();


            usedTweenTargets.Add(toReturn);
            return toReturn;
        }
    }
}