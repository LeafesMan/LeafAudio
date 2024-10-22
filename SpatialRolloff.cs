/*
 * Auth: Ian
 * 
 * Proj: ---
 * 
 * Date: 7/24/24
 * 
 * Desc: A Piston that may extend and retract, Either extending to a specified length or stopping after collision
 *          * designed to work with the friction system from PlatformerMovement
 */
using UnityEngine;

[System.Serializable]
public class SpatialRolloff
{
    public readonly Transform origin;
    public readonly Vector3 offset;

    public readonly Vector2 range;
    public readonly float power;


    /// <param name="offset">The shakes positional offset from the passed in origin</param>
    /// <param name="origin">The shakes positional origin</param>
    public SpatialRolloff(Vector2 range, float power, Vector3 offset, Transform origin = null)
    {
        this.offset = offset;
        this.origin = origin;

        this.range = range;
        this.power = Mathf.Clamp(power, 1, 4);
    }

    /// <param name="offset">The shakes positional offset from the passed in origin</param>
    /// <param name="origin">The shakes positional origin</param>
    public SpatialRolloff(Rolloff rolloff, Vector3 offset, Transform origin = null)
    {
        this.origin = origin;
        this.offset = offset;


        if (rolloff == null) return; // Rolloff exists Gate
        this.range = rolloff.Range;
        this.power = rolloff.Power;
    }
}