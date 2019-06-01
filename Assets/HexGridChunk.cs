using UnityEngine;
using UnityEngine.UI;

public class HexGridChunk : MonoBehaviour {

	HexCell[] cells;

	HexMesh hexMesh;
	Canvas gridCanvas;

	public void AddCell (int index, HexCell cell) {
		cells[index] = cell;
		cell.transform.SetParent(transform, false);
		cell.uiRect.SetParent(gridCanvas.transform, false);
		cell.Elevation = 0;
		cell.chunk = this;
	}

	void Awake () {
		gridCanvas = GetComponentInChildren<Canvas>();
		hexMesh = GetComponentInChildren<HexMesh>();

		cells = new HexCell[HexMetrics.chunkSizeX * HexMetrics.chunkSizeZ];
		ShowUI(false);
	}

	public void ShowUI (bool visible) {
		gridCanvas.gameObject.SetActive(visible);
	}

	public void Refresh () {
		enabled = true;
	}

	public void LateUpdate () {
		hexMesh.Triangulate(cells);
		enabled = false;
	}
}