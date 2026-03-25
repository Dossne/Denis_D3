using System;
using System.Collections.Generic;

namespace Tiles.Core
{
    public enum GameStatus
    {
        Playing = 0,
        Won = 1,
        Lost = 2
    }

    public sealed class TileGameCore
    {
        private const int MatchSize = 3;
        private const int BoardColumns = 6;
        private const int BoardRows = 6;
        private const int MaxStacksPerLayer = BoardColumns * BoardRows;

        private readonly List<TileModel> _tiles = new List<TileModel>();
        private readonly List<TileType> _tray = new List<TileType>();
        private readonly Stack<GameSnapshot> _history = new Stack<GameSnapshot>();

        private int _trayCapacity = 7;

        public IReadOnlyList<TileModel> Tiles => _tiles;
        public IReadOnlyList<TileType> Tray => _tray;
        public int TrayCapacity => _trayCapacity;
        public GameStatus Status { get; private set; } = GameStatus.Playing;
        public LevelDefinition CurrentLevel { get; private set; }
        public bool CanUndo => _history.Count > 0;

        public void StartLevel(LevelDefinition definition, int seed = 0)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            ValidateDefinition(definition);

            CurrentLevel = definition;
            _trayCapacity = definition.TrayCapacity;
            _history.Clear();
            _tray.Clear();
            _tiles.Clear();

            var random = new Random(seed);
            var tileTypes = BuildTileTypeList(definition, random);
            var slotsPerLayer = definition.TileCount / definition.LayerCount;
            var coordinates = BuildCoordinates(slotsPerLayer);

            var tileId = 0;
            var typeIndex = 0;
            for (var layer = 0; layer < definition.LayerCount; layer++)
            {
                for (var i = 0; i < coordinates.Count; i++)
                {
                    var coordinate = coordinates[i];
                    _tiles.Add(new TileModel(
                        id: tileId,
                        type: tileTypes[typeIndex],
                        column: coordinate.Column,
                        row: coordinate.Row,
                        layer: layer));

                    tileId++;
                    typeIndex++;
                }
            }

            var freeTilesAtStart = CountFreeTiles();
            if (definition.StartingFreeTiles > 0 && freeTilesAtStart != definition.StartingFreeTiles)
            {
                throw new InvalidOperationException(
                    "Generated free tiles count does not match LevelDefinition.StartingFreeTiles.");
            }

            Status = GameStatus.Playing;
            UpdateStatus();
        }

        public bool TrySelectTile(int tileId)
        {
            if (Status != GameStatus.Playing)
            {
                return false;
            }

            var tile = FindTile(tileId);
            if (tile == null || tile.IsRemoved || !IsTileFree(tileId))
            {
                return false;
            }

            SaveSnapshot();

            tile.SetRemoved(true);
            _tray.Add(tile.Type);
            ResolveTrayMatches();
            UpdateStatus();

            return true;
        }

        public bool Undo()
        {
            if (_history.Count == 0)
            {
                return false;
            }

            var snapshot = _history.Pop();
            Restore(snapshot);
            return true;
        }

        public int? GetHintTileId()
        {
            if (Status != GameStatus.Playing)
            {
                return null;
            }

            var freeTiles = new List<TileModel>();
            for (var i = 0; i < _tiles.Count; i++)
            {
                var tile = _tiles[i];
                if (!tile.IsRemoved && IsTileFree(tile.Id))
                {
                    freeTiles.Add(tile);
                }
            }

            if (freeTiles.Count == 0)
            {
                return null;
            }

            var trayTypeCounts = new Dictionary<TileType, int>();
            for (var i = 0; i < _tray.Count; i++)
            {
                var tileType = _tray[i];
                if (!trayTypeCounts.ContainsKey(tileType))
                {
                    trayTypeCounts[tileType] = 0;
                }

                trayTypeCounts[tileType]++;
            }

            TileModel bestCandidate = null;
            var bestScore = -1;
            for (var i = 0; i < freeTiles.Count; i++)
            {
                var tile = freeTiles[i];
                var score = trayTypeCounts.ContainsKey(tile.Type) ? trayTypeCounts[tile.Type] : 0;
                if (score > bestScore)
                {
                    bestScore = score;
                    bestCandidate = tile;
                }
            }

            return bestCandidate != null ? bestCandidate.Id : freeTiles[0].Id;
        }

        public bool IsTileFree(int tileId)
        {
            var tile = FindTile(tileId);
            if (tile == null || tile.IsRemoved)
            {
                return false;
            }

            for (var i = 0; i < _tiles.Count; i++)
            {
                var other = _tiles[i];
                if (other.IsRemoved)
                {
                    continue;
                }

                var blocksFromAbove = other.Column == tile.Column
                    && other.Row == tile.Row
                    && other.Layer > tile.Layer;
                if (blocksFromAbove)
                {
                    return false;
                }
            }

            return true;
        }

        private void SaveSnapshot()
        {
            var removed = new bool[_tiles.Count];
            for (var i = 0; i < _tiles.Count; i++)
            {
                removed[i] = _tiles[i].IsRemoved;
            }

            _history.Push(new GameSnapshot(removed, new List<TileType>(_tray), Status));
        }

        private void Restore(GameSnapshot snapshot)
        {
            for (var i = 0; i < _tiles.Count; i++)
            {
                _tiles[i].SetRemoved(snapshot.Removed[i]);
            }

            _tray.Clear();
            _tray.AddRange(snapshot.Tray);
            Status = snapshot.Status;
            UpdateStatus();
        }

