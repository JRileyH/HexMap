using UnityEngine;
using System.Collections.Generic;

public class Player : MonoBehaviour {

	bool mouseDown = false;
	HexCell cell, destCell;
	List<HexCell> destPath = new List<HexCell>();
	HexDirection direction, destDirection;

	float hoverSpeed = 0.4f;
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
				AddToPath(grid.GetCell(hit.point));
			}
			mouseDown = true;
		} else if(mouseDown && !Input.GetMouseButton(0)) {
			mouseDown = false;
		}

		// Handle Moving Along The Path
		if (destPath.Count == 0 && Input.GetAxis("Drive") != 0f) {
			destCell = Next();
		}
		if (destPath.Count != 0) {
			if(cell == destCell) destCell = Next();
			transform.localPosition = Vector3.MoveTowards(transform.localPosition, destCell.Position, 0.5f);
			if (transform.position == destCell.Position) {
				cell = destCell;
				destPath.RemoveAt(0);
			}
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

		float zoomDelta = Input.GetAxis("Mouse ScrollWheel");
		if (zoomDelta != 0f) {
			zoom = Mathf.Clamp01(zoom + zoomDelta);
			float distance = Mathf.Lerp(stickMinZoom, stickMaxZoom, zoom);
			stick.localPosition = new Vector3(0f, 0f, distance);
		}
		
		/*
		float rotationDelta = Input.GetAxis("Rotation");
		if (rotationDelta != 0f) {
			AdjustRotation(rotationDelta);
		}
		
		float drive = Input.GetAxis("Drive");
		if (drive != 0f) {
			AdjustPosition(drive);
		}

		HexCell cellUnderCam = grid.GetCell(cam.position);
		if(cellUnderCam != null) {
			stickElevation = cellUnderCam.Elevation * HexMetrics.elevationStep;
		}
		if(stick.localPosition.y != stickElevation) {
			Vector3 position = stick.localPosition;
			if(position.y > stickElevation){
				position.y--;
			} else {
				position.y++;
			}
			stick.localPosition = position;
		}*/

	}

	void AdjustRotation (float delta) {
		rotationAngle += delta * rotationSpeed * Time.deltaTime;
		if (rotationAngle < 0f) {
			rotationAngle += 360f;
		}
		else if (rotationAngle >= 360f) {
			rotationAngle -= 360f;
		}
		transform.localRotation = Quaternion.Euler(0f, rotationAngle, 0f);
	}

	void AdjustPosition (float drive) {
		Vector3 direction = transform.localRotation * new Vector3(0f, 0f, drive).normalized;
		Vector3 position = transform.localPosition;
		position += direction;
		transform.localPosition = ClampPosition(position);
	}

	Vector3 ClampPosition (Vector3 position) {
		float xMax = (grid.chunkCountX * HexMetrics.chunkSizeX - 0.5f) * (2f * HexMetrics.innerRadius);
		float zMax = (grid.chunkCountZ * HexMetrics.chunkSizeZ - 1f) * (1.5f * HexMetrics.outerRadius);

		position.x = Mathf.Clamp(position.x, 0f, xMax);
		position.z = Mathf.Clamp(position.z, 0f, zMax);

		return position;
	}

	void Awake () {
		swivel = transform.GetChild(0);
		stick = swivel.GetChild(0);
		cam = stick.GetChild(0);
	}

	void Start () {
		cell = grid.GetCell(new HexCoordinates(9, 7));
		destCell = cell;
		transform.localPosition = cell.Position;
		direction = HexDirection.NE;
		rotationAngle = direction.Angle();
		transform.localRotation = Quaternion.Euler(0f, rotationAngle, 0f);
		grid.ShowUI(true);
		grid.ShowGrid(true);
	}
}
