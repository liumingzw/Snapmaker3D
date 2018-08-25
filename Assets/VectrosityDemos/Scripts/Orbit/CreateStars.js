#pragma strict
import Vectrosity;
import System.Collections.Generic;

var numberOfStars = 2000;
private var stars : VectorLine;

function Start () {
	// Make a bunch of points in a spherical distribution
	var starPoints = new Vector3[numberOfStars];
	for (var i = 0; i < numberOfStars; i++) {
		starPoints[i] = Random.onUnitSphere * 100.0;
	}
	// Make each star have a size ranging from 1.5 to 2.5
	var starSizes = new float[numberOfStars];
	for (i = 0; i < numberOfStars; i++) {
		starSizes[i] = Random.Range(1.5, 2.5);
	}
	// Make each star have a random shade of grey
	var starColors = new Color32[numberOfStars];
	for (i = 0; i < numberOfStars; i++) {
		var greyValue = Random.value * .75 + .25;
		starColors[i] = Color(greyValue, greyValue, greyValue);
	}
	
	// We want the stars to be drawn behind 3D objects, like a skybox. So we use SetCanvasCamera,
	// which makes the canvas draw with RenderMode.OverlayCamera using Camera.main 
	VectorLine.SetCanvasCamera (Camera.main);
	VectorLine.canvas.planeDistance = Camera.main.farClipPlane-1;
	
	stars = new VectorLine("Stars", new List.<Vector3>(starPoints), 1.0, LineType.Points);
	stars.SetColors (new List.<Color32>(starColors));
	stars.SetWidths (new List.<float>(starSizes));
}

function LateUpdate () {
	stars.Draw();
}