        private void ResolveTrayMatches()
        {
            var occurrences = new Dictionary<TileType, int>();
            for (var i = 0; i < _tray.Count; i++)
            {
                var tileType = _tray[i];
                if (!occurrences.ContainsKey(tileType))
                {
                    occurrences[tileType] = 0;
                }

                occurrences[tileType]++;
            }

            foreach (var pair in occurrences)
            {
                var matchesToRemove = pair.Value / MatchSize;
                for (var matchIndex = 0; matchIndex < matchesToRemove; matchIndex++)
                {
                    RemoveFirstInTray(pair.Key);
                    RemoveFirstInTray(pair.Key);
                    RemoveFirstInTray(pair.Key);
                }
            }
        }

        private void RemoveFirstInTray(TileType type)
        {
            for (var i = 0; i < _tray.Count; i++)
            {
                if (_tray[i] == type)
                {
                    _tray.RemoveAt(i);
                    return;
                }
            }
        }

        private void UpdateStatus()
        {
            var allTilesRemoved = true;
            for (var i = 0; i < _tiles.Count; i++)
            {
                if (!_tiles[i].IsRemoved)
                {
                    allTilesRemoved = false;
                    break;
                }
            }

            if (allTilesRemoved && _tray.Count == 0)
            {
                Status = GameStatus.Won;
                return;
            }

            if (_tray.Count >= _trayCapacity)
            {
                Status = GameStatus.Lost;
                return;
            }

            Status = GameStatus.Playing;
        }

        private TileModel FindTile(int tileId)
        {
            for (var i = 0; i < _tiles.Count; i++)
            {
                if (_tiles[i].Id == tileId)
                {
                    return _tiles[i];
                }
            }

            return null;
        }

        private int CountFreeTiles()
        {
            var count = 0;
            for (var i = 0; i < _tiles.Count; i++)
            {
                var tile = _tiles[i];
                if (!tile.IsRemoved && IsTileFree(tile.Id))
                {
                    count++;
                }
            }

            return count;
        }

        private static List<TileType> BuildTileTypeList(LevelDefinition definition, Random random)
        {
            var groupsCount = definition.TileCount / MatchSize;
            if (groupsCount < definition.SymbolCount)
            {
                throw new InvalidOperationException("SymbolCount cannot exceed TileCount / 3 for MVP generation.");
            }

            var allTileTypes = (TileType[])Enum.GetValues(typeof(TileType));
            if (definition.SymbolCount > allTileTypes.Length)
            {
                throw new InvalidOperationException("SymbolCount is larger than available TileType values.");
            }

            var usedTypes = new TileType[definition.SymbolCount];
            for (var i = 0; i < definition.SymbolCount; i++)
            {
                usedTypes[i] = allTileTypes[i];
            }

            var tiles = new List<TileType>(definition.TileCount);
            for (var group = 0; group < groupsCount; group++)
            {
                var type = usedTypes[group % usedTypes.Length];
                tiles.Add(type);
                tiles.Add(type);
                tiles.Add(type);
            }

            Shuffle(tiles, random);
            return tiles;
        }

        private static List<BoardCoordinate> BuildCoordinates(int slotsCount)
        {
            if (slotsCount > MaxStacksPerLayer)
            {
                throw new InvalidOperationException("Stacks per layer cannot exceed 36 for a 6x6 board.");
            }

            var coordinates = new List<BoardCoordinate>(slotsCount);

            for (var i = 0; i < slotsCount; i++)
            {
                var column = i % BoardColumns;
                var row = i / BoardColumns;
                coordinates.Add(new BoardCoordinate(column, row));
            }

            return coordinates;
        }

        private static void Shuffle<T>(List<T> list, Random random)
        {
            for (var i = list.Count - 1; i > 0; i--)
            {
                var swapIndex = random.Next(i + 1);
                var temp = list[i];
                list[i] = list[swapIndex];
                list[swapIndex] = temp;
            }
        }

        private static void ValidateDefinition(LevelDefinition definition)
        {
            if (definition.TileCount <= 0)
            {
                throw new InvalidOperationException("TileCount must be greater than 0.");
            }

            if (definition.TileCount % MatchSize != 0)
            {
                throw new InvalidOperationException("TileCount must be divisible by 3.");
            }

            if (definition.LayerCount <= 0)
            {
                throw new InvalidOperationException("LayerCount must be greater than 0.");
            }

            if (definition.TileCount % definition.LayerCount != 0)
            {
                throw new InvalidOperationException("TileCount must be divisible by LayerCount.");
            }

            var stacksPerLayer = definition.TileCount / definition.LayerCount;
            if (stacksPerLayer > MaxStacksPerLayer)
            {
                throw new InvalidOperationException("TileCount / LayerCount cannot exceed 36 stacks for the 6x6 board.");
            }

            if (definition.SymbolCount <= 0)
            {
                throw new InvalidOperationException("SymbolCount must be greater than 0.");
            }

            if (definition.TrayCapacity < MatchSize)
            {
                throw new InvalidOperationException("TrayCapacity must be at least 3.");
            }
        }

        private readonly struct BoardCoordinate
        {
            public BoardCoordinate(int column, int row)
            {
                Column = column;
                Row = row;
            }

            public int Column { get; }
            public int Row { get; }
        }

        private sealed class GameSnapshot
        {
            public GameSnapshot(bool[] removed, List<TileType> tray, GameStatus status)
            {
                Removed = removed;
                Tray = tray;
                Status = status;
            }

            public bool[] Removed { get; }
            public List<TileType> Tray { get; }
            public GameStatus Status { get; }
        }
    }
}
