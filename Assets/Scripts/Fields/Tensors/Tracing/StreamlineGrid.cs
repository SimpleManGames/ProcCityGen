namespace ProcCityGen.Fields.Tensors.Tracing
{
    using System.Collections.Generic;
    using System.Linq;

    using Unity.Mathematics;

    using UnityEngine;

    public class StreamlineGrid
    {
        private class GridCell
        {
            public List<float2> streamline = new List<float2>();
        }

        private readonly float2 _worldDimensions;
        private readonly float2 _origin;
        private readonly float _dsep;

        private readonly float _dsepSq;
        private int2 _gridDimensions;
        private GridCell[] _grid;

        public StreamlineGrid(float2 worldDimensions, float2 origin, float dsep)
        {
            _worldDimensions = worldDimensions;
            _origin = origin;
            _dsep = dsep;
            _dsepSq = _dsep * _dsep;
            _gridDimensions = (int2)math.round(worldDimensions / new float2(dsep));

            int size = (int)_gridDimensions.x * (int)_gridDimensions.y;
            _grid = new GridCell[size];

            for (int x = 0; x < _gridDimensions.x; x++)
            {
                for (int y = 0; y < _gridDimensions.y; y++)
                {
                    _grid[y * (int)_gridDimensions.x + x] = new GridCell
                    {
                        streamline = new List<float2>()
                    };
                }
            }
        }

        public bool IsValidSample(float2 point, float distanceSq)
        {
            float2 coords = GetSampleCoords(point);

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    float2 cell = coords + new float2(x, y);

                    if (PointOutOfBounds(cell, _gridDimensions))
                    {
                        continue;
                    }

                    if (!PointFarFromPoints(point, _grid[(int)cell.y * _gridDimensions.x + (int)cell.x].streamline, distanceSq))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private bool PointFarFromPoints(float2 point, List<float2> streamline, float distanceSq)
        {
            foreach (float2 streamlineSegment in streamline)
            {
                if (streamlineSegment.Equals(point))
                {
                    continue;
                }

                float disSq = math.distancesq(streamlineSegment, point);

                if (disSq < distanceSq)
                {
                    return false;
                }
            }

            return true;
        }

        public void AddPolyline(float2[] streamline)
        {
            foreach (float2 v in streamline)
            {
                AddSample(v);
            }
        }

        private void AddSample(float2 point, float2? coords = null)
        {
            coords ??= GetSampleCoords(point);

            try
            {
                _grid[(int)coords.Value.y * _gridDimensions.x + (int)coords.Value.x].streamline.Add(point);
            }
            catch
            {
                int i = 0;
            }
        }

        private float2 GetSampleCoords(float2 worldPosition)
        {
            float2 v = WorldToGrid(worldPosition);
            return PointOutOfBounds(v, _worldDimensions) ? float2.zero : new float2(math.floor(v.x / _dsep), math.floor(v.y / _dsep));
        }

        private float2 WorldToGrid(float2 point)
        {
            return point - _origin;
        }

        private float2 GridToWorld(float2 point)
        {
            return point + _origin;
        }

        private bool PointOutOfBounds(float2 point, float2 bounds)
        {
            return point.x < 0 || point.y < 0 || point.x >= bounds.x || point.y >= bounds.y;
        }
    }
}
