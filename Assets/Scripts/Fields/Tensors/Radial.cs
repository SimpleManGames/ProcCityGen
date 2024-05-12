namespace ProcCityGen.Fields.Tensors
{
    using ProcCityGen.Data;
    using ProcCityGen.Interfaces;

    using Unity.Mathematics;

    using UnityEngine;

    public class Radial : IBasisField
    {
        [field: SerializeField]
        public float2 Center { get; private set; }

        [field: SerializeField]
        public float Size { get; private set; }

        [field: SerializeField]
        public float Decay { get; private set; }

        public Radial(float2 center)
        {
            Center = center;
        }

        public Tensor GetTensor(float2 point)
        {
            float2 t = new float2(point.x - Center.x, point.y - Center.y);
            float t1 = math.pow(t.y, 2) - math.pow(t.x, 2);
            float t2 = -2 * t.x * t.y;
            return new Tensor(1, t1, t2);
        }
    }
}
