using UnityEngine;
using System.Collections;

public class PhotosensitiveResistorElm : CircuitElm {

	public float resistance;                                            //电阻值

	void Start() {
		type = CirSim.TYPES.PhotosensitiveResistorElm;                	//声明类型为光敏电阻
		downHole = 2;													//声明下方孔数
		for (int i = 0; i < 4; i++) {
			interfaceList.Add(i, null);									//为每个纽扣接口赋初值
		}
		floor = 0;                                                      //设置层数
		state = 0;                                                      //元器件起始状态
		resistance = 300;                                               //初始电阻为10欧姆
		points = new string[2];                                         //初始端点数组
		nodes = new int[getPostCount()];                                //创建结点数组
		volts = new float[getPostCount()];                              //创建电压数组
		voltSource = new int[getVoltageSourceCount()];                  //创建电压源数组
		transform.position = originalPos;                      	        //还原初始位置
	}

	//计算电流值
	public override void calculateCurrent() {
		current = (volts[0] - volts[1]) / resistance;
	}

	//标记元器件
	public override void stamp() {
		CirSim.stampResistor(nodes[0], nodes[1], resistance);
	}

	// MaterialArray 0-黄色材质 1-红色文字 2-银色材质
	// OBJ 0 黄色外壳 1-3 红色文字  4-9 银色

	//元器件透明与不透明的相互转化 direction -true 向透明转换 -false 向不透明转换
	public override void convertState(bool direction) {
		//判断更改材质数组
		Material[] temp = direction ? transparentMaterial : opaqueMaterial;
		for (int i = 0; i < obj.Length; i++) {
			if (i == 0) {
				obj [i].GetComponent<Renderer> ().material = temp [0];
			} else if (i == 1) {
				obj [i].GetComponent<Renderer> ().material = temp [1];
			} else if (i >= 2 && i <= 6) {
				obj [i].GetComponent<Renderer> ().material = temp [2];
			} else if (i == 7) {
				obj [i].GetComponent<Renderer> ().material = temp [3];
			} else if (i >= 8 && i <= 9) {
				obj [i].GetComponent<Renderer> ().material = temp [4];
			}
		}
	}

}
