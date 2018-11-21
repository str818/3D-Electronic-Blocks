using UnityEngine;
using UnityEngine.UI;
using System.Text;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System.Collections;

public class MenuListener : MonoBehaviour {

	//主界面变量
	public Animation titleAnimation;				//标题动画
	public Animation buttonAnimation;				//按钮动画
	public GameObject Fade;							//渐隐效果Cube
	public GameObject Mat;							//垫子位置
	public GameObject MyMainCamera; 				//主摄像机
	public GameObject rabbit;						//兔子模型
	public GameObject AudioPanel;					//声音设置Panel
	public Sprite[] sprites;						//精灵数组
	public RectTransform[] Button;          		//0-实验模式按钮 1-自由模式按钮 2-实验模式按钮阴影 3-自由模式按钮阴影
	private bool isEject;							//是否弹出完成
	private bool[] isClick;                 		//点击按钮 0-实验室模式按钮 1-自由模式按钮

	//选实验界面
	//public Animation Breath2;						//呼吸动画
	public GameObject PipeClamp;					//按钮父类
	public GameObject UIPrefab;                     //下载资源包预制件
	public GameObject SelectPerviousButton;			//前一个按钮
	public GameObject SelectNextButton;				//后一个按钮
	private bool isSelectMove;						//是否正在移动
	private int count;								//按钮数量
	private int Pages;								//总共页数
	private int PageIndex;							//当前页数（0开始）
	private GameObject[] buttons;					//按钮数组
	private string ImagePath = Application.persistentDataPath + "/EX";  //本地图片包文件夹

	//所有元器件介绍界面
	public GameObject[] AllIntroductions;			//介绍元器件
	public GameObject AllUnitGroup;					//介绍的元件组
	public GameObject AllContent;					//文本
	public GameObject AllPreviousButton;       		//前一个按钮
	public GameObject AllNextButton;           		//下一个按钮

    //自由模式变量
	public GameObject onConnectedButton;    		//确认连接按钮
	public GameObject offConnectedButton;   		//断开连接按钮

	//实验界面
	private EventTrigger eventTrigger;				//事件触发系统
	public int totleUnits;                          //需要介绍的元器件总量
    public GameObject Tip;                          //提示界面
    public GameObject TipText;                      //提示文字
    public GameObject MyCamera;						//主摄像机
	public GameObject CameraController;				//主摄像机
	public GameObject[] introductions;				//介绍元器件
	public GameObject UnitGroup;					//介绍的元件组
	public GameObject unitGroup;					//实验器件管理物体
	public GameObject unitGroup2;					//实验器件管理物体(用于自由模式)
	public GameObject Plate;						//底板
	public GameObject Room2;						//房间
	public GameObject PreviousButton;       		//前一个按钮
	public GameObject NextButton;           		//下一个按钮
	public GameObject BackButton;           		//返回按钮
	public GameObject Scroll;						//滑条
	public GameObject IntroductionUI;           	//介绍元器件画布
    public GameObject EXIntroductionUI;             //介绍实验画布
    public GameObject content;						//介绍元器件界面文本
    public GameObject contentEX;                    //介绍实验界面文本
	public GameObject Rabbit;						//兔子模型
    public GameObject Movie;                        //电视板
    public GameObject movieController;              //视频播放控制器
	public Animation animation;						//按钮动画
	private GameObject PhotosensitiveResistorElm;	//光敏电阻
	private Quaternion CameraRotation;				//进入介绍界面前摄像机旋转角
	private Vector3 CameraControllerPosition;		//进入介绍界面前摄像机旋转角
	private Quaternion CameraControllerRotation;	//进入介绍界面前摄像机旋转角
	private string loadExperimentPrefabPath;		//实验器材下载路径
	private string loadIntroductionPrefabPath;		//介绍器材下载路径
	private bool isMoving = false;					//介绍器件是否正在移动
	private int introductionIndex;					//介绍界面第几个元器件索引
	private bool switchunit;						//切换实验器件的标志位  true-unitGroup在第一层
	private bool isSwitchMoving;					//切换器件是否在移动

    void Start() {
		if (Application.loadedLevel == Utils.MainInterface) {					//如果当前场景为主界面场景
            isClick = new bool[2];              								//创建点击按钮数组
			isEject = false;													//弹出未完成
            for (int i = 0; i < Button.Length; i++) {							//遍历按钮数组
                Button[i].localScale = new Vector3(0, 0, 0);        			//按钮初始不可见
            }
			setRabbit ();														//设置兔子模型位置
		} else if(Application.loadedLevel == Utils.ExperimentalInterface) {		//如果当前场景实验场景
			loadExperimentPrefabPath = "ExperimentPrefabs/";					//初始化下载路径
			loadIntroductionPrefabPath = "IntroductionPrefabs/";				//初始化下载路径
			introductionIndex = 0;												//第0号元件开始介绍
			Utils.isIntroduction = false;										//未处于实验界面
			Utils.ElmCounts = 0;
			if(Utils.ExperimentalType == 1) {
				transform.FindChild ("EXInterduction").gameObject.SetActive (false);
				transform.FindChild ("Switch").gameObject.SetActive (true);
			}
			LoadExperimentUnit();												//加载介绍元器件
            bitTwinkle();														//播放闪烁动画
			LoadIntroductionUnit();												//加载介绍元器件
			UnitGroup.SetActive(false);											//介绍元器件消失
			unitGroup2.SetActive(false);										//第二层实验器件消失
			switchunit = true;													//初始unitGroup在第一层
		} else if(Application.loadedLevel == Utils.SelectedInterface) {			//如果是选实验界面
			updateButton();
            suitLayout ();
		} else if(Application.loadedLevel == Utils.AllIntroduceInterface) {
			loadIntroductionPrefabPath = "IntroductionPrefabs/";				//初始化下载路径
			ReadAllIntroductionUnit ();
			LoadAllIntroductionUnit ();
			if(introductionIndex != 0) {
				IntroductionUI.transform.FindChild ("Previous").gameObject.SetActive (true);
			}
			AllUnitGroup.transform.GetChild (0).GetComponent<RotationSelf> ().enabled = true;
			AllContent.GetComponent<Read> ().read (Utils.experiments [introductionIndex]);
		}
    }

    void Update() {
		if (Application.loadedLevel == Utils.MainInterface 
			&& !titleAnimation.IsPlaying("Title")) {      		//若当前处于开始菜单的场景
			ejectButton();                      				//弹出按钮
			breathButton();										//呼吸效果
            hideButton();                       				//隐藏按钮
        }
		EscapeListen ();										//监听返回键
    }

