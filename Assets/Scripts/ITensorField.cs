namespace ProcCityGen
{
    using Unity.Mathematics;

    public interface ITensorField
    {
        void Sample(ref float2 position, out Tensor result);
    }
}