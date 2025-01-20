using System;
using UnityEngine;

[Serializable]
public class GeoConsts {
    public static readonly int GEO_POSITION_RADIUS = 10000; //100000000; //10000; //1000000;
    public static readonly float RAYCAST_LENGTH = 1000f; 
    public static readonly float RAYCAST_ORIGIN_OFFSET = 500f; 
    public static readonly string RAYCAST_LAYER_TERRAIN = "Terrain";
    public static readonly string RAYCAST_LAYER_CCU_STORAGE = "CCUCollider";
    public static readonly string GEO_OBJECT_TAG = "GeoObject";
    public static bool USE_CUSTOM_SCALING = true;

    // TIMERS //
    public static readonly WaitForSeconds GEO_UPDATE_INTERVAL = new WaitForSeconds(120f);
    public static readonly WaitForSeconds ALIGN_TO_TERRAIN_INTERVAL = new WaitForSeconds(1f);
    public static readonly WaitForSeconds INTER_OBJECT_TICK_RATE = new WaitForSeconds(0.01f);
    public static readonly WaitForSeconds INTER_OBJECT_TICK_RATE_DELETE = new WaitForSeconds(0.01f);
}

public class CategoryCode {
    public const string FORKLIFT = "FORKLIFT";
    public const string CRANE = "CRANE";
    public const string TRUCK = "TRUCK";
    public const string VESSEL = "VESSEL";
    public const string CCU = "CCU";
    public const string WAREHOUSEENTRY = "WAREHOUSEENTRY";
    public const string CARGO_ITEM_TYPE = "CARGOITEMTYPE";
    public const string CCUSTACKCHARTROW = "CCUSTACKCHARTROW";
    public const string STACKER = "STACKER";
    public const string LOCATION = "LOCATION";
    public const string SUBLOCATION = "SUBLOCATION";
    public const string QUAY = "QUAY";
    public const string STATIC = "STATIC";
    public static readonly string[] CONTAINER_20FT_DEF = { "HGFU", "DRILL", "Drill", "MUD SKIP", "Mudskip" };
    public static readonly string[] TANK_20FT_DEF = { "ISO", "IPO", "CO-20ft-Tank" };
    public static readonly string[] TANK_40FT_DEF = { "SKNU", "CO-40ft-Tank" };
}

public class SourceEntityType {
    public const string STACKING_CHART = "logistics.ccustackchartrow";
}

