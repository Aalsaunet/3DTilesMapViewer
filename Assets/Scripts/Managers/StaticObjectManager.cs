using System.Text.RegularExpressions;
using UnityEngine;

public class StaticObjectManager : MonoBehaviour
{
    ////// PUBLIC PREFABS /////

    // TANKS and PIPES PREFABS // 
    public GameObject smallWhiteStationaryTankPrefab;
    public GameObject smallGreyStationaryTankPrefab;
    public GameObject smallBlueStationaryTankPrefab;
    public GameObject smallRedStationaryTankPrefab;
    public GameObject mediumWhiteStationaryTankPrefab;
    public GameObject mediumGreyStationaryTankPrefab;
    public GameObject mediumBlueStationaryTankPrefab;
    public GameObject mediumRedStationaryTankPrefab;
    public GameObject largeStationaryTankPrefab;
    public GameObject SphereStationaryTankPrefab;
    public GameObject shortSiloPrefab;
    public GameObject tallSiloPrefab;
    public GameObject pipesPrefab;
    public GameObject capsuleTankPrefab;
    public GameObject capsuleTankTallPrefab;
    public GameObject squatTankPrefab;
    public GameObject mStationPrefab;
    public GameObject mStationCompactPrefab;
    public GameObject pipesAssemblyPrefab;
    public GameObject pipesPartStraightPrefab;
    public GameObject pipesPartAscendingPrefab;
    public GameObject pipesPartTurnPrefab;
    public GameObject chimneyPrefab;
    public GameObject lightMastPrefab;

    // MISC PREFABS //
    public GameObject lampPostPrefab;
    public GameObject boomBarrierPrefab;
    public GameObject fenceGatePrefab;
    public GameObject millenniumFalconPrefab;
    public GameObject unicornPrefab;
    public GameObject greenBuoyPrefab;
    public GameObject redBuoyPrefab;
    public GameObject kioskBillboardPrefab;

    public void InstantiateStaticObject(GeoPosition gpos, Vector3 position, Quaternion rotation) {
        GameObject geoGameObject = Instantiate(FindPrefabForGeoObject(gpos), position, rotation, 
                                                RefManager.Get<GeoPositionManager>().geoObjectContainer);
        GeoObjectInstance goi = geoGameObject.AddComponent<GeoObjectInstance>();
        goi.info = new GeoObjectInfo(gpos);
        geoGameObject.name = goi.info.DisplayName;
        if (gpos.GeoModelCode == StaticGeoCode.LIGHT_MAST)
            goi.isLightMast = true;

        if (GeoConsts.USE_CUSTOM_SCALING)
            TryParseAndScaleGeoArea(geoGameObject, gpos, goi);

        GeoPositionManager.AddToGeoPositionDataStructures(gpos.Id.ToString(), gpos, geoGameObject, goi);
    }

    private GameObject FindPrefabForGeoObject(GeoPosition gpos) {
        return gpos.GeoModelCode switch {
            StaticGeoCode.LARGE_STATIONARY_TANK_WHITE => largeStationaryTankPrefab,
            StaticGeoCode.MEDIUM_STATIONARY_TANK_WHITE => mediumWhiteStationaryTankPrefab,
            StaticGeoCode.MEDIUM_STATIONARY_TANK_GREY => mediumGreyStationaryTankPrefab,
            StaticGeoCode.MEDIUM_STATIONARY_TANK_BLUE => mediumBlueStationaryTankPrefab,
            StaticGeoCode.MEDIUM_STATIONARY_TANK_RED => mediumRedStationaryTankPrefab,
            StaticGeoCode.SMALL_STATIONARY_TANK_WHITE => smallWhiteStationaryTankPrefab,
            StaticGeoCode.SMALL_STATIONARY_TANK_GREY => smallGreyStationaryTankPrefab,
            StaticGeoCode.SMALL_STATIONARY_TANK_BLUE => smallBlueStationaryTankPrefab,
            StaticGeoCode.SMALL_STATIONARY_TANK_RED => smallRedStationaryTankPrefab,
            StaticGeoCode.SPHERE_STATIONARY_TANK_WHITE => SphereStationaryTankPrefab,
            StaticGeoCode.SHORT_SILO => shortSiloPrefab,
            StaticGeoCode.TALL_SILO => tallSiloPrefab,
            StaticGeoCode.PIPES => pipesPrefab,
            StaticGeoCode.CAPSULE_TANK => capsuleTankPrefab,
            StaticGeoCode.CAPSULE_TANK_TALL => capsuleTankTallPrefab,
            StaticGeoCode.SQUAT_TANK => squatTankPrefab,
            StaticGeoCode.M_STATION => mStationPrefab,
            StaticGeoCode.M_STATION_COMPACT => mStationCompactPrefab,
            StaticGeoCode.PIPES_ASSEMBLY => pipesAssemblyPrefab,
            StaticGeoCode.PIPES_PART_STRAIGHT => pipesPartStraightPrefab,
            StaticGeoCode.PIPES_PART_ASCENDING => pipesPartAscendingPrefab,
            StaticGeoCode.PIPES_PART_TURN => pipesPartTurnPrefab,
            StaticGeoCode.CHIMNEY => chimneyPrefab,
            StaticGeoCode.LIGHT_MAST => lightMastPrefab,
            StaticGeoCode.MILLENNIUMFALCON => millenniumFalconPrefab,
            StaticGeoCode.UNICORN => unicornPrefab,
            StaticGeoCode.LAMPPOST => lampPostPrefab,
            StaticGeoCode.BOOM_BARRIER => boomBarrierPrefab,
            StaticGeoCode.FENCE_GATE => fenceGatePrefab,
            StaticGeoCode.GREENBUOY => greenBuoyPrefab,
            StaticGeoCode.REDBUOY => redBuoyPrefab,
            StaticGeoCode.KIOSKBILLBOARD => kioskBillboardPrefab,
            _ => RefManager.Get<GeoPositionManager>().missingObjectPrefab,
        };
    }

