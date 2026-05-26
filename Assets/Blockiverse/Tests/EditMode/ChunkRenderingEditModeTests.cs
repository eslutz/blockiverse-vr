using System.Linq;
using Blockiverse.Gameplay;
using Blockiverse.Voxel;
using NUnit.Framework;
using UnityEngine;

namespace Blockiverse.Tests.EditMode
{
    public sealed class ChunkRenderingEditModeTests
    {
        [Test]
        public void MeshBuilderEmitsOnlyExteriorFacesForSingleSolidBlock()
        {
            BlockRegistry registry = BlockRegistry.CreateDefault();
            var world = new VoxelWorld(new WorldBounds(16, 16, 16), chunkSize: 16, seed: 1);
            world.SetBlock(new BlockPosition(1, 1, 1), BlockRegistry.Slate, trackChange: false);

            ChunkMeshData mesh = ChunkMeshBuilder.Build(world, registry, new ChunkCoordinate(0, 0, 0));

            Assert.That(mesh.FaceCount, Is.EqualTo(6));
            Assert.That(mesh.Vertices, Has.Count.EqualTo(24));
            Assert.That(mesh.Triangles, Has.Count.EqualTo(36));
            Assert.That(mesh.Uvs, Has.Count.EqualTo(24));
        }

        [Test]
        public void MeshBuilderRemovesInternalFacesBetweenAdjacentSolidBlocks()
        {
            BlockRegistry registry = BlockRegistry.CreateDefault();
            var world = new VoxelWorld(new WorldBounds(16, 16, 16), chunkSize: 16, seed: 1);
            world.SetBlock(new BlockPosition(1, 1, 1), BlockRegistry.Slate, trackChange: false);
            world.SetBlock(new BlockPosition(2, 1, 1), BlockRegistry.Loam, trackChange: false);

            ChunkMeshData mesh = ChunkMeshBuilder.Build(world, registry, new ChunkCoordinate(0, 0, 0));

            Assert.That(mesh.FaceCount, Is.EqualTo(10));
            Assert.That(mesh.Triangles, Has.Count.EqualTo(60));
        }

        [Test]
        public void DirtyChunkQueueMarksOnlyMutatedChunkAwayFromBorders()
        {
            var world = new VoxelWorld(new WorldBounds(32, 16, 16), chunkSize: 16, seed: 1);
            var queue = new ChunkRebuildQueue(world);

            world.SetBlock(new BlockPosition(4, 1, 4), BlockRegistry.Slate);

            CollectionAssert.AreEquivalent(
                new[] { new ChunkCoordinate(0, 0, 0) },
                queue.DrainDirtyChunks().ToArray());
        }

        [Test]
        public void DirtyChunkQueueMarksNeighborChunkWhenBorderBlockChanges()
        {
            var world = new VoxelWorld(new WorldBounds(32, 16, 16), chunkSize: 16, seed: 1);
            var queue = new ChunkRebuildQueue(world);

            world.SetBlock(new BlockPosition(15, 1, 4), BlockRegistry.Slate);

            CollectionAssert.AreEquivalent(
                new[] { new ChunkCoordinate(0, 0, 0), new ChunkCoordinate(1, 0, 0) },
                queue.DrainDirtyChunks().ToArray());
        }

        [Test]
        public void RenderStatsReportChunkTriangleAndQueueCounts()
        {
            var stats = new VoxelRenderStats(chunkCount: 4, triangleCount: 120, queuedRebuildCount: 2);

            Assert.That(stats.ChunkCount, Is.EqualTo(4));
            Assert.That(stats.TriangleCount, Is.EqualTo(120));
            Assert.That(stats.QueuedRebuildCount, Is.EqualTo(2));
        }

        [Test]
        public void VisualAtlasContainsDistinctTilesForEveryRenderableBlock()
        {
            BlockRegistry registry = BlockRegistry.CreateDefault();
            BlockDefinition[] renderableBlocks = registry.All
                .Where(block => block.IsRenderable)
                .ToArray();

            Rect[] tileRects = renderableBlocks
                .Select(block => BlockVisualAtlas.GetTileRect(block.Id))
                .ToArray();

            Assert.That(tileRects, Has.Length.EqualTo(renderableBlocks.Length));
            Assert.That(tileRects.Distinct().Count(), Is.EqualTo(renderableBlocks.Length));
            Assert.That(tileRects.All(rect => rect.width > 0.0f && rect.height > 0.0f), Is.True);
        }

        [Test]
        public void MeshBuilderUsesBlockSpecificAtlasUvs()
        {
            BlockRegistry registry = BlockRegistry.CreateDefault();
            var world = new VoxelWorld(new WorldBounds(8, 8, 8), chunkSize: 16, seed: 1);
            world.SetBlock(new BlockPosition(1, 1, 1), BlockRegistry.MeadowTurf, trackChange: false);
            world.SetBlock(new BlockPosition(5, 1, 1), BlockRegistry.Slate, trackChange: false);

            ChunkMeshData mesh = ChunkMeshBuilder.Build(world, registry, new ChunkCoordinate(0, 0, 0));

            Rect meadowRect = BlockVisualAtlas.GetTileRect(BlockRegistry.MeadowTurf);
            Rect slateRect = BlockVisualAtlas.GetTileRect(BlockRegistry.Slate);

            Assert.That(mesh.Uvs.Any(uv => IsInside(uv, meadowRect)), Is.True);
            Assert.That(mesh.Uvs.Any(uv => IsInside(uv, slateRect)), Is.True);
        }

        static bool IsInside(Vector2 uv, Rect rect)
        {
            return uv.x >= rect.xMin &&
                   uv.x <= rect.xMax &&
                   uv.y >= rect.yMin &&
                   uv.y <= rect.yMax;
        }
    }
}
