namespace ProcCityGen.Field.Vectors
{
    using ProcCityGen.Interfaces.Fields.Vectors;

    using Unity.Mathematics;

    public class Constant : IVector2Field
    {
        private readonly float2 _value;

        public Constant(float2 value)
        {
            this._value = value;
        }
        
        public float2 Sample(float2 position)
        {
            return _value;
        }
    }
}
