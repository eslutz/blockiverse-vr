using System;
using System.Collections.Generic;
using Blockiverse.Voxel;
using UnityEngine;

namespace Blockiverse.Gameplay
{
    public static class BlockVisualAtlas
    {
        public const int Columns = 4;
        public const int Rows = 4;
        public const int TilePixels = 16;

        const float UvInset = 0.001f;

        static readonly Dictionary<int, TilePaint> PaintByBlockId = new()
        {
            { BlockRegistry.MeadowTurf.Value, new TilePaint(0, new Color(0.27f, 0.62f, 0.34f), new Color(0.56f, 0.82f, 0.36f), BlockPattern.Mottled, 11) },
            { BlockRegistry.Loam.Value, new TilePaint(1, new Color(0.43f, 0.28f, 0.18f), new Color(0.62f, 0.42f, 0.26f), BlockPattern.Grain, 23) },
            { BlockRegistry.Slate.Value, new TilePaint(2, new Color(0.38f, 0.43f, 0.50f), new Color(0.59f, 0.64f, 0.69f), BlockPattern.Strata, 37) },
            { BlockRegistry.Timber.Value, new TilePaint(3, new Color(0.54f, 0.35f, 0.18f), new Color(0.79f, 0.55f, 0.30f), BlockPattern.Rings, 41) },
            { BlockRegistry.Leafmass.Value, new TilePaint(4, new Color(0.20f, 0.53f, 0.25f), new Color(0.42f, 0.79f, 0.34f), BlockPattern.Leaves, 53) },
            { BlockRegistry.Clearstone.Value, new TilePaint(5, new Color(0.33f, 0.74f, 0.88f), new Color(0.78f, 0.95f, 0.98f), BlockPattern.Crystal, 67) },
            { BlockRegistry.Coalstone.Value, new TilePaint(6, new Color(0.12f, 0.12f, 0.14f), new Color(0.35f, 0.35f, 0.39f), BlockPattern.Veins, 71) },
            { BlockRegistry.Copperstone.Value, new TilePaint(7, new Color(0.39f, 0.35f, 0.31f), new Color(0.91f, 0.48f, 0.24f), BlockPattern.Veins, 83) },
            { BlockRegistry.Ironstone.Value, new TilePaint(8, new Color(0.34f, 0.36f, 0.37f), new Color(0.83f, 0.78f, 0.68f), BlockPattern.Veins, 97) },
            { BlockRegistry.Workbench.Value, new TilePaint(9, new Color(0.57f, 0.39f, 0.22f), new Color(0.86f, 0.67f, 0.39f), BlockPattern.Grid, 101) },
            { BlockRegistry.Torchbud.Value, new TilePaint(10, new Color(0.25f, 0.33f, 0.20f), new Color(1.00f, 0.72f, 0.20f), BlockPattern.Glow, 109) },
            { BlockRegistry.StorageCrate.Value, new TilePaint(11, new Color(0.45f, 0.31f, 0.18f), new Color(0.76f, 0.55f, 0.29f), BlockPattern.Crate, 127) }
        };

        public static Rect GetTileRect(BlockId blockId)
        {
            TilePaint paint = GetPaint(blockId);
            int column = paint.TileIndex % Columns;
            int row = paint.TileIndex / Columns;
            float width = 1.0f / Columns;
            float height = 1.0f / Rows;

            return new Rect(
                column * width + UvInset,
                1.0f - (row + 1) * height + UvInset,
                width - UvInset * 2.0f,
                height - UvInset * 2.0f);
        }

        public static Texture2D CreateTexture()
        {
            int width = Columns * TilePixels;
            int height = Rows * TilePixels;
            Texture2D texture = new(width, height, TextureFormat.RGBA32, mipChain: false)
            {
                name = "Blockiverse Generated Block Atlas",
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };

            Fill(texture, new Color(0.36f, 0.37f, 0.39f));

            foreach (TilePaint paint in PaintByBlockId.Values)
                PaintTile(texture, paint);

            texture.Apply(updateMipmaps: false, makeNoLongerReadable: true);
            return texture;
        }

