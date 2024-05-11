using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

using ProcCityGen.Fields.Tensors;
using ProcCityGen.Interfaces.Fields.Eigens;
using ProcCityGen.Interfaces.Fields.Tensor;

using Sirenix.OdinInspector;
using Sirenix.Serialization;

using Unity.Mathematics;

using UnityEngine;

using Debug = UnityEngine.Debug;
using Random = Unity.Mathematics.Random;

[ShowOdinSerializedPropertiesInInspector]
public class LineIntegralConvolution : SerializedMonoBehaviour
{
    [MinValue(1)]
    public int width, height, lineLength;

    public ComputeShader licCompute;

    public RenderTexture result;
    public Material material;

    [InlineEditor, TypeFilter("GetITensorFieldTypeList"), OdinSerialize]
    public ITensorField Field { get; private set; }

    private float2[] _vectorMap;
    private float4[] _whiteNoise;
    private IEigenField _eigenField;

    private ComputeBuffer _vectorMapBuffer;

    private ComputeBuffer _whiteNoiseBuffer;

    private static readonly int ResultId = Shader.PropertyToID("Result");
    private static readonly int WhiteNoiseId = Shader.PropertyToID("WhiteNoise");
    private static readonly int VectorsId = Shader.PropertyToID("Vectors");
    private static readonly int MainTex = Shader.PropertyToID("_MainTex");
    private static readonly int WidthId = Shader.PropertyToID("width");
    private static readonly int HeightId = Shader.PropertyToID("height");
    private static readonly int StreamlineLengthId = Shader.PropertyToID("streamlineLength");

    private int Kernel => licCompute.FindKernel("CSMain");

    // ReSharper disable once UnusedMember.Local
    private IEnumerable<Type> GetITensorFieldTypeList()
    {
        IEnumerable<Type> q = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
            .Where(x => !x.IsAbstract)
            .Where(x => !x.IsGenericTypeDefinition)
            .Where(x => typeof(ITensorField).IsAssignableFrom(x));

        return q;
    }

    [Button("Execute")]
    public void Start()
    {
        result = new RenderTexture(width, height, 24)
        {
            enableRandomWrite = true
        };
        result.Create();
        _whiteNoise = new float4[width * height];
        _vectorMap = new float2[width * height];
        _vectorMapBuffer = new ComputeBuffer(width * height, Marshal.SizeOf(typeof(float2)));
        _whiteNoiseBuffer = new ComputeBuffer(width * height, Marshal.SizeOf(typeof(float4)));

        licCompute.SetTexture(Kernel, ResultId, result);

        licCompute.SetInt(WidthId, width);
        licCompute.SetInt(HeightId, height);
        licCompute.SetInt(StreamlineLengthId, lineLength);

        CreateWhiteNoise();
        licCompute.SetBuffer(Kernel, WhiteNoiseId, _whiteNoiseBuffer);

        PopulateVectorTextureWithFieldValues();
        licCompute.SetBuffer(Kernel, VectorsId, _vectorMapBuffer);

        _whiteNoiseBuffer.SetData(_whiteNoise);
        _vectorMapBuffer.SetData(_vectorMap);

        licCompute.GetKernelThreadGroupSizes(Kernel, out uint x, out uint y, out uint z);
        licCompute.Dispatch(Kernel, width / (int)x, height / (int)y, (int)z);

        material.SetTexture(MainTex, result);
        _vectorMapBuffer.Dispose();
        _whiteNoiseBuffer.Dispose();
    }

    private void CreateWhiteNoise()
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        Random rand = new Random();
        rand.InitState();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float value = Unity.Mathematics.noise.snoise(rand.NextFloat2(0f, 1.0f));
                _whiteNoise[y * width + x] = new float4(value, value, value, 1.0f);
            }
        }
        stopwatch.Stop();
        Debug.Log($"{nameof(CreateWhiteNoise)} {stopwatch.Elapsed.Seconds}:{stopwatch.Elapsed.Milliseconds}");
    }

    private void PopulateVectorTextureWithFieldValues()
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        _eigenField = Field.PreSample(new float2(0), new float2(width, height), (uint)width);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float2 v = _eigenField.MajorEigenVectors.Sample(new float2(x, y));
                _vectorMap[y * width + x] = v;
            }
        }
        stopwatch.Stop();
        Debug.Log($"{nameof(PopulateVectorTextureWithFieldValues)} {stopwatch.Elapsed.Seconds}:{stopwatch.Elapsed.Milliseconds}");
    }

    // private void Run()
    // {
    //     _eigenField = Field.PreSample(new float2(0), new float2(width, height), (uint)width);
    //     result = new Texture2D(width, height);
    //
    //     for (int x = 0; x < width; x++)
    //     {
    //         for (int y = 0; y < height; y++)
    //         {
    //             Color c = whiteNoise.GetPixel(x, y);
    //             float3 col = new float3(c.r, c.g, c.b);
    //             int w = 0;
    //
    //             float2 v = _eigenField.MajorEigenVectors.Sample(new float2(x, y)); // (_eigenField.MajorEigenVectors.Sample(new float2(x, y)) - new float2(0.5)) * 2;
    //
    //             float2 st0 = new float2(x, y);
    //
    //             for (int i = 0; i < lineLength; i++)
    //             {
    //                 st0 += v;
    //                 Color n = whiteNoise.GetPixel((int)st0.x, (int)st0.y);
    //                 col += new float3(n.r, n.g, n.b);
    //                 w++;
    //             }
    //
    //             float2 st1 = new float2(x, y);
    //
    //             for (int i = 0; i < lineLength; i++)
    //             {
    //                 st1 -= v;
    //                 Color n = whiteNoise.GetPixel((int)st1.x, (int)st1.y);
    //                 col += new float3(n.r, n.g, n.b);
    //                 w++;
    //             }
    //
    //             col /= w;
    //
    //             result.SetPixel(x, y, new Color(col.x, col.y, col.z, 1.0f));
    //         }
    //     }
    //     result.Apply();
    //     _material.SetTexture("_MainTex", result);
    // }
}
