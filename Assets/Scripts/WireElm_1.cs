using UnityEngine;
using System.Collections;

public class WireElm_1 : CircuitElm {

	void Start() {
		type = CirSim.TYPES.WireElm;										//声明类型为导线
		downHole = 1;														//声明下方孔数
		for (int i = 0; i < 2; i++) {										//0-输入端，其他均为输出端
			interfaceList.Add(i, null);										//为每个纽扣接口赋初值
		}
		floor = 0;															//高度
		state = 0;                                                          //元器件起始状态
		points = new string[1];                                             //初始端点数组
		nodes = new int[getPostCount()];                                    //创建结点数组
		volts = new float[getPostCount()];                                  //创建电压数组
		voltSource = new int[getVoltageSourceCount()];                      //创建电压源数组
		transform.position = originalPos;                                   //还原初始位置
	}

	//返回端口点
	public override string getPost(int n) {
		return points[n];
	}

	//获取端点数量
	public override int getPostCount() {
		return 1;
	}
    
    //设置端口点
    public override void setPosts() {
        if (floor == 1) {
            points[0] = ((GameObject)buttonColliders[0]).name;
        }
        else {
            CircuitElm ce = ((GameObject)buttonColliders[0]).transform.parent.GetComponent<CircuitElm>();   //获取元器件
            int index = int.Parse(((GameObject)buttonColliders[0]).name);                                   //被碰撞元器件连接纽扣
            points[0] = ce.getPostFromIndex(index);                                                         //获取结点名称
        }
    }

    // MaterialArray 0-蓝色材质 1-银色材质
    // OBJ 0-外框 1-纽扣

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
		}
	}

	//判断是否是导线
	public override bool isWire() {
		return true;
	}

}
