using Tiles.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tiles.Gameplay
{
    public sealed class TileGameSampleSceneController : MonoBehaviour
    {
        private const string TargetSceneName = "SampleScene";
        private const float Padding = 16f;
        private const float Gap = 6f;
        private const float HintDurationSeconds = 2f;
        private const int DefaultTrayCapacity = 7;

        private readonly TileGameCore _game = new TileGameCore();

        private int _currentLevelIndex;
        private int? _hintTileId;
        private float _hintExpiresAt;

        private GUIStyle _titleStyle;
        private GUIStyle _statusStyle;
        private GUIStyle _tileStyle;
        private GUIStyle _trayStyle;
        private GUIStyle _buttonStyle;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureControllerInSampleScene()
        {
            var scene = SceneManager.GetActiveScene();
            if (scene.name != TargetSceneName)
            {
                return;
            }

            if (Object.FindObjectOfType<TileGameSampleSceneController>() != null)
            {
                return;
            }

            var controllerObject = new GameObject("TileGameSampleSceneController");
            controllerObject.AddComponent<TileGameSampleSceneController>();
        }

        private void Start()
        {
            StartCurrentLevel();
        }

        private void OnGUI()
        {
            EnsureStyles();

            if (_hintTileId.HasValue && Time.unscaledTime > _hintExpiresAt)
            {
                _hintTileId = null;
            }

            var topHeight = 58f;
            var controlsHeight = 56f;
            var trayHeight = 110f;

            var topRect = new Rect(Padding, Padding, Screen.width - (Padding * 2f), topHeight);
            var controlsRect = new Rect(
                Padding,
                Screen.height - trayHeight - controlsHeight - (Padding * 2f),
                Screen.width - (Padding * 2f),
                controlsHeight);
            var trayRect = new Rect(
                Padding,
                Screen.height - trayHeight - Padding,
                Screen.width - (Padding * 2f),
                trayHeight);
            var boardRect = new Rect(
                Padding,
                topRect.yMax + Padding,
                Screen.width - (Padding * 2f),
                controlsRect.yMin - topRect.yMax - (Padding * 2f));

            DrawTop(topRect);
            DrawBoard(boardRect);
            DrawControls(controlsRect);
            DrawTray(trayRect);
            DrawOverlay();
        }

        private void DrawTop(Rect rect)
        {
            GUI.Box(rect, string.Empty);

            var levelText = "Level " + (_currentLevelIndex + 1);
            var statusText = "Status: " + _game.Status;
            var tilesLeft = CountTilesLeft();
            var infoText = "Tiles left: " + tilesLeft;

            GUI.Label(new Rect(rect.x + 10f, rect.y + 8f, rect.width * 0.4f, 24f), levelText, _titleStyle);
            GUI.Label(new Rect(rect.x + 10f, rect.y + 30f, rect.width * 0.4f, 22f), infoText, _statusStyle);
            GUI.Label(new Rect(rect.x + rect.width * 0.45f, rect.y + 16f, rect.width * 0.5f, 24f), statusText, _statusStyle);
        }

        private void DrawBoard(Rect rect)
        {
            GUI.Box(rect, string.Empty);

            var maxLayer = -1;
            var maxColumn = -1;
            var maxRow = -1;
            for (var i = 0; i < _game.Tiles.Count; i++)
            {
                var tile = _game.Tiles[i];
                if (tile.IsRemoved)
                {
                    continue;
                }

                if (tile.Layer > maxLayer)
                {
                    maxLayer = tile.Layer;
                }

                if (tile.Column > maxColumn)
                {
                    maxColumn = tile.Column;
                }

                if (tile.Row > maxRow)
                {
                    maxRow = tile.Row;
                }
            }

            if (maxLayer < 0)
            {
                GUI.Label(new Rect(rect.x + 12f, rect.y + 12f, rect.width - 24f, 24f), "Board cleared", _statusStyle);
                return;
            }

            var layersCount = maxLayer + 1;
            var columnsCount = maxColumn + 1;
            var rowsCount = maxRow + 1;

            var perLayerHeader = 18f;
            var layerGap = 10f;
            var totalHeaderHeight = layersCount * perLayerHeader;
            var totalLayerGapHeight = (layersCount - 1) * layerGap;
            var availableGridHeight = rect.height - totalHeaderHeight - totalLayerGapHeight - 10f;
            var gridHeightPerLayer = Mathf.Max(1f, availableGridHeight / layersCount);
            var tileSize = Mathf.Min(
                (rect.width - ((columnsCount - 1) * Gap) - 12f) / columnsCount,
                (gridHeightPerLayer - ((rowsCount - 1) * Gap)) / rowsCount);
            tileSize = Mathf.Max(24f, tileSize);

            for (var layerOffset = 0; layerOffset < layersCount; layerOffset++)
            {
                var layer = maxLayer - layerOffset;
                var layerTop = rect.y + 6f + layerOffset * (perLayerHeader + gridHeightPerLayer + layerGap);

                GUI.Label(
                    new Rect(rect.x + 8f, layerTop, rect.width - 16f, perLayerHeader),
                    "Layer " + (layer + 1),
                    _statusStyle);

                var gridTop = layerTop + perLayerHeader;
                for (var i = 0; i < _game.Tiles.Count; i++)
                {
                    var tile = _game.Tiles[i];
                    if (tile.IsRemoved || tile.Layer != layer)
                    {
                        continue;
                    }

                    var tileRect = new Rect(
                        rect.x + 6f + tile.Column * (tileSize + Gap),
                        gridTop + tile.Row * (tileSize + Gap),
                        tileSize,
                        tileSize);

                    var isFree = _game.IsTileFree(tile.Id);
                    var isHint = _hintTileId.HasValue && _hintTileId.Value == tile.Id;
                    var previousColor = GUI.color;
                    GUI.color = GetTileColor(tile.Type, isFree, isHint);

                    var label = GetTileShortCode(tile.Type);
                    if (isHint)
                    {
                        label = "Hint\n" + label;
                    }

                    if (isFree && _game.Status == GameStatus.Playing)
                    {
                        if (GUI.Button(tileRect, label, _tileStyle))
                        {
                            _game.TrySelectTile(tile.Id);
                            _hintTileId = null;
                        }
                    }
                    else
                    {
                        GUI.Box(tileRect, label, _tileStyle);
                    }

                    GUI.color = previousColor;
                }
            }
        }

        private void DrawControls(Rect rect)
        {
            GUI.Box(rect, string.Empty);

            var buttonWidth = (rect.width - (Gap * 2f) - 20f) / 3f;
            var buttonHeight = rect.height - 16f;
            var buttonY = rect.y + 8f;

            var undoRect = new Rect(rect.x + 8f, buttonY, buttonWidth, buttonHeight);
            var hintRect = new Rect(undoRect.xMax + Gap, buttonY, buttonWidth, buttonHeight);
            var restartRect = new Rect(hintRect.xMax + Gap, buttonY, buttonWidth, buttonHeight);

            var oldEnabled = GUI.enabled;

            GUI.enabled = _game.CanUndo && _game.Status == GameStatus.Playing;
            if (GUI.Button(undoRect, "Undo", _buttonStyle))
            {
                _game.Undo();
                _hintTileId = null;
            }

            GUI.enabled = _game.Status == GameStatus.Playing;
            if (GUI.Button(hintRect, "Hint", _buttonStyle))
            {
                var hint = _game.GetHintTileId();
                if (hint.HasValue)
                {
                    _hintTileId = hint.Value;
                    _hintExpiresAt = Time.unscaledTime + HintDurationSeconds;
                }
            }

            GUI.enabled = true;
            if (GUI.Button(restartRect, "Restart", _buttonStyle))
            {
                StartCurrentLevel();
            }

            GUI.enabled = oldEnabled;
        }

        private void DrawTray(Rect rect)
        {
            GUI.Box(rect, string.Empty);
            GUI.Label(
                new Rect(rect.x + 8f, rect.y + 6f, rect.width - 16f, 24f),
                "Tray " + _game.Tray.Count + "/" + _game.TrayCapacity,
                _statusStyle);

            var capacity = _game.TrayCapacity > 0 ? _game.TrayCapacity : DefaultTrayCapacity;
            var slotsTop = rect.y + 32f;
            var slotSize = Mathf.Min(72f, (rect.width - ((capacity - 1) * Gap) - 12f) / capacity);

            for (var i = 0; i < capacity; i++)
            {
                var slotRect = new Rect(
                    rect.x + 6f + i * (slotSize + Gap),
                    slotsTop,
                    slotSize,
                    slotSize);

                if (i < _game.Tray.Count)
                {
                    var type = _game.Tray[i];
                    var previousColor = GUI.color;
                    GUI.color = GetTileColor(type, true, false);
                    GUI.Box(slotRect, GetTileShortCode(type), _trayStyle);
                    GUI.color = previousColor;
                }
                else
                {
                    GUI.Box(slotRect, string.Empty, _trayStyle);
                }
            }
        }

        private void DrawOverlay()
        {
            if (_game.Status == GameStatus.Playing)
            {
                return;
            }

            var panelWidth = Mathf.Min(360f, Screen.width - 40f);
            var panelHeight = 170f;
            var panelRect = new Rect(
                (Screen.width - panelWidth) * 0.5f,
                (Screen.height - panelHeight) * 0.5f,
                panelWidth,
                panelHeight);

            GUI.Box(panelRect, string.Empty);

            var title = _game.Status == GameStatus.Won ? "WIN" : "LOSE";
            var action = _game.Status == GameStatus.Won ? "Next Level" : "Try Again";

            GUI.Label(new Rect(panelRect.x, panelRect.y + 20f, panelRect.width, 32f), title, _titleStyle);
            if (GUI.Button(new Rect(panelRect.x + 30f, panelRect.y + 78f, panelRect.width - 60f, 52f), action, _buttonStyle))
            {
                if (_game.Status == GameStatus.Won)
                {
                    _currentLevelIndex++;
                }

                StartCurrentLevel();
            }
        }

        private void StartCurrentLevel()
        {
            _hintTileId = null;
            _hintExpiresAt = 0f;

            var definition = BuildLevelDefinition(_currentLevelIndex);
            _game.StartLevel(definition, seed: _currentLevelIndex + 1);
        }

        private LevelDefinition BuildLevelDefinition(int levelIndex)
        {
            if (levelIndex <= 0)
            {
                return LevelDefinition.CreateFirstLevel();
            }

            var tileCount = 12 + (levelIndex * 6);
            var symbolsCount = Mathf.Min(4 + levelIndex, 10);
            var groupsCount = tileCount / 3;
            if (symbolsCount > groupsCount)
            {
                symbolsCount = groupsCount;
            }

            var layerCount = 2;
            var startingFreeTiles = tileCount / layerCount;
            return new LevelDefinition(tileCount, symbolsCount, layerCount, startingFreeTiles, DefaultTrayCapacity);
        }

        private int CountTilesLeft()
        {
            var count = 0;
            for (var i = 0; i < _game.Tiles.Count; i++)
            {
                if (!_game.Tiles[i].IsRemoved)
                {
                    count++;
                }
            }

            return count;
        }

        private static Color GetTileColor(TileType tileType, bool isFree, bool isHint)
        {
            var hue = ((int)tileType * 0.11f) % 1f;
            var color = Color.HSVToRGB(hue, 0.45f, 0.95f);

            if (!isFree)
            {
                color = Color.Lerp(color, Color.gray, 0.5f);
            }

            if (isHint)
            {
                color = Color.Lerp(color, Color.yellow, 0.45f);
            }

            return color;
        }

        private static string GetTileShortCode(TileType tileType)
        {
            switch (tileType)
            {
                case TileType.Apple:
                    return "A";
                case TileType.Ball:
                    return "B";
                case TileType.Cat:
                    return "C";
                case TileType.Diamond:
                    return "D";
                case TileType.Flower:
                    return "F";
                case TileType.Gift:
                    return "G";
                case TileType.Heart:
                    return "H";
                case TileType.IceCream:
                    return "I";
                case TileType.Jelly:
                    return "J";
                case TileType.Key:
                    return "K";
                default:
                    return "?";
            }
        }

        private void EnsureStyles()
        {
            if (_titleStyle != null)
            {
                return;
            }

            _titleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 24,
                fontStyle = FontStyle.Bold
            };

            _statusStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 16
            };

            _tileStyle = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 16,
                fontStyle = FontStyle.Bold
            };

            _trayStyle = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 15,
                fontStyle = FontStyle.Bold
            };

            _buttonStyle = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 18,
                fontStyle = FontStyle.Bold
            };
        }
    }
}
