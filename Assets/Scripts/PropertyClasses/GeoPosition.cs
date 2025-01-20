using System;
using System.Text;

[Serializable]
public class GeoPosition
{
    // Represent data from the GeoPositions API //
    #nullable enable
    public Guid Id { get; set; }
    public string? SourceEntityType { get; set; }
    public Guid? SourceEntityId { get; set; }
    public DateTime? DateTimeStamp { get; set; }
    public string? DisplayName { get; set; }
    public string? ObjectModelId { get; set; }
    public string? ObjectReferenceNo { get; set; }
    public bool? IsMovableObject { get; set; }
    public bool? IsGeofenceSupported { get; set; }
    public string? TrackingId { get; set; }
    public string? LastUpdated { get; set; }
    public double? Direction { get; set; }
    public string? HideFromMap { get; set; }
    public string? AltModelRef { get; set; }
    public int? EventCode { get; set; }
    public string? EventRef { get; set; }
    public Guid? GeoObjectCategoryId { get; set; }
    public double? CurrentLongitude { get; set; }
    public double? CurrentLatitude { get; set; }
    public string? CategoryCode { get; set; }
    public string? CategoryDisplayName { get; set; }
    public string? CategoryReferenceType { get; set; }
    public string? IconReference { get; set; }
    public string? GeoModelCode { get; set; }
    public string? GeoModelName { get; set; }
    public string? AltModelId { get; set; }
    public string? Configuration { get; set; }
    public bool? IsNoTracking { get; set; }
    public bool? IsEditable { get; set; }
    public double? GoodsOffset { get; set; }
    public double? GoodsDirectionOffset { get; set; }

    public string PrintProperties() {
        StringBuilder sb = new StringBuilder();
        sb.AppendFormat("{0}: {1}\n", "Name", DisplayName);
        sb.AppendFormat("{0}: {1}\n", "Category", CategoryDisplayName);
        sb.AppendFormat("{0}: {1}\n", "GeoModelName", GeoModelName);
        sb.AppendFormat("{0}: {1}\n", "GeoCode", GeoModelCode);
        sb.AppendFormat("{0}: {1}\n", "Tracking ID", TrackingId);
        sb.AppendFormat("{0}: {1}\n", "Last Updated", LastUpdated);
        return sb.ToString();
    }
    #nullable disable
}

/* Sample: 
{
    "id": "f6db219d-d558-429f-aab4-241dc029391e",
    "sourceEntityType": "common.truck",
    "sourceEntityId": "9c9e5647-1c52-4134-a2af-ce240eeb53fa",
    "dateTimeStamp": "2024-03-04T10:02:20.2316077Z",
    "displayName": "BS37797",
    "objectModelId": "751fa783-923d-430b-8a3d-acef0723daab",
    "objectReferenceNo": "BS37797",
    "isMovableObject": true,
    "isGeofenceSupported": false,
    "trackingId": "BS37797",
    "lastUpdated": "2024-03-04T10:02:22.5502806Z",
    "direction": 172,
    "hideFromMap": null,
    "altModelRef": null,
    "eventCode": null,
    "eventRef": null,
    "geoObjectCategoryId": "0610ebc5-4a39-4524-8f13-1081f5224338",
    "currentLongitude": 10.65801,
    "currentLatitude": 59.428412,
    "categoryCode": "TRUCK",
    "categoryDisplayName": "Truck",
    "categoryReferenceType": "Registration No.",
    "iconReference": "common.icon.truckcargo",
    "geoModelCode": "20",
    "geoModelName": "Volvo FH",
    "altModelId": null,
    "configuration": null,
    "isNoTracking": false,
    "isEditable": false,
    "goodsOffset": null,
    "goodsDirectionOffset": null
}
*/

// Old version:
// public class GeoPosition
// {
//     #nullable enable
//     public Guid Id { get; set; }
//     public string? SourceEntityType { get; set; }
//     public Guid? SourceEntityId { get; set; }
//     public DateTime? DateTimeStamp { get; set; }
//     public string? DisplayName { get; set; }
//     public string? ObjectModelId { get; set; }
//     public string? ObjectReferenceNo { get; set; }
//     public bool? IsMovableObject { get; set; }
//     public bool? IsGeofenceSupported { get; set; }
//     public string? TrackingId { get; set; }
//     public string? LastUpdated { get; set; }
//     public double? Direction { get; set; }
//     public string? HideFromMap { get; set; }
//     public Guid? GeoObjectCategoryId { get; set; }
//     public double? CurrentLongitude { get; set; }
//     public double? CurrentLatitude { get; set; }
//     public string? CategoryCode { get; set; }
//     public string? CategoryDisplayName { get; set; }
//     public string? CategoryReferenceType { get; set; }
//     public string? IconReference { get; set; }
//     public string? GeoModelCode { get; set; }
//     public string? GeoModelName { get; set; }
//     public string? Configuration { get; set; }

//     public string PrintProperties() {
//         StringBuilder sb = new StringBuilder();
//         //sb.AppendFormat("{0}: {1}\n", "ID", Id);
//         sb.AppendFormat("{0}: {1}\n", "Name", DisplayName);
//         sb.AppendFormat("{0}: {1}\n", "Category", CategoryDisplayName);
//         sb.AppendFormat("{0}: {1}\n", "GeoModelName", GeoModelName);
//         sb.AppendFormat("{0}: {1}\n", "GeoCode", GeoModelCode);
//         sb.AppendFormat("{0}: {1}\n", "Tracking ID", TrackingId);
//         sb.AppendFormat("{0}: {1}\n", "Last Updated", LastUpdated);
//         return sb.ToString();
//     }
//     #nullable disable
// }
