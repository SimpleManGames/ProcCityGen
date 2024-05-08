namespace ProcCityGen.Fields.Tensors
{
    using ProcCityGen.Data;
    using ProcCityGen.Interfaces.Fields.Tensor;

    using Sirenix.Serialization;

    using Unity.Mathematics;

    using UnityEngine;

    public class PointDistanceDecay : ITensorField
    {
        [OdinSerialize]
        private ITensorField _field;
        [SerializeField]
        private readonly float2 _center;
        [SerializeField]
        private readonly float _decay;

        public PointDistanceDecay(ITensorField field, float2 center, float decay)
        {
            _field = field;
            _center = center;
            _decay = decay;
        }
        
        public void Sample(ref float2 position, out Tensor result)
        {
            float exp = DistanceDecay(_decay, math.lengthsq(position - _center));

            _field.Sample(ref position, out Tensor sample);
            result = exp * sample;
        }

        private float DistanceDecay(float decay, float distanceSqr)
        {
            return math.exp(-decay * distanceSqr);
        }
    }
}
