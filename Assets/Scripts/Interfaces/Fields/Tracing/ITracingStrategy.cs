namespace ProcCityGen.Interfaces.Fields.Tracing
{
    using ProcCityGen.Interfaces.Fields.Tensor;

    using Unity.Mathematics;

    public interface ITracingStrategy
    {
        ITensorField Field { get; }
        
        float2 Trace(float2 point, bool isMajor);

        bool OnLand(float2 point);
    }

}
