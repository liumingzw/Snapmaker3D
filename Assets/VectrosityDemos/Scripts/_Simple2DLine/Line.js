// Use this method if you need more control than you get with Vector.SetLine
#pragma strict
import Vectrosity;
import System.Collections.Generic;

function Start () {
	// Make a Vector2 list; in this case we just use 2 elements...
	var linePoints = new List.<Vector2>();
	linePoints.Add (Vector2(0, Random.Range(0, Screen.height)));				// ...one on the left side of the screen somewhere
	linePoints.Add (Vector2(Screen.width-1, Random.Range(0, Screen.height)));	// ...and one on the right
	
	// Make a VectorLine object using the above points, with a width of 2 pixels
	var line = new VectorLine("Line", linePoints, 2.0);
	
	// Draw the line
	line.Draw();
}