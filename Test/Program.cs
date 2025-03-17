using Engine;

namespace Test
{
    internal class Program
    {
        private static float _framesPerSecondOutput;
        private static float _framesPerSecond;
        private static float _lastTime;
        private static float _time;

        private static void Main()
        {
            Screen.OnKeyPressed += KeyPressed;
            Screen.OnDraw += Draw;
            Screen.Initialize(120, 30, "Test");
        }

        private static void KeyPressed(ConsoleKeyInfo info)
        {
            if (info.Key == ConsoleKey.Escape) Environment.Exit(0);
        }
        private static void Draw(float deltaTime)
        {
            Screen.ConvexShape(
                new(3, 4, Colors.Red), new(65, 10, Colors.Yellow),
                new(30, 20, Colors.Cyan), new(10, 20, Colors.Blue));

            _time += deltaTime;
            _framesPerSecond++;
            if (_time >= Math.Ceiling(_lastTime))
            {
                _framesPerSecondOutput = _framesPerSecond;
                _framesPerSecond = 0;
            }
            _lastTime = _time;

            Screen.Text(0, 0, $"fps: {_framesPerSecondOutput:00}");
            Screen.Text(0, 1, $"time: {_time:0.00}");
            Screen.Text(0, 2, $"resolution: {Screen.WindowSize}");
        }
    }
}
