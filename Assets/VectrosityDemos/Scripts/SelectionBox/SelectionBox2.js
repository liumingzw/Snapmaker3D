#pragma strict
import Vectrosity;
import System.Collections.Generic;

var lineTexture : Texture;
var textureScale = 4.0;
private var selectionLine : VectorLine;
private var originalPos : Vector2;

function Start () {
	selectionLine = new VectorLine("Selection", new List.<Vector2>(5), lineTexture, 4.0, LineType.Continuous);
	selectionLine.textureScale = textureScale;
	// Prevent line from getting blurred by anti-aliasing (the line width is 4 but the texture has transparency that makes it effectively 1)
	selectionLine.alignOddWidthToPixels = true;
}

function OnGUI () {
	GUI.Label(Rect(10, 10, 300, 25), "Click & drag to make a selection box");
}

function Update () {
	if (Input.GetMouseButtonDown(0)) {
		originalPos = Input.mousePosition;
	}
	if (Input.GetMouseButton(0)) {
		selectionLine.MakeRect (originalPos, Input.mousePosition);
		selectionLine.Draw();
	}
	selectionLine.textureOffset = -Time.time*2.0 % 1;
}