	//*****************************************主界面*****************************************
	//设置兔子模型的位置
	public void setRabbit() {
		/*Vector3 matPosition = Mat.transform.position;
		Vector2 v1 = new Vector2 (Screen.width / 2.0f, Screen.height / 2.0f);
		Vector2 v2 = Camera.main.WorldToScreenPoint (matPosition);
		Vector2 center = new Vector2((v1.x + v2.x) / 2.0f, (v1.y + v2.y) / 2.0f);
		Vector3 rabbitPostion = Camera.main.ScreenToWorldPoint (new Vector3(center.x, center.y, 90.0f));
		rabbit.transform.position = new Vector3(rabbitPostion.x * 2.0f, rabbitPostion.y * 2.0f, rabbitPostion.z);*/
		Vector3 cameraPosition = MyMainCamera.transform.position;									//获取摄像机位置
		Vector3 matPosition = Mat.transform.position;												//获取垫子位置
		Vector3 rabbitPostion = new Vector3 ((cameraPosition.x + matPosition.x) / 2.0f, 
			(cameraPosition.y + matPosition.y) / 2.0f, (cameraPosition.z + matPosition.z) / 2.0f);	//计算兔子位置
		rabbit.transform.position = rabbitPostion;													//设置兔子位置
	}

	//弹出按钮
	private void ejectButton() {
		if(!isEject) {
			for (int i = 0; i < Button.Length; i++) {										//遍历按钮
				Button [i].localScale = new Vector3 (Button [i].localScale.x + 0.05f,
					Button [i].localScale.y + 0.05f, Button [i].localScale.y + 0.05f);		//调整按钮大小
			}
			if (Button[0].localScale == new Vector3(1, 1, 1)) {								//如果弹出完成
				isEject = true;            													//按钮能够呼吸
				Utils.audioManager.playAudio(3);											//播放音效
			}
		}
	}

	//呼吸动画
	public void breathButton() {
		if(isEject) {
			transform.GetComponent<Animation> ().enabled = true;
			buttonAnimation["Breath"].speed = 0.3f;									//减慢呼吸动画的播放速度为正常速度的0.2倍
		}
	}

    //隐藏按钮
    private void hideButton() {
        for (int i = 0; i < 2; i++) {
            if (!isClick[i]) continue;      													//若没点击则不执行下面代码
			transform.GetComponent<Animation> ().enabled = false;
			Button [i].localScale = new Vector3 (Button [i].localScale.x - 0.05f,
				Button [i].localScale.y - 0.05f, Button [i].localScale.y - 0.05f);    			//调整按钮大小
			Button [i + 2].localScale = new Vector3 (Button [i + 2].localScale.x - 0.05f,
				Button [i + 2].localScale.y - 0.05f, Button [i + 2].localScale.y - 0.05f);  	//调整阴影大小
            if (Button[i].localScale.x < 0) {													//如果缩小到0
                isClick[i] = false;         													//停止缩小
				switch(i) {
				case 0:																			//实验模式
					Application.LoadLevel(Utils.SelectedInterface);                         	//加载对应的场景（选择）
					break;
				case 1:																			//自由模式
					Utils.loadAimScene = Utils.ExperimentalInterface;
					Application.LoadLevel (Utils.TransitionInterface);							//进入过度场景
					break;
				}
            }
        }
    }

	//实验模式按钮
	public void OnExperimentalMode() {
		if (isClick[1]) return;               	//若点击了另外一个按钮，则此按钮不可点击
		Utils.audioManager.playAudio(1);      	//播放音效
		isClick[0] = true;                    	//点击按钮置true
		Utils.ExperimentalType = 0;				//实验模式
	}

	//自由模式按钮
	public void OnFreeMode() {
		if (isClick[0]) return;                 //若点击了另外一个按钮，则此按钮不可点击
		Utils.audioManager.playAudio(1);        //播放音效
		isClick[1] = true;                      //点击按钮置true
		Utils.ExperimentalType = 1;				//自由模式
		Utils.ExperimentIndex = -1;
		Utils.buttonName = EventSystem.current.currentSelectedGameObject.name;			//获取点击按钮名称
		Utils.gestureLayer = false;				//重置手势层
		Fade.SetActive(true);					//打开渐隐组件
	}

	//声音设置
	public void OpenAudioPanel() {
		Utils.audioManager.playAudio(1);        //播放音效
		AudioPanel.SetActive (true);			//打开设置画布
	}

	//关闭声音设置
	public void CloseAudioPanel() {
		Utils.audioManager.playAudio(1);        //播放音效
		AudioPanel.SetActive (false);			//打开设置画布
	}

	//打开音乐
	public void MusicSwitch() {
		Utils.isMusic = !Utils.isMusic;
		if (Utils.isMusic) {
			AudioPanel.transform.FindChild ("MusicSwitch").GetComponent<Image> ().sprite = sprites [0];
		} else {
			AudioPanel.transform.FindChild ("MusicSwitch").GetComponent<Image> ().sprite = sprites [1];
		}
	}

	//打开音效
	public void AudioSwitch() {
		Utils.isAudio = !Utils.isAudio;
		if (Utils.isAudio) {
			AudioPanel.transform.FindChild ("AudioSwitch").GetComponent<Image> ().sprite = sprites [2];
		} else {
			AudioPanel.transform.FindChild ("AudioSwitch").GetComponent<Image> ().sprite = sprites [3];
		}
	}

    //进入加载实验资源包界面
    public void ToLoadEX() {
        Utils.audioManager.playAudio(1);       	 					//播放音效
        Application.LoadLevel(Utils.LoadEXSenceInterface);          
    }

	//所有元器件介绍界面
	public void AllUnitsIntroduction() {
		Utils.audioManager.playAudio(1);       	 					//播放音效
		Utils.loadAimScene = Utils.AllIntroduceInterface;
		Application.LoadLevel(Utils.TransitionInterface);   		//进入所有元器件介绍界面
	}

	//退出
	public void Exit() {
		Utils.audioManager.playAudio (1);				//播放声音
		Application.Quit ();
	}
	//****************************************************************************************
    
	//****************************************选关界面****************************************
	//更新按钮
	public void updateButton() {
		if(Utils.localEXList.Count > Utils.BuiltInCounts) {
			ArrayList rgbList = new ArrayList();
			loadText (ImagePath + "/Images/ImagesRGB.txt", rgbList);							//初始颜色rgb列表

			for(int i = 6; i < Utils.localEXList.Count; i++) {
				GameObject temp = Instantiate(UIPrefab);                       			 		//初始化资源包预制件
				temp.transform.SetParent(transform.FindChild("PipeClamp"));						//设置父物体
				temp.transform.name = ((string[])Utils.localEXList[i])[0];						//设置名称
				temp.transform.localScale = new Vector3(1, 1, 1);								//设置缩放比
				temp.transform.FindChild("Text").transform.GetComponent<Text>().text 
					= ((string[])Utils.localEXList[i])[1];										//更改资源包名称

				for (int j = 0; j < rgbList.Count; j++) {
					if (temp.transform.name.Equals (((string[])rgbList [j]) [0])) {
						float r = float.Parse (((string[])rgbList[j]) [1]) / 255.0f;				//获取TXT r值
						float g = float.Parse (((string[])rgbList[j]) [2]) / 255.0f;				//获取TXT g值
						float b = float.Parse (((string[])rgbList[j]) [3]) / 255.0f;				//获取TXTr b值
						temp.transform.FindChild("Text").transform.GetComponent<Text>().color = 
							new Color(r, g, b, 1.0f);													//更改字体颜色
					}
				}
				
				temp.transform.FindChild("Image").transform.GetComponent<Image>().sprite = 
					Utils.LoadImage(ImagePath  + "/Images/" + temp.name + ".png");				//更改图片精灵
				temp.GetComponent<Button>().onClick.AddListener(delegate {						//点击委托
					ToExperimentOfIndex();														//进入实验界面
				});
			}
		}
	}

