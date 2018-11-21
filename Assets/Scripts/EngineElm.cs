using UnityEngine;
using System.Collections;

//发动机逻辑代码
public class EngineElm : CircuitElm {

	public float resistance;                                            //电阻值
	public GameObject shaft;                                            //转杆
    public GameObject fan;                                              //风扇对象
    public GameObject canvas;                                           //画布
    private float runCurrent;                                           //转动时的电流
    private float rotFre;                                               //转动频率
    private float rotFre_Fan;                                           //风扇转动的频率
    private Vector3 iniCamRot;                                          //摄像机初始位置
    void Start () {
		type = CirSim.TYPES.EngineElm;                                  //声明类型为电动机
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
		rotFre = 0;                                                     //转动频率置0
		transform.position = originalPos;                      	        //还原初始位置
        canvas = GameObject.Find("Canvas");                             //获取画布
	}

	void Update() {
        MoveUnit();                                                     //移动元器件
		shaft.transform.Rotate(new Vector3(0, rotFre, 0), Space.World);  //风扇转动
        if (Input.GetKeyDown(KeyCode.A)) {
            current = -0.3f;
            work();
        }

        if (Input.GetKeyDown(KeyCode.S)) {
            current = 0f;
            work();
        }
    }

	//发动机转动
	public override void work() {
        if (Mathf.Abs(current) <= 0) {  //电流为0
            if (runCurrent < 0) {       //之前的电流大于0
                StartCoroutine(Rot());  //开始转动风扇
            }
            StartCoroutine(Quit(0.5f, false));      //停止风扇
            return;
        }
        StopCoroutine("Quit");          //停止协程
        runCurrent = current;           //记录运行电流
		StartCoroutine(Run());          //启动风扇
	}

	//发动机停止转动
	public override void stop() {
		StartCoroutine(Quit(0.5f, true));      //停止风扇
	}

	//计算电流值
	public override void calculateCurrent() {
		current = (volts[0] - volts[1]) / resistance;
	}

	//标记元器件
	public override void stamp() {
		CirSim.stampResistor(nodes[0], nodes[1], resistance);
	}

	// MaterialArray 0-黄色材质 1-白色材质 2-红色文字 3-外观 4-银色材质 5-黑色材质
	// OBJ 0-4 风扇 5-8 白色文字 9-10 红色文字 11-12 黄色外观 13-22 银色材质 23-24黑色材质

	//元器件透明与不透明的相互转化 direction -true 向透明转换 -false 向不透明转换
	public override void convertState(bool direction) {

		//判断更改材质数组
		Material[] temp = direction ? transparentMaterial : opaqueMaterial;
		for (int i = 0; i < obj.Length; i++) {
			if (i >= 0 && i <= 4) {
				obj[i].GetComponent<Renderer>().material = temp[0];
			}
			else if (i >= 5 && i <= 8) {
				obj[i].GetComponent<Renderer>().material = temp[1];
			}
			else if (i >= 9 && i <= 10) {
				obj[i].GetComponent<Renderer>().material = temp[2];
			}
			else if (i >= 11 && i <= 12) {
				obj[i].GetComponent<Renderer>().material = temp[3];
			}
			else if (i >= 13 && i <= 22) {
				obj[i].GetComponent<Renderer>().material = temp[4];
			}
			else if (i >= 23 && i <= 24) {
				obj[i].GetComponent<Renderer>().material = temp[5];
			}
		}
	}

    //启动风扇
    IEnumerator Run() {
        while (Mathf.Abs(rotFre) < Mathf.Abs(current * 100)) {
            if (current > 0) {
                rotFre += current * 5;      //增加转速
            }
            else {
                rotFre -= current * 5;
            }
            yield return 0;                 //等待一帧
        }
    }

    //停止风扇
    IEnumerator Quit(float speed, bool flag) {
        while (rotFre > 0.5 || rotFre < -0.5) {
            if (current != 0 && !flag) break;      //若电流大于0 退出循环继续转动/*666*/
            if (runCurrent > 0) {
                rotFre -= runCurrent * speed;      //减小转速
            }
            else {
                rotFre += runCurrent * speed;
            }
            yield return 0;                         //等待一帧
        }
        if (current == 0) rotFre = 0;                 //转速置0
    }

    //风扇飞起时的转动效果
    IEnumerator Rot() {
        rotFre_Fan = rotFre;                                                //获取转动频率
        iniCamRot = Utils.camera.transform.localRotation.eulerAngles;       //记录摄像机的初始转向

        //随机设置位置
        float randomX, randomZ;
        float random = Random.Range(0, 3);
        if (random < 1) {
            randomX = Random.Range(2, 5);
            randomZ = Random.Range(2, 5);
        }
        else if (random < 2) {
            randomX = Random.Range(2, 5);
            randomZ = Random.Range(-2, -5);
        }
        else {
            randomX = Random.Range(-2, -5);
            randomZ = Random.Range(2, 5);
        }

        GameObject newFan = (GameObject)Instantiate(fan,fan.transform.position,Quaternion.Euler(-90,0,0));//新建风扇
        fan.SetActive(false);
        newFan.AddComponent<Rigidbody>();                              //添加刚体
        newFan.AddComponent<BoxCollider>();                            //添加碰撞器
        newFan.GetComponent<Rigidbody>().velocity = new Vector3(randomX, 10 + Mathf.Abs(runCurrent) * 10, randomZ);
        FollowTarget.target = newFan;                               //目标定为风扇
        Utils.Magnifier.SetActive(false);
        Utils.isCameraMove = true;              //开始跟随


        canvas.SetActive(false);                //画布设为不可见
        while (newFan.GetComponent<Rigidbody>().velocity != Vector3.zero) {    //若还有速度
            if (runCurrent > 0) {
                rotFre_Fan -= runCurrent * 0.1f;    //减少转速
            }
            else {
                rotFre_Fan += runCurrent * 0.1f;
            }
            newFan.transform.Rotate(new Vector3(0, rotFre_Fan, 0), Space.World);
            yield return 0;
        }

        Utils.isCameraMove = false;             //摄像机停止跟随运动
        StartCoroutine(Quit(0.05f, true));      //风扇落到地面后逐渐停止转动
        yield return new WaitForSeconds(3.0f);  //等待3秒后

        //各种参数置0
        rotFre = 0;
        rotFre_Fan = 0;
        runCurrent = 0;

        Utils.camera.transform.localPosition = Vector3.zero;  //摄像机位置恢复原位
        Utils.camera.transform.localEulerAngles = iniCamRot;  //摄像机旋转角恢复原位


        Destroy(newFan);                           //销毁转出去的风扇
        fan.SetActive(true);                    //风扇设为可见

        canvas.SetActive(true);                //画布设为可见
    }
}