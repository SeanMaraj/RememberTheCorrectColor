#pragma strict


var rotSpeed: float = 60;
function Start () {

}

function Update () {
	transform.Rotate(0, rotSpeed * Time.deltaTime, 0, Space.World);
}