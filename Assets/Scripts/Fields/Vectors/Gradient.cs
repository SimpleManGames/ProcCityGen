namespace ProcCityGen.Field.Vectors
{
    using ProcCityGen.Interfaces.Fields.Scalars;
    using ProcCityGen.Interfaces.Fields.Vectors;

    using Sirenix.Serialization;

    using Unity.Mathematics;

    public class Gradient : IVector2Field
    {
        [OdinSerialize]
        private readonly IScalarField _scalar;
        
        public Gradient(IScalarField scalar)
        {
            _scalar = scalar;
        }
        
        public float2 Sample(float2 position)
        {
            float v = _scalar.Sample(position);
            float x = _scalar.Sample(new float2(position.x + 1f, position.y));
            float y = _scalar.Sample(new float2(position.x, position.y + 1f));

            return new float2(v - x, v - y);
        }
    }
}
