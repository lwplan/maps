# Unity integration notes

The map generator already compiles to a Unity-friendly plugin (see `scripts/publish-unity-plugin.sh`). The snippets below show how to drive the generator to retrieve map data and then use Unity APIs to upload textures.

## Generating map data inside Unity

1. Reference the compiled plugin (or source) from `UnityProject/Assets/Scripts/MapGen` so your MonoBehaviours can call the generator types.
2. Instantiate the pipeline with your desired parameters and generate a map in play mode.

```csharp
using maps;

public class MapPreviewBehaviour : MonoBehaviour
{
    [SerializeField] private RawImage preview;

    void Start()
    {
        var pipeline = new GameMapPipeline.GameMapPipeline();
        var parameters = new MapGenParams
        {
            RegionSize = 12,
            NumLevels = 5,
            MinNodesPerLevel = 1,
            MaxNodesPerLevel = 3,
            BifurcationFactor = 0.55f,
            MinDistance = 2f
        };

        var map = pipeline.Execute(parameters); // Calls the generator to retrieve geometry and metadata.

        // You can now use the map for gameplay, routing, or texture generation.
    }
}
```

## Uploading rendered maps to a Unity texture

Render the generated map to a PNG in memory and use `Texture2D.LoadImage` / `ImageConversion.LoadImage` to upload the pixels into Unity's texture.

```csharp
using maps;
using UnityEngine;
using UnityEngine.UI;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;

public class MapPreviewBehaviour : MonoBehaviour
{
    [SerializeField] private RawImage preview;

    void Start()
    {
        var pipeline = new GameMapPipeline.GameMapPipeline();
        var map = pipeline.Execute(new MapGenParams { RegionSize = 12, NumLevels = 5, MinNodesPerLevel = 1, MaxNodesPerLevel = 3 });

        using var rendered = BitmapMapRenderer.Render(map);
        using var stream = new MemoryStream();
        rendered.Save(stream, new PngEncoder());
        var pngBytes = stream.ToArray();

        var texture = new Texture2D(2, 2, TextureFormat.RGBA32, mipChain: false);
        ImageConversion.LoadImage(texture, pngBytes); // or texture.LoadImage(pngBytes);
        texture.Apply(updateMipmaps: false);

        preview.texture = texture;
    }
}
```

`ImageConversion.LoadImage` and `Texture2D.LoadImage` both accept the PNG bytes produced by `BitmapMapRenderer`. The generator is responsible for producing the geometry and renderable data; Unity APIs only handle the upload to GPU memory.

## Using raw pixel buffers

If you prefer to skip PNG encoding, copy the raw RGBA32 pixel buffer from ImageSharp and upload it with `Texture2D.LoadRawTextureData`.

```csharp
using maps;
using UnityEngine;
using SixLabors.ImageSharp.PixelFormats;

// ... inside your MonoBehaviour
var map = pipeline.Execute(parameters); // Generator call
using var rendered = BitmapMapRenderer.Render(map);
using var cloned = rendered.CloneAs<Rgba32>();
var pixelSpan = cloned.GetPixelSpan();

var texture = new Texture2D(cloned.Width, cloned.Height, TextureFormat.RGBA32, mipChain: false);
texture.LoadRawTextureData(pixelSpan); // Upload raw bytes in one call
texture.Apply(updateMipmaps: false);
```

`LoadRawTextureData` expects the raw byte layout to match the `TextureFormat` you choose. `Rgba32` aligns with `TextureFormat.RGBA32`, so the byte order from ImageSharp maps directly to Unity.
