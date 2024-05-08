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

    public class WeightedAverage : ITensorField
    {
        public struct WeightedTenserField
        {
            [TypeFilter("GetITensorFieldTypeList")]
            public ITensorField field;

            public float weight;

            private IEnumerable<Type> GetITensorFieldTypeList()
            {
                var q = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
                    .Where(x => !x.IsAbstract)
                    .Where(x => !x.IsGenericTypeDefinition)
                    .Where(x => typeof(ITensorField).IsAssignableFrom(x));

                return q;
            }
        }

        public float2 Center { get; }

        public float Size { get; }

        public float Decay { get; }

        private float TotalWeight => _blends.Sum(x => x.weight);

        [OdinSerialize]
        private readonly List<WeightedTenserField> _blends = new List<WeightedTenserField>();

        public void Blend(ITensorField field, float weight = 1)
        {
            _blends.Add(new WeightedTenserField
            {
                field = field,
                weight = weight
            });
        }

        public void Sample(ref float2 position, out Tensor result)
        {
            result = new Tensor(0, 0, 0);

            foreach (WeightedTenserField blendPair in _blends)
            {
                result += blendPair.weight / TotalWeight * blendPair.field.Sample(position);
            }
        }
    }
}
