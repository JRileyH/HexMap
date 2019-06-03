using UnityEngine;
using System.Collections.Generic;

public class Player : MonoBehaviour {

	bool mouseDown = false;
	HexCell cell, destCell;
	List<HexCell> destPath = new List<HexCell>();
	HexDirection direction, destDirection;

	Vector3 hover = Vector3.up * 5f;
	public HexGrid grid;
	Transform swivel, stick, cam;
	public float stickMinZoom, stickMaxZoom, stickElevation = 0;
	public float moveSpeedMinZoom, moveSpeedMaxZoom;
	public float rotationSpeed;
	float rotationAngle;
	float zoom = 1f;
	
	public void AddToPath(List<HexCell> list) {
		destPath.AddRange(list);
	}

	public void AddToPath(HexCell toCell) {
		AddToPath(grid.FindPath(cell, toCell));
	}
	public void AddToPath(HexDirection dir) {
		HexCell next = destCell.GetNeighbor(dir);
		if (next != null) {
			destPath.Add(next);
		}
	}
	public HexCell Next() {
		if(destPath.Count > 0) {
			HexCell next = destPath[0];
			destDirection = cell.GetDirection(next);
			return next;
		}
		float currDelta = Mathf.Abs(rotationAngle - direction.Angle());
		float destDelta = Mathf.Abs(rotationAngle - destDirection.Angle());
		if (currDelta < destDelta) {
			AddToPath(direction);
		} else {
			AddToPath(destDirection);
		}
		return Next();
	}

	void Update () {

		if (!mouseDown && Input.GetMouseButton(0)) {
			Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast(inputRay, out hit)) {
				destPath.Clear();
				AddToPath(grid.GetCell(hit.point));
			}
			mouseDown = true;
		} else if(mouseDown && !Input.GetMouseButton(0)) {
			mouseDown = false;
		}

		// Handle Rotating
		float rotationInput = Input.GetAxis("Rotation");
		if (direction == destDirection && rotationInput != 0f) {
			if(rotationInput > 0f) {
				destDirection = direction.Next();
			} else {
				destDirection = direction.Previous();
			}
		}
		if (direction != destDirection) {
			float comp = direction.Compare(destDirection);
			float rotationStep = rotationSpeed * Time.deltaTime;
			rotationAngle += comp * rotationStep;
			if (rotationAngle < 0f) {
				rotationAngle += 360f;
			}
			else if (rotationAngle >= 360f) {
				rotationAngle -= 360f;
			}
			float angleDelta = Mathf.Abs(rotationAngle - destDirection.Angle());
			if (angleDelta <= rotationStep) {
				rotationAngle = destDirection.Angle();
				direction = destDirection;
			}
			transform.localRotation = Quaternion.Euler(0f, rotationAngle, 0f);
		}

		// Handle Moving Along The Path
		if (destPath.Count == 0 && Input.GetAxis("Drive") != 0f) {
			destCell = Next();
		}
		if (destPath.Count != 0) {
			if(cell == destCell) destCell = Next();
			Vector3 rest = destCell.Position + hover;
			transform.localPosition = Vector3.MoveTowards(transform.localPosition, rest, 0.5f);
			if (transform.position == rest) {
				cell = destCell;
				destPath.RemoveAt(0);
			}
		}

		float zoomDelta = Input.GetAxis("Mouse ScrollWheel");
		if (zoomDelta != 0f) {
			zoom = Mathf.Clamp01(zoom + zoomDelta);
			float distance = Mathf.Lerp(stickMinZoom, stickMaxZoom, zoom);
			stick.localPosition = new Vector3(0f, 0f, distance);
		}

	}

	void Awake () {
		swivel = transform.GetChild(0);
		stick = swivel.GetChild(0);
		cam = stick.GetChild(0);
	}

	void Start () {
		cell = grid.GetCell(new HexCoordinates(9, 7));
		destCell = cell;
		transform.localPosition = cell.Position + hover;
		direction = HexDirection.NE;
		rotationAngle = direction.Angle();
		transform.localRotation = Quaternion.Euler(0f, rotationAngle, 0f);
		// grid.ShowUI(true);
		// grid.ShowGrid(true);
	}
}
