namespace ProcCityGen.Interfaces
{
    using ProcCityGen.Data;

    using Unity.Mathematics;

    public interface IBasisField
    {
        float2 Center { get; }

        float Size { get; }

        float Decay { get; }

        Tensor GetTensor(float2 point);
    }

}
