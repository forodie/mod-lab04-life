using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Newtonsoft.Json;
using System.Text.Json.Serialization;
using System.Xml;

namespace cli_life
{
    public class GameSettings
    {
        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("cellSize")]
        public int CellSize { get; set; }

        [JsonProperty("liveDensity")]
        public double LiveDensity { get; set; }

        [JsonProperty("simulationSpeed")]
        public int SimulationSpeed { get; set; }
    }

    public class Cell
    {
        public bool IsAlive;
        public readonly List<Cell> neighbors = new List<Cell>();
        private bool IsAliveNext;
        public void DetermineNextLiveState()
        {
            int liveNeighbors = neighbors.Where(x => x.IsAlive).Count();
            if (IsAlive)
                IsAliveNext = liveNeighbors == 2 || liveNeighbors == 3;
            else
                IsAliveNext = liveNeighbors == 3;
        }
        public void Advance()
        {
            IsAlive = IsAliveNext;
        }
    }
    public class Board
    {
        public readonly Cell[,] Cells;
        public readonly int CellSize;

        public int Columns { get { return Cells.GetLength(0); } }
        public int Rows { get { return Cells.GetLength(1); } }
        public int Width { get { return Columns * CellSize; } }
        public int Height { get { return Rows * CellSize; } }

        public Board(int width, int height, int cellSize, double liveDensity = .1)
        {
            CellSize = cellSize;

            Cells = new Cell[width / cellSize, height / cellSize];
            for (int x = 0; x < Columns; x++)
                for (int y = 0; y < Rows; y++)
                    Cells[x, y] = new Cell();

            ConnectNeighbors();
            Randomize(liveDensity);
        }

        readonly Random rand = new Random();
        public void Randomize(double liveDensity)
        {
            foreach (var cell in Cells)
                cell.IsAlive = rand.NextDouble() < liveDensity;
        }

        public void Advance()
        {
            foreach (var cell in Cells)
                cell.DetermineNextLiveState();
            foreach (var cell in Cells)
                cell.Advance();
        }
        private void ConnectNeighbors()
        {
            for (int x = 0; x < Columns; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    int xL = (x > 0) ? x - 1 : Columns - 1;
                    int xR = (x < Columns - 1) ? x + 1 : 0;

                    int yT = (y > 0) ? y - 1 : Rows - 1;
                    int yB = (y < Rows - 1) ? y + 1 : 0;

                    Cells[x, y].neighbors.Add(Cells[xL, yT]);
                    Cells[x, y].neighbors.Add(Cells[x, yT]);
                    Cells[x, y].neighbors.Add(Cells[xR, yT]);
                    Cells[x, y].neighbors.Add(Cells[xL, y]);
                    Cells[x, y].neighbors.Add(Cells[xR, y]);
                    Cells[x, y].neighbors.Add(Cells[xL, yB]);
                    Cells[x, y].neighbors.Add(Cells[x, yB]);
                    Cells[x, y].neighbors.Add(Cells[xR, yB]);
                }
            }
        }

        public void SaveToFile(string fileName)
        {
            StringBuilder sb = new StringBuilder();
            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Columns; col++)
                {
                    sb.Append(Cells[col, row].IsAlive ? '*' : ' ');
                }
                sb.AppendLine();
            }
            File.WriteAllText(fileName, sb.ToString());
        }

        public void LoadFromFile(string fileName)
        {
            if (File.Exists(fileName))
            {
                string[] lines = File.ReadAllLines(fileName);
                for (int row = 0; row < Rows && row < lines.Length; row++)
                {
                    for (int col = 0; col < Columns && col < lines[row].Length; col++)
                    {
                        Cells[col, row].IsAlive = (lines[row][col] == '*');
                    }
                }
            }
        }
        public int CountLiveCells()
        {
            int liveCells = 0;
            foreach (var cell in Cells)
            {
                if (cell.IsAlive)
                {
                    liveCells++;
                }
            }
            return liveCells;
        }
        public List<Pattern> ClassifyElements(Patterns patterns)
        {
            return patterns.MatchPatterns(this);
        }

    }

    public class Pattern
    {
        public string Name { get; set; }
        public bool[,] Shape { get; set; }

        public Pattern(string name, bool[,] shape)
        {
            Name = name;
            Shape = shape;
        }
    }

    public class Patterns
    {
        public List<Pattern> PatternList { get; set; }

        public Patterns()
        {
            PatternList = new List<Pattern>();

            // стандартные образцы
            PatternList.Add(new Pattern("Block", new bool[,]
            {
            { true, true },
            { true, true }
            }));

            PatternList.Add(new Pattern("Blinker", new bool[,]
            {
            { true, true, true }
            }));
        }

        public List<Pattern> MatchPatterns(Board board)
        {
            List<Pattern> matchedPatterns = new List<Pattern>();

            foreach (var pattern in PatternList)
            {
                for (int x = 0; x < board.Columns; x++)
                {
                    for (int y = 0; y < board.Rows; y++)
                    {
                        if (IsPatternMatch(pattern, board, x, y))
                        {
                            matchedPatterns.Add(pattern);
                        }
                    }
                }
            }

            return matchedPatterns;
        }
        private bool IsPatternMatch(Pattern pattern, Board board, int startX, int startY)
        {
            for (int x = 0; x < pattern.Shape.GetLength(0); x++)
            {
                for (int y = 0; y < pattern.Shape.GetLength(1); y++)
                {
                    int boardX = (startX + x) % board.Columns;
                    int boardY = (startY + y) % board.Rows;

                    if (board.Cells[boardX, boardY].IsAlive != pattern.Shape[x, y])
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }

    class Program
    {
        static Board board;
        static private void Reset(int width, int height, int cellSize, double liveDensity)
        {
            board = new Board(
                width: 50,
                height: 20,
                cellSize: 1,
                liveDensity: 0.5);
        }
        static void Render()
        {
            for (int row = 0; row < board.Rows; row++)
            {
                for (int col = 0; col < board.Columns; col++)
                {
                    var cell = board.Cells[col, row];
                    if (cell.IsAlive)
                    {
                        Console.Write('*');
                    }
                    else
                    {
                        Console.Write(' ');
                    }
                }
                Console.Write('\n');
            }
        }
        static int CountSymmetricElements()
        {
            int symmetricElements = 0;

            for (int row = 0; row < board.Rows; row++)
            {
                for (int col = 0; col < board.Columns / 2; col++)
                {
                    var cellLeft = board.Cells[col, row];
                    var cellRight = board.Cells[board.Columns - col - 1, row];

                    if (cellLeft.IsAlive == cellRight.IsAlive)
                    {
                        symmetricElements++;
                    }
                }
            }

            return symmetricElements;
        }

        static void Main(string[] args)
        {
            Patterns patterns = new Patterns();
            int generationCount = 0;
            // Загрузка настроек из файла
            string settingsFile = "settings.json";
            GameSettings settings;
            if (File.Exists(settingsFile))
            {
                string jsonString = File.ReadAllText(settingsFile);
                settings = JsonConvert.DeserializeObject<GameSettings>(jsonString);
            }
            else
            {
                settings = new GameSettings
                {
                    Width = 50,
                    Height = 20,
                    CellSize = 1,
                    LiveDensity = 0.5,
                    SimulationSpeed = 1000
                };
                string jsonString = JsonConvert.SerializeObject(settings, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(settingsFile, jsonString);
            }

            // Инициализация игры с настройками
            Reset(settings.Width, settings.Height, settings.CellSize, settings.LiveDensity);

            // Основной цикл игры
            ConsoleKeyInfo keyInfo;
            do
            {
                Console.Clear();
                Render();
                board.Advance();
                generationCount++;
                Thread.Sleep(settings.SimulationSpeed);
                // Вызовите методы для подсчета живых клеток, классификации элементов и подсчета симметричных элементов
                int liveCells = board.CountLiveCells();
                List<Pattern> matchedPatterns = board.ClassifyElements(patterns);
                int symmetricElements = CountSymmetricElements();

                // Выведите результаты на консоль или сохраните их для дальнейшего анализа
                Console.WriteLine($"Поколение: {generationCount}");
                Console.WriteLine($"Живые клетки: {liveCells}");
                Console.WriteLine($"Сопоставленные образцы: {matchedPatterns.Count}");
                Console.WriteLine($"Симметричные элементы: {symmetricElements}");

                if (Console.KeyAvailable)
                {
                    keyInfo = Console.ReadKey(intercept: true);
                    if (keyInfo.Key == ConsoleKey.S)
                    {
                        Console.Write("Введите имя файла для сохранения: ");
                        string fileName = Console.ReadLine();
                        board.SaveToFile(fileName);
                        Console.WriteLine("Состояние сохранено.");
                        Thread.Sleep(2000);
                    }
                    else if (keyInfo.Key == ConsoleKey.L)
                    {
                        Console.Write("Введите имя файла для загрузки: ");
                        string fileName = Console.ReadLine();
                        board.LoadFromFile(fileName);
                        Console.WriteLine("Состояние загружено.");
                        Thread.Sleep(2000);
                    }
                }
                else
                {
                    keyInfo = new ConsoleKeyInfo();
                }
            }
            while (keyInfo.Key != ConsoleKey.Escape);
        }
    }
}