namespace ProcCityGen.Fields.Tensors.Tracing
{
    using ProcCityGen.Interfaces.Fields.Tensor;
    using ProcCityGen.Interfaces.Fields.Tracing;

    using Unity.Mathematics;

    public class Rk4TracingStrategy : ITracingStrategy
    {
        private readonly float _dstep;

        public ITensorField Field { get; }

        public Rk4TracingStrategy(ITensorField field, float dstep)
        {
            _dstep = dstep;
            Field = field;
        }

        public float2 Trace(float2 point, bool isMajor)
        {
            float2 k1 = this.SampleFieldVector(point, isMajor);
            float2 k23 = this.SampleFieldVector(point + new float2(_dstep) / 2, isMajor);
            float2 k4 = this.SampleFieldVector(point + new float2(_dstep), isMajor);
            
            return k1 + k23 * new float2(4) + k4 * new float2(_dstep / 6);
        }

        public bool OnLand(float2 point)
        {
            // TODO:
            return true;
        }
    }
}
