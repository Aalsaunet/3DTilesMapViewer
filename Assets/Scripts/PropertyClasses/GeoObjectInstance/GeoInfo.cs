public abstract class GeoInfo 
{
    public string DisplayName { get; set; }
    public string TrackingId { get; set; }
    public string Dimensions { get; set; }
    public string LastUpdatedTime { get; set; }
    public bool IsScaledToRealWorld { get; set; }
    public bool IsFromGeoPositionAPI { get; set; }

    public abstract string PrintProperties();
}