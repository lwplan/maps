# CI failure reproduction

Running the renderer CLI build locally shows the same failure the GitHub workflow would hit when restoring `TestBitmap/TestBitmap.csproj`:

```
dotnet build TestBitmap/TestBitmap.csproj -c Release
```

The restore step cannot download dependencies from nuget.org because outbound HTTP(S) traffic is forced through an unauthenticated proxy and receives `403` responses:

```
Retrying 'FindPackagesByIdAsync' for source 'https://api.nuget.org/v3-flatcontainer/sixlabors.fonts/index.json'.
The proxy tunnel request to proxy 'http://proxy:8080/' failed with status code '403'.
...
error NU1301: Failed to retrieve information about 'SixLabors.ImageSharp' from remote source 'https://api.nuget.org/v3-flatcontainer/sixlabors.imagesharp/index.json'.
```

Because the restore step fails, the `renderer-cli` job in `.github/workflows/build.yml` will also fail until the build environment can reach nuget.org without being blocked by the proxy.
