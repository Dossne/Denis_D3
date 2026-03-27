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
        private const string LevelBackgroundResourcePath = "UI/Backgrounds/level_background";
        private const string StartScreenBackgroundResourcePath = "UI/StartScreen/Meta_screen";
        private const string StartScreenActionTextResourcePath = "UI/StartScreen/Actiontext_Start";
        private const string StartScreenScientistWonderResourcePath = "UI/StartScreen/Scientist_wonder";
        private const string StartScreenScientistHappyResourcePath = "UI/StartScreen/Scientist_happy";
        private const string StartScreenScientistSupportResourcePath = "UI/StartScreen/Scientist_support";
        private const string StartScreenScientistDissappointment2ResourcePath = "UI/StartScreen/Scientist_dissappointment_2";
        private const string StartScreenDialogueWindowResourcePath = "UI/StartScreen/dialogue_window";
        private const string MetaScreenStage01BackgroundResourcePath = "UI/MetaStages/Stage_01";
        private const string MetaScreenStage02BackgroundResourcePath = "UI/MetaStages/Stage_02";
        private const string MetaScreenStage03BackgroundResourcePath = "UI/MetaStages/Stage_03";
        private const string MetaScreenStage04BackgroundResourcePath = "UI/MetaStages/Stage_04";
        private const string MetaScreenStage05BackgroundResourcePath = "UI/MetaStages/Stage_05";
        private const string MetaScreenStartButtonResourcePath = "UI/MetaScreen/Start_button";
        private const string MetaScreenProgressBarResourcePath = "UI/MetaScreen/progressbar";
        private const string WinWindowResourcePath = "UI/ResultScreens/Win_window";
        private const string LoseWindowResourcePath = "UI/ResultScreens/Lose_window";
        private const string BgmResourcePath = "Music/tiles_main_theme";
        private const string StartScreenMusicResourcePath = "Music/start";
        private const string TileTouchSfxResourcePath = "Sfx/tile_touch";
        private const string Match3SfxResourcePath = "Sfx/match3";
        private const string WinSfxResourcePath = "Sfx/win";
        private const string LoseSfxResourcePath = "Sfx/lose";
        private const string LevelsResourcePath = "Levels/";
        private const float TileIconSizeFactor = 0.696f;
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
        private const float MixVfxDurationSeconds = 0.26f;
        private const float MixFlashPhaseRatio = 0.44f;
        private const float MixSymbolPulseScale = 0.08f;
        private const float StartScreenBlinkPeriodSeconds = 1f;
        private const float StartScreenActionTextScale = 0.75f;
        private const float StartScreenScientistFadeDurationSeconds = 0.35f;
        private const float StartScreenDialogueFadeDurationSeconds = 0.35f;
        private const float MetaScreenOutroFadeDurationSeconds = 0.45f;
        private const float StartScreenDialogueScaleMultiplier = 2f;
        private const float StartScreenHappyScientistScaleMultiplier = 0.8f;
        private const float StartScreenMusicVolume = 0.5f;
        private const float BgmVolume = 0.45f;
        private const float TileTouchSfxVolume = 0.75f;
        private const float Match3SfxVolume = 0.9f;
        private const float ResultSfxVolume = 0.95f;
        private const int DefaultTrayCapacity = 7;
        private const int MaxSymbolsOnLevel = 26;
        private const int MaxStackHeightPerSector = 9;
        private const int MaxPlayableLevels = 20;
        private const int BoardColumns = 6;
        private const int BoardRows = 6;
        private const int MaxStacksPerLayer = BoardColumns * BoardRows;
        private const string MetaScreenDialogueLine1 = "Отлично! Добытых тобой плиткобайтов уже хватило для начала работ!";
        private const string MetaScreenDialogueLine2 = "Продолжай в том же духе, и мы улетим с этой богом проклятой планеты!";
        private const string MetaScreenDialogueLine3 = "\u0414\u0443\u043c\u0430\u044e, \u0442\u044b \u0438 \u0442\u0430\u043a \u043f\u043e\u043d\u044f\u043b, \u0447\u0442\u043e \u0432\u0432\u0435\u0440\u0445\u0443 \u2014 \u043f\u0440\u043e\u0433\u0440\u0435\u0441\u0441 \u0440\u0430\u0431\u043e\u0442, \u0430 \u0441\u0440\u0430\u0437\u0443 \u0437\u0430 \u043c\u043d\u043e\u0439 \u2014 \u043a\u043d\u043e\u043f\u043a\u0430 \u0441\u043b\u0435\u0434\u0443\u044e\u0449\u0435\u0433\u043e \u0443\u0440\u043e\u0432\u043d\u044f... \u0422\u044b \u0436\u0435 \u0421\u0443\u043f\u0435\u0440\u043a\u043e\u043c\u043f\u044c\u044e\u0442\u0435\u0440";
        private const string StartScreenDialogueLine1 = "Великий Суперкомпьютер! Ты заработал!";
        private const string StartScreenDialogueLine2 = "Теперь у человечества есть шанс!";
        private const string StartScreenDialogueLine3 = "Вперёд! Обрабатывай плиткобайты информации, чтобы найти решение!";
        private static readonly Rect TileBaseCropUv = new Rect(0.15625f, 0.115234375f, 0.6875f, 0.7529296875f);

        private static readonly Dictionary<TileType, string> TileSymbolFileByType = new Dictionary<TileType, string>
        {
            { TileType.A, "Symbol_1" },
            { TileType.B, "Symbol_2" },
            { TileType.C, "Symbol_3" },
            { TileType.D, "Symbol_4" },
            { TileType.E, "Symbol_5" },
            { TileType.F, "Symbol_6" },
            { TileType.G, "Symbol_7" },
            { TileType.H, "Symbol_8" },
            { TileType.I, "Symbol_9" },
            { TileType.J, "Symbol_10" },
            { TileType.K, "Symbol_11" },
            { TileType.L, "Symbol_12" },
            { TileType.M, "Symbol_13" },
            { TileType.N, "Symbol_14" },
            { TileType.O, "Symbol_15" },
            { TileType.P, "Symbol_16" },
            { TileType.Q, "Symbol_17" },
            { TileType.R, "Symbol_18" },
            { TileType.S, "Symbol_19" },
            { TileType.T, "Symbol_20" },
            { TileType.U, "Symbol_21" },
            { TileType.V, "Symbol_22" },
            { TileType.W, "Symbol_23" },
            { TileType.X, "Symbol_24" },
            { TileType.Y, "Symbol_25" },
            { TileType.Z, "Symbol_26" }
        };

        private readonly TileGameCore _game = new TileGameCore();

        private int _currentLevelIndex;
        private int? _hintTileId;
        private float _hintExpiresAt;

        private GUIStyle _titleStyle;
        private GUIStyle _topLevelStyle;
        private GUIStyle _statusStyle;
        private GUIStyle _tileStyle;
        private GUIStyle _trayStyle;
        private GUIStyle _buttonStyle;
        private GUIStyle _tileOverlayStyle;
        private GUIStyle _startScreenDialogueStyle;
        private GUIStyle _startScreenDialogueLine3Style;
        private bool _isPortrait = true;
        private bool _styleIsPortrait = true;
        private float _uiScale = 1f;
        private float _styleScale = -1f;
        private Texture2D _tileTexture;
        private Texture2D _levelBackgroundTexture;
        private Texture2D _startScreenBackgroundTexture;
        private Texture2D _startScreenActionTextTexture;
        private Texture2D _startScreenScientistWonderTexture;
        private Texture2D _startScreenScientistHappyTexture;
        private Texture2D _startScreenScientistSupportTexture;
        private Texture2D _startScreenScientistDissappointment2Texture;
        private Texture2D _startScreenDialogueWindowTexture;
        private Texture2D _metaScreenStage01BackgroundTexture;
        private Texture2D _metaScreenStage02BackgroundTexture;
        private Texture2D _metaScreenStage03BackgroundTexture;
        private Texture2D _metaScreenStage04BackgroundTexture;
        private Texture2D _metaScreenStage05BackgroundTexture;
        private Texture2D _metaScreenStartButtonTexture;
        private Texture2D _metaScreenProgressBarTexture;
        private Texture2D _winWindowTexture;
        private Texture2D _loseWindowTexture;
        private Texture2D _undoButtonTexture;
        private Texture2D _hintButtonTexture;
        private Texture2D _restartButtonTexture;
        private AudioClip _bgmClip;
        private AudioClip _startScreenClip;
        private AudioClip _tileTouchClip;
        private AudioClip _match3Clip;
        private AudioClip _winClip;
        private AudioClip _loseClip;
        private AudioSource _bgmSource;
        private AudioSource _startScreenAudioSource;
        private AudioSource _sfxSource;
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
        private readonly Dictionary<int, TileType> _mixOldTileTypesById = new Dictionary<int, TileType>();
        private bool _hasPendingCompactTargetTray;
        private bool _isMixVfxActive;
        private bool _isCheatPanelVisible;
        private bool _isStartScreenActive = true;
        private bool _isMetaScreenActive;
        private bool _isMetaScreenIntroFlow;
        private bool _isStartScreenDialogueVisible;
        private bool _isMetaScreenDialogueVisible;
        private float _mixVfxStartedAt;
        private float _startScreenScientistTransitionStartedAt = -1f;
        private float _startScreenDialogueFadeStartedAt = -1f;
        private float _startScreenTapUnlockAt;
        private float _metaScreenScientistTransitionStartedAt = -1f;
        private float _metaScreenDialogueFadeStartedAt = -1f;
        private float _metaScreenOutroFadeStartedAt = -1f;
        private float _metaScreenTapUnlockAt;
        private int _flightSequence;
        private int _trayMatchVfxSeed;
        private int _highestCompletedLevel;
        private int _metaNextLevelIndex;
        private int _cheatTargetLevelIndex;
        private GameStatus _lastResultStatus = GameStatus.Playing;
        private string _startScreenDialogueText = string.Empty;
        private string _pendingStartScreenDialogueText = string.Empty;
        private string _metaScreenDialogueText = string.Empty;
        private string _pendingMetaScreenDialogueText = string.Empty;
        private StartScreenStage _startScreenStage = StartScreenStage.Prompt;
        private MetaScreenStage _metaScreenStage = MetaScreenStage.SupportIntro;
        private Texture2D _currentStartScreenScientistTexture;
        private Texture2D _previousStartScreenScientistTexture;
        private Texture2D _currentMetaScreenScientistTexture;
        private Texture2D _previousMetaScreenScientistTexture;

        private enum StartScreenStage
        {
            Prompt = 0,
            WonderIntro = 1,
            HappyIntro = 2,
            SupportIntro = 3,
            ReadyToStart = 4
        }

        private enum MetaScreenStage
        {
            SupportIntro = 0,
            HappyIntro = 1,
            DissappointmentIntro = 2,
            FadingOutToUiOnly = 3,
            ReadyToStartLevel2 = 4
        }

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
            _levelBackgroundTexture = Resources.Load<Texture2D>(LevelBackgroundResourcePath);
            _startScreenBackgroundTexture = Resources.Load<Texture2D>(StartScreenBackgroundResourcePath);
            _startScreenActionTextTexture = Resources.Load<Texture2D>(StartScreenActionTextResourcePath);
            _startScreenScientistWonderTexture = Resources.Load<Texture2D>(StartScreenScientistWonderResourcePath);
            _startScreenScientistHappyTexture = Resources.Load<Texture2D>(StartScreenScientistHappyResourcePath);
            _startScreenScientistSupportTexture = Resources.Load<Texture2D>(StartScreenScientistSupportResourcePath);
            _startScreenScientistDissappointment2Texture = Resources.Load<Texture2D>(StartScreenScientistDissappointment2ResourcePath);
            _startScreenDialogueWindowTexture = Resources.Load<Texture2D>(StartScreenDialogueWindowResourcePath);
            _metaScreenStage01BackgroundTexture = Resources.Load<Texture2D>(MetaScreenStage01BackgroundResourcePath);
            _metaScreenStage02BackgroundTexture = Resources.Load<Texture2D>(MetaScreenStage02BackgroundResourcePath);
            _metaScreenStage03BackgroundTexture = Resources.Load<Texture2D>(MetaScreenStage03BackgroundResourcePath);
            _metaScreenStage04BackgroundTexture = Resources.Load<Texture2D>(MetaScreenStage04BackgroundResourcePath);
            _metaScreenStage05BackgroundTexture = Resources.Load<Texture2D>(MetaScreenStage05BackgroundResourcePath);
            _metaScreenStartButtonTexture = Resources.Load<Texture2D>(MetaScreenStartButtonResourcePath);
            _metaScreenProgressBarTexture = Resources.Load<Texture2D>(MetaScreenProgressBarResourcePath);
            _winWindowTexture = Resources.Load<Texture2D>(WinWindowResourcePath);
            _loseWindowTexture = Resources.Load<Texture2D>(LoseWindowResourcePath);
            _undoButtonTexture = Resources.Load<Texture2D>(UndoButtonIconResourcePath);
            _hintButtonTexture = Resources.Load<Texture2D>(HintButtonIconResourcePath);
            _restartButtonTexture = Resources.Load<Texture2D>(RestartButtonIconResourcePath);
            _bgmClip = Resources.Load<AudioClip>(BgmResourcePath);
            _startScreenClip = Resources.Load<AudioClip>(StartScreenMusicResourcePath);
            _tileTouchClip = Resources.Load<AudioClip>(TileTouchSfxResourcePath);
            _match3Clip = Resources.Load<AudioClip>(Match3SfxResourcePath);
            _winClip = Resources.Load<AudioClip>(WinSfxResourcePath);
            _loseClip = Resources.Load<AudioClip>(LoseSfxResourcePath);
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

            if (_startScreenBackgroundTexture == null)
            {
                Debug.LogError("Start screen background not found at Resources/" + StartScreenBackgroundResourcePath + ".png");
            }
            if (_startScreenActionTextTexture == null)
            {
                Debug.LogError("Start screen action text not found at Resources/" + StartScreenActionTextResourcePath + ".png");
            }
            if (_startScreenScientistWonderTexture == null)
            {
                Debug.LogError("Start screen scientist wonder not found at Resources/" + StartScreenScientistWonderResourcePath + ".png");
            }
            if (_startScreenScientistHappyTexture == null)
            {
                Debug.LogError("Start screen scientist happy not found at Resources/" + StartScreenScientistHappyResourcePath + ".png");
            }
            if (_startScreenScientistSupportTexture == null)
            {
                Debug.LogError("Start screen scientist support not found at Resources/" + StartScreenScientistSupportResourcePath + ".png");
            }
            if (_startScreenScientistDissappointment2Texture == null)
            {
                Debug.LogError("Start screen scientist dissappointment_2 not found at Resources/" + StartScreenScientistDissappointment2ResourcePath + ".png");
            }
            if (_startScreenDialogueWindowTexture == null)
            {
                Debug.LogError("Start screen dialogue window not found at Resources/" + StartScreenDialogueWindowResourcePath + ".png");
            }
            if (_metaScreenStage01BackgroundTexture == null)
            {
                Debug.LogError("Meta screen stage 01 background not found at Resources/" + MetaScreenStage01BackgroundResourcePath + ".png");
            }
            if (_metaScreenStage02BackgroundTexture == null)
            {
                Debug.LogError("Meta screen stage 02 background not found at Resources/" + MetaScreenStage02BackgroundResourcePath + ".png");
            }
            if (_metaScreenStage03BackgroundTexture == null)
            {
                Debug.LogError("Meta screen stage 03 background not found at Resources/" + MetaScreenStage03BackgroundResourcePath + ".png");
            }
            if (_metaScreenStage04BackgroundTexture == null)
            {
                Debug.LogError("Meta screen stage 04 background not found at Resources/" + MetaScreenStage04BackgroundResourcePath + ".png");
            }
            if (_metaScreenStage05BackgroundTexture == null)
            {
                Debug.LogError("Meta screen stage 05 background not found at Resources/" + MetaScreenStage05BackgroundResourcePath + ".png");
            }
            if (_metaScreenStartButtonTexture == null)
            {
                Debug.LogError("Meta screen start button not found at Resources/" + MetaScreenStartButtonResourcePath + ".png");
            }
            if (_metaScreenProgressBarTexture == null)
            {
                Debug.LogError("Meta screen progress bar not found at Resources/" + MetaScreenProgressBarResourcePath + ".png");
            }
            if (_winWindowTexture == null)
            {
                Debug.LogError("Win window texture not found at Resources/" + WinWindowResourcePath + ".png");
            }
            if (_loseWindowTexture == null)
            {
                Debug.LogError("Lose window texture not found at Resources/" + LoseWindowResourcePath + ".png");
            }
            if (_startScreenClip == null)
            {
                Debug.LogError("Start screen music clip not found at Resources/" + StartScreenMusicResourcePath + ".mp3");
            }

            if (_tileTouchClip == null)
            {
                Debug.LogError("Tile touch SFX clip not found at Resources/" + TileTouchSfxResourcePath + ".mp3");
            }
            if (_match3Clip == null)
            {
                Debug.LogError("Match3 SFX clip not found at Resources/" + Match3SfxResourcePath + ".mp3");
            }
            if (_winClip == null)
            {
                Debug.LogError("Win SFX clip not found at Resources/" + WinSfxResourcePath + ".mp3");
            }
            if (_loseClip == null)
            {
                Debug.LogError("Lose SFX clip not found at Resources/" + LoseSfxResourcePath + ".mp3");
            }

            _sfxSource = gameObject.AddComponent<AudioSource>();
            _sfxSource.playOnAwake = false;
            _sfxSource.loop = false;
            _sfxSource.spatialBlend = 0f;
            _sfxSource.volume = TileTouchSfxVolume;

            _startScreenAudioSource = gameObject.AddComponent<AudioSource>();
            _startScreenAudioSource.playOnAwake = false;
            _startScreenAudioSource.loop = true;
            _startScreenAudioSource.spatialBlend = 0f;
            _startScreenAudioSource.volume = StartScreenMusicVolume;
            if (_startScreenClip != null)
            {
                _startScreenAudioSource.clip = _startScreenClip;
            }

            LoadTileSymbols();
            InitializeStartScreenState();
            InitializeMetaScreenState();
            StartStartScreenAudio();
        }

        private void Update()
        {
            UpdateMixVfx();
            UpdateTileFlights();
            UpdateTrayShiftVfx();
            UpdateTrayMatchVfx();
            UpdateTrayCompactVfx();
            UpdateResultScreenSfx();
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

            if (_isStartScreenActive)
            {
                DrawStartScreen();
                DrawCheatPanel();
                return;
            }

            if (_isMetaScreenActive)
            {
                DrawMetaScreen();
                DrawCheatPanel();
                return;
            }

            if (_levelBackgroundTexture != null)
            {
                GUI.DrawTexture(
                    new Rect(0f, 0f, Screen.width, Screen.height),
                    _levelBackgroundTexture,
                    ScaleMode.ScaleAndCrop,
                    true);
            }

            if (_hintTileId.HasValue && Time.unscaledTime > _hintExpiresAt)
            {
                _hintTileId = null;
            }

            var padding = _isPortrait ? Scale(Padding) : Scale(16f);
            var topHeight = _isPortrait ? Scale(76f) : Scale(64f);
            var controlsHeight = _isPortrait ? Scale(270f) : Scale(240f);
            const float trayHeightMultiplier = 1.4f;
            var trayHeight = Scale(142f * trayHeightMultiplier);
            var traySideMargin = Scale(8f);
            var trayBottomMargin = Scale(8f);
            var controlsToTrayGap = Scale(8f);
            var minimumBoardHeight = Scale(140f);
            var minimumTrayHeight = Scale(140f);
            var minimumControlsHeight = Scale(52f);
            var safeTopInset = Mathf.Max(0f, Screen.height - Screen.safeArea.yMax);
            var topExtraDrop = _isPortrait ? Scale(28f) : Scale(12f);

            var topRect = new Rect(
                padding,
                safeTopInset + padding + topExtraDrop,
                Screen.width - (padding * 2f),
                topHeight);
            var trayRect = new Rect();
            var controlsRect = new Rect();

            RecalculateBottomStack();

            var lowerUiTop = Mathf.Min(trayRect.yMin, controlsRect.yMin);
            var boardHeight = lowerUiTop - topRect.yMax - (padding * 2f);
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
                lowerUiTop = Mathf.Min(trayRect.yMin, controlsRect.yMin);
                boardHeight = lowerUiTop - topRect.yMax - (padding * 2f);
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
            DrawOverlay(topRect, trayRect, controlsRect);
            DrawCheatPanel();

            void RecalculateBottomStack()
            {
                controlsRect = new Rect(
                    padding,
                    Screen.height - controlsHeight - trayBottomMargin,
                    Screen.width - (padding * 2f),
                    controlsHeight);

                trayRect = new Rect(
                    traySideMargin,
                    controlsRect.y - trayHeight - controlsToTrayGap,
                    Screen.width - (traySideMargin * 2f),
                    trayHeight);
            }
        }

        private void DrawTop(Rect rect)
        {
            var levelText = (_currentLevelIndex + 1).ToString();
            GUI.Label(rect, levelText, _topLevelStyle);
        }

        private bool CheatsEnabled => Application.isEditor || Debug.isDebugBuild;

        private void DrawCheatPanel()
        {
            if (!CheatsEnabled)
            {
                return;
            }

            var safeTopInset = Mathf.Max(0f, Screen.height - Screen.safeArea.yMax);
            var margin = Scale(10f);
            var toggleWidth = Mathf.Min(Scale(160f), Screen.width - (margin * 2f));
            var toggleHeight = Mathf.Max(48f, Scale(56f));
            var toggleRect = new Rect(
                margin,
                safeTopInset + margin,
                toggleWidth,
                toggleHeight);

            if (GUI.Button(toggleRect, "CHEATS", _buttonStyle))
            {
                _isCheatPanelVisible = !_isCheatPanelVisible;
                if (_isCheatPanelVisible)
                {
                    _cheatTargetLevelIndex = Mathf.Clamp(_currentLevelIndex, 0, MaxPlayableLevels - 1);
                }
            }

            if (!_isCheatPanelVisible)
            {
                return;
            }

            _cheatTargetLevelIndex = Mathf.Clamp(_cheatTargetLevelIndex, 0, MaxPlayableLevels - 1);

            var panelWidth = Mathf.Min(Scale(420f), Screen.width - (margin * 2f));
            var panelPadding = Scale(10f);
            var rowHeight = Mathf.Max(40f, Scale(44f));
            var rowGap = Scale(6f);
            var panelHeight = panelPadding * 2f + rowHeight * 5f + rowGap * 4f;
            var panelRect = new Rect(
                margin,
                toggleRect.yMax + Scale(8f),
                panelWidth,
                panelHeight);

            GUI.Box(panelRect, string.Empty);

            var rowY = panelRect.y + panelPadding;
            var titleRect = new Rect(panelRect.x + panelPadding, rowY, panelRect.width - panelPadding * 2f, rowHeight);
            GUI.Label(titleRect, "Dev QA Cheats", _statusStyle);
            rowY += rowHeight + rowGap;

            var levelRect = new Rect(panelRect.x + panelPadding, rowY, panelRect.width - panelPadding * 2f, rowHeight);
            GUI.Label(levelRect, "Selected Level: " + (_cheatTargetLevelIndex + 1), _statusStyle);
            rowY += rowHeight + rowGap;

            var halfGap = Scale(4f);
            var halfWidth = (panelRect.width - panelPadding * 2f - halfGap) * 0.5f;
            var prevRect = new Rect(panelRect.x + panelPadding, rowY, halfWidth, rowHeight);
            var nextRect = new Rect(prevRect.xMax + halfGap, rowY, halfWidth, rowHeight);
            if (GUI.Button(prevRect, "Prev Level", _buttonStyle))
            {
                _cheatTargetLevelIndex = Mathf.Max(0, _cheatTargetLevelIndex - 1);
            }

            if (GUI.Button(nextRect, "Next Level", _buttonStyle))
            {
                _cheatTargetLevelIndex = Mathf.Min(MaxPlayableLevels - 1, _cheatTargetLevelIndex + 1);
            }

            rowY += rowHeight + rowGap;

            var loadRect = new Rect(panelRect.x + panelPadding, rowY, panelRect.width - panelPadding * 2f, rowHeight);
            if (GUI.Button(loadRect, "Load Level", _buttonStyle))
            {
                LoadCheatLevel();
            }

            rowY += rowHeight + rowGap;

            var canForceResult = CanForceDebugResult();
            var previousEnabled = GUI.enabled;
            GUI.enabled = canForceResult;

            var winRect = new Rect(panelRect.x + panelPadding, rowY, halfWidth, rowHeight);
            var loseRect = new Rect(winRect.xMax + halfGap, rowY, halfWidth, rowHeight);
            if (GUI.Button(winRect, "Force WIN", _buttonStyle))
            {
                TriggerDebugWin();
            }

            if (GUI.Button(loseRect, "Force LOSE", _buttonStyle))
            {
                TriggerDebugLose();
            }

            GUI.enabled = previousEnabled;
        }

        private void LoadCheatLevel()
        {
            _currentLevelIndex = Mathf.Clamp(_cheatTargetLevelIndex, 0, MaxPlayableLevels - 1);
            StartCurrentLevel();
        }

        private bool CanForceDebugResult()
        {
            return !_isStartScreenActive
                && !_isMetaScreenActive
                && _game.Status == GameStatus.Playing;
        }

        private void TriggerDebugWin()
        {
            if (!CanForceDebugResult())
            {
                return;
            }

            ClearTransientUiStateForDebugResult();
            _game.ForceWinForDebug();
            SyncProjectedTrayWithGame();
            ApplyVisualTrayFromDenseList(_game.Tray);
            _lastResultStatus = GameStatus.Playing;
        }

        private void TriggerDebugLose()
        {
            if (!CanForceDebugResult())
            {
                return;
            }

            ClearTransientUiStateForDebugResult();
            _game.ForceLoseForDebug();
            SyncProjectedTrayWithGame();
            ApplyVisualTrayFromDenseList(_game.Tray);
            _lastResultStatus = GameStatus.Playing;
        }

        private void ClearTransientUiStateForDebugResult()
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
            _mixOldTileTypesById.Clear();
            _isMixVfxActive = false;
            _mixVfxStartedAt = 0f;
        }

        private void InitializeStartScreenState()
        {
            _isStartScreenActive = true;
            _isStartScreenDialogueVisible = false;
            _startScreenStage = StartScreenStage.Prompt;
            _startScreenDialogueText = string.Empty;
            _pendingStartScreenDialogueText = string.Empty;
            _currentStartScreenScientistTexture = null;
            _previousStartScreenScientistTexture = null;
            _startScreenScientistTransitionStartedAt = -1f;
            _startScreenDialogueFadeStartedAt = -1f;
            _startScreenTapUnlockAt = 0f;
        }

        private void InitializeMetaScreenState()
        {
            _isMetaScreenActive = false;
            _isMetaScreenIntroFlow = false;
            _isMetaScreenDialogueVisible = false;
            _metaScreenStage = MetaScreenStage.SupportIntro;
            _metaScreenDialogueText = string.Empty;
            _pendingMetaScreenDialogueText = string.Empty;
            _currentMetaScreenScientistTexture = null;
            _previousMetaScreenScientistTexture = null;
            _metaScreenScientistTransitionStartedAt = -1f;
            _metaScreenDialogueFadeStartedAt = -1f;
            _metaScreenOutroFadeStartedAt = -1f;
            _metaScreenTapUnlockAt = 0f;
            _metaNextLevelIndex = 0;
        }

        private void OpenMetaScreenAfterLevel1Win()
        {
            OpenMetaScreen(1, useIntroFlow: true);
        }

        private void OpenMetaScreen(int nextLevelIndex, bool useIntroFlow)
        {
            InitializeMetaScreenState();
            _isMetaScreenActive = true;
            _isMetaScreenIntroFlow = useIntroFlow;
            _metaNextLevelIndex = Mathf.Clamp(nextLevelIndex, 0, MaxPlayableLevels - 1);

            if (_isMetaScreenIntroFlow)
            {
                BeginMetaScreenScientistStep(
                    MetaScreenStage.SupportIntro,
                    _startScreenScientistSupportTexture,
                    MetaScreenDialogueLine1,
                    fadeDialogueWindow: true,
                    Time.unscaledTime);
            }
            else
            {
                _metaScreenStage = MetaScreenStage.ReadyToStartLevel2;
                _currentMetaScreenScientistTexture = null;
                _previousMetaScreenScientistTexture = null;
                _metaScreenDialogueText = string.Empty;
                _pendingMetaScreenDialogueText = string.Empty;
                _isMetaScreenDialogueVisible = false;
                _metaScreenScientistTransitionStartedAt = -1f;
                _metaScreenDialogueFadeStartedAt = -1f;
                _metaScreenOutroFadeStartedAt = -1f;
                _metaScreenTapUnlockAt = 0f;
            }

            StartStartScreenAudio();
        }

        private void DrawStartScreen()
        {
            var now = Time.unscaledTime;
            UpdateStartScreenSceneState(now);

            var screenRect = new Rect(0f, 0f, Screen.width, Screen.height);
            if (_startScreenBackgroundTexture != null)
            {
                GUI.DrawTexture(screenRect, _startScreenBackgroundTexture, ScaleMode.ScaleAndCrop, true);
            }
            else
            {
                var previousColor = GUI.color;
                GUI.color = Color.black;
                GUI.DrawTexture(screenRect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true);
                GUI.color = previousColor;
            }

            if (_startScreenStage == StartScreenStage.Prompt && _startScreenActionTextTexture != null)
            {
                var pulse = 0.5f + 0.5f * Mathf.Sin((Time.unscaledTime / StartScreenBlinkPeriodSeconds) * Mathf.PI * 2f);
                var alpha = Mathf.Lerp(0.15f, 1f, pulse);
                var maxWidth = Screen.width * 0.75f;
                var maxHeight = Screen.height * 0.2f;
                var scaleByWidth = _startScreenActionTextTexture.width > 0 ? maxWidth / _startScreenActionTextTexture.width : 1f;
                var scaleByHeight = _startScreenActionTextTexture.height > 0 ? maxHeight / _startScreenActionTextTexture.height : 1f;
                var scale = Mathf.Min(scaleByWidth, scaleByHeight);
                var drawWidth = _startScreenActionTextTexture.width * scale * StartScreenActionTextScale;
                var drawHeight = _startScreenActionTextTexture.height * scale * StartScreenActionTextScale;
                var actionRect = new Rect(
                    (Screen.width - drawWidth) * 0.5f,
                    (Screen.height - drawHeight) * 0.5f,
                    drawWidth,
                    drawHeight);

                var previousColor = GUI.color;
                GUI.color = new Color(1f, 1f, 1f, alpha);
                GUI.DrawTexture(actionRect, _startScreenActionTextTexture, ScaleMode.ScaleToFit, true);
                GUI.color = previousColor;
            }
            else
            {
                DrawStartScreenDialogueScene(now);
            }

            if (GUI.Button(screenRect, GUIContent.none, GUIStyle.none))
            {
                HandleStartScreenTap(now);
            }
        }

        private void DrawMetaScreen()
        {
            var now = Time.unscaledTime;
            UpdateMetaScreenSceneState(now);

            var screenRect = new Rect(0f, 0f, Screen.width, Screen.height);
            var backgroundTexture = GetMetaScreenBackgroundTexture();
            if (backgroundTexture != null)
            {
                GUI.DrawTexture(screenRect, backgroundTexture, ScaleMode.ScaleAndCrop, true);
            }
            else
            {
                var previousColor = GUI.color;
                GUI.color = Color.black;
                GUI.DrawTexture(screenRect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true);
                GUI.color = previousColor;
            }

            DrawMetaScreenProgressAndNextLevelButton();
            DrawMetaScreenDialogueScene(now);

            if (ShouldHandleMetaScreenFullscreenTap() && GUI.Button(screenRect, GUIContent.none, GUIStyle.none))
            {
                HandleMetaScreenTap(now);
            }
        }

        private void DrawStartScreenDialogueScene(float now)
        {
            var scientistTextureForLayout = _currentStartScreenScientistTexture != null
                ? _currentStartScreenScientistTexture
                : _previousStartScreenScientistTexture;
            if (scientistTextureForLayout == null)
            {
                return;
            }

            var scientistMaxWidth = Screen.width * (_isPortrait ? 0.9f : 0.56f);
            var scientistMaxHeight = Screen.height * (_isPortrait ? 0.92f : 1.48f);
            var layoutScientistScale = GetMetaScientistScaleMultiplier(scientistTextureForLayout, scientistMaxWidth, scientistMaxHeight);
            var scientistRect = BuildBottomCenteredFittedRect(
                scientistTextureForLayout,
                Screen.width * 0.5f,
                Screen.height,
                scientistMaxWidth * layoutScientistScale,
                scientistMaxHeight * layoutScientistScale);

            var scientistProgress = GetStartScreenScientistTransitionProgress(now);
            var currentScientistAlpha = 1f;
            var previousScientistAlpha = 0f;
            if (_startScreenScientistTransitionStartedAt >= 0f)
            {
                if (_previousStartScreenScientistTexture != null)
                {
                    previousScientistAlpha = 1f - scientistProgress;
                    currentScientistAlpha = scientistProgress;
                }
                else
                {
                    currentScientistAlpha = scientistProgress;
                }
            }

            if (_previousStartScreenScientistTexture != null && previousScientistAlpha > 0f)
            {
                var previousScientistScale = GetStartScreenScientistScaleMultiplier(_previousStartScreenScientistTexture);
                var previousScientistRect = BuildBottomCenteredFittedRect(
                    _previousStartScreenScientistTexture,
                    Screen.width * 0.5f,
                    Screen.height,
                    scientistMaxWidth * previousScientistScale,
                    scientistMaxHeight * previousScientistScale);
                DrawTextureWithAlpha(previousScientistRect, _previousStartScreenScientistTexture, previousScientistAlpha, ScaleMode.ScaleToFit);
            }

            if (_currentStartScreenScientistTexture != null && currentScientistAlpha > 0f)
            {
                var currentScientistScale = GetStartScreenScientistScaleMultiplier(_currentStartScreenScientistTexture);
                var currentScientistRect = BuildBottomCenteredFittedRect(
                    _currentStartScreenScientistTexture,
                    Screen.width * 0.5f,
                    Screen.height,
                    scientistMaxWidth * currentScientistScale,
                    scientistMaxHeight * currentScientistScale);
                DrawTextureWithAlpha(currentScientistRect, _currentStartScreenScientistTexture, currentScientistAlpha, ScaleMode.ScaleToFit);
            }

            var dialogueAlpha = GetStartScreenDialogueAlpha(now);
            if (dialogueAlpha <= 0f)
            {
                return;
            }

            var dialogueMaxWidth = Screen.width * (_isPortrait ? 0.72f : 0.58f);
            var dialogueTopSafeMargin = Screen.height * 0.02f;
            var dialogueMaxHeight = Mathf.Max(1f, scientistRect.y - dialogueTopSafeMargin);
            var baseDialogueRect = BuildBottomCenteredFittedRect(
                _startScreenDialogueWindowTexture,
                Screen.width * 0.5f,
                scientistRect.y,
                dialogueMaxWidth,
                dialogueMaxHeight);

            if (_startScreenDialogueWindowTexture == null)
            {
                var fallbackHeight = Mathf.Min(Screen.height * (_isPortrait ? 0.24f : 0.36f), dialogueMaxHeight);
                baseDialogueRect = new Rect(
                    (Screen.width - dialogueMaxWidth) * 0.5f,
                    scientistRect.y - fallbackHeight,
                    dialogueMaxWidth,
                    fallbackHeight);
            }

            var dialogueScaledWidth = baseDialogueRect.width * StartScreenDialogueScaleMultiplier;
            var dialogueScaledHeight = baseDialogueRect.height * StartScreenDialogueScaleMultiplier;
            var dialogueWidthClamp = Screen.width * 0.98f;
            var widthFit = dialogueScaledWidth > 0f ? dialogueWidthClamp / dialogueScaledWidth : 1f;
            var heightFit = dialogueScaledHeight > 0f ? dialogueMaxHeight / dialogueScaledHeight : 1f;
            var dialogueFit = Mathf.Min(1f, widthFit, heightFit);
            dialogueScaledWidth *= dialogueFit;
            dialogueScaledHeight *= dialogueFit;
            var dialogueRect = new Rect(
                (Screen.width - dialogueScaledWidth) * 0.5f,
                scientistRect.y - dialogueScaledHeight,
                dialogueScaledWidth,
                dialogueScaledHeight);

            if (_startScreenDialogueWindowTexture != null)
            {
                DrawTextureWithAlpha(dialogueRect, _startScreenDialogueWindowTexture, dialogueAlpha, ScaleMode.ScaleToFit);
            }

            if (!string.IsNullOrEmpty(_startScreenDialogueText))
            {
                var textRect = new Rect(
                    dialogueRect.x + dialogueRect.width * 0.11f,
                    dialogueRect.y + dialogueRect.height * 0.11f,
                    dialogueRect.width * 0.78f,
                    dialogueRect.height * 0.74f);
                var dialogueStyle = _startScreenDialogueText == StartScreenDialogueLine3
                    ? _startScreenDialogueLine3Style
                    : _startScreenDialogueStyle;
                if (dialogueStyle == null)
                {
                    dialogueStyle = _startScreenDialogueStyle;
                }

                var previousColor = GUI.color;
                GUI.color = new Color(1f, 1f, 1f, dialogueAlpha);
                GUI.Label(textRect, _startScreenDialogueText, dialogueStyle);
                GUI.color = previousColor;
            }
        }

        private void DrawMetaScreenDialogueScene(float now)
        {
            if (!_isMetaScreenIntroFlow)
            {
                return;
            }

            if (_isMetaScreenIntroFlow && _metaScreenStage == MetaScreenStage.ReadyToStartLevel2)
            {
                return;
            }

            var scientistTextureForLayout = _currentMetaScreenScientistTexture != null
                ? _currentMetaScreenScientistTexture
                : _previousMetaScreenScientistTexture;
            if (scientistTextureForLayout == null)
            {
                return;
            }

            var scientistMaxWidth = Screen.width * (_isPortrait ? 0.9f : 0.56f);
            var scientistMaxHeight = Screen.height * (_isPortrait ? 0.92f : 1.48f);
            var layoutScientistScale = GetStartScreenScientistScaleMultiplier(scientistTextureForLayout);
            var scientistRect = BuildBottomCenteredFittedRect(
                scientistTextureForLayout,
                Screen.width * 0.5f,
                Screen.height,
                scientistMaxWidth * layoutScientistScale,
                scientistMaxHeight * layoutScientistScale);

            var isOutroFading = _metaScreenStage == MetaScreenStage.FadingOutToUiOnly;
            var outroFadeProgress = isOutroFading ? GetMetaScreenOutroFadeProgress(now) : 0f;
            var currentScientistAlpha = isOutroFading ? Mathf.Clamp01(1f - outroFadeProgress) : 1f;
            var previousScientistAlpha = 0f;
            if (!isOutroFading && _metaScreenScientistTransitionStartedAt >= 0f)
            {
                var scientistProgress = GetMetaScreenScientistTransitionProgress(now);
                if (_previousMetaScreenScientistTexture != null)
                {
                    previousScientistAlpha = 1f - scientistProgress;
                    currentScientistAlpha = scientistProgress;
                }
                else
                {
                    currentScientistAlpha = scientistProgress;
                }
            }

            if (_previousMetaScreenScientistTexture != null && previousScientistAlpha > 0f)
            {
                var previousScientistScale = GetMetaScientistScaleMultiplier(_previousMetaScreenScientistTexture, scientistMaxWidth, scientistMaxHeight);
                var previousScientistRect = BuildBottomCenteredFittedRect(
                    _previousMetaScreenScientistTexture,
                    Screen.width * 0.5f,
                    Screen.height,
                    scientistMaxWidth * previousScientistScale,
                    scientistMaxHeight * previousScientistScale);
                DrawTextureWithAlpha(previousScientistRect, _previousMetaScreenScientistTexture, previousScientistAlpha, ScaleMode.ScaleToFit);
            }

            if (_currentMetaScreenScientistTexture != null && currentScientistAlpha > 0f)
            {
                var currentScientistScale = GetMetaScientistScaleMultiplier(_currentMetaScreenScientistTexture, scientistMaxWidth, scientistMaxHeight);
                var currentScientistRect = BuildBottomCenteredFittedRect(
                    _currentMetaScreenScientistTexture,
                    Screen.width * 0.5f,
                    Screen.height,
                    scientistMaxWidth * currentScientistScale,
                    scientistMaxHeight * currentScientistScale);
                if (isOutroFading)
                {
                    DrawInterlacedTextureFade(currentScientistRect, _currentMetaScreenScientistTexture, outroFadeProgress);
                }
                else
                {
                    DrawTextureWithAlpha(currentScientistRect, _currentMetaScreenScientistTexture, currentScientistAlpha, ScaleMode.ScaleToFit);
                }
            }

            var dialogueAlpha = GetMetaScreenDialogueAlpha(now);
            if (isOutroFading)
            {
                dialogueAlpha = Mathf.Clamp01(1f - outroFadeProgress);
            }

            if (dialogueAlpha <= 0f)
            {
                return;
            }

            var dialogueMaxWidth = Screen.width * (_isPortrait ? 0.72f : 0.58f);
            var dialogueTopSafeMargin = Screen.height * 0.02f;
            var dialogueMaxHeight = Mathf.Max(1f, scientistRect.y - dialogueTopSafeMargin);
            var baseDialogueRect = BuildBottomCenteredFittedRect(
                _startScreenDialogueWindowTexture,
                Screen.width * 0.5f,
                scientistRect.y,
                dialogueMaxWidth,
                dialogueMaxHeight);

            if (_startScreenDialogueWindowTexture == null)
            {
                var fallbackHeight = Mathf.Min(Screen.height * (_isPortrait ? 0.24f : 0.36f), dialogueMaxHeight);
                baseDialogueRect = new Rect(
                    (Screen.width - dialogueMaxWidth) * 0.5f,
                    scientistRect.y - fallbackHeight,
                    dialogueMaxWidth,
                    fallbackHeight);
            }

            var dialogueScaledWidth = baseDialogueRect.width * StartScreenDialogueScaleMultiplier;
            var dialogueScaledHeight = baseDialogueRect.height * StartScreenDialogueScaleMultiplier;
            var dialogueWidthClamp = Screen.width * 0.98f;
            var widthFit = dialogueScaledWidth > 0f ? dialogueWidthClamp / dialogueScaledWidth : 1f;
            var heightFit = dialogueScaledHeight > 0f ? dialogueMaxHeight / dialogueScaledHeight : 1f;
            var dialogueFit = Mathf.Min(1f, widthFit, heightFit);
            dialogueScaledWidth *= dialogueFit;
            dialogueScaledHeight *= dialogueFit;
            var dialogueRect = new Rect(
                (Screen.width - dialogueScaledWidth) * 0.5f,
                scientistRect.y - dialogueScaledHeight,
                dialogueScaledWidth,
                dialogueScaledHeight);

            if (_startScreenDialogueWindowTexture != null)
            {
                DrawTextureWithAlpha(dialogueRect, _startScreenDialogueWindowTexture, dialogueAlpha, ScaleMode.ScaleToFit);
            }

            if (!string.IsNullOrEmpty(_metaScreenDialogueText))
            {
                var textRect = new Rect(
                    dialogueRect.x + dialogueRect.width * 0.11f,
                    dialogueRect.y + dialogueRect.height * 0.11f,
                    dialogueRect.width * 0.78f,
                    dialogueRect.height * 0.74f);
                var dialogueStyle = GetMetaDialogueStyle(_metaScreenDialogueText, textRect);

                var previousColor = GUI.color;
                GUI.color = new Color(1f, 1f, 1f, dialogueAlpha);
                GUI.Label(textRect, _metaScreenDialogueText, dialogueStyle);
                GUI.color = previousColor;
            }
        }

        private void HandleStartScreenTap(float now)
        {
            if (!_isStartScreenActive || now < _startScreenTapUnlockAt)
            {
                return;
            }

            switch (_startScreenStage)
            {
                case StartScreenStage.Prompt:
                    BeginStartScreenScientistStep(
                        StartScreenStage.WonderIntro,
                        _startScreenScientistWonderTexture,
                        StartScreenDialogueLine1,
                        fadeDialogueWindow: true,
                        now);
                    break;
                case StartScreenStage.WonderIntro:
                    BeginStartScreenScientistStep(
                        StartScreenStage.HappyIntro,
                        _startScreenScientistHappyTexture,
                        StartScreenDialogueLine2,
                        fadeDialogueWindow: false,
                        now);
                    break;
                case StartScreenStage.HappyIntro:
                    BeginStartScreenScientistStep(
                        StartScreenStage.SupportIntro,
                        _startScreenScientistSupportTexture,
                        StartScreenDialogueLine3,
                        fadeDialogueWindow: false,
                        now);
                    break;
                case StartScreenStage.ReadyToStart:
                    StartGameplayFromStartScreen();
                    break;
            }
        }

        private void HandleMetaScreenTap(float now)
        {
            if (!_isMetaScreenActive || !_isMetaScreenIntroFlow || now < _metaScreenTapUnlockAt)
            {
                return;
            }

            switch (_metaScreenStage)
            {
                case MetaScreenStage.SupportIntro:
                    BeginMetaScreenScientistStep(
                        MetaScreenStage.HappyIntro,
                        _startScreenScientistHappyTexture,
                        MetaScreenDialogueLine2,
                        fadeDialogueWindow: false,
                        now);
                    break;
                case MetaScreenStage.HappyIntro:
                {
                    var dissappointmentTexture = _startScreenScientistDissappointment2Texture != null
                        ? _startScreenScientistDissappointment2Texture
                        : _startScreenScientistHappyTexture;
                    BeginMetaScreenScientistStep(
                        MetaScreenStage.DissappointmentIntro,
                        dissappointmentTexture,
                        MetaScreenDialogueLine3,
                        fadeDialogueWindow: false,
                        now);
                    break;
                }
                case MetaScreenStage.DissappointmentIntro:
                    _metaScreenStage = MetaScreenStage.FadingOutToUiOnly;
                    _metaScreenOutroFadeStartedAt = now;
                    _metaScreenTapUnlockAt = now + MetaScreenOutroFadeDurationSeconds;
                    break;
            }
        }

        private void BeginStartScreenScientistStep(
            StartScreenStage stage,
            Texture2D nextScientistTexture,
            string nextDialogueText,
            bool fadeDialogueWindow,
            float now)
        {
            _startScreenStage = stage;

            var scientistTransitionDuration = 0f;
            if (nextScientistTexture != null)
            {
                var currentScientistTexture = _currentStartScreenScientistTexture;
                _currentStartScreenScientistTexture = nextScientistTexture;
                if (currentScientistTexture == nextScientistTexture)
                {
                    _previousStartScreenScientistTexture = null;
                    _startScreenScientistTransitionStartedAt = -1f;
                }
                else
                {
                    _previousStartScreenScientistTexture = currentScientistTexture;
                    _startScreenScientistTransitionStartedAt = now;
                    scientistTransitionDuration = StartScreenScientistFadeDurationSeconds;
                }
            }
            else
            {
                _previousStartScreenScientistTexture = null;
                _startScreenScientistTransitionStartedAt = -1f;
            }

            if (fadeDialogueWindow)
            {
                _startScreenDialogueText = nextDialogueText;
                _pendingStartScreenDialogueText = string.Empty;
                _isStartScreenDialogueVisible = false;
                _startScreenDialogueFadeStartedAt = now + scientistTransitionDuration;
                _startScreenTapUnlockAt = _startScreenDialogueFadeStartedAt + StartScreenDialogueFadeDurationSeconds;
            }
            else
            {
                if (scientistTransitionDuration > 0f)
                {
                    _pendingStartScreenDialogueText = nextDialogueText;
                }
                else
                {
                    _startScreenDialogueText = nextDialogueText;
                    _pendingStartScreenDialogueText = string.Empty;
                }

                _isStartScreenDialogueVisible = true;
                _startScreenDialogueFadeStartedAt = -1f;
                _startScreenTapUnlockAt = now + scientistTransitionDuration;

                if (_startScreenStage == StartScreenStage.SupportIntro && scientistTransitionDuration <= 0f)
                {
                    _startScreenStage = StartScreenStage.ReadyToStart;
                }
            }
        }

        private void BeginMetaScreenScientistStep(
            MetaScreenStage stage,
            Texture2D nextScientistTexture,
            string nextDialogueText,
            bool fadeDialogueWindow,
            float now)
        {
            _metaScreenStage = stage;
            _metaScreenOutroFadeStartedAt = -1f;

            var scientistTransitionDuration = 0f;
            if (nextScientistTexture != null)
            {
                var currentScientistTexture = _currentMetaScreenScientistTexture;
                _currentMetaScreenScientistTexture = nextScientistTexture;
                if (currentScientistTexture == nextScientistTexture)
                {
                    _previousMetaScreenScientistTexture = null;
                    _metaScreenScientistTransitionStartedAt = -1f;
                }
                else
                {
                    _previousMetaScreenScientistTexture = currentScientistTexture;
                    _metaScreenScientistTransitionStartedAt = now;
                    scientistTransitionDuration = StartScreenScientistFadeDurationSeconds;
                }
            }
            else
            {
                _previousMetaScreenScientistTexture = null;
                _metaScreenScientistTransitionStartedAt = -1f;
            }

            if (fadeDialogueWindow)
            {
                _metaScreenDialogueText = nextDialogueText;
                _pendingMetaScreenDialogueText = string.Empty;
                _isMetaScreenDialogueVisible = false;
                _metaScreenDialogueFadeStartedAt = now + scientistTransitionDuration;
                _metaScreenTapUnlockAt = _metaScreenDialogueFadeStartedAt + StartScreenDialogueFadeDurationSeconds;
            }
            else
            {
                if (scientistTransitionDuration > 0f)
                {
                    _pendingMetaScreenDialogueText = nextDialogueText;
                }
                else
                {
                    _metaScreenDialogueText = nextDialogueText;
                    _pendingMetaScreenDialogueText = string.Empty;
                }

                _isMetaScreenDialogueVisible = true;
                _metaScreenDialogueFadeStartedAt = -1f;
                _metaScreenTapUnlockAt = now + scientistTransitionDuration;
            }
        }

        private void UpdateStartScreenSceneState(float now)
        {
            if (!_isStartScreenActive || _startScreenStage == StartScreenStage.Prompt)
            {
                return;
            }

            if (_startScreenScientistTransitionStartedAt >= 0f)
            {
                var progress = GetStartScreenScientistTransitionProgress(now);
                if (progress >= 1f)
                {
                    _startScreenScientistTransitionStartedAt = -1f;
                    _previousStartScreenScientistTexture = null;

                    if (!string.IsNullOrEmpty(_pendingStartScreenDialogueText))
                    {
                        _startScreenDialogueText = _pendingStartScreenDialogueText;
                        _pendingStartScreenDialogueText = string.Empty;
                    }

                    if (_startScreenStage == StartScreenStage.SupportIntro)
                    {
                        _startScreenStage = StartScreenStage.ReadyToStart;
                        _startScreenTapUnlockAt = now;
                    }
                }
            }

            if (!_isStartScreenDialogueVisible && _startScreenDialogueFadeStartedAt >= 0f)
            {
                if (now >= _startScreenDialogueFadeStartedAt + StartScreenDialogueFadeDurationSeconds)
                {
                    _isStartScreenDialogueVisible = true;
                    _startScreenDialogueFadeStartedAt = -1f;
                    _startScreenTapUnlockAt = Mathf.Max(_startScreenTapUnlockAt, now);
                }
            }
        }

        private void UpdateMetaScreenSceneState(float now)
        {
            if (!_isMetaScreenActive)
            {
                return;
            }

            if (_metaScreenScientistTransitionStartedAt >= 0f)
            {
                var progress = GetMetaScreenScientistTransitionProgress(now);
                if (progress >= 1f)
                {
                    _metaScreenScientistTransitionStartedAt = -1f;
                    _previousMetaScreenScientistTexture = null;

                    if (!string.IsNullOrEmpty(_pendingMetaScreenDialogueText))
                    {
                        _metaScreenDialogueText = _pendingMetaScreenDialogueText;
                        _pendingMetaScreenDialogueText = string.Empty;
                    }
                }
            }

            if (!_isMetaScreenDialogueVisible && _metaScreenDialogueFadeStartedAt >= 0f)
            {
                if (now >= _metaScreenDialogueFadeStartedAt + StartScreenDialogueFadeDurationSeconds)
                {
                    _isMetaScreenDialogueVisible = true;
                    _metaScreenDialogueFadeStartedAt = -1f;
                    _metaScreenTapUnlockAt = Mathf.Max(_metaScreenTapUnlockAt, now);
                }
            }

            if (_metaScreenStage == MetaScreenStage.FadingOutToUiOnly)
            {
                var fadeProgress = GetMetaScreenOutroFadeProgress(now);
                if (fadeProgress >= 1f)
                {
                    _metaScreenStage = MetaScreenStage.ReadyToStartLevel2;
                    _metaScreenOutroFadeStartedAt = -1f;
                    _metaScreenDialogueText = string.Empty;
                    _pendingMetaScreenDialogueText = string.Empty;
                    _isMetaScreenDialogueVisible = false;
                    _metaScreenDialogueFadeStartedAt = -1f;
                    _currentMetaScreenScientistTexture = null;
                    _previousMetaScreenScientistTexture = null;
                    _metaScreenTapUnlockAt = now;
                }
            }
        }

        private float GetStartScreenScientistTransitionProgress(float now)
        {
            if (_startScreenScientistTransitionStartedAt < 0f)
            {
                return 1f;
            }

            return Mathf.Clamp01((now - _startScreenScientistTransitionStartedAt) / StartScreenScientistFadeDurationSeconds);
        }

        private float GetStartScreenDialogueAlpha(float now)
        {
            if (_isStartScreenDialogueVisible)
            {
                return 1f;
            }

            if (_startScreenDialogueFadeStartedAt < 0f || now < _startScreenDialogueFadeStartedAt)
            {
                return 0f;
            }

            return Mathf.Clamp01((now - _startScreenDialogueFadeStartedAt) / StartScreenDialogueFadeDurationSeconds);
        }

        private float GetMetaScreenScientistTransitionProgress(float now)
        {
            if (_metaScreenScientistTransitionStartedAt < 0f)
            {
                return 1f;
            }

            return Mathf.Clamp01((now - _metaScreenScientistTransitionStartedAt) / StartScreenScientistFadeDurationSeconds);
        }

        private float GetMetaScreenDialogueAlpha(float now)
        {
            if (_isMetaScreenDialogueVisible)
            {
                return 1f;
            }

            if (_metaScreenDialogueFadeStartedAt < 0f || now < _metaScreenDialogueFadeStartedAt)
            {
                return 0f;
            }

            return Mathf.Clamp01((now - _metaScreenDialogueFadeStartedAt) / StartScreenDialogueFadeDurationSeconds);
        }

        private float GetMetaScreenOutroFadeProgress(float now)
        {
            if (_metaScreenStage != MetaScreenStage.FadingOutToUiOnly || _metaScreenOutroFadeStartedAt < 0f)
            {
                return 0f;
            }

            return Mathf.Clamp01((now - _metaScreenOutroFadeStartedAt) / MetaScreenOutroFadeDurationSeconds);
        }

        private bool ShouldHandleMetaScreenFullscreenTap()
        {
            if (!_isMetaScreenActive || !_isMetaScreenIntroFlow)
            {
                return false;
            }

            return _metaScreenStage == MetaScreenStage.SupportIntro
                || _metaScreenStage == MetaScreenStage.HappyIntro
                || _metaScreenStage == MetaScreenStage.DissappointmentIntro;
        }

        private Texture2D GetMetaScreenBackgroundTexture()
        {
            if (_highestCompletedLevel >= 20 && _metaScreenStage05BackgroundTexture != null)
            {
                return _metaScreenStage05BackgroundTexture;
            }

            if (_highestCompletedLevel >= 15 && _metaScreenStage04BackgroundTexture != null)
            {
                return _metaScreenStage04BackgroundTexture;
            }

            if (_highestCompletedLevel >= 10 && _metaScreenStage03BackgroundTexture != null)
            {
                return _metaScreenStage03BackgroundTexture;
            }

            if (_highestCompletedLevel >= 5 && _metaScreenStage02BackgroundTexture != null)
            {
                return _metaScreenStage02BackgroundTexture;
            }

            return _metaScreenStage01BackgroundTexture;
        }

        private float GetMetaProgressNormalized()
        {
            return Mathf.Clamp01(_highestCompletedLevel / (float)MaxPlayableLevels);
        }

        private void DrawMetaScreenProgressAndNextLevelButton()
        {
            var safeTopInset = Mathf.Max(0f, Screen.height - Screen.safeArea.yMax);
            var safeBottomInset = Mathf.Max(0f, Screen.safeArea.yMin);
            var barWidth = Mathf.Min(Screen.width * 0.84f, Scale(860f));
            var barHeight = Scale(36f);
            var barRect = new Rect(
                (Screen.width - barWidth) * 0.5f,
                safeTopInset + Scale(18f),
                barWidth,
                barHeight);

            var progress = GetMetaProgressNormalized();
            var previousColor = GUI.color;
            GUI.color = new Color(0.04f, 0.08f, 0.16f, 0.78f);
            GUI.DrawTexture(barRect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true);

            var fillPadding = Scale(3f);
            var fillWidth = Mathf.Max(0f, (barRect.width - (fillPadding * 2f)) * progress);
            if (fillWidth > 0f)
            {
                var fillRect = new Rect(
                    barRect.x + fillPadding,
                    barRect.y + fillPadding,
                    fillWidth,
                    barRect.height - (fillPadding * 2f));
                GUI.color = new Color(0.16f, 0.84f, 1f, 0.94f);
                GUI.DrawTexture(fillRect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true);
            }

            GUI.color = previousColor;

            if (_metaScreenProgressBarTexture != null)
            {
                var textureWidth = Mathf.Max(1, _metaScreenProgressBarTexture.width);
                var textureHeight = Mathf.Max(1, _metaScreenProgressBarTexture.height);
                var frameWidth = Screen.width;
                var frameHeight = frameWidth * (textureHeight / (float)textureWidth);
                var frameRect = new Rect(
                    0f,
                    barRect.yMax + Scale(8f),
                    frameWidth,
                    frameHeight);
                GUI.DrawTexture(frameRect, _metaScreenProgressBarTexture, ScaleMode.ScaleToFit, true);
            }

            var canStartFromButton = !_isMetaScreenIntroFlow || _metaScreenStage == MetaScreenStage.ReadyToStartLevel2;
            var previousEnabled = GUI.enabled;
            GUI.enabled = canStartFromButton;

            if (_metaScreenStartButtonTexture != null)
            {
                var buttonBottomMargin = Scale(20f);
                var buttonBottomY = Screen.height - safeBottomInset - buttonBottomMargin;
                var buttonMaxWidth = Mathf.Min(Screen.width * 0.68f, Scale(560f));
                var buttonMaxHeight = Mathf.Min(Screen.height * 0.17f, Scale(260f));
                var buttonRect = BuildBottomCenteredFittedRect(
                    _metaScreenStartButtonTexture,
                    Screen.width * 0.5f,
                    buttonBottomY,
                    buttonMaxWidth,
                    buttonMaxHeight);

                var previousButtonColor = GUI.color;
                if (!canStartFromButton)
                {
                    GUI.color = new Color(previousButtonColor.r, previousButtonColor.g, previousButtonColor.b, previousButtonColor.a * 0.45f);
                }

                GUI.DrawTexture(buttonRect, _metaScreenStartButtonTexture, ScaleMode.ScaleToFit, true);
                GUI.color = previousButtonColor;

                if (GUI.Button(buttonRect, GUIContent.none, GUIStyle.none))
                {
                    StartSelectedLevelFromMetaScreen();
                }
            }
            else
            {
                var buttonWidth = Mathf.Min(Scale(220f), Screen.width - Scale(48f));
                var buttonHeight = Mathf.Max(48f, Scale(64f));
                var buttonRect = new Rect(
                    (Screen.width - buttonWidth) * 0.5f,
                    Screen.height - safeBottomInset - buttonHeight - Scale(20f),
                    buttonWidth,
                    buttonHeight);

                if (GUI.Button(buttonRect, (_metaNextLevelIndex + 1).ToString(), _buttonStyle))
                {
                    StartSelectedLevelFromMetaScreen();
                }
            }

            GUI.enabled = previousEnabled;
        }

        private static Rect BuildBottomCenteredFittedRect(Texture2D texture, float centerX, float bottomY, float maxWidth, float maxHeight)
        {
            if (texture == null || texture.width <= 0 || texture.height <= 0)
            {
                return new Rect(centerX - (maxWidth * 0.5f), bottomY - maxHeight, maxWidth, maxHeight);
            }

            var scale = Mathf.Min(maxWidth / texture.width, maxHeight / texture.height);
            var width = texture.width * scale;
            var height = texture.height * scale;
            return new Rect(centerX - (width * 0.5f), bottomY - height, width, height);
        }

        private float GetStartScreenScientistScaleMultiplier(Texture2D texture)
        {
            if (texture != null && texture == _startScreenScientistHappyTexture)
            {
                return StartScreenHappyScientistScaleMultiplier;
            }

            return 1f;
        }

        private float GetMetaScientistScaleMultiplier(Texture2D texture, float maxWidth, float maxHeight)
        {
            var baseMultiplier = GetStartScreenScientistScaleMultiplier(texture);
            if (texture == null
                || texture != _startScreenScientistDissappointment2Texture
                || _startScreenScientistSupportTexture == null
                || maxWidth <= 0f
                || maxHeight <= 0f)
            {
                return baseMultiplier;
            }

            var supportMultiplier = GetStartScreenScientistScaleMultiplier(_startScreenScientistSupportTexture);
            var dissRect = BuildBottomCenteredFittedRect(
                texture,
                0f,
                maxHeight,
                maxWidth * baseMultiplier,
                maxHeight * baseMultiplier);
            var supportRect = BuildBottomCenteredFittedRect(
                _startScreenScientistSupportTexture,
                0f,
                maxHeight,
                maxWidth * supportMultiplier,
                maxHeight * supportMultiplier);

            if (dissRect.width <= supportRect.width && dissRect.height <= supportRect.height)
            {
                return baseMultiplier;
            }

            var widthClamp = dissRect.width > 0f ? supportRect.width / dissRect.width : 1f;
            var heightClamp = dissRect.height > 0f ? supportRect.height / dissRect.height : 1f;
            var clampFactor = Mathf.Clamp01(Mathf.Min(widthClamp, heightClamp));
            return baseMultiplier * clampFactor;
        }

        private GUIStyle GetMetaDialogueStyle(string dialogueText, Rect textRect)
        {
            if (dialogueText != MetaScreenDialogueLine3)
            {
                return _startScreenDialogueStyle;
            }

            var fallbackStyle = _startScreenDialogueLine3Style ?? _startScreenDialogueStyle;
            var fittedStyle = new GUIStyle(fallbackStyle)
            {
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true
            };
            var landscapeTextFactor = _isPortrait ? 1f : 0.9f;
            var minFontSize = Mathf.Max(10, ScaleFont(24, landscapeTextFactor));
            var content = new GUIContent(dialogueText);
            var measuredHeight = fittedStyle.CalcHeight(content, textRect.width);

            while (fittedStyle.fontSize > minFontSize && measuredHeight > textRect.height)
            {
                fittedStyle.fontSize--;
                measuredHeight = fittedStyle.CalcHeight(content, textRect.width);
            }

            return fittedStyle;
        }

        private static void DrawTextureWithAlpha(Rect rect, Texture2D texture, float alpha, ScaleMode scaleMode)
        {
            if (texture == null || alpha <= 0f)
            {
                return;
            }

            var previousColor = GUI.color;
            GUI.color = new Color(previousColor.r, previousColor.g, previousColor.b, previousColor.a * Mathf.Clamp01(alpha));
            GUI.DrawTexture(rect, texture, scaleMode, true);
            GUI.color = previousColor;
        }

        private void DrawInterlacedTextureFade(Rect rect, Texture2D texture, float fadeProgress)
        {
            if (texture == null || rect.width <= 0f || rect.height <= 0f)
            {
                return;
            }

            var clampedProgress = Mathf.Clamp01(fadeProgress);
            if (clampedProgress >= 1f)
            {
                return;
            }

            var stripeHeight = Mathf.Max(1f, Mathf.Round(Scale(4f)));
            var stripeCount = Mathf.Max(1, Mathf.CeilToInt(rect.height / stripeHeight));
            var baseAlpha = 1f - clampedProgress;

            for (var stripeIndex = 0; stripeIndex < stripeCount; stripeIndex++)
            {
                var stripeY = rect.y + (stripeIndex * stripeHeight);
                var currentStripeHeight = Mathf.Min(stripeHeight, rect.yMax - stripeY);
                if (currentStripeHeight <= 0f)
                {
                    continue;
                }

                var parityOffset = (stripeIndex & 1) == 0 ? 0f : 0.5f;
                var stripeVisibility = 1f - Mathf.Clamp01((clampedProgress * 2f) - parityOffset);
                if (stripeVisibility <= 0f)
                {
                    continue;
                }

                var stripeRect = new Rect(rect.x, stripeY, rect.width, currentStripeHeight);
                var normalizedY = (stripeY - rect.y) / rect.height;
                var normalizedHeight = currentStripeHeight / rect.height;
                var textureCoords = new Rect(0f, 1f - (normalizedY + normalizedHeight), 1f, normalizedHeight);

                var previousColor = GUI.color;
                GUI.color = new Color(
                    previousColor.r,
                    previousColor.g,
                    previousColor.b,
                    previousColor.a * baseAlpha * stripeVisibility);
                GUI.DrawTextureWithTexCoords(stripeRect, texture, textureCoords, true);
                GUI.color = previousColor;
            }
        }

        private void StartStartScreenAudio()
        {
            if ((!_isStartScreenActive && !_isMetaScreenActive) || _startScreenAudioSource == null || _startScreenClip == null)
            {
                return;
            }

            if (_startScreenAudioSource.clip != _startScreenClip)
            {
                _startScreenAudioSource.clip = _startScreenClip;
            }

            if (!_startScreenAudioSource.isPlaying)
            {
                _startScreenAudioSource.Play();
            }
        }

        private void StopStartScreenAudio()
        {
            if (_startScreenAudioSource != null && _startScreenAudioSource.isPlaying)
            {
                _startScreenAudioSource.Stop();
            }
        }

        private void UpdateResultScreenSfx()
        {
            if (_isStartScreenActive || _isMetaScreenActive)
            {
                _lastResultStatus = GameStatus.Playing;
                return;
            }

            var currentStatus = _game.Status;
            if (currentStatus == _lastResultStatus)
            {
                return;
            }

            if (_lastResultStatus == GameStatus.Playing)
            {
                if (currentStatus == GameStatus.Won)
                {
                    PlayResultSfx(_winClip);
                }
                else if (currentStatus == GameStatus.Lost)
                {
                    PlayResultSfx(_loseClip);
                }
            }

            _lastResultStatus = currentStatus;
        }

        private void PlayResultSfx(AudioClip clip)
        {
            if (_sfxSource == null || clip == null)
            {
                return;
            }

            _sfxSource.PlayOneShot(clip, ResultSfxVolume);
        }

        private void StartGameplayFromStartScreen()
        {
            if (!_isStartScreenActive)
            {
                return;
            }

            _isStartScreenActive = false;
            StopStartScreenAudio();
            _currentLevelIndex = 0;
            StartCurrentLevel();
        }

        private void StartSelectedLevelFromMetaScreen()
        {
            if (!_isMetaScreenActive)
            {
                return;
            }

            _isMetaScreenActive = false;
            _currentLevelIndex = Mathf.Clamp(_metaNextLevelIndex, 0, MaxPlayableLevels - 1);
            StartCurrentLevel();
        }

        private void SyncBackgroundMusic()
        {
            if (_bgmSource == null || _bgmClip == null)
            {
                return;
            }

            if (_isStartScreenActive || _isMetaScreenActive)
            {
                if (_bgmSource.isPlaying)
                {
                    _bgmSource.Stop();
                }

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
            var gap = Scale(Gap);
            var now = Time.unscaledTime;
            var canSelectTiles = _game.Status == GameStatus.Playing && !HasTrayInteractionLock();
            var hasActiveMixVfx = HasActiveMixVfx();
            var mixProgress = hasActiveMixVfx ? GetMixVfxProgress(now) : 1f;

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
                        if (hasActiveMixVfx)
                        {
                            TileType oldTileType;
                            if (TryGetMixOldTileType(tile.Id, out oldTileType))
                            {
                                DrawMixTileSymbolTransition(tileRect, oldTileType, tile.Type, mixProgress);
                            }
                            else
                            {
                                DrawCenteredTileSymbol(tileRect, symbolTexture);
                            }
                        }
                        else
                        {
                            DrawCenteredTileSymbol(tileRect, symbolTexture);
                        }
                    }

                    GUI.color = previousColor;
                }
            }

            if (hasActiveMixVfx)
            {
                DrawMixBoardFlash(rect, mixProgress);
            }
        }

        private void DrawControls(Rect rect)
        {
            var gap = Scale(Gap);

            var maxButtonSizeByWidth = (rect.width - (gap * 2f) - Scale(16f)) / 3f;
            var maxButtonSizeByHeight = rect.height - Scale(45f);
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

            GUI.enabled = canUseBoosters;
            if (DrawControlButton(restartRect, _restartButtonTexture, "Mix"))
            {
                TryMixBoardSymbols();
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

        private void TryMixBoardSymbols()
        {
            _mixOldTileTypesById.Clear();
            for (var i = 0; i < _game.Tiles.Count; i++)
            {
                var tile = _game.Tiles[i];
                if (tile.IsRemoved)
                {
                    continue;
                }

                _mixOldTileTypesById[tile.Id] = tile.Type;
            }

            if (!_game.TryMixBoardSymbols())
            {
                _mixOldTileTypesById.Clear();
                _isMixVfxActive = false;
                return;
            }

            _hintTileId = null;
            _hintExpiresAt = 0f;
            _isMixVfxActive = true;
            _mixVfxStartedAt = Time.unscaledTime;
        }

        private void DrawTray(Rect rect)
        {
            var gap = Scale(Gap);

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
            var maxSlotSizeByHeight = Mathf.Max(Scale(24f), rect.height - Scale(16f));
            slotSize = Mathf.Min(maxSlotSizeByHeight, (rect.width - ((capacity - 1) * gap) - Scale(12f)) / capacity);
            slotsTop = rect.y + ((rect.height - slotSize) * 0.5f);
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

        private bool HasActiveMixVfx()
        {
            return _isMixVfxActive;
        }

        private void UpdateMixVfx()
        {
            if (!_isMixVfxActive)
            {
                return;
            }

            if (Time.unscaledTime - _mixVfxStartedAt < MixVfxDurationSeconds)
            {
                return;
            }

            _isMixVfxActive = false;
            _mixOldTileTypesById.Clear();
        }

        private bool HasTrayInteractionLock()
        {
            return HasActiveTileFlights() ||
                   HasActiveTrayShiftVfx() ||
                   HasActiveTrayMatchVfx() ||
                   HasActiveTrayCompactVfx() ||
                   HasActiveMixVfx() ||
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
            PlayTileTouchSfx();
        }

        private void PlayTileTouchSfx()
        {
            if (_sfxSource == null || _tileTouchClip == null)
            {
                return;
            }

            _sfxSource.PlayOneShot(_tileTouchClip);
        }

        private void PlayMatch3Sfx()
        {
            if (_sfxSource == null || _match3Clip == null)
            {
                return;
            }

            _sfxSource.PlayOneShot(_match3Clip, Match3SfxVolume);
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

            PlayMatch3Sfx();
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

        private void DrawOverlay(Rect topRect, Rect trayRect, Rect controlsRect)
        {
            if (_game.Status == GameStatus.Playing)
            {
                return;
            }

            var safeTop = topRect.yMax + Scale(8f);
            var safeBottom = Mathf.Min(trayRect.yMin, controlsRect.yMin) - Scale(8f);
            var safeHeight = Mathf.Max(Scale(140f), safeBottom - safeTop);

            var resultTexture = _game.Status == GameStatus.Won ? _winWindowTexture : _loseWindowTexture;
            if (resultTexture == null)
            {
                DrawOverlayFallback(safeTop, safeBottom, safeHeight);
                return;
            }

            var maxPanelWidth = Screen.width - Scale(40f);
            var maxPanelHeight = safeHeight - Scale(8f);
            var scaleByWidth = resultTexture.width > 0 ? maxPanelWidth / resultTexture.width : 1f;
            var scaleByHeight = resultTexture.height > 0 ? maxPanelHeight / resultTexture.height : 1f;
            var textureScale = Mathf.Min(scaleByWidth, scaleByHeight);
            var panelWidth = Mathf.Max(Scale(160f), resultTexture.width * textureScale);
            var panelHeight = Mathf.Max(Scale(120f), resultTexture.height * textureScale);
            var centerY = (safeTop + safeBottom) * 0.5f;
            var panelY = centerY - (panelHeight * 0.5f);
            panelY = Mathf.Clamp(panelY, safeTop, safeBottom - panelHeight);

            var panelRect = new Rect(
                (Screen.width - panelWidth) * 0.5f,
                panelY,
                panelWidth,
                panelHeight);

            GUI.DrawTexture(panelRect, resultTexture, ScaleMode.ScaleToFit, true);

            if (GUI.Button(panelRect, GUIContent.none, GUIStyle.none))
            {
                HandleResultOverlayTap();
            }
        }

        private void DrawOverlayFallback(float safeTop, float safeBottom, float safeHeight)
        {
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
            GUI.Label(titleRect, title, _titleStyle);

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
                HandleResultOverlayTap();
            }
        }

        private void HandleResultOverlayTap()
        {
            var playedLevelIndex = _currentLevelIndex;
            var playedLevelNumber = playedLevelIndex + 1;

            if (_game.Status == GameStatus.Won)
            {
                _highestCompletedLevel = Mathf.Clamp(
                    Mathf.Max(_highestCompletedLevel, playedLevelNumber),
                    0,
                    MaxPlayableLevels);

                if (playedLevelIndex == 0)
                {
                    OpenMetaScreenAfterLevel1Win();
                    return;
                }

                var nextLevelIndex = Mathf.Min(playedLevelIndex + 1, MaxPlayableLevels - 1);
                OpenMetaScreen(nextLevelIndex, useIntroFlow: false);
                return;
            }

            if (playedLevelIndex == 0)
            {
                StartCurrentLevel();
                return;
            }

            OpenMetaScreen(playedLevelIndex, useIntroFlow: false);
        }

        private void StartCurrentLevel()
        {
            _isStartScreenActive = false;
            InitializeMetaScreenState();
            StopStartScreenAudio();
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
            _mixOldTileTypesById.Clear();
            _isMixVfxActive = false;
            _mixVfxStartedAt = 0f;
            _flightSequence = 0;
            _trayMatchVfxSeed = 0;
            _lastResultStatus = GameStatus.Playing;

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

        private bool TryGetMixOldTileType(int tileId, out TileType oldTileType)
        {
            return _mixOldTileTypesById.TryGetValue(tileId, out oldTileType);
        }

        private float GetMixVfxProgress(float now)
        {
            if (!_isMixVfxActive)
            {
                return 1f;
            }

            return Mathf.Clamp01((now - _mixVfxStartedAt) / MixVfxDurationSeconds);
        }

        private void DrawMixTileSymbolTransition(Rect tileRect, TileType oldTileType, TileType newTileType, float mixProgress)
        {
            var oldSymbolTexture = GetTileSymbolTexture(oldTileType);
            var newSymbolTexture = GetTileSymbolTexture(newTileType);
            if (oldSymbolTexture == null && newSymbolTexture == null)
            {
                return;
            }

            var progress = Mathf.Clamp01(mixProgress);
            var pulse = 1f + Mathf.Sin(progress * Mathf.PI) * MixSymbolPulseScale;
            var oldAlpha = 1f - progress;
            var newAlpha = progress;

            if (oldSymbolTexture != null && oldAlpha > 0.001f)
            {
                DrawCenteredTileSymbol(tileRect, oldSymbolTexture, oldAlpha, pulse * 1.04f);
            }

            if (newSymbolTexture != null && newAlpha > 0.001f)
            {
                DrawCenteredTileSymbol(tileRect, newSymbolTexture, newAlpha, pulse);
            }
        }

        private void DrawMixBoardFlash(Rect boardRect, float mixProgress)
        {
            var flashRatio = Mathf.Clamp01(MixFlashPhaseRatio);
            if (flashRatio <= 0f)
            {
                return;
            }

            var flashProgress = Mathf.Clamp01(mixProgress / flashRatio);
            var intensity = 1f - flashProgress;
            if (intensity <= 0.001f)
            {
                return;
            }

            var pulse = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 26f);
            var previousColor = GUI.color;

            GUI.color = new Color(0.36f, 0.86f, 1f, Mathf.Lerp(0.11f, 0.2f, pulse) * intensity);
            GUI.DrawTexture(boardRect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true);

            GUI.color = new Color(1f, 1f, 1f, Mathf.Lerp(0.06f, 0.15f, pulse) * intensity);
            GUI.DrawTexture(boardRect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true);

            GUI.color = previousColor;
        }

        private void DrawCenteredTileSymbol(Rect hostRect, Texture2D symbolTexture, float alpha = 1f, float scale = 1f)
        {
            if (symbolTexture == null)
            {
                return;
            }

            var iconSize = Mathf.Min(hostRect.width, hostRect.height) * TileIconSizeFactor * Mathf.Max(0.01f, scale);
            if (iconSize <= 0f)
            {
                return;
            }

            var iconRect = new Rect(
                hostRect.x + ((hostRect.width - iconSize) * 0.5f),
                hostRect.y + ((hostRect.height - iconSize) * 0.5f),
                iconSize,
                iconSize);

            var previousColor = GUI.color;
            var clampedAlpha = Mathf.Clamp01(alpha);
            GUI.color = new Color(previousColor.r, previousColor.g, previousColor.b, previousColor.a * clampedAlpha);
            GUI.DrawTexture(iconRect, symbolTexture, ScaleMode.ScaleToFit, true);
            GUI.color = previousColor;
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

            _topLevelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = ScaleFont(90, landscapeTextFactor),
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

            var dialogueFontSize = ScaleFont(68, landscapeTextFactor);
            _startScreenDialogueStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = dialogueFontSize,
                fontStyle = FontStyle.Bold,
                wordWrap = true
            };
            _startScreenDialogueStyle.normal.textColor = new Color(0.12f, 0.15f, 0.2f, 1f);
            _startScreenDialogueStyle.active.textColor = _startScreenDialogueStyle.normal.textColor;
            _startScreenDialogueStyle.focused.textColor = _startScreenDialogueStyle.normal.textColor;
            _startScreenDialogueStyle.hover.textColor = _startScreenDialogueStyle.normal.textColor;

            _startScreenDialogueLine3Style = new GUIStyle(_startScreenDialogueStyle)
            {
                fontSize = Mathf.Max(10, Mathf.RoundToInt(dialogueFontSize * 0.7f))
            };
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
                DrawHintGlow(tileRect);
            }

            GUI.color = previousColor;
        }

        private void DrawHintGlow(Rect tileRect)
        {
            var pulse = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 7.5f);

            DrawHintGlowLayer(
                ExpandRect(tileRect, Scale(Mathf.Lerp(9f, 14f, pulse))),
                new Color(0.35f, 0.9f, 1f, Mathf.Lerp(0.18f, 0.34f, pulse)));

            DrawHintGlowLayer(
                ExpandRect(tileRect, Scale(Mathf.Lerp(5f, 8f, pulse))),
                new Color(0.72f, 0.96f, 1f, Mathf.Lerp(0.2f, 0.38f, pulse)));

            DrawHintGlowLayer(
                ExpandRect(tileRect, Scale(Mathf.Lerp(1f, 3f, pulse))),
                new Color(1f, 1f, 1f, Mathf.Lerp(0.2f, 0.4f, pulse)));
        }

        private void DrawHintGlowLayer(Rect rect, Color color)
        {
            if (_tileTexture != null)
            {
                DrawCroppedTileBase(rect, color);
                return;
            }

            var previousColor = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true);
            GUI.color = previousColor;
        }

        private static Rect ExpandRect(Rect rect, float amount)
        {
            return new Rect(
                rect.x - amount,
                rect.y - amount,
                rect.width + amount * 2f,
                rect.height + amount * 2f);
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
