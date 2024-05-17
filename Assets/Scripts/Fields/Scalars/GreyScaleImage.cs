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

            if (x < 0 || y < 0 || x >= _image.width || y >= _image.height)
            {
                return 0;
            }

            try
            {
                return _values[y * _image.width + x];
            }
            catch
            {
                int i = 0;
            }
            return 0;
        }

        private float[] GetValuesFromImage()
        {
            _values = new float[_image.width * _image.height];

            for (int x = 0; x < _image.width - 1; x++)
            {
                for (int y = 0; y < _image.height - 1; y++)
                {
                    _values[y * _image.width + x] = _image.GetPixel(x, y).r;
                }
            }

            return _image.GetPixels().Select(x => x.r).ToArray();
        }
    }
}
