namespace ProcCityGen
{
    using Unity.Mathematics;

    public interface IScalarField
    {
        float Sample(float2 position);
    }

    public static class ScalarFieldExtensions
    {
        public static IVector2Field Gradient(this IScalarField scalarField)
        {
            return new Gradient(scalarField);
        }
    }
}
