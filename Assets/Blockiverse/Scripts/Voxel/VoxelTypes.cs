using System;
using System.Collections.Generic;

namespace Blockiverse.Voxel
{
    public readonly struct BlockId : IEquatable<BlockId>
    {
        public BlockId(int value)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Block IDs must be non-negative.");

            Value = value;
        }

        public int Value { get; }

        public bool Equals(BlockId other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is BlockId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public static bool operator ==(BlockId left, BlockId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(BlockId left, BlockId right)
        {
            return !left.Equals(right);
        }
    }

    public enum BlockCategory
    {
        Air,
        Terrain,
        Organic,
        Crafted,
        Resource
    }

    public sealed class BlockDefinition
    {
        public BlockDefinition(BlockId id, string name, BlockCategory category, bool isSolid, bool isRenderable)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Block names must be non-empty.", nameof(name));

            Id = id;
            Name = name;
            Category = category;
            IsSolid = isSolid;
            IsRenderable = isRenderable;
        }

        public BlockId Id { get; }
        public string Name { get; }
        public BlockCategory Category { get; }
        public bool IsSolid { get; }
        public bool IsRenderable { get; }
    }

    public sealed class BlockRegistry
    {
        readonly Dictionary<BlockId, BlockDefinition> definitionsById = new();
        readonly Dictionary<string, BlockDefinition> definitionsByName = new(StringComparer.OrdinalIgnoreCase);

        public static readonly BlockId Air = new(0);
        public static readonly BlockId MeadowTurf = new(1);
        public static readonly BlockId Loam = new(2);
        public static readonly BlockId Slate = new(3);
        public static readonly BlockId Timber = new(4);
        public static readonly BlockId Leafmass = new(5);
        public static readonly BlockId Clearstone = new(6);
        public static readonly BlockId Coalstone = new(7);
        public static readonly BlockId Copperstone = new(8);
        public static readonly BlockId Ironstone = new(9);
        public static readonly BlockId Workbench = new(10);
        public static readonly BlockId Torchbud = new(11);
        public static readonly BlockId StorageCrate = new(12);

        public IReadOnlyCollection<BlockDefinition> All => definitionsById.Values;

        public static BlockRegistry CreateDefault()
        {
            var registry = new BlockRegistry();
            registry.Register(new BlockDefinition(Air, "Air", BlockCategory.Air, isSolid: false, isRenderable: false));
            registry.Register(new BlockDefinition(MeadowTurf, "Meadow Turf", BlockCategory.Terrain, isSolid: true, isRenderable: true));
            registry.Register(new BlockDefinition(Loam, "Loam", BlockCategory.Terrain, isSolid: true, isRenderable: true));
            registry.Register(new BlockDefinition(Slate, "Slate", BlockCategory.Terrain, isSolid: true, isRenderable: true));
            registry.Register(new BlockDefinition(Timber, "Timber", BlockCategory.Organic, isSolid: true, isRenderable: true));
            registry.Register(new BlockDefinition(Leafmass, "Leafmass", BlockCategory.Organic, isSolid: true, isRenderable: true));
            registry.Register(new BlockDefinition(Clearstone, "Clearstone", BlockCategory.Terrain, isSolid: true, isRenderable: true));
            registry.Register(new BlockDefinition(Coalstone, "Coalstone", BlockCategory.Resource, isSolid: true, isRenderable: true));
            registry.Register(new BlockDefinition(Copperstone, "Copperstone", BlockCategory.Resource, isSolid: true, isRenderable: true));
            registry.Register(new BlockDefinition(Ironstone, "Ironstone", BlockCategory.Resource, isSolid: true, isRenderable: true));
            registry.Register(new BlockDefinition(Workbench, "Workbench", BlockCategory.Crafted, isSolid: true, isRenderable: true));
            registry.Register(new BlockDefinition(Torchbud, "Torchbud", BlockCategory.Crafted, isSolid: false, isRenderable: true));
            registry.Register(new BlockDefinition(StorageCrate, "Storage Crate", BlockCategory.Crafted, isSolid: true, isRenderable: true));
            return registry;
        }

        public void Register(BlockDefinition definition)
        {
            if (definition == null)
                throw new ArgumentNullException(nameof(definition));

            if (definitionsById.ContainsKey(definition.Id))
                throw new InvalidOperationException($"Block ID is already registered: {definition.Id}");

            if (definitionsByName.ContainsKey(definition.Name))
                throw new InvalidOperationException($"Block name is already registered: {definition.Name}");

            definitionsById.Add(definition.Id, definition);
            definitionsByName.Add(definition.Name, definition);
        }

