using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class HexMesh : MonoBehaviour {

	Mesh hexMesh;
	MeshBuilder meshBuilder;
	MeshCollider meshCollider;
	static List<Vector3> vertices = new List<Vector3>();
	List<Vector3> terrainTypes = new List<Vector3>();
	static List<int> triangles = new List<int>();

	/// Colors to not translate directly into colors in game
	/// These colors are passed to the mesh shader as a mechanism for blending textures between cells via a splat map of color data
	/// Red represents the cells own terrain texture and translates to the solid terrain in the center of a cell
	/// Green represents the terrain of the direct neighbor being blended. It is blended with red across the bridge mesh (TriangulateBridge)
	/// Green represents the terrain of the next (clock-wise) neighbor to the one being blended. It is blended with red and green in the small corner filler triangle between bridges (TriangulateCorner)
	static List<Color> colors = new List<Color>();
	static Color ownTerrain = new Color(1f, 0f, 0f);
	static Color neighborTerrain = new Color(0f, 1f, 0f);
	static Color nextNeighborTerrain = new Color(0f, 0f, 1f);

	private Vector3 SolidTerrain(HexCell cell) {
		return Vector3.one * (int)cell.Terrain;
	}
	private Vector3 BlendedTerrain(HexCell cell, HexCell neighbor) {
		Vector3 blendedTerrain = SolidTerrain(cell);
		blendedTerrain.y = (float)neighbor.Terrain;
		return blendedTerrain;
	}
	private Vector3 BlendedTerrain(HexCell cell, HexCell neighbor, HexCell nextNeighbor) {
		Vector3 blendedTerrain = SolidTerrain(cell);
		blendedTerrain.y = (float)neighbor.Terrain;
		blendedTerrain.z = (float)nextNeighbor.Terrain;
		return blendedTerrain;
	}

	/// <summary>  
	///	Clears entire mesh data and recalculates each cell based for an entire chunk
	/// </summary>  
	public void Triangulate(HexCell[] cells) {
		hexMesh.Clear();
		vertices.Clear();
		terrainTypes.Clear();
		triangles.Clear();
		colors.Clear();
		for (int i = 0; i < cells.Length; i++) {
			Triangulate (cells [i]);
		}
		hexMesh.SetVertices(vertices);
		hexMesh.SetTriangles(triangles, 0);
		hexMesh.SetColors(colors);
		hexMesh.SetUVs(2, terrainTypes);
		hexMesh.RecalculateNormals();
		meshCollider.sharedMesh = hexMesh;
	}

	void Triangulate(HexCell cell) {
		for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++) {
			Triangulate(d, cell);
		}
	}

	/// <summary>  
	///	Generated the triangles and mesh data for the center of a hex cell
	/// Also calls further traingulation functions to fill out cell connection triangles
	/// </summary>  
	void Triangulate(HexDirection direction, HexCell cell) {
		Vector3 v1 = cell.transform.localPosition;
		Vector3 begin = v1 + HexMetrics.GetFirstSolidCorner(direction);
		Vector3 end = v1 + HexMetrics.GetSecondSolidCorner(direction);
		for (int i = 1; i <= HexMetrics.hexEdgeFactor; i++) {
			float first = (i - 1) / (float)HexMetrics.hexEdgeFactor;
			float second = i / (float)HexMetrics.hexEdgeFactor;

			Vector3 v2 = Vector3.Lerp (begin, end, first);
			Vector3 v3 = Vector3.Lerp (begin, end, second);

			meshBuilder.AddTriangle(ref vertices, ref triangles, v1, v2, v3, HexMetrics.cellPerturbStrength);
			meshBuilder.AddTriangleColor(ref colors, ownTerrain);
			meshBuilder.AddTriangleTerrain(ref terrainTypes, SolidTerrain(cell));

			if (direction <= HexDirection.SE) {
				
				TriangulateBridge(direction, cell, v2, v3);
			}
			if (direction <= HexDirection.E && i == HexMetrics.hexEdgeFactor) {

				TriangulateCorner (direction, cell, end);
			}
		}
	}

	/// <summary>  
	///	Generates the bridge of trianges between two hex cells in a specified direction
	/// </summary>  
	void TriangulateBridge(HexDirection direction, HexCell cell, Vector3 v1, Vector3 v2) {
		HexCell neighbor = cell.GetNeighbor(direction);
		if (neighbor == null) return;
		Vector3 bridge = HexMetrics.GetBridge (direction);
		Vector3 v3 = v1 + bridge;
		Vector3 v4 = v2 + bridge;
		v3.y = v4.y = neighbor.Position.y;

		meshBuilder.AddQuad(ref vertices, ref triangles, v1, v2, v3, v4, HexMetrics.cellPerturbStrength);
		meshBuilder.AddQuadColor(ref colors, ownTerrain, neighborTerrain);
		meshBuilder.AddQuadTerrain(ref terrainTypes, BlendedTerrain(cell, neighbor));
	}

	/// <summary>  
	///	Generates the small triangle that lies between three hex cells' bridges
	/// </summary>  
	void TriangulateCorner(HexDirection direction, HexCell cell, Vector3 v1) {
		HexCell neighbor = cell.GetNeighbor(direction);
		HexCell nextNeighbor = cell.GetNeighbor(direction.Next());
		if (neighbor == null || nextNeighbor == null) return;
		Vector3 v2 = v1 + HexMetrics.GetBridge(direction);
		v2.y = neighbor.Position.y;
		Vector3 v3 = v1 + HexMetrics.GetBridge(direction.Next());
		v3.y = nextNeighbor.Position.y;

		meshBuilder.AddTriangle(ref vertices, ref triangles, v1, v2, v3, HexMetrics.cellPerturbStrength);
		meshBuilder.AddTriangleColor(ref colors, ownTerrain, neighborTerrain, nextNeighborTerrain);
		meshBuilder.AddTriangleTerrain(ref terrainTypes, BlendedTerrain(cell, neighbor, nextNeighbor));
	}

	void Awake () {
		GetComponent<MeshFilter>().mesh = hexMesh = new Mesh();
		GetComponent<MeshRenderer>().material.SetInt("_MinDepth", (int)(HexMetrics.elevationMin * HexMetrics.elevationStep));
		GetComponent<MeshRenderer>().material.SetInt("_MaxDepth", (int)(HexMetrics.elevationMax * HexMetrics.elevationStep));
		GetComponent<MeshRenderer>().material.SetColor("_Color", HexMetrics.waterColor);
		meshBuilder = new MeshBuilder(HexMetrics.Noise(scale: 10f, shift: true), HexMetrics.noiseScale, MeshBuilder.Orientation.Lateral);
		meshCollider = gameObject.AddComponent<MeshCollider>();
		hexMesh.name = "Hex Mesh";
	}
}