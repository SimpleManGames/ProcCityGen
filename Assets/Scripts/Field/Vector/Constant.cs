namespace ProcCityGen.Vector2Field
{
    using Unity.Mathematics;

    public class Constant : IVector2Field
    {
        private readonly float2 value;

        public Constant(float2 value)
        {
            this.value = value;
        }
        
        public float2 Sample(float2 position)
        {
            return value;
        }
    }
}
