namespace ProcCityGen.Interfaces.Fields.Tensor
{
    using System;

    using ProcCityGen.Data;

    using Unity.Mathematics;

    public interface ITensorField
    {
        float2 Center { get; }

        float Size { get; }

        float Decay { get; }

        void Sample(ref float2 position, out Tensor result);
    }
}
