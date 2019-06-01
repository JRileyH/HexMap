using UnityEngine;
using System.Collections.Generic;
using System.IO;

public static class HexMetrics {
	public const int chunkSizeX = 5, chunkSizeZ = 5;
	public const float cellPerturbStrength = 3f;
	public const float elevationPerturbStrength = 2f;
	public const float outerRadius = 10f;
	public const float innerRadius = outerRadius * 0.866025404f;
	public const float solidFactor = 0.80f;
	public const float blendFactor = 1f - solidFactor;
	public static readonly Color waterColor = new Color(0.05f, 0.65f, 0.81f, 1f);
	public const int elevationMax = 5;
	public const int elevationMin = -3;
	public const float elevationStep = 5f;
	public const int slopeBreakPoint = 2;
	public const int hexEdgeFactor = 4;
	public const float noiseScale = 0.003f;
	static Vector3[] corners = {
		new Vector3(0f, 0f, outerRadius),
		new Vector3(innerRadius, 0f, 0.5f * outerRadius),
		new Vector3(innerRadius, 0f, -0.5f * outerRadius),
		new Vector3(0f, 0f, -outerRadius),
		new Vector3(-innerRadius, 0f, -0.5f * outerRadius),
		new Vector3(-innerRadius, 0f, 0.5f * outerRadius),
		new Vector3(0f, 0f, outerRadius)
	};
	public static Vector3 GetFirstCorner (HexDirection direction) {
		return corners[(int)direction];
	}
	public static Vector3 GetSecondCorner (HexDirection direction) {
		return corners[(int)direction + 1];
	}
	public static Vector3 GetFirstSolidCorner (HexDirection direction) {
		return corners[(int)direction] * solidFactor;
	}
	public static Vector3 GetSecondSolidCorner (HexDirection direction) {
		return corners[(int)direction + 1] * solidFactor;
	}
	public static Vector3 GetBridge (HexDirection direction) {
		return (corners[(int)direction] + corners[(int)direction + 1]) * blendFactor;
	}
	private static Dictionary<string, Texture2D> NoiseCache = new Dictionary<string, Texture2D>();

	private static readonly string noisePath = Application.dataPath + "/../noise/";
	public static Texture2D Noise(
		float seed = 1f,
		float scale = 1f,
		float rscale = -1f,
		float gscale = -1f,
		float bscale = -1f,
		float ascale = -1f,
		int width = 512,
		int height = 512,
		bool shift = false,
		bool store = false
	) {
		if(rscale < 0) rscale = scale;
		if(gscale < 0) gscale = scale;
		if(bscale < 0) bscale = scale;
		if(ascale < 0) ascale = scale;
		string key = 
			seed.ToString() + "-" +
			rscale.ToString() + "," +
			gscale.ToString() + "," +
			bscale.ToString() + "," +
			ascale.ToString() + "-" +
			width.ToString() + "x" + height.ToString() + "-" +
			(shift ? "s" : "u");
		Texture2D noise = null;
		if(!NoiseCache.TryGetValue(key, out noise)) {
			if(store && File.Exists(noisePath + key + ".png")) {
				byte[] bx = File.ReadAllBytes(noisePath + key + ".png");
				noise = new Texture2D(1, 1);
				noise.LoadImage(bx);
			} else {
				noise = new Texture2D(width, height);
				Color[] px = new Color[width * height];
				int _shift = shift ? 1 : 0;
				for(int x = 0; x < width; x++) {
					for(int y = 0; y < height; y++) {
						float _x = (x / (float)width);
						float _y = (y / (float)height);
						float _r = Mathf.PerlinNoise((_x * rscale) + seed + (_shift * 1), (_y * rscale) + seed + (_shift * 1));
						float _g = Mathf.PerlinNoise((_x * gscale) + seed + (_shift * 2), (_y * gscale) + seed + (_shift * 2));
						float _b = Mathf.PerlinNoise((_x * bscale) + seed + (_shift * 3), (_y * bscale) + seed + (_shift * 3));
						float _a = Mathf.PerlinNoise((_x * ascale) + seed, (_y * ascale) + seed);
						px[x + (y * width)] = new Color(_r, _g, _b, _a);
					}
				}
				noise.SetPixels(px);
				noise.Apply();
				if (store) {
					byte[] bx = noise.EncodeToPNG();
					File.WriteAllBytes(noisePath + key + ".png", bx);
				}
			}
			NoiseCache.Add(key, noise);
		}
		return noise;
	}

	static HexMetrics() {

	}
}
