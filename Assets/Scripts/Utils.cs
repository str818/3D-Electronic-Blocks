using System;
using System.IO;
using System.Text;
using UnityEngine;
using System.Collections;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;
using System.Collections.Generic;

//工具类
public class Utils : MonoBehaviour {

	public static int MainInterface = 0;										//主界面代号
	public static int ExperimentalInterface = 1;								//实验界面代号
	public static int SelectedInterface = 2;									//选择界面代号
	public static int TransitionInterface = 3;									//过渡界面代号
	public static int AllIntroduceInterface = 4;                                //所有元器件介绍界面代号
    public static int LoadEXSenceInterface = 5;                                 //加载实验资源包界面代号

	public static int BuiltInCounts = 6;                                 		//加载实验资源包界面代号

    public static bool isMusic = true;											//是否打开音乐
	public static bool isAudio = true;											//是否打开音效

	//public static string IP = "http://120.24.217.144:8080";					//云服务器地址
	//public static string IP = "http://10.128.114.148:8080";					//本地服务器地址
	public static string IP = "http://192.168.43.79:8080";						//hjc手机热点服务器地址

    public static ArrayList loadEXList = new ArrayList();                       //服务器资源包列表
    public static ArrayList localEXList = new ArrayList();                      //本地资源包列表
    public static MenuListener menuListener;                                    //菜单管理脚本
    public static AudioManager audioManager;                                    //音效脚本
	public static GameObject plate;												//底板对象
    public static GameObject cameraController;                                  //摄像机管理对象
    public static GameObject camera;                                            //摄像机
	public static GameObject Magnifier;                                         //放大镜
    public static MicphoneTest MyMicphone;                                      //录音对象
    public static string holdingName;                                           //当前正在触摸的元器件
    public static bool gestureLayer;                                            //手势层数 false-触摸器件层 true-连接后电路操作层
	public static int ExperimentalType;											//实验类型 0-试验模式 1-自由模式
	public static int experimentCount;											//传输到实验界面的元器件数量
	public static int[] experiments;											//实验界面的元器件代号数组
	public static string buttonName;											//被点击按钮名称
	public static int ExperimentIndex;											//第几个实验
	public static bool isIntroduction;											//是否处于介绍界面
    public static bool isCameraMove;                                            //摄像机是否跟随风扇移动
	public static int loadAimScene;												//目标场景
	public static int ElmCounts;												//安在地板上的器件总个数

    void Start() {
		plate = GameObject.Find ("Plate");												//获取底板引用
		holdingName = "null";                                                   		//手持元器件置null
		audioManager = transform.GetComponent<AudioManager> ();                  		//获取音效脚本
		cameraController = GameObject.Find ("CameraController");                 		//获取摄像机管理对象
		camera = GameObject.Find ("CameraController/Main Camera");               		//获取摄像机对象
		if (Application.loadedLevel == Utils.ExperimentalInterface) {					//如果是实验界面
			Magnifier = GameObject.Find ("Magnifier");                           		//获取摄像机对象
			Magnifier.SetActive (false);												//不可见放大镜
			menuListener = GameObject.Find ("Canvas").GetComponent<MenuListener> ();  	//获取菜单管理脚本
            MyMicphone = GameObject.Find("MicphoneObject").GetComponent<MicphoneTest>();//获取录音脚本
		}
	}

    //重置摄像机与底板位置
    public static void reset() {
		//设置底板位置
		plate.transform.position = new Vector3 (0, 0, 0);
		plate.transform.rotation = new Quaternion (0, 0, 0, 0);
		//设置摄像机控制对象位置
		cameraController.transform.position = new Vector3 (0, 4.05f, 3.41f);
		cameraController.transform.rotation = new Quaternion (0, 0, 0, 0);
		//设置摄像机位置
		camera.transform.rotation = Quaternion.Euler (55, 180, 0);
	}

    //计算下一个元器件的连接端口（输入端口）
	//root--此元器件 next--下一个元器件
    public static int calNextPort(CircuitElm root, CircuitElm next) {
        for (int i = 0; i < next.interfaceList.Count; i++) {
            if (next.interfaceList[i]!=null 
				&& next.interfaceList[i].Equals(root)) {					//若该端口连接为此元器件
                return i;													//返回连接端口号
            }
        }
        return -1;															//连接错误
	}

    //计算同侧端口
	//index-端口
    private static int calSamePort(int index) {
        if (index % 2 == 0) {												//该端口在上方
            return index + 1;												//返回下方端口号
        }else if(index % 2 == 1) {											//该端口在下方
            return index - 1;												//返回上方端口号
        }
        return -1;															//错误
	}

    //计算中心的方法
    public static Vector3 calCore(ArrayList colliders, List<int> collidersIndex, int downHole) {

        Vector3 resultV = new Vector3(0.0f, 0.0f, 0.0f);
        switch (downHole) {
            case 1:
            case 2:
                return calSymmetryCenter(colliders, collidersIndex, 2);
            case 3:
                return calSymmetryCenter(colliders, collidersIndex, 3);
            case 4:
                return calSymmetryCenter(colliders, collidersIndex, 4);
        }
        return resultV;
    }

