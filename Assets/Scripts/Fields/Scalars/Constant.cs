namespace ProcCityGen.Field.Scalars
{
    using ProcCityGen.Interfaces.Fields.Scalars;

    using Unity.Mathematics;

    public class Constant : IScalarField
    {
        private readonly float _constant;

        public Constant(float constant)
        {
            _constant = constant;
        }
        
        public float Sample(float2 position)
        {
            return _constant;
        }
    }
}
