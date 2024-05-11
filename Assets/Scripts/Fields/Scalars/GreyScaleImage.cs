namespace ProcCityGen.Fields
{
    using System.Linq;

    using ProcCityGen.Interfaces.Fields.Scalars;

    using Unity.Mathematics;

    using UnityEngine;

    public class GreyScaleImage : IScalarField
    {
        [SerializeField]
        private readonly Texture2D _image;

        private float[] _values;

        public GreyScaleImage(Texture2D image)
        {
            _image = image;
            _values = GetValuesFromImage();
        }

        public float Sample(float2 position)
        {
            _values ??= GetValuesFromImage();

            int x = (int)position.x;
            int y = (int)position.y;

            return _values[y * _image.width + x];
        }

        private float[] GetValuesFromImage()
        {
            return _image.GetPixels().Select(x => x.r).ToArray();
        }
    }
}
