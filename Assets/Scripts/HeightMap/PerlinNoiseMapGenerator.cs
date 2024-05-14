using System.Collections.Generic;
using Simplicity.Noise;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Simplicity.ProceduralGeneration
{
    using ProcCityGen;

    public class PerlinNoiseMapGenerator
    {
        [field: SerializeField, Min(1), BoxGroup("Size", LabelText = "Size"), LabelText("Width")]
        public int MapWidth { get; private set; }

        [field: SerializeField, Min(1), BoxGroup("Size"), LabelText("Height")]
        public int MapHeight { get; private set; }

        [field: SerializeField, Min(0.001f), BoxGroup("Perlin", LabelText = "Perlin Noise Settings")]
        private float NoiseScale { get; set; }

        [field: SerializeField, Min(0), BoxGroup("Perlin")]
        private int Octaves { get; set; }

        [field: SerializeField, Range(0, 1), BoxGroup("Perlin")]
        private float Persistence { get; set; }

        [field: SerializeField, Min(1), BoxGroup("Perlin")]
        private float Lacunarity { get; set; }

        [field: SerializeField, BoxGroup("Perlin")]
        private Vector2 Offset { get; set; }

        [field: SerializeField]
        private bool UseFalloffMap { get; set; }

        private float[,] _falloffMap;

        [field: SerializeField, HorizontalGroup("Seed"), InlineButton("RandomSeed", "Generate New Seed")]
        public int Seed { get; private set; }

        [SerializeField, HorizontalGroup("Seed"), LabelText("Randomize"), LabelWidth(70)]
        private bool newSeedOnStart;

        [field: Title("Color Settings")]
        [field: SerializeField, InlineProperty, ListDrawerSettings(ShowFoldout = false)]
        public List<ColorRegions> Regions { get; private set; }

        [SerializeField]
        private Gradient noiseGradient;

        [ShowInInspector, HorizontalGroup(Title = "Debug Textures", GroupName = "VisualizeTextures"),
         PreviewField(100.0f, ObjectFieldAlignment.Center), HideLabel]
        private Texture2D _noiseTexture;

        [ShowInInspector, HorizontalGroup(Title = "Debug Textures", GroupName = "VisualizeTextures"),
         PreviewField(100.0f, ObjectFieldAlignment.Center), HideLabel]
        private Texture2D _colorTexture;

        [ShowInInspector, HorizontalGroup(Title = "Debug Textures", GroupName = "VisualizeTextures"),
         PreviewField(100.0f, ObjectFieldAlignment.Center), HideLabel]
        private Texture2D _falloffTexture;

        [field: SerializeField, HorizontalGroup("Generate", Width = 0.1f), HideLabel]
        private bool AutoUpdate { get; set; }

        private string ButtonName => AutoUpdate ? "Auto Update Enabled" : "Generate Noise";

        [Button(Name = "@this.ButtonName"), HorizontalGroup("Generate"), DisableIf("@this.AutoUpdate")]
        private void DebugGenerateNoise()
        {
            GenerateNoise();
        }

        [Button(Name = "Generate Color"), HorizontalGroup("Generate"), DisableIf("@this.AutoUpdate")]
        private void DebugGenerateColor()
        {
            GenerateColorDataFromNoise(GenerateNoise());
        }

        [Button(Name = "Generate Falloff"), HorizontalGroup("Generate"), DisableIf("@this.AutoUpdate")]
        private void DebugGenerateFalloff()
        {
            _falloffTexture = CreateFalloffMap();
        }

        private void RandomSeed()
        {
            Seed = Random.Range(int.MinValue, int.MaxValue);

            if (AutoUpdate)
                GenerateColorDataFromNoise(GenerateNoise());
        }
        
        public float[,] GenerateNoise()
        {
            var noise = PerlinNoise.GeneratePerlinNoise(MapWidth, MapHeight, Seed, NoiseScale, Octaves, Persistence,
                Lacunarity, Offset);

            if (UseFalloffMap)
            {
                CreateFalloffMap();

                for (var y = 0; y < MapHeight; y++)
                {
                    for (var x = 0; x < MapWidth; x++)
                        noise[x, y] = Mathf.Clamp01(noise[x, y] - _falloffMap[x, y]);
                }
            }

            _noiseTexture = CreateNoiseMap(noise);

            return noise;
        }

        public int[,] CreateIndexArrayFromNoiseHeight(float[,] noise)
        {
            var indices = new int[MapWidth, MapHeight];

            for (int y = 0; y < MapHeight; y++)
            {
                for (var x = 0; x < MapWidth; x++)
                {
                    var currentHeight = noise[x, y];
                    for (var r = 0; r < Regions.Count; r++)
                    {
                        if (currentHeight > Regions[r].Height)
                            continue;

                        indices[x, y] = r;
                        break;
                    }
                }
            }

            return indices;
        }

        public Color[] GenerateColorDataFromNoise(float[,] noise)
        {
            var indices = CreateIndexArrayFromNoiseHeight(noise);

            var colors = new Color[MapWidth * MapHeight];
            _colorTexture = new Texture2D(MapWidth, MapHeight);

            for (var y = 0; y < indices.GetLength(1); y++)
            {
                for (var x = 0; x < indices.GetLength(0); x++)
                    colors[y * indices.GetLength(1) + x] = Regions[indices[x, y]].Color;
            }

            _colorTexture.SetPixels(colors);
            _colorTexture.filterMode = FilterMode.Point;
            _colorTexture.Apply();

            return colors;
        }

        public Texture2D CreateColorTextureMap()
        {
            GenerateColorDataFromNoise(GenerateNoise());
            return _colorTexture;
        }

        private Texture2D CreateNoiseMap(float[,] noise)
        {
            var width = noise.GetLength(0);
            var height = noise.GetLength(1);

            var texture = new Texture2D(width, height);

            var colorMap = new Color[width * height];
            for (int y = 0, i = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++, i++)
                {
                    colorMap[i] = noiseGradient.Evaluate(noise[x, y]);
                }
            }

            texture.SetPixels(colorMap);
            texture.filterMode = FilterMode.Point;
            texture.Apply();

            return texture;
        }

        private Texture2D CreateFalloffMap()
        {
            var size = Mathf.Max(MapWidth, MapHeight);
            _falloffMap = FalloffGenerator.GenerateFalloffMap(size);
            return CreateNoiseMap(_falloffMap);
        }

        public struct ColorRegions
        {
            [field: SerializeField, HideLabel, HorizontalGroup]
            public Color Color { get; private set; }

            [field: SerializeField, HideLabel, HorizontalGroup, Range(0, 1)]
            public float Height { get; private set; }
        }
    }
}