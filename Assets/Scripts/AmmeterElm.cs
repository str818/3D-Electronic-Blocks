using UnityEngine;
using System.Collections;

//电流表逻辑代码（指针的范围是-47 ~ 45） 正极在右侧
public class AmmeterElm : CircuitElm {

    public GameObject pointer;                                          //指针
    public int maxCurrent;                                              //最大电流
    public int speed;                                                   //转动速度
    public Transform camPos;                                           	//UI摄像机观察位置
    private float runCurrent;                                           //运行时的电流
    private float originalZ;                                            //震动位置
    private bool isShaking;                                             //是否正在震动

    void Start() {
		type = CirSim.TYPES.AmmeterElm;                                 //声明类型为电流表
        downHole = 2;													//声明下方孔数
        for (int i = 0; i < 4; i++) {
            interfaceList.Add(i, null);									//为每个纽扣接口赋初值
        }
        floor = 0;                                                      //设置层数
        state = 0;                                                      //元器件起始状态
        points = new string[2];                                         //初始端点数组
        nodes = new int[getPostCount()];                                //创建结点数组
        volts = new float[getPostCount()];                              //创建电压数组
        voltSource = new int[getVoltageSourceCount()];                  //创建电压源数组
        maxCurrent = 1;                                                 //最大电流为1A
        speed = 1;                                                      //设置速度为1
		transform.position = originalPos;                               //还原初始位置
        originalZ = -47;                                                //初始震动位置为-47
        isShaking = true;                                               //指针正在振动
        StartCoroutine(Shake());                                        //开启振动协程
    }

    //电流表转动
    public override void work() {
        if (current > 0) {
            //电流流向相反(烧毁)
        } else {
            current = -current;
        }
        runCurrent = current;                                           //记录运行电流
        StartCoroutine(RotPointer(
			pointer.transform.localEulerAngles.z, current * 92));      	//指针转动 92-指针旋转间距
    }

    //电流表转回
    public override void stop() {
        StartCoroutine(RotPointer(runCurrent * 92, 0));           		//指针转动 92-指针旋转间距
    }

    //返回端口点
    public override string getPost(int n) {
		return points[n];												//返回端口点
    }


    //获取电压源数量
    public override int getVoltageSourceCount() {
		return 1;														//获取电压源数量
    }

    //标记元器件
    public override void stamp() {
		CirSim.stampVoltageSource(nodes[0], nodes[1], voltSource[0], 0);//标记元器件
    }

    //判断是否是导线
    public override bool isWire() {
		return true;													//判断是否是导线
    }

    // MaterialArray 0-白色材质 1-黑色材质 2-红色材质 3-银色材质
    // OBJ 0-8 白色物体 9-12 黑色 13-17 红色 18-19 银色纽扣

    //元器件透明与不透明的相互转化 direction -true 向透明转换 -false 向不透明转换
    public override void convertState(bool direction) {
        //判断更改材质数组
        Material[] temp = direction ? transparentMaterial : opaqueMaterial;
        for (int i = 0; i < obj.Length; i++) {
            if (i >= 0 && i <= 8) {
                obj[i].GetComponent<Renderer>().material = temp[0];
            } else if (i >= 9 && i <= 12) {
                obj[i].GetComponent<Renderer>().material = temp[1];
            } else if (i >= 13 && i <= 17) {
                obj[i].GetComponent<Renderer>().material = temp[2];
            } else if (i >= 18 && i <= 19) {
                obj[i].GetComponent<Renderer>().material = temp[3];
            }
        }
    }

    //指针转动
    IEnumerator RotPointer(float from, float to) {
        isShaking = false;       						//电流表指针停止振动
        //规定指针的转动范围
        if (to > 92) to = 92;
        if (to < 0) to = 0;

        float rotAngle = from;  						//获取指针转动角度
        if (rotAngle > 300) rotAngle -= 360 - 47;
        else rotAngle += 47;

        int dir = 1;           	 						//转动方向 -1 向左转 1 向右转
        if (to < rotAngle) {
            dir = -1;           						//向左转动
        }

        while ((dir == 1) ? rotAngle <= to : rotAngle >= to) {            //没有转到指定位置 向右转动
            rotAngle = pointer.transform.localRotation.eulerAngles.z;     //获取指针转动角度
            if (rotAngle > 300) rotAngle -= 360 - 47;
            else rotAngle += 47;

            pointer.transform.localRotation = Quaternion.Euler(0, 0, dir * speed + pointer.transform.localRotation.eulerAngles.z);//转动
            yield return 0;
        }

        isShaking = true;           //电流表指针开始震动
        StartCoroutine(Shake());    //开启振动协程
    }


    //指针震动
    IEnumerator Shake() {
        originalZ = pointer.transform.localRotation.eulerAngles.z;      //记录初始轴向
        if (originalZ > 300) originalZ -= 360 - 47;                     //将originalZ转换到0-92之间
        else originalZ += 47;
        int shakeRange = 2;                                             //震动幅度为2
        int dir = 1;                                                    //转动方向 1 向右转 -1 向左转
        float rotAngle;                                                 //指针转动的z值
        while (true) {
            if (!isShaking) break;                                      //若没有震动
            rotAngle = pointer.transform.localRotation.eulerAngles.z;   //获取指针转动角度
            if (rotAngle > 300) rotAngle -= 360 - 47;
            else rotAngle += 47;

            if (rotAngle > originalZ + shakeRange) {                    //若超过最大振幅
                dir = -1;                                               //转向
            }
            if (rotAngle < originalZ - shakeRange) {
                dir = 1;
            }
            pointer.transform.localRotation = Quaternion.Euler(0, 0, dir * 0.1f + pointer.transform.localRotation.eulerAngles.z);//振动
            yield return 0;
        }
	}

}