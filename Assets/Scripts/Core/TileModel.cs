namespace Tiles.Core
{
    public sealed class TileModel
    {
        public TileModel(int id, TileType type, int column, int row, int layer)
        {
            Id = id;
            Type = type;
            Column = column;
            Row = row;
            Layer = layer;
            IsRemoved = false;
        }

        public int Id { get; }
        public TileType Type { get; private set; }
        public int Column { get; }
        public int Row { get; }
        public int Layer { get; }
        public bool IsRemoved { get; private set; }

        internal void SetType(TileType value)
        {
            Type = value;
        }

        internal void SetRemoved(bool value)
        {
            IsRemoved = value;
        }
    }
}
