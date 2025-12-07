# TileW materials naming guide

The `path1.png` … `path11.png` textures represent 3×3 tiles where the walkable area can enter from the north, south, east, or west to form straights, bends, T-junctions, a crossroad, a fully filled tile, or an empty/background tile. The numbered filenames make it hard to know which texture to drop into a tileset, so use the following directional naming scheme instead.

## Proposed naming pattern

Use a consistent `path_<shape>[_<orientation>]` pattern based on which edges are open:

- `path_blank` – no path at all (background only).
- `path_full` – path covers the whole tile (all adjacent tiles connect).
- `path_ns`, `path_ew` – straight corridors.
- `path_ne`, `path_nw`, `path_se`, `path_sw` – 90° bends with two directions.
- `path_nse`, `path_new`, `path_esw`, `path_nsw` – T-junctions (three-way connections listed in clockwise order starting at north).
- `path_nesw` – four-way crossroads.

If you want variants (e.g., cracked vs. clean), append a suffix such as `_v1`, `_v2`, or `_mossy`.

## How to map the existing files

1. Open each numbered texture in your viewer and note which edges are walkable.
2. Rename or alias the file using the table above (e.g., the tile with north+east connections becomes `path_ne.png`).
3. Keep metadata GUIDs stable by renaming through the Unity editor when possible.

To speed up step 1 in this repo, you can run a quick ASCII preview that samples luminance and hints at the shape:

```bash
python scripts/preview_paths.py UnityProject/Assets/TileW/materials/path*.png
```

The script prints a tiny ASCII grid for each texture so you can see whether the path runs straight, turns, or branches. Once you identify each shape, rename the files to match the directional scheme above so future agents immediately understand which tile to use.
