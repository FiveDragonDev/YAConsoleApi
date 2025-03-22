using System.Runtime.InteropServices;
using System.Text;

namespace Engine
{
    public static class Screen
    {
        public static uint Frames { get; private set; }
        public static float Time { get; private set; }

        public static Vector2i WindowSize { get; private set; }
        public static string Title
        {
            get => Console.Title;
            set => Console.Title = value;
        }

        public static Color BackgroundColor { get; set; } = Colors.Black;

        public static event Action<ConsoleKeyInfo>? OnKeyPressed;
        public static event Action<float>? OnDraw;
        public static event Action? OnClose;

        public static bool IsOpen { get; private set; }

        private static char[]? _symbols;
        private static byte[]? _pixels;

        private static HashSet<int>? _changedPixels;

        private static StringBuilder? _outputBuffer;
        private static readonly HashSet<float> _frametime = [];

        private static readonly Dictionary<(Color, char), string> _colorCache = [];

        /// <summary>
        /// Initializes screen with yours params
        /// </summary>
        /// <param name="width">Screen width</param>
        /// <param name="height">Screen height</param>
        /// <param name="title">Screen title</param>
        public static void Initialize(int width, int height, string title)
        {
            var uptime = DateTime.Now;
            WindowSize = new(width, height);
            Title = title;

            Console.SetWindowSize(width, height);
            Console.SetBufferSize(width, height);
            Console.Clear();
            Console.CursorVisible = false;
            EnableAnsiCodes();

            InitializeArrays(width, height);

            IsOpen = true;

            Loger.Log($"Screen was initialized (" +
                $"Resolution: {WindowSize} symbols; " +
                $"Uptime: {(DateTime.Now - uptime).TotalMilliseconds:0.000} ms)");

            RunMainLoop();
        }
        /// <summary>
        /// Closes screen
        /// </summary>
        public static void Shutdown()
        {
            IsOpen = false;

            Frames = 0;
            Time = 0;
            OnClose?.Invoke();

            Loger.Log($"Screen was closed");
            Loger.Log($"Frametime: (" +
                $"Max: {_frametime.Max():0.000} ms; " +
                $"Min: {_frametime.Min():0.000} ms; " +
                $"Average: {_frametime.Average():0.000} ms)");

            Loger.Shutdown();
        }

        private static void DrawScreen()
        {
            if (_outputBuffer == null || _changedPixels == null) return;

            if (_changedPixels.Count == 0)
            {
                Console.Write(_outputBuffer.ToString());
                return;
            }

            _outputBuffer.Clear();
            _ = _outputBuffer.Append("\x1b[H\x1b[0m");

            var pixelCodes = new string[WindowSize.X * WindowSize.Y];
            string background = GetColorCode(BackgroundColor, ' ');

            Parallel.ForEach(_changedPixels, index =>
            {
                int x = index % WindowSize.X;
                int y = index / WindowSize.X;

                var symbol = GetSymbol(x, y);
                var color = GetPixel(x, y);

                pixelCodes[index] = GetColorCode(color, symbol);
            });

            _ = Parallel.For(0, pixelCodes.Length, i =>
            {
                if (pixelCodes[i] == null)
                {
                    pixelCodes[i] = background;
                }
            });

            _ = _outputBuffer.Append(string.Join(string.Empty, pixelCodes));
            _changedPixels.Clear();
            Console.Write(_outputBuffer.ToString());
        }

        #region Simple shapes
        /// <summary>
        /// Draws text on the screen
        /// </summary>
        /// <param name="left">Left offset</param>
        /// <param name="top">Top offset</param>
        /// <param name="text">Text that will rendered</param>
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

