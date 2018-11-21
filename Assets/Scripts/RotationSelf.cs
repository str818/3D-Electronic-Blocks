using UnityEngine;
using System.Collections;

public class RotationSelf : MonoBehaviour {

	public float speed;

	void Start () {
	
	}

	void Update () {
		transform.Rotate (0, 0, -speed * Time.deltaTime);
	}

}
