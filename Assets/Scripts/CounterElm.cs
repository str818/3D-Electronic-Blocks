using UnityEngine;
using System.Collections;

//数码管 D-0 DP-1 E-2 C-3 F-4 G-5 A-6 B-7 +-8
public class CounterElm : CircuitElm {
	
	public float resistance;                                            //每个电阻的阻值,一共8端电阻
	public float[] currentsArray;                                       //电流数组
	public GameObject[] lampArray;                                      //发光二极管数组
	public Material lampOn;                                             //二极管发光材质
	public Material lampOff;                                            //二极管熄灭材质

	void Start() {
		type = CirSim.TYPES.CounterElm;                                 //声明类型为数码管
		downHole = 3;													//声明下方孔数
		for (int i = 0; i < 18; i++) {
			interfaceList.Add(i, null);									//为每个纽扣接口赋初值
		}
		this.transform.position = originalPos;                          //还原初始位置
		floor = 0;                                                      //设置层数
		state = 0;                                                      //元器件起始状态
		resistance = 100;                                               //初始每段电阻为100欧姆
		points = new string[9];                                         //初始端点数组
		nodes = new int[getPostCount()];                                //创建结点数组
		volts = new float[getPostCount()];                              //创建电压数组
		voltSource = new int[getVoltageSourceCount()];                  //创建电压源数组
		currentsArray = new float[8];                                   //创建电流数组
	}

	//设置端口点
	public override void setPosts() {
		base.setPosts();
		int pointIndex0 = int.Parse(points[0].Substring(6, 2));        //获取端点0的序号
		int pointIndex1 = int.Parse(points[1].Substring(6, 2));        //获取端点1的序号
		int pointIndex8 = int.Parse(points[8].Substring(6, 2));        //获取端点3的序号
		float rot = Utils.plate.transform.rotation.eulerAngles.y;      //底板旋转方向
		if (rot == 0) {                                                //未旋转
			points[2] = "Point0" + (pointIndex0 + 1);
			points[4] = "Point0" + (pointIndex0 + 2);
			points[6] = "Point0" + (pointIndex0 + 3);
			points[3] = "Point0" + (pointIndex1 + 1);
			points[5] = "Point0" + (pointIndex1 + 2);
			points[7] = "Point0" + (pointIndex1 + 3);
		}
		else if (rot == 90) {                                          //旋转90度
			points[2] = "Point0" + (pointIndex0 + 7);
			points[4] = "Point0" + (pointIndex0 + 14);
			points[6] = "Point0" + (pointIndex0 + 21);
			points[3] = "Point0" + (pointIndex1 + 7);
			points[5] = "Point0" + (pointIndex1 + 14);
			points[7] = "Point0" + (pointIndex1 + 21);
		}
		else if (rot == 180) {                                         //旋转180度
			points[2] = "Point0" + (pointIndex0 - 1);
			points[4] = "Point0" + (pointIndex0 - 2);
			points[6] = "Point0" + (pointIndex0 - 3);
			points[3] = "Point0" + (pointIndex1 - 1);
			points[5] = "Point0" + (pointIndex1 - 2);
			points[7] = "Point0" + (pointIndex1 - 3);
		}
		else if (rot == 270) {                                         //旋转270度
			points[2] = "Point0" + (pointIndex0 - 7);
			points[4] = "Point0" + (pointIndex0 - 14);
			points[6] = "Point0" + (pointIndex0 - 21);
			points[3] = "Point0" + (pointIndex1 - 7);
			points[5] = "Point0" + (pointIndex1 - 14);
			points[7] = "Point0" + (pointIndex1 - 21);
		}
	}

	//器件开始工作——灯泡亮
	public override void work() {
		for (int i = 0; i < getPostCount() - 1; i++) {
			if (currentsArray[i] > 0) {                                 //若电流为正方向
				enLightOn(i);                                           //使第i个二极管发光
			}else {                                                     //若电流为0或为反方向
				enLightOff(i);                                          //使二极管熄灭
			}
		}
	}

	//器件停止工作——灭灯泡
	public override void stop() {
		for(int i = 0; i < getPostCount() - 1; i++) {
			enLightOff(i);                                              //熄灭所有的二极管
		}
	}

	//使满足条件的二极管发光 第n个二极管发光
	public void enLightOn(int n) {
		Debug.Log (n);
		lampArray[n].GetComponent<Renderer>().material = lampOn;
	}

	//使二极管熄灭 第n个二极管熄灭
	public void enLightOff(int n) {
		lampArray[n].GetComponent<Renderer>().material = lampOff;
	}

	// MaterialArray 0-黑线材质 1-灯泡透明材质 2-外部蓝色材质 3-红字材质 4-银色材质 5-不锈钢材质
	// OBJ 0-3 黑线 4-5 透明玻璃材质 6 外部蓝色材质 7-14 红字材质 15-20 银色材质 21-25 不锈钢材质

	//元器件透明与不透明的相互转化 direction -true 向透明转换 -false 向不透明转换
	public override void convertState(bool direction) {

		//判断更改材质数组
		Material[] temp = direction ? transparentMaterial : opaqueMaterial;
		for (int i = 0; i < obj.Length; i++) {
			if (i == 0) {
				obj[i].GetComponent<Renderer>().material = temp[0];
			}
			else if (i == 1) {
				obj[i].GetComponent<Renderer>().material = temp[1];
			}
			else if (i >= 2 && i <= 27) {
				obj[i].GetComponent<Renderer>().material = temp[2];
			}
			else if (i >= 28 && i <= 40) {
				obj[i].GetComponent<Renderer>().material = temp[3];
			}
			else if (i >= 41 && i <= 49) {
				obj[i].GetComponent<Renderer>().material = temp[4];
			}
		}
	}

	//计算电流值
	public override void calculateCurrent() {
		for(int i = 0; i < getPostCount() - 1; i++) {
			currentsArray[i] = (volts[8] - volts[i]) / resistance;  //计算每段支路的电流
		}
	}

	//标记元器件
	public override void stamp() {
		for(int i = 0; i < getPostCount()-1; i++) {                 
			CirSim.stampResistor(nodes[i], nodes[8], resistance);   //每个端点都与正极之间有一个电阻
		}
	}

	//获取端点数量
	public override int getPostCount() {
		return 9;
	}

}
