namespace ProcCityGen
{
    using Unity.Mathematics;

    internal class Inverse
    {
        private readonly IVector2Field _field;

        public Inverse(IVector2Field field)
        {
            _field = field;
        }

        public float2 Sample(float2 position)
        {
            return -_field.Sample(position);
        }
    }
}
