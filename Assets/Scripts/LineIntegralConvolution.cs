using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

using ProcCityGen.Field.Eigens;
using ProcCityGen.Fields.Tensors.Tracing;
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

    [InlineEditor, Sirenix.OdinInspector.TypeFilter("GetITensorFieldTypeList"), OdinSerialize]
    public ITensorField Field { get; private set; }

    [SerializeField]
    public Streamlines.StreamlineParams streamlineParams;

    [SerializeField]
    private bool _drawMajor, _drawMinor;

    private Streamlines _streamlines;

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

        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        _streamlines = new Streamlines(new Rk4TracingStrategy(Field, streamlineParams.dstep), new float2(0), new float2(width, height), streamlineParams);
        bool isMajor = true;
        // _streamlines.CreateStreamlines(isMajor);

        while (_streamlines.CreateStreamlines(isMajor))
        {
            isMajor = !isMajor;
        }
        stopwatch.Stop();
        Debug.Log($"{nameof(_streamlines.CreateStreamlines)} {stopwatch.Elapsed.Seconds}:{stopwatch.Elapsed.Milliseconds}");
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
        _eigenField = ResampleAndRescale.Create(Field, new float2(0), new float2(width, height), (uint)width);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float2 v = _eigenField.MinorEigenVectors.Sample(new float2(x, y));

                // Field.SamplePoint(new float2(x, y)).EigenVectors(out float2 major, out float2 minor);
                _vectorMap[y * width + x] = v;
            }
        }
        stopwatch.Stop();
        Debug.Log($"{nameof(PopulateVectorTextureWithFieldValues)} {stopwatch.Elapsed.Seconds}:{stopwatch.Elapsed.Milliseconds}");
    }

    private void OnDrawGizmos()
    {
        if (_drawMajor && _streamlines?.majorStreamlines != null)
        {
            Gizmos.color = Color.green;
            foreach (float2[] streamlineSegments in _streamlines.majorStreamlines)
            {
                for (int i = 0; i + 1 < streamlineSegments.Length; i++)
                {
                    float2 pointStart = streamlineSegments[i];
                    float2 pointEnd = streamlineSegments[i + 1];
                    Gizmos.DrawLine(new float3(pointStart, 0.0f), new float3(pointEnd, 0.0f));
                }
            }
        }

        if (_drawMinor && _streamlines?.minorStreamlines != null)
        {
            Gizmos.color = Color.yellow;

            foreach (float2[] streamlineSegments in _streamlines.minorStreamlines)
            {
                for (int i = 0; i + 1 < streamlineSegments.Length; i++)
                {
                    float2 pointStart = streamlineSegments[i];
                    float2 pointEnd = streamlineSegments[i + 1];
                    Gizmos.DrawLine(new float3(pointStart, 0.0f), new float3(pointEnd, 0.0f));
                }
            }
        }
    }
}
