using System;
using System.Collections.Generic;
using System.IO;
using Blockiverse.Voxel;
using UnityEngine;

namespace Blockiverse.Persistence
{
    [Serializable]
    public sealed class SavedBlockDelta
    {
        public int X;
        public int Y;
        public int Z;
        public int BlockId;
    }

    [Serializable]
    public sealed class WorldSaveData
    {
        public int SchemaVersion;
        public string WorldName;
        public int Width;
        public int Height;
        public int Depth;
        public int ChunkSize;
        public int Seed;
        public SavedBlockDelta[] ChangedBlocks;
    }

    public sealed class WorldLoadResult
    {
        WorldLoadResult(bool success, WorldSaveData data, string error)
        {
            Success = success;
            Data = data;
            Error = error;
        }

        public bool Success { get; }
        public WorldSaveData Data { get; }
        public string Error { get; }

        public static WorldLoadResult Loaded(WorldSaveData data)
        {
            return new WorldLoadResult(true, data, string.Empty);
        }

        public static WorldLoadResult Failed(string error)
        {
            return new WorldLoadResult(false, null, error);
        }

        public void ApplyTo(VoxelWorld world)
        {
            if (!Success)
                throw new InvalidOperationException("Cannot apply a failed save load result.");
            if (world == null)
                throw new ArgumentNullException(nameof(world));

            foreach (SavedBlockDelta delta in Data.ChangedBlocks ?? Array.Empty<SavedBlockDelta>())
            {
                world.SetBlock(
                    new BlockPosition(delta.X, delta.Y, delta.Z),
                    new BlockId(delta.BlockId),
                    trackChange: false);
            }

            world.ClearChangedBlocks();
        }
    }

    public sealed class WorldSaveMigrationRegistry
    {
        readonly Dictionary<int, Func<WorldSaveData, WorldSaveData>> migrations = new();

        public void Register(int fromSchemaVersion, Func<WorldSaveData, WorldSaveData> migration)
        {
            if (migration == null)
                throw new ArgumentNullException(nameof(migration));

            migrations[fromSchemaVersion] = migration;
        }

        public bool TryMigrateToCurrent(WorldSaveData data, int currentSchemaVersion, out WorldSaveData migrated, out string error)
        {
            migrated = data;
            error = string.Empty;
            var visitedSchemaVersions = new HashSet<int>();

            while (migrated.SchemaVersion != currentSchemaVersion)
            {
                if (!visitedSchemaVersions.Add(migrated.SchemaVersion))
                {
                    error = $"World save migration loop detected at schema {migrated.SchemaVersion}.";
                    return false;
                }

                if (!migrations.TryGetValue(migrated.SchemaVersion, out Func<WorldSaveData, WorldSaveData> migration))
                {
                    error = $"No migration registered for world save schema {migrated.SchemaVersion}.";
                    return false;
                }

                migrated = migration(migrated);

                if (migrated == null)
                {
                    error = "World save migration returned no data.";
                    return false;
                }
            }

            return true;
        }
    }

    public sealed class WorldSaveService
    {
        readonly WorldSaveMigrationRegistry migrationRegistry;

        public const int CurrentSchemaVersion = 1;

        public WorldSaveService(WorldSaveMigrationRegistry migrations)
        {
            migrationRegistry = migrations ?? throw new ArgumentNullException(nameof(migrations));
        }

        public void Save(string path, string worldName, VoxelWorld world)
        {
            if (world == null)
                throw new ArgumentNullException(nameof(world));
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Save path must be non-empty.", nameof(path));

            WorldSaveData data = CreateSaveData(worldName, world);
            string directory = Path.GetDirectoryName(path);

            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            string tempPath = path + ".tmp";
            File.WriteAllText(tempPath, JsonUtility.ToJson(data, prettyPrint: true));
            ReplaceWithTempFile(tempPath, path);
        }

