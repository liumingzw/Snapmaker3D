#pragma strict
import Vectrosity;
import System.Collections.Generic;

var numberOfLines = 20;
var distanceBetweenLines = 2.0;
var moveSpeed = 8.0;
var rotateSpeed = 70.0;
var lineWidth = 2.0;

function Start () {
	numberOfLines = Mathf.Clamp (numberOfLines, 2, 8190);
	var points = new List.<Vector3>();
	// Lines down X axis
	for (var i = 0; i < numberOfLines; i++) {
		points.Add (Vector3(i * distanceBetweenLines, 0, 0));
		points.Add (Vector3(i * distanceBetweenLines, 0, (numberOfLines-1) * distanceBetweenLines));
	}
	// Lines down Z axis
	for (i = 0; i < numberOfLines; i++) {
		points.Add (Vector3(0, 0, i * distanceBetweenLines));
		points.Add (Vector3((numberOfLines-1) * distanceBetweenLines, 0, i * distanceBetweenLines));
	}
	var line = new VectorLine("Grid", points, lineWidth);
	line.Draw3DAuto();
	
	// Move camera X position to middle of grid
	transform.position.x = ((numberOfLines - 1) * distanceBetweenLines) / 2;
}

function Update () {
	if (Input.GetKey (KeyCode.LeftShift) || Input.GetKey (KeyCode.RightShift)) {
		transform.Rotate (Vector3.up * Input.GetAxis ("Horizontal") * Time.deltaTime * rotateSpeed);
		transform.Translate (Vector3.up * Input.GetAxis ("Vertical") * Time.deltaTime * moveSpeed);
	}
	else {
		transform.Translate (Vector3(Input.GetAxis ("Horizontal") * Time.deltaTime * moveSpeed, 0, Input.GetAxis ("Vertical") * Time.deltaTime * moveSpeed));
	}
}

function OnGUI () {
	GUILayout.Label (" Use arrow keys to move camera. Hold Shift + arrow up/down to move vertically. Hold Shift + arrow left/right to rotate.");
}