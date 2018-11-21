using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

//初始实验加载列表
public class InitialEXList : MonoBehaviour {

    public GameObject UIPrefab;                                 				//下载资源包预制件
	public GameObject UIPrefabEnabled;											//不能下载资源包预制件
    public GameObject Canvas;                                  		 			//画布对象
	private Vector2[] buttonsPosition;											//按钮坐标数组
	private string loadPath = Utils.IP + "/dianzijimu";							//下服务器上图片资源包位置路径
	private string ImagePath = Application.persistentDataPath + "/EX";  		//本地图片包文件夹路径
	private ArrayList rgbList;													//存储rgb颜色的列表

	void Start () {
		StartCoroutine(downAsFile(loadPath + "/Images.zip",
			ImagePath + "/Images.zip"));										//下载图片资源包并且根据是否下载该数据包实例化相应的预制件
	}

	//从服务器上下载文件并保存到本地
	IEnumerator downAsFile(string path, string filename) {                              //path-服务器上的下载路径 filename-下载到的本地路径
		WWW www = new WWW(path);                                                        //定义WWW类
		yield return www;                                                               //返回www
		if (www.error == null) {                                                        //如果解压完成
			byte[] stream = www.bytes;                                                  //定义字节数组
			if (!File.Exists(filename)) {
				FileStream fs = new FileStream(filename, FileMode.CreateNew);           //定义FileStream
				BinaryWriter w = new BinaryWriter(fs);                                  //定义BinaryWriter
				w.Write(stream);                                                        //写入字节数组
				fs.Close();                                                             //关闭FileStream
				w.Close();                                                              //关闭BinaryWriter										
			}
			Utils.UnZipFile(ImagePath + "/Images.zip", ImagePath); 						//解压下载的图片资源包
			Debug.Log("结束下载图片包");												//打印完成信息
			rgbList = new ArrayList();
			loadText (ImagePath + "/Images/ImagesRGB.txt", rgbList);				//初始颜色rgb列表
			InitButtonPos (Utils.loadEXList);											//根据服务器上实验数据包个数得到排版坐标数组
			for(int i = 0; i < Utils.loadEXList.Count; i++) {							//遍历下载实验列表
				if (!isExist ((string[])Utils.loadEXList [i])) {  						//若本地不存在此实验资源包
					initialPrefab ((string[])Utils.loadEXList [i], i, true);			//初始下载实验资源包按钮
				} else {																//若本地存在此实验资源包
					initialPrefab ((string[])Utils.loadEXList [i], i, false);			//初始不能下载实验资源包按钮
				}
			}
			yield return new WaitForSeconds(1.0f);                  					//等待1秒
		}
	}

    //初始化预制件
	public void initialPrefab(string[] str, int i, bool type) {
        GameObject temp = Instantiate(UIPrefab);            										//初始化资源包预制件
        temp.name = str[0];                                 										//更改名称（EX_#）
        temp.transform.FindChild("Text").transform.GetComponent<Text>().text = str[1];				//更改资源包名称（顽皮的指针）

		float r = float.Parse (((string[])rgbList[i]) [1]) / 255.0f;								//获取TXT r值
		float g = float.Parse (((string[])rgbList[i]) [2]) / 255.0f;								//获取TXT g值
		float b = float.Parse (((string[])rgbList[i]) [3]) / 255.0f;								//获取TXTr b值
		temp.transform.FindChild("Text").transform.GetComponent<Text>().color = 
			new Color(r, g, b, 1.0f);																//更改字体颜色

		temp.transform.FindChild("Image").transform.GetComponent<Image>().sprite = 
			Utils.LoadImage(ImagePath  + "/Images/" + temp.name + ".png");							//更改图片精灵
		temp.transform.localScale = new Vector3(0.9f, 1.1f, 1.0f);									//设置缩放比
		temp.transform.SetParent(Canvas.transform.FindChild("ButtonGroup"));  	        			//设为ButtonGroup的子对象
		temp.transform.GetComponent<RectTransform>().anchoredPosition = buttonsPosition[i];			//设置坐标位置
		if(!type) {																					//如果已经是已经下载的实验数据包
			temp.GetComponent<Image> ().color = new Color (0.67f, 0.55f, 0.55f, 1.0f);				//Image颜色变灰
			temp.transform.FindChild ("Image").GetComponent<Image>().color = 
				new Color (0.67f, 0.55f, 0.55f, 1.0f);												//子物体Image颜色变灰
			temp.transform.FindChild ("Text").GetComponent<Text> ().color = 
				new Color (0.56f, 0.49f, 0.49f, 1.0f);												//子物体Text颜色变灰
			temp.transform.FindChild ("Over").gameObject.SetActive (true);							//打开‘已下载’提示字样
			temp.GetComponent<Button> ().enabled = false;											//删除点击事件
		}
    }

    //判断本地是否存在该实验资源包
    public bool isExist(string[] str) {
        for(int i = 0; i < Utils.localEXList.Count; i++) {  				//遍历本地实验包
			if (str[0].Equals(((string[])(Utils.localEXList[i]))[0])) {    	//本地已经存在该实验包
				return true;												//返回已经存在
            }
        }
        return false;														//返回未下载
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

	//为数组赋值
	public void InitButtonPos (ArrayList al){
		switch(al.Count) {
		case 1:
			buttonsPosition = new Vector2[1];
			buttonsPosition [0] = new Vector2 (0, 0);
			break;
		case 2:
			buttonsPosition = new Vector2[2];
			buttonsPosition [0] = new Vector2 (-295.0f, 0);
			buttonsPosition [1] = new Vector2 (295.0f, 0);
			break;
		case 3:
			buttonsPosition = new Vector2[3];
			buttonsPosition [0] = new Vector2 (-600.0f, 0.0f);
			buttonsPosition [1] = new Vector2 (0.0f, 0.0f);
			buttonsPosition [2] = new Vector2 (600.0f, 0.0f);
			break;
		case 4:
			buttonsPosition = new Vector2[4];
			buttonsPosition [0] = new Vector2 (-295.0f, 200.0f);
			buttonsPosition [1] = new Vector2 (295.0f, 200.0f);
			buttonsPosition [2] = new Vector2 (-295.0f, -200.0f);
			buttonsPosition [3] = new Vector2 (295.0f, -200.0f);
			break;
		case 5:
			buttonsPosition = new Vector2[5];
			buttonsPosition [0] = new Vector2 (-600.0f, 200.0f);
			buttonsPosition [1] = new Vector2 (0.0f, 200.0f);
			buttonsPosition [2] = new Vector2 (600.0f, 200.0f);
			buttonsPosition [3] = new Vector2 (-295.0f, -200.0f);
			buttonsPosition [4] = new Vector2 (295.0f, -200.0f);
			break;
		case 6:
			buttonsPosition = new Vector2[6];
			buttonsPosition [0] = new Vector2 (-600.0f, 200.0f);
			buttonsPosition [1] = new Vector2 (0.0f, 200.0f);
			buttonsPosition [2] = new Vector2 (600.0f, 200.0f);
			buttonsPosition [3] = new Vector2 (-600.0f, -200.0f);
			buttonsPosition [4] = new Vector2 (0.0f, -200.0f);
			buttonsPosition [5] = new Vector2 (600.0f, -200.0f);
			break;
		}
	}

}
