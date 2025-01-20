using System;
using System.Collections.Generic;
using UnityEngine;

public class RefManager : MonoBehaviour
{
    public static RefManager self;

    // Dictionary to store singleton instances of MonoBehavior components
    private static Dictionary<string, MonoBehaviour> instances;

    void Awake() {
        if (self != null)
            return;

        self = this;
        instances = new Dictionary<string, MonoBehaviour>();
    }

    public static T Get<T>() where T : MonoBehaviour {
        string typeName = typeof(T).Name;

        // Check if already cached
        if (instances.ContainsKey(typeName))
            return (T)instances[typeName];

        T[] result = FindObjectsOfType<T>();
        if (result == null || result.Length == 0) {
            Debug.LogError($"Could not find {typeName} instance in the scene.");
            return null;
        } else if (result.Length != 1) {
            Debug.LogError($"This function should only be used with Singletons. Found {result.Length} instance of {typeName}.");
            return null;
        } else { 
            instances[typeName] = result[0]; // Cache for further use
            return result[0];
        } 
    }
}
