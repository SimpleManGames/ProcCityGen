namespace ProcCityGen.Fields.Scalars
{
    using ProcCityGen.Interfaces.Field.Scalars;

    using Unity.Mathematics;

    public class Multiply : IScalarField
    {
        private readonly IScalarField _baseField;
        private readonly float _scale;

        public Multiply(IScalarField baseField, float scale)
        {
            _baseField = baseField;
            _scale = scale;
        }

        public float Sample(float2 position)
        {
            return _baseField.Sample(position) * _scale;
        }
    }
}
