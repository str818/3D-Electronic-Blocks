using UnityEngine;
using System.Collections;

//滑动变阻器逻辑代码
public class SlidingResistanceElm : CircuitElm {

	public float initialRot;                            //初始底盘旋转角
	public float ratio;                                 //比率
	public float maxResistance;                         //总电阻值
    public GameObject Switch;                           //开关
	private float speed;                                //滑块移动速度
    void Start () {
        type = CirSim.TYPES.SlidingResistanceElm;       //声明类型为滑动变阻器
        downHole = 3;									//声明下方孔数
        for (int i = 0; i < 6; i++) {
            interfaceList.Add(i, null);                 //为每个纽扣接口赋初值
        }
		ratio = 0.5f;                                   //初始位置位于中间
		floor = 0;                                      //初始层数设置为0
		state = 0;                                      //元器件起始状态
		points = new string[3];                         //初始端点数组
		maxResistance = 50;                             //定义总电阻值为50欧姆
		nodes = new int[getPostCount()];                //创建结点数组
		volts = new float[getPostCount()];              //创建电压数组
		voltSource = new int[getVoltageSourceCount()];  //创建电压源数组
		speed = 1f;                                     //滑块移动速度
		this.transform.position = originalPos;          //还原初始位置
    }

	//移动滑块
	public void slideSwitch(Vector2 dir) {
		float diffRot = Mathf.Abs(Utils.plate.transform.rotation.eulerAngles.y - initialRot) % 360;   //计算底板旋转绝对值
		if (diffRot > -5 && diffRot < 5) {                                                  //没有旋转
			moveSwitch(-dir.x);
		}
		else if (diffRot > 175 && diffRot < 185) {                                          //旋转180度
			moveSwitch(dir.x);
		}
		else if (diffRot > 85 && diffRot < 95) {                                            //旋转90度
			moveSwitch(dir.y);
		}
		else if(diffRot > 265 && diffRot < 275) {                                           //旋转270度
			moveSwitch(-dir.y);
		}
		ratio = Mathf.Abs(Switch.transform.localPosition.x) / 0.21f + Mathf.Abs(Switch.transform.localPosition.x) % 0.21f;//计算比率
	}

	//移动滑块具体方法
	public void moveSwitch(float diff) {
		float offset = speed * diff * Time.deltaTime;
		if (Switch.transform.localPosition.x + offset > 0 || Switch.transform.localPosition.x + offset < -0.21f) {//超出范围
			return;
		}
		Switch.transform.Translate(offset, 0, 0);
	}

    //返回端口点
    public override string getPost(int n) {
        return (n == 0) ? points[0] : (n == 1) ? points[1] : points[2];
    }

    //获取端点数量
    public override int getPostCount() {
        return 3;
    }

    //标记元器件
    public override void stamp() {

		CirSim.stampResistor(nodes[0], nodes[2], maxResistance * ratio);
		CirSim.stampResistor(nodes[1], nodes[2], maxResistance * (1 - ratio));
    }

    // MaterialArray 0-白色材质 1-外观材质 2-银色材质
    // OBJ 0-6 文字 7-外观 8-11 纽扣

    //元器件透明与不透明的相互转化 direction -true 向透明转换 -false 向不透明转换
    public override void convertState(bool direction) {

        //判断更改材质数组
        Material[] temp = direction ? transparentMaterial : opaqueMaterial;
        for (int i = 0; i < obj.Length; i++) {
            if (i >= 0 && i <= 6) {
                obj[i].GetComponent<Renderer>().material = temp[0];
            }
            else if (i == 7) {
                obj[i].GetComponent<Renderer>().material = temp[1];
            }
            else if (i >= 8 && i <= 11) {//红色文字
                obj[i].GetComponent<Renderer>().material = temp[2];
            }
        }
    }

    //获取电压源数量
    /*public override int getVoltageSourceCount() {
        return endIndex == 0 ? 1 : 0;                           //若滑头在尽头则有一个电压源，否则没有
    }*/

}
