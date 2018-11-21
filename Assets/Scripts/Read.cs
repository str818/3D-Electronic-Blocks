using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System.IO;
using System.Collections;

//读取数据文件进行文字介绍
public class Read : MonoBehaviour {

	public float intervalTime;									//文字间隔时间
	private float Timer;										//计时器
	private int count;											//计数器
	private string content;										//存储文本内容的字符串
	private char[] c;											//定义char数组

	void Start () {
		Timer = 0.0f;											//初始化计时器
		count = 0;												//初始化计数器
		intervalTime = 0.08f;									//初始化文字间隔时间
	}

	//读取元器件文字介绍
	public void read(int code) {
		switch(code) {															//分别做动态加载
		case 1:                                                                 //1-导线
			content = Utils.LoadFile("WireElm_1.txt");                          //获取相关文本文字
			break;
		case 2:                                                                 //2-导线
			content = Utils.LoadFile("WireElm_2.txt");                          //获取相关文本文字
			break;
		case 3:                                                                 //3-导线
			content = Utils.LoadFile("WireElm_3.txt");                          //获取相关文本文字
			break;
		case 15:																//15-开关
			content = Utils.LoadFile("SwitchElm.txt");                          //获取相关文本文字
			break;
		case 16:																//16-光敏电阻
			content = Utils.LoadFile("PhotosensitiveResistorElm.txt");          //获取相关文本文字
			break;
		case 17:                                                                //17-二极管
			content = Utils.LoadFile("DiodeElm.txt");                          	//获取相关文本文字
			break;
		case 18:																//18-小灯泡
			content = Utils.LoadFile("BulbElm.txt");                          	//获取相关文本文字
			break;
		case 19:																//19-电池
			content = Utils.LoadFile("VoltageElm.txt");                         //获取相关文本文字
			break;
		case 20:                                                                //20-喇叭
			content = Utils.LoadFile("HornElm.txt");                          	//获取相关文本文字
			break;
		case 21:                                                                //21-音乐集成电路
			content = Utils.LoadFile("MusicChipElm.txt");                      	//获取相关文本文字
			break;
		case 24:																//24-风扇
			content = Utils.LoadFile("EngineElm.txt");                          //获取相关文本文字
			break;
		case 28:																//28-话筒
			content = Utils.LoadFile("MicrophoneElm.txt");                      //获取相关文本文字
			break;
		case 30:                                                                //30-电阻
			content = Utils.LoadFile("ResistanceElm.txt");                      //获取相关文本文字
			break;
		case 53:																//53-滑动变阻器
			content = Utils.LoadFile("SlidingResistanceElm.txt");               //获取相关文本文字
			break;
		case 56:                                                                //56-电流表
			content = Utils.LoadFile("AmmeterElm.txt");                         //获取相关文本文字
			break;
		case 58:                                                              	//58-数码管
			content = Utils.LoadFile("CounterElm.txt");                        	//获取相关文本文字
			break;
		case 62:                                                             	//62-录音IC
			content = Utils.LoadFile("RecorderChipElm.txt");                    //获取相关文本文字
			break;
        }
		count = 0;												//计数器归0
		c = content.ToCharArray ();								//存储在char数组中
	}

	//读取实验介绍
	public void readex() {
		if (Utils.ExperimentIndex < Utils.BuiltInCounts) {						//如果当前为内置实验
			content = Utils.LoadFile (Utils.buttonName + ".txt");    			//获取相关文本文字
		} else {
			ArrayList li = new ArrayList ();									//定义ArrayList
			int k = 0;															//定义局部变量k
			Utils.loadText(Application.persistentDataPath + "/EX/" + 
				Utils.buttonName + "/" + Utils.buttonName + ".txt", li);    	//获取相关文本文字
			for(int i = 0; i < li.Count; i++) {									//遍历ArrayList
				if(k != 1) {
					content += li [i];
				}
				if(k < 2) {
					content += "\n";
					k++;
				}

			}
		}
		count = 0;												//计数器归0
		c = content.ToCharArray ();								//存储在char数组中
	}

	void Update () {
		if (c.Length == 1) {									//如果c的长度为1
			return;												//直接返回
		}
		Timer += Time.deltaTime;								//开始计时
		if(Timer > intervalTime && count < c.Length + 1) {		//如果计时器大于设定的文字间隔时间
			string ss = "";										//定义临时string变量
			for (int i = 0; i < count; i++) {
				ss += c [i];									//更新文本
			}
			transform.GetComponent<Text> ().text = ss;			//设置text显示文字
			count++;											//计数器加一
			Timer = 0.0f;										//计数器归零
		}
	}

}
