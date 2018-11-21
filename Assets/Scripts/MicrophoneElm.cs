using UnityEngine;
using System.Collections;

//话筒
public class MicrophoneElm : CircuitElm {
    
    public float resistance;                                            //电阻值

    void Start() {
        type = CirSim.TYPES.MicrophoneElm;                              //声明类型为话筒
        downHole = 2;													//声明下方孔数
        for (int i = 0; i < 4; i++) {
            interfaceList.Add(i, null);									//为每个纽扣接口赋初值
        }
        this.transform.position = originalPos;                          //还原初始位置
        floor = 0;                                                      //设置层数
        state = 0;                                                      //元器件起始状态
        resistance = 10;                                                //初始电阻为10欧姆
        points = new string[2];                                         //初始端点数组
        nodes = new int[getPostCount()];                                //创建结点数组
        volts = new float[getPostCount()];                              //创建电压数组
        voltSource = new int[getVoltageSourceCount()];                  //创建电压源数组
    }

    //器件开始工作
    public override void work() {

        Debug.Log("话筒： "+volts[0] + " "+ volts[1] + " " +current);
        if(volts[1] - volts[0] >0.0555-0.01 && volts[1] - volts[0] < 0.0555 + 0.01 && current > 0) {      //接收到了录音机的输出电压
            Debug.Log("话筒接收到了录音机的电压");
            Utils.MyMicphone.StartRecord();         //开始录音
        }
    }

    //器件停止工作
    public override void stop() {
       
    }

    // MaterialArray 0-黑线材质 1-透明材质 2-外部灰色材质 3-红字材质 4-银色材质 
    // OBJ 0 黑色材质 1 透明玻璃 2-3 灰色外框 4-7 红色文字 8-12 银色纽扣

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
            else if (i >= 2 && i <= 3) {
                obj[i].GetComponent<Renderer>().material = temp[2];
            }
            else if (i >= 4 && i <= 7) {
                obj[i].GetComponent<Renderer>().material = temp[3];
            }
            else if (i >= 8 && i <= 12) {
                obj[i].GetComponent<Renderer>().material = temp[4];
            }
        }
    }

    //计算电流值
    public override void calculateCurrent() {
        current = (volts[1] - volts[0]) / resistance;
    }

    //标记元器件
    public override void stamp() {
        CirSim.stampResistor(nodes[0], nodes[1], resistance);
    }

}
