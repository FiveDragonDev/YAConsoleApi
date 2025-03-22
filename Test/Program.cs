using Engine;

namespace Test
{
    internal class Program
    {
        private static int _framesPerSecondCounter;
        private static readonly List<int> _framesPerSecond = [];
        private static float _lastTime;
        private static float _time;

        private static void Main()
        {
            Screen.OnKeyPressed += KeyPressed;
            Screen.OnDraw += Draw;
            Screen.OnClose += () => Loger.Log($"Average FPS was: " +
                $"{_framesPerSecond.Average():0.0000}");
            Screen.Initialize(120, 30, "Test");
        }

        private static void KeyPressed(ConsoleKeyInfo info)
        {
            if (info.Key == ConsoleKey.Escape) Screen.Shutdown();
        }
        private static void Draw(float deltaTime)
        {
            _ = Parallel.For(0, Screen.WindowSize.X * Screen.WindowSize.Y, index =>
            {
                int x = index % Screen.WindowSize.X;
                int y = index / Screen.WindowSize.X;
                Screen.SetPixel(x, y,
                    new((float)x / Screen.WindowSize.X,
                        (float)y / Screen.WindowSize.Y,
                        (-MathF.Cos(Screen.Time * MathF.PI / 3) + 1) / 2f));
            });

            Screen.ConvexShape(
                new(new(3, 6, Colors.Red), new(15, 3, Colors.Yellow),
                new(32, 14, Colors.Yellow), new(29, 22, Colors.Cyan),
                new(9, 19, Colors.Blue)));

            _time += deltaTime;

            _framesPerSecondCounter++;
            if (_time >= MathF.Ceiling(_lastTime))
            {
                _framesPerSecond.Insert(0, _framesPerSecondCounter);
                _framesPerSecondCounter = 0;
            }
            _lastTime = _time;

            Screen.Text(0, 0, $"fps: {_framesPerSecond[0]:00}");
            Screen.Text(0, 1, $"time: {_time:0.00}");
            Screen.Text(0, 2, $"resolution: {Screen.WindowSize}");
        }
    }
}