    //计算非三角形的中心点
    private static Vector3 calSymmetryCenter(ArrayList colliders, List<int> collidersIndex, int downHole) {

        Vector3 center = new Vector3(0, 0, 0);      //中心位置
        if (downHole == 2) {                    //端口数为0
            float totalX = 0;
            float totalZ = 0;
            for (int i = 0; i < downHole; i++) {
                totalX += ((GameObject)colliders[i]).transform.position.x;
                totalZ += ((GameObject)colliders[i]).transform.position.z;
            }
            center = new Vector3(totalX / downHole, 0f, totalZ / downHole);
        }
        else {
            int x = 0, y = 0;                       //0，1两个端点对应的纽扣号
            int x_port = 0, y_port = 1;             //决定两个端点的序号，若是三个底口，将决定设置位置的端口定为0-1
            if (downHole == 4) {                    //若是4个底口（录音机），将决定设置位置的端口定为0-2
                y_port = 2;
            }
            for (int i = 0; i < downHole; i++) {             //获取对应的纽扣号
                if (collidersIndex[i] == x_port) x = i;
                if (collidersIndex[i] == y_port) y = i;
            }
            float totalX = 0;
            float totalZ = 0;
            totalX += ((GameObject)colliders[x]).transform.position.x;
            totalZ += ((GameObject)colliders[x]).transform.position.z;
            totalX += ((GameObject)colliders[y]).transform.position.x;
            totalZ += ((GameObject)colliders[y]).transform.position.z;
            center = new Vector3(totalX / 2, 0f, totalZ / 2);
        }
        return center;
    }

    //调整元器件的高度(真正实现的方法)
    //unit-元器件引用 floor-元器件层数
    private static void adjustHeightAchieve(Transform unit, int floor) {
        float height = 0f;          	//对应层数的高度值
        switch (floor) {
            case 0:
                height = 0f;
                break;
            case 1:
                height = 0.064f;
                break;
            case 2:
                height = 0.157f;
                break;
            case 3:
                height = 0.250f;
                break;
            case 4:
                height = 0.343f;
                break;
        }
        unit.position = new Vector3(unit.position.x, height, unit.position.z);	//重新调整元器件的高度值
    }

    //调整元器件的高度
    public static void adjustHeight(Transform unit, ArrayList unitColliders) {//unit-元器件引用 unitColliders-元器件碰撞列表
        CircuitElm temp = unit.GetComponent<CircuitElm>();//元器件脚本
        int currentFloor = temp.floor;  //元器件当前层数
        int maxFloor = 0;               //碰撞列表中的最大层数

        //获取最大层数
        for(int i = 0; i < unitColliders.Count; i++) {
            CircuitElm tempSon = (CircuitElm)unitColliders[i];//强制转换为原始类型
            if (tempSon.floor > maxFloor) maxFloor = tempSon.floor;
        }

        temp.floor = maxFloor + 1;              //提高层数
        adjustHeightAchieve(unit, maxFloor + 1);//调整高度
    }

	//解压缩Zip的方法,zipFilePath为zip包路径
	public static void UnZipFile(string zipFilePath, string downpath){
		using (ZipInputStream s = new ZipInputStream(File.OpenRead(zipFilePath))) {			//读取资源包，并放入解压缩流
			ZipEntry theEntry;														
			while ((theEntry = s.GetNextEntry()) != null){									//当前文件夹内不为空
				string directoryName = Path.GetDirectoryName(theEntry.Name);				//获取文件夹路径
				string fileName = Path.GetFileName(theEntry.Name);							//获取文件名
				if (directoryName.Length > 0){
					Directory.CreateDirectory(downpath + "/" + directoryName);				//创建解压路径目录
				}
				if (fileName != String.Empty){												//文件名不为空
					using (FileStream streamWriter = File.Create(downpath + "/" + theEntry.Name)){
						int size = 2048;													//解压文件到指定目录
						byte[] data = new byte[2048];										//每次读取数据长度
						while (true){
							size = s.Read(data, 0, data.Length);							//读取数据
							if (size > 0) {
								streamWriter.Write(data, 0, size);							//写入数据
							} else {
								break;
							}
						}
					}
				}
			}
		}
	}

	//读取本地Txt文件
	public static string LoadFile(string filePath) {
		string url = Application.streamingAssetsPath + "/" + filePath;
		#if UNITY_EDITOR
		return File.ReadAllText(url);
		#elif UNITY_ANDROID
		WWW www = new WWW(url);
		while (!www.isDone) { }
		return www.text;
		//return www.text.Split(new string[]{"\r\n"}, System.StringSplitOptions.None);
		#endif
	}

	//读取txt中的信息
	public static void loadText(string path, ArrayList al) {
		al.Clear();                                                                 //清空列表
		using (System.IO.StreamReader sr = new System.IO.StreamReader(path, Encoding.Default)) {
			string str;
			while ((str = sr.ReadLine()) != null) {
				al.Add(str);                                                   		//读取文本中的内容到布展包列表中
			}
		}
	}

	//获取图片
	public static Sprite LoadImage(string path) {
		//创建文件读取流
		FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
		fileStream.Seek (0, SeekOrigin.Begin);
		//创建文件长度缓冲区
		byte[] bytes = new byte[fileStream.Length];
		//读取文件
		fileStream.Read(bytes, 0, (int)fileStream.Length);
		//释放文件读取流
		fileStream.Close();
		fileStream.Dispose ();
		fileStream = null;

		//创建Texture
		int width = 800;
		int height = 500;
		Texture2D texture = new Texture2D(width, height);
		texture.LoadImage(bytes);

		//创建Sprite
		Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
		return sprite;
	}
	
	public static void CreatFolder(string spath) {											//创建手机本地文件夹
        if (!Directory.Exists(spath)) { 											//如果不存在该文件夹
			Directory.CreateDirectory(spath);										//创建文件夹
        }
    }
	
	public static void CreateFile(string spath, string name) {								//创建txt文本
		if (Directory.Exists (spath) && !File.Exists (spath + "/" + name)) { 		//如果不存在该文件夹
			FileStream stream = File.Create (spath + "/" + name);					//创建文件
			stream.Close ();														//如不及时关闭,短时间进行其他操作会有问题
		}
    }



}
