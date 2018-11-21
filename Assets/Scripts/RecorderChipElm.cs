using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//录音机集成电路 0-播放录音触发 1-负极 2-开始录音触发 3-连接话筒的负极 4-输出 5-连接话筒的正极 6-正极
public class RecorderChipElm : CircuitElm {

    public float outCurrent;                                            //输出端电流
    public int resistance;                                              //正负极之间的电阻
    public int ICState;                                                 //录音机的状态 -1 录音 0 未工作 1 播放录音
    public float[] currentArray;                                        //录音机内的电阻电流 0 （6-0） 1 （6-1） 2 （6-2）

    void Start() {
        type = CirSim.TYPES.RecorderChipElm;                            //声明类型为录音机
        downHole = 4;                                                   //声明下方孔数
        for (int i = 0; i < 14; i++) {
            interfaceList.Add(i, null);                                 //为每个纽扣接口赋初值
        }
        floor = 0;                                                      //设置层数
        state = 0;                                                      //元器件起始状态
        points = new string[7];                                         //初始端点数组
        nodes = new int[getPostCount()];                                //创建结点数组
        volts = new float[getPostCount()];                              //创建电压数组
        voltSource = new int[getVoltageSourceCount()];                  //创建电压源数组
        resistance = 5;                                                 //电阻初始化
        transform.position = originalPos;                               //还原初始位置
        currentArray = new float[3];                                    //初始化电路数组
        ICState = 0;                                                    //录音机初始状态为未工作
    }

    //返回端口点
    public override string getPost(int n) {
        return points[n];
    }

    //工作
    public override void work() {
        if(ICState == 0) {
            Debug.Log("录音机Work ICState停止录音");
            Utils.MyMicphone.QuitAudio();                               //停止播放声音
        }
        
    }

    //设置端口点
    public override void setPosts() {
        base.setPosts();
        int pointIndex0 = int.Parse(points[0].Substring(6, 2));        //获取端点0的序号
        int pointIndex2 = int.Parse(points[2].Substring(6, 2));        //获取端点2的序号
        float rot = Utils.plate.transform.rotation.eulerAngles.y;      //底板旋转方向
        if (rot == 0) {                                                //未旋转
            points[1] = "Point0" + (pointIndex0 - 7);
            points[3] = "Point0" + (pointIndex0 + 1);
            points[5] = "Point0" + (pointIndex0 + 2);
            points[4] = "Point0" + (pointIndex2 + 1);
            points[6] = "Point0" + (pointIndex2 + 2);
        }
        else if (rot == 90) {
            points[1] = "Point0" + (pointIndex0 + 1);
            points[3] = "Point0" + (pointIndex0 + 7);
            points[5] = "Point0" + (pointIndex0 + 14);
            points[4] = "Point0" + (pointIndex2 + 7);
            points[6] = "Point0" + (pointIndex2 + 14);
        }
        else if (rot == 180) {
            points[1] = "Point0" + (pointIndex0 + 7);
            points[3] = "Point0" + (pointIndex0 - 1);
            points[5] = "Point0" + (pointIndex0 - 2);
            points[4] = "Point0" + (pointIndex2 - 1);
            points[6] = "Point0" + (pointIndex2 - 2);
        }
        else if (rot == 270) {
            points[1] = "Point0" + (pointIndex0 - 1);
            points[3] = "Point0" + (pointIndex0 - 7);
            points[5] = "Point0" + (pointIndex0 - 14);
            points[4] = "Point0" + (pointIndex2 - 7);
            points[6] = "Point0" + (pointIndex2 - 14);
        }
    }

    //获取端点数量
    public override int getPostCount() {
        return 7;
    }

    //判断两端口是否相连
    public override bool getConnection(int n1, int n2) {
        return false;
    }

    //标记元器件
    public override void stamp() {

        if(ICState == -1) {                                                     //开始录音，在连接话筒的两个端口施加电压
            CirSim.stampVoltageSource(nodes[3], nodes[5], voltSource[0], 0.0555f); //标记两个连接话筒的端点 0.0555位话筒接收的电压值
            Debug.Log("ICState状态为-1");
        }

        if(ICState == 1) {                                                      //播放录音，在输出端口施加电压
            CirSim.stampVoltageSource(0, nodes[4], voltSource[0], 998f);        //输出电压为998
            Debug.Log("ICState状态为1");
        }

        Debug.Log("ICState状态为0");
        //标记电阻
        CirSim.stampResistor(nodes[6], nodes[0], resistance);          //6 0结点间为5欧姆的电阻
        CirSim.stampResistor(nodes[6], nodes[1], resistance);          //6 1结点间为5欧姆的电阻
        CirSim.stampResistor(nodes[6], nodes[2], resistance);          //6 2结点间为5欧姆的电阻
    }

    //设置电流值
    public override void setCurrent(int x, float c) {
        outCurrent = c;
    }

    //计算电流值
    public override void calculateCurrent() {
        for(int i = 0; i < 3; i++) {
            currentArray[i] = (volts[6] - volts[i]) / resistance;       //为三条支路计算电流
        }
    }

    //进行下一步
    public override void doStep() {
        //当正负极电流为正方向、并且触发录音
        if (currentArray[1] > 0.01 && currentArray[2] > 0.01 && currentArray[0] < 0.01) {
            Debug.Log("触发录音");
            ICState = -1;
            voltSource = new int[getVoltageSourceCount()];                  //创建电压源数组
        }
        else if(currentArray[1] > 0.01 && currentArray[2] < 0.01 && currentArray[0] > 0.01) {//触发播放录音
            Debug.Log("播放录音");
            ICState = 1;
            voltSource = new int[getVoltageSourceCount()];                  //创建电压源数组
        }
        else {
            Debug.Log("录音机未工作");
            ICState = 0;                                                //录音机未工作
            Utils.MyMicphone.StopRecord();                              //停止记录录音
            Debug.Log("停止记录录音");    
        }
        Debug.Log("doStep下一步测试输出： "+currentArray[0]+"  "+ currentArray[1] + "  "+ currentArray[2]);
    } 

    // MaterialArray 0-灰色材质 1-红色材质 2-白色材质 3-银色材质
    // OBJ 0-1 灰色 2-3 红色 4-5 白色 6-12 银色纽扣

    //元器件透明与不透明的相互转化 direction -true 向透明转换 -false 向不透明转换
    public override void convertState(bool direction) {
        //判断更改材质数组
        Material[] temp = direction ? transparentMaterial : opaqueMaterial;
        for (int i = 0; i < obj.Length; i++) {
            if (i >= 0 && i <= 1) {
                obj[i].GetComponent<Renderer>().material = temp[0];
            }
            else if (i >= 2 && i <= 3) {
                obj[i].GetComponent<Renderer>().material = temp[1];
            }
            else if (i >= 4 && i <= 5) {
                obj[i].GetComponent<Renderer>().material = temp[2];
            }
            else if (i >= 6 && i <= 12) {
                obj[i].GetComponent<Renderer>().material = temp[3];
            }
        }
    }

    //获取电压源数量
    public override int getVoltageSourceCount() {
        if(ICState != 0) {
            return 1;
        }
        return 0;
    }

    //输出端接地
    public override bool hasGroundConnection(int n) {
        if(ICState == 1) {
            return n == 4;
        }
        return false;
    }

}
