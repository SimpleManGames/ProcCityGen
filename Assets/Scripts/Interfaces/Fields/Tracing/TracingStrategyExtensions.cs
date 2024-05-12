namespace ProcCityGen.Interfaces.Fields.Tracing
{
    using ProcCityGen.Data;

    using Unity.Mathematics;

    public static class TracingStrategyExtensions
    {
        public static float2 SampleFieldVector(this ITracingStrategy tracingStrategy, float2 point, bool isMajor)
        {
            Tensor tensor = tracingStrategy.Field.SamplePoint(point);
            tensor.EigenVectors(out float2 major, out float2 minor);
            return isMajor ? major : minor;
        }
    }
}
