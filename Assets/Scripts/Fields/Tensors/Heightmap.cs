namespace ProcCityGen.Fields.Tensors
{
    using ProcCityGen.Data;
    using ProcCityGen.Field.Vectors;
    using ProcCityGen.Interfaces.Fields.Scalars;
    using ProcCityGen.Interfaces.Fields.Tensor;
    using ProcCityGen.Interfaces.Fields.Vectors;

    using Unity.Mathematics;

    public class Heightmap : ITensorField
    {
        private readonly IVector2Field _gradient;

        public Heightmap(IScalarField height)
        {
            _gradient = new Gradient(height);
        }
        
        public void Sample(ref float2 position, out Tensor result)
        {
            float2 gradient = _gradient.Sample(position);

            float theta = math.atan2(gradient.y, gradient.x) + (math.PI / 2);
            float r = math.sqrt(gradient.x * gradient.x + gradient.y * gradient.y);

            result = Tensor.Normalize(Tensor.FromRTheta(r, theta));
        }
    }
}
