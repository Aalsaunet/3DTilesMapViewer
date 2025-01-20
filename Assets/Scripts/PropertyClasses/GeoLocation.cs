using System;
using System.Text;

[Serializable]
public class GeoLocation
{ 
    // Represent data from the SignalR API //
    #nullable enable
    public DateTime? TimeStamp { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? TrackingId { get; set; }
    public double? Direction { get; set; }
    public Guid? ObjectId { get; set; }
    public Guid? SourceId { get; set; }
    public string? SourceType { get; set; }
    public int Event { get; set; }
    public string EventRef { get; set; }
    #nullable disable

    public string PrintProperties() {
        StringBuilder sb = new StringBuilder();
        sb.AppendFormat("{0}: {1}\n", "TrackingId", TrackingId);
        sb.AppendFormat("{0}: {1}\n", "Latitude", Latitude);
        sb.AppendFormat("{0}: {1}\n", "Longitude", Longitude);
        sb.AppendFormat("{0}: {1}\n", "ObjectId", ObjectId);
        sb.AppendFormat("{0}: {1}\n", "SourceId", SourceId);
        sb.AppendFormat("{0}: {1}\n", "SourceType", SourceType);
        sb.AppendFormat("{0}: {1}\n", "Event", Event);
        sb.AppendFormat("{0}: {1}\n", "EventRef", EventRef);
        return sb.ToString();
    }
}