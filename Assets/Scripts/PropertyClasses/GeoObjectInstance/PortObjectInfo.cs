using System.Text;

public class PortObjectInfo : GeoInfo {
    
    public string Category { get; set; }
    public string MaxLoad { get; set; }
    public string CertificationDate { get; set; }

    
    public PortObjectInfo(string name, string tid, string category, string maxLoad, string certificationDate) {
        DisplayName = name;
        TrackingId = tid;
        Category = category;
        MaxLoad = maxLoad;
        CertificationDate = certificationDate;
    }

    public override string PrintProperties() {
        StringBuilder sb = new StringBuilder();
        sb.AppendFormat("{0}: {1}\n", "Name", DisplayName);
        sb.AppendFormat("{0}: {1}\n", "TrackingId", TrackingId);
        sb.AppendFormat("{0}: {1}\n", "Category", Category);
        sb.AppendFormat("{0}: {1}\n", "Max load", MaxLoad);
        sb.AppendFormat("{0}: {1}\n", "CertificationDate", CertificationDate);
        return sb.ToString();
    }
}