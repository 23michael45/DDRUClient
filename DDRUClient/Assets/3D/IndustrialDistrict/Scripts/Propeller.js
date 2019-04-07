var RotationSpeed:int = 100;

function Awake(){	

	if (RotationSpeed > 0){
		RotationSpeed = RotationSpeed + Random.Range(1,10);
	}
		
}

function Update(){

	transform.Rotate(Vector3(0,0,1) * Time.deltaTime * RotationSpeed);
}