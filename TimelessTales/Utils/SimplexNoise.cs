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
            // 3D Simplex noise implementation
            const float F3 = 1.0f / 3.0f;
            const float G3 = 1.0f / 6.0f;
            const float NOISE_SCALE_3D = 32.0f; // Scaling factor to normalize output to [-1, 1]

            float n0, n1, n2, n3;

            // Skew the input space to determine which simplex cell we're in
            float s = (x + y + z) * F3;
            int i = FastFloor(x + s);
            int j = FastFloor(y + s);
            int k = FastFloor(z + s);

            float t = (i + j + k) * G3;
            float X0 = i - t;
            float Y0 = j - t;
            float Z0 = k - t;
            float x0 = x - X0;
            float y0 = y - Y0;
            float z0 = z - Z0;

            // Determine which simplex we are in
            int i1, j1, k1; // Offsets for second corner of simplex in (i,j,k) coords
            int i2, j2, k2; // Offsets for third corner of simplex in (i,j,k) coords

            if (x0 >= y0)
            {
                if (y0 >= z0)
                {
                    i1 = 1; j1 = 0; k1 = 0; i2 = 1; j2 = 1; k2 = 0;
                }
                else if (x0 >= z0)
                {
                    i1 = 1; j1 = 0; k1 = 0; i2 = 1; j2 = 0; k2 = 1;
                }
                else
                {
                    i1 = 0; j1 = 0; k1 = 1; i2 = 1; j2 = 0; k2 = 1;
                }
            }
            else
            {
                if (y0 < z0)
                {
                    i1 = 0; j1 = 0; k1 = 1; i2 = 0; j2 = 1; k2 = 1;
                }
                else if (x0 < z0)
                {
                    i1 = 0; j1 = 1; k1 = 0; i2 = 0; j2 = 1; k2 = 1;
                }
                else
                {
                    i1 = 0; j1 = 1; k1 = 0; i2 = 1; j2 = 1; k2 = 0;
                }
            }

            // Offsets for corners in (x,y,z) coords
            float x1 = x0 - i1 + G3;
            float y1 = y0 - j1 + G3;
            float z1 = z0 - k1 + G3;
            float x2 = x0 - i2 + 2.0f * G3;
            float y2 = y0 - j2 + 2.0f * G3;
            float z2 = z0 - k2 + 2.0f * G3;
            float x3 = x0 - 1.0f + 3.0f * G3;
            float y3 = y0 - 1.0f + 3.0f * G3;
            float z3 = z0 - 1.0f + 3.0f * G3;

            // Work out the hashed gradient indices of the four simplex corners
            int ii = i & 255;
            int jj = j & 255;
            int kk = k & 255;
            int gi0 = _perm[ii + _perm[jj + _perm[kk]]] % 12;
            int gi1 = _perm[ii + i1 + _perm[jj + j1 + _perm[kk + k1]]] % 12;
            int gi2 = _perm[ii + i2 + _perm[jj + j2 + _perm[kk + k2]]] % 12;
            int gi3 = _perm[ii + 1 + _perm[jj + 1 + _perm[kk + 1]]] % 12;

            // Calculate the contribution from the four corners
            float t0 = 0.6f - x0 * x0 - y0 * y0 - z0 * z0;
            if (t0 < 0) n0 = 0.0f;
            else
            {
                t0 *= t0;
                n0 = t0 * t0 * Dot(Grad3[gi0], x0, y0, z0);
            }

            float t1 = 0.6f - x1 * x1 - y1 * y1 - z1 * z1;
            if (t1 < 0) n1 = 0.0f;
            else
            {
                t1 *= t1;
                n1 = t1 * t1 * Dot(Grad3[gi1], x1, y1, z1);
            }

            float t2 = 0.6f - x2 * x2 - y2 * y2 - z2 * z2;
            if (t2 < 0) n2 = 0.0f;
            else
            {
                t2 *= t2;
                n2 = t2 * t2 * Dot(Grad3[gi2], x2, y2, z2);
            }

            float t3 = 0.6f - x3 * x3 - y3 * y3 - z3 * z3;
            if (t3 < 0) n3 = 0.0f;
            else
            {
                t3 *= t3;
                n3 = t3 * t3 * Dot(Grad3[gi3], x3, y3, z3);
            }

            // Add contributions from each corner to get the final noise value
            // The result is scaled to return values in the interval [-1,1]
            return NOISE_SCALE_3D * (n0 + n1 + n2 + n3);
        }

        private static int FastFloor(float x)
        {
            return x > 0 ? (int)x : (int)x - 1;
        }

        private static float Dot(int[] g, float x, float y)
        {
            return g[0] * x + g[1] * y;
        }

        private static float Dot(int[] g, float x, float y, float z)
        {
            return g[0] * x + g[1] * y + g[2] * z;
        }

        private static readonly int[][] Grad3 = new int[][]
        {
            new int[] {1,1,0}, new int[] {-1,1,0}, new int[] {1,-1,0}, new int[] {-1,-1,0},
            new int[] {1,0,1}, new int[] {-1,0,1}, new int[] {1,0,-1}, new int[] {-1,0,-1},
            new int[] {0,1,1}, new int[] {0,-1,1}, new int[] {0,1,-1}, new int[] {0,-1,-1}
        };
    }
}
