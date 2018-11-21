using UnityEngine;
using System.Collections;

public class Magnifier : MonoBehaviour {

	public float thresholdTime;
	public float thresholdTransparency;
	private float Timer;
	private float Transparency;

	void Start () {
		Timer = 0;
		Transparency = 1.0f;
	}

	void Update () {
		Timer += Time.deltaTime;
		if(Input.touchCount > 0 && Utils.gestureLayer) {
			reset ();
		}
		if (Timer > thresholdTime) {
			Pale ();
		}
		transform.GetComponent<Renderer>().material.SetColor("_Color", new Color(1, 1, 1, Transparency));	//更改透明度
	}

	//变淡
	private void Pale() {
		Transparency -= 0.01f;			//一直变淡
		if(Transparency < thresholdTransparency) {
			gameObject.SetActive (false);
		}
	}

	//恢复
	public void reset() {
		Timer = 0;
		Transparency = 1.0f;			//恢复
	}

}
