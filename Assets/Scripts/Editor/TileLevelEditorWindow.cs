using System.IO;
using System.Text.RegularExpressions;
using Tiles.Core;
using UnityEditor;
using UnityEngine;

namespace Tiles.Editor
{
    public sealed class TileLevelEditorWindow : EditorWindow
    {
        private const int GridColumns = 6;
        private const int GridRows = 6;
        private const int CellCount = GridColumns * GridRows;
        private const int MaxSymbolsOnLevel = 26;
        private const int MaxStackHeight = 9;
        private const string LevelsFolderPath = "Assets/Resources/Levels";
        private static readonly Regex LevelIdRegex = new Regex(@"^\d{3}$");

        private readonly int[] _sectorStacks = new int[CellCount];
        private string _levelId = "001";
        private int _symbolsCount;
        private string _statusMessage = "Заполните сетку и сохраните уровень.";
        private MessageType _statusType = MessageType.Info;

        [MenuItem("Tools/Tiles/Level Editor")]
        private static void OpenWindow()
        {
            var window = GetWindow<TileLevelEditorWindow>("Tile Level Editor");
            window.minSize = new Vector2(460f, 420f);
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Tile Level Editor (6x6)", EditorStyles.boldLabel);
            EditorGUILayout.Space(4f);

            _levelId = EditorGUILayout.TextField("Level ID (NNN)", _levelId);
            _symbolsCount = EditorGUILayout.IntField("Symbols Count", _symbolsCount);
            _symbolsCount = Mathf.Clamp(_symbolsCount, 0, MaxSymbolsOnLevel);

            EditorGUILayout.Space(8f);
            DrawGrid();
            EditorGUILayout.Space(8f);
            DrawSummary();
            EditorGUILayout.Space(8f);
            DrawButtons();
            EditorGUILayout.Space(8f);
            EditorGUILayout.HelpBox(_statusMessage, _statusType);
        }

