namespace ProcCityGen.Interfaces.Fields.Tensor
{
    using ProcCityGen.Data;

    using Unity.Mathematics;

    public interface ITensorField
    {
        Tensor SamplePoint(float2 point);
    }
}
