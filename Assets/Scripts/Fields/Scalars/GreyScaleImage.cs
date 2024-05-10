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
            GetValuesFromImage();
        }

        public float Sample(float2 position)
        {
            if (_values == null)
            {
                GetValuesFromImage();
            }

            int x = (int)(position.x);
            int y = (int)(position.y);

            // try
            // {
                return _values[y * _image.width + x];
            // }
            // catch
            // {
                // Debug.Log($"{x} {y} {_image.width} {y * _image.width + x}");
            // }
            // return 0;
        }

        private void GetValuesFromImage()
        {
            _values = _image.GetPixels().Select(x => x.r).ToArray();
        }
    }
}