        private void DrawGrid()
        {
            EditorGUILayout.LabelField("Секторы 6x6 (значения 0..9)", EditorStyles.boldLabel);
            for (var row = 0; row < GridRows; row++)
            {
                EditorGUILayout.BeginHorizontal();
                for (var column = 0; column < GridColumns; column++)
                {
                    var index = row * GridColumns + column;
                    var value = EditorGUILayout.IntField(_sectorStacks[index], GUILayout.Width(54f));
                    _sectorStacks[index] = Mathf.Clamp(value, 0, MaxStackHeight);
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawSummary()
        {
            var tileCount = GetTileCount();
            var nonEmptySectors = GetNonEmptySectorsCount();
            var maxLayer = GetMaxStackHeight();

            EditorGUILayout.LabelField("Tiles:", tileCount.ToString());
            EditorGUILayout.LabelField("Non-empty sectors:", nonEmptySectors.ToString());
            EditorGUILayout.LabelField("Max stack height:", maxLayer.ToString());
        }

        private void DrawButtons()
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Создать новый уровень", GUILayout.Height(28f)))
            {
                CreateNewLevel();
            }

            if (GUILayout.Button("Сохранить уровень", GUILayout.Height(28f)))
            {
                SaveLevel();
            }

            if (GUILayout.Button("Загрузить уровень", GUILayout.Height(28f)))
            {
                LoadLevel();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void CreateNewLevel()
        {
            for (var i = 0; i < _sectorStacks.Length; i++)
            {
                _sectorStacks[i] = 0;
            }

            _symbolsCount = 0;
            SetStatus("Новый уровень создан: сетка и symbols очищены.", MessageType.Info);
        }

        private void SaveLevel()
        {
            string validationError;
            if (!TryValidateLevelId(_levelId, out validationError))
            {
                SetStatus(validationError, MessageType.Error);
                return;
            }

            if (!TryValidateCurrentData(out validationError))
            {
                SetStatus(validationError, MessageType.Error);
                return;
            }

            Directory.CreateDirectory(LevelsFolderPath);
            var filePath = Path.Combine(LevelsFolderPath, _levelId + ".json");

            var data = new TileLevelFileData
            {
                symbolsCount = _symbolsCount,
                sectorStacks = CloneSectorStacks()
            };

            var json = JsonUtility.ToJson(data, true);
            File.WriteAllText(filePath, json);
            AssetDatabase.Refresh();

            SetStatus("Уровень " + _levelId + " сохранён: " + filePath, MessageType.Info);
        }

        private void LoadLevel()
        {
            string validationError;
            if (!TryValidateLevelId(_levelId, out validationError))
            {
                SetStatus(validationError, MessageType.Error);
                return;
            }

            var filePath = Path.Combine(LevelsFolderPath, _levelId + ".json");
            if (!File.Exists(filePath))
            {
                SetStatus("Файл уровня не найден: " + filePath, MessageType.Error);
                return;
            }

            var json = File.ReadAllText(filePath);
            var data = JsonUtility.FromJson<TileLevelFileData>(json);
            if (data == null || data.sectorStacks == null || data.sectorStacks.Length != CellCount)
            {
                SetStatus("Некорректный формат файла уровня: " + filePath, MessageType.Error);
                return;
            }

            _symbolsCount = Mathf.Clamp(data.symbolsCount, 0, MaxSymbolsOnLevel);
            for (var i = 0; i < _sectorStacks.Length; i++)
            {
                _sectorStacks[i] = Mathf.Clamp(data.sectorStacks[i], 0, MaxStackHeight);
            }

            SetStatus("Уровень " + _levelId + " загружен.", MessageType.Info);
        }

        private bool TryValidateCurrentData(out string error)
        {
            error = null;

            var tileCount = GetTileCount();
            if (tileCount <= 0)
            {
                error = "Сумма плиток должна быть больше нуля.";
                return false;
            }

            if (tileCount % 3 != 0)
            {
                error = "Сумма плиток по 36 секторам должна быть кратна 3.";
                return false;
            }

            if (_symbolsCount < 1 || _symbolsCount > MaxSymbolsOnLevel)
            {
                error = "Symbols Count должен быть в диапазоне 1.." + MaxSymbolsOnLevel + ".";
                return false;
            }

            var groupsCount = tileCount / 3;
            if (_symbolsCount > groupsCount)
            {
                error = "Symbols Count не может быть больше tileCount/3.";
                return false;
            }

            return true;
        }

        private static bool TryValidateLevelId(string levelId, out string error)
        {
            error = null;
            if (string.IsNullOrWhiteSpace(levelId) || !LevelIdRegex.IsMatch(levelId))
            {
                error = "Level ID должен состоять ровно из 3 цифр (например: 001).";
                return false;
            }

            return true;
        }

        private int GetTileCount()
        {
            var tileCount = 0;
            for (var i = 0; i < _sectorStacks.Length; i++)
            {
                tileCount += _sectorStacks[i];
            }

            return tileCount;
        }

        private int GetNonEmptySectorsCount()
        {
            var count = 0;
            for (var i = 0; i < _sectorStacks.Length; i++)
            {
                if (_sectorStacks[i] > 0)
                {
                    count++;
                }
            }

            return count;
        }

        private int GetMaxStackHeight()
        {
            var max = 0;
            for (var i = 0; i < _sectorStacks.Length; i++)
            {
                if (_sectorStacks[i] > max)
                {
                    max = _sectorStacks[i];
                }
            }

            return max;
        }

        private int[] CloneSectorStacks()
        {
            var clone = new int[_sectorStacks.Length];
            for (var i = 0; i < _sectorStacks.Length; i++)
            {
                clone[i] = _sectorStacks[i];
            }

            return clone;
        }

        private void SetStatus(string message, MessageType messageType)
        {
            _statusMessage = message;
            _statusType = messageType;
        }
    }
}
