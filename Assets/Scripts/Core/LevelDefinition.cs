namespace Tiles.Core
{
    public sealed class LevelDefinition
    {
        public LevelDefinition(int tileCount, int symbolCount, int layerCount, int startingFreeTiles, int trayCapacity)
            : this(tileCount, symbolCount, layerCount, startingFreeTiles, trayCapacity, null)
        {
        }

        public LevelDefinition(
            int tileCount,
            int symbolCount,
            int layerCount,
            int startingFreeTiles,
            int trayCapacity,
            int[] sectorStacks)
        {
            TileCount = tileCount;
            SymbolCount = symbolCount;
            LayerCount = layerCount;
            StartingFreeTiles = startingFreeTiles;
            TrayCapacity = trayCapacity;
            SectorStacks = sectorStacks != null ? (int[])sectorStacks.Clone() : null;
        }

        public int TileCount { get; }
        public int SymbolCount { get; }
        public int LayerCount { get; }
        public int StartingFreeTiles { get; }
        public int TrayCapacity { get; }
        public int[] SectorStacks { get; }
        public bool HasCustomSectorStacks => SectorStacks != null && SectorStacks.Length > 0;

        public static LevelDefinition CreateFirstLevel()
        {
            return new LevelDefinition(
                tileCount: 12,
                symbolCount: 4,
                layerCount: 2,
                startingFreeTiles: 6,
                trayCapacity: 7);
        }
    }

    [System.Serializable]
    public sealed class TileLevelFileData
    {
        public int symbolsCount;
        public int[] sectorStacks;
    }
}