	//读取txt中的信息
	public void loadText(string path, ArrayList al) {
		al.Clear();                                                                 //清空列表
		//初始化内嵌实验资源包 到本地实验包列表
		using (System.IO.StreamReader sr = new System.IO.StreamReader(
			path, Encoding.Default)) {
			string str;																//定义局部变量保存每行字符串
			while ((str = sr.ReadLine()) != null) {									//如果此行不为空
				string[] ss = str.Split(new char[] { ' ' });						//根据空格分隔字符串 
				al.Add(ss);                                                   		//读取文本中的内容到列表中
			}
		}
	}

	//调整布局
	public void suitLayout() {
		count = transform.FindChild("PipeClamp").childCount;									//获取总共实验按钮个数
		Pages = count / 7 + 1;																	//获取总页数
		if(Pages == 1) {																		//如果只有一页
			SelectNextButton.SetActive (false);													//下一个按钮消失
		}
		buttons = new GameObject[count];
		for(int i = 0; i < count; i++) {
			buttons [i] = transform.FindChild("PipeClamp").GetChild (i).gameObject;
		}
		switch(count) {
		case 7:
			buttons [6].transform.localPosition = new Vector3 (2000.0f, 0.0f, 0.0f);
			break;
		case 8:
			buttons [6].transform.localPosition = new Vector3 (1705.0f, 0.0f, 0.0f);
			buttons [7].transform.localPosition = new Vector3 (2295.0f, 0.0f, 0.0f);
			break;
		case 9:
			buttons [6].transform.localPosition = new Vector3 (1400.0f, 0.0f, 0.0f);
			buttons [7].transform.localPosition = new Vector3 (2000.0f, 0.0f, 0.0f);
			buttons [8].transform.localPosition = new Vector3 (2600.0f, 0.0f, 0.0f);
			break;
		case 10:
			buttons [6].transform.localPosition = new Vector3 (1705.0f, 200.0f, 0.0f);
			buttons [7].transform.localPosition = new Vector3 (2295.0f, 200.0f, 0.0f);
			buttons [8].transform.localPosition = new Vector3 (1705.0f, -200.0f, 0.0f);
			buttons [9].transform.localPosition = new Vector3 (2295.0f, -200.0f, 0.0f);
			break;
		case 11:
			buttons [6].transform.localPosition = new Vector3 (1400.0f, 200.0f, 0.0f);
			buttons [7].transform.localPosition = new Vector3 (2000.0f, 200.0f, 0.0f);
			buttons [8].transform.localPosition = new Vector3 (2600.0f, 200.0f, 0.0f);
			buttons [9].transform.localPosition = new Vector3 (1705.0f, -200.0f, 0.0f);
			buttons [10].transform.localPosition = new Vector3 (2295.0f, -200.0f, 0.0f);
			break;
		case 12:
			buttons [6].transform.localPosition = new Vector3 (1400.0f, 200.0f, 0.0f);
			buttons [7].transform.localPosition = new Vector3 (2000.0f, 200.0f, 0.0f);
			buttons [8].transform.localPosition = new Vector3 (2600.0f, 200.0f, 0.0f);
			buttons [9].transform.localPosition = new Vector3 (1400.0f, -200.0f, 0.0f);
			buttons [10].transform.localPosition = new Vector3 (2000.0f, -200.0f, 0.0f);
			buttons [11].transform.localPosition = new Vector3 (2600.0f, -200.0f, 0.0f);
			break;
		}
	}

	//从选择实验界面进入实验界面 index
	public void ToExperimentOfIndex() {
		Utils.audioManager.playAudio (1);				//播放声音
		Utils.buttonName = EventSystem.current.currentSelectedGameObject.name;			//获取点击按钮名称
		Debug.Log(Utils.buttonName);
		Utils.ExperimentIndex = int.Parse(Utils.buttonName.Substring(3));				//实验序号
		Utils.gestureLayer = false;														//重置手势层
		Utils.loadAimScene = Utils.ExperimentalInterface;
		Application.LoadLevel (Utils.TransitionInterface);								//进入过度场景
	}

	//返回主界面的方法
	public void backToMain() {
		Utils.audioManager.playAudio(1);        										//播放音效
		Application.LoadLevel (Utils.MainInterface);									//进入主界面场景
	}

	//前一个
	public void SelectPrevious() {
		if(!isSelectMove) {												//如果没有正在移动
			Utils.audioManager.playAudio (1);							//播放声音
			PageIndex--;												//介绍元器件索引加一
			float aimX = 0.0f - PageIndex * 1800.0f;					//计算目标X值
			StartCoroutine (SelectMove(aimX));							//开启前翻协成
		}
		if(PageIndex == 0) {											//如果当前是第1页
			SelectPerviousButton.SetActive (false);						//关闭前一个按钮
		}
		if(PageIndex == Pages - 2) {									//如果当前是第2页
			SelectNextButton.SetActive (true);							//打开下一个按钮
		}
	}

	//后一个
	public void SelectNext() {
		if(!isSelectMove) {												//如果没有正在移动
			Utils.audioManager.playAudio (1);							//播放声音
			PageIndex++;												//介绍元器件索引加一
			float aimX = 0.0f - PageIndex * 1800.0f;					//计算目标X值
			StartCoroutine (SelectMove(aimX));							//开启前翻协成
		}
		if(PageIndex == Pages - 1) {									//如果当前是倒数第1页
			SelectNextButton.SetActive (false);							//关闭下一个按钮
		}
		if(PageIndex == 1) {											//如果当前是第2页
			SelectPerviousButton.SetActive (true);						//打开下一个按钮
		}
	}

	//移动（翻页）
	IEnumerator SelectMove(float temp) {
		Vector3 aimPosition = new Vector3 (temp, PipeClamp.transform.localPosition.y,
			PipeClamp.transform.localPosition.z);													//定义目标点
		while (Vector3.Distance(PipeClamp.transform.localPosition, aimPosition) > 0.001f) {			//如果距离不到预期目标
			isSelectMove = true;																	//正在移动
			PipeClamp.transform.localPosition = Vector3 .Lerp(
				PipeClamp.transform.localPosition, aimPosition, 0.25f);								//移动
			yield return 0;																			//返回
		}
		isSelectMove = false;																		//改为没有正在移动
	}

