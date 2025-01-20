using System;
using UnityEngine;

public class PortVehicleManager : MonoBehaviour
{
    ////// PUBLIC PREFABS /////
    
    // FORKLIFTS // 
    public GameObject kalmarT2OnlyTrailerPrefab;
    public GameObject kalmarT2WithoutTrailerPrefab;
    public GameObject kalmarT2WithTrailerPrefab;
    public GameObject forklift3T;
    public GameObject kalmarForklift7T;
    public GameObject kalmarForklift15T;
    public GameObject stackerPrefab;
    public GameObject kalmarStackerPrefab;
    public GameObject wheelLoaderPrefab;
    
    // CRANES // 
    public GameObject cranePrefab;
    public GameObject mobileCranePrefab;
    public GameObject rtgCranePrefab;
    public GameObject harbourCraneExtended;
    public GameObject harbourCraneUnextended;

    public void InstantiatePortObject(GeoPosition gpos, Vector3 position, Quaternion rotation) {
        GameObject geoGameObject = Instantiate(FindPrefabForGeoObject(gpos) , position, rotation, 
                                                RefManager.Get<GeoPositionManager>().geoObjectContainer);
        GeoObjectInstance goi = geoGameObject.AddComponent<GeoObjectInstance>();
        geoGameObject.tag = GeoConsts.GEO_OBJECT_TAG;
        goi.info = new GeoObjectInfo(gpos);
        geoGameObject.name = goi.info.DisplayName;
        GeoPositionManager.AddToGeoPositionDataStructures(gpos.Id.ToString(), gpos, geoGameObject, goi);
    }

    private GameObject FindPrefabForGeoObject(GeoPosition gpos) {    
        switch (gpos.CategoryCode) {
            case CategoryCode.FORKLIFT:
                if (gpos.GeoModelCode == ForkliftGeoCode.KALMARFORKLIFT7T) return kalmarForklift7T; 
                else if (gpos.GeoModelCode == ForkliftGeoCode.KALMARFORKLIFT15) return kalmarForklift15T; 
                else if (gpos.GeoModelCode == ForkliftGeoCode.KALMARSTACKER) return kalmarStackerPrefab; 
                else if (gpos.GeoModelCode == ForkliftGeoCode.STACKER) return stackerPrefab;
                else if (gpos.GeoModelCode == ForkliftGeoCode.WHEELLOADER) return wheelLoaderPrefab;
                else if (gpos.GeoModelCode == ForkliftGeoCode.KALMAR_T2_WITH_TRAILER) return kalmarT2WithTrailerPrefab;
                else if (gpos.GeoModelCode == ForkliftGeoCode.KALMAR_T2_WITHOUT_TRAILER) return kalmarT2WithoutTrailerPrefab;
                else if (gpos.GeoModelCode == ForkliftGeoCode.KALMAR_T2_ONLY_TRAILER) return kalmarT2OnlyTrailerPrefab;
                else return forklift3T;
            case CategoryCode.CRANE:
                if (gpos.GeoModelCode == CraneGeoCode.RTG_CRANE) return rtgCranePrefab;
                else if (gpos.GeoModelCode == CraneGeoCode.HARBOUR_CRANE_EXTENDED) return harbourCraneExtended;
                else if (gpos.GeoModelCode == CraneGeoCode.HARBOUR_CRANE_UNEXTENDED) return harbourCraneUnextended;
                else if (gpos.GeoModelCode == CraneGeoCode.MOBILECRANE) return mobileCranePrefab;
                else return cranePrefab; 
            default:
                return RefManager.Get<GeoPositionManager>().missingObjectPrefab;
        }
    }
}
