namespace ProcCityGen.Fields.Tensors
{
    using System;

    using ProcCityGen.Data;
    using ProcCityGen.Interfaces.Fields.Tensor;

    using Unity.Mathematics;

    using UnityEngine;

    public class Gridline : ITensorField
    {
        public float2 Center { get; }

        public float Size { get; }

        public float Decay { get; }

        [SerializeField] private readonly float _angleInDegrees;

        private float theta => _angleInDegrees * (math.PI / 180);

        public void Sample(ref float2 position, out Tensor result)
        {
            float cos = math.cos(2 * theta);
            float sin = math.sin(2 * theta);
            result =  new Tensor(1, cos, sin);
        }
    }
}