	//呼吸动画
	/*public void Breeath2() {
		Breath2["Breath2"].speed = 0.3f;					//减慢呼吸动画的播放速度为正常速度的0.2倍
	}*/
	//****************************************************************************************

	//****************************************实验界面****************************************
	public void LoadExperimentUnit() {
		string[] ss;
		if(Utils.ExperimentIndex < 0) {
			string s = Utils.LoadFile (Utils.buttonName + ".txt");								//下载相关txt数据文件
			ss = s.Split ('\n');																//以回车切割文件为string数组
		} else if (Utils.ExperimentIndex >= 0 && Utils.ExperimentIndex < Utils.BuiltInCounts) {
			string s = Utils.LoadFile (Utils.buttonName + "_Info.txt");							//下载相关txt数据文件
			ss = s.Split ('\n');																//以回车切割文件为string数组
		} else {
			ArrayList li = new ArrayList();
			li.Clear ();
			Utils.loadText (Application.persistentDataPath + "/EX/" + Utils.buttonName + "/" + Utils.buttonName + "_Info.txt", li);				//下载相关txt数据文件
			ss  = (string[])li.ToArray (typeof(string));
		}
		Utils.experimentCount = int.Parse (ss [0].Trim ());											//获取需要介绍的元器件数量
		Utils.experiments = new int[Utils.experimentCount];											//初始化实验元件代号数组
		for (int i = 1; i < ss.Length; i++) {														//遍历txt文件每一行（除第一行外）
			string[] sss = ss [i].Split (' ');														//以空格分割
			Utils.experiments [i - 1] = int.Parse (sss [0]);											//赋值
			switch (Utils.experiments [i - 1]) {														//分别做动态加载
			case 1:                                                                             //1-导线
				LoadExperimentModel (loadExperimentPrefabPath + "WireElm_1", sss, i - 1);              //加载对应模型
				break;
			case 2:                                                                             			//2-导线
				LoadExperimentModel (loadExperimentPrefabPath + "WireElm_2", sss, i - 1);              		//加载对应模型
				break;
			case 3:                                                                            			 	//3-导线
				LoadExperimentModel (loadExperimentPrefabPath + "WireElm_3", sss, i - 1);					//加载对应模型
				break;
			case 15:																						//15-开关
				LoadExperimentModel (loadExperimentPrefabPath + "SwitchElm", sss, i - 1);					//加载对应模型
				break;
			case 16:																						//16-光敏电阻
				LoadExperimentModel (loadExperimentPrefabPath + "PhotosensitiveResistorElm", sss, i - 1);	//加载对应模型
				break;
			case 17:                                                                            			//17-二极管
				LoadExperimentModel (loadExperimentPrefabPath + "DiodeElm", sss, i - 1);                	//加载对应模型
				break;
			case 18:																						//18-小灯泡
				LoadExperimentModel (loadExperimentPrefabPath + "BulbElm", sss, i - 1);						//加载对应模型
				break;
			case 19:																						//19-电池
				LoadExperimentModel (loadExperimentPrefabPath + "VoltageElm", sss, i - 1);					//加载对应模型
				break;
			case 20:                                                                            			//20-喇叭
				LoadExperimentModel (loadExperimentPrefabPath + "HornElm", sss, i - 1);                		//加载对应模型
				break;
			case 21:                                                                            			//21-音乐集成电路
				LoadExperimentModel (loadExperimentPrefabPath + "MusicChipElm", sss, i - 1);           		//加载对应模型
				break;
			case 24:																						//24-风扇
				LoadExperimentModel (loadExperimentPrefabPath + "EngineElm", sss, i - 1);					//加载对应模型
				break;
			case 28:																						//28-话筒
				LoadExperimentModel (loadExperimentPrefabPath + "MicrophoneElm", sss, i - 1);				//加载对应模型
				break;
			case 30:                                                                            			//30-电阻
				LoadExperimentModel (loadExperimentPrefabPath + "ResistanceElm", sss, i - 1);          		//加载对应模型
				break;
			case 53:																						//53-滑动变阻器
				LoadExperimentModel (loadExperimentPrefabPath + "SlidingResistanceElm", sss, i - 1);		//加载对应模型
				break;
			case 56:                                                                            			//56-电流表
				LoadExperimentModel (loadExperimentPrefabPath + "AmmeterElm", sss, i - 1);      			//加载对应模型
				break;
			case 58:                                                                            			//58-数码管
				LoadExperimentModel (loadExperimentPrefabPath + "CounterElm", sss, i - 1);      			//加载对应模型
				break;
			case 62:                                                                            			//62-录音IC
				LoadExperimentModel (loadExperimentPrefabPath + "RecorderChipElm", sss, i - 1);        			//加载对应模型
				break;
			}
		}

	}

	//加载模型的方法
	private void LoadExperimentModel(string filePath, string[] sss, int i) {
		GameObject prefab = Resources.Load (filePath) as GameObject;							//获取预制件转换为GameObject
		prefab.transform.GetComponent<CircuitElm> ().originalPos.x = float.Parse(sss [1]);		//按照txt数据文件设置初始坐标
		prefab.transform.GetComponent<CircuitElm> ().originalPos.y = float.Parse(sss [2]);		//按照txt数据文件设置初始坐标
		prefab.transform.GetComponent<CircuitElm> ().originalPos.z = float.Parse(sss [3]);		//按照txt数据文件设置初始坐标
		prefab.transform.GetComponent<CircuitElm>().originalRot.x = float.Parse(sss [4]);		//按照txt数据文件设置初始旋转角
		prefab.transform.GetComponent<CircuitElm>().originalRot.y = float.Parse(sss [5]);		//按照txt数据文件设置初始旋转角
		prefab.transform.GetComponent<CircuitElm>().originalRot.z = float.Parse(sss [6]);		//按照txt数据文件设置初始旋转角
		GameObject Model = (GameObject)Instantiate (prefab);									//获取加载模型
		Model.name = Model.name.Replace("(Clone)", "");											//去除克隆名称
		if (i <= 7) {
			Model.transform.SetParent(unitGroup.transform);										//设置父物体
		} else {
			Model.transform.SetParent(unitGroup2.transform);									//设置父物体
		}
	}

	//点击确认连接按钮
	public void OnConnected() {
		Utils.audioManager.playAudio(1);        //播放音效
		if(Utils.buttonName.Equals("EX_9") && exist(GameObject.FindGameObjectsWithTag("unit"), "PhotosensitiveResistorElm") > 0) {
			Scroll.SetActive (true);
			Scroll.GetComponent<LightUpdate> ().Init ();
		}
		onConnectedButton.SetActive(false);     //隐藏确认连接按钮
		offConnectedButton.SetActive(true);     //显示断开连接按钮
		Utils.gestureLayer = true;              //更改手势层数
		CirSim.analyzeCircuit();                //分析电路
	}

