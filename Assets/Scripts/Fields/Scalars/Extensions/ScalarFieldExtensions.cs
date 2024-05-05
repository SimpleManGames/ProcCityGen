namespace ProcCityGen.Fields.Scalars.Extensions
{
    using ProcCityGen.Fields.Vectors;
    using ProcCityGen.Interfaces.Field.Scalars;
    using ProcCityGen.Interfaces.Field.Vectors;

    public static class ScalarFieldExtensions
    {
        public static IVector2Field Gradient(this IScalarField scalarField)
        {
            return new Gradient(scalarField);
        }
    }
}
