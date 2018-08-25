#pragma strict
import Vectrosity;
import System.Collections.Generic;

var numberOfRects = 30;
var lineColor = Color.green;
var mask : GameObject;
var moveSpeed = 2.0;
private var rectLine : VectorLine;
private var t = 0.0;
private var startPos : Vector3;

function Start () {
	rectLine = new VectorLine("Rects", new List.<Vector3>(numberOfRects*8), 2.0);
	var idx = 0;
	for (var i = 0; i < numberOfRects; i++) {
		rectLine.MakeRect (Rect(Random.Range(-5.0, 5.0), Random.Range(-5.0, 5.0), Random.Range(0.25, 3.0), Random.Range(0.25, 2.0)), idx);
		idx += 8;
	}
	rectLine.color = lineColor;
	rectLine.capLength = 1.0;
	rectLine.drawTransform = transform;
	rectLine.SetMask (mask);
	
	startPos = transform.position;
}

function Update () {
	// Move this transform around in a circle, and the line uses the same movement since it's using this transform with .drawTransform
	t = Mathf.Repeat (t + Time.deltaTime * moveSpeed, 360.0);
	transform.position = Vector2(startPos.x + Mathf.Sin (t) * 1.5, startPos.y + Mathf.Cos (t) * 1.5);
	rectLine.Draw();
}