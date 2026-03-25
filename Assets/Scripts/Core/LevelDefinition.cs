namespace Tiles.Core
{
    public sealed class LevelDefinition
    {
        public LevelDefinition(int tileCount, int symbolCount, int layerCount, int startingFreeTiles, int trayCapacity)
        {
            TileCount = tileCount;
            SymbolCount = symbolCount;
            LayerCount = layerCount;
            StartingFreeTiles = startingFreeTiles;
            TrayCapacity = trayCapacity;
        }

        public int TileCount { get; }
        public int SymbolCount { get; }
        public int LayerCount { get; }
        public int StartingFreeTiles { get; }
        public int TrayCapacity { get; }

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
}
