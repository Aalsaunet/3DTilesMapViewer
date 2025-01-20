using System.Collections.Generic;
using UnityEngine;

public class DemoAssetsController : MonoBehaviour
{
    public GameObject fourProRoofPrefab;
    public GameObject norlinesRoofPrefab;
    public GameObject allianceVCRoofPrefab;
    public GameObject kristiansundBaseDemoPrefab;
    public GameObject soharDemoPrefab;

    private GameObject norlinesRoofInstance;
    private GameObject fourProRoofInstance;
    private GameObject allianceVCRoofInstance;
    private GameObject kristiansundBaseDemoInstance;
    private GameObject soharDemoInstance;

    private Dictionary<string, GameObject> demoAssetNameToDemoRootObject;

    void Awake() {
        demoAssetNameToDemoRootObject = new Dictionary<string, GameObject>();
        fourProRoofInstance = Instantiate(fourProRoofPrefab);
        norlinesRoofInstance = Instantiate(norlinesRoofPrefab);
        allianceVCRoofInstance = Instantiate(allianceVCRoofPrefab);
        kristiansundBaseDemoInstance = Instantiate(kristiansundBaseDemoPrefab);
        soharDemoInstance = Instantiate(soharDemoPrefab);

        demoAssetNameToDemoRootObject.Add("FourProEastDemoAsset", fourProRoofInstance);
        demoAssetNameToDemoRootObject.Add("NorlinesDemoAsset", norlinesRoofInstance);
        demoAssetNameToDemoRootObject.Add("AllianceVCDemoAsset", allianceVCRoofInstance);
        demoAssetNameToDemoRootObject.Add("KRBSDemoAsset", kristiansundBaseDemoInstance);
        demoAssetNameToDemoRootObject.Add("SoharDemoAsset", soharDemoInstance);

        DeactivateAll();
        
    }

    public void TryEnableDemoAsset(string demoAssetName) {
        DeactivateAll();
        if (demoAssetNameToDemoRootObject.ContainsKey(demoAssetName)) {
            demoAssetNameToDemoRootObject[demoAssetName].SetActive(true);
        }
    }

    private void DeactivateAll() {
        fourProRoofInstance.SetActive(false);
        norlinesRoofInstance.SetActive(false);
        allianceVCRoofInstance.SetActive(false);
        kristiansundBaseDemoInstance.SetActive(false);
        soharDemoInstance.SetActive(false);
    }
}

// SetDemoAssetFromDestinationConfig((int)GetComponent<LocationController>().currentDestination);

// public void SetFourProRoofActive(bool active) {
//     fourProRoofInstance.SetActive(active);
// }

// public void SetNorlineRoofActive(bool active) {
//     norlinesRoofInstance.SetActive(active);
// }

// public void SetAllianceVCRoofActive(bool active) {
//     allianceVCRoofInstance.SetActive(active);
// }

// public void SetKristiansundBaseActive(bool active) {
//     kristiansundBaseDemoInstance.SetActive(active);
// }

// public void SetDemoAssetFromDestinationConfig(int dstIndex) {            
//     SetFourProRoofActive(dstIndex == 0); // Only have active in FourPro East
//     SetAllianceVCRoofActive(dstIndex == 2); // Only have active in Oslo
//     SetNorlineRoofActive(dstIndex == 4); // Only have active in Trondheim
//     SetKristiansundBaseActive(dstIndex == 7); // Only have active in Kristiansund
// }