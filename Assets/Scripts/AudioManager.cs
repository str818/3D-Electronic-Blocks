using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour {

	public AudioSource musicSource;								//背景音乐
    public AudioSource audioSource;                             //声音源
    //0-安装元器件音效 1-点击按钮音效 2-元器件烧毁音效 3-弹出按钮音效 4-生日快乐音效 5-滴声 6-滴滴声 
    public AudioClip[] audioClip;                               //音效数组

	void Update() {
		if (!musicSource.isPlaying && Utils.isMusic) {
			musicSource.Play ();
		} else if (musicSource.isPlaying && !Utils.isMusic) {
			musicSource.Stop ();
		}
	}

    //播放音效
    public void playAudio(int n) {
        audioSource.clip = audioClip[n];                        //切换音效
		if(Utils.isAudio) {
			audioSource.Play();                                 //播放
		}
    }
	
}