namespace ProcCityGen.Field.Scalars.Extensions
{
    using ProcCityGen.Field.Vectors;
    using ProcCityGen.Interfaces.Fields.Scalars;
    using ProcCityGen.Interfaces.Fields.Vectors;

    public static class ScalarFieldExtensions
    {
        public static IVector2Field Gradient(this IScalarField scalarField)
        {
            return new Gradient(scalarField);
        }
    }
}
