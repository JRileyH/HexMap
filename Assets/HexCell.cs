using UnityEngine;

public class HexCell : MonoBehaviour {

	public enum TerrainType {
		Stone, Mud, Sand, Snow, Grass
	}
	public enum HexEdgeType {
		Flat, Slope, Cliff
	}
	public HexGridChunk chunk;
	public HexCoordinates coordinates;
	public RectTransform uiRect;
	int elevation = int.MinValue;

	[SerializeField]
	HexCell[] neighbors;

	public Vector3 Position {
		get {
			return transform.localPosition;
		}
	}
// why
	public int Elevation {
		get {
			return elevation;
		}
		set {
			if (elevation == value) {
				return;
			}

			elevation = Mathf.Clamp(value, HexMetrics.elevationMin, HexMetrics.elevationMax);

			//Set Cell Elevation
			Vector3 position = Position;
			position.y = elevation * HexMetrics.elevationStep;
			position = MeshBuilder.Perturb (position, HexMetrics.elevationPerturbStrength, HexMetrics.Noise(scale: 10f, shift: true), HexMetrics.noiseScale, MeshBuilder.Orientation.Vertical);
			transform.localPosition = position;

			//Set Cell Depth
			Depth = Mathf.InverseLerp(HexMetrics.elevationMin, HexMetrics.elevationMax, elevation);

			// Set Grid Number Elevation
			Vector3 uiPosition = uiRect.localPosition;
			uiPosition.z = -position.y - 0.1f;
			uiRect.localPosition = uiPosition;

			Refresh();
		}
	}

	[System.ComponentModel.DefaultValue(TerrainType.Grass)]
	public TerrainType Terrain { get; set; }

	public float Depth { get; private set; }

	public HexCell GetNeighbor (HexDirection direction) {
		return neighbors[(int)direction];
	}

	public HexEdgeType GetEdgeType (HexDirection direction) {
		HexCell neighbor = GetNeighbor(direction);
		if (elevation == neighbor.elevation) {
			return HexEdgeType.Flat;
		}
		if (Mathf.Abs(elevation - neighbor.elevation) < HexMetrics.slopeBreakPoint + 1) {
			return HexEdgeType.Slope;
		}
		return HexEdgeType.Cliff;
	}

	public void SetNeighbor (HexDirection direction, HexCell cell) {
		neighbors[(int)direction] = cell;
		cell.neighbors[(int)direction.Opposite()] = this;
	}

	void Refresh () {
		if (chunk) {
			chunk.Refresh ();
			for (int i = 0; i < neighbors.Length; i++) {
				HexCell neighbor = neighbors[i];
				if (neighbor != null && neighbor.chunk != chunk) {
					neighbor.chunk.Refresh();
				}
			}
		}
	}
}
