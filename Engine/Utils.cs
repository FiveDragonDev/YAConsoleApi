using System.Collections;

namespace Engine
{
    public static class Math
    {
        public static float Clamp(float value, float min, float max) =>
            MathF.Max(MathF.Min(value, max), min);
        public static float Clamp01(float value) => Clamp(value, 0, 1);
    }

    public readonly struct Matrix(int rows, int columns) : IEnumerable<float>
    {
        public readonly float this[int row, int column]
        {
            get => _table[(row * Columns) + column];
            set => _table[(row * Columns) + column] = value;
        }

        public int Rows { get; } = rows;
        public int Columns { get; } = columns;

        private readonly float[] _table = new float[rows * columns];

        public static Matrix Identity(int size)
        {
            Matrix matrix = new(size, size);
            for (int i = 0; i < size; i++) matrix[i, i] = 1;
            return matrix;
        }

        public static Matrix operator *(Matrix left, Matrix right)
        {
            if (left.Columns != right.Rows)
                throw new InvalidOperationException(
                    "Number of left matrix columns must be equal to number of right matrix rows");

            Matrix result = new(left.Rows, right.Columns);
            for (int i = 0; i < left.Rows; i++)
                for (int j = 0; j < right.Columns; j++)
                    for (int k = 0; k < left.Columns; k++)
                        result[i, j] += left[i, k] * right[k, j];
            return result;
        }
        public static float[] operator *(float[] vector, Matrix matrix)
        {
            if (vector.Length != matrix.Columns) throw new ArgumentException(
                "Number of rows and vectors dimensions must be equal");

            var result = new float[matrix.Rows];
            for (int i = 0; i < matrix.Rows; i++)
                for (int j = 0; j < matrix.Columns; j++)
                    result[i] += matrix[i, j] * vector[j];
            return result;
        }

        public static implicit operator float[,](Matrix matrix)
        {
            var result = new float[matrix.Rows, matrix.Columns];
            for (int i = 0; i < matrix.Rows * matrix.Columns; i++)
            {
                int column = i % matrix.Columns;
                int row = i / matrix.Columns;
                result[row, column] = matrix._table[i];
            }
            return result;
        }

        public static implicit operator Matrix(float[,] matrix)
        {
            Matrix result = new(matrix.GetLength(0), matrix.GetLength(1));
            for (int i = 0; i < result.Rows; i++)
            {
                for (int j = 0; j < result.Columns; j++)
                {
                    result[i, j] = matrix[i, j];
                }
            }
            return result;
        }

        public IEnumerator<float> GetEnumerator() => (IEnumerator<float>)_table.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

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

        public override readonly string? ToString() => $"({X:0.000}, {Y:0.000})";
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

    public struct Color(byte r, byte g, byte b, byte a) : IEquatable<Color>
    {
        public byte R = r, G = g, B = b, A = a;

        public Color(byte r, byte g, byte b) : this(r, g, b, 255) { }
        public Color(float r, float g, float b, float a) :
            this((byte)(Math.Clamp01(r) * 255), (byte)(Math.Clamp01(g) * 255),
                (byte)(Math.Clamp01(b) * 255), (byte)(Math.Clamp01(a) * 255))
        { }
        public Color(float r, float g, float b) : this(r, g, b, 1) { }

        public static Color Invert(Color color) =>
            new((byte)(255 - color.R), (byte)(255 - color.G),
                (byte)(255 - color.B), (byte)(255 - color.A));

        public static Color Mix(Color from, Color to, float value)
        {
            value = MathF.Max(MathF.Min(value, 1), 0);

            var r = (byte)(from.R + ((to.R - from.R) * value));
            var g = (byte)(from.G + ((to.G - from.G) * value));
            var b = (byte)(from.B + ((to.B - from.B) * value));
            var a = (byte)(from.A + ((to.A - from.A) * value));

            return new Color(r, g, b, a);
        }

        public static implicit operator (byte r, byte g, byte b, byte a)
            (Color v) => (v.R, v.G, v.B, v.A);
        public static implicit operator byte[](Color v) => [v.R, v.G, v.B, v.A];

        public static implicit operator Color
            ((byte r, byte g, byte b, byte a) v) => new(v.r, v.g, v.b, v.a);
        public static implicit operator Color(byte[] v) => new(v[0], v[1], v[2], v[3]);

        public static bool operator ==(Color left, Color right) => left.Equals(right);
        public static bool operator !=(Color left, Color right) => !(left == right);

        public override readonly bool Equals(object? obj) =>
            obj is Color color && Equals(color);
        public readonly bool Equals(Color other) => GetHashCode() == other.GetHashCode();
        public override readonly int GetHashCode() => HashCode.Combine(R, G, B, A);

        public override readonly string? ToString() => $"({R}, {G}, {B}, {A})";
    }
    public static class Colors
    {
        public static readonly Color White = new(1f, 1f, 1f);
        public static readonly Color LightGray = new(0.75f, 0.75f, 0.75f);
        public static readonly Color Gray = new(0.5f, 0.5f, 0.5f);
        public static readonly Color DarkGray = new(0.25f, 0.25f, 0.25f);
        public static readonly Color Black = new(0, 0, 0);

        public static readonly Color Red = new(1f, 0, 0);
        public static readonly Color Yellow = new(1f, 1f, 0);
        public static readonly Color Green = new(0, 1f, 0);
        public static readonly Color Cyan = new(0, 1f, 1f);
        public static readonly Color Blue = new(0, 0, 1f);
        public static readonly Color Magenta = new(1f, 0, 1f);

        public static readonly Color Transparent = new(0, 0, 0, 0);
    }

    public readonly struct VertexArray : IEnumerable<Vertex>, IList<Vertex>
    {
        public readonly Vertex this[int index]
        { get => _vertices[index]; set => _vertices[index] = value; }

        public readonly int Count => _vertices.Count;
        public readonly bool IsReadOnly => ((ICollection<Vertex>)_vertices).IsReadOnly;

        private readonly List<Vertex> _vertices = [];

        public VertexArray() => _vertices = [];
        public VertexArray(int capacity) => _vertices = new(capacity);
        public VertexArray(IEnumerable<Vertex> vertices) => _vertices = [.. vertices];
        public VertexArray(params Vertex[] vertices) => _vertices = [.. vertices];

        public readonly void AddRange(params Vertex[] vertices) =>
            _vertices.AddRange(vertices);
        public readonly void Add(Vertex vertex) => _vertices.Add(vertex);

        public readonly void InsertRange(int index, params Vertex[] vertices) =>
            _vertices.InsertRange(index, vertices);
        public readonly void Insert(int index, Vertex vertex) =>
            _vertices.Insert(index, vertex);

        public readonly void RemoveRange(int start, int count) =>
            _vertices.RemoveRange(start, count);
        public readonly void RemoveAt(int index) => _vertices.RemoveAt(index);
        public readonly bool Remove(Vertex vertex) => _vertices.Remove(vertex);

        public readonly int IndexOf(Vertex vertex) =>
            ((IList<Vertex>)_vertices).IndexOf(vertex);

        public readonly bool Contains(Vertex vertex) => _vertices.Contains(vertex);

        public readonly void Clear() => _vertices.Clear();

        public readonly void CopyTo(Vertex[] array, int arrayIndex) =>
            _vertices.CopyTo(array, arrayIndex);

        public readonly IEnumerator<Vertex> GetEnumerator() => _vertices.GetEnumerator();
        readonly IEnumerator IEnumerable.GetEnumerator() => _vertices.GetEnumerator();

        public static implicit operator VertexArray(Vertex[] vertices) =>
            [.. vertices];
        public static implicit operator Vertex[](VertexArray vertices) =>
            [.. vertices._vertices];
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