public class VesselGeoCode {
    /* 
    VesselGeoCodes are based on AIS Ship Types: https://api.vtexplorer.com/docs/ref-aistypes.html
    0	Not available (default)
    1-19	Reserved for future use
    20	Wing in ground (WIG), all ships of this type
    21	Wing in ground (WIG), Hazardous category A
    22	Wing in ground (WIG), Hazardous category B
    23	Wing in ground (WIG), Hazardous category C
    24	Wing in ground (WIG), Hazardous category D
    25	Wing in ground (WIG), Reserved for future use
    26	Wing in ground (WIG), Reserved for future use
    27	Wing in ground (WIG), Reserved for future use
    28	Wing in ground (WIG), Reserved for future use
    29	Wing in ground (WIG), Reserved for future use
    30	Fishing
    31	Towing
    32	Towing: length exceeds 200m or breadth exceeds 25m
    33	Dredging or underwater ops
    34	Diving ops
    35	Military ops
    36	Sailing
    37	Pleasure Craft
    38	Reserved
    39	Reserved
    40	High speed craft (HSC), all ships of this type
    41	High speed craft (HSC), Hazardous category A
    42	High speed craft (HSC), Hazardous category B
    43	High speed craft (HSC), Hazardous category C
    44	High speed craft (HSC), Hazardous category D
    45	High speed craft (HSC), Reserved for future use
    46	High speed craft (HSC), Reserved for future use
    47	High speed craft (HSC), Reserved for future use
    48	High speed craft (HSC), Reserved for future use
    49	High speed craft (HSC), No additional information
    50	Pilot Vessel
    51	Search and Rescue vessel
    52	Tug
    53	Port Tender
    54	Anti-pollution equipment
    55	Law Enforcement
    56	Spare - Local Vessel
    57	Spare - Local Vessel
    58	Medical Transport
    59	Noncombatant ship according to RR Resolution No. 18
    60	Passenger, all ships of this type
    61	Passenger, Hazardous category A
    62	Passenger, Hazardous category B
    63	Passenger, Hazardous category C
    64	Passenger, Hazardous category D
    65	Passenger, Reserved for future use
    66	Passenger, Reserved for future use
    67	Passenger, Reserved for future use
    68	Passenger, Reserved for future use
    69	Passenger, No additional information
    70	Cargo, all ships of this type
    71	Cargo, Hazardous category A
    72	Cargo, Hazardous category B
    73	Cargo, Hazardous category C
    74	Cargo, Hazardous category D
    75	Cargo, Reserved for future use
    76	Cargo, Reserved for future use
    77	Cargo, Reserved for future use
    78	Cargo, Reserved for future use
    79	Cargo, No additional information
    80	Tanker, all ships of this type
    81	Tanker, Hazardous category A
    82	Tanker, Hazardous category B
    83	Tanker, Hazardous category C
    84	Tanker, Hazardous category D
    85	Tanker, Reserved for future use
    86	Tanker, Reserved for future use
    87	Tanker, Reserved for future use
    88	Tanker, Reserved for future use
    89	Tanker, No additional information
    90	Other Type, all ships of this type
    91	Other Type, Hazardous category A
    92	Other Type, Hazardous category B
    93	Other Type, Hazardous category C
    94	Other Type, Hazardous category D
    95	Other Type, Reserved for future use
    96	Other Type, Reserved for future use
    97	Other Type, Reserved for future use
    98	Other Type, Reserved for future use
    99	Other Type, no additional information
    */
    public const string CRUISE = "20";
    public const string FISHING_BOAT = "30";
    public const string TOWING_VESSEL1 = "31";
    public const string TOWING_VESSEL2 = "32";
    public const string DREDGE_UNDERWATER_OPTS = "33";
    public const string DIVING_OPTS_VESSEL = "34";
    public const string MILITARY_VESSEL = "35";
    public const string SAIL_BOAT = "36";
    public const string PLEASURE_CRAFT = "37";
    public const string HIGH_SPEED_CRAFT1 = "40";
    public const string HIGH_SPEED_CRAFT2 = "41";
    public const string HIGH_SPEED_CRAFT3 = "42";
    public const string HIGH_SPEED_CRAFT4 = "43";
    public const string HIGH_SPEED_CRAFT5 = "44";
    public const string HIGH_SPEED_CRAFT6 = "45";
    public const string HIGH_SPEED_CRAFT7 = "46";
    public const string HIGH_SPEED_CRAFT8 = "47";
    public const string HIGH_SPEED_CRAFT9 = "48";
    public const string HIGH_SPEED_CRAFT10 = "49";
    public const string PSV_GREEN = "50";
    public const string SEARCH_AND_RESCUE_RHIB = "51";
    public const string TUGBOAT = "52";
    public const string PORT_TENDER_VESSEL = "53";
    public const string PASSENGER_VESSEL1 = "60"; 
    public const string PASSENGER_VESSEL2 = "61"; 
    public const string PASSENGER_VESSEL3 = "62"; 
    public const string PASSENGER_VESSEL4 = "63"; 
    public const string PASSENGER_VESSEL5 = "64"; 
    public const string PASSENGER_VESSEL6 = "65"; 
    public const string PASSENGER_VESSEL7 = "66"; 
    public const string PASSENGER_VESSEL8 = "67"; 
    public const string PASSENGER_VESSEL9 = "68"; 
    public const string PASSENGER_VESSEL10 = "69"; 
    public const string CARGO_VESSEL1 = "70";
    public const string CARGO_VESSEL2 = "71"; 
    public const string CARGO_VESSEL3 = "72";
    public const string CARGO_VESSEL4 = "73";
    public const string CARGO_VESSEL5 = "74";
    public const string CARGO_VESSEL6 = "75";
    public const string CARGO_VESSEL7 = "76";
    public const string CARGO_VESSEL8 = "77";
    public const string CARGO_VESSEL9 = "78";
    public const string CARGO_VESSEL10 = "79";
    public const string TANKER_VESSEL1 = "80"; // LNG
    public const string TANKER_VESSEL2 = "81"; 
    public const string TANKER_VESSEL3 = "82"; 
    public const string TANKER_VESSEL4 = "83"; 
    public const string TANKER_VESSEL5 = "84"; 
    public const string TANKER_VESSEL6 = "85"; 
    public const string TANKER_VESSEL7 = "86"; 
    public const string TANKER_VESSEL8 = "87"; 
    public const string TANKER_VESSEL9 = "88"; 
    public const string TANKER_VESSEL10 = "89"; 
    public const string PSV_RED = "100";
    public const string SUBMARINE = "101"; 
    public const string OILRIG = "1000";  
}

