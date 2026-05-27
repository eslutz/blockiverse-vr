using System;
using System.Linq;
using Blockiverse.Voxel;
using Blockiverse.WorldGen;
using NUnit.Framework;

namespace Blockiverse.Tests.EditMode
{
    public sealed class VoxelCoreEditModeTests
    {
        [Test]
        public void DefaultRegistryContainsOriginalM2BlockList()
        {
            BlockRegistry registry = BlockRegistry.CreateDefault();

            string[] expectedNames =
            {
                "Air",
                "Meadow Turf",
                "Loam",
                "Slate",
                "Timber",
                "Leafmass",
                "Clearstone",
                "Coalstone",
                "Copperstone",
                "Ironstone",
                "Workbench",
                "Torchbud",
                "Storage Crate"
            };

            CollectionAssert.AreEquivalent(expectedNames, registry.All.Select(block => block.Name));
            Assert.That(registry.Get(BlockRegistry.Air).Category, Is.EqualTo(BlockCategory.Air));
            Assert.That(registry.Get(BlockRegistry.MeadowTurf).Category, Is.EqualTo(BlockCategory.Terrain));
            Assert.That(registry.Get(BlockRegistry.Timber).Category, Is.EqualTo(BlockCategory.Organic));
            Assert.That(registry.Get(BlockRegistry.Workbench).Category, Is.EqualTo(BlockCategory.Crafted));
            Assert.That(registry.Get(BlockRegistry.Coalstone).Category, Is.EqualTo(BlockCategory.Resource));
        }

        [Test]
        public void RegistryRejectsDuplicateIds()
        {
            var registry = new BlockRegistry();
            registry.Register(new BlockDefinition(new BlockId(7), "First", BlockCategory.Terrain, isSolid: true, isRenderable: true));

            Assert.Throws<InvalidOperationException>(() =>
                registry.Register(new BlockDefinition(new BlockId(7), "Second", BlockCategory.Terrain, isSolid: true, isRenderable: true)));
        }

        [Test]
        public void ChunkCoordinatesMapBoundaryAndNegativePositions()
        {
            Assert.That(ChunkCoordinate.FromBlockPosition(new BlockPosition(0, 0, 0), 16), Is.EqualTo(new ChunkCoordinate(0, 0, 0)));
            Assert.That(ChunkCoordinate.FromBlockPosition(new BlockPosition(15, 15, 15), 16), Is.EqualTo(new ChunkCoordinate(0, 0, 0)));
            Assert.That(ChunkCoordinate.FromBlockPosition(new BlockPosition(16, 16, 16), 16), Is.EqualTo(new ChunkCoordinate(1, 1, 1)));
            Assert.That(ChunkCoordinate.FromBlockPosition(new BlockPosition(-1, -1, -1), 16), Is.EqualTo(new ChunkCoordinate(-1, -1, -1)));

            Assert.That(ChunkCoordinate.LocalPositionFromBlockPosition(new BlockPosition(16, 17, 31), 16), Is.EqualTo(new BlockPosition(0, 1, 15)));
            Assert.That(ChunkCoordinate.LocalPositionFromBlockPosition(new BlockPosition(-1, -17, -32), 16), Is.EqualTo(new BlockPosition(15, 15, 0)));
        }

        [Test]
        public void BoundedWorldStoresBlocksAcrossChunkBoundaries()
        {
            var world = new VoxelWorld(new WorldBounds(32, 16, 32), chunkSize: 16, seed: 42);
            var first = new BlockPosition(15, 1, 15);
            var second = new BlockPosition(16, 1, 16);

            world.SetBlock(first, BlockRegistry.MeadowTurf);
            world.SetBlock(second, BlockRegistry.Slate);

            Assert.That(world.GetBlock(first), Is.EqualTo(BlockRegistry.MeadowTurf));
            Assert.That(world.GetBlock(second), Is.EqualTo(BlockRegistry.Slate));
            Assert.That(world.GetChunkCoordinate(first), Is.EqualTo(new ChunkCoordinate(0, 0, 0)));
            Assert.That(world.GetChunkCoordinate(second), Is.EqualTo(new ChunkCoordinate(1, 0, 1)));
        }

        [Test]
        public void UntrackedBlockMutationDoesNotRecordOrEmitChange()
        {
            var world = new VoxelWorld(new WorldBounds(4, 4, 4), chunkSize: 16, seed: 42);
            var position = new BlockPosition(1, 1, 1);
            int eventCount = 0;
            world.BlockChanged += _ => eventCount++;

            world.SetBlock(position, BlockRegistry.Slate, trackChange: false);

            Assert.That(world.GetBlock(position), Is.EqualTo(BlockRegistry.Slate));
            Assert.That(world.GetChangedBlocks(), Is.Empty);
            Assert.That(eventCount, Is.Zero);
        }

        [Test]
        public void BoundedWorldRejectsOutOfRangeCoordinates()
        {
            var world = new VoxelWorld(new WorldBounds(4, 4, 4), chunkSize: 16, seed: 7);

            Assert.That(world.Bounds.Contains(new BlockPosition(3, 3, 3)), Is.True);
            Assert.That(world.Bounds.Contains(new BlockPosition(4, 3, 3)), Is.False);
            Assert.Throws<ArgumentOutOfRangeException>(() => world.GetBlock(new BlockPosition(-1, 0, 0)));
            Assert.Throws<ArgumentOutOfRangeException>(() => world.SetBlock(new BlockPosition(0, 4, 0), BlockRegistry.Loam));
        }

        [Test]
        public void SetBlockCommandCanUndoMutation()
        {
            var world = new VoxelWorld(new WorldBounds(4, 4, 4), chunkSize: 16, seed: 11);
            var position = new BlockPosition(1, 1, 1);
            world.SetBlock(position, BlockRegistry.Loam, trackChange: false);

            var command = new SetBlockCommand(position, BlockRegistry.Clearstone);
            BlockChange change = command.Execute(world);

            Assert.That(change.PreviousBlock, Is.EqualTo(BlockRegistry.Loam));
            Assert.That(change.NewBlock, Is.EqualTo(BlockRegistry.Clearstone));
            Assert.That(world.GetBlock(position), Is.EqualTo(BlockRegistry.Clearstone));

            command.Undo(world);

            Assert.That(world.GetBlock(position), Is.EqualTo(BlockRegistry.Loam));
        }

        [Test]
        public void FlatCreativePresetCreatesBoundedSpawnSafeWorld()
        {
            BlockRegistry registry = BlockRegistry.CreateDefault();
            var settings = new WorldGenerationSettings(width: 16, height: 8, depth: 16, chunkSize: 16, seed: 123, groundHeight: 2);
            var preset = new FlatCreativeWorldPreset(registry, settings);

            VoxelWorld world = preset.Generate();

            Assert.That(world.Bounds, Is.EqualTo(new WorldBounds(16, 8, 16)));
            Assert.That(world.Seed, Is.EqualTo(123));
            Assert.That(world.GetBlock(new BlockPosition(0, 0, 0)), Is.EqualTo(BlockRegistry.Loam));
            Assert.That(world.GetBlock(new BlockPosition(0, 1, 0)), Is.EqualTo(BlockRegistry.MeadowTurf));
            Assert.That(world.GetBlock(new BlockPosition(0, 2, 0)), Is.EqualTo(BlockRegistry.Air));
            Assert.That(world.GetChangedBlocks(), Is.Empty);
            Assert.That(world.Bounds.Contains(settings.SpawnPosition), Is.True);
            Assert.That(world.GetBlock(settings.SpawnPosition), Is.EqualTo(BlockRegistry.Air));
        }
    }
}