	private int exist(GameObject[] gs, string s) {
		for (int i = 0; i < gs.Length; i++) {
			if(gs[i].transform.name.Length > 23) {
				if(gs[i].transform.name.Substring(0, 25).Equals(s) && gs[i].transform.parent != null) {
					Utils.ElmCounts++;
				}
			}
		}
		return Utils.ElmCounts;
	}

	//释放连接按钮
	public void OffConnected() {
		Utils.audioManager.playAudio(1);        			//播放音效
		if(Utils.buttonName.Equals("EX_9") && exist(GameObject.FindGameObjectsWithTag("unit"), "PhotosensitiveResistorElm") > 0) {
			Scroll.SetActive (false);
			GameObject[] gs = GameObject.FindGameObjectsWithTag ("SpotLight");
			for (int i = 0; i < gs.Length; i++) {
				gs [i].GetComponent<Light> ().intensity = 0.0f;
				gs [i].SetActive (false);
			}
		}
		onConnectedButton.SetActive(true);      			//隐藏确认连接按钮
		offConnectedButton.SetActive(false);    			//显示断开连接按钮
		Utils.gestureLayer = false;             			//更改手势层数
		Utils.ElmCounts = 0;
		Utils.Magnifier.SetActive(false);					//关闭放大镜
		for (int i = 0; i < CirSim.elmList.Count; i++) {	//遍历元器件列表
			CirSim.getElm(i).stop();						//停止工作
		}
	}

	//返回到选关界面
	public void BackToSelection() {
		Utils.audioManager.playAudio (1);				//播放声音
		CirSim.elmList.Clear();                     			//清空元器件列表
		CirSim.nodeList.Clear();                    			//清空结点列表
		if(Utils.ExperimentalType == 1) {						//自由模式
			Application.LoadLevel (Utils.MainInterface);		//进入选择界面
		} else if(Utils.ExperimentalType == 0) {				//实验模式
			Application.LoadLevel (Utils.SelectedInterface);	//进入选择界面
		}
    }

    //弹出提示UI
    public void PopTipUI(string str) {
        StartCoroutine(Wait(4, str));   //等待弹出协程
    }

    //等待time秒
    IEnumerator Wait(float time, string str) {

        Tip.SetActive(true);                        //弹出提示界面
        TipText.GetComponent<Text>().text = str;    //更改提示文字
        yield return new WaitForSeconds(time);
        Color tip = Tip.GetComponent<Image>().color;                                            //获取提示图片颜色
        Color tipText = TipText.GetComponent<Text>().color;                                     //获取文字颜色
        while (tip.a > 0) {
            Tip.GetComponent<Image>().color = new Color(tip.r, tip.g, tip.b, tip.a - 0.02f);                //增加透明度
            TipText.GetComponent<Text>().color = new Color(tipText.r, tipText.g, tipText.b, tip.a - 0.02f); //获取提示文字颜色
            tipText = TipText.GetComponent<Text>().color;                                       //获取文字颜色
            tip = Tip.GetComponent<Image>().color;                                              //获取提示图片颜色
            yield return 0;
        }
        Debug.Log("设为隐藏");
        Tip.GetComponent<Image>().color = new Color(tip.r, tip.g, tip.b, 1);                    //增加透明度
        TipText.GetComponent<Text>().color = new Color(tipText.r, tipText.g, tipText.b, 1);     //获取提示文字颜色
        Tip.SetActive(false);                                                                   //隐藏提示文字
    }

    //重置实验
    public void Reset() {
		Utils.audioManager.playAudio (1);				//播放声音
		if (Utils.gestureLayer) {											//如果确认连接了
			return;															//不能重置实验
		}
		for (int i = 0; i < CirSim.elmList.Count; i++) {					//遍历元器件列表
			Destroy(CirSim.getElm(i).gameObject);  				            //销毁所有元器件
		}
        for (int i = 0; i < CirSim.damageList.Count; i++) {                 //遍历损坏元器件列表
            Destroy(CirSim.damageList[i].gameObject);                       //销毁损坏的元器件
        }
		CirSim.damageList.Clear ();											//清空损坏器件列表
        CirSim.elmList.Clear();                             				//清空元器件列表
		CirSim.nodeList.Clear();                            				//清空结点列表
		Utils.reset();                                      				//重置摄像机与底板位置
	}

	//进入介绍元器件界面
	public void ToIntroduction() {
		Utils.audioManager.playAudio (1);				//播放声音
		Utils.isIntroduction = true;													//处于实验界面
		CameraRotation = MyCamera.transform.rotation;									//记录之前摄像机旋转角
		CameraControllerPosition = CameraController.transform.position;					//记录之前摄像机父类位置
		CameraControllerRotation = CameraController.transform.rotation;					//记录之前摄像机父类旋转角
		Utils.reset();                                      							//重置摄像机与底板位置
		transform.GetComponent<Canvas> ().renderMode = RenderMode.ScreenSpaceCamera;	//调整画布模式
		IntroductionUI.SetActive (true);												//进入介绍界面
		if(introductionIndex != 0) {
			IntroductionUI.transform.FindChild ("Previous").gameObject.SetActive (true);
		}
		UnitGroup.SetActive(true);														//介绍器件显示
		Rabbit.SetActive(true);															//显示兔子模型
		Debug.Log ("10");
		UnitGroup.transform.GetChild (0).GetComponent<RotationSelf> ().enabled = true;
		content.GetComponent<Read> ().read (Utils.experiments [introductionIndex]);
		Plate.SetActive (false);														//底板消失
		Room2.SetActive(false);															//房间消失
		unitGroup.SetActive(false);														//实验器材消失
		unitGroup2.SetActive(false);													//实验器材消失
	}

	//返回实验的方法
	public void BackToExperiment() {
		Utils.audioManager.playAudio (1);				//播放声音
		Utils.isIntroduction = false;													//未处于实验界面
		CameraController.transform.position = CameraControllerPosition;					//重置摄像机父类
		CameraController.transform.rotation = Quaternion.Euler(
			CameraControllerRotation.eulerAngles); 										//重置摄像机父类
		MyCamera.transform.rotation = Quaternion.Euler(CameraRotation.eulerAngles); 	//重置摄像机
		Plate.SetActive (true);															//底板重现
		Room2.SetActive(true);															//房间重现
		unitGroup.SetActive(true);														//实验期器件重现
		transform.GetComponent<Canvas> ().renderMode = RenderMode.ScreenSpaceOverlay;	//调整画布模式
		UnitGroup.SetActive(false);														//介绍元件消失
		content.GetComponent<Text>().text = "";											//元器件介绍文字置空
        contentEX.GetComponent<Text>().text = "";                                       //实验介绍文字置空
		Rabbit.SetActive(false);														//不显示兔子模型
        Movie.SetActive(false);                                                         //显示屏隐藏
		IntroductionUI.SetActive (false);												//介绍元器件界面隐藏
        EXIntroductionUI.SetActive(false);                                              //介绍实验界面隐藏
	}

