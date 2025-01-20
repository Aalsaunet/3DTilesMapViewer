using System.Text;

public class CcuInfo : GeoInfo {
    
    public string Description { get; set; }
    public string GrossWeight { get; set; }
    public string LastModifiedUser { get; set; }
    public bool IsStandaloneCCU { get; set; }

    
    public CcuInfo(dynamic ccuObject) {
        DisplayName = ccuObject.name;
        TrackingId = ((string)ccuObject.serialNo).Trim();
        Description = ccuObject.description;
        GrossWeight = ccuObject.grossWeight;
        LastUpdatedTime = ccuObject.dateTimeLastModified;
        LastModifiedUser = ccuObject.lastModifiedByUser;
        IsStandaloneCCU = false;
    }

    public CcuInfo(GeoPosition gpos) {
        DisplayName = gpos?.DisplayName?.Trim() ?? "[Not set]";
        TrackingId = gpos?.TrackingId?.Trim();
        // CategoryCode = gpos?.CategoryCode?.Trim() ?? "[Not set]";
        // CategoryDisplayName = gpos?.CategoryDisplayName?.Trim() ?? "[Not set]";
        // GeoModelCode = gpos?.GeoModelCode?.Trim() ?? "[Not set]";
        // GeoModelName = gpos?.GeoModelName?.Trim() ?? "[Not set]";
        // CurrentLongitude = gpos?.CurrentLongitude ?? 0.0;
        // CurrentLatitude = gpos?.CurrentLatitude ?? 0.0;
        // LastUpdatedTime = gpos?.LastUpdated?.Trim() ?? "[Not set]";
        Dimensions = "[Not set]";
        IsStandaloneCCU = true;
    }

    public override string PrintProperties() {
        StringBuilder sb = new StringBuilder();
        sb.AppendFormat("{0}: {1}\n", "Name", DisplayName);
        sb.AppendFormat("{0}: {1}\n", "Serial Number", TrackingId);
        sb.AppendFormat("{0}: {1}\n", "Description", Description);
        sb.AppendFormat("{0}: {1}kg\n", "Gross weight", GrossWeight);
        sb.AppendFormat("{0}: {1}\n", "Last modified", LastUpdatedTime);
        sb.AppendFormat("{0}: {1}\n", "Modified by", LastModifiedUser);
        return sb.ToString();
    }
}