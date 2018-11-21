using UnityEngine;  
using System.Collections;  
using System.Collections.Generic;  
   
public class MicphoneTest : MonoBehaviour {
	
    AudioSource _audio;
    AudioSource audio {
		get {
			if (_audio == null) {
				_audio = gameObject.AddComponent<AudioSource> ();
			}
			return _audio;
		}
	}
	int deviceCount;
	public int sFrequency = 10000;
	public int lengthSec = 6;
    
    void Start() {
        string[] ms = Microphone.devices;
        deviceCount = ms.Length;
        if (deviceCount == 0) {
			Debug.Log ("no microphone found");
        }
    }

	public void StartRecord() {
        if (deviceCount < 0 || Microphone.IsRecording (null)) {
            return;
		}
		audio.Stop ();
		audio.loop = false;
		audio.mute = true;
		audio.clip = Microphone.Start (null, false, lengthSec, sFrequency);
		while (!(Microphone.GetPosition (null) > 0)) {
			
		}
		audio.Play ();
    }

	public void StopRecord() {
		if (!Microphone.IsRecording (null)) {
			return;
		}
		Microphone.End (null);
		audio.Stop ();
	}

	public void PlayRecord() {
		if (deviceCount < 0 || Microphone.IsRecording (null)) {
            return;
        }
		if (audio.clip == null) {
            return;
		}
		audio.mute = false;
		audio.loop = false;
        audio.Play ();
    }

    //停止播放声音
    public void QuitAudio() {
        audio.Stop();
    }
}