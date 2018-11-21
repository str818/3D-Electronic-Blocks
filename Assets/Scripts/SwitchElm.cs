using UnityEngine;
using System.Collections;

//开关逻辑代码
public class SwitchElm : CircuitElm {

    public bool isOpen;//是否打开开关
    public GameObject derail;//开关引用
    private float onPosX;//开关打开时的x轴位置
    private float offPosX;//开关关闭时的x轴位置

    void Start() {

        type = CirSim.TYPES.SwitchElm;                                  //声明类型
		downHole = 2;													//声明下方孔数
        //switchOff();//默认开关为关闭状态
        onPosX = 0.0078f;//开关打开时位置
        offPosX = 0.0463f;//开关关闭时位置
        for (int i = 0; i < 4; i++) {
            interfaceList.Add(i, null);//为每个纽扣接口赋初值
        }
        floor = 0;
        //originalPos = new Vector3(3.409f, 0f, 0.39f);                     //为初始位置赋值
        //originalRot = new Vector3(-90f, 180f, 0f);                        //位初始旋转角度赋值
        this.transform.position = originalPos;                              //还原初始位置
        state = 0;                                                          //元器件起始状态
        points = new string[2];                                             //初始端点数组
        nodes = new int[getPostCount()];                                    //创建结点数组
        volts = new float[getPostCount()];                                  //创建电压数组
        voltSource = new int[getVoltageSourceCount()];                      //创建电压源数组
    }

    //打开开关
    public void switchOn() {
        isOpen = true;
        derail.transform.localPosition = new Vector3(onPosX, derail.transform.localPosition.y, derail.transform.localPosition.z);
    }

    //关闭开关
    public void switchOff() {
        isOpen = false;
        derail.transform.localPosition = new Vector3(offPosX, derail.transform.localPosition.y, derail.transform.localPosition.z);
    }

    //转换开关状态
    public void convert() {
        if (isOpen) switchOff();
        else switchOn();
        voltSource = new int[getVoltageSourceCount()];                  //创建电压源数组
    }

    // MaterialArray 0-黑色材质 1-棕色材质 2-白色材质 3-银色材质
    // OBJ 0 黑色外框 1-2 棕色 3-11 白色文字 12-23 银边

    //元器件透明与不透明的相互转化 direction -true 向透明转换 -false 向不透明转换
    public override void convertState(bool direction) {
        //判断更改材质数组
        Material[] temp = direction ? transparentMaterial : opaqueMaterial;
        for (int i = 0; i < obj.Length; i++) {
            if (i == 0) {
                obj[i].GetComponent<Renderer>().material = temp[0];
            }
            else if (i >= 1 && i <= 2) {
                obj[i].GetComponent<Renderer>().material = temp[1];
            }
            else if (i >= 3 && i <= 11) {
                obj[i].GetComponent<Renderer>().material = temp[2];
            }
            else if (i >= 12 && i <= 23) {
                obj[i].GetComponent<Renderer>().material = temp[3];
            }
        }
    }

    //获取电压源数量
    public override int getVoltageSourceCount() {
        return isOpen ? 1 : 0;
    }

    //计算电流值
    public override void calculateCurrent() {
        if (!isOpen) {  
            current = 0;    //若没有打开开关，电流为0
        }
    }

    //标记元器件
    public override void stamp() {
        if (isOpen) {
            CirSim.stampVoltageSource(nodes[0], nodes[1], voltSource[0], 0);
        }
    }

    //判断两端口是否相连
    public override bool getConnection(int n1, int n2) {
        return isOpen;
    }

    //判断是否是导线
    public override bool isWire() {
        return true;
    }
}
