using UnityEngine;

public class SliderAttribute : PropertyAttribute
{
    public float min = 0;
    public float max = 1;
	public SliderAttribute(float min, float max)
    {
        this.min = min;
        this.max = max;
    }
}
