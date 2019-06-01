using UnityEngine;
using System.Collections.Generic;

public class MeshBuilder {
	
	public enum Orientation {
		Universal,
		Lateral,
		Vertical
	}
	private Texture2D _noiseSource;
	private float _noiseScale;
	private Orientation _orientation;
	private bool _canPerturb;

	public MeshBuilder() {
		_canPerturb = false;
	}
	public MeshBuilder(Texture2D noiseSource, float noiseScale, Orientation orientation = Orientation.Universal) {
		_noiseSource = noiseSource;
		_noiseScale = noiseScale;
		_orientation = orientation;
		_canPerturb = true;
	}

	// **** Static Noise Perturbation **** //
	public static Vector4 SampleNoise(Vector3 position, Texture2D source, float scale){
		return source.GetPixelBilinear(position.x * scale, position.z * scale);
	}
	public static Vector3 Perturb (Vector3 position, float strength, Texture2D source, float scale, Orientation orientation = Orientation.Universal) {
		Vector4 sample = SampleNoise(position, source, scale);
		if(orientation == Orientation.Universal || orientation == Orientation.Lateral) position.x += (sample.x * 2f - 1f) * strength;
		if(orientation == Orientation.Universal || orientation == Orientation.Vertical) position.y += (sample.x * 2f - 1f) * strength;
		if(orientation == Orientation.Universal || orientation == Orientation.Lateral) position.z += (sample.z * 2f - 1f) * strength;
		return position;
	}

	// **** Building Triangles **** //
	public void AddTriangle(ref List<Vector3> vertices, ref List<int> triangles, Vector3 v1, Vector3 v2, Vector3 v3, float perturbStrength = 0f) {
		int vertexIndex = vertices.Count;
		if (perturbStrength > 0f && _canPerturb) {
			v1 = Perturb(v1, perturbStrength, _noiseSource, _noiseScale, _orientation);
			v2 = Perturb(v2, perturbStrength, _noiseSource, _noiseScale, _orientation);
			v3 = Perturb(v3, perturbStrength, _noiseSource, _noiseScale, _orientation);
		}
		vertices.Add (v1);
		vertices.Add (v2);
		vertices.Add (v3);
		triangles.Add (vertexIndex);
		triangles.Add (vertexIndex + 1);
		triangles.Add (vertexIndex + 2);
	}

	public void AddTriangleColor(ref List<Color> colors, Color color) {
		colors.Add (color);
		colors.Add (color);
		colors.Add (color);
	}

	public void AddTriangleColor(ref List<Color> colors, Color c1, Color c2, Color c3) {
		colors.Add (c1);
		colors.Add (c2);
		colors.Add (c3);
	}

	public void AddTriangleTerrain(ref List<Vector3> terrainTypes, Vector3 types) {
		terrainTypes.Add (types);
		terrainTypes.Add (types);
		terrainTypes.Add (types);
	}

	// **** Building Quads **** //
	public void AddQuad (ref List<Vector3> vertices, ref List<int> triangles, Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, float perturbStrength = 0f ) {
		if (perturbStrength > 0f && _canPerturb) {
			v1 = Perturb(v1, perturbStrength, _noiseSource, _noiseScale, _orientation);
			v2 = Perturb(v2, perturbStrength, _noiseSource, _noiseScale, _orientation);
			v3 = Perturb(v3, perturbStrength, _noiseSource, _noiseScale, _orientation);
			v4 = Perturb(v4, perturbStrength, _noiseSource, _noiseScale, _orientation);
		}
		int vertexIndex = vertices.Count;
		vertices.Add (v1);
		vertices.Add (v2);
		vertices.Add (v3);
		vertices.Add (v4);
		triangles.Add (vertexIndex);
		triangles.Add (vertexIndex + 2);
		triangles.Add (vertexIndex + 1);
		triangles.Add (vertexIndex + 1);
		triangles.Add (vertexIndex + 2);
		triangles.Add (vertexIndex + 3);
	}

	public void AddQuadColor (ref List<Color> colors, Color color) {
		colors.Add (color);
		colors.Add (color);
		colors.Add (color);
		colors.Add (color);
	}

	public void AddQuadColor (ref List<Color> colors, Color c1, Color c2) {
		colors.Add (c1);
		colors.Add (c1);
		colors.Add (c2);
		colors.Add (c2);
	}

	public void AddQuadColor (ref List<Color> colors, Color c1, Color c2, Color c3, Color c4) {
		colors.Add (c1);
		colors.Add (c2);
		colors.Add (c3);
		colors.Add (c4);
	}

	public void AddQuadTerrain(ref List<Vector3> terrainTypes, Vector3 types) {
		terrainTypes.Add (types);
		terrainTypes.Add (types);
		terrainTypes.Add (types);
		terrainTypes.Add (types);
	}
}
