using System;
using System.Collections.Generic;
using System.Text;

[Serializable]
public class VesselInfoWrapper
{
    //public VesselInfo[] Vessels { get; set; }
    public List<VesselInfo> Vessels { get; set; }
}

[Serializable]
public class VesselInfo
{
    #nullable enable
    public Guid Id { get; set; }
    public string? ImoNumber { get; set; }
    public string? Name { get; set; }
    public string? MmsiNumber { get; set; }
    public double? GrossTonnageITC69 { get; set; }
    public string? Flag { get; set; }
    public string? TrackingId { get; set; }
    public double? MetersToBow { get; set; }
    public double? MetersToStern { get; set; }
    public double? MetersToPortSide { get; set; }
    public double? MetersToStarboardSide { get; set; }

    public string PrintProperties() {
        StringBuilder sb = new StringBuilder();
        sb.AppendFormat("{0}: {1}\n", "ID", Id);
        sb.AppendFormat("{0}: {1}\n", "Name", Name);
        sb.AppendFormat("{0}: {1}\n", "ImoNumber", ImoNumber);
        sb.AppendFormat("{0}: {1}\n", "MmsiNumber", MmsiNumber);
        sb.AppendFormat("{0}: {1}\n", "GrossTonnageITC69", GrossTonnageITC69);
        sb.AppendFormat("{0}: {1}\n", "Flag", Flag);
        sb.AppendFormat("{0}: {1}\n", "Tracking ID", TrackingId);
        return sb.ToString();
    }
    #nullable disable
}
