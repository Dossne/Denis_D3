using Tiles.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tiles.Gameplay
{
    public sealed class TileGameSampleSceneController : MonoBehaviour
    {
        private const string TargetSceneName = "SampleScene";
        private const string TileTextureResourcePath = "Tiles/tile_base";
        private const float Padding = 20f;
        private const float Gap = 8f;
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
        private GUIStyle _tileOverlayStyle;
        private bool _isPortrait = true;
        private bool _styleIsPortrait = true;
        private float _uiScale = 1f;
        private float _styleScale = -1f;
        private Texture2D _tileTexture;

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
            _tileTexture = Resources.Load<Texture2D>(TileTextureResourcePath);
            StartCurrentLevel();
        }

        private void OnGUI()
        {
            _isPortrait = Screen.height >= Screen.width;
            var referenceWidth = _isPortrait ? 1080f : 1920f;
            var referenceHeight = _isPortrait ? 1920f : 1080f;
            var scaleByWidth = (float)Screen.width / referenceWidth;
            var scaleByHeight = (float)Screen.height / referenceHeight;
            _uiScale = Mathf.Clamp(Mathf.Min(scaleByWidth, scaleByHeight), 0.8f, 1.15f);
            EnsureStyles();

            if (_hintTileId.HasValue && Time.unscaledTime > _hintExpiresAt)
            {
                _hintTileId = null;
            }

            var padding = _isPortrait ? Scale(Padding) : Scale(16f);
            var topHeight = _isPortrait ? Scale(76f) : Scale(64f);
            var controlsHeight = _isPortrait ? Scale(72f) : Scale(64f);
            const float trayHeightMultiplier = 1.4f;
            var trayHeight = Scale(142f * trayHeightMultiplier);
            var traySideMargin = Scale(8f);
            var trayBottomMargin = Scale(8f);
            var controlsToTrayGap = Scale(8f);
            var minimumBoardHeight = Scale(140f);
            var minimumTrayHeight = Scale(140f);
            var minimumControlsHeight = Scale(52f);

            var topRect = new Rect(padding, padding, Screen.width - (padding * 2f), topHeight);
            var trayRect = new Rect();
            var controlsRect = new Rect();

            RecalculateBottomStack();

            var boardHeight = controlsRect.yMin - topRect.yMax - (padding * 2f);
            if (boardHeight < minimumBoardHeight)
            {
                var missingHeight = minimumBoardHeight - boardHeight;
                var trayReduce = Mathf.Min(missingHeight, Mathf.Max(0f, trayHeight - minimumTrayHeight));
                trayHeight -= trayReduce;
                missingHeight -= trayReduce;

                if (missingHeight > 0f)
                {
                    var controlsReduce = Mathf.Min(missingHeight, Mathf.Max(0f, controlsHeight - minimumControlsHeight));
                    controlsHeight -= controlsReduce;
                }

                RecalculateBottomStack();
                boardHeight = controlsRect.yMin - topRect.yMax - (padding * 2f);
            }

            var boardRect = new Rect(
                padding,
                topRect.yMax + padding,
                Screen.width - (padding * 2f),
                Mathf.Max(Scale(8f), boardHeight));

            DrawTop(topRect);
            DrawBoard(boardRect);
            DrawControls(controlsRect);
            DrawTray(trayRect);
            DrawOverlay(topRect, controlsRect);

            void RecalculateBottomStack()
            {
                trayRect = new Rect(
                    traySideMargin,
                    Screen.height - trayHeight - trayBottomMargin,
                    Screen.width - (traySideMargin * 2f),
                    trayHeight);

                controlsRect = new Rect(
                    padding,
                    trayRect.y - controlsHeight - controlsToTrayGap,
                    Screen.width - (padding * 2f),
                    controlsHeight);
            }
        }

        private void DrawTop(Rect rect)
        {
            GUI.Box(rect, string.Empty);

            var levelText = "Level " + (_currentLevelIndex + 1);
            var statusText = "Status: " + _game.Status;
            var tilesLeft = CountTilesLeft();
            var infoText = "Tiles left: " + tilesLeft;

            GUI.Label(
                new Rect(rect.x + Scale(12f), rect.y + Scale(10f), rect.width * 0.44f, Scale(34f)),
                levelText,
                _titleStyle);
            GUI.Label(
                new Rect(rect.x + Scale(12f), rect.y + Scale(44f), rect.width * 0.44f, Scale(28f)),
                infoText,
                _statusStyle);
            GUI.Label(
                new Rect(rect.x + rect.width * 0.5f, rect.y + Scale(24f), rect.width * 0.46f, Scale(30f)),
                statusText,
                _statusStyle);
        }

        private void DrawBoard(Rect rect)
        {
            GUI.Box(rect, string.Empty);
            var gap = Scale(Gap);

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

            var perLayerHeader = Scale(24f);
            var layerGap = Scale(10f);
            var totalHeaderHeight = layersCount * perLayerHeader;
            var totalLayerGapHeight = (layersCount - 1) * layerGap;
            var availableGridHeight = rect.height - totalHeaderHeight - totalLayerGapHeight - Scale(10f);
            var gridHeightPerLayer = Mathf.Max(1f, availableGridHeight / layersCount);
            var tileSize = Mathf.Min(
                (rect.width - ((columnsCount - 1) * gap) - Scale(12f)) / columnsCount,
                (gridHeightPerLayer - ((rowsCount - 1) * gap)) / rowsCount);
            tileSize = Mathf.Max(Scale(28f), tileSize);

            for (var layerOffset = 0; layerOffset < layersCount; layerOffset++)
            {
                var layer = maxLayer - layerOffset;
                var layerTop = rect.y + Scale(6f) + layerOffset * (perLayerHeader + gridHeightPerLayer + layerGap);

                GUI.Label(
                    new Rect(rect.x + Scale(8f), layerTop, rect.width - Scale(16f), perLayerHeader),
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
                        rect.x + Scale(6f) + tile.Column * (tileSize + gap),
                        gridTop + tile.Row * (tileSize + gap),
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

                    if (_tileTexture != null)
                    {
                        GUI.color = Color.white;
                        GUI.DrawTexture(tileRect, _tileTexture, ScaleMode.StretchToFill, true);
                        DrawTileStateOverlay(tileRect, isFree, isHint);
                        GUI.color = Color.white;

                        if (isFree && _game.Status == GameStatus.Playing)
                        {
                            if (GUI.Button(tileRect, label, _tileOverlayStyle))
                            {
                                _game.TrySelectTile(tile.Id);
                                _hintTileId = null;
                            }
                        }
                        else
                        {
                            GUI.Box(tileRect, label, _tileOverlayStyle);
                        }
                    }
                    else
                    {
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
                    }

                    GUI.color = previousColor;
                }
            }
        }

        private void DrawControls(Rect rect)
        {
            GUI.Box(rect, string.Empty);
            var gap = Scale(Gap);

            var buttonWidth = (rect.width - (gap * 2f) - Scale(20f)) / 3f;
            var buttonHeight = Mathf.Max(48f, rect.height - Scale(16f));
            var buttonY = rect.y + ((rect.height - buttonHeight) * 0.5f);

            var undoRect = new Rect(rect.x + Scale(8f), buttonY, buttonWidth, buttonHeight);
            var hintRect = new Rect(undoRect.xMax + gap, buttonY, buttonWidth, buttonHeight);
            var restartRect = new Rect(hintRect.xMax + gap, buttonY, buttonWidth, buttonHeight);

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
            var gap = Scale(Gap);
            GUI.Label(
                new Rect(rect.x + Scale(10f), rect.y + Scale(8f), rect.width - Scale(20f), Scale(30f)),
                "Tray " + _game.Tray.Count + "/" + _game.TrayCapacity,
                _statusStyle);

            var capacity = _game.TrayCapacity > 0 ? _game.TrayCapacity : DefaultTrayCapacity;
            var slotsTop = rect.y + Scale(42f);
            var maxSlotSizeByHeight = Mathf.Max(Scale(24f), rect.height - Scale(48f));
            var slotSize = Mathf.Min(maxSlotSizeByHeight, (rect.width - ((capacity - 1) * gap) - Scale(12f)) / capacity);

            for (var i = 0; i < capacity; i++)
            {
                var slotRect = new Rect(
                    rect.x + Scale(6f) + i * (slotSize + gap),
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

        private void DrawOverlay(Rect topRect, Rect controlsRect)
        {
            if (_game.Status == GameStatus.Playing)
            {
                return;
            }

            var safeTop = topRect.yMax + Scale(8f);
            var safeBottom = controlsRect.yMin - Scale(8f);
            var safeHeight = Mathf.Max(Scale(140f), safeBottom - safeTop);

            var panelWidth = Mathf.Min(Scale(420f), Screen.width - Scale(40f));
            var panelHeight = Mathf.Min(Scale(220f), safeHeight - Scale(8f));
            panelHeight = Mathf.Max(Scale(140f), panelHeight);
            var centerY = (safeTop + safeBottom) * 0.5f;
            var panelY = centerY - (panelHeight * 0.5f);
            panelY = Mathf.Clamp(panelY, safeTop, safeBottom - panelHeight);

            var panelRect = new Rect(
                (Screen.width - panelWidth) * 0.5f,
                panelY,
                panelWidth,
                panelHeight);

            GUI.Box(panelRect, string.Empty);

            var title = _game.Status == GameStatus.Won ? "WIN" : "LOSE";
            var action = _game.Status == GameStatus.Won ? "Next Level" : "Try Again";

            var titleRect = new Rect(panelRect.x, panelRect.y + Scale(20f), panelRect.width, Scale(44f));
            GUI.Label(
                titleRect,
                title,
                _titleStyle);

            var buttonHeight = Mathf.Max(48f, Mathf.Min(Scale(68f), panelRect.height - Scale(96f)));
            var buttonY = panelRect.yMax - buttonHeight - Scale(20f);
            var minimumButtonY = titleRect.yMax + Scale(8f);
            buttonY = Mathf.Max(buttonY, minimumButtonY);

            if (GUI.Button(
                    new Rect(
                        panelRect.x + Scale(30f),
                        buttonY,
                        panelRect.width - Scale(60f),
                        buttonHeight),
                    action,
                    _buttonStyle))
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
            if (_titleStyle != null && Mathf.Abs(_styleScale - _uiScale) < 0.01f && _styleIsPortrait == _isPortrait)
            {
                return;
            }
            _styleScale = _uiScale;
            _styleIsPortrait = _isPortrait;
            var landscapeTextFactor = _isPortrait ? 1f : 0.9f;

            _titleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = ScaleFont(30, landscapeTextFactor),
                fontStyle = FontStyle.Bold
            };

            _statusStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = ScaleFont(20, landscapeTextFactor)
            };

            _tileStyle = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = ScaleFont(20, landscapeTextFactor),
                fontStyle = FontStyle.Bold
            };

            _trayStyle = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = ScaleFont(19, landscapeTextFactor),
                fontStyle = FontStyle.Bold
            };

            _buttonStyle = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = ScaleFont(22, landscapeTextFactor),
                fontStyle = FontStyle.Bold
            };

            _tileOverlayStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = ScaleFont(20, landscapeTextFactor),
                fontStyle = FontStyle.Bold
            };
            _tileOverlayStyle.normal.textColor = new Color(0.08f, 0.08f, 0.08f, 0.95f);
            _tileOverlayStyle.active.textColor = _tileOverlayStyle.normal.textColor;
            _tileOverlayStyle.focused.textColor = _tileOverlayStyle.normal.textColor;
            _tileOverlayStyle.hover.textColor = _tileOverlayStyle.normal.textColor;
        }

        private void DrawTileStateOverlay(Rect tileRect, bool isFree, bool isHint)
        {
            if (!isFree)
            {
                GUI.color = new Color(0f, 0f, 0f, 0.28f);
                GUI.DrawTexture(tileRect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true);
            }

            if (isHint)
            {
                GUI.color = new Color(1f, 0.92f, 0.35f, 0.25f);
                GUI.DrawTexture(tileRect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true);
            }
        }

        private float Scale(float value)
        {
            return value * _uiScale;
        }

        private int ScaleFont(int fontSize, float multiplier = 1f)
        {
            return Mathf.Max(10, Mathf.RoundToInt(fontSize * _uiScale * multiplier));
        }
    }
}
