namespace ProcCityGen.Fields.Tensors
{
    using ProcCityGen.Data;
    using ProcCityGen.Interfaces.Fields.Tensor;

    using Unity.Mathematics;

    public class Gridline : ITensorField
    {
        private readonly Tensor _basis;

        public Gridline(float angle, float length)
        {
            _basis = Tensor.FromRTheta(length, angle);
        }

        public void Sample(ref float2 position, out Tensor result)
        {
            result = _basis;
        }
    }
}
