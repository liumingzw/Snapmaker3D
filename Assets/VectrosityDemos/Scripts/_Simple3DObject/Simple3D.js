// See Simple3D 2.js for another way of doing this that uses TextAsset.bytes instead.
// If the vector object doesn't appear, make sure the scene view isn't visible while in play mode
#pragma strict
import Vectrosity;
import System.Collections.Generic;

function Start () {
	// Make a Vector3 array that contains points for a cube that's 1 unit in size
	var cubePoints = new List.<Vector3>([Vector3(-0.5, -0.5, 0.5), Vector3(0.5, -0.5, 0.5), Vector3(-0.5, 0.5, 0.5), Vector3(-0.5, -0.5, 0.5), Vector3(0.5, -0.5, 0.5), Vector3(0.5, 0.5, 0.5), Vector3(0.5, 0.5, 0.5), Vector3(-0.5, 0.5, 0.5), Vector3(-0.5, 0.5, -0.5), Vector3(-0.5, 0.5, 0.5), Vector3(0.5, 0.5, 0.5), Vector3(0.5, 0.5, -0.5), Vector3(0.5, 0.5, -0.5), Vector3(-0.5, 0.5, -0.5), Vector3(-0.5, -0.5, -0.5), Vector3(-0.5, 0.5, -0.5), Vector3(0.5, 0.5, -0.5), Vector3(0.5, -0.5, -0.5), Vector3(0.5, -0.5, -0.5), Vector3(-0.5, -0.5, -0.5), Vector3(-0.5, -0.5, 0.5), Vector3(-0.5, -0.5, -0.5), Vector3(0.5, -0.5, -0.5), Vector3(0.5, -0.5, 0.5)]);
	
	// Make a line using the above points, with a width of 2 pixels
	var line = new VectorLine(gameObject.name, cubePoints, 2.0);
	
	// Make this transform have the vector line object that's defined above
	// This object is a rigidbody, so the vector object will do exactly what this object does
	VectorManager.ObjectSetup (gameObject, line, Visibility.Dynamic, Brightness.None);
}