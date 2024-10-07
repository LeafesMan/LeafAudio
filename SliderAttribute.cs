/*
 *  Name: Ian
 *
 *  Proj: MonMon
 *
 *  Date: 7/27/23 
 *  
 *  Desc: Property Attribute for a sliders min and max value.
 */


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
