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
        private AudioClip _bgmClip;
        private AudioSource _bgmSource;
        private readonly Dictionary<TileType, Texture2D> _tileSymbols = new Dictionary<TileType, Texture2D>();
        private readonly List<TileFlightAnimation> _activeTileFlights = new List<TileFlightAnimation>();
        private readonly List<TileFlightAnimation> _completedTileFlights = new List<TileFlightAnimation>();
        private readonly HashSet<int> _pendingTileIds = new HashSet<int>();
        private readonly List<TrayMatchVfx> _activeTrayMatchVfx = new List<TrayMatchVfx>();
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
        }

        private sealed class TrayMatchVfx
        {
            public TileType tileType;
            public int[] slotIndices;
            public float startTime;
            public int seed;
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
            UpdateTrayMatchVfx();
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

            var hasTilesLeft = false;
            for (var i = 0; i < _game.Tiles.Count; i++)
            {
                var tile = _game.Tiles[i];
                if (tile.IsRemoved || _pendingTileIds.Contains(tile.Id))
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
                    if (tile.IsRemoved || _pendingTileIds.Contains(tile.Id) || tile.Layer != layer)
                    {
                        continue;
                    }

                    var tileRect = new Rect(
                        gridLeft + tile.Column * (tileSize + gap),
                        baseGridTop + tile.Row * (tileSize + gap) - (layer * layerVisualStep),
                        tileSize,
                        tileSize);

                    var isFree = IsTileFreeVisual(tile.Id);
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

                        if (isFree && _game.Status == GameStatus.Playing)
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
                        if (isFree && _game.Status == GameStatus.Playing)
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

            var buttonWidth = (rect.width - (gap * 2f) - Scale(20f)) / 3f;
            var buttonHeight = Mathf.Max(48f, rect.height - Scale(16f));
            var buttonY = rect.y + ((rect.height - buttonHeight) * 0.5f);

            var undoRect = new Rect(rect.x + Scale(8f), buttonY, buttonWidth, buttonHeight);
            var hintRect = new Rect(undoRect.xMax + gap, buttonY, buttonWidth, buttonHeight);
            var restartRect = new Rect(hintRect.xMax + gap, buttonY, buttonWidth, buttonHeight);

            var oldEnabled = GUI.enabled;
            var hasBlockingAnimation = HasActiveTileFlights() || HasActiveTrayMatchVfx();
            var canUseBoosters = _game.Status == GameStatus.Playing && !hasBlockingAnimation;

            GUI.enabled = _game.CanUndo && canUseBoosters;
            if (GUI.Button(undoRect, "Undo", _buttonStyle))
            {
                _game.Undo();
                _hintTileId = null;
            }

            GUI.enabled = canUseBoosters;
            if (GUI.Button(hintRect, "Hint", _buttonStyle))
            {
                var hint = _game.GetHintTileId();
                if (hint.HasValue)
                {
                    _hintTileId = hint.Value;
                    _hintExpiresAt = Time.unscaledTime + HintDurationSeconds;
                }
            }

            GUI.enabled = !hasBlockingAnimation;
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

            var capacity = GetTrayCapacity();
            float slotsTop;
            float slotSize;
            float slotsStartX;
            CalculateTrayLayoutMetrics(rect, capacity, gap, out slotsTop, out slotSize, out slotsStartX);

            for (var i = 0; i < capacity; i++)
            {
                var slotRect = BuildTraySlotRect(slotsStartX, slotsTop, slotSize, gap, i);

                if (i < _game.Tray.Count)
                {
                    var type = _game.Tray[i];
                    DrawTrayTileVisual(slotRect, type, 1f);
                }
                else
                {
                    GUI.Box(slotRect, string.Empty, _trayStyle);
                }
            }

            DrawTrayMatchVfx(rect);
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

        private bool HasActiveTileFlights()
        {
            return _activeTileFlights.Count > 0;
        }

        private void StartTileFlight(TileModel tile, Rect startRect, Rect trayRect)
        {
            if (tile == null || _pendingTileIds.Contains(tile.Id) || _game.Status != GameStatus.Playing)
            {
                return;
            }

            var targetRect = GetNextFlightTargetRect(trayRect);
            _pendingTileIds.Add(tile.Id);
            _activeTileFlights.Add(new TileFlightAnimation
            {
                tileId = tile.Id,
                tileType = tile.Type,
                startRect = startRect,
                targetRect = targetRect,
                startTime = Time.unscaledTime,
                sequence = _flightSequence++
            });

            _hintTileId = null;
        }

        private Rect GetNextFlightTargetRect(Rect trayRect)
        {
            var capacity = GetTrayCapacity();
            var gap = Scale(Gap);
            float slotsTop;
            float slotSize;
            float slotsStartX;
            CalculateTrayLayoutMetrics(trayRect, capacity, gap, out slotsTop, out slotSize, out slotsStartX);

            var virtualTrayCount = _game.Tray.Count + _activeTileFlights.Count;
            var targetIndex = Mathf.Clamp(virtualTrayCount, 0, capacity - 1);
            return BuildTraySlotRect(slotsStartX, slotsTop, slotSize, gap, targetIndex);
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
                var beforeTray = new List<TileType>(_game.Tray);
                if (!_game.TrySelectTile(flight.tileId))
                {
                    Debug.LogWarning(
                        "Tile flight completed but TrySelectTile failed for tileId=" + flight.tileId + ".");
                    continue;
                }

                TryStartTrayMatchVfx(beforeTray, flight.tileType);
            }
        }

        private void TryStartTrayMatchVfx(IReadOnlyList<TileType> beforeTray, TileType selectedType)
        {
            if (beforeTray == null)
            {
                return;
            }

            var beforeCount = beforeTray.Count;
            var afterCount = _game.Tray.Count;
            if (afterCount != beforeCount - 2)
            {
                return;
            }

            var trayAfterAdd = new List<TileType>(beforeCount + 1);
            for (var i = 0; i < beforeCount; i++)
            {
                trayAfterAdd.Add(beforeTray[i]);
            }
            trayAfterAdd.Add(selectedType);

            var matchedSlotIndices = new int[3];
            var foundCount = 0;
            for (var i = 0; i < trayAfterAdd.Count; i++)
            {
                if (trayAfterAdd[i] != selectedType)
                {
                    continue;
                }

                if (foundCount < matchedSlotIndices.Length)
                {
                    matchedSlotIndices[foundCount] = i;
                }

                foundCount++;
                if (foundCount == 3)
                {
                    break;
                }
            }

            if (foundCount < 3)
            {
                return;
            }

            _activeTrayMatchVfx.Add(new TrayMatchVfx
            {
                tileType = selectedType,
                slotIndices = matchedSlotIndices,
                startTime = Time.unscaledTime,
                seed = _trayMatchVfxSeed++
            });
        }

        private void UpdateTrayMatchVfx()
        {
            if (_activeTrayMatchVfx.Count == 0)
            {
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

        private bool IsTileFreeVisual(int tileId)
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
                if (candidate.Id == tileId || candidate.IsRemoved || _pendingTileIds.Contains(candidate.Id))
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
            _activeTrayMatchVfx.Clear();
            _flightSequence = 0;
            _trayMatchVfxSeed = 0;

            var definition = LoadLevelDefinition(_currentLevelIndex + 1);
            _game.StartLevel(definition, seed: _currentLevelIndex + 1);
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
