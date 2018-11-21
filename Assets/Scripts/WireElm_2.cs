using UnityEngine;
using System.Collections;

//两个端点的导线
public class WireElm_2 : CircuitElm {

    public int index;                                                       //导线的孔数

    void Start() {
        type = CirSim.TYPES.WireElm;										//声明类型为导线
        index = 2;
        downHole = 2;														//声明下方孔数
        for (int i = 0; i < index + 2; i++) {								//0-输入端，其他均为输出端
            interfaceList.Add(i, null);										//为每个纽扣接口赋初值
        }
        floor = 0;
        //originalPos = new Vector3(3.4f, 0f, -0.368f);                     //为初始位置赋值
        //originalRot = new Vector3(-90f, 0f, 0f);                          //为初始旋转角赋值
        this.transform.position = originalPos;                              //还原初始位置
        state = 0;                                                          //元器件起始状态
        points = new string[2];                                             //初始端点数组
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
        return 2;
    }

    // MaterialArray 0-蓝色材质 1-白色材质 2-银色材质
    // OBJ 0-外框 1-6 白色文字 7-8 银色按钮

    //元器件透明与不透明的相互转化 direction -true 向透明转换 -false 向不透明转换
    public override void convertState(bool direction) {
        //判断更改材质数组
        Material[] temp = direction ? transparentMaterial : opaqueMaterial;
        for (int i = 0; i < obj.Length; i++) {
            if (i == 0) {
                obj[i].GetComponent<Renderer>().material = temp[0];
            }
            else if (i >= 1 && i <= 6) {
                obj[i].GetComponent<Renderer>().material = temp[1];
            }
            else if (i >= 7 && i <= 8) {
                obj[i].GetComponent<Renderer>().material = temp[2];
            }
        }
    }

    //获取电压源数量
    public override int getVoltageSourceCount() {
        return 1;
    }

    //标记元器件
    public override void stamp() {
        CirSim.stampVoltageSource(nodes[0], nodes[1], voltSource[0], 0);
    }

    //判断是否是导线
    public override bool isWire() {
        return true;
    }

}