    public void TryParseAndScaleGeoArea(GameObject go, GeoPosition gp, GeoObjectInstance goi) {
        if (go == null || gp == null || goi == null || gp.GeoModelName == null)
            return;

        string oneCoordinatePattern = @"\[\s*(\d+\.{0,1}\d*)\s*\]";
        bool result = ParseCoordinates(oneCoordinatePattern, 1, gp.GeoModelName, out float width, out float height, out float depth);
        
        if (!result) {
            string twoCoordinatePattern = @"\[\s*(\d+\.{0,1}\d*)\s*,\s*(\d+\.{0,1}\d*)\s*\]";
            result = ParseCoordinates(twoCoordinatePattern, 2, gp.GeoModelName, out width, out height, out depth);
        }
        if (!result) {
            string threeCoordinatePattern = @"\[\s*(\d+\.{0,1}\d*)\s*,\s*(\d+\.{0,1}\d*)\s*,\s*(\d+\.{0,1}\d*)\s*\]";
            result = ParseCoordinates(threeCoordinatePattern, 3, gp.GeoModelName, out width, out height, out depth);
        }

        if (!result)
            return;

        BoxCollider collider = go.GetComponent<BoxCollider>(); ; // Use box collider scale as a proxy to bounds
        if (collider == null)
            return;

        Vector3 modelSize = collider.bounds.size;
        go.transform.localScale = new Vector3(width / modelSize.x, height / modelSize.y, depth / modelSize.z);
        goi.info.IsScaledToRealWorld = true;
        goi.info.Dimensions = "Width: " + width + "m, height: " + height + "m, depth: " + depth + "m.";
    }

    private static bool ParseCoordinates(string pattern, int coordinateCount, string modelName, out float width, out float height, out float depth) {
        width = 0f; height = 0f; depth = 0f;
        Regex r = new(pattern, RegexOptions.IgnorePatternWhitespace);
        Match m = r.Match(modelName); // e.g TANK[5], LIGHTMAST[5,10] or GATE[8, 3, 0.5]
        if (!m.Success || m.Groups.Count < coordinateCount)
            return false;

        switch (coordinateCount) {
            case 1: 
                if (float.TryParse(m.Groups[1].Value, out width)) {
                    height = depth = width;
                    return true;
                }
                break;
            case 2:
                if (float.TryParse(m.Groups[1].Value, out width) && float.TryParse(m.Groups[2].Value, out height)) {
                    depth = width;
                    return true;
                }
                break;
            case 3:
                if (float.TryParse(m.Groups[1].Value, out width) && float.TryParse(m.Groups[2].Value, out height) && float.TryParse(m.Groups[3].Value, out depth)) {
                    return true;
                }
                break;
        }
            
        return true;
    }

    // private static bool ParseCoordinates(string pattern, int coordinateCount, string modelName, out float width, out float height, out float depth) {
    //     width = 0f; height = 0f; depth = 0f;
    //     Regex r = new(pattern, RegexOptions.IgnorePatternWhitespace);
    //     Match m = r.Match(modelName); // e.g TANK[5], LIGHTMAST[5,10] or GATE[8, 3, 0.5]
    //     if (!m.Success || m.Groups.Count < coordinateCount)
    //         return false;

    //     if (!float.TryParse(m.Groups[1].Value, out width) || !float.TryParse(m.Groups[2].Value, out height))
    //         return false;
        
    //     if (coordinateCount == 3) {
    //         if (!float.TryParse(m.Groups[3].Value, out depth))
    //             return false;
    //     } else {
    //         depth = width;
    //     }
            
    //     return true;
    // }
}