public class TruckGeoCode {
    public const string VOLVOTRUCK = "20";
    public const string TRAILERFORTRUCK = "30";
}

public class ForkliftGeoCode {
    public const string FORKLIFT3T = "10";
    public const string KALMARFORKLIFT7T = "20";
    public const string KALMARFORKLIFT15 = "30";
    public const string STACKER = "40";
    public const string KALMARSTACKER = "50";
    public const string WHEELLOADER = "60";
    public const string KALMAR_T2_WITH_TRAILER = "70";
    public const string KALMAR_T2_WITHOUT_TRAILER = "80";
    public const string KALMAR_T2_ONLY_TRAILER = "90";
}

public class CraneGeoCode {
    public const string DEFAULTCRANE = "10";
    public const string RTG_CRANE = "60";  
    public const string HARBOUR_CRANE_EXTENDED = "70";  
    public const string HARBOUR_CRANE_UNEXTENDED = "80";  
    public const string MOBILECRANE = "90";  
}

public class StackingChartGeoCode {
    public const string LONGSIDE_20FT = "10";
    public const string LONGSIDE_40FT = "20";  
    public const string LONGSIDE_TANKS_20FT = "30";
    public const string LONGSIDE_TANKS_40FT = "40";  
    public const string SHORTSIDE_20FT = "510";
    public const string SHORTSIDE_40FT = "520";  
    public const string SHORTSIDE_TANKS_20FT = "530";
    public const string SHORTSIDE_TANKS_40FT = "540"; 
}

public class StaticGeoCode {
    public const string LARGE_STATIONARY_TANK_WHITE = "20";
    public const string MEDIUM_STATIONARY_TANK_WHITE = "30";  
    public const string MEDIUM_STATIONARY_TANK_GREY = "31";  
    public const string MEDIUM_STATIONARY_TANK_BLUE = "32";  
    public const string MEDIUM_STATIONARY_TANK_RED = "33";  
    public const string SMALL_STATIONARY_TANK_WHITE = "40";
    public const string SMALL_STATIONARY_TANK_GREY = "41";
    public const string SMALL_STATIONARY_TANK_BLUE = "42";
    public const string SMALL_STATIONARY_TANK_RED = "43";
    public const string SPHERE_STATIONARY_TANK_WHITE = "50";  
    public const string SHORT_SILO = "60";
    public const string TALL_SILO = "70";  
    public const string PIPES = "80";
    public const string CAPSULE_TANK = "90";
    public const string CAPSULE_TANK_TALL = "100";  
    public const string SQUAT_TANK = "110";
    public const string M_STATION = "120";  
    public const string M_STATION_COMPACT = "130";
    public const string PIPES_ASSEMBLY = "140";  
    public const string PIPES_PART_STRAIGHT = "150";   
    public const string PIPES_PART_ASCENDING = "160";   
    public const string PIPES_PART_TURN = "170";   
    public const string CHIMNEY = "180";   
    public const string LIGHT_MAST = "190";
    public const string MILLENNIUMFALCON = "1000";
    public const string UNICORN = "1001";
    public const string LAMPPOST = "1002";
    public const string BOOM_BARRIER = "1003";
    public const string FENCE_GATE = "1004";
    public const string GREENBUOY = "2000";
    public const string REDBUOY = "2001";
    public const string KIOSKBILLBOARD = "2002";
       
}

