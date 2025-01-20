using System;

[Serializable]
public class BarentswatchAisPosition
{ 
    // Represent data from the Barentswatch API //
    #nullable enable
    public int Mmsi { get; set; }
    public DateTime Msgtime { get; set; }
    public int? Altitude { get; set; }
    public double? CourseOverGround { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public double? SpeedOverGround { get; set; }
    public int? TrueHeading { get; set; }
    public double? RateOfTurn { get; set; }
    #nullable disable
}