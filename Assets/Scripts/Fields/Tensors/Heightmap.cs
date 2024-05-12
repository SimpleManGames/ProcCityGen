namespace ProcCityGen.Fields.Tensors
{
    using ProcCityGen.Data;
    using ProcCityGen.Field.Vectors;
    using ProcCityGen.Interfaces;
    using ProcCityGen.Interfaces.Fields.Scalars;
    using ProcCityGen.Interfaces.Fields.Vectors;

    using Sirenix.Serialization;

    using Unity.Mathematics;

    public class Heightmap : IBasisField
    {
        public float2 Center { get; }

        public float Size { get; }

        public float Decay { get; }
        
        [OdinSerialize]
        private readonly IVector2Field _gradient;

        public Heightmap(IScalarField height)
        {
            _gradient = new Gradient(height);
        }

        public Tensor GetTensor(float2 point)
        {
            float2 gradient = _gradient.Sample(point);

            float r = math.sqrt(gradient.x * gradient.x + gradient.y * gradient.y);
            float theta = (float)math.atan2(gradient.y / r, gradient.x / r) / 2;
            // float theta = math.atan2(gradient.y, gradient.x) + (math.PI / 2);

            return Tensor.FromTheta(r, theta);
        }
    }
}