        public WorldLoadResult Load(string path)
        {
            try
            {
                if (!File.Exists(path))
                    return WorldLoadResult.Failed($"World save does not exist: {path}");

                string json = File.ReadAllText(path);

                if (string.IsNullOrWhiteSpace(json) || !json.TrimEnd().EndsWith("}", StringComparison.Ordinal))
                    return WorldLoadResult.Failed("World save is corrupt or incomplete.");

                WorldSaveData data = JsonUtility.FromJson<WorldSaveData>(json);

                if (!IsValid(data, out string validationError))
                    return WorldLoadResult.Failed($"World save is corrupt: {validationError}");

                if (data.SchemaVersion != CurrentSchemaVersion &&
                    !migrationRegistry.TryMigrateToCurrent(data, CurrentSchemaVersion, out data, out string migrationError))
                {
                    return WorldLoadResult.Failed(migrationError);
                }

                if (!IsValid(data, out validationError))
                    return WorldLoadResult.Failed($"World save is corrupt: {validationError}");

                return WorldLoadResult.Loaded(data);
            }
            catch (Exception exception) when (exception is IOException || exception is ArgumentException || exception is UnauthorizedAccessException)
            {
                return WorldLoadResult.Failed($"World save is corrupt or unreadable: {exception.Message}");
            }
        }

        static WorldSaveData CreateSaveData(string worldName, VoxelWorld world)
        {
            var deltas = new List<SavedBlockDelta>();

            foreach (BlockChange change in world.GetChangedBlocks())
            {
                deltas.Add(new SavedBlockDelta
                {
                    X = change.Position.X,
                    Y = change.Position.Y,
                    Z = change.Position.Z,
                    BlockId = change.NewBlock.Value
                });
            }

            return new WorldSaveData
            {
                SchemaVersion = CurrentSchemaVersion,
                WorldName = string.IsNullOrWhiteSpace(worldName) ? "Creative World" : worldName,
                Width = world.Bounds.Width,
                Height = world.Bounds.Height,
                Depth = world.Bounds.Depth,
                ChunkSize = world.ChunkSize,
                Seed = world.Seed,
                ChangedBlocks = deltas.ToArray()
            };
        }

        static bool IsValid(WorldSaveData data, out string error)
        {
            if (data == null)
            {
                error = "missing root data";
                return false;
            }

            if (data.SchemaVersion < 0)
            {
                error = "invalid schema version";
                return false;
            }

            if (data.Width <= 0 || data.Height <= 0 || data.Depth <= 0 || data.ChunkSize <= 0)
            {
                error = "invalid world dimensions";
                return false;
            }

            if (data.ChangedBlocks == null)
                data.ChangedBlocks = Array.Empty<SavedBlockDelta>();

            foreach (SavedBlockDelta delta in data.ChangedBlocks)
            {
                if (delta == null)
                {
                    error = "missing changed block delta";
                    return false;
                }

                bool deltaInBounds = delta.X >= 0 && delta.X < data.Width &&
                                     delta.Y >= 0 && delta.Y < data.Height &&
                                     delta.Z >= 0 && delta.Z < data.Depth;

                if (!deltaInBounds)
                {
                    error = "changed block delta is outside world bounds";
                    return false;
                }

                if (delta.BlockId < 0)
                {
                    error = "changed block delta has an invalid block id";
                    return false;
                }
            }

            error = string.Empty;
            return true;
        }

        static void ReplaceWithTempFile(string tempPath, string path)
        {
            if (!File.Exists(path))
            {
                File.Move(tempPath, path);
                return;
            }

            string backupPath = path + ".bak";

            try
            {
                File.Replace(tempPath, path, backupPath);

                if (File.Exists(backupPath))
                    File.Delete(backupPath);
            }
            catch (Exception exception) when (exception is IOException || exception is PlatformNotSupportedException || exception is UnauthorizedAccessException)
            {
                File.Delete(path);
                File.Move(tempPath, path);
            }
        }
    }
}
