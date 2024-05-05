namespace ProcCityGen.Fields.Tensors
{
    using System.Collections.Generic;

    using ProcCityGen.Data;
    using ProcCityGen.Interfaces.Fields.Tensor;

    using Unity.Mathematics;

    public class WeightedAverage : ITensorField
    {
        private float _totalWeight;
        private readonly List<KeyValuePair<ITensorField, float>> _blends = new List<KeyValuePair<ITensorField, float>>();

        public void Blend(ITensorField field, float weight = 1)
        {
            _blends.Add(new KeyValuePair<ITensorField, float>(field, weight));
            _totalWeight += weight;
        }
        
        public void Sample(ref float2 position, out Tensor result)
        {
            result = new Tensor(0, 0);

            foreach (KeyValuePair<ITensorField, float> blendPair in _blends)
            {
                result += blendPair.Value / _totalWeight * blendPair.Key.Sample(position);
            }
        }
    }
}
