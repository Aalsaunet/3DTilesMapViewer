using System;

[Serializable]
public class AISTrackerOffsets
{
    // Every distance is given in meters
    public float? FrontDistance { get; set; }
    public float? BackDistance { get; set; }
    public float? LeftDistance { get; set; }
    public float? RightDistance { get; set; }
}