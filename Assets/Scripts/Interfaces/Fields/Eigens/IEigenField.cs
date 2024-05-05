namespace ProcCityGen.Interfaces.Fields.Eigens
{
    using ProcCityGen.Interfaces.Fields.Vectors;

    public interface IEigenField
    {
        IVector2Field MajorEigenVectors { get; }

        IVector2Field MinorEigenVectors { get; }
    }
}
