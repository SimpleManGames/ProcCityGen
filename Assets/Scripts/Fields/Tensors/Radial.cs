namespace ProcCityGen.Fields.Tensors
{
    using ProcCityGen.Data;
    using ProcCityGen.Interfaces.Fields.Tensor;

    using Unity.Mathematics;

    public class Radial : ITensorField
    {
        private readonly float2 _center;

        public Radial(float2 center)
        {
            _center = center;
        }
        
        public void Sample(ref float2 position, out Tensor result)
        {
            result = Tensor.FromXY(position - _center);
        }
    }
}
