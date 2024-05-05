namespace ProcCityGen
{
    using Unity.Mathematics;

    public class Gradient : IVector2Field
    {
        private readonly IScalarField _scalar;
        
        public Gradient(IScalarField scalar)
        {
            _scalar = scalar;
        }
        
        public float2 Sample(float2 position)
        {
            float v = _scalar.Sample(position);
            float x = _scalar.Sample(new float2(position.x + 1, position.y));
            float y = _scalar.Sample(new float2(position.x, position.y + 1));

            return new float2(v - x, v - y);
        }
    }
}
