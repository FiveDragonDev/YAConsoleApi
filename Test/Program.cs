using Engine;

namespace Test
{
    internal class Program
    {
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
            Vector2 size = Screen.WindowSize;
            
            for (int i = 0; i < size.Y; i++)
            {
                for (int j = 0; j < size.X; j++)
                {
                    Screen.SetPixel(j, i, (byte)(j / size.X * 255), (byte)(i / size.Y * 255),
                        (byte)((MathF.Sin(_time / 10f * 3.141592f) + 1) / 2 * 255));
                }
            }
            _time += deltaTime;

            Screen.Text(0, 0, (1f / deltaTime).ToString("000.00 fps"));
            Screen.Text(0, 1, _time.ToString("0.00 sec"));
        }
    }
}
