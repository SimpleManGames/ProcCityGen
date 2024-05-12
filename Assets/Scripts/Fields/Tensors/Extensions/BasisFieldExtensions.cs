namespace ProcCityGen.Fields.Tensors.Extensions
{
    using ProcCityGen.Data;
    using ProcCityGen.Interfaces;

    using Unity.Mathematics;

    public static class BasisFieldExtensions
    {
        public static Tensor GetWeightedTensor(this IBasisField field, float2 point, bool smooth)
        {
            return field.GetTensor(point).Scale(field.GetTensorWeight(point, smooth));
        }

        private static float GetTensorWeight(this IBasisField field, float2 point, bool smooth)
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

            return math.pow(math.max(0, 1 - normDistanceToCenter), field.Decay);
        }
    }
}
