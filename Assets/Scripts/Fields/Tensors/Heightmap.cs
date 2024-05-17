namespace ProcCityGen.Fields.Tensors
{
    using System;

    using ProcCityGen.Data;
    using ProcCityGen.Interfaces;
    using ProcCityGen.Interfaces.Fields.Scalars;
    using ProcCityGen.Interfaces.Fields.Vectors;

    using Sirenix.Serialization;

    using Unity.Mathematics;

    using UnityEngine;

    using Gradient = ProcCityGen.Field.Vectors.Gradient;

    [Serializable]
    public class Heightmap : IBasisField
    {
        [field: SerializeField]
        public float2 Center { get; private set; }

        [field: SerializeField]
        public float Size { get; private set; } = 512;

        [field: SerializeField]
        public float Decay { get; private set; }
        
        [OdinSerialize]
        private readonly IVector2Field _gradient;

        public Heightmap(IScalarField height)
        {
            _gradient = new Gradient(height);
        }

        public Tensor GetTensor(float2 point)
        {
            float2 gradient = _gradient.Sample(point);

            float r = math.sqrt(gradient.x * gradient.x + gradient.y * gradient.y);
            //float theta = (float)math.atan2(gradient.y / r, gradient.x / r) / 2;
            float theta = math.atan2(gradient.y, gradient.x) + (math.PI / 2);

            return Tensor.FromTheta(r, theta);
        }
    }
}
