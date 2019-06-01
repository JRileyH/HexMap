using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class HexMapEditor : MonoBehaviour {

	public HexGrid hexGrid;
	private int activeElevation;
	private int brushSize;
	private bool mouseDown, rightMouseDown = false;
	private enum Tool {
		Shovel
	}
	private Tool currentTool = Tool.Shovel;

	void Awake () {
		Slider slider = GameObject.Find ("Terraform Slider").GetComponent <Slider> ();
		slider.minValue = HexMetrics.elevationMin;
		slider.maxValue = HexMetrics.elevationMax;
		SetBrushSize(0);
		SelectElevation(1);
	}

	void Update() {
		if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject()) {
			HandleClick(true);
			mouseDown = true;
		} else if(mouseDown) {
			mouseDown = false;
		}
		if (Input.GetMouseButton(1) && !EventSystem.current.IsPointerOverGameObject()) {
			HandleClick(false);
			rightMouseDown = true;
		} else if(rightMouseDown) {
			rightMouseDown = false;
		}
	}

	void HandleClick (bool mainAction) {
		Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast(inputRay, out hit)) {
			EditCells(hexGrid.GetCell(hit.point), mainAction);
		}
	}

	void EditCells(HexCell center, bool mainAction) {
		int centerX = center.coordinates.X;
		int centerZ = center.coordinates.Z;
		for (int r = 0, z = centerZ - brushSize; z <= centerZ; z++, r++) {
			for (int x = centerX - r; x <= centerX + brushSize; x++) {
				EditCell(hexGrid.GetCell(new HexCoordinates(x, z)), mainAction);
			}
		}
		for (int r = 0, z = centerZ + brushSize; z > centerZ; z--, r++) {
			for (int x = centerX - brushSize; x <= centerX + r; x++) {
				EditCell(hexGrid.GetCell(new HexCoordinates(x, z)), mainAction);
			}
		}
	}

	void EditCell(HexCell cell, bool mainAction) {
		if(cell == null) return;
		switch (currentTool) {
		case Tool.Shovel:
			if (mainAction) {
				cell.Elevation = activeElevation;
			} else { 
				cell.Elevation = 0;
			}
			break;
		}
	}

	public void SelectElevation(float value) {
		activeElevation = (int)value;
	}

	public void SetBrushSize (float size) {
		brushSize = (int)size;
	}

	public void SelectShovel () {
		currentTool = Tool.Shovel;
	}

	public void ShowUI (bool visible) {
		hexGrid.ShowUI(visible);
	}
}