using System;
using System.Text;

[Serializable]
public class GeoObjectInfo : GeoInfo
{ 
    // Represent aggregated data about the GeoObjectInstance //
    public string CategoryCode { get; set; }
    public string CategoryDisplayName { get; set; }
    public string GeoModelCode { get; set; }
    public string GeoModelName { get; set; }
    public double CurrentLongitude { get; set; }
    public double CurrentLatitude { get; set; }
    
    public GeoObjectInfo() {}
    
    public GeoObjectInfo(GeoPosition gpos) {
        DisplayName = gpos?.DisplayName?.Trim() ?? "[Not set]";
        TrackingId = gpos?.TrackingId?.Trim() ?? "[Not set]";
        CategoryCode = gpos?.CategoryCode?.Trim() ?? "[Not set]";
        CategoryDisplayName = gpos?.CategoryDisplayName?.Trim() ?? "[Not set]";
        GeoModelCode = gpos?.GeoModelCode?.Trim() ?? "[Not set]";
        GeoModelName = gpos?.GeoModelName?.Trim() ?? "[Not set]";
        CurrentLongitude = gpos?.CurrentLongitude ?? 0.0;
        CurrentLatitude = gpos?.CurrentLatitude ?? 0.0;
        LastUpdatedTime = gpos?.LastUpdated?.Trim() ?? "[Not set]";
        Dimensions = "[Not set]";
    }

    public string GetCategory() {
        return CategoryDisplayName + $" ({CategoryCode}).";
    }

    public string GetGeoModelInfo() {
        return GeoModelName + $" ({GeoModelCode}).";
    }
    
    public override string PrintProperties() {
        StringBuilder sb = new StringBuilder();
        sb.AppendFormat("{0}: {1}\n", "Name", DisplayName);
        sb.AppendFormat("{0}: {1}\n", "TrackingID", TrackingId);
        sb.AppendFormat("{0}: {1}\n", "Category", GetCategory());
        sb.AppendFormat("{0}: {1}\n", "GeoModel", GetGeoModelInfo());        
        sb.AppendFormat("{0}: {1}\n", "Dimensions", Dimensions);        
        sb.AppendFormat("{0}: {1}\n", "Last Updated", LastUpdatedTime);
        return sb.ToString();
    }
}