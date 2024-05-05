namespace ProcCityGen.Field.Eigens
{
    using ProcCityGen.Data;
    using ProcCityGen.Interfaces.Fields.Eigens;
    using ProcCityGen.Interfaces.Fields.Tensor;
    using ProcCityGen.Interfaces.Fields.Vectors;

    using Unity.Mathematics;

    public class ResampleAndRescale : IEigenField
    {
        public IVector2Field MajorEigenVectors { get; private set; }

        public IVector2Field MinorEigenVectors { get; private set; }

        private readonly float2[,] _majorEigenVectors;
        private readonly float _xLength;
        private readonly float _yLength;

        private readonly float2 _size;
        private readonly float2 _min;

        private readonly bool _zeroSize;

        private ResampleAndRescale(float2[,] major, float2 min, float2 max)
        {
            _min = min;
            _size = max - min;

            _zeroSize = math.abs(_size.x) < math.EPSILON || math.abs(_size.y) < math.EPSILON;

            if (_zeroSize)
            {
                return;
            }

            _majorEigenVectors = major;

            _xLength = major.GetLength(0) - 1;
            _yLength = major.GetLength(1) - 1;

            MinorEigenVectors = new EigenAccessor(true, this);
            MajorEigenVectors = new EigenAccessor(false, this);
        }

        private void Sample(bool major, ref float2 position, out float2 result)
        {
            if (_zeroSize)
            {
                result = float2.zero;
                return;
            }

            float2 p = (position - _min) / _size;
            float2 ij = p * new float2(_xLength, _yLength);
            int i = (int)math.clamp(ij.x, 0, _xLength);
            int j = (int)math.clamp(ij.y, 0, _yLength);

            result = _majorEigenVectors[i, j];

            if (!major)
            {
                result = new float2(-result.y, result.x);
            }
        }

        public static IEigenField Create(ITensorField baseField, float2 min, float2 max, uint resolution)
        {
            float2[,] major = new float2[resolution + 1, resolution + 1];

            for (int i = 0; i < resolution + 1; i++)
            {
                for (int j = 0; j < resolution + 1; j++)
                {
                    float2 p = new float2(i / (float)resolution, j / (float)resolution) + min / new float2(resolution, resolution);

                    baseField.Sample(ref p, out Tensor t);

                    t.EigenVectors(out float2 majorEigen, out float2 minorEigen);

                    major[i, j] = majorEigen;
                }
            }

            return new ResampleAndRescale(major, min, max);
        }

        private class EigenAccessor : IVector2Field
        {
            private readonly bool _major;
            private readonly ResampleAndRescale _field;

            public EigenAccessor(bool major, ResampleAndRescale field)
            {
                _major = major;
                _field = field;
            }

            public float2 Sample(float2 position)
            {
                _field.Sample(_major, ref position, out var result);
                return result;
            }
        }
    }
}
