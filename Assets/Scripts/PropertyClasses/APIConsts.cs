using System;

[Serializable]
public class APIConsts {

    // GeoPosition API constants
    public static string URL_POSITIONS = "https://api.fourprosolutions.com/geo-locations/positions";
    public static string URL_VESSELS = "https://api.fourprosolutions.com/geo-locations/vessels";
    public static string URL_STACK_DIAGRAM = "https://api.fourprosolutions.com/geo-locations/stackdiagram/"; // $"api.fourprosolutions.com/geo-locations/stackdiagram/{gpos.ObjectReferenceNo}"
    public static string URL_STACK_ROW_CONTENT = "https://api.fourprosolutions.com/geo-locations/stackrowcontent/"; // $"https://api.fourprosolutions.com/geo-locations/stackrowcontent/{gpos.ObjectReferenceNo}/{gpos.SourceEntityId}"
    public static string URL_LIST_OBJECT_GEOFENCES = "https://api.fourprosolutions.com/geo-locations/geofencesbyobject/"; // $"https://api.fourprosolutions.com/geo-locations/geofencesbyobject/{geoobjectid}"
    public static string URL_GET_GEOFENCE_DEFINITION = "https://api.fourprosolutions.com/geo-locations/geofence/"; // $"https://api.fourprosolutions.com/geo-locations/geofence/{id}"
    public static string URL_MAINTENANCE = "https://api.fourprosolutions.com/maintenance/queryrequests";
    
    // GeoPosition API keys
    public static string GEO_API_KEY_DEMO = "8f97590faa6c4541a6b9303716b02c23"; // This is the demo key. The dev key is "1f1737cc5c2e42b09d6274feff80669f";
    public static string GEO_API_KEY_KB = "2c33779c2766480cafce1dd83b3e8b9a"; // Kristiansund base;
    public static string GEO_API_KEY_TAN = "38df5a2e6db542a3a394d584034bc28e"; // Tananger (ASCO)";
    public static string GEO_API_KEY_OSLO = "1f1737cc5c2e42b09d6274feff80669f"; // FourPro DEV
    public static string GEO_API_KEY_TRONDHEIM = "d228c8a683e74bf59cca9a722b6a7d4a"; // Trondheim
    public static string MAINTENANCE_API_KEY = "a892c753ad5448a094abe27197501545";

    // Kartverket API constants
    public static string KARTVERKET_FORTOYNINGSINNREDNING_URL = "https://wfs.geonorge.no/skwms1/wfs.havnedata?service=WFS&request=GetFeature&version=2.0.0&typename=app:Fort%C3%B8yningsinnretning";
    public static string KARTVERKET_KAIFRONT_URL = "https://wfs.geonorge.no/skwms1/wfs.havnedata?service=WFS&request=GetFeature&version=2.0.0&typename=app:Kaifront";

    // Signal R constants
    public static string SIGNALR_ENDPOINT_URL = "https://fourpro-core-socket-dev.service.signalr.net/client/?hub=";
    public static string SIGNALR_HUB_DEMO = "fourpro_demo_geo"; // "fourpro_demo_geo" // "norlines_geo" //"ascotan_prod_geo"; //"kristiansund_prod_geo"; //"/visco"; //asco_geo // fourpro_dev_geo
    public static string SIGNALR_HUB_KB = "kristiansund_prod_geo"; 
    public static string SIGNALR_HUB_TAN = "ascotan_prod_geo";
    public static string SIGNALR_HUB_TRONDHEIM = "norlines_geo"; 
    public static string SIGNALR_METHOD_NAME = "setGeoLocation";
    public static string SIGNALR_API_KEY = "hHklQMfunBwp0BGDe+RqjfZTdxbJMkGunvglT3NkHb8="; // Same key used for every hub/location?
    public static string SIGNALR_USERNAME = "FourPro";
    public const int SIGNALR_EVENT_POSITION_UPDATE = 0;
    public const int SIGNALR_EVENT_PICKUP = 10;
    public const int SIGNALR_EVENT_STACK = 20;

    // BarentsWatch constants
    // Combined parameter combines e.g Position and Static messages
    public static string BW_TOKEN_URL = "https://id.barentswatch.no/connect/token";
    public static string BW_CLIENT_ID = "andreas.aalsaunet@fourprosolutions.com:FourPro3DMaps-AIS"; 
    public static string BW_SCOPE = "ais"; 
    public static string BW_CLIENT_SECRET = "fourprosolutions"; 
    public static string BW_GRANT_TYPE = "client_credentials"; 
    public static string BW_CONTENT_TYPE = "application/x-www-form-urlencoded"; 

    public static string BW_DATA_URL_COMBINED_FULL ="https://live.ais.barentswatch.no/v1/combined?modelType=Full"; 
    public static string BW_DATA_URL_COMBINED_SIMPLE ="https://live.ais.barentswatch.no/v1/combined?modelType=Simple";
    public static string BW_DATA_URL = "https://live.ais.barentswatch.no/v1/ais";
    public static string BW_MESSAGE_TYPE_POSITION = "Position";
    public static string BW_MESSAGE_TYPE_STATIC = "Staticdata";

    
    // 3DTiles conversion server constants
    public static string TCS_URL = "http://85.252.76.8:7878/tileset.json";

    // General constants
    public static int MMSI_REQUIRED_DIGIT_COUNT = 9; 

    public enum RTSource {
        SR,
        BW,
        GP
    }

    public enum BWQueryFiler {
        MMSI,
        GEOMETRY,
        ALL_GET,
        ALL_POST
    }
}