namespace ProcCityGen.Fields.Tensors
{
    using ProcCityGen.Data;
    using ProcCityGen.Field.Eigens;
    using ProcCityGen.Interfaces.Fields.Eigens;
    using ProcCityGen.Interfaces.Fields.Tensor;

    using Unity.Mathematics;

    public static class TensorFieldExtensions
    {
        public static IEigenField PreSample(this ITensorField field, float2 min, float2 max, uint resolution)
        {
            return ResampleAndRescale.Create(field, min, max, resolution);
        }

        public static Tensor Sample(this ITensorField field, float2 position)
        {
            field.Sample(ref position, out Tensor result);
            return result;
        }

        public static float GetTensorWeight(this ITensorField field, float2 point, bool smooth)
        {
            float normDistanceToCenter = math.length(point - field.Center) / field.Size;

            if (smooth)
            {
                return math.pow(normDistanceToCenter, -field.Decay);
            }

            if (field.Decay == 0 && normDistanceToCenter >= 1)
            {
                return 0;
            }

            return math.max(0, math.pow(1 - normDistanceToCenter, field.Decay));
        }
    }
}
