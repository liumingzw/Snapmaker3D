#pragma strict
import Vectrosity;
import System.Collections.Generic;

var lineTex : Texture;
var lineTex2 : Texture;
var lineTex3 : Texture;
var frontTex : Texture;
var backTex : Texture;
var capTex : Texture;

function Start () {
	VectorLine.SetEndCap ("arrow", EndCap.Front, lineTex, frontTex);
	VectorLine.SetEndCap ("arrow2", EndCap.Both, lineTex2, frontTex, backTex);
	VectorLine.SetEndCap ("rounded", EndCap.Mirror, lineTex3, capTex);

	var line1 = new VectorLine("Arrow", new List.<Vector2>(50), 30.0, LineType.Continuous, Joins.Weld);
	line1.useViewportCoords = true;
	var splinePoints = [Vector2(.1, .15), Vector2(.3, .5), Vector2(.5, .6), Vector2(.7, .5), Vector2(.9, .15)];
	line1.MakeSpline (splinePoints);
	line1.endCap = "arrow";
	line1.Draw();

	var line2 = new VectorLine("Arrow2", new List.<Vector2>(50), 40.0, LineType.Continuous, Joins.Weld);
	line2.useViewportCoords = true;
	splinePoints = [Vector2(.1, .85), Vector2(.3, .5), Vector2(.5, .4), Vector2(.7, .5), Vector2(.9, .85)];
	line2.MakeSpline (splinePoints);
	line2.endCap = "arrow2";
	line2.continuousTexture = true;
	line2.Draw();
	
	var line3 = new VectorLine("Rounded", new List.<Vector2>([Vector2(.1, .5), Vector2(.9, .5)]), 20.0);
	line3.useViewportCoords = true;
	line3.endCap = "rounded";
	line3.Draw();
}