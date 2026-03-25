using System.Collections.Generic;
using Tiles.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tiles.Gameplay
{
    public sealed class TileGameSampleSceneController : MonoBehaviour
    {
        private const string TargetSceneName = "SampleScene";
        private const string TileTextureResourcePath = "Tiles/tile_base";
        private const string TileSymbolsResourcePath = "TileSymbols";
        private const float TileIconSizeFactor = 0.58f;
        private const float Padding = 20f;
        private const float Gap = 8f;
        private const float HintDurationSeconds = 2f;
        private const int DefaultTrayCapacity = 7;

        private static readonly Dictionary<TileType, string> TileSymbolFileByType = new Dictionary<TileType, string>
        {
            { TileType.A, "itemicon_s_arrow" },
            { TileType.B, "itemicon_s_bomb" },
            { TileType.C, "itemicon_s_chest" },
            { TileType.D, "itemicon_s_energy" },
            { TileType.E, "itemicon_s_gem_1" },
            { TileType.F, "itemicon_s_gem_2" },
            { TileType.G, "itemicon_s_gem_3" },
            { TileType.H, "itemicon_s_gem_4" },
            { TileType.I, "itemicon_s_gift" },
            { TileType.J, "itemicon_s_gold_1" },
            { TileType.K, "itemicon_s_gold_2" },
            { TileType.L, "itemicon_s_gold_3" },
            { TileType.M, "itemicon_s_gold_4" },
            { TileType.N, "itemicon_s_hammer" },
            { TileType.O, "itemicon_s_key" },
            { TileType.P, "itemicon_s_life" },
            { TileType.Q, "itemicon_s_lock" },
            { TileType.R, "itemicon_s_medal" },
            { TileType.S, "itemicon_s_mission" },
            { TileType.T, "itemicon_s_postbox" },
            { TileType.U, "itemicon_s_potion" },
            { TileType.V, "itemicon_s_save_money" },
            { TileType.W, "itemicon_s_setting" },
            { TileType.X, "itemicon_s_shield" },
            { TileType.Y, "itemicon_s_time" },
            { TileType.Z, "itemicon_s_trophy" }
        };

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
        private readonly Dictionary<TileType, Texture2D> _tileSymbols = new Dictionary<TileType, Texture2D>();

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
            LoadTileSymbols();
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

            var columnsCount = maxColumn + 1;
            var rowsCount = maxRow + 1;
            var boardPadding = Scale(8f);
            var availableGridWidth = rect.width - (boardPadding * 2f);
            var availableGridHeight = rect.height - (boardPadding * 2f);
            var tileSize = Mathf.Min(
                (availableGridWidth - ((columnsCount - 1) * gap)) / columnsCount,
                (availableGridHeight - ((rowsCount - 1) * gap)) / rowsCount);
            tileSize = Mathf.Max(Scale(28f), tileSize);

            var gridWidth = columnsCount * tileSize + ((columnsCount - 1) * gap);
            var gridHeight = rowsCount * tileSize + ((rowsCount - 1) * gap);
            var layerVisualStep = Mathf.Clamp(Scale(6f), Scale(3f), tileSize * 0.14f);
            var stackOffsetTotal = maxLayer * layerVisualStep;
            var stackHeight = gridHeight + stackOffsetTotal;
            var gridLeft = rect.x + ((rect.width - gridWidth) * 0.5f);
            var stackTop = rect.y + ((rect.height - stackHeight) * 0.5f);
            var baseGridTop = stackTop + stackOffsetTotal;

            for (var layer = 0; layer <= maxLayer; layer++)
            {
                for (var i = 0; i < _game.Tiles.Count; i++)
                {
                    var tile = _game.Tiles[i];
                    if (tile.IsRemoved || tile.Layer != layer)
                    {
                        continue;
                    }

                    var tileRect = new Rect(
                        gridLeft + tile.Column * (tileSize + gap),
                        baseGridTop + tile.Row * (tileSize + gap) - (layer * layerVisualStep),
                        tileSize,
                        tileSize);

                    var isFree = _game.IsTileFree(tile.Id);
                    var isHint = _hintTileId.HasValue && _hintTileId.Value == tile.Id;
                    var previousColor = GUI.color;
                    GUI.color = GetTileColor(tile.Type, isFree, isHint);
                    var symbolTexture = GetTileSymbolTexture(tile.Type);
                    var label = symbolTexture == null ? GetTileShortCode(tile.Type) : string.Empty;
                    if (isHint && symbolTexture == null)
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

                    if (symbolTexture != null)
                    {
                        DrawCenteredTileSymbol(tileRect, symbolTexture);
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
                    var symbolTexture = GetTileSymbolTexture(type);
                    var label = symbolTexture == null ? GetTileShortCode(type) : string.Empty;
                    var previousColor = GUI.color;
                    GUI.color = GetTileColor(type, true, false);
                    GUI.Box(slotRect, label, _trayStyle);
                    if (symbolTexture != null)
                    {
                        DrawCenteredTileSymbol(slotRect, symbolTexture);
                    }
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
            var symbolsCount = Mathf.Min(4 + levelIndex, 26);
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

        private void LoadTileSymbols()
        {
            _tileSymbols.Clear();
            foreach (var pair in TileSymbolFileByType)
            {
                var texture = Resources.Load<Texture2D>(TileSymbolsResourcePath + "/" + pair.Value);
                if (texture != null)
                {
                    _tileSymbols[pair.Key] = texture;
                }
            }
        }

        private Texture2D GetTileSymbolTexture(TileType tileType)
        {
            Texture2D texture;
            return _tileSymbols.TryGetValue(tileType, out texture) ? texture : null;
        }

        private void DrawCenteredTileSymbol(Rect hostRect, Texture2D symbolTexture)
        {
            if (symbolTexture == null)
            {
                return;
            }

            var iconSize = Mathf.Min(hostRect.width, hostRect.height) * TileIconSizeFactor;
            if (iconSize <= 0f)
            {
                return;
            }

            var iconRect = new Rect(
                hostRect.x + ((hostRect.width - iconSize) * 0.5f),
                hostRect.y + ((hostRect.height - iconSize) * 0.5f),
                iconSize,
                iconSize);
            GUI.DrawTexture(iconRect, symbolTexture, ScaleMode.ScaleToFit, true);
        }

        private static string GetTileShortCode(TileType tileType)
        {
            return tileType.ToString();
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