    //进入介绍实验界面
    public void ToEXIntroduction() {
		Utils.audioManager.playAudio (1);				//播放声音
        Utils.isIntroduction = true;													//处于实验界面
        CameraRotation = MyCamera.transform.rotation;                                   //记录之前摄像机旋转角
        CameraControllerPosition = CameraController.transform.position;                 //记录之前摄像机父类位置
        CameraControllerRotation = CameraController.transform.rotation;					//记录之前摄像机父类旋转角
        Utils.reset();                                                                  //重置摄像机与底板位置
        transform.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceCamera;	    //调整画布模式
        EXIntroductionUI.SetActive(true);												//进入实验介绍界面
        int movieIndex = int.Parse(Utils.buttonName.Substring(3));                      //获取实验序号
        //读取文本到文本框
        Movie.SetActive(true);															//显示视频模型
        Plate.SetActive(false);                                                         //底板消失
        Room2.SetActive(false);                                                         //房间消失
        unitGroup.SetActive(false);														//实验器材消失
		unitGroup2.SetActive(false);													//实验器材消失
		contentEX.GetComponent<Read> ().readex ();

		if (Utils.ExperimentIndex < Utils.BuiltInCounts) {
			movieController.GetComponent<MMT.MobileMovieTexture> ().AbsolutePath = false;
			movieController.GetComponent<MMT.MobileMovieTexture> ().Path = Utils.buttonName + ".ogv";//更换播放视频
		} else {
			movieController.GetComponent<MMT.MobileMovieTexture> ().AbsolutePath = true;
			movieController.GetComponent<MMT.MobileMovieTexture> ().Path = Application.persistentDataPath + "/EX/" + Utils.buttonName + "/" + Utils.buttonName + ".ogv";//更换播放视频
		}

		movieController.GetComponent<MMT.MobileMovieTexture>().Play();                	//播放视频
    }

	//切换元器件（自由模式）
	public void Switch() {
		if(isSwitchMoving) {
			return;
		}
		if (switchunit) {
			StartCoroutine (switchGroup (unitGroup, unitGroup2));
		} else {
			StartCoroutine (switchGroup (unitGroup2, unitGroup));
		}
		switchunit = !switchunit;
	}

	//切换元器件的协成
	IEnumerator switchGroup(GameObject One, GameObject Two) {
		isSwitchMoving = true;
		bool isMoving = true;
		float temp = 0.03f;
		while (isMoving) {
			temp += 0.002f;
			One.transform.position = new Vector3 (One.transform.position.x, One.transform.position.y - temp, One.transform.position.z);
			yield return 0;
			if (One.transform.position.y < -1.14f) {
				One.transform.position = new Vector3 (One.transform.position.x, -1.15f, One.transform.position.z);
				One.SetActive (false);
				Two.SetActive (true);
				isMoving = false;
			}
		}
		isMoving = true;
		temp = 0.05f;
		while (isMoving) {
			temp += 0.002f;
			Two.transform.position = new Vector3 (Two.transform.position.x, Two.transform.position.y + temp, Two.transform.position.z);
			yield return 0;
			if (Two.transform.position.y > 0.0f) {
				Two.transform.position = new Vector3 (Two.transform.position.x, 0.0f, Two.transform.position.z);
				for(int i = 0; i < Two.transform.childCount; i++) {
					Transform tf = Two.transform.GetChild (i);
					tf.position = new Vector3 (tf.position.x, 0.0f, tf.position.z);
					tf.GetComponent<CircuitElm>().originalPos = new Vector3 (tf.position.x, 0.0f, tf.position.z);
				}
				isMoving = false;
			}
		}
		isSwitchMoving = false;
	}

	//返回主界面 
	public void BackToHome() {
		Utils.audioManager.playAudio (1);					//播放声音
		Application.LoadLevel (Utils.MainInterface);		//返回主界面
	}

	//加载介绍模型
	public void LoadIntroductionUnit() {
		introductions = new GameObject[Utils.experiments.Length];						//初始化数组
		for (int i = 0; i < Utils.experiments.Length; i++) {							//遍历介绍元件数组
			switch(Utils.experiments[i]) {												//分别做动态加载
			case 1:                                                                     //1-导线
				LoadModel(loadIntroductionPrefabPath + "WireElm_1", i);					//加载对应模型
				break;
			case 2:                                                                     //2-导线
				LoadModel(loadIntroductionPrefabPath + "WireElm_2", i);					//加载对应模型
				break;
			case 3:                                                                     //3-导线
				LoadModel(loadIntroductionPrefabPath + "WireElm_3", i);					//加载对应模型
				break;
			case 15:																	//15-开关
				LoadModel(loadIntroductionPrefabPath + "SwitchElm", i);					//加载对应模型
				break;
			case 16:																	//16-光敏电阻
				LoadModel(loadIntroductionPrefabPath + "PhotosensitiveResistorElm", i);	//加载对应模型
				break;
			case 17:                                                                    //17-二极管
				LoadModel(loadIntroductionPrefabPath + "DiodeElm", i);					//加载对应模型
				break;
			case 18:																	//18-小灯泡
				LoadModel(loadIntroductionPrefabPath + "BulbElm", i);					//加载对应模型
				break;
			case 19:																	//19-电池
				LoadModel(loadIntroductionPrefabPath + "VoltageElm", i);				//加载对应模型
				break;
			case 20:                                                                    //20-喇叭
				LoadModel(loadIntroductionPrefabPath + "HornElm", i);					//加载对应模型
				break;
			case 21:                                                                    //21-音乐集成电路
				LoadModel(loadIntroductionPrefabPath + "MusicChipElm", i);				//加载对应模型
				break;
			case 24:																	//24-风扇
				LoadModel(loadIntroductionPrefabPath + "EngineElm", i);					//加载对应模型
				break;
			case 28:																	//28-话筒
				LoadModel(loadIntroductionPrefabPath + "MicrophoneElm", i);				//加载对应模型
				break;
			case 30:                                                                    //30-电阻
				LoadModel(loadIntroductionPrefabPath + "ResistanceElm", i);				//加载对应模型
				break;
			case 53:																	//53-滑动变阻器
				LoadModel(loadIntroductionPrefabPath + "SlidingResistanceElm", i);		//加载对应模型
				break;
			case 56:                                                                    //56-电流表
				LoadModel(loadIntroductionPrefabPath + "AmmeterElm", i);				//加载对应模型
				break;
			case 58:                                                                  	//58-数码管
				LoadModel(loadIntroductionPrefabPath + "CounterElm", i);				//加载对应模型
				break;
			case 62:                                                                  	//62-录音IC
				LoadModel(loadIntroductionPrefabPath + "RecorderChipElm", i);			//加载对应模型
				break;
			}
		}
	}

