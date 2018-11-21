using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LightUpdate : MonoBehaviour {

	public float value;
	private float oldValue;
	private int count;
	private GameObject[] Elms;
	private GameObject[] PhotosensitiveResistorElm;

	public void Init () {
		count = 0;
		Elms = GameObject.FindGameObjectsWithTag ("unit");
		PhotosensitiveResistorElm = new GameObject[Utils.ElmCounts];
		for (int i = 0; i < Elms.Length; i++) {
			if (Elms [i].transform.name.Length > 23) {
				if(Elms[i].name.Substring(0, 25).Equals("PhotosensitiveResistorElm")) {
					PhotosensitiveResistorElm[count] = Elms [i];
					PhotosensitiveResistorElm[count++].transform.FindChild ("Spotlight").gameObject.SetActive (true);
				}
			}
		}
				
		transform.GetComponent<Scrollbar> ().value = 0.0f;
		value = transform.GetComponent<Scrollbar> ().value;
		Debug.Log (PhotosensitiveResistorElm.Length);
	}

	void Update () {
		value = transform.GetComponent<Scrollbar> ().value;
		if(value == oldValue) {
			return;
		}
		for (int i = 0; i < PhotosensitiveResistorElm.Length; i++) {
			Debug.Log (PhotosensitiveResistorElm.Length);
			PhotosensitiveResistorElm[i].GetComponent<PhotosensitiveResistorElm> ().resistance = 100.0f * (1 - value);
			PhotosensitiveResistorElm[i].transform.FindChild ("Spotlight").GetComponent<Light> ().intensity = 8.0f * value;
			if(PhotosensitiveResistorElm[i].GetComponent<PhotosensitiveResistorElm> ().resistance < 5.0f) {
				PhotosensitiveResistorElm[i].GetComponent<PhotosensitiveResistorElm> ().resistance = 5.0f;
			}
		}
		CirSim.analyzeCircuit();                //分析电路
		oldValue = transform.GetComponent<Scrollbar> ().value;
	}

}
