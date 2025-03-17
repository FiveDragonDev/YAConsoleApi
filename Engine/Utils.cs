namespace Engine
{
    public struct Vector2(float x, float y) : IEquatable<Vector2>
    {
        public float X = x, Y = y;

        public Vector2 XX
        {
            readonly get => new(X, X);
            set => this = value;
        }
        public Vector2 XY
        {
            readonly get => this;
            set => this = value;
        }
        public Vector2 YX
        {
            readonly get => new(Y, X);
            set => this = value;
        }
        public Vector2 YY
        {
            readonly get => new(Y, Y);
            set => this = value;
        }

        public Vector2(float value) : this(value, value) { }

        public static float Distance(Vector2 v1, Vector2 v2) =>
            MathF.Sqrt(MathF.Pow(v1.X - v2.X, 2) + MathF.Pow(v1.Y - v2.Y, 2));

        public static implicit operator (float x, float y)(Vector2 v) => (v.X, v.Y);
        public static implicit operator float[](Vector2 v) => [v.X, v.Y];

        public static implicit operator Vector2(Vector2i v) => (v.X, v.Y);
        public static implicit operator Vector2((float x, float y) v) => new(v.x, v.y);
        public static implicit operator Vector2(float[] v) => new(v[0], v[1]);

        public static Vector2 operator -(Vector2 v) => new(-v.X, -v.Y);
        public static Vector2 operator +(Vector2 v1, Vector2 v2) =>
            new(v1.X + v2.X, v1.Y + v2.Y);
        public static Vector2 operator -(Vector2 v1, Vector2 v2) => v1 + (-v2);

        public static Vector2 operator *(Vector2 v, float scalar) =>
            new(v.X * scalar, v.Y * scalar);
        public static Vector2 operator /(Vector2 v, float scalar) => v * (1f / scalar);

        public static bool operator ==(Vector2 left, Vector2 right) => left.Equals(right);
        public static bool operator !=(Vector2 left, Vector2 right) => !(left == right);

        public override readonly bool Equals(object? obj) =>
            obj is Vector2 vector && Equals(vector);
        public readonly bool Equals(Vector2 other) => X == other.X && Y == other.Y;
        public override readonly int GetHashCode() => HashCode.Combine(X, Y);

        public override readonly string? ToString() => $"({X}, {Y})";
    }

    public struct Vector2i(int x, int y) : IEquatable<Vector2i>
    {
        public int X = x, Y = y;

        public Vector2i XX
        {
            readonly get => new(X, X);
            set => this = value;
        }
        public Vector2i XY
        {
            readonly get => this;
            set => this = value;
        }
        public Vector2i YX
        {
            readonly get => new(Y, X);
            set => this = value;
        }
        public Vector2i YY
        {
            readonly get => new(Y, Y);
            set => this = value;
        }

        public Vector2i(int value) : this(value, value) { }

        public static float Distance(Vector2i v1, Vector2i v2) =>
            MathF.Sqrt(MathF.Pow(v1.X - v2.X, 2) + MathF.Pow(v1.Y - v2.Y, 2));

        public static implicit operator (int x, int y)(Vector2i v) => (v.X, v.Y);
        public static implicit operator float[](Vector2i v) => [v.X, v.Y];

        public static explicit operator Vector2i(Vector2 v) => new((int)v.X, (int)v.Y);
        public static implicit operator Vector2i((int x, int y) v) => new(v.x, v.y);
        public static implicit operator Vector2i(int[] v) => new(v[0], v[1]);

        public static Vector2i operator -(Vector2i v) => new(-v.X, -v.Y);
        public static Vector2i operator +(Vector2i v1, Vector2i v2) =>
            new(v1.X + v2.X, v1.Y + v2.Y);
        public static Vector2i operator -(Vector2i v1, Vector2i v2) => v1 + (-v2);

        public static Vector2i operator *(Vector2i v, int scalar) =>
            new(v.X * scalar, v.Y * scalar);
        public static Vector2 operator /(Vector2i v, int scalar) => v / scalar;

        public static bool operator ==(Vector2i left, Vector2i right) => left.Equals(right);
        public static bool operator !=(Vector2i left, Vector2i right) => !(left == right);

        public override readonly bool Equals(object? obj) =>
            obj is Vector2i vector && Equals(vector);
        public readonly bool Equals(Vector2i other) => X == other.X && Y == other.Y;
        public override readonly int GetHashCode() => HashCode.Combine(X, Y);

        public override readonly string? ToString() => $"({X}, {Y})";
    }

    public struct Color(byte r, byte g, byte b)
    {
        public byte R = r, G = g, B = b;

        public static Color Invert(Color color) => new((byte)(255 - color.R),
            (byte)(255 - color.G), (byte)(255 - color.B));

        public static Color Mix(Color from, Color to, float value)
        {
            value = MathF.Max(MathF.Min(value, 1), 0);

            var r = (byte)(from.R + ((to.R - from.R) * value));
            var g = (byte)(from.G + ((to.G - from.G) * value));
            var b = (byte)(from.B + ((to.B - from.B) * value));

            return new Color(r, g, b);
        }

        public static implicit operator (byte r, byte g, byte b)(Color v) => (v.R, v.G, v.B);
        public static implicit operator byte[](Color v) => [v.R, v.G, v.B];

        public static implicit operator Color
            ((byte r, byte g, byte b) v) => new(v.r, v.g, v.b);
        public static implicit operator Color(byte[] v) => new(v[0], v[1], v[2]);

        public override readonly string? ToString() => $"({R}, {G}, {B})";
    }
    public static class Colors
    {
        public static readonly Color White = new(255, 255, 255);
        public static readonly Color Red = new(255, 0, 0);
        public static readonly Color Yellow = new(255, 255, 0);
        public static readonly Color Green = new(0, 255, 0);
        public static readonly Color Cyan = new(0, 255, 255);
        public static readonly Color Blue = new(0, 0, 255);
        public static readonly Color Purple = new(255, 0, 255);
        public static readonly Color Gray = new(128, 128, 128);
        public static readonly Color Black = new(0, 0, 0);
    }
}
