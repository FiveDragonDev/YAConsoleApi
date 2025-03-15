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

        public static implicit operator (float x, float y)(Vector2 v) => (v.X, v.Y);
        public static implicit operator float[](Vector2 v) => [v.X, v.Y];

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
}
