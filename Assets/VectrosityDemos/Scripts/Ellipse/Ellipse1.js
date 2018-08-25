// This script draws an ellipse using a continuous line
#pragma strict
import Vectrosity;
import System.Collections.Generic;

var lineTexture : Texture;
var xRadius = 120.0;
var yRadius = 120.0;
var segments = 60;
var pointRotation = 0.0;

function Start () {
	// Make Vector2 list where the size is the number of segments plus one (since the first and last points must be the same)
	var linePoints = new List.<Vector2>(segments+1);
	// Make a VectorLine object using the above points, with a width of 3 pixels
	var line = new VectorLine("Line", linePoints, lineTexture, 3.0, LineType.Continuous);
	// Create an ellipse in the VectorLine object, where the origin is the center of the screen
	// If xRadius and yRadius are the same, you can use MakeCircleInLine instead, which needs just one radius value instead of two
	line.MakeEllipse (Vector2(Screen.width/2, Screen.height/2), xRadius, yRadius, segments, pointRotation);
	// Draw the line
	line.Draw();
}