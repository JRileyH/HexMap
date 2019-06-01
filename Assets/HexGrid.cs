using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class HexGrid : MonoBehaviour {
	public float WorldScale = 1f;
	public int chunkCountX = 1, chunkCountZ = 1;
	int cellCountX, cellCountZ;
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
		label.text = cell.coordinates.ToStringOnSeparateLines();
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

		Texture2D mapNoise = HexMetrics.Noise(seed: 100f, scale: WorldScale, width: cellCountX, height: cellCountZ, ascale: WorldScale * 0.9f);

		CreateChunks ();
		CreateCells (mapNoise);
	}

	public void ShowUI (bool visible) {
		for (int i = 0; i < chunks.Length; i++) {
			chunks[i].ShowUI(visible);
		}
	}
	
}
