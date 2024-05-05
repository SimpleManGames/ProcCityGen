namespace ProcCityGen.Fields.Tensors
{
    using System.Collections.Generic;

    using ProcCityGen.Data;
    using ProcCityGen.Interfaces.Fields.Tensor;

    using Unity.Mathematics;

    public class Addition : ITensorField
    {
        private readonly List<ITensorField> _fields = new List<ITensorField>();

        public Addition(params ITensorField[] fields)
        {
            foreach (ITensorField tensorField in fields)
            {
                Add(tensorField);
            }
        }
        
        public void Add(ITensorField tensorField)
        {
            _fields.Add(tensorField);
        }

        public void Sample(ref float2 position, out Tensor result)
        {
            result = new Tensor(0, 0);

            foreach (ITensorField t in _fields)
            {
                result += t.Sample(position);
            }
        }
    }
}
