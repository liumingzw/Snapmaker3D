#pragma strict
import Vectrosity;
import System.Collections.Generic;

var numberOfPoints = 100;
var lineColor = Color.yellow;
var mask : GameObject;
var lineWidth = 9.0;
var lineHeight = 17.0;
private var spikeLine : VectorLine;
private var t = 0.0;
private var startPos : Vector3;

function Start () {
	spikeLine = new VectorLine("SpikeLine", new List.<Vector3>(numberOfPoints), 2.0, LineType.Continuous);
	var y = lineHeight / 2;
	for (var i = 0; i < numberOfPoints; i++) {
		spikeLine.points3[i] = Vector2(Random.Range(-lineWidth/2, lineWidth/2), y);
		y -= lineHeight / numberOfPoints;
	}
	spikeLine.color = lineColor;
	spikeLine.drawTransform = transform;
	spikeLine.SetMask (mask);
	
	startPos = transform.position;
}

function Update () {
	// Move this transform around in a circle, and the line uses the same movement since it's using this transform with .drawTransform
	t = Mathf.Repeat (t + Time.deltaTime, 360.0);
	transform.position = Vector2(startPos.x, startPos.y + Mathf.Cos (t) * 4);
	spikeLine.Draw();
}