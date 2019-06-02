using UnityEngine;
using UnityEngine.UI;

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

	public HexCell PathFrom { get; set; }

	public int SearchHeuristic { get; set; }

	public int SearchPriority {
		get {
			return distance + SearchHeuristic;
		}
	}
	public HexCell NextWithSamePriority { get; set; }

	public Vector3 Position {
		get {
			return transform.localPosition;
		}
	}
	int distance;
	public int Distance {
		get { return distance; }
		set {
			distance = value;
			Text label = uiRect.GetComponent<Text>();
			label.text = distance == int.MaxValue ? "" : distance.ToString();
		}
	}
	public bool Passable = true;
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

	public HexDirection GetDirection (HexCell neighbor) {
		for (int i = 0; i < neighbors.Length; i++) {
			if(neighbors[i] == neighbor) return (HexDirection)i;
		}
		return HexDirection.NE;
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

	public void DisableHighlight () {
		Image highlight = uiRect.GetChild(0).GetComponent<Image>();
		highlight.enabled = false;
	}
	
	public void EnableHighlight () {
		EnableHighlight(Color.white);
	}

	public void EnableHighlight (Color color) {
		Image highlight = uiRect.GetChild(0).GetComponent<Image>();
		highlight.color = color;
		highlight.enabled = true;
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
