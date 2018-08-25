#pragma strict
import Vectrosity;
import System.Collections.Generic;

var lineTexture : Texture;
var lineMaterial : Material;
var lineWidth = 14;
private var color1 = Color.green;
private var color2 = Color.blue;
private var line : VectorLine;
private var lineColors : List.<Color32>;
private var numberOfPoints = 350;

function Start () {
	line = new VectorLine("Line", new List.<Vector3>(numberOfPoints), null, lineWidth, LineType.Continuous,Joins.Fill);
	line.material = lineMaterial;
	line.drawTransform = transform;
	LineSetup (false);
}

function LineSetup (resize : boolean) {
	if (resize) {
		lineColors = null;
		line.Resize (numberOfPoints);
	}
	for (var i = 0; i < line.points3.Count; i++) {
		line.points3[i] = Vector3(Random.Range (-5.0, 5.0), Random.Range (-5.0, 5.0), Random.Range (-5.0, 5.0));
	}
	SetLineColors();
}

function SetLineColors () {
	if (lineColors == null) {
		lineColors = new List.<Color32>(new Color32[numberOfPoints-1]);
	}
	for (var i = 0; i < lineColors.Count; i++) {
		lineColors[i] = Color.Lerp (color1, color2, (i+0.0)/lineColors.Count);
	}
	line.SetColors (lineColors);
}

function LateUpdate () {
	line.Draw();
}

function OnGUI() {
	GUI.Label (Rect(20, 10, 250, 30), "Zoom with scrollwheel or arrow keys");
	if (GUI.Button (Rect(20, 50, 100, 30), "Change colors")) {
		// Select random R G B components, making sure they are different, so color1 and color2 will be guaranteed to be not the same color
		var component1 = Random.Range(0, 3);
		do {
			var component2 = Random.Range(0, 3);
		} while (component2 == component1);
		color1 = RandomColor (color1, component1);
		color2 = RandomColor (color2, component2);
		SetLineColors();
	}
	GUI.Label (Rect(20, 100, 150, 30), "Number of points: " + numberOfPoints);
	numberOfPoints = GUI.HorizontalSlider (Rect(20, 130, 120, 30), numberOfPoints, 50, 1000);
	if (GUI.Button (Rect(160, 120, 40, 30), "Set")) {
		LineSetup (true);
	}
}

function RandomColor (color : Color, component : int) : Color {
	// The specified R G B component will be darker than the others
	for (var i = 0; i < 3; i++) {
		if (i == component) {
			color[i] = Random.value*.25;
		}
		else {
			color[i] = Random.value*.5 + .5;
		}
	}
	return color;
}