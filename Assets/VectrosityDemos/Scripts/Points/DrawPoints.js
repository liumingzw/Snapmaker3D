#pragma strict
import Vectrosity;
import System.Collections.Generic;

var dotSize = 2.0;
var numberOfDots = 100;
var numberOfRings = 8;
var dotColor = Color.cyan;

function Start () {
	var totalDots = numberOfDots * numberOfRings;
	var dotPoints = new Vector2[totalDots];
	var dotColors = new Color32[totalDots];
	
	var reduceAmount = 1.0 - .75/totalDots;
	for (c in dotColors) {
		c = dotColor;
		dotColor *= reduceAmount;
	}
	
	var dots = new VectorLine("Dots", new List.<Vector2>(dotPoints), null, dotSize, LineType.Points);
	dots.SetColors (new List.<Color32>(dotColors));
	for (var i = 0; i < numberOfRings; i++) {
		dots.MakeCircle (Vector2(Screen.width/2, Screen.height/2), Screen.height/(i+2), numberOfDots, numberOfDots*i);	
	}
	dots.Draw();
}