namespace ProcCityGen.Interfaces.Field.Scalars
{
    using Unity.Mathematics;

    public interface IScalarField
    {
        float Sample(float2 position);
    }
}
