namespace ProcCityGen.Interfaces.Fields.Vectors
{
    using Unity.Mathematics;

    public interface IVector2Field
    {
        float2 Sample(float2 position);
    }
}