	//加载模型的方法
	private void LoadModel(string filePath, int i) {
		introductions[i] = Resources.Load(filePath) as GameObject;
		/*float aimX = 0.95f - i * 5.16f;												//计算目标位置X
		Vector3 InstantiatePosition = new Vector3(aimX, 1.55f, 1.75f);					//目标位置
		Quaternion InstantiateRotation = new Quaternion(0, 0, 0, 0);					//定义四元数
		InstantiateRotation.eulerAngles = new Vector3(-90, 160, 0);						//更改旋转角*/

		float aimZ = 1.7f - i * 4.2f;													//计算目标位置X
		Vector3 InstantiatePosition = new Vector3(0.8f, 1.55f, aimZ);					//目标位置
		Quaternion InstantiateRotation = new Quaternion(0, 0, 0, 0);					//定义四元数
		InstantiateRotation.eulerAngles = new Vector3(-45, 180, 0);						//更改旋转角

		GameObject Model = (GameObject)Instantiate (introductions[i], 
			InstantiatePosition, InstantiateRotation);									//获取加载模型
		Model.name = Model.name.Replace("(Clone)", "");									//去除克隆名称
		Model.transform.SetParent(UnitGroup.transform);									//设置父物体
	}

	//介绍界面呼吸灯效果
	public void bitTwinkle() {
		animation["Twinkle"].speed = 0.2f;			//减慢呼吸动画的播放速度为正常速度的0.2倍
	}

	//前一个元器件介绍
	public void Previous() {
		if(!isMoving) {													//如果没有正在移动
			Utils.audioManager.playAudio (1);				//播放声音
			content.GetComponent<Read> ().read (Utils.experiments [introductionIndex-1]);
			introductionIndex--;										//介绍元器件索引减一
			StartCoroutine (move(-4.2f));								//开启前翻协成
		}
		if(introductionIndex == 0) {									//如果当前是第1个介绍元件
			PreviousButton.SetActive (false);							//关闭前一个按钮
		}
		if(introductionIndex == Utils.experimentCount - 2) {			//如果当前是倒数第2个介绍元件
			NextButton.SetActive (true);								//打开下一个按钮
		}
	}

	//下一个元器件介绍
	public void Next() {
		if(!isMoving) {													//如果没有正在移动
			Utils.audioManager.playAudio (1);				//播放声音
			content.GetComponent<Read> ().read (Utils.experiments [introductionIndex+1]);
			introductionIndex++;										//介绍元器件索引减一
			StartCoroutine (move(4.2f));								//开启前翻协成
		}
		if(introductionIndex == 1) {									//如果当前是第2个介绍元件
			PreviousButton.SetActive (true);							//打开前一个按钮
		}
		if(introductionIndex == Utils.experimentCount - 1) {			//如果当前是最后1个介绍元件
			NextButton.SetActive (false);								//关闭下一个按钮
		}
	}

	//移动元器件介绍
	IEnumerator move(float temp) {
		/*Vector3 aimPosition = new Vector3 (UnitGroup.transform.position.x + temp, 
			UnitGroup.transform.position.y, UnitGroup.transform.position.z);			//定义目标点
		while (Vector3.Distance(UnitGroup.transform.position, aimPosition) > 0.001f) {	//如果距离不到预期目标
			isMoving = true;															//正在移动
			UnitGroup.transform.position = Vector3 .Lerp(
				UnitGroup.transform.position, aimPosition, 0.1f);						//移动
			yield return 0;																//返回
		}*/

		Vector3 aimPosition = new Vector3 (UnitGroup.transform.position.x, 
			UnitGroup.transform.position.y, UnitGroup.transform.position.z + temp);		//定义目标点
		while (Vector3.Distance(UnitGroup.transform.position, aimPosition) > 0.001f) {	//如果距离不到预期目标
			isMoving = true;															//正在移动
			UnitGroup.transform.position = Vector3 .Lerp(
				UnitGroup.transform.position, aimPosition, 0.1f);						//移动
			yield return 0;																//返回
		}
		UnitGroup.transform.GetChild (introductionIndex).GetComponent<RotationSelf> ().enabled = true;
		if (introductionIndex + 1 < UnitGroup.transform.childCount) {
			UnitGroup.transform.GetChild (introductionIndex + 1).GetComponent<RotationSelf> ().enabled = false;
			UnitGroup.transform.GetChild (introductionIndex + 1).transform.rotation =  Quaternion.Euler(-45, 180, 0);
		}
		if (introductionIndex - 1 >= 0) {
			UnitGroup.transform.GetChild (introductionIndex - 1).GetComponent<RotationSelf> ().enabled = false;
			UnitGroup.transform.GetChild (introductionIndex - 1).transform.rotation =  Quaternion.Euler(-45, 180, 0);
		}
		isMoving = false;																//改为没有正在移动
	}
	//****************************************************************************************

	//***********************************所有元器件介绍界面***********************************
	//加载所有介绍元器件的方法
	public void ReadAllIntroductionUnit() {
		string s = Utils.LoadFile ("ExperimentFree.txt");											//下载相关txt数据文件
		string[] ss = s.Split ('\n');																//以回车切割文件为string数组
		Utils.experimentCount = int.Parse(ss[0].Trim());											//获取需要介绍的元器件数量
		Utils.experiments = new int[Utils.experimentCount];											//初始化实验元件代号数组
		for (int i = 1; i < ss.Length; i++) {														//遍历txt文件每一行（除第一行外）
			string[] sss = ss [i].Split (' ');														//以空格分割
			Utils.experiments [i - 1] = int.Parse(sss [0]);											//赋值
		}
	}

