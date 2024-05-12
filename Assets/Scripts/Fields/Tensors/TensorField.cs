namespace ProcCityGen.Fields.Tensors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using ProcCityGen.Data;
    using ProcCityGen.Fields.Tensors.Extensions;
    using ProcCityGen.Interfaces;
    using ProcCityGen.Interfaces.Fields.Tensor;

    using Sirenix.OdinInspector;
    using Sirenix.Serialization;

    using Unity.Mathematics;

    [Serializable]
    public class TensorField : ITensorField
    {
        public bool smooth;
        
        [InlineEditor, TypeFilter("GetIBasisFieldTypeList"), OdinSerialize]
        private List<IBasisField> _basisFields = new List<IBasisField>();

        public Tensor SamplePoint(float2 point)
        {
            if (_basisFields.Count == 0)
            {
                return new Tensor(1, 0, 0);
            }

            Tensor tensor = Tensor.Zero;
            
            _basisFields.ForEach(field => tensor.Combine(field.GetWeightedTensor(point, smooth), smooth));

            return tensor;
        }
        
        private IEnumerable<Type> GetIBasisFieldTypeList()
        {
            IEnumerable<Type> q = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
                .Where(x => !x.IsAbstract)
                .Where(x => !x.IsGenericTypeDefinition)
                .Where(x => typeof(IBasisField).IsAssignableFrom(x));

            return q;
        }
    }
}
