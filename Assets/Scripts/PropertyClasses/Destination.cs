using System.ComponentModel;
using UnityEngine;

public enum Destination
{
    [Description("East")]
    east,
    [Description("West")]
    west,
    [Description("Oslo")]
    oslo,
    [Description("Bergen")]
    bergen,
    [Description("Trondheim")]
    trondheim,
    [Description("Tananger")]
    tanager,
    [Description("Fredrikstad")]
    fredrikstad,
    [Description("Kristiansund")]
    kristiansund,
    [Description("Aberdeen")]
    aberdeen,
    [Description("Senegal")]
    senegal,
    [Description("Tromsø")]
    tromso
} 

public static class DestinationExtensions
{
    public static string GetName(this Destination val) {
        DescriptionAttribute[] attributes = (DescriptionAttribute[])val
           .GetType()
           .GetField(val.ToString())
           .GetCustomAttributes(typeof(DescriptionAttribute), false);
        return attributes.Length > 0 ? attributes[0].Description : string.Empty;
    }
}

public class LocationParameters {
    private static int dIndexGenerator;
    public readonly int destinationIndex;
    public readonly string name;
    public readonly double latitude;
    public readonly double longitude;
    public readonly string siteId;
    public readonly string apiKey;
    public readonly string signalRHub;
    public readonly string demoAssetBundle;
    public readonly float cameraYLimit;
    public readonly Texture2D icon;

    public LocationParameters(string n, double lat, double lon, string siteId, string apiKey, string hubName, string demoAssetBundle, float cameraYLimit, Texture2D icon) {
        destinationIndex = dIndexGenerator++;
        name = n;
        latitude = lat;
        longitude = lon;
        this.siteId = siteId;
        this.apiKey = apiKey;
        signalRHub = hubName;
        this.demoAssetBundle = demoAssetBundle;
        this.cameraYLimit = cameraYLimit;
        this.icon = icon;
    }
}