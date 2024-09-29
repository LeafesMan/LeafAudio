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

    public SpatialRolloff(Vector2 range, float power, Vector3 offset, Transform origin = null)
    {
        this.offset = offset;
        this.range = range;
        this.power = Mathf.Clamp(power, 1, 4);
        this.origin = origin;
    }
    public SpatialRolloff(Rolloff rolloff, Vector3 offset, Transform origin = null) : this(rolloff.Range, rolloff.Power, offset, origin) { }

}