        public static Material CreateMaterial(Material sourceMaterial)
        {
            Shader shader = sourceMaterial != null
                ? sourceMaterial.shader
                : Shader.Find("Universal Render Pipeline/Lit") ??
                  Shader.Find("Standard") ??
                  Shader.Find("Sprites/Default");

            Material material = sourceMaterial != null ? new Material(sourceMaterial) : new Material(shader);
            material.name = "Blockiverse Generated Block Atlas Material";
            Texture2D texture = CreateTexture();

            if (material.HasProperty("_BaseMap"))
                material.SetTexture("_BaseMap", texture);

            if (material.HasProperty("_MainTex"))
                material.SetTexture("_MainTex", texture);

            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", Color.white);
            else if (material.HasProperty("_Color"))
                material.SetColor("_Color", Color.white);

            return material;
        }

        static TilePaint GetPaint(BlockId blockId)
        {
            if (PaintByBlockId.TryGetValue(blockId.Value, out TilePaint paint))
                return paint;

            throw new ArgumentException($"No visual atlas tile is registered for block ID {blockId}.", nameof(blockId));
        }

        static void Fill(Texture2D texture, Color color)
        {
            for (int y = 0; y < texture.height; y++)
            {
                for (int x = 0; x < texture.width; x++)
                    texture.SetPixel(x, y, color);
            }
        }

        static void PaintTile(Texture2D texture, TilePaint paint)
        {
            int tileX = paint.TileIndex % Columns;
            int tileY = paint.TileIndex / Columns;
            int originX = tileX * TilePixels;
            int originY = (Rows - 1 - tileY) * TilePixels;

            for (int y = 0; y < TilePixels; y++)
            {
                for (int x = 0; x < TilePixels; x++)
                {
                    Color color = ChooseColor(paint, x, y);
                    texture.SetPixel(originX + x, originY + y, color);
                }
            }
        }

        static Color ChooseColor(TilePaint paint, int x, int y)
        {
            int hash = Hash(x, y, paint.Seed);
            float edgeShade = x == 0 || y == 0 || x == TilePixels - 1 || y == TilePixels - 1 ? 0.82f : 1.0f;

            Color color = paint.Pattern switch
            {
                BlockPattern.Grain => x % 5 == 0 || hash % 9 == 0 ? paint.AccentColor : paint.BaseColor,
                BlockPattern.Strata => y % 4 == 0 || hash % 17 == 0 ? paint.AccentColor : paint.BaseColor,
                BlockPattern.Rings => DistanceFromCenter(x, y) % 5 <= 1 ? paint.AccentColor : paint.BaseColor,
                BlockPattern.Leaves => hash % 5 <= 1 ? paint.AccentColor : paint.BaseColor,
                BlockPattern.Crystal => x == y || x + y == TilePixels - 1 || hash % 13 == 0 ? paint.AccentColor : paint.BaseColor,
                BlockPattern.Veins => x == (hash + y) % TilePixels || hash % 19 == 0 ? paint.AccentColor : paint.BaseColor,
                BlockPattern.Grid => x % 7 == 0 || y % 7 == 0 ? paint.AccentColor : paint.BaseColor,
                BlockPattern.Glow => DistanceFromCenter(x, y) < 5 || hash % 23 == 0 ? paint.AccentColor : paint.BaseColor,
                BlockPattern.Crate => x < 2 || y < 2 || x > 13 || y > 13 || x == y || x + y == 15 ? paint.AccentColor : paint.BaseColor,
                _ => hash % 7 <= 1 ? paint.AccentColor : paint.BaseColor
            };

            return color * edgeShade;
        }

        static int DistanceFromCenter(int x, int y)
        {
            int dx = x - TilePixels / 2;
            int dy = y - TilePixels / 2;
            return Mathf.RoundToInt(Mathf.Sqrt(dx * dx + dy * dy));
        }

        static int Hash(int x, int y, int seed)
        {
            unchecked
            {
                int hash = seed;
                hash = hash * 397 ^ x;
                hash = hash * 397 ^ y;
                hash ^= hash >> 13;
                hash *= 1274126177;
                return Math.Abs(hash);
            }
        }

        readonly struct TilePaint
        {
            public TilePaint(int tileIndex, Color baseColor, Color accentColor, BlockPattern pattern, int seed)
            {
                TileIndex = tileIndex;
                BaseColor = baseColor;
                AccentColor = accentColor;
                Pattern = pattern;
                Seed = seed;
            }

            public int TileIndex { get; }
            public Color BaseColor { get; }
            public Color AccentColor { get; }
            public BlockPattern Pattern { get; }
            public int Seed { get; }
        }

        enum BlockPattern
        {
            Mottled,
            Grain,
            Strata,
            Rings,
            Leaves,
            Crystal,
            Veins,
            Grid,
            Glow,
            Crate
        }
    }
}
