# Path tile filenames

The tiles use a 3×3 subgrid to represent paving (`#`) and sand background (`@`). Each
filename describes which edges of the grid contain a path so that it is easy to match
tiles when building a scene.

Tile previews:

```path_all.png
###
###
###
```

```path_center.png
@@@
@#@
@@@
```

```path_except_ne_and_se_corners.png
##@
###
##@
```

```path_except_swcorner.png
###
###
@##
```

```path_except_w_side.png
@##
@##
@##
```

```path_ne_necorner.png
@##
@##
@@@
```

```path_ne.png
@#@
@##
@@@
```

```path_nesw.png
@#@
###
@#@
```

```path_none.png
@@@
@@@
@@@
```

```path_we.png
@@@
###
@@@
```

```path_wne.png
@#@
###
@@@
```

path2.xcf : Gimp source file

## Suggested improvements
- Use consistent directional abbreviations (e.g., `n`, `s`, `e`, `w`) and clarify whether
  order matters when multiple directions are present.
- Add missing combinations (such as `path_ns.png` or `path_nw.png`) or note why they are
  absent to avoid confusion when selecting tiles.
- Deduplicate filenames—`path_wne.png` appears twice above—so each asset is listed once
  with a single canonical description.
- Include rendered thumbnails or a legend image in this folder to make visual selection
  faster when browsing assets.
