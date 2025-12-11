namespace TimelessTales.Utils
{
    /// <summary>
    /// Simplex noise implementation for terrain generation
    /// Based on Stefan Gustavson's implementation
    /// </summary>
    public class SimplexNoise
    {
        private readonly int[] _perm;
        private readonly Random _random;

        public SimplexNoise(int seed)
        {
            _random = new Random(seed);
            _perm = new int[512];
            
            int[] p = new int[256];
            for (int i = 0; i < 256; i++)
                p[i] = i;
            
            // Shuffle
            for (int i = 0; i < 256; i++)
            {
                int j = _random.Next(256);
                (p[i], p[j]) = (p[j], p[i]);
            }
            
            // Duplicate for wrapping
            for (int i = 0; i < 512; i++)
                _perm[i] = p[i & 255];
        }

        public float Evaluate(float x, float y)
        {
            // 2D Simplex noise
            float F2 = 0.5f * (MathF.Sqrt(3.0f) - 1.0f);
            float G2 = (3.0f - MathF.Sqrt(3.0f)) / 6.0f;

            float n0, n1, n2;
            float s = (x + y) * F2;
            int i = FastFloor(x + s);
            int j = FastFloor(y + s);
            float t = (i + j) * G2;
            float X0 = i - t;
            float Y0 = j - t;
            float x0 = x - X0;
            float y0 = y - Y0;

            int i1, j1;
            if (x0 > y0) { i1 = 1; j1 = 0; }
            else { i1 = 0; j1 = 1; }

            float x1 = x0 - i1 + G2;
            float y1 = y0 - j1 + G2;
            float x2 = x0 - 1.0f + 2.0f * G2;
            float y2 = y0 - 1.0f + 2.0f * G2;

            int ii = i & 255;
            int jj = j & 255;
            int gi0 = _perm[ii + _perm[jj]] % 12;
            int gi1 = _perm[ii + i1 + _perm[jj + j1]] % 12;
            int gi2 = _perm[ii + 1 + _perm[jj + 1]] % 12;

            float t0 = 0.5f - x0 * x0 - y0 * y0;
            if (t0 < 0) n0 = 0.0f;
            else
            {
                t0 *= t0;
                n0 = t0 * t0 * Dot(Grad3[gi0], x0, y0);
            }

            float t1 = 0.5f - x1 * x1 - y1 * y1;
            if (t1 < 0) n1 = 0.0f;
            else
            {
                t1 *= t1;
                n1 = t1 * t1 * Dot(Grad3[gi1], x1, y1);
            }

            float t2 = 0.5f - x2 * x2 - y2 * y2;
            if (t2 < 0) n2 = 0.0f;
            else
            {
                t2 *= t2;
                n2 = t2 * t2 * Dot(Grad3[gi2], x2, y2);
            }

            return 70.0f * (n0 + n1 + n2);
        }

        public float Evaluate(float x, float y, float z)
        {
            // TODO: Implement proper 3D simplex noise for better performance
            // This is a simplified version that averages three 2D samples
            // Consider implementing full 3D simplex noise algorithm
            return (Evaluate(x, y) + Evaluate(y, z) + Evaluate(x, z)) / 3.0f;
        }

        private static int FastFloor(float x)
        {
            return x > 0 ? (int)x : (int)x - 1;
        }

        private static float Dot(int[] g, float x, float y)
        {
            return g[0] * x + g[1] * y;
        }

        private static readonly int[][] Grad3 = new int[][]
        {
            new int[] {1,1,0}, new int[] {-1,1,0}, new int[] {1,-1,0}, new int[] {-1,-1,0},
            new int[] {1,0,1}, new int[] {-1,0,1}, new int[] {1,0,-1}, new int[] {-1,0,-1},
            new int[] {0,1,1}, new int[] {0,-1,1}, new int[] {0,1,-1}, new int[] {0,-1,-1}
        };
    }
}
