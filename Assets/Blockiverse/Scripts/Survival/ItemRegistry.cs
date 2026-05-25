using System;
using System.Collections.Generic;
using Blockiverse.Voxel;

namespace Blockiverse.Survival
{
    public sealed class ItemRegistry
    {
        readonly Dictionary<ItemId, ItemDefinition> definitionsById = new();
        readonly Dictionary<BlockId, ItemDefinition> definitionsByBlock = new();
        readonly Dictionary<string, ItemDefinition> definitionsByName = new(StringComparer.OrdinalIgnoreCase);

        public const int ResourceStackSize = 64;
        public const int ToolStackSize = 1;
        public const int RecoveryWrapStackSize = 16;

        public IReadOnlyCollection<ItemDefinition> All => definitionsById.Values;

        public static ItemRegistry CreateDefault()
        {
            var registry = new ItemRegistry();
            registry.Register(new ItemDefinition(ItemId.None, "None", ItemKind.None, maxStackSize: 0));
            registry.Register(new ItemDefinition(ItemId.MeadowTurf, "Meadow Turf", ItemKind.Resource, ResourceStackSize, BlockRegistry.MeadowTurf));
            registry.Register(new ItemDefinition(ItemId.Loam, "Loam", ItemKind.Resource, ResourceStackSize, BlockRegistry.Loam));
            registry.Register(new ItemDefinition(ItemId.Slate, "Slate", ItemKind.Resource, ResourceStackSize, BlockRegistry.Slate));
            registry.Register(new ItemDefinition(ItemId.Timber, "Timber", ItemKind.Resource, ResourceStackSize, BlockRegistry.Timber));
            registry.Register(new ItemDefinition(ItemId.Leafmass, "Leafmass", ItemKind.Resource, ResourceStackSize, BlockRegistry.Leafmass));
            registry.Register(new ItemDefinition(ItemId.Clearstone, "Clearstone", ItemKind.Resource, ResourceStackSize, BlockRegistry.Clearstone));
            registry.Register(new ItemDefinition(ItemId.Coalstone, "Coalstone", ItemKind.Resource, ResourceStackSize, BlockRegistry.Coalstone));
            registry.Register(new ItemDefinition(ItemId.Copperstone, "Copperstone", ItemKind.Resource, ResourceStackSize, BlockRegistry.Copperstone));
            registry.Register(new ItemDefinition(ItemId.Ironstone, "Ironstone", ItemKind.Resource, ResourceStackSize, BlockRegistry.Ironstone));
            registry.Register(new ItemDefinition(ItemId.Workbench, "Workbench", ItemKind.Placeable, ResourceStackSize, BlockRegistry.Workbench));
            registry.Register(new ItemDefinition(ItemId.Torchbud, "Torchbud", ItemKind.Placeable, ResourceStackSize, BlockRegistry.Torchbud));
            registry.Register(new ItemDefinition(ItemId.StorageCrate, "Storage Crate", ItemKind.Placeable, ResourceStackSize, BlockRegistry.StorageCrate));
            registry.Register(new ItemDefinition(ItemId.Chipper, "Chipper", ItemKind.Tool, ToolStackSize));
            registry.Register(new ItemDefinition(ItemId.Mallet, "Mallet", ItemKind.Tool, ToolStackSize));
            registry.Register(new ItemDefinition(ItemId.Pick, "Pick", ItemKind.Tool, ToolStackSize));
            registry.Register(new ItemDefinition(ItemId.RecoveryWrap, "Recovery Wrap", ItemKind.Consumable, RecoveryWrapStackSize));
            return registry;
        }

        public void Register(ItemDefinition definition)
        {
            if (definition == null)
                throw new ArgumentNullException(nameof(definition));

            if (definitionsById.ContainsKey(definition.Id))
                throw new InvalidOperationException($"Item ID is already registered: {definition.Id}");

            if (definitionsByName.ContainsKey(definition.Name))
                throw new InvalidOperationException($"Item name is already registered: {definition.Name}");

            if (definition.BlockId.HasValue && definitionsByBlock.ContainsKey(definition.BlockId.Value))
                throw new InvalidOperationException($"Block ID already has an item mapping: {definition.BlockId.Value}");

            definitionsById.Add(definition.Id, definition);
            definitionsByName.Add(definition.Name, definition);

            if (definition.BlockId.HasValue)
                definitionsByBlock.Add(definition.BlockId.Value, definition);
        }

        public ItemDefinition Get(ItemId id)
        {
            if (!definitionsById.TryGetValue(id, out ItemDefinition definition))
                throw new KeyNotFoundException($"Item ID is not registered: {id}");

            return definition;
        }

        public bool TryGet(ItemId id, out ItemDefinition definition)
        {
            return definitionsById.TryGetValue(id, out definition);
        }

        public bool TryGetItemForBlock(BlockId blockId, out ItemDefinition definition)
        {
            return definitionsByBlock.TryGetValue(blockId, out definition);
        }

        public ItemStack CreateDropForBlock(BlockId blockId, int count = 1)
        {
            if (count <= 0)
                throw new ArgumentOutOfRangeException(nameof(count), "Drop count must be positive.");

            return TryGetItemForBlock(blockId, out ItemDefinition definition)
                ? new ItemStack(definition.Id, count)
                : ItemStack.Empty;
        }
    }
}