public class WarehouseEntryGeoCode {
    public const string TANK_20FT = "10";
    public const string CCU_20FT = "20";  
    public const string TANK_40FT = "30";
    public const string CCU_40FT = "40";  
    public const string CCU_45FT = "45";
    public const string MUDSKIP = "50";
    public const string PALLET_1 = "100";
    public const string PALLET_2 = "101";
    public const string PALLET_3 = "102";
    public const string PALLET_4 = "103";
    public const string PALLET_5 = "104";
    public const string PALLET_6 = "105";
    public const string PALLET_7 = "106";
    public const string PALLET_8 = "107";
    public const string PALLET_9 = "108";
    public const string PALLET_10 = "109";
    public const string PALLET_11 = "110";
    public const string PALLET_12 = "111";
    public const string PALLET_13 = "112";
    public const string PALLET_14 = "113";
    public const string PALLET_15 = "114";
    public const string PALLET_16 = "115";
    public const string PALLET_17 = "116";
    public const string PALLET_18 = "117";
    public const string PALLET_19 = "118";
    public const string PALLET_20 = "119";
}

public class SublocationGeoCode {
    public const string STACKING_AREA = "10";
    public const string FENCE_AREA = "100000";
    public const string HEDGE_AREA = "100001";
    public const string BRICKWALL_AREA = "100002";
}

[Serializable]
public class GeoBounds { 
    public Coordinate northEastCoordinate;
    public Coordinate southWestCoordinate;

    public GeoBounds(Coordinate neb, Coordinate swb) {
        northEastCoordinate = neb; southWestCoordinate = swb;
    }

    public GeoBounds((Coordinate, Coordinate) neswb) {
        northEastCoordinate = neswb.Item1; southWestCoordinate = neswb.Item2;
    }

    public bool Contains(double? latitude, double? longitude) {
        if (latitude == null || longitude == null)
            return false;

        bool inLatRange = latitude < northEastCoordinate.latitude && latitude > southWestCoordinate.latitude;
        bool inLongRange = longitude < northEastCoordinate.longitude && longitude > southWestCoordinate.longitude;
        return inLatRange && inLongRange;
    }

}

[Serializable]
public struct Coordinate {
    public double latitude;
    public double longitude;

    public Coordinate(double lat, double lon) {
        latitude = lat; longitude = lon;
    }

    public Coordinate((double, double) latLong) {
        latitude = latLong.Item1; longitude = latLong.Item2;
    }

    public readonly string PrintLongLat() {
        return $"[{longitude}, {latitude}]";
    }

    public readonly string PrintLatLong() {
        return $"[{latitude}, {longitude}]";
    }

    public static bool operator >(Coordinate a, Coordinate b) { 
        return a.latitude > b.latitude && a.longitude > b.longitude; 
    }

    public static bool operator >=(Coordinate a, Coordinate b) { 
        return a.latitude >= b.latitude && a.longitude >= b.longitude; 
    }

    public static bool operator <(Coordinate a, Coordinate b) { 
        return a.latitude < b.latitude && a.longitude < b.longitude; 
    }

    public static bool operator <=(Coordinate a, Coordinate b) { 
        return a.latitude <= b.latitude && a.longitude <= b.longitude; 
    }
}