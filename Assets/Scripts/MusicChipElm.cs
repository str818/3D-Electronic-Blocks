using UnityEngine;
using System.Collections;

//音乐集成电路
//0-负极 1-输出端 2-空引脚 3-正极 4-触发
public class MusicChipElm : CircuitElm {

	public float outCurrent;                                            //输出端电流
	public int resistance;                                              //正负极之间的电阻
	private bool isRun;                                                 //输出标志位

	void Start() {
		type = CirSim.TYPES.MusicChipElm;                               //声明类型为灯泡
		downHole = 3;													//声明下方孔数
		for (int i = 0; i < 10; i++) {
			interfaceList.Add(i, null);									//为每个纽扣接口赋初值
		}
		floor = 0;                                                      //设置层数
		state = 0;                                                      //元器件起始状态
		points = new string[5];                                         //初始端点数组
		nodes = new int[getPostCount()];                                //创建结点数组
		volts = new float[getPostCount()];                              //创建电压数组
		voltSource = new int[getVoltageSourceCount()];                  //创建电压源数组
		resistance = 10;                                                //电阻初始化
		transform.position = originalPos;                               //还原初始位置
	}

	//返回端口点
	public override string getPost(int n) {
		return points[n];
	}

	//工作
	public override void work() {

	}

	//设置端口点
	public override void setPosts() {
		base.setPosts();
		int pointIndex0 = int.Parse(points[0].Substring(6, 2));        //获取端点0的序号
		int pointIndex1 = int.Parse(points[1].Substring(6, 2));        //获取端点1的序号
		int pointIndex3 = int.Parse(points[3].Substring(6, 2));        //获取端点3的序号
		float rot = Utils.plate.transform.rotation.eulerAngles.y;      //底板旋转方向
		if (rot == 0) {                                                //未旋转
			points[2] = "Point0" + (pointIndex0 + 1);
			points[4] = "Point0" + (pointIndex1 + 1);
		}
		else if (rot == 90) {
			points[2] = "Point0" + (pointIndex3 - 1);
			points[4] = "Point0" + (pointIndex3 + 1);
		}
		else if (rot == 180) {
			points[2] = "Point0" + (pointIndex0 - 1);
			points[4] = "Point0" + (pointIndex1 - 1);
		}
		else if (rot == 270) {
			points[2] = "Point0" + (pointIndex3 + 1);
			points[4] = "Point0" + (pointIndex3 - 1);
		}
	}

	//获取端点数量
	public override int getPostCount() {
		return 5;
	}

	//判断两端口是否相连
	public override bool getConnection(int n1, int n2) {
		return false;
	}

	//标记元器件
	public override void stamp() {
		if (isRun) {
			CirSim.stampVoltageSource(0, nodes[1], voltSource[0], 999);     //输出电压为999
			isRun = false;                                                  //运行标志位置false
		}else {
			CirSim.stampVoltageSource(0, nodes[1], voltSource[0], 0);       //输出电压为0
		}

		CirSim.stampResistor(nodes[0], nodes[3], 10);                   //0 3结点间为10欧姆的电阻
	}

	//设置电流值
	public override void setCurrent(int x, float c) {
		outCurrent = c;
	}

	//计算电流值
	public override void calculateCurrent() {
		current = (volts[3] - volts[0]) / resistance;
		Debug.Log("计算集成电路电流为："+current);
	}

	//进行下一步
	public override void doStep() {

		Debug.Log("进入下一步 " + current + " " + volts[4]);
		if (current > 0) {
			Debug.Log("电流大于0");
		}

		if (volts[4] > 0) {
			Debug.Log("电压大于0");
		}

		if (current > 0 && volts[4] > 0) {       //集成电路正负极连接并且触发端有电压
			Debug.Log("集成电路准备完毕");
			isRun = true;
			//CirSim.stampVoltageSource(0, nodes[1], voltSource[0], 10);       //3 1结点间为导线
			//CirSim.updateVoltageSource(0, nodes[1], voltSource[0], 10);
		}
	}

	// MaterialArray 0-白色文字 1-蓝色外观 2-银色材质
	// OBJ 0-4 白色 5-6 蓝色外观 7-10 银色材质

	//元器件透明与不透明的相互转化 direction -true 向透明转换 -false 向不透明转换
	public override void convertState(bool direction) {

		//判断更改材质数组
		Material[] temp = direction ? transparentMaterial : opaqueMaterial;
		for (int i = 0; i < obj.Length; i++) {
			if (i >= 0 && i <= 3) {
				obj[i].GetComponent<Renderer>().material = temp[0];
			}
			else if (i >= 4 && i <= 5) {
				obj[i].GetComponent<Renderer>().material = temp[1];
			}
			else if (i >= 6 && i <= 10) {
				obj[i].GetComponent<Renderer>().material = temp[2];
			}
		}
	}

	//获取电压源数量
	public override int getVoltageSourceCount() {
		return 1;
	}

	public override bool hasGroundConnection(int n) {
		return n == 1; 
	}
}
