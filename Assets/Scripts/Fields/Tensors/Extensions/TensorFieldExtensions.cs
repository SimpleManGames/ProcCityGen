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

        public static ITensorField DecayDistanceFromPoint(this ITensorField field, float2 center, float decay)
        {
            return new PointDistanceDecay(field, center, decay);
        }
    }
}