        /// <summary>
        /// Draws line on the screen
        /// </summary>
        /// <param name="start">Start point of line</param>
        /// <param name="end">End point of line</param>
        public static void Line(Vertex start, Vertex end)
        {
            if (_pixels == null) return;

            int x0 = start.Position.X;
            int y0 = start.Position.Y;
            int x1 = end.Position.X;
            int y1 = end.Position.Y;

            int dx = (int)MathF.Abs(x1 - x0);
            int dy = (int)MathF.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;

            float distance = Vertex.Distance(start, end);

            while (true)
            {
                var currentDistance = Vector2.Distance(new(x0, y0), end.Position);
                var color = Color.Mix(start.Color, end.Color, currentDistance / distance);
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

        /// <summary>
        /// Draws the convex shape on the screen
        /// </summary>
        /// <param name="vertices">Points that will used to draw a N-gon</param>
        public static void ConvexShape(VertexArray vertices)
        {
            if (vertices.Count < 3 || _pixels == null) return;

            var minY = vertices.Min(p => p.Position.Y);
            var maxY = vertices.Max(p => p.Position.Y);

            for (int y = minY; y <= maxY; y++)
            {
                List<(int X, Color Color)> intersections = [];
                for (int i = 0; i < vertices.Count; i++)
                {
                    var p1 = vertices[i];
                    var p2 = vertices[(i + 1) % vertices.Count];

                    if (p1.Position.Y == p2.Position.Y) continue;

                    if (y < MathF.Min(p1.Position.Y, p2.Position.Y) ||
                        y > MathF.Max(p1.Position.Y, p2.Position.Y)) continue;

                    float t = (float)(y - p1.Position.Y) / (p2.Position.Y - p1.Position.Y);
                    int x = (int)(p1.Position.X + (t * (p2.Position.X - p1.Position.X)));

                    var color = Color.Mix(p1.Color, p2.Color, t);
                    intersections.Add((x, color));
                }

                intersections.Sort((a, b) => a.X.CompareTo(b.X));

                for (int i = 0; i < intersections.Count - 1; i++)
                {
                    var start = intersections[i];
                    var end = intersections[i + 1];

                    var length = end.X - start.X;
                    if (length == 0) continue;

                    for (int x = start.X; x <= end.X; x++)
                    {
                        float t = (float)(x - start.X) / length;
                        var color = Color.Mix(start.Color, end.Color, t);
                        SetPixel(x, y, color);
                    }
                }
            }
        }
        #endregion

        #region Set/Get Pixel/Symbol
        public static void SetPixel(int left, int top, Color color)
        {
            if (_pixels == null || _changedPixels == null) return;

            color = Color.Mix(BackgroundColor, color, color.A / 255f);

            var index = GetPixelIndex(left, top);
            var currentColor = new Color(_pixels[index], _pixels[index + 1], _pixels[index + 2]);

            if (currentColor == color) return;

            lock (_pixels)
            {
                _pixels[index] = color.R;
                _pixels[index + 1] = color.G;
                _pixels[index + 2] = color.B;

                _changedPixels.Add((top * WindowSize.X) + left);
            }
        }
        public static Color GetPixel(int left, int top)
        {
            if (_pixels == null) return Colors.Transparent;

            var index = GetPixelIndex(left, top);
            return new Color(_pixels[index], _pixels[index + 1], _pixels[index + 2]);
        }
        public static int GetPixelIndex(int left, int top) =>
            ((top * WindowSize.X) + left) * 3;

        public static void SetSymbol(int left, int top, char symbol)
        {
            if (_symbols == null || _changedPixels == null) return;

            var index = GetSymbolIndex(left, top);
            _symbols[index] = symbol;
            _ = _changedPixels!.Add(index);
        }
        public static char GetSymbol(int left, int top)
        {
            if (_symbols == null) return ' ';

            var index = GetSymbolIndex(left, top);
            return _symbols[index];
        }
        public static int GetSymbolIndex(int left, int top) =>
            (top * WindowSize.X) + left;
        #endregion

        #region Utilities
        private static void EnableAnsiCodes()
        {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT) return;

            var handle = GetStdHandle(-11);
            _ = GetConsoleMode(handle, out uint mode);
            _ = SetConsoleMode(handle, mode | 0x0004);
        }

        private static string GetColorCode(Color color, char symbol)
        {
            if (!_colorCache.TryGetValue((color, symbol), out var code))
            {
                StringBuilder sb = new();
                sb.Append("\x1b[48;2;");
                sb.Append(color.R);
                sb.Append(';');
                sb.Append(color.G);
                sb.Append(';');
                sb.Append(color.B);
                sb.Append('m');
                sb.Append(symbol);
                sb.Append("\u001b[0m");
                code = sb.ToString();
                lock (_colorCache) _colorCache[(color, symbol)] = code;
            }
            return code;
        }

        private static void SetChangedAll()
        {
            if (_changedPixels == null) return;

            _ = Parallel.For(0, WindowSize.X * WindowSize.Y, i =>
            {
                lock (_changedPixels) _ = _changedPixels.Add(i);
            });
        }

        private static void InitializeArrays(int width, int height)
        {
            _pixels = new byte[width * height * 3];
            _symbols = new char[width * height];
            _changedPixels = new(width * height);

            Array.Fill(_symbols, ' ');
            Fill(_pixels, BackgroundColor);

            _outputBuffer = new(width * height * 15);
        }
        private static void RunMainLoop()
        {
            var time = DateTime.Now;
            var deltaTime = 0f;

            while (IsOpen)
            {
                if (Console.KeyAvailable) OnKeyPressed?.Invoke(Console.ReadKey(true));

                Fill(_pixels, BackgroundColor);
                Array.Fill(_symbols!, ' ');

                OnDraw?.Invoke(deltaTime);

                DrawScreen();

                Time += deltaTime;
                Frames++;

                deltaTime = (float)(DateTime.Now - time).TotalSeconds;
                _frametime.Add((float)(DateTime.Now - time).TotalMilliseconds);
                time = DateTime.Now;
            }
        }

        private static void Fill(byte[]? pixels, Color color)
        {
            if (pixels == null) return;

            for (int i = 0; i < pixels.Length; i += 3)
            {
                pixels[i] = color.R;
                pixels[i + 1] = color.G;
                pixels[i + 2] = color.B;
            }
        }
        #endregion

        #region Dlls
#pragma warning disable SYSLIB1054

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

#pragma warning restore SYSLIB1054
        #endregion
    }
}