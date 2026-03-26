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
        private const string UndoButtonIconResourcePath = "UI/ControlButtons/Undo";
        private const string HintButtonIconResourcePath = "UI/ControlButtons/Hint";
        private const string RestartButtonIconResourcePath = "UI/ControlButtons/Mix";
        private const string BgmResourcePath = "Music/tiles_main_theme";
        private const string LevelsResourcePath = "Levels/";
        private const float TileIconSizeFactor = 0.58f;
        private const float Padding = 20f;
        private const float Gap = 8f;
        private const float HintDurationSeconds = 2f;
        private const float TileBounceDurationSeconds = 0.08f;
        private const float TileFlightDurationSeconds = 0.24f;
        private const float TileBounceScaleFactor = 1.1f;
        private const float TileFlightArcFactor = 0.18f;
        private const float TrayMatchFadeDurationSeconds = 0.22f;
        private const float TrayMatchSparkDurationSeconds = 0.32f;
        private const float TrayMatchScaleEnd = 0.65f;
        private const int TrayMatchSparksPerTile = 10;
        private const float TrayShiftDurationSeconds = 0.14f;
        private const float TrayCompactDurationSeconds = 0.14f;
        private const float BgmVolume = 0.45f;
        private const int DefaultTrayCapacity = 7;
        private const int MaxSymbolsOnLevel = 26;
        private const int MaxStackHeightPerSector = 9;
        private const int BoardColumns = 6;
        private const int BoardRows = 6;
        private const int MaxStacksPerLayer = BoardColumns * BoardRows;
        private static readonly Rect TileBaseCropUv = new Rect(0.15625f, 0.115234375f, 0.6875f, 0.7529296875f);

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
        private Texture2D _undoButtonTexture;
        private Texture2D _hintButtonTexture;
        private Texture2D _restartButtonTexture;
        private AudioClip _bgmClip;
        private AudioSource _bgmSource;
        private readonly Dictionary<TileType, Texture2D> _tileSymbols = new Dictionary<TileType, Texture2D>();
        private readonly List<TileFlightAnimation> _activeTileFlights = new List<TileFlightAnimation>();
        private readonly List<TileFlightAnimation> _completedTileFlights = new List<TileFlightAnimation>();
        private readonly HashSet<int> _pendingTileIds = new HashSet<int>();
        private readonly List<TileType> _projectedTray = new List<TileType>();
        private readonly List<TileType?> _visualTraySlots = new List<TileType?>();
        private readonly List<TrayShiftVfx> _activeTrayShiftVfx = new List<TrayShiftVfx>();
        private readonly List<TrayMatchVfx> _activeTrayMatchVfx = new List<TrayMatchVfx>();
        private readonly List<TrayCompactVfx> _activeTrayCompactVfx = new List<TrayCompactVfx>();
        private readonly List<TileType> _pendingCompactTargetTray = new List<TileType>();
        private bool _hasPendingCompactTargetTray;
        private int _flightSequence;
        private int _trayMatchVfxSeed;

        private sealed class TileFlightAnimation
        {
            public int tileId;
            public TileType tileType;
            public Rect startRect;
            public Rect targetRect;
            public float startTime;
            public int sequence;
            public int insertIndex;
            public int[] matchedSlotIndicesBeforeRemoval;
            public List<TileType> trayAfterInsert;
            public List<TileType> trayAfterResolve;
        }

        private sealed class TrayShiftVfx
        {
            public TileType tileType;
            public int fromIndex;
            public int toIndex;
            public float startTime;
        }

        private sealed class TrayMatchVfx
        {
            public TileType tileType;
            public int[] slotIndices;
            public float startTime;
            public int seed;
        }

        private sealed class TrayCompactVfx
        {
            public TileType tileType;
            public int fromIndex;
            public int toIndex;
            public float startTime;
        }

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
            _undoButtonTexture = Resources.Load<Texture2D>(UndoButtonIconResourcePath);
            _hintButtonTexture = Resources.Load<Texture2D>(HintButtonIconResourcePath);
            _restartButtonTexture = Resources.Load<Texture2D>(RestartButtonIconResourcePath);
            _bgmClip = Resources.Load<AudioClip>(BgmResourcePath);
            _bgmSource = GetComponent<AudioSource>();
            if (_bgmSource == null)
            {
                _bgmSource = gameObject.AddComponent<AudioSource>();
            }

            _bgmSource.playOnAwake = false;
            _bgmSource.loop = true;
            _bgmSource.spatialBlend = 0f;
            _bgmSource.volume = BgmVolume;

            if (_bgmClip == null)
            {
                Debug.LogError("BGM clip not found at Resources/" + BgmResourcePath + ".mp3");
            }
            else
            {
                _bgmSource.clip = _bgmClip;
            }

            LoadTileSymbols();
            StartCurrentLevel();
        }

        private void Update()
        {
            UpdateTileFlights();
            UpdateTrayShiftVfx();
            UpdateTrayMatchVfx();
            UpdateTrayCompactVfx();
            SyncBackgroundMusic();
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
            DrawBoard(boardRect, trayRect);
            DrawActiveTileFlights();
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

        private void SyncBackgroundMusic()
        {
            if (_bgmSource == null || _bgmClip == null)
            {
                return;
            }

            if (_game.Status == GameStatus.Playing)
            {
                if (_bgmSource.clip != _bgmClip)
                {
                    _bgmSource.clip = _bgmClip;
                }

                if (!_bgmSource.isPlaying)
                {
                    _bgmSource.Play();
                }
            }
            else if (_bgmSource.isPlaying)
            {
                _bgmSource.Stop();
            }
        }

        private void DrawBoard(Rect rect, Rect trayRect)
        {
            GUI.Box(rect, string.Empty);
            var gap = Scale(Gap);
            var now = Time.unscaledTime;
            var canSelectTiles = _game.Status == GameStatus.Playing && !HasTrayInteractionLock();

            var hasTilesLeft = false;
            for (var i = 0; i < _game.Tiles.Count; i++)
            {
                var tile = _game.Tiles[i];
                if (tile.IsRemoved || IsPendingTileVisuallyRemoved(tile.Id, now))
                {
                    continue;
                }

                hasTilesLeft = true;
                break;
            }

            if (!hasTilesLeft)
            {
                GUI.Label(new Rect(rect.x + 12f, rect.y + 12f, rect.width - 24f, 24f), "Board cleared", _statusStyle);
                return;
            }

            var columnsCount = BoardColumns;
            var rowsCount = BoardRows;
            var boardPadding = Scale(8f);
            var availableGridWidth = rect.width - (boardPadding * 2f);
            var availableGridHeight = rect.height - (boardPadding * 2f);
            var tileSize = Mathf.Min(
                (availableGridWidth - ((columnsCount - 1) * gap)) / columnsCount,
                (availableGridHeight - ((rowsCount - 1) * gap)) / rowsCount);
            tileSize = Mathf.Max(Scale(28f), tileSize);

            var gridWidth = columnsCount * tileSize + ((columnsCount - 1) * gap);
            var gridHeight = rowsCount * tileSize + ((rowsCount - 1) * gap);
            var levelLayerCount = _game.CurrentLevel != null ? _game.CurrentLevel.LayerCount : 1;
            var maxLayer = Mathf.Max(0, levelLayerCount - 1);
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
                    if (tile.IsRemoved || IsPendingTileVisuallyRemoved(tile.Id, now) || tile.Layer != layer)
                    {
                        continue;
                    }

                    var tileRect = new Rect(
                        gridLeft + tile.Column * (tileSize + gap),
                        baseGridTop + tile.Row * (tileSize + gap) - (layer * layerVisualStep),
                        tileSize,
                        tileSize);

                    var isFree = IsTileFreeVisual(tile.Id, now);
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
                        DrawCroppedTileBase(tileRect, Color.white);
                        DrawTileStateOverlay(tileRect, isFree, isHint);
                        GUI.color = Color.white;

                        if (isFree && canSelectTiles)
                        {
                            if (GUI.Button(tileRect, label, _tileOverlayStyle))
                            {
                                StartTileFlight(tile, tileRect, trayRect);
                            }
                        }
                        else
                        {
                            GUI.Box(tileRect, label, _tileOverlayStyle);
                        }
                    }
                    else
                    {
                        if (isFree && canSelectTiles)
                        {
                            if (GUI.Button(tileRect, label, _tileStyle))
                            {
                                StartTileFlight(tile, tileRect, trayRect);
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

            var maxButtonSizeByWidth = (rect.width - (gap * 2f) - Scale(16f)) / 3f;
            var maxButtonSizeByHeight = rect.height - Scale(12f);
            var buttonSize = Mathf.Min(maxButtonSizeByWidth, maxButtonSizeByHeight);
            buttonSize = Mathf.Max(48f, buttonSize);

            var buttonsTotalWidth = (buttonSize * 3f) + (gap * 2f);
            if (buttonsTotalWidth > rect.width)
            {
                buttonSize = Mathf.Max(1f, (rect.width - (gap * 2f)) / 3f);
                buttonsTotalWidth = (buttonSize * 3f) + (gap * 2f);
            }

            var buttonsStartX = rect.x + ((rect.width - buttonsTotalWidth) * 0.5f);
            var buttonY = rect.y + ((rect.height - buttonSize) * 0.5f);

            var undoRect = new Rect(buttonsStartX, buttonY, buttonSize, buttonSize);
            var hintRect = new Rect(undoRect.xMax + gap, buttonY, buttonSize, buttonSize);
            var restartRect = new Rect(hintRect.xMax + gap, buttonY, buttonSize, buttonSize);

            var oldEnabled = GUI.enabled;
            var hasBlockingAnimation = HasTrayInteractionLock();
            var canUseBoosters = _game.Status == GameStatus.Playing && !hasBlockingAnimation;

            GUI.enabled = _game.CanUndo && canUseBoosters;
            if (DrawControlButton(undoRect, _undoButtonTexture, "Undo"))
            {
                _game.Undo();
                _hintTileId = null;
                SyncProjectedTrayWithGame();
                ApplyVisualTrayFromDenseList(_game.Tray);
                _pendingCompactTargetTray.Clear();
                _hasPendingCompactTargetTray = false;
            }

            GUI.enabled = canUseBoosters;
            if (DrawControlButton(hintRect, _hintButtonTexture, "Hint"))
            {
                var hint = _game.GetHintTileId();
                if (hint.HasValue)
                {
                    _hintTileId = hint.Value;
                    _hintExpiresAt = Time.unscaledTime + HintDurationSeconds;
                }
            }

            GUI.enabled = !hasBlockingAnimation;
            if (DrawControlButton(restartRect, _restartButtonTexture, "Restart"))
            {
                StartCurrentLevel();
            }

            GUI.enabled = oldEnabled;
        }

        private bool DrawControlButton(Rect buttonRect, Texture2D iconTexture, string fallbackLabel)
        {
            if (iconTexture == null)
            {
                return GUI.Button(buttonRect, fallbackLabel, _buttonStyle);
            }

            var clicked = GUI.Button(buttonRect, GUIContent.none, GUIStyle.none);

            var previousColor = GUI.color;
            var iconAlpha = GUI.enabled ? 1f : 0.45f;
            GUI.color = new Color(1f, 1f, 1f, iconAlpha);
            GUI.DrawTexture(buttonRect, iconTexture, ScaleMode.ScaleToFit, true);
            GUI.color = previousColor;

            return clicked;
        }

        private void DrawTray(Rect rect)
        {
            GUI.Box(rect, string.Empty);
            var gap = Scale(Gap);
            GUI.Label(
                new Rect(rect.x + Scale(10f), rect.y + Scale(8f), rect.width - Scale(20f), Scale(30f)),
                "Tray " + _game.Tray.Count + "/" + _game.TrayCapacity,
                _statusStyle);

            var capacity = GetTrayCapacity();
            EnsureVisualTraySlotsCapacity();
            float slotsTop;
            float slotSize;
            float slotsStartX;
            CalculateTrayLayoutMetrics(rect, capacity, gap, out slotsTop, out slotSize, out slotsStartX);
            var now = Time.unscaledTime;

            for (var i = 0; i < capacity; i++)
            {
                var slotRect = BuildTraySlotRect(slotsStartX, slotsTop, slotSize, gap, i);
                TileType visualTileType;
                var hasVisualTile = TryGetVisualTraySlotTileType(i, out visualTileType);
                var isAnimatingOut = IsTraySlotAnimatingOut(i, now);

                if (hasVisualTile && !isAnimatingOut)
                {
                    DrawTrayTileVisual(slotRect, visualTileType, 1f);
                }
                else
                {
                    GUI.Box(slotRect, string.Empty, _trayStyle);
                }
            }

            DrawTrayShiftVfx(rect);
            DrawTrayMatchVfx(rect);
            DrawTrayCompactVfx(rect);
        }

        private int GetTrayCapacity()
        {
            var capacity = _game.TrayCapacity > 0 ? _game.TrayCapacity : DefaultTrayCapacity;
            return Mathf.Max(1, capacity);
        }

        private void CalculateTrayLayoutMetrics(
            Rect rect,
            int capacity,
            float gap,
            out float slotsTop,
            out float slotSize,
            out float slotsStartX)
        {
            slotsTop = rect.y + Scale(42f);
            var maxSlotSizeByHeight = Mathf.Max(Scale(24f), rect.height - Scale(48f));
            slotSize = Mathf.Min(maxSlotSizeByHeight, (rect.width - ((capacity - 1) * gap) - Scale(12f)) / capacity);
            slotsStartX = rect.x + Scale(6f);
        }

        private static Rect BuildTraySlotRect(float slotsStartX, float slotsTop, float slotSize, float gap, int index)
        {
            return new Rect(
                slotsStartX + index * (slotSize + gap),
                slotsTop,
                slotSize,
                slotSize);
        }

        private void DrawTrayTileVisual(Rect slotRect, TileType type, float alpha)
        {
            var clampedAlpha = Mathf.Clamp01(alpha);
            var symbolTexture = GetTileSymbolTexture(type);
            var label = symbolTexture == null ? GetTileShortCode(type) : string.Empty;
            var previousColor = GUI.color;

            if (_tileTexture != null)
            {
                DrawCroppedTileBase(slotRect, new Color(1f, 1f, 1f, clampedAlpha));
                if (!string.IsNullOrEmpty(label))
                {
                    GUI.color = new Color(1f, 1f, 1f, clampedAlpha);
                    GUI.Label(slotRect, label, _trayStyle);
                }
            }
            else
            {
                var fallbackColor = GetTileColor(type, true, false);
                fallbackColor.a = clampedAlpha;
                GUI.color = fallbackColor;
                GUI.Box(slotRect, label, _trayStyle);
            }

            if (symbolTexture != null)
            {
                GUI.color = new Color(1f, 1f, 1f, clampedAlpha);
                DrawCenteredTileSymbol(slotRect, symbolTexture);
            }

            GUI.color = previousColor;
        }

        private bool HasActiveTrayMatchVfx()
        {
            return _activeTrayMatchVfx.Count > 0;
        }

        private bool HasActiveTrayCompactVfx()
        {
            return _activeTrayCompactVfx.Count > 0;
        }

        private bool HasActiveTrayShiftVfx()
        {
            return _activeTrayShiftVfx.Count > 0;
        }

        private bool HasActiveTileFlights()
        {
            return _activeTileFlights.Count > 0;
        }

        private bool HasTrayInteractionLock()
        {
            return HasActiveTileFlights() ||
                   HasActiveTrayShiftVfx() ||
                   HasActiveTrayMatchVfx() ||
                   HasActiveTrayCompactVfx() ||
                   _pendingTileIds.Count > 0 ||
                   _hasPendingCompactTargetTray;
        }

        private void StartTileFlight(TileModel tile, Rect startRect, Rect trayRect)
        {
            if (tile == null || _pendingTileIds.Contains(tile.Id) || _game.Status != GameStatus.Playing)
            {
                return;
            }

            if (HasTrayInteractionLock())
            {
                return;
            }

            SyncProjectedTrayWithGame();

            var simulation = SimulateTrayInsert(_projectedTray, tile.Type);
            if (simulation == null)
            {
                return;
            }

            var capacity = GetTrayCapacity();
            if (simulation.insertIndex < 0 || simulation.insertIndex >= capacity)
            {
                return;
            }

            if (simulation.trayAfterResolve.Count > capacity)
            {
                return;
            }

            Rect targetRect;
            if (!TryGetTraySlotRect(trayRect, simulation.insertIndex, out targetRect))
            {
                return;
            }

            var now = Time.unscaledTime;
            var hasShift = simulation.insertIndex < _projectedTray.Count;
            if (hasShift)
            {
                for (var sourceIndex = simulation.insertIndex; sourceIndex < _projectedTray.Count; sourceIndex++)
                {
                    var destinationIndex = sourceIndex + 1;
                    if (destinationIndex >= capacity)
                    {
                        continue;
                    }

                    _activeTrayShiftVfx.Add(new TrayShiftVfx
                    {
                        tileType = _projectedTray[sourceIndex],
                        fromIndex = sourceIndex,
                        toIndex = destinationIndex,
                        startTime = now
                    });
                }
            }

            _projectedTray.Clear();
            _projectedTray.AddRange(simulation.trayAfterResolve);

            _pendingTileIds.Add(tile.Id);
            _activeTileFlights.Add(new TileFlightAnimation
            {
                tileId = tile.Id,
                tileType = tile.Type,
                startRect = startRect,
                targetRect = targetRect,
                startTime = now + (hasShift ? TrayShiftDurationSeconds : 0f),
                sequence = _flightSequence++,
                insertIndex = simulation.insertIndex,
                matchedSlotIndicesBeforeRemoval = simulation.matchedSlotIndicesBeforeRemoval,
                trayAfterInsert = simulation.trayAfterInsert,
                trayAfterResolve = simulation.trayAfterResolve
            });

            _hintTileId = null;
        }

        private bool TryGetTraySlotRect(Rect trayRect, int slotIndex, out Rect slotRect)
        {
            slotRect = new Rect();
            var capacity = GetTrayCapacity();
            if (slotIndex < 0 || slotIndex >= capacity)
            {
                return false;
            }

            var gap = Scale(Gap);
            float slotsTop;
            float slotSize;
            float slotsStartX;
            CalculateTrayLayoutMetrics(trayRect, capacity, gap, out slotsTop, out slotSize, out slotsStartX);
            slotRect = BuildTraySlotRect(slotsStartX, slotsTop, slotSize, gap, slotIndex);
            return true;
        }

        private sealed class TrayInsertSimulation
        {
            public int insertIndex;
            public int[] matchedSlotIndicesBeforeRemoval;
            public List<TileType> trayAfterInsert;
            public List<TileType> trayAfterResolve;
        }

        private static int FindTrayInsertIndex(IReadOnlyList<TileType> tray, TileType type)
        {
            for (var i = tray.Count - 1; i >= 0; i--)
            {
                if (tray[i] == type)
                {
                    return i + 1;
                }
            }

            return tray.Count;
        }

        private static int[] FindFirstAdjacentMatchIndices(IReadOnlyList<TileType> tray)
        {
            for (var i = 0; i <= tray.Count - 3; i++)
            {
                if (tray[i] == tray[i + 1] && tray[i + 1] == tray[i + 2])
                {
                    return new[] { i, i + 1, i + 2 };
                }
            }

            return null;
        }

        private static void ResolveAdjacentTrayMatches(List<TileType> tray)
        {
            for (var i = 0; i <= tray.Count - 3;)
            {
                if (tray[i] == tray[i + 1] && tray[i + 1] == tray[i + 2])
                {
                    tray.RemoveRange(i, 3);
                    if (i > 0)
                    {
                        i--;
                    }

                    continue;
                }

                i++;
            }
        }

        private static TrayInsertSimulation SimulateTrayInsert(IReadOnlyList<TileType> tray, TileType type)
        {
            var insertIndex = FindTrayInsertIndex(tray, type);
            var trayAfterInsert = new List<TileType>(tray.Count + 1);
            for (var i = 0; i < tray.Count; i++)
            {
                trayAfterInsert.Add(tray[i]);
            }

            trayAfterInsert.Insert(insertIndex, type);
            var matchedSlotIndicesBeforeRemoval = FindFirstAdjacentMatchIndices(trayAfterInsert);

            var trayAfterResolve = new List<TileType>(trayAfterInsert);
            ResolveAdjacentTrayMatches(trayAfterResolve);

            return new TrayInsertSimulation
            {
                insertIndex = insertIndex,
                matchedSlotIndicesBeforeRemoval = matchedSlotIndicesBeforeRemoval,
                trayAfterInsert = new List<TileType>(trayAfterInsert),
                trayAfterResolve = trayAfterResolve
            };
        }

        private void UpdateTileFlights()
        {
            if (_activeTileFlights.Count == 0)
            {
                return;
            }

            var now = Time.unscaledTime;
            var totalDuration = TileBounceDurationSeconds + TileFlightDurationSeconds;
            _completedTileFlights.Clear();

            for (var i = _activeTileFlights.Count - 1; i >= 0; i--)
            {
                var flight = _activeTileFlights[i];
                if (now - flight.startTime < totalDuration)
                {
                    continue;
                }

                _activeTileFlights.RemoveAt(i);
                _completedTileFlights.Add(flight);
            }

            if (_completedTileFlights.Count == 0)
            {
                return;
            }

            _completedTileFlights.Sort((left, right) => left.sequence.CompareTo(right.sequence));
            for (var i = 0; i < _completedTileFlights.Count; i++)
            {
                var flight = _completedTileFlights[i];
                _pendingTileIds.Remove(flight.tileId);
                if (!_game.TrySelectTile(flight.tileId))
                {
                    Debug.LogWarning(
                        "Tile flight completed but TrySelectTile failed for tileId=" + flight.tileId + ".");
                    SyncProjectedTrayWithGame();
                    ApplyVisualTrayFromDenseList(_game.Tray);
                    _pendingCompactTargetTray.Clear();
                    _hasPendingCompactTargetTray = false;
                    continue;
                }

                var hasMatch = flight.matchedSlotIndicesBeforeRemoval != null &&
                               flight.matchedSlotIndicesBeforeRemoval.Length == 3 &&
                               flight.trayAfterInsert != null &&
                               flight.trayAfterResolve != null;

                if (hasMatch)
                {
                    ApplyVisualTrayFromDenseList(flight.trayAfterInsert);
                    ClearVisualTraySlotsAtIndices(flight.matchedSlotIndicesBeforeRemoval);
                    StartTrayMatchVfx(flight.tileType, flight.matchedSlotIndicesBeforeRemoval);
                    QueueTrayCompactTarget(flight.trayAfterResolve);
                }
                else
                {
                    var resolvedTray = flight.trayAfterResolve != null ? flight.trayAfterResolve : new List<TileType>(_game.Tray);
                    ApplyVisualTrayFromDenseList(resolvedTray);
                    _pendingCompactTargetTray.Clear();
                    _hasPendingCompactTargetTray = false;
                }
            }

            RebuildProjectedTrayFromGameAndActiveFlights();
        }

        private void StartTrayMatchVfx(TileType selectedType, int[] slotIndices)
        {
            if (slotIndices == null || slotIndices.Length != 3)
            {
                return;
            }

            _activeTrayMatchVfx.Add(new TrayMatchVfx
            {
                tileType = selectedType,
                slotIndices = new[] { slotIndices[0], slotIndices[1], slotIndices[2] },
                startTime = Time.unscaledTime,
                seed = _trayMatchVfxSeed++
            });
        }

        private void SyncProjectedTrayWithGame()
        {
            _projectedTray.Clear();
            for (var i = 0; i < _game.Tray.Count; i++)
            {
                _projectedTray.Add(_game.Tray[i]);
            }
        }

        private void RebuildProjectedTrayFromGameAndActiveFlights()
        {
            SyncProjectedTrayWithGame();
            if (_activeTileFlights.Count == 0)
            {
                return;
            }

            var orderedFlights = new List<TileFlightAnimation>(_activeTileFlights);
            orderedFlights.Sort((left, right) => left.sequence.CompareTo(right.sequence));
            for (var i = 0; i < orderedFlights.Count; i++)
            {
                var simulation = SimulateTrayInsert(_projectedTray, orderedFlights[i].tileType);
                if (simulation == null)
                {
                    continue;
                }

                _projectedTray.Clear();
                _projectedTray.AddRange(simulation.trayAfterResolve);
            }
        }

        private void EnsureVisualTraySlotsCapacity()
        {
            var capacity = GetTrayCapacity();
            while (_visualTraySlots.Count < capacity)
            {
                _visualTraySlots.Add(null);
            }

            if (_visualTraySlots.Count > capacity)
            {
                _visualTraySlots.RemoveRange(capacity, _visualTraySlots.Count - capacity);
            }
        }

        private void ApplyVisualTrayFromDenseList(IReadOnlyList<TileType> tray)
        {
            EnsureVisualTraySlotsCapacity();
            for (var i = 0; i < _visualTraySlots.Count; i++)
            {
                _visualTraySlots[i] = null;
            }

            if (tray == null)
            {
                return;
            }

            var count = Mathf.Min(_visualTraySlots.Count, tray.Count);
            for (var i = 0; i < count; i++)
            {
                _visualTraySlots[i] = tray[i];
            }
        }

        private bool TryGetVisualTraySlotTileType(int slotIndex, out TileType tileType)
        {
            EnsureVisualTraySlotsCapacity();
            if (slotIndex < 0 || slotIndex >= _visualTraySlots.Count || !_visualTraySlots[slotIndex].HasValue)
            {
                tileType = default;
                return false;
            }

            tileType = _visualTraySlots[slotIndex].Value;
            return true;
        }

        private void SetVisualTraySlotTileType(int slotIndex, TileType? tileType)
        {
            EnsureVisualTraySlotsCapacity();
            if (slotIndex < 0 || slotIndex >= _visualTraySlots.Count)
            {
                return;
            }

            _visualTraySlots[slotIndex] = tileType;
        }

        private void ClearVisualTraySlotsAtIndices(IReadOnlyList<int> indices)
        {
            if (indices == null)
            {
                return;
            }

            for (var i = 0; i < indices.Count; i++)
            {
                SetVisualTraySlotTileType(indices[i], null);
            }
        }

        private void QueueTrayCompactTarget(IReadOnlyList<TileType> targetTray)
        {
            _pendingCompactTargetTray.Clear();
            if (targetTray != null)
            {
                for (var i = 0; i < targetTray.Count; i++)
                {
                    _pendingCompactTargetTray.Add(targetTray[i]);
                }
            }

            _hasPendingCompactTargetTray = true;
        }

        private void StartTrayCompactVfxFromVisualState()
        {
            if (!_hasPendingCompactTargetTray)
            {
                return;
            }

            EnsureVisualTraySlotsCapacity();
            _activeTrayCompactVfx.Clear();

            var sourceIndices = new List<int>();
            var sourceTypes = new List<TileType>();
            for (var i = 0; i < _visualTraySlots.Count; i++)
            {
                if (_visualTraySlots[i].HasValue)
                {
                    sourceIndices.Add(i);
                    sourceTypes.Add(_visualTraySlots[i].Value);
                }
            }

            var moveCount = Mathf.Min(sourceIndices.Count, _pendingCompactTargetTray.Count);
            var startTime = Time.unscaledTime;
            for (var i = 0; i < moveCount; i++)
            {
                var fromIndex = sourceIndices[i];
                var toIndex = i;
                var tileType = sourceTypes[i];
                if (fromIndex == toIndex)
                {
                    continue;
                }

                _activeTrayCompactVfx.Add(new TrayCompactVfx
                {
                    tileType = tileType,
                    fromIndex = fromIndex,
                    toIndex = toIndex,
                    startTime = startTime
                });
            }

            for (var i = 0; i < _activeTrayCompactVfx.Count; i++)
            {
                SetVisualTraySlotTileType(_activeTrayCompactVfx[i].fromIndex, null);
            }

            if (_activeTrayCompactVfx.Count == 0)
            {
                ApplyVisualTrayFromDenseList(_pendingCompactTargetTray);
                _pendingCompactTargetTray.Clear();
                _hasPendingCompactTargetTray = false;
            }
        }

        private void ApplyCompletedShiftToVisualTray(TrayShiftVfx shift)
        {
            EnsureVisualTraySlotsCapacity();
            TileType tileType;
            if (!TryGetVisualTraySlotTileType(shift.fromIndex, out tileType))
            {
                tileType = shift.tileType;
            }

            SetVisualTraySlotTileType(shift.toIndex, tileType);
            SetVisualTraySlotTileType(shift.fromIndex, null);
        }

        private bool IsTraySlotAnimatingOut(int slotIndex, float now)
        {
            for (var i = 0; i < _activeTrayShiftVfx.Count; i++)
            {
                var shift = _activeTrayShiftVfx[i];
                if (shift.fromIndex != slotIndex)
                {
                    continue;
                }

                var elapsed = now - shift.startTime;
                if (elapsed >= 0f && elapsed < TrayShiftDurationSeconds)
                {
                    return true;
                }
            }

            for (var i = 0; i < _activeTrayCompactVfx.Count; i++)
            {
                var compact = _activeTrayCompactVfx[i];
                if (compact.fromIndex != slotIndex)
                {
                    continue;
                }

                var elapsed = now - compact.startTime;
                if (elapsed >= 0f && elapsed < TrayCompactDurationSeconds)
                {
                    return true;
                }
            }

            return false;
        }

        private void UpdateTrayShiftVfx()
        {
            if (_activeTrayShiftVfx.Count == 0)
            {
                return;
            }

            var now = Time.unscaledTime;
            for (var i = _activeTrayShiftVfx.Count - 1; i >= 0; i--)
            {
                var shift = _activeTrayShiftVfx[i];
                var elapsed = now - shift.startTime;
                if (elapsed >= TrayShiftDurationSeconds)
                {
                    ApplyCompletedShiftToVisualTray(shift);
                    _activeTrayShiftVfx.RemoveAt(i);
                }
            }
        }

        private void DrawTrayShiftVfx(Rect trayRect)
        {
            if (_activeTrayShiftVfx.Count == 0)
            {
                return;
            }

            var capacity = GetTrayCapacity();
            var gap = Scale(Gap);
            float slotsTop;
            float slotSize;
            float slotsStartX;
            CalculateTrayLayoutMetrics(trayRect, capacity, gap, out slotsTop, out slotSize, out slotsStartX);

            var now = Time.unscaledTime;
            for (var i = 0; i < _activeTrayShiftVfx.Count; i++)
            {
                var shift = _activeTrayShiftVfx[i];
                if (shift.fromIndex < 0 || shift.fromIndex >= capacity || shift.toIndex < 0 || shift.toIndex >= capacity)
                {
                    continue;
                }

                var elapsed = now - shift.startTime;
                if (elapsed < 0f)
                {
                    continue;
                }

                var progress = TrayShiftDurationSeconds <= 0f
                    ? 1f
                    : Mathf.Clamp01(elapsed / TrayShiftDurationSeconds);
                var eased = 1f - Mathf.Pow(1f - progress, 3f);
                var alpha = 1f;

                var fromRect = BuildTraySlotRect(slotsStartX, slotsTop, slotSize, gap, shift.fromIndex);
                var toRect = BuildTraySlotRect(slotsStartX, slotsTop, slotSize, gap, shift.toIndex);
                var rect = LerpRect(fromRect, toRect, eased);
                DrawTrayTileVisual(rect, shift.tileType, alpha);
            }
        }

        private void UpdateTrayMatchVfx()
        {
            if (_activeTrayMatchVfx.Count == 0)
            {
                if (_hasPendingCompactTargetTray && !HasActiveTrayCompactVfx())
                {
                    StartTrayCompactVfxFromVisualState();
                }

                return;
            }

            var now = Time.unscaledTime;
            for (var i = _activeTrayMatchVfx.Count - 1; i >= 0; i--)
            {
                var elapsed = now - _activeTrayMatchVfx[i].startTime;
                if (elapsed >= TrayMatchSparkDurationSeconds)
                {
                    _activeTrayMatchVfx.RemoveAt(i);
                }
            }

            if (_activeTrayMatchVfx.Count == 0 && _hasPendingCompactTargetTray && !HasActiveTrayCompactVfx())
            {
                StartTrayCompactVfxFromVisualState();
            }
        }

        private void DrawTrayMatchVfx(Rect trayRect)
        {
            if (_activeTrayMatchVfx.Count == 0)
            {
                return;
            }

            var capacity = GetTrayCapacity();
            var gap = Scale(Gap);
            float slotsTop;
            float slotSize;
            float slotsStartX;
            CalculateTrayLayoutMetrics(trayRect, capacity, gap, out slotsTop, out slotSize, out slotsStartX);

            var now = Time.unscaledTime;
            for (var vfxIndex = 0; vfxIndex < _activeTrayMatchVfx.Count; vfxIndex++)
            {
                var vfx = _activeTrayMatchVfx[vfxIndex];
                var elapsed = Mathf.Max(0f, now - vfx.startTime);
                var fadeProgress = TrayMatchFadeDurationSeconds <= 0f
                    ? 1f
                    : Mathf.Clamp01(elapsed / TrayMatchFadeDurationSeconds);
                var sparkProgress = TrayMatchSparkDurationSeconds <= 0f
                    ? 1f
                    : Mathf.Clamp01(elapsed / TrayMatchSparkDurationSeconds);
                var alpha = 1f - fadeProgress;
                var tileScale = Mathf.Lerp(1f, TrayMatchScaleEnd, fadeProgress);

                for (var slotCursor = 0; slotCursor < vfx.slotIndices.Length; slotCursor++)
                {
                    var slotIndex = vfx.slotIndices[slotCursor];
                    if (slotIndex < 0 || slotIndex >= capacity)
                    {
                        continue;
                    }

                    var slotRect = BuildTraySlotRect(slotsStartX, slotsTop, slotSize, gap, slotIndex);
                    var animatedRect = ScaleRectAroundCenter(slotRect, tileScale);
                    DrawTrayTileVisual(animatedRect, vfx.tileType, alpha);
                    DrawTrayMatchSparks(slotRect, trayRect, sparkProgress, vfx.seed + (slotCursor * 97));
                }
            }
        }

        private void UpdateTrayCompactVfx()
        {
            if (_activeTrayCompactVfx.Count == 0)
            {
                if (_hasPendingCompactTargetTray)
                {
                    ApplyVisualTrayFromDenseList(_pendingCompactTargetTray);
                    _pendingCompactTargetTray.Clear();
                    _hasPendingCompactTargetTray = false;
                }

                return;
            }

            var now = Time.unscaledTime;
            for (var i = _activeTrayCompactVfx.Count - 1; i >= 0; i--)
            {
                var elapsed = now - _activeTrayCompactVfx[i].startTime;
                if (elapsed >= TrayCompactDurationSeconds)
                {
                    _activeTrayCompactVfx.RemoveAt(i);
                }
            }

            if (_activeTrayCompactVfx.Count == 0 && _hasPendingCompactTargetTray)
            {
                ApplyVisualTrayFromDenseList(_pendingCompactTargetTray);
                _pendingCompactTargetTray.Clear();
                _hasPendingCompactTargetTray = false;
            }
        }

        private void DrawTrayCompactVfx(Rect trayRect)
        {
            if (_activeTrayCompactVfx.Count == 0)
            {
                return;
            }

            var capacity = GetTrayCapacity();
            var gap = Scale(Gap);
            float slotsTop;
            float slotSize;
            float slotsStartX;
            CalculateTrayLayoutMetrics(trayRect, capacity, gap, out slotsTop, out slotSize, out slotsStartX);

            var now = Time.unscaledTime;
            for (var i = 0; i < _activeTrayCompactVfx.Count; i++)
            {
                var compact = _activeTrayCompactVfx[i];
                if (compact.fromIndex < 0 || compact.fromIndex >= capacity || compact.toIndex < 0 || compact.toIndex >= capacity)
                {
                    continue;
                }

                var elapsed = now - compact.startTime;
                if (elapsed < 0f)
                {
                    continue;
                }

                var progress = TrayCompactDurationSeconds <= 0f
                    ? 1f
                    : Mathf.Clamp01(elapsed / TrayCompactDurationSeconds);
                var eased = 1f - Mathf.Pow(1f - progress, 3f);
                var fromRect = BuildTraySlotRect(slotsStartX, slotsTop, slotSize, gap, compact.fromIndex);
                var toRect = BuildTraySlotRect(slotsStartX, slotsTop, slotSize, gap, compact.toIndex);
                var rect = LerpRect(fromRect, toRect, eased);
                DrawTrayTileVisual(rect, compact.tileType, 1f);
            }
        }

        private void DrawTrayMatchSparks(Rect sourceRect, Rect trayBoundsRect, float sparkProgress, int seed)
        {
            if (sparkProgress >= 1f)
            {
                return;
            }

            var center = sourceRect.center;
            var inverseProgress = 1f - sparkProgress;
            var previousColor = GUI.color;

            for (var i = 0; i < TrayMatchSparksPerTile; i++)
            {
                var particleSeed = seed + (i * 37);
                var angle = PseudoRandom01(particleSeed) * Mathf.PI * 2f;
                var direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                var speed = Mathf.Lerp(Scale(16f), Scale(60f), PseudoRandom01(particleSeed + 1));
                var travel = speed * sparkProgress;
                var jitterX = (PseudoRandom01(particleSeed + 2) - 0.5f) * Scale(4f) * inverseProgress;
                var jitterY = (PseudoRandom01(particleSeed + 3) - 0.5f) * Scale(4f) * inverseProgress;
                var particleCenter = new Vector2(
                    center.x + direction.x * travel + jitterX,
                    center.y + direction.y * travel + jitterY);

                var size = Mathf.Lerp(Scale(7f), Scale(1.6f), sparkProgress);
                size *= Mathf.Lerp(0.8f, 1.25f, PseudoRandom01(particleSeed + 4));
                var halfSize = size * 0.5f;

                particleCenter.x = Mathf.Clamp(particleCenter.x, trayBoundsRect.xMin + halfSize, trayBoundsRect.xMax - halfSize);
                particleCenter.y = Mathf.Clamp(particleCenter.y, trayBoundsRect.yMin + halfSize, trayBoundsRect.yMax - halfSize);

                var particleRect = new Rect(
                    particleCenter.x - halfSize,
                    particleCenter.y - halfSize,
                    size,
                    size);

                var warmMix = PseudoRandom01(particleSeed + 5);
                var color = Color.Lerp(new Color(1f, 0.92f, 0.45f, 1f), Color.white, warmMix);
                color.a = Mathf.Clamp01(inverseProgress * (0.45f + 0.45f * PseudoRandom01(particleSeed + 6)));

                GUI.color = color;
                GUI.DrawTexture(particleRect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true);
            }

            GUI.color = previousColor;
        }

        private static float PseudoRandom01(int seed)
        {
            var value = Mathf.Sin(seed * 12.9898f + 78.233f) * 43758.5453f;
            return value - Mathf.Floor(value);
        }

        private bool TryGetActiveTileFlight(int tileId, out TileFlightAnimation flight)
        {
            for (var i = 0; i < _activeTileFlights.Count; i++)
            {
                if (_activeTileFlights[i].tileId == tileId)
                {
                    flight = _activeTileFlights[i];
                    return true;
                }
            }

            flight = null;
            return false;
        }

        private bool IsPendingTileVisuallyRemoved(int tileId, float now)
        {
            if (!_pendingTileIds.Contains(tileId))
            {
                return false;
            }

            TileFlightAnimation flight;
            if (!TryGetActiveTileFlight(tileId, out flight))
            {
                return true;
            }

            return now >= flight.startTime;
        }

        private bool IsTileFreeVisual(int tileId, float now)
        {
            if (_pendingTileIds.Contains(tileId))
            {
                return false;
            }

            TileModel tile = null;
            for (var i = 0; i < _game.Tiles.Count; i++)
            {
                if (_game.Tiles[i].Id == tileId)
                {
                    tile = _game.Tiles[i];
                    break;
                }
            }

            if (tile == null || tile.IsRemoved)
            {
                return false;
            }

            for (var i = 0; i < _game.Tiles.Count; i++)
            {
                var candidate = _game.Tiles[i];
                if (candidate.Id == tileId || candidate.IsRemoved || IsPendingTileVisuallyRemoved(candidate.Id, now))
                {
                    continue;
                }

                if (candidate.Column == tile.Column && candidate.Row == tile.Row && candidate.Layer > tile.Layer)
                {
                    return false;
                }
            }

            return true;
        }

        private void DrawActiveTileFlights()
        {
            if (_activeTileFlights.Count == 0)
            {
                return;
            }

            var now = Time.unscaledTime;
            for (var i = 0; i < _activeTileFlights.Count; i++)
            {
                var flight = _activeTileFlights[i];
                if (now < flight.startTime)
                {
                    continue;
                }

                var flightRect = EvaluateFlightRect(flight, now);
                DrawFlightTileVisual(flightRect, flight.tileType);
            }
        }

        private Rect EvaluateFlightRect(TileFlightAnimation flight, float now)
        {
            var elapsed = Mathf.Max(0f, now - flight.startTime);
            if (elapsed <= TileBounceDurationSeconds)
            {
                var bounceProgress = TileBounceDurationSeconds <= 0f
                    ? 1f
                    : Mathf.Clamp01(elapsed / TileBounceDurationSeconds);
                var bounceScale = Mathf.Lerp(1f, TileBounceScaleFactor, Mathf.Sin(bounceProgress * Mathf.PI));
                return ScaleRectAroundCenter(flight.startRect, bounceScale);
            }

            var flightProgress = TileFlightDurationSeconds <= 0f
                ? 1f
                : Mathf.Clamp01((elapsed - TileBounceDurationSeconds) / TileFlightDurationSeconds);
            var eased = 1f - Mathf.Pow(1f - flightProgress, 3f);

            var startCenter = new Vector2(flight.startRect.center.x, flight.startRect.center.y);
            var targetCenter = new Vector2(flight.targetRect.center.x, flight.targetRect.center.y);
            var center = Vector2.Lerp(startCenter, targetCenter, eased);
            var distance = Vector2.Distance(startCenter, targetCenter);
            var arcHeight = Mathf.Clamp(distance * TileFlightArcFactor, Scale(18f), Scale(110f));
            center.y -= Mathf.Sin(flightProgress * Mathf.PI) * arcHeight;

            var width = Mathf.Lerp(flight.startRect.width, flight.targetRect.width, eased);
            var height = Mathf.Lerp(flight.startRect.height, flight.targetRect.height, eased);
            return new Rect(center.x - (width * 0.5f), center.y - (height * 0.5f), width, height);
        }

        private void DrawFlightTileVisual(Rect tileRect, TileType tileType)
        {
            var symbolTexture = GetTileSymbolTexture(tileType);
            var label = symbolTexture == null ? GetTileShortCode(tileType) : string.Empty;

            if (_tileTexture != null)
            {
                DrawCroppedTileBase(tileRect, Color.white);
                if (!string.IsNullOrEmpty(label))
                {
                    GUI.Label(tileRect, label, _tileOverlayStyle);
                }
            }
            else
            {
                var previousColor = GUI.color;
                GUI.color = GetTileColor(tileType, true, false);
                GUI.Box(tileRect, label, _tileStyle);
                GUI.color = previousColor;
            }

            if (symbolTexture != null)
            {
                DrawCenteredTileSymbol(tileRect, symbolTexture);
            }
        }

        private static Rect ScaleRectAroundCenter(Rect rect, float scale)
        {
            var width = rect.width * scale;
            var height = rect.height * scale;
            return new Rect(
                rect.center.x - (width * 0.5f),
                rect.center.y - (height * 0.5f),
                width,
                height);
        }

        private static Rect LerpRect(Rect fromRect, Rect toRect, float t)
        {
            return new Rect(
                Mathf.Lerp(fromRect.x, toRect.x, t),
                Mathf.Lerp(fromRect.y, toRect.y, t),
                Mathf.Lerp(fromRect.width, toRect.width, t),
                Mathf.Lerp(fromRect.height, toRect.height, t));
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
            _activeTileFlights.Clear();
            _completedTileFlights.Clear();
            _pendingTileIds.Clear();
            _projectedTray.Clear();
            _activeTrayShiftVfx.Clear();
            _activeTrayMatchVfx.Clear();
            _activeTrayCompactVfx.Clear();
            _pendingCompactTargetTray.Clear();
            _hasPendingCompactTargetTray = false;
            _flightSequence = 0;
            _trayMatchVfxSeed = 0;

            var definition = LoadLevelDefinition(_currentLevelIndex + 1);
            _game.StartLevel(definition, seed: _currentLevelIndex + 1);
            SyncProjectedTrayWithGame();
            ApplyVisualTrayFromDenseList(_game.Tray);
        }

        private LevelDefinition LoadLevelDefinition(int levelNumber)
        {
            var levelId = levelNumber.ToString("000");
            var levelAsset = Resources.Load<TextAsset>(LevelsResourcePath + levelId);
            if (levelAsset == null)
            {
                throw new System.InvalidOperationException(
                    "Level file not found: Assets/Resources/Levels/" + levelId + ".json");
            }

            var levelData = JsonUtility.FromJson<TileLevelFileData>(levelAsset.text);
            if (levelData == null)
            {
                throw new System.InvalidOperationException("Failed to parse level JSON for ID " + levelId + ".");
            }

            if (levelData.sectorStacks == null || levelData.sectorStacks.Length != MaxStacksPerLayer)
            {
                throw new System.InvalidOperationException(
                    "Level " + levelId + " must contain exactly 36 values in sectorStacks.");
            }

            var tileCount = 0;
            var layerCount = 0;
            var startingFreeTiles = 0;
            for (var i = 0; i < levelData.sectorStacks.Length; i++)
            {
                var stackHeight = levelData.sectorStacks[i];
                if (stackHeight < 0 || stackHeight > MaxStackHeightPerSector)
                {
                    throw new System.InvalidOperationException(
                        "Level " + levelId + " contains invalid stack height " + stackHeight + ". Allowed: 0..9.");
                }

                tileCount += stackHeight;
                if (stackHeight > layerCount)
                {
                    layerCount = stackHeight;
                }

                if (stackHeight > 0)
                {
                    startingFreeTiles++;
                }
            }

            if (tileCount <= 0)
            {
                throw new System.InvalidOperationException("Level " + levelId + " must contain at least one tile.");
            }

            if (tileCount % 3 != 0)
            {
                throw new System.InvalidOperationException("Level " + levelId + " tile count must be divisible by 3.");
            }

            var symbolsCount = levelData.symbolsCount;
            if (symbolsCount < 1 || symbolsCount > MaxSymbolsOnLevel)
            {
                throw new System.InvalidOperationException(
                    "Level " + levelId + " symbolsCount must be between 1 and " + MaxSymbolsOnLevel + ".");
            }

            var groupsCount = tileCount / 3;
            if (symbolsCount > groupsCount)
            {
                throw new System.InvalidOperationException(
                    "Level " + levelId + " symbolsCount cannot exceed tileCount/3.");
            }

            return new LevelDefinition(
                tileCount,
                symbolsCount,
                layerCount,
                startingFreeTiles,
                DefaultTrayCapacity,
                levelData.sectorStacks);
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

        private void DrawCroppedTileBase(Rect tileRect, Color tint)
        {
            if (_tileTexture == null)
            {
                return;
            }

            var previousColor = GUI.color;
            GUI.color = tint;
            GUI.DrawTextureWithTexCoords(tileRect, _tileTexture, TileBaseCropUv, true);
            GUI.color = previousColor;
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
            var previousColor = GUI.color;

            if (!isFree)
            {
                if (_tileTexture != null)
                {
                    DrawCroppedTileBase(tileRect, new Color(0f, 0f, 0f, 0.28f));
                }
                else
                {
                    GUI.color = new Color(0f, 0f, 0f, 0.28f);
                    GUI.DrawTexture(tileRect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true);
                }
            }

            if (isHint)
            {
                if (_tileTexture != null)
                {
                    DrawCroppedTileBase(tileRect, new Color(1f, 0.92f, 0.35f, 0.25f));
                }
                else
                {
                    GUI.color = new Color(1f, 0.92f, 0.35f, 0.25f);
                    GUI.DrawTexture(tileRect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true);
                }
            }

            GUI.color = previousColor;
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
