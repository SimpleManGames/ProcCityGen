using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

using ProcCityGen.Field.Eigens;
using ProcCityGen.Interfaces.Fields.Eigens;
using ProcCityGen.Interfaces.Fields.Tensor;

using Sirenix.OdinInspector;

using Unity.Mathematics;

using UnityEngine;

using Debug = UnityEngine.Debug;
using Random = Unity.Mathematics.Random;

[Serializable]
[ShowOdinSerializedPropertiesInInspector]
public class LineIntegralConvolution
{
    [SerializeField]
    private ComputeShader licCompute, whiteNoiseCompute;

    [SerializeField]
    private Material material;

    [MinValue(1), SerializeField]
    private int lineLength;

    private RenderTexture _whiteNoiseResults;
    private RenderTexture _lineIntegralConvolutionResult;

    private float2[] _vectorMap;
    private float4[] _whiteNoise;
    private IEigenField _eigenField;

    private ComputeBuffer _vectorMapBuffer;

    private ITensorField _field;
    private int _width;
    private int _height;

    private static readonly int ResultId = Shader.PropertyToID("Result");
    private static readonly int WhiteNoiseId = Shader.PropertyToID("WhiteNoise");
    private static readonly int VectorsId = Shader.PropertyToID("Vectors");
    private static readonly int MainTex = Shader.PropertyToID("_MainTex");
    private static readonly int WidthId = Shader.PropertyToID("width");
    private static readonly int HeightId = Shader.PropertyToID("height");
    private static readonly int StreamlineLengthId = Shader.PropertyToID("streamlineLength");

    private int Kernel => licCompute.FindKernel("CSMain");

    public void RenderLic(ITensorField field, int width, int height)
    {
        _field = field;
        _width = width;
        _height = height;
        _whiteNoise = new float4[width * height];
        _vectorMap = new float2[width * height];
        uint x, y, z;

        if (_whiteNoiseResults == null || _whiteNoiseResults.width != _width || _whiteNoiseResults.height != _height)
        {
            SetupWhiteNoiseComputeShader();
            whiteNoiseCompute.GetKernelThreadGroupSizes(Kernel, out x, out y, out z);
            whiteNoiseCompute.Dispatch(Kernel, width / (int)x, height / (int)y, (int)z);
        }

        SetupLineIntegralConvolutionComputeShader();

        licCompute.GetKernelThreadGroupSizes(Kernel, out x, out y, out z);
        licCompute.Dispatch(Kernel, width / (int)x, height / (int)y, (int)z);

        material.SetTexture(MainTex, _lineIntegralConvolutionResult);
        _vectorMapBuffer.Dispose();
    }

    private void SetupLineIntegralConvolutionComputeShader()
    {
        _lineIntegralConvolutionResult = new RenderTexture(_width, _height, 24)
        {
            enableRandomWrite = true
        };
        _lineIntegralConvolutionResult.Create();

        licCompute.SetTexture(Kernel, ResultId, _lineIntegralConvolutionResult);
        licCompute.SetTexture(Kernel, WhiteNoiseId, _whiteNoiseResults);

        licCompute.SetInt(WidthId, _width);
        licCompute.SetInt(HeightId, _height);
        licCompute.SetInt(StreamlineLengthId, lineLength);

        _vectorMapBuffer = new ComputeBuffer(_width * _height, Marshal.SizeOf(typeof(float2)));

        PopulateVectorTextureWithFieldValues();
        licCompute.SetBuffer(Kernel, VectorsId, _vectorMapBuffer);

        _vectorMapBuffer.SetData(_vectorMap);
    }

    private void SetupWhiteNoiseComputeShader()
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        _whiteNoiseResults = new RenderTexture(_width, _height, 24)
        {
            enableRandomWrite = true
        };
        _whiteNoiseResults.Create();
        whiteNoiseCompute.SetTexture(Kernel, ResultId, _whiteNoiseResults);
    }

    private void CreateWhiteNoise()
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        Random rand = new Random();
        rand.InitState();

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                float value = Unity.Mathematics.noise.snoise(rand.NextFloat2(0f, 1.0f));
                _whiteNoise[y * _width + x] = new float4(value, value, value, 1.0f);
            }
        }
        stopwatch.Stop();
        Debug.Log($"{nameof(CreateWhiteNoise)} {stopwatch.Elapsed.Seconds}:{stopwatch.Elapsed.Milliseconds}");
    }

    private void PopulateVectorTextureWithFieldValues()
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        _eigenField = ResampleAndRescale.Create(_field, new float2(0), new float2(_width, _height), (uint)_width);

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                float2 v = _eigenField.MajorEigenVectors.Sample(new float2(x, y));

                // Field.SamplePoint(new float2(x, y)).EigenVectors(out float2 major, out float2 minor);
                _vectorMap[y * _width + x] = v;
            }
        }
        stopwatch.Stop();
        Debug.Log($"{nameof(PopulateVectorTextureWithFieldValues)} {stopwatch.Elapsed.Seconds}:{stopwatch.Elapsed.Milliseconds}");
    }
}
