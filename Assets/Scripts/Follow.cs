using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Follow : MonoBehaviour {

	public Slider progress;							//进度条

	void Update () {
		follow ();
	}

	//跟随进度条
	public void follow() {
		float x = -7.5f + progress.value / 6.67f;
		transform.position = new Vector3 (x, transform.position.y, transform.position.z);
	}

}