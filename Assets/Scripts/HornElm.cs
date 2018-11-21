using UnityEngine;
using System.Collections;

//喇叭
public class HornElm : CircuitElm {

    public float resistance;                                            //电阻值
    void Start() {
        type = CirSim.TYPES.HornElm;                                    //声明类型为喇叭
        downHole = 2;													//声明下方孔数
        for (int i = 0; i < 4; i++) {
            interfaceList.Add(i, null);									//为每个纽扣接口赋初值
        }
        floor = 0;                                                      //设置层数
        state = 0;                                                      //元器件起始状态
        resistance = 10;                                                //初始电阻为10欧姆
        points = new string[2];                                         //初始端点数组
        nodes = new int[getPostCount()];                                //创建结点数组
        volts = new float[getPostCount()];                              //创建电压数组
        voltSource = new int[getVoltageSourceCount()];                  //创建电压源数组
		transform.position = originalPos;                               //还原初始位置
    }


    //计算电流值
    public override void calculateCurrent() {
        current = (volts[0] - volts[1]) / resistance;
    }

    //喇叭响
    public override void work() {
        if(current != 0) {
            if(volts[0] == 998 || volts[1] == 998) {                    //若其中一个端点接收到了录音机集成电路的输出信号
                Debug.Log("播放录音Horn");
                Utils.MyMicphone.PlayRecord();                          //播放录音
            }

            if(volts[0] == 999 || volts[1] == 999) {                    //若其中一个端点接收到了音乐集成电路的输出信号
                Utils.audioManager.audioSource.loop = true;             //循环播放音效
                Utils.audioManager.playAudio(4);                        //播放生日快乐音效
            }else {
                Utils.audioManager.audioSource.Stop();                  //停止播放
            }


        }
        else {
            Utils.audioManager.audioSource.loop = false;                //循环播放音效
            Utils.audioManager.audioSource.Stop();                      //停止播放
        }
    }

    //停止工作
    public override void stop() {
        Utils.audioManager.audioSource.loop = false;                    //循环播放音效
        Utils.audioManager.audioSource.Stop();                          //停止播放
    }

    //标记元器件
    public override void stamp() {
        CirSim.stampResistor(nodes[0], nodes[1], resistance);
    }

    // MaterialArray 0-白色材质 1-红色材质 2-棕色材质 3-黑色材质 4-透明材质 5-银色材质
    // OBJ 0-2 白色 3-4 红色外观 5-6 棕色 7-9 黑色 10-11 透明塑料 12-21 银色纽扣
     
    //元器件透明与不透明的相互转化 direction -true 向透明转换 -false 向不透明转换
    public override void convertState(bool direction) {

        //判断更改材质数组
        Material[] temp = direction ? transparentMaterial : opaqueMaterial;
        for (int i = 0; i < obj.Length; i++) {
            if (i >= 0 && i <= 2) {
                obj[i].GetComponent<Renderer>().material = temp[0];
            }
            else if (i >= 3 && i <= 4) {
                obj[i].GetComponent<Renderer>().material = temp[1];
            }
            else if (i >= 5 && i <= 6) {
                obj[i].GetComponent<Renderer>().material = temp[2];
            }
            else if (i >= 7 && i <= 9) {
                obj[i].GetComponent<Renderer>().material = temp[3];
            }
            else if (i >= 10 && i <= 11) {
                obj[i].GetComponent<Renderer>().material = temp[4];
            }
            else if (i >= 12 && i <= 21) {
                obj[i].GetComponent<Renderer>().material = temp[5];
            }
        }
    }
}
