namespace ProcCityGen.Fields.Tensors
{
    using ProcCityGen.Data;
    using ProcCityGen.Interfaces.Fields.Tensor;

    using Unity.Mathematics;

    using UnityEngine;

    public class Radial : ITensorField
    {
        [SerializeField]
        private readonly float2 _center;

        public Radial(float2 center)
        {
            _center = center;
        }
        
        public void Sample(ref float2 position, out Tensor result)
        {
            float2 t = new float2(position.x - _center.x, position.y - _center.y);
            float t1 = (t.y * t.y) - (t.x * t.x);
            float t2 = -2 * t.x * t.y;
            result = new Tensor(t1, t2);
        }
    }
}
