namespace ProcCityGen.Fields.Tensors
{
    using System;

    using ProcCityGen.Data;
    using ProcCityGen.Interfaces.Fields.Tensor;

    using Unity.Mathematics;

    using UnityEngine;

    public class Gridline : ITensorField
    {
        [field: SerializeField]
        public float2 Center { get; private set; }

        [field: SerializeField]
        public float Size { get; private set; } = 1;

        [field: SerializeField]
        public float Decay { get; private set; } = 1;

        [SerializeField] private readonly float _angleInDegrees;

        private float theta => _angleInDegrees * (math.PI / 180);

        public void Sample(ref float2 position, out Tensor result)
        {
            float cos = math.cos(2 * theta);
            float sin = math.sin(2 * theta);
            result = new Tensor(1, cos, sin);
        }
    }
}
