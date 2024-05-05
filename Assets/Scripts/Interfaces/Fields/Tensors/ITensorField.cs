namespace ProcCityGen.Interfaces.Fields.Tensor
{
    using ProcCityGen.Data;

    using Unity.Mathematics;

    public interface ITensorField
    {
        void Sample(ref float2 position, out Tensor result);
    }
}