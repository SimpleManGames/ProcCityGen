namespace ProcCityGen.Fields.Tensors.Tracing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using ProcCityGen.Interfaces.Fields.Tracing;

    using Unity.Mathematics;

    using UnityEngine;

    using Random = Unity.Mathematics.Random;

    public class Streamlines
    {
        public List<float2[]> allStreamlines = new List<float2[]>();
        public List<float2[]> majorStreamlines = new List<float2[]>();
        public List<float2[]> minorStreamlines = new List<float2[]>();

        private Random _r = new Random();

        private StreamlineGrid _majorGrid;
        private StreamlineGrid _minorGrid;

        private readonly ITracingStrategy _strategy;
        private readonly float2 _origin;
        private readonly float2 _worldSize;
        private readonly StreamlineParams _streamlineStreamlineParams;
        private StreamlineParams _streamlineStreamlineParamsSq;

        private float _streamlineStep;
        private float _streamlineLookBack;
        private float _dCollideSelfSq;

        private Queue<float2> _candidateSeedsMajor = new Queue<float2>();
        private Queue<float2> _candidateSeedsMinor = new Queue<float2>();

        private bool _streamlinesDone = true;

        private Action _resolve;
        private bool _lastStreamlineMajor = true;

        private readonly bool _seedAtEndpoints = false;

        public Streamlines(ITracingStrategy strategy, float2 origin, float2 worldSize, StreamlineParams streamlineStreamlineParams)
        {
            _strategy = strategy;
            _origin = origin;
            _worldSize = worldSize;
            _streamlineStreamlineParams = streamlineStreamlineParams;

            _streamlineStreamlineParams.dtest = math.min(_streamlineStreamlineParams.dtest, _streamlineStreamlineParams.dsep);

            _dCollideSelfSq = math.pow(_streamlineStreamlineParams.dcirclejoin / 2, 2);
            _streamlineStep = math.floor(_streamlineStreamlineParams.dcirclejoin / _streamlineStreamlineParams.dstep);
            _streamlineLookBack = 2 * _streamlineStep;

            _majorGrid = new StreamlineGrid(worldSize, origin, _streamlineStreamlineParams.dsep);
            _minorGrid = new StreamlineGrid(worldSize, origin, _streamlineStreamlineParams.dsep);

            _r.InitState();

            SetParamsSq();
        }

        public void JoinLooseStreamlines()
        {
            JoinLooseStreamlineStep(true);
            JoinLooseStreamlineStep(false);
        }

        private void JoinLooseStreamlineStep(bool isMajor)
        {
            List<float2[]> streamlines = GetStreamline(isMajor);

            for (int index = 0; index < streamlines.Count; index++)
            {
                float2[] streamline = streamlines[index];

                if (streamline[0].Equals(streamline[^1]))
                {
                    continue;
                }

                float2? newStartNullable = GetBestNextPoint(streamline[0], streamline[4], streamline);

                if (!newStartNullable.HasValue)
                {
                    continue;
                }

                float2 newStart = newStartNullable.Value;

                foreach (float2 p in PointsBetween(streamline[0], newStart, _streamlineStreamlineParams.dstep))
                {
                    // Not the best approach for adding to the front of an array
                    if (isMajor)
                    {
                        majorStreamlines[index] = streamline.Prepend(p).ToArray();
                    }
                    else
                    {
                        minorStreamlines[index] = streamline.Prepend(p).ToArray();
                    }
                    GetGrid(isMajor).AddSample(p);
                }
            }
        }

        private float2[] PointsBetween(float2 v1, float2 v2, float distance)
        {
            float d = math.distance(v1, v2);
            float steps = math.floor(d / distance);

            if (steps == 0)
            {
                return new float2[]
                    { };
            }

            float2 stepVector = v2 - v1;
            List<float2> output = new List<float2>();

            int i = 1;
            float2 next = v1 + stepVector * new float2(i / steps);

            for (i = 1; i < steps; i++)
            {
                if (math.lengthsq(_strategy.Trace(next, true)) > 0.001)
                {
                    output.Add(next);
                }
                else
                {
                    return output.ToArray();
                }

                next = v1 + stepVector * new float2(i / steps);
            }

            return output.ToArray();
        }

        public bool CreateStreamlines(bool isMajor)
        {
            float2? seedNullable = GetSeed(isMajor);

            if (seedNullable == null)
            {
                return false;
            }

            float2 seed = seedNullable.Value;

            float2[] streamline = TraceStreamline(seed, isMajor);

            if (!ValidStreamline(streamline))
                return true;

            GetGrid(isMajor).AddPolyline(streamline);
            GetStreamline(isMajor).Add(streamline);
            allStreamlines.Add(streamline);

            if (streamline[0].Equals(streamline[^1]))
                return true;

            CandidateSeeds(!isMajor).Enqueue(streamline[0]);
            CandidateSeeds(!isMajor).Enqueue(streamline[^1]);

            return true;
        }

        private float2? GetBestNextPoint(float2 point, float2 previousPoint, float2[] streamline)
        {
            float2[] nearbyPoints = _majorGrid.GetNearbyPoints(point, _streamlineStreamlineParams.dlookahead);
            float2[] minorNearbyPoints = _minorGrid.GetNearbyPoints(point, _streamlineStreamlineParams.dlookahead);

            foreach (float2 minorNearbyPoint in minorNearbyPoints)
            {
                nearbyPoints = nearbyPoints.Append(minorNearbyPoint).ToArray();
            }

            float2 direction = point - previousPoint;

            float2? closestSample = null;
            float closestDistance = float.MaxValue;

            foreach (float2 sample in nearbyPoints)
            {
                if (sample.Equals(point) || sample.Equals(previousPoint))
                    continue;

                float2 differenceVector = sample - point;

                if (math.dot(differenceVector, direction) > 0)
                {
                    continue;
                }

                float distanceToSample = math.distancesq(point, sample);

                if (distanceToSample < 2 * _streamlineStreamlineParamsSq.dstep)
                {
                    closestSample = sample;
                    break;
                }

                float angleBetween = math.abs(math.atan2(differenceVector.y - direction.y, differenceVector.x - direction.x));

                if (!(angleBetween < _streamlineStreamlineParams.joinangle) || !(distanceToSample < closestDistance))
                    continue;

                closestDistance = distanceToSample;
                closestSample = sample;
            }

            if (closestSample != null)
            {
                closestSample += math.normalize(direction) * _streamlineStreamlineParams.simplifyTolerance * 4;
            }

            return closestSample;
        }

        private bool ValidStreamline(float2[] streamline) => streamline.Length > 5;

        private void SetParamsSq()
        {
            _streamlineStreamlineParamsSq = new StreamlineParams()
            {
                dsep = _streamlineStreamlineParams.dsep * _streamlineStreamlineParams.dsep,
                dtest = _streamlineStreamlineParams.dtest * _streamlineStreamlineParams.dtest,
                dstep = _streamlineStreamlineParams.dstep * _streamlineStreamlineParams.dstep,
                dcirclejoin = _streamlineStreamlineParams.dcirclejoin * _streamlineStreamlineParams.dcirclejoin,
                dlookahead = _streamlineStreamlineParams.dlookahead * _streamlineStreamlineParams.dlookahead,
                joinangle = _streamlineStreamlineParams.joinangle * _streamlineStreamlineParams.joinangle,
                pathIterations = _streamlineStreamlineParams.pathIterations * _streamlineStreamlineParams.pathIterations,
                seedTries = _streamlineStreamlineParams.seedTries * _streamlineStreamlineParams.seedTries,
                simplifyTolerance = _streamlineStreamlineParams.simplifyTolerance * _streamlineStreamlineParams.simplifyTolerance,
                collideEarly = _streamlineStreamlineParams.collideEarly * _streamlineStreamlineParams.collideEarly
            };
        }

        private float2[] TraceStreamline(float2 point, bool isMajor)
        {
            int count = 0;
            bool pointsEscaped = false;

            bool collideBoth = _r.NextFloat(0.0f, 1.0f) < _streamlineStreamlineParams.collideEarly;

            float2 d = _strategy.Trace(point, isMajor);

            TracingParams forwardParams = new TracingParams()
            {
                point = point,
                originalDirection = d,
                streamline = new List<float2>
                {
                    point
                },
                previousDirection = d,
                previousPoint = point + d,
                isValid = true
            };
            forwardParams.isValid = PointInBounds(point);

            float2 negD = new float2(-d.x, -d.y);

            TracingParams backwardsParams = new TracingParams()
            {
                point = point,
                originalDirection = negD,
                streamline = new List<float2>
                {
                    point
                },
                previousDirection = negD,
                previousPoint = point + negD,
                isValid = true
            };
            backwardsParams.isValid = PointInBounds(point);

            while (count < _streamlineStreamlineParams.pathIterations && (forwardParams.isValid || backwardsParams.isValid))
            {
                StreamlineTraceStep(forwardParams, isMajor, collideBoth);
                StreamlineTraceStep(backwardsParams, isMajor, collideBoth);

                float2 distance = math.distance(forwardParams.previousPoint, backwardsParams.previousPoint);
                float sqDistanceBetweenPoints = math.distancesq(distance.x, distance.y);

                if (!pointsEscaped && sqDistanceBetweenPoints > _streamlineStreamlineParamsSq.dcirclejoin)
                {
                    pointsEscaped = true;
                }

                if (pointsEscaped && sqDistanceBetweenPoints <= _streamlineStreamlineParamsSq.dcirclejoin)
                {
                    forwardParams.streamline.Add(forwardParams.previousPoint);
                    forwardParams.streamline.Add(backwardsParams.previousPoint);
                    backwardsParams.streamline.Add(backwardsParams.previousPoint);
                    break;
                }

                count++;
            }

            backwardsParams.streamline.Reverse();
            backwardsParams.streamline.AddRange(forwardParams.streamline);
            return backwardsParams.streamline.Distinct().ToArray();
        }

        private bool PointInBounds(float2 point)
        {
            return point.x >= _origin.x && point.y >= _origin.y && point.x < _worldSize.x + _origin.x && point.y < _worldSize.y + _origin.y;
        }

        private void StreamlineTraceStep(TracingParams tracingParams, bool isMajor, bool collideBoth)
        {
            if (!tracingParams.isValid)
            {
                return;
            }

            tracingParams.streamline.Add(tracingParams.previousPoint);

            float2 nextDirection = _strategy.Trace(tracingParams.previousPoint, isMajor);

            if (math.lengthsq(nextDirection) < 0.01f)
            {
                tracingParams.isValid = false;
                return;
            }

            if (math.dot(nextDirection, tracingParams.previousDirection) < 0)
            {
                nextDirection = new float2(-nextDirection.x, -nextDirection.y);
            }

            float2 nextPoint = tracingParams.previousPoint + nextDirection;

            if (PointInBounds(nextPoint) &&
                IsValidSample(isMajor, nextPoint, _streamlineStreamlineParamsSq.dtest, collideBoth) &&
                !HasStreamlineTurned(tracingParams.point, tracingParams.originalDirection, nextPoint, nextDirection))
            {
                tracingParams.previousPoint = nextPoint;
                tracingParams.previousDirection = nextDirection;
            }
            else
            {
                tracingParams.streamline.Add(nextPoint);
                tracingParams.isValid = false;
            }
        }

        private bool HasStreamlineTurned(float2 point, float2 originalDirection, float2 nextPoint, float2 direction)
        {
            if (!(math.dot(originalDirection, direction) < 0))
            {
                return false;
            }

            float2 perpendicularVector = new float2(originalDirection.y, -originalDirection.x);
            bool isLeft = math.dot(point - nextPoint, perpendicularVector) < 0;
            bool directionUp = math.dot(direction, perpendicularVector) > 0;
            return isLeft == directionUp;
        }

        private bool IsValidSample(bool isMajor, float2 point, float distanceSq, bool bothGrids = false)
        {
            bool gridValid = GetGrid(isMajor).IsValidSample(point, distanceSq);

            if (bothGrids)
            {
                gridValid = gridValid && GetGrid(!isMajor).IsValidSample(point, distanceSq);
            }

            return _strategy.OnLand(point) && gridValid;
        }

        private StreamlineGrid GetGrid(bool isMajor) => isMajor ? _majorGrid : _minorGrid;

        private float2? GetSeed(bool isMajor)
        {
            if (_seedAtEndpoints && CandidateSeeds(isMajor).Count > 0)
            {
                while (CandidateSeeds(isMajor).Count > 0)
                {
                    float2 queueSeed = CandidateSeeds(isMajor).Dequeue();

                    if (IsValidSample(isMajor, queueSeed, _streamlineStreamlineParamsSq.dsep))
                    {
                        return queueSeed;
                    }
                }
            }

            float2 seed = SampleRandomPoint();
            int i = 0;

            while (!IsValidSample(isMajor, seed, _streamlineStreamlineParamsSq.dsep))
            {
                if (i >= _streamlineStreamlineParamsSq.seedTries)
                {
                    return null;
                }

                seed = SampleRandomPoint();
                i++;
            }

            return seed;
        }

        private Queue<float2> CandidateSeeds(bool isMajor) => isMajor ? _candidateSeedsMajor : _candidateSeedsMinor;

        private float2 SampleRandomPoint() => _r.NextFloat2(float2.zero, _worldSize) + _origin;

        private List<float2[]> GetStreamline(bool isMajor) => isMajor ? majorStreamlines : minorStreamlines;

        [Serializable]
        public struct StreamlineParams
        {
            public float dsep;
            public float dtest;
            public float dstep;
            public float dcirclejoin;
            public float dlookahead;
            public float joinangle;
            public float pathIterations;
            public float seedTries;
            public float simplifyTolerance;

            [SerializeField, Range(0.0f, 1.0f)]
            public float collideEarly;
        }

        private class TracingParams
        {
            public float2 point;
            public float2 originalDirection;
            public List<float2> streamline;
            public float2 previousDirection;
            public float2 previousPoint;
            public bool isValid;
        }
    }
}
