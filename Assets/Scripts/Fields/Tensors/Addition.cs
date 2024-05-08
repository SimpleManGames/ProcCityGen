namespace ProcCityGen.Fields.Tensors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using ProcCityGen.Data;
    using ProcCityGen.Interfaces.Fields.Tensor;

    using Sirenix.OdinInspector;
    using Sirenix.Serialization;

    using Unity.Mathematics;

    public class Addition : ITensorField
    {
        [TypeFilter("GetITensorFieldTypeList"), OdinSerialize]
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

        private IEnumerable<Type> GetITensorFieldTypeList()
        {
            var q = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
                .Where(x => !x.IsAbstract)
                .Where(x => !x.IsGenericTypeDefinition)
                .Where(x => typeof(ITensorField).IsAssignableFrom(x));

            return q;
        }
    }
}
