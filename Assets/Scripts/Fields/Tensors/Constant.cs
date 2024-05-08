namespace ProcCityGen.Fields.Tensors
{
    using ProcCityGen.Data;
    using ProcCityGen.Interfaces.Fields.Tensor;

    using Unity.Mathematics;

    using UnityEngine;

    public class Constant : ITensorField
    {
        public float2 Center { get; }

        public float Size { get; }

        public float Decay { get; }
        
        [SerializeField]
        private readonly Tensor _value;

        public Constant(Tensor value)
        {
            _value = value;
        }

        public void Sample(ref float2 position, out Tensor result)
        {
            result = _value;
        }
    }
}