	//加载介绍模型
	public void LoadAllIntroductionUnit() {
		AllIntroductions = new GameObject[Utils.experiments.Length];					//初始化数组
		for (int i = 0; i < Utils.experiments.Length; i++) {							//遍历介绍元件数组
			switch(Utils.experiments[i]) {												//分别做动态加载
			case 1:                                                                     //1-导线
				LoadAllModel(loadIntroductionPrefabPath + "WireElm_1", i);					//加载对应模型
				break;
			case 2:                                                                     //2-导线
				LoadAllModel(loadIntroductionPrefabPath + "WireElm_2", i);					//加载对应模型
				break;
			case 3:                                                                     //3-导线
				LoadAllModel(loadIntroductionPrefabPath + "WireElm_3", i);					//加载对应模型
				break;
			case 15:																	//15-开关
				LoadAllModel(loadIntroductionPrefabPath + "SwitchElm", i);					//加载对应模型
				break;
			case 16:																	//16-光敏电阻
				LoadAllModel(loadIntroductionPrefabPath + "PhotosensitiveResistorElm", i);					//加载对应模型
				break;
			case 17:                                                                    //17-二极管
				LoadAllModel(loadIntroductionPrefabPath + "DiodeElm", i);					//加载对应模型
				break;
			case 18:																	//18-小灯泡
				LoadAllModel(loadIntroductionPrefabPath + "BulbElm", i);					//加载对应模型
				break;
			case 19:																	//19-电池
				LoadAllModel(loadIntroductionPrefabPath + "VoltageElm", i);				//加载对应模型
				break;
			case 20:                                                                    //20-喇叭
				LoadAllModel(loadIntroductionPrefabPath + "HornElm", i);					//加载对应模型
				break;
			case 21:                                                                    //21-音乐集成电路
				LoadAllModel(loadIntroductionPrefabPath + "MusicChipElm", i);				//加载对应模型
				break;
			case 24:																	//24-风扇
				LoadAllModel(loadIntroductionPrefabPath + "EngineElm", i);					//加载对应模型
				break;
			case 28:																	//28-话筒
				LoadAllModel(loadIntroductionPrefabPath + "MicrophoneElm", i);					//加载对应模型
				break;
			case 30:                                                                    //30-电阻
				LoadAllModel(loadIntroductionPrefabPath + "ResistanceElm", i);				//加载对应模型
				break;
			case 53:																	//53-滑动变阻器
				LoadAllModel(loadIntroductionPrefabPath + "SlidingResistanceElm", i);		//加载对应模型
				break;
			case 56:                                                                    //56-电流表
				LoadAllModel(loadIntroductionPrefabPath + "AmmeterElm", i);				//加载对应模型
				break;
			case 58:                                                                  	//58-数码管
				LoadAllModel(loadIntroductionPrefabPath + "CounterElm", i);				//加载对应模型
				break;
			case 62:                                                                  	//62-录音IC
				LoadAllModel(loadIntroductionPrefabPath + "RecorderChipElm", i);				//加载对应模型
				break;
			}
		}
	}

	//加载模型的方法
	private void LoadAllModel(string filePath, int i) {
		AllIntroductions[i] = Resources.Load(filePath) as GameObject;

		float aimY = 0.95f - i * 3.2f;													//计算目标位置X
		Vector3 InstantiatePosition = new Vector3(-0.65f, aimY, -6.5f);					//目标位置
		Quaternion InstantiateRotation = new Quaternion(0, 0, 0, 0);					//定义四元数
		InstantiateRotation.eulerAngles = new Vector3(-90, 0, 0);					//更改旋转角

		GameObject Model = (GameObject)Instantiate (AllIntroductions[i], 
			InstantiatePosition, InstantiateRotation);									//获取加载模型
		Model.name = Model.name.Replace("(Clone)", "");									//去除克隆名称
		Model.transform.SetParent(AllUnitGroup.transform);								//设置父物体
	}

	//返回到主界面
	public void BackToMain() {
		Utils.audioManager.playAudio (1);				//播放声音
		Application.LoadLevel (Utils.MainInterface);	//进入主界面
	}

	//前一个元器件介绍
	public void AllPrevious() {
		if(!isMoving) {													//如果没有正在移动
			Utils.audioManager.playAudio (1);							//播放声音
			introductionIndex--;										//介绍元器件索引减一
			if(introductionIndex == -1) {
				introductionIndex = 12;
				AllUnitGroup.transform.position = new Vector3 (0, 13.0f * 3.2f, 0);
			}
			AllContent.GetComponent<Read> ().read (Utils.experiments [introductionIndex]);
			//Debug.Log (AllUnitGroup.transform.GetChild (introductionIndex).name);
			float aimY = 0.0f + introductionIndex * 3.2f;
			StartCoroutine (AllMove(aimY));								//开启前翻协成
		}
	}

	//下一个元器件介绍
	public void AllNext() {
		if(!isMoving) {													//如果没有正在移动
			Utils.audioManager.playAudio (1);				//播放声音
			introductionIndex++;										//介绍元器件索引加一
			if(introductionIndex == 13) {
				introductionIndex = 0;
				AllUnitGroup.transform.position = new Vector3 (0, -3.2f, 0);
			}
			AllContent.GetComponent<Read> ().read (Utils.experiments [introductionIndex]);
			//Debug.Log (AllUnitGroup.transform.GetChild (introductionIndex).name);
			float aimY = 0.0f + introductionIndex * 3.2f;
			StartCoroutine (AllMove(aimY));								//开启前翻协成
		}
	}

	//移动元器件介绍
	IEnumerator AllMove(float temp) {
		Vector3 aimPosition = new Vector3 (AllUnitGroup.transform.position.x, 
			temp, AllUnitGroup.transform.position.z);		//定义目标点
		while (Vector3.Distance(AllUnitGroup.transform.position, aimPosition) > 0.001f) {	//如果距离不到预期目标
			isMoving = true;																//正在移动
			AllUnitGroup.transform.position = Vector3 .Lerp(
				AllUnitGroup.transform.position, aimPosition, 0.1f);						//移动
			yield return 0;																	//返回
		}
		AllUnitGroup.transform.GetChild (introductionIndex).GetComponent<RotationSelf> ().enabled = true;
		if (introductionIndex + 1 < AllUnitGroup.transform.childCount) {
			AllUnitGroup.transform.GetChild (introductionIndex + 1).GetComponent<RotationSelf> ().enabled = false;
			AllUnitGroup.transform.GetChild (introductionIndex + 1).transform.rotation =  Quaternion.Euler(-90, 0, 0);
		}
		if (introductionIndex - 1 >= 0) {
			AllUnitGroup.transform.GetChild (introductionIndex - 1).GetComponent<RotationSelf> ().enabled = false;
			AllUnitGroup.transform.GetChild (introductionIndex - 1).transform.rotation =  Quaternion.Euler(-90, 0, 0);
		}
		isMoving = false;																	//改为没有正在移动
	}


	//****************************************************************************************
	//监听返回键的方法
	private void EscapeListen() {
		if (Input.GetKeyDown (KeyCode.Escape)) {						//如果点击返回（安卓）
			switch(Application.loadedLevel) {							//区分本场景做出反应
			case 0:														//如果是主界面场景
				Application.Quit ();									//程序退出
				break;													//跳出判断
			case 1:														//如果是实验场景
				if (!Utils.isIntroduction) {							//如果处于实验界面
					BackToSelection(); 									//返回选关界面
				} else {
					BackToExperiment ();								//返回实验界面
				}
				break;													//跳出判断
			case 2:														//如果是介绍场景
				Application.LoadLevel (Utils.MainInterface);			//进入主场景界面
				break;													//跳出判断
			case 4:
				Application.LoadLevel (Utils.MainInterface);			//进入主场景界面
				break;
			}
		}
	}

}
