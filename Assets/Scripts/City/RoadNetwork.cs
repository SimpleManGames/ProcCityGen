using System;
using System.Diagnostics;

using ProcCityGen.Fields.Tensors.Tracing;
using ProcCityGen.Interfaces.Fields.Tensor;

using Sirenix.OdinInspector;
using Sirenix.Serialization;

using Unity.Mathematics;

using UnityEngine;

using Debug = UnityEngine.Debug;

[Serializable]
[ShowOdinSerializedPropertiesInInspector]
public class RoadNetwork
{
    [SerializeField, Delayed]
    public Streamlines.StreamlineParams streamlineParams;

    private Streamlines _streamlines;
    
    [SerializeField]
    private bool drawMajor;

    [SerializeField]
    private bool drawMinor;

    public void GenerateStreamlines(ITensorField field, int width, int height)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        _streamlines = new Streamlines(new Rk4TracingStrategy(field, streamlineParams.dstep), new float2(0), new float2(width, height), streamlineParams);
        bool isMajor = true;

        // _streamlines.CreateStreamlines(isMajor);
        
        while (_streamlines.CreateStreamlines(isMajor))
        {
            isMajor = !isMajor;
        }
        
        _streamlines.JoinLooseStreamlines();
        stopwatch.Stop();
        Debug.Log($"{nameof(_streamlines.CreateStreamlines)} {stopwatch.Elapsed.Seconds}:{stopwatch.Elapsed.Milliseconds}");
    }
    
    public void DrawStreamlines()
    {
        if (drawMajor && _streamlines?.majorStreamlines != null)
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

        if (drawMinor && _streamlines?.minorStreamlines != null)
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
