using System;
using TMPro;
using UnityEngine;

public class TruckManager : MonoBehaviour
{  
    ////// PUBLIC REFABS /////
    // TRUCKS //
    public GameObject truckPrefab; 
    public GameObject trailerForTruckPrefab;

    public void InstantiateTruck(GeoPosition gpos, Vector3 position, Quaternion rotation) {
        bool addTrackingIDSigns = false;
        GameObject truckModelToInstantiate;
        
        switch (gpos.GeoModelCode) {
            case TruckGeoCode.VOLVOTRUCK: truckModelToInstantiate = truckPrefab; break;
            case TruckGeoCode.TRAILERFORTRUCK: truckModelToInstantiate = trailerForTruckPrefab; addTrackingIDSigns = true; break;
            default: truckModelToInstantiate =  RefManager.Get<GeoPositionManager>().missingObjectPrefab; break;
        }

        GameObject geoGameObject = Instantiate(truckModelToInstantiate, position, rotation, RefManager.Get<GeoPositionManager>().geoObjectContainer);
        GeoObjectInstance goi = geoGameObject.AddComponent<GeoObjectInstance>();
        geoGameObject.tag = GeoConsts.GEO_OBJECT_TAG;
        goi.info = new GeoObjectInfo(gpos);
        geoGameObject.name = goi.info.DisplayName;
        GeoPositionManager.AddToGeoPositionDataStructures(gpos.Id.ToString(), gpos, geoGameObject, goi);
        
        if (addTrackingIDSigns)
            SetTrackingIDSigns(geoGameObject, goi.info.TrackingId);
    }

    private void SetTrackingIDSigns(GameObject ccuObject, string idText) {
        ccuObject.transform.Find("SignTop/Text").GetComponent<TMP_Text>().text = idText;
        ccuObject.transform.Find("SignLeft/Text").GetComponent<TMP_Text>().text = idText;
        ccuObject.transform.Find("SignRight/Text").GetComponent<TMP_Text>().text = idText;
        ccuObject.transform.Find("SignBack/Text").GetComponent<TMP_Text>().text = idText;
    }
}
