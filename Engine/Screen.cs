﻿using System.Runtime.InteropServices;
using System.Text;

namespace Engine
{
    public static class Screen
    {
        public static Vector2 WindowSize { get; private set; }
        public static string Title
        {
            get => Console.Title;
            set => Console.Title = value;
        }

        public static event Action<ConsoleKeyInfo>? OnKeyPressed;
        public static event Action<float>? OnDraw;

        public static bool IsOpen { get; private set; }

        private static readonly StringBuilder _output = new();
        private static char[]? _symbols;
        private static byte[]? _pixels;

        public static void Initialize(int width, int height, string title)
        {
            WindowSize = new Vector2(width, height);
            Title = title;

            _pixels = new byte[width * height * 3];
            Array.Fill(_pixels, (byte)0);

            _symbols = new char[width * height];
            Array.Fill(_symbols, ' ');

            Console.SetWindowSize(width, height);
            Console.SetBufferSize(width, height);

            EnableAnsiCodes();
            DisableResize();

            Console.Clear();
            Console.CursorVisible = false;

            IsOpen = true;

            DateTime time = DateTime.Now;
            float deltaTime = 0;
            while (IsOpen)
            {
                ConsoleKeyInfo? input;
                if (Console.KeyAvailable && (input = Console.ReadKey(true)) != null)
                    OnKeyPressed?.Invoke((ConsoleKeyInfo)input);

                _output.Clear();
                Array.Fill(_symbols, ' ');

                OnDraw?.Invoke(deltaTime);

                for (int y = 0; y < WindowSize.Y; y++)
                {
                    for (int x = 0; x < WindowSize.X; x++)
                    {
                        var symbol = _symbols[(int)((y * WindowSize.X) + x)];
                        var (r, g, b) = GetPixel(x, y);
                        _output.Append($"\x1b[48;2;{r};{g};{b}m{symbol}\x1b[0m");
                    }
                    if (y >= WindowSize.Y - 1) continue;
                    _output.AppendLine();
                }
                Console.SetCursorPosition(0, 0);
                Console.Write(_output.ToString());

                deltaTime = (float)(DateTime.Now - time).TotalSeconds;
                time = DateTime.Now;
            }
        }
        public static void Close() => IsOpen = false;

        public static void Text(int left, int top, string text)
        {
            if (_symbols == null) return;
            for (int i = 0; i < text.Length; i++)
            {
                SetPixel(left + i, top, 0, 0, 0);
                _symbols[(int)((top * WindowSize.X) + left + i)] = text[i];
            }
        }

        public static void SetPixel(int left, int top, byte r, byte g, byte b)
        {
            if (_pixels == null) return;
            var index = GetPixelIndex(left, top);
            _pixels[index] = r;
            _pixels[index + 1] = g;
            _pixels[index + 2] = b;
        }
        public static (byte r, byte g, byte b) GetPixel(int left, int top)
        {
            if (_pixels == null) return (0, 0, 0);
            var index = GetPixelIndex(left, top);
            (byte r, byte g, byte b) color = (0, 0, 0);

            color.r = _pixels[index];
            color.g = _pixels[index + 1];
            color.b = _pixels[index + 2];

            return color;
        }
        public static int GetPixelIndex(int left, int top) =>
            (int)(((top * WindowSize.X) + left) * 3);

        private static void EnableAnsiCodes()
        {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT) return;

            var handle = GetStdHandle(-11);
            _ = GetConsoleMode(handle, out uint mode);
            _ = SetConsoleMode(handle, mode | 0x0004);
        }
        private static void DisableResize()
        {
            var handle = GetConsoleWindow();
            if (handle == IntPtr.Zero) return;
            var sysMenu = GetSystemMenu(handle, false);
            _ = DeleteMenu(sysMenu, 0xF030, 0x00000000);
            _ = DeleteMenu(sysMenu, 0xF000, 0x00000000);
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        [DllImport("user32.dll")]
        public static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetConsoleWindow();
    }
}
