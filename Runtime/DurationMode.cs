namespace LeafAudio
{
    public enum DurationMode
    {
        /// <summary>
        /// Duration is measured in amount of time remaining regardless of pitch.
        /// </summary>
        RealTime,
        /// <summary>
        /// Duration is measured as progression through the clip. This scales with pitch so playtime will shrink as pitch increases and vice versa.
        /// </summary>
        ClipTime
    }
}