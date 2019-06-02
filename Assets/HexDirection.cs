using UnityEngine;
public enum HexDirection {
	NE, E, SE, SW, W, NW
}

public static class HexDirectionExtensions {

	public static float Compare (this HexDirection direction, HexDirection dest) {
		if(dest == direction.Previous() || dest == direction.Previous().Previous()) {
			return -1f;
		}
		return 1f;
	}

	public static float Angle (this HexDirection direction) {
		float angle = (float)direction * 60 + 30f;
		if(angle > 360f) angle -= 360f;
		return angle;
	}

	public static HexDirection Opposite (this HexDirection direction) {
		return (int)direction < 3 ? (direction + 3) : (direction - 3);
	}
	public static HexDirection Previous (this HexDirection direction) {
		return direction == HexDirection.NE ? HexDirection.NW : (direction - 1);
	}

	public static HexDirection Next (this HexDirection direction) {
		return direction == HexDirection.NW ? HexDirection.NE : (direction + 1);
	}

}