using System.Runtime.InteropServices;
using System.Text;

namespace Engine
{
    public static class Screen
    {
        public static Vector2i WindowSize { get; private set; }
        public static string Title
        {
            get => Console.Title;
            set => Console.Title = value;
        }

        public static event Action<ConsoleKeyInfo>? OnKeyPressed;
        public static event Action<float>? OnDraw;

        public static bool IsOpen { get; private set; }

        private static char[]? _symbols;
        private static byte[]? _pixels;
        private static StringBuilder? _outputBuffer;

        public static void Initialize(int width, int height, string title)
        {
            WindowSize = new(width, height);
            Title = title;

            _pixels = new byte[width * height * 3];
            Array.Clear(_pixels, 0, _pixels.Length);

            _symbols = new char[width * height];
            Array.Fill(_symbols, ' ');

            _outputBuffer = new(width * height * 20);

            Console.SetWindowSize(width, height);
            Console.SetBufferSize(width, height);

            EnableAnsiCodes();

            Console.Clear();
            Console.CursorVisible = false;

            IsOpen = true;

            DateTime time = DateTime.Now;
            float deltaTime = 0;

            while (IsOpen)
            {
                if (Console.KeyAvailable) OnKeyPressed?.Invoke(Console.ReadKey(true));

                Array.Clear(_pixels, 0, _pixels.Length);
                Array.Fill(_symbols, ' ');
                _outputBuffer.Clear();

                OnDraw?.Invoke(deltaTime);

                _outputBuffer.Append($"\x1b[H\x1b[0m");
                for (int y = 0; y < WindowSize.Y; y++)
                {
                    for (int x = 0; x < WindowSize.X; x++)
                    {
                        var symbol = GetSymbol(x, y);
                        var color = GetPixel(x, y);

                        _outputBuffer.Append($"\x1b[48;2;" +
                            $"{color.R};{color.G};{color.B}m{symbol}\x1b[0m");
                    }
                    if (y < WindowSize.Y - 1) _outputBuffer.AppendLine();
                }

                Console.Write(_outputBuffer.ToString());

                deltaTime = (float)(DateTime.Now - time).TotalSeconds;
                time = DateTime.Now;
            }
        }

        public static void Close() => IsOpen = false;

        public static void Text(int left, int top, string text)
        {
            if (_symbols == null || _pixels == null) return;

            for (int i = 0; i < text.Length; i++)
            {
                int newLeft = left + i;
                SetPixel(newLeft, top, Colors.Black);
                SetSymbol(newLeft, top, text[i]);
            }
        }

        public static void Line(Vertex start, Vertex end)
        {
            int x0 = start.Position.X;
            int y0 = start.Position.Y;
            int x1 = end.Position.X;
            int y1 = end.Position.Y;

            int dx = Math.Abs(x1 - x0);
            int dy = Math.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;

            float distance = Vertex.Distance(start, end);

            while (true)
            {
                float currentDistance = Vector2.Distance(new(x0, y0), end.Position);
                Color color = Color.Mix(start.Color, end.Color, currentDistance / distance);
                SetPixel(x0, y0, color);

                if (x0 == x1 && y0 == y1) break;

                int e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    x0 += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    y0 += sy;
                }
            }
        }

        public static void ConvexShape(params Vertex[] vertices)
        {
            if (vertices.Length < 3) return;

            var minY = vertices.Min(p => p.Position.Y);
            var maxY = vertices.Max(p => p.Position.Y);

            for (int y = minY; y <= maxY; y++)
            {
                List<(int X, Color Color)> intersections = new();
                for (int i = 0; i < vertices.Length; i++)
                {
                    var p1 = vertices[i];
                    var p2 = vertices[(i + 1) % vertices.Length];

                    if (p1.Position.Y == p2.Position.Y) continue;

                    if (y < Math.Min(p1.Position.Y, p2.Position.Y) ||
                        y > Math.Max(p1.Position.Y, p2.Position.Y)) continue;

                    float t = (float)(y - p1.Position.Y) / (p2.Position.Y - p1.Position.Y);
                    int x = (int)(p1.Position.X + t * (p2.Position.X - p1.Position.X));

                    var color = Color.Mix(p1.Color, p2.Color, t);
                    intersections.Add((x, color));
                }

                intersections.Sort((a, b) => a.X.CompareTo(b.X));

                for (int i = 0; i < intersections.Count - 1; i += 2)
                {
                    int xStart = intersections[i].X;
                    int xEnd = intersections[i + 1].X;

                    var colorStart = intersections[i].Color;
                    var colorEnd = intersections[i + 1].Color;

                    float length = xEnd - xStart;
                    for (int x = xStart; x <= xEnd; x++)
                    {
                        float t = (x - xStart) / length;
                        var color = Color.Mix(colorStart, colorEnd, t);
                        SetPixel(x, y, color);
                    }
                }
            }
        }

        public static void SetPixel(int left, int top, Color color)
        {
            if (_pixels == null) return;

            var index = GetPixelIndex(left, top);
            _pixels[index] = color.R;
            _pixels[index + 1] = color.G;
            _pixels[index + 2] = color.B;
        }
        public static Color GetPixel(int left, int top)
        {
            if (_pixels == null) return (0, 0, 0);

            var index = GetPixelIndex(left, top);
            return new(_pixels[index], _pixels[index + 1], _pixels[index + 2]);
        }
        public static int GetPixelIndex(int left, int top) =>
            ((top * WindowSize.X) + left) * 3;

        public static void SetSymbol(int left, int top, char symbol)
        {
            if (_symbols == null) return;

            var index = GetSymbolIndex(left, top);
            _symbols[index] = symbol;
        }
        public static char GetSymbol(int left, int top)
        {
            if (_symbols == null) return ' ';

            var index = GetSymbolIndex(left, top);
            return _symbols[index];
        }
        public static int GetSymbolIndex(int left, int top) =>
            (top * WindowSize.X) + left;

        private static void EnableAnsiCodes()
        {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT) return;

            var handle = GetStdHandle(-11);
            _ = GetConsoleMode(handle, out uint mode);
            _ = SetConsoleMode(handle, mode | 0x0004);
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);
    }

    public struct Vertex(Vector2i position, Color color)
    {
        public Vector2i Position = position;
        public Color Color = color;

        public Vertex(int x, int y, Color color) : this(new(x, y), color) { }

        public static float Distance(Vertex v1, Vertex v2) =>
            Vector2.Distance(v1.Position, v2.Position);
    }
}