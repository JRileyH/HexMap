using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class HexGrid : MonoBehaviour {
	public float WorldScale = 1f;
	public int chunkCountX = 1, chunkCountZ = 1;
	int cellCountX, cellCountZ;
	HexCellPriorityQueue searchFrontier;
	public HexGridChunk chunkPrefab;
	public HexCell cellPrefab;
	public Text cellLabelPrefab;

	HexCell[] cells;
	HexGridChunk[] chunks;

	void CreateChunks () {
		chunks = new HexGridChunk[chunkCountX * chunkCountZ];

		for (int z = 0, i = 0; z < chunkCountZ; z++) {
			for (int x = 0; x < chunkCountX; x++) {
				HexGridChunk chunk = chunks[i++] = Instantiate(chunkPrefab);
				chunk.transform.SetParent(transform);
			}
		}
	}

	void CreateCells (Texture2D noise = null) {
		cells = new HexCell[cellCountZ * cellCountX];

		for (int z = 0, i = 0; z < cellCountZ; z++) {
			for (int x = 0; x < cellCountX; x++) {
				HexCell createdCell = CreateCell (i++, x, z);
				if(noise != null) {
					Color cellPixel = noise.GetPixel(x, z);
					int e = (int)Mathf.Round(Mathf.Lerp(HexMetrics.elevationMin, HexMetrics.elevationMax, cellPixel.r));
					int t = (int)Mathf.Round(Mathf.Lerp(0, 5, cellPixel.a));
					createdCell.Elevation = e;
					createdCell.Terrain = (HexCell.TerrainType)t;
				}
			}
		}
	}

	HexCell CreateCell(int i, int x, int z) {
		
		// Create Grid Cell
		Vector3 position = new Vector3 (
			(x + z * 0.5f - z / 2) * (HexMetrics.innerRadius * 2f),
			0f,
			z * (HexMetrics.outerRadius * 1.5f)
		);
		HexCell cell = cells[i] = Instantiate<HexCell>(cellPrefab);
		cell.transform.localPosition = position;
		cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);

		// Set Cell neighbors
		if (x > 0) {
			cell.SetNeighbor(HexDirection.W, cells[i - 1]);
		}
		if (z > 0) {
			if ((z & 1) == 0) {
				cell.SetNeighbor(HexDirection.SE, cells[i - cellCountX]);
				if (x > 0) {
					cell.SetNeighbor(HexDirection.SW, cells[i - cellCountX - 1]);
				}
			}
			else {
				cell.SetNeighbor(HexDirection.SW, cells[i - cellCountX]);
				if (x < cellCountX - 1) {
					cell.SetNeighbor(HexDirection.SE, cells[i - cellCountX + 1]);
				}
			}
		}

		// Create Grid Cell Overlay
		Text label = Instantiate<Text>(cellLabelPrefab);
		label.rectTransform.anchoredPosition = new Vector2(position.x, position.z);
		label.text = "";
		cell.uiRect = label.rectTransform;

		AddCellToChunk (x, z, cell);

		return cell;
	}

	void AddCellToChunk (int x, int z, HexCell cell) {
		int chunkX = x / HexMetrics.chunkSizeX;
		int chunkZ = z / HexMetrics.chunkSizeZ;
		HexGridChunk chunk = chunks[chunkX + chunkZ * chunkCountX];

		int localX = x - chunkX * HexMetrics.chunkSizeX;
		int localZ = z - chunkZ * HexMetrics.chunkSizeZ;
		chunk.AddCell(localX + localZ * HexMetrics.chunkSizeX, cell);
	}

	public Material terrainMaterial;
	public void ShowGrid (bool visible) {
		if (visible) {
			terrainMaterial.EnableKeyword("GRID_ON");
		}
		else {
			terrainMaterial.DisableKeyword("GRID_ON");
		}
	}

	public List<HexCell> FindPath (HexCell fromCell, HexCell toCell) {
		List<HexCell> list = new List<HexCell>();
		foreach(HexCell c in Search(fromCell, toCell)) {
			c.EnableHighlight(Color.green);
			list.Insert(0, c);
		}
		list.Add(toCell);
		return list;
	}

	IEnumerable<HexCell> Search (HexCell fromCell, HexCell toCell) {
		if (searchFrontier == null) {
			searchFrontier = new HexCellPriorityQueue();
		}
		else {
			searchFrontier.Clear();
		}

		for (int i = 0; i < cells.Length; i++) {
			cells[i].Distance = int.MaxValue;
			cells[i].DisableHighlight();
		}
		fromCell.EnableHighlight(Color.blue);
		toCell.EnableHighlight(Color.red);

		fromCell.Distance = 0;
		searchFrontier.Enqueue(fromCell);
		while (searchFrontier.Count > 0) {
			HexCell current = searchFrontier.Dequeue();

			if (current == toCell) {
				current = current.PathFrom;
				while (current != fromCell) {
					yield return current;
					current = current.PathFrom;
				}
				break;
			}

			for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++) {
				HexCell neighbor = current.GetNeighbor(d);
				if (neighbor == null || !neighbor.Passable) {
					continue;
				}
				int weight = Mathf.Max(neighbor.Elevation - current.Elevation, 0) + 1;
				int distance = current.Distance + weight;
				if (neighbor.Distance == int.MaxValue) {
					neighbor.Distance = distance;
					neighbor.PathFrom = current;
					neighbor.SearchHeuristic = neighbor.coordinates.DistanceTo(toCell.coordinates);
					searchFrontier.Enqueue(neighbor);
				} else if (distance < neighbor.Distance) {
					int oldPriority = neighbor.SearchPriority;
					neighbor.Distance = distance;
					neighbor.PathFrom = current;
					searchFrontier.Change(neighbor, oldPriority);
				}
			}
		}
	}

	public HexCell GetCell(Vector3 position) {
		position = transform.InverseTransformPoint(position);
		return GetCell(HexCoordinates.FromPosition(position));
	}
	public HexCell GetCell (HexCoordinates coordinates) {
		int z = coordinates.Z;
		if (z < 0 || z >= cellCountZ) {
			return null;
		}
		int x = coordinates.X + z / 2;
		if (x < 0 || x >= cellCountX) {
			return null;
		}
		return cells[x + z * cellCountX];
	}

	void Awake() {

		chunkCountX = (int)(chunkCountX * WorldScale);
		chunkCountZ = (int)(chunkCountZ * WorldScale);

		cellCountX = chunkCountX * HexMetrics.chunkSizeX;
		cellCountZ = chunkCountZ * HexMetrics.chunkSizeZ;

		Texture2D mapNoise = HexMetrics.Noise(seed: 19f, scale: WorldScale, width: cellCountX, height: cellCountZ, ascale: WorldScale * 0.9f);

		CreateChunks ();
		CreateCells (mapNoise);
	}

	public void ShowUI (bool visible) {
		for (int i = 0; i < chunks.Length; i++) {
			chunks[i].ShowUI(visible);
		}
	}
	
}
