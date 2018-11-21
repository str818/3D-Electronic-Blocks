using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Progress : MonoBehaviour {
	
    public Image loadimage;							//loading界面背景图片
	public Slider progress;							//进度条
	public GameObject[] models;						//模型数组
	public GameObject[] Tips;						//提示数组

	void Start () {
		progress.enabled = false;					//禁用Slider
		progress.gameObject.SetActive (true);		//进度条可见
		RandomModel();								//随机选择一个跟随模型
		StartCoroutine(LoadingScene(Utils.loadAimScene));			//开始协同程序
	}

	private IEnumerator LoadingScene(int Scene) {	//开启线程方法
		int displayProgress = 0;
		int toProgress = 0;
		AsyncOperation op = Application.LoadLevelAsync (Scene);
        op.allowSceneActivation = false;			//禁止Unity加载完毕后自动切换场景
		while (op.progress < 0.9f) {
			toProgress = (int)(op.progress * 100);	//标志位等于加载进度值
			while (displayProgress < toProgress) {
				++displayProgress;
				setProgressValue (displayProgress);	//设置进度条值				
				yield return new WaitForEndOfFrame ();		//等待帧结束后
			}
		}
		toProgress = 100;
		while (displayProgress<toProgress) {
			++displayProgress;						//进度条值加一
			setProgressValue(displayProgress);
            yield return new WaitForEndOfFrame();	//等待帧结束后
		}
		op.allowSceneActivation = true;
	}

	private void setProgressValue(int value) {
		progress.value = value;						//设置进度条值，即Slider控件中“progress”中Value
	}

	private void RandomModel() {						//随机图片
		int random = Random.Range(0, 4);
		models [random].SetActive (true);
		Tips [random].SetActive (true);
    }

}