        public BlockDefinition Get(BlockId id)
        {
            if (!definitionsById.TryGetValue(id, out BlockDefinition definition))
                throw new KeyNotFoundException($"Block ID is not registered: {id}");

            return definition;
        }

        public bool TryGet(BlockId id, out BlockDefinition definition)
        {
            return definitionsById.TryGetValue(id, out definition);
        }
    }

    public readonly struct BlockPosition : IEquatable<BlockPosition>
    {
        public BlockPosition(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public int X { get; }
        public int Y { get; }
        public int Z { get; }

        public bool Equals(BlockPosition other)
        {
            return X == other.X && Y == other.Y && Z == other.Z;
        }

        public override bool Equals(object obj)
        {
            return obj is BlockPosition other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = X;
                hash = (hash * 397) ^ Y;
                hash = (hash * 397) ^ Z;
                return hash;
            }
        }

        public override string ToString()
        {
            return $"({X}, {Y}, {Z})";
        }

        public static BlockPosition operator +(BlockPosition left, BlockPosition right)
        {
            return new BlockPosition(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
        }

        public static bool operator ==(BlockPosition left, BlockPosition right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(BlockPosition left, BlockPosition right)
        {
            return !left.Equals(right);
        }
    }

    public readonly struct ChunkCoordinate : IEquatable<ChunkCoordinate>
    {
        public ChunkCoordinate(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public int X { get; }
        public int Y { get; }
        public int Z { get; }

        public static ChunkCoordinate FromBlockPosition(BlockPosition position, int chunkSize)
        {
            ValidateChunkSize(chunkSize);
            return new ChunkCoordinate(
                FloorDiv(position.X, chunkSize),
                FloorDiv(position.Y, chunkSize),
                FloorDiv(position.Z, chunkSize));
        }

        public static BlockPosition LocalPositionFromBlockPosition(BlockPosition position, int chunkSize)
        {
            ValidateChunkSize(chunkSize);
            return new BlockPosition(
                FloorMod(position.X, chunkSize),
                FloorMod(position.Y, chunkSize),
                FloorMod(position.Z, chunkSize));
        }

        public bool Equals(ChunkCoordinate other)
        {
            return X == other.X && Y == other.Y && Z == other.Z;
        }

        public override bool Equals(object obj)
        {
            return obj is ChunkCoordinate other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = X;
                hash = (hash * 397) ^ Y;
                hash = (hash * 397) ^ Z;
                return hash;
            }
        }

        public override string ToString()
        {
            return $"({X}, {Y}, {Z})";
        }

        public static bool operator ==(ChunkCoordinate left, ChunkCoordinate right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ChunkCoordinate left, ChunkCoordinate right)
        {
            return !left.Equals(right);
        }

        static void ValidateChunkSize(int chunkSize)
        {
            if (chunkSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(chunkSize), "Chunk size must be positive.");
        }

        static int FloorDiv(int value, int divisor)
        {
            int quotient = value / divisor;
            int remainder = value % divisor;
            return remainder < 0 ? quotient - 1 : quotient;
        }

        static int FloorMod(int value, int divisor)
        {
            int result = value % divisor;
            return result < 0 ? result + divisor : result;
        }
    }

    public readonly struct WorldBounds : IEquatable<WorldBounds>
    {
        public WorldBounds(int width, int height, int depth)
        {
            if (width <= 0)
                throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0)
                throw new ArgumentOutOfRangeException(nameof(height));
            if (depth <= 0)
                throw new ArgumentOutOfRangeException(nameof(depth));

            Width = width;
            Height = height;
            Depth = depth;
        }

        public int Width { get; }
        public int Height { get; }
        public int Depth { get; }

        public bool Contains(BlockPosition position)
        {
            return position.X >= 0 && position.X < Width &&
                   position.Y >= 0 && position.Y < Height &&
                   position.Z >= 0 && position.Z < Depth;
        }

        public bool Equals(WorldBounds other)
        {
            return Width == other.Width && Height == other.Height && Depth == other.Depth;
        }

        public override bool Equals(object obj)
        {
            return obj is WorldBounds other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = Width;
                hash = (hash * 397) ^ Height;
                hash = (hash * 397) ^ Depth;
                return hash;
            }
        }

        public static bool operator ==(WorldBounds left, WorldBounds right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(WorldBounds left, WorldBounds right)
        {
            return !left.Equals(right);
        }
    }
}
