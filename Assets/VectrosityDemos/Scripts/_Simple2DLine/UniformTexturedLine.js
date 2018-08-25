#pragma strict
import Vectrosity;
import System.Collections.Generic;

var lineTexture : Texture;
var lineWidth = 8.0;
var textureScale = 1.0;

function Start () {
	// Make a Vector2 list with 2 elements...
	var linePoints = new List.<Vector2>();
	linePoints.Add (Vector2(0, Random.Range(0, Screen.height/2)));				// ...one on the left side of the screen somewhere
	linePoints.Add (Vector2(Screen.width-1, Random.Range(0, Screen.height)));	// ...and one on the right
	
	// Make a VectorLine object using the above points, with the texture as specified in the inspector, and set the texture scale
	var line = new VectorLine("Line", linePoints, lineTexture, lineWidth);
	line.textureScale = textureScale;
	
	// Draw the line
	line.Draw();
}