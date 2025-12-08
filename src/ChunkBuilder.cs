using System;
using System.Collections.Concurrent;
using System.Threading;
using maps.Map3D;

namespace maps
{
    /// <summary>
    /// Asynchronously builds <see cref="TileInfo"/> blocks for a <see cref="GameMap"/> in chunk-sized slices.
    /// The service mirrors <see cref="Map3D.TileMapBuilder"/> logic while keeping the caller non-blocking via
    /// a background worker thread and concurrent queues.
    /// </summary>
    public class ChunkBuilder : IDisposable
    {
        public const int DefaultChunkSize = 64;

        private readonly GameMap _map;
        private readonly int _chunkSize;
        private readonly Map3D.BiomeType[,] _biomeArray;
        private readonly ConcurrentQueue<ChunkRequest> _requests = new();
        private readonly ConcurrentQueue<BuiltChunk> _builtChunks = new();
        private readonly ManualResetEventSlim _workAvailable = new(initialState: false);
        private readonly CancellationTokenSource _cts = new();
        private readonly object _startLock = new();

        private Thread? _workerThread;

        public ChunkBuilder(GameMap map, int chunkSize = DefaultChunkSize)
        {
            _map = map;
            _chunkSize = chunkSize;
            _biomeArray = ConvertBiomes(map.Biomes);
        }

        /// <summary>
        /// Starts the background worker thread. Safe to call multiple times.
        /// </summary>
        public void StartWorker()
        {
            lock (_startLock)
            {
                if (_workerThread != null || _cts.IsCancellationRequested)
                    return;

                _workerThread = new Thread(WorkerLoop)
                {
                    IsBackground = true,
                    Name = "ChunkBuilderWorker"
                };
                _workerThread.Start();
            }
        }

        /// <summary>
        /// Queues a chunk by chunk grid coordinates (chunkX, chunkY).
        /// </summary>
        public void RequestChunk(int chunkX, int chunkY)
        {
            ThrowIfDisposed();
            _requests.Enqueue(new ChunkRequest(chunkX, chunkY));
            _workAvailable.Set();
        }

        /// <summary>
        /// Queues the chunk that contains the provided world tile coordinates.
        /// </summary>
        public void RequestChunkForTile(int tileX, int tileY)
        {
            int chunkX = GetChunkCoordForTile(tileX, _chunkSize);
            int chunkY = GetChunkCoordForTile(tileY, _chunkSize);
            RequestChunk(chunkX, chunkY);
        }

        /// <summary>
        /// Attempts to dequeue a built chunk without blocking.
        /// </summary>
        public bool TryDequeueBuiltChunk(out BuiltChunk chunk) => _builtChunks.TryDequeue(out chunk);

        /// <summary>
        /// Signals the worker to stop and releases waiting threads.
        /// </summary>
        public void Cancel()
        {
            if (_cts.IsCancellationRequested)
                return;

            _cts.Cancel();
            _workAvailable.Set();
        }

        public void Dispose()
        {
            Cancel();

            if (_workerThread != null && _workerThread.IsAlive)
            {
                _workerThread.Join(TimeSpan.FromSeconds(1));
            }

            _workAvailable.Dispose();
            _cts.Dispose();
        }

        public static int GetChunkCoordForTile(int tileCoordinate, int chunkSize = DefaultChunkSize)
        {
            return (int)Math.Floor((double)tileCoordinate / chunkSize);
        }

        private void WorkerLoop()
        {
            var token = _cts.Token;

            try
            {
                while (!token.IsCancellationRequested)
                {
                    if (!_requests.TryDequeue(out var request))
                    {
                        _workAvailable.Wait(token);
                        _workAvailable.Reset();
                        continue;
                    }

                    var chunk = BuildChunk(request, token);
                    _builtChunks.Enqueue(chunk);
                }
            }
            catch (OperationCanceledException)
            {
                // Expected during shutdown
            }
        }

        private BuiltChunk BuildChunk(ChunkRequest request, CancellationToken token)
        {
            int chunkStartTileX = request.ChunkX * _chunkSize;
            int chunkStartTileY = request.ChunkY * _chunkSize;

            // Clamp the chunk to the bounds of the generated map
            int mapMinX = _map.OffsetX;
            int mapMinY = _map.OffsetY;
            int mapMaxX = _map.OffsetX + _map.TileWidth;
            int mapMaxY = _map.OffsetY + _map.TileHeight;

            int clampedStartX = Math.Max(chunkStartTileX, mapMinX);
            int clampedStartY = Math.Max(chunkStartTileY, mapMinY);
            int clampedEndX = Math.Min(chunkStartTileX + _chunkSize, mapMaxX);
            int clampedEndY = Math.Min(chunkStartTileY + _chunkSize, mapMaxY);

            int width = clampedEndX - clampedStartX;
            int height = clampedEndY - clampedStartY;

            if (width <= 0 || height <= 0)
                return new BuiltChunk(request.ChunkX, request.ChunkY, new TileInfo[0, 0]);

            var tiles = new TileInfo[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    token.ThrowIfCancellationRequested();

                    int mapX = clampedStartX + x - _map.OffsetX;
                    int mapY = clampedStartY + y - _map.OffsetY;

                    tiles[x, y] = BuildTile(mapX, mapY);
                }
            }

            return new BuiltChunk(request.ChunkX, request.ChunkY, tiles);
        }

        private TileInfo BuildTile(int x, int y)
        {
            TileInfo t = new TileInfo
            {
                IsPaved = _map.PavedMask[x, y],
                IsEventNode = _map.EventMask[x, y],
                Biome = _biomeArray[x, y],
                ElevationLevel = _map.Elevation?[x, y] ?? 0
            };

            t.PathNeighbors4 = TileNeighbors.GetPathNeighbors(_map.PathMask, x, y);

            t.PavingMask8 = TileNeighbors.GetPavingMask(_map.PavedMask, x, y);
            (t.PavingPattern, t.Rotation) = TileClassifier.ClassifyPaving(t.PavingMask8);

            return t;
        }

        private Map3D.BiomeType[,] ConvertBiomes(BiomeMap biomes)
        {
            int w = biomes.Width;
            int h = biomes.Height;
            var array = new Map3D.BiomeType[w, h];

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    array[x, y] = ConvertBiome(biomes[x, y]);
                }
            }

            return array;
        }

        private Map3D.BiomeType ConvertBiome(BiomeType b)
        {
            return b switch
            {
                BiomeType.Dunes => Map3D.BiomeType.Dune,
                BiomeType.Canyon => Map3D.BiomeType.Canyon,
                BiomeType.Mountain => Map3D.BiomeType.Mountain,
                BiomeType.Sea => Map3D.BiomeType.Sea,
                BiomeType.Town => Map3D.BiomeType.City,
                BiomeType.Battlement => Map3D.BiomeType.Ruins,
                _ => Map3D.BiomeType.Desert
            };
        }

        private void ThrowIfDisposed()
        {
            if (_cts.IsCancellationRequested)
                throw new ObjectDisposedException(nameof(ChunkBuilder));
        }
    }

    public readonly record struct ChunkRequest(int ChunkX, int ChunkY);

    public readonly record struct BuiltChunk(int ChunkX, int ChunkY, TileInfo[,] Tiles);
}
