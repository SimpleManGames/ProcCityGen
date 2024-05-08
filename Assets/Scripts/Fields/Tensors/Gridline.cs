namespace ProcCityGen.Fields.Tensors
{
    using System;

    using ProcCityGen.Data;
    using ProcCityGen.Interfaces.Fields.Tensor;

    using Unity.Mathematics;

    using UnityEngine;

    public class Gridline : ITensorField
    {
        private Tensor? _basis;

        private Tensor Basis
        {
            get
            {
                _basis ??= CalculateBasis();
                return (Tensor)_basis;
            }
        }

        [SerializeField] private readonly float _angle;

        [SerializeField] private readonly float _length;

        public Gridline(float angle, float length)
        {
            _angle = angle;
            _length = length;

            _basis = CalculateBasis();
        }

        public void Sample(ref float2 position, out Tensor result)
        {
            result = CalculateBasis();
        }

        private Tensor CalculateBasis()
        {
            return Tensor.FromRTheta(_length, math.radians(_angle));
        }
    }
}
