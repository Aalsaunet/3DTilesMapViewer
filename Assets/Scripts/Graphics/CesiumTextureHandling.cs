using UnityEngine;
using CesiumForUnity;
using Microsoft.IdentityModel.Tokens;

public class CesiumTextureHandling : MonoBehaviour {
    
    public Texture2D[] sourceTextures;
    public string[] destinationTextureNames;
    public bool useMipMaps;
    protected Cesium3DTileset tileset;

    void Awake() {
        tileset = GetComponent<Cesium3DTileset>();
        tileset.OnTileGameObjectCreated += OnTileGameObjectCreated;
    }

    private void OnTileGameObjectCreated(GameObject tile) {
        if (sourceTextures.IsNullOrEmpty() || destinationTextureNames.IsNullOrEmpty() || sourceTextures.Length != destinationTextureNames.Length) {
            Debug.LogError("sourceTextures or sourceTextureNames were null or empty, or they had different length (they are required to be the same length).");
            return;
        }
        
        MeshRenderer mesh = tile.GetComponentInChildren<MeshRenderer>(true);
        if (mesh == null)
            return;

        for (int i = 0; i < sourceTextures.Length; i++) {
            Texture2D texCopy = new Texture2D(sourceTextures[i].width, sourceTextures[i].height, sourceTextures[i].format, useMipMaps);
            Graphics.CopyTexture(sourceTextures[i], texCopy);
            mesh.material.SetTexture(destinationTextureNames[i], texCopy);
        }
    }

    void OnDestroy() {
        tileset.OnTileGameObjectCreated -= OnTileGameObjectCreated;
    }
}
