using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UtilityCoroutines : MonoBehaviour
{
    public static IEnumerator DestroyContainerContent(Transform container) {
        foreach (Transform t in container) {
            Destroy(t.gameObject);
            yield return GeoConsts.INTER_OBJECT_TICK_RATE_DELETE;
        }   
    }
}
