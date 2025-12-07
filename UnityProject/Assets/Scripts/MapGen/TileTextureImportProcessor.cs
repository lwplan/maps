#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Ensures the tile textures under Assets/TileW/materials import with crisp, per-pixel settings.
/// </summary>
public class TileTextureImportProcessor : AssetPostprocessor
{
    private const string MaterialsFolder = "Assets/TileW/materials";

    private void OnPreprocessTexture()
    {
        if (!assetPath.StartsWith(MaterialsFolder))
        {
            return;
        }

        var textureImporter = (TextureImporter)assetImporter;
        textureImporter.mipmapEnabled = false;
        textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
        textureImporter.filterMode = FilterMode.Point;
        textureImporter.wrapMode = TextureWrapMode.Clamp;
        textureImporter.isReadable = true;
    }
}
#endif
