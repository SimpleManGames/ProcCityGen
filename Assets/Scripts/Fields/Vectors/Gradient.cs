namespace ProcCityGen.Field.Vectors
{
    using System;

    using ProcCityGen.Interfaces.Fields.Scalars;
    using ProcCityGen.Interfaces.Fields.Vectors;

    using Sirenix.Serialization;

    using Unity.Mathematics;

    public class Gradient : IVector2Field
    {
        [OdinSerialize]
        private readonly IScalarField _scalar;

        private float[,] _gradientMagnitudes;

        private Func<float, float, float> gradientFunc = (x, y) => math.pow(x, 2) + math.pow(y, 2);

        public Gradient(IScalarField scalar)
        {
            _scalar = scalar;
        }

        public float2 Sample(float2 position)
        {
            _gradientMagnitudes ??= GetGradientMagnitudes();

            int x = (int)position.x;
            int y = (int)position.y;
            int step = 2;

            try
            {
                float d2Hdx2 = (_gradientMagnitudes[x + step, y] - 2 * _gradientMagnitudes[x, y] + _gradientMagnitudes[x - step, y]);
                float d2Hdy2 = (_gradientMagnitudes[x, y + step] - 2 * _gradientMagnitudes[x, y] + _gradientMagnitudes[x, y - step]);
                float d2Hdxdy = (_gradientMagnitudes[x + step, y + step] - _gradientMagnitudes[x + step, y - step] - _gradientMagnitudes[x - step, y + step] + _gradientMagnitudes[x - step, y]) / 4.0f;

                return new float2(d2Hdx2, d2Hdy2);
            }
            catch
            {
                return 0;
            }
        }

        private float[,] GetGradientMagnitudes()
        {
            _gradientMagnitudes = new float[512, 512];

            for (int x = 0; x < _gradientMagnitudes.GetLength(0) - 1; x++)
            {
                for (int y = 0; y < _gradientMagnitudes.GetLength(1) - 1; y++)
                {
                    float aboveX = _scalar.Sample(new float2(x + 1f, y));
                    float aboveY = _scalar.Sample(new float2(x, y + 1f));
                    float belowX = _scalar.Sample(new float2(x - 1f, y));
                    float belowY = _scalar.Sample(new float2(x, y - 1f));

                    float dHdx = (aboveX - belowX) / 2.0f;
                    float dHdy = (aboveY - belowY) / 2.0f;

                    _gradientMagnitudes[x, y] = math.sqrt(dHdx * dHdx + dHdy * dHdy);
                }
            }

            return _gradientMagnitudes;
        }
    }
}
