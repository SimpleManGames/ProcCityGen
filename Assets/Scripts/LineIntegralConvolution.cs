using System;
using System.Collections.Generic;
using System.Linq;

using ProcCityGen.Fields.Tensors;
using ProcCityGen.Interfaces.Fields.Eigens;
using ProcCityGen.Interfaces.Fields.Scalars;
using ProcCityGen.Interfaces.Fields.Tensor;

using Sirenix.OdinInspector;
using Sirenix.Serialization;

using Unity.Mathematics;

using UnityEngine;

using Random = Unity.Mathematics.Random;

[ShowOdinSerializedPropertiesInInspector]
public class LineIntegralConvolution : SerializedMonoBehaviour
{
    public int width, height, lineLength;

    public Texture2D output;
    public Texture2D whiteNoise;

    [InlineEditor, TypeFilter("GetITensorFieldTypeList"), OdinSerialize]
    public ITensorField Field { get; private set; }
    
    private IEigenField _eigenField;
    private Material _material;

    // ReSharper disable once UnusedMember.Local
    private IEnumerable<Type> GetITensorFieldTypeList()
    {
        IEnumerable<Type> q = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
            .Where(x => !x.IsAbstract)
            .Where(x => !x.IsGenericTypeDefinition)
            .Where(x => typeof(ITensorField).IsAssignableFrom(x));

        return q;
    }

    [Button]
    public void Start()
    {
        CreateWhiteNoise();

        Run();
        
        
    }

    private void CreateWhiteNoise()
    {
        whiteNoise = new Texture2D(width, height);

        Random rand = new Random();
        rand.InitState();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float value = Unity.Mathematics.noise.snoise(rand.NextFloat2(0f, 255f));
                Color color = new Color(value, value, value, 1.0f);
                whiteNoise.SetPixel(x, y, color);
            }
        }

        whiteNoise.Apply();
    }

    private void Run()
    {
        _eigenField = Field.PreSample(new float2(0), new float2(width, height), (uint)width);
        output = new Texture2D(width, height);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Color c = whiteNoise.GetPixel(x, y);
                float3 col = new float3(c.r, c.g, c.b);
                int w = 0;

                float2 v = _eigenField.MajorEigenVectors.Sample(new float2(x, y)); // (_eigenField.MajorEigenVectors.Sample(new float2(x, y)) - new float2(0.5)) * 2;

                float2 st0 = new float2(x, y);

                for (int i = 0; i < lineLength; i++)
                {
                    st0 += v;
                    Color n = whiteNoise.GetPixel((int)st0.x, (int)st0.y);
                    col += new float3(n.r, n.g, n.b);
                    w++;
                }

                float2 st1 = new float2(x, y);

                for (int i = 0; i < lineLength; i++)
                {
                    st1 -= v;
                    Color n = whiteNoise.GetPixel((int)st1.x, (int)st1.y);
                    col += new float3(n.r, n.g, n.b);
                    w++;
                }

                col /= w;

                output.SetPixel(x, y, new Color(col.x, col.y, col.z, 1.0f));
            }
        }
        output.Apply();
        _material.SetTexture("_MainTex", output);
    }

    private void OnValidate()
    {
        if (_material == null)
            _material = GetComponent<MeshRenderer>().sharedMaterial;
    }
}
