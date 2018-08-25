#pragma strict
import Vectrosity;
import System.Collections.Generic;

var orbitSpeed = -45.0;
var rotateSpeed = 200.0;
var orbitLineResolution = 150;
var lineMaterial : Material;	// This should be a material with a shader that will draw on top of the stars

function Start () {
	var orbitLine = new VectorLine("OrbitLine", new List.<Vector3>(orbitLineResolution), 2.0, LineType.Continuous);
	orbitLine.material = lineMaterial;
	orbitLine.MakeCircle (Vector3.zero, Vector3.up, Vector3.Distance(transform.position, Vector3.zero));
	orbitLine.Draw3DAuto();
}

function Update () {
	transform.RotateAround (Vector3.zero, Vector3.up, orbitSpeed * Time.deltaTime);
	transform.Rotate (Vector3.up * rotateSpeed * Time.deltaTime);
}