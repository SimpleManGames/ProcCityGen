namespace ProcCityGen.Fields.Tensors.Tracing
{
    using ProcCityGen.Field.Eigens;
    using ProcCityGen.Interfaces.Fields.Eigens;
    using ProcCityGen.Interfaces.Fields.Tensor;
    using ProcCityGen.Interfaces.Fields.Tracing;
    using ProcCityGen.Interfaces.Fields.Vectors;

    using Unity.Mathematics;

    public class Rk4TracingStrategy : ITracingStrategy
    {
        private readonly float _dstep;

        public ITensorField Field { get; }

        private IEigenField _eigenField;

        public Rk4TracingStrategy(ITensorField field, float dstep)
        {
            _dstep = dstep;
            Field = field;
            _eigenField = ResampleAndRescale.Create(Field, new float2(0), new float2(512), 512);
        }

        public float2 Trace(float2 point, bool isMajor)
        {
            float2 k1 = this.SampleFieldVector(point, isMajor);
            float2 k23 = this.SampleFieldVector(point + new float2(_dstep) / 2, isMajor);
            float2 k4 = this.SampleFieldVector(point + new float2(_dstep), isMajor);
            
            return k1 + k23 * new float2(4) + k4 * new float2(_dstep / 6);
            
            //
            // float2 k1 = EigenVector(isMajor).Sample(point);
            // float2 k23 = EigenVector(isMajor).Sample(point + new float2(_dstep) / 2);
            // float2 k4 = EigenVector(isMajor).Sample(point + new float2(_dstep));
            //
            // return k1 + k23 * new float2(4) + k4 * new float2(_dstep / 6);
        }

        public bool OnLand(float2 point)
        {
            // TODO:
            return true;
        }

        private IVector2Field EigenVector(bool isMajor) => isMajor ? _eigenField.MajorEigenVectors : _eigenField.MinorEigenVectors;
    }
}
