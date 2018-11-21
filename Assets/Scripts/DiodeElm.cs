using UnityEngine;
using System.Collections;

//二极管逻辑代码
public class DiodeElm : CircuitElm {

    public Material turnOn;                                             //二极管灯泡发光的材质
    public Material turnOff;                                            //二极管灯泡熄灭的材质
    public GameObject bulb;                                             //灯泡
    public const int FLAG_FWDROP = 1;
    public const float defaultdrop = 0.80590483f;
    float fwdrop, zvoltage, vt, vdcoef, zoffset, leakage, lastvoltdiff, vcrit;

    void Start() {
        type = CirSim.TYPES.DiodeElm;                                   //声明类型为二极管
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
        transform.position = originalPos;                               //还原初始位置
		setUp();
    }

	//初始计算参数
	public void setUp(){
		//初始计算变量值
		fwdrop = defaultdrop;
		zvoltage = 0;
		leakage = 1e-14f;
		vdcoef = Mathf.Log(1 / leakage + 1) / fwdrop;
		vt = 1 / vdcoef;
		vcrit = vt * Mathf.Log(vt / (Mathf.Sqrt(2) * leakage));
		if (zvoltage == 0) {
			zoffset = 0;
		}
		else {
			float i = -0.005f;
			zoffset = zvoltage - Mathf.Log(-(1 + i / leakage)) / vdcoef;
		}
	}
		
    public override void work() {
        Debug.Log(current);
        if (current > 0) {
            bulb.GetComponent<Renderer>().material = turnOn;            
        }else {
            bulb.GetComponent<Renderer>().material = turnOff;
        }
    }

    public override void stop() {
        bulb.GetComponent<Renderer>().material = turnOff;
    }

    //标记矩阵
    public override void stamp() {
        //标记两个结点矩阵的非线性
        CirSim.stampNonLinear(nodes[0]);
        CirSim.stampNonLinear(nodes[1]);
        //doStep();
    }

    //计算电流值
    public override void calculateCurrent() {
        float voltdiff = volts[0] - volts[1];                           //电势
        if (voltdiff >= 0 || zvoltage == 0) {
            current = leakage * (Mathf.Exp(voltdiff * vdcoef) - 1);     //计算电流值
        }
        else {
            current = leakage * (Mathf.Exp(voltdiff * vdcoef) - Mathf.Exp((-voltdiff - zoffset) * vdcoef) - 1);
        }
    }

    float limitStep(float vnew, float vold) {
        float arg;
        float oo = vnew;

        // check new voltage; has current changed by factor of e^2?
        if (vnew > vcrit && Mathf.Abs(vnew - vold) > (vt + vt)) {
            if (vold > 0) {
                arg = 1 + (vnew - vold) / vt;
                if (arg > 0) {
                    // adjust vnew so that the current is the same
                    // as in linearized model from previous iteration.
                    // current at vnew = old current * arg
                    vnew = vold + vt * Mathf.Log(arg);
                    // current at v0 = 1uA
                    float v0 = Mathf.Log(1e-6f / leakage) * vt;
                    vnew = Mathf.Max(v0, vnew);
                }
                else {
                    vnew = vcrit;
                }
            }
            else {
                // adjust vnew so that the current is the same
                // as in linearized model from previous iteration.
                // (1/vt = slope of load line)
                vnew = vt * Mathf.Log(vnew / vt);
            }
            //System.out.println(vnew + " " + oo + " " + vold);
        }
        else if (vnew < 0 && zoffset != 0) {
            // for Zener breakdown, use the same logic but translate the values
            vnew = -vnew - zoffset;
            vold = -vold - zoffset;

            if (vnew > vcrit && Mathf.Abs(vnew - vold) > (vt + vt)) {
                if (vold > 0) {
                    arg = 1 + (vnew - vold) / vt;
                    if (arg > 0) {
                        vnew = vold + vt * Mathf.Log(arg);
                        float v0 = Mathf.Log(1e-6f / leakage) * vt;
                        vnew = Mathf.Max(v0, vnew);
                        //System.out.println(oo + " " + vnew);
                    }
                    else {
                        vnew = vcrit;
                    }
                }
                else {
                    vnew = vt * Mathf.Log(vnew / vt);
                }
            }
            vnew = -(vnew + zoffset);
        }
        return vnew;
    }

    //进行下一步
    public override void doStep() {
		setUp ();
        float voltdiff = volts[0] - volts[1];                           //电势

        voltdiff = limitStep(voltdiff, lastvoltdiff);
        lastvoltdiff = voltdiff;

        if (voltdiff >= 0 || zvoltage == 0) {
            // regular diode or forward-biased zener
            float eval = Mathf.Exp(voltdiff * vdcoef);
            // make diode linear with negative voltages; aids convergence
            if (voltdiff < 0)
                eval = 1;
            float geq = vdcoef * leakage * eval;
            float nc = (eval - 1) * leakage - geq * voltdiff;
            CirSim.stampConductance(nodes[0], nodes[1], geq);
            CirSim.stampCurrentSource(nodes[0], nodes[1], nc);
        }
        else {
            // Zener diode

            /* 
			 * I(Vd) = Is * (exp[Vd*C] - exp[(-Vd-Vz)*C] - 1 )
			 *
			 * geq is I'(Vd)
			 * nc is I(Vd) + I'(Vd)*(-Vd)
			 */

            float geq = leakage * vdcoef * (
                Mathf.Exp(voltdiff * vdcoef) + Mathf.Exp((-voltdiff - zoffset) * vdcoef));

            float nc = leakage * (
                Mathf.Exp(voltdiff * vdcoef)
                - Mathf.Exp((-voltdiff - zoffset) * vdcoef)
                - 1
                ) + geq * (-voltdiff);

            CirSim.stampConductance(nodes[0], nodes[1], geq);
            CirSim.stampCurrentSource(nodes[0], nodes[1], nc);
        }
    }

    // MaterialArray 0-红色材质 1-白色材质 2-红色灯泡  3-银色材质
    // OBJ  0 红色外观 1-5 白色文字 6 红色灯泡 7-13 银色

    //元器件透明与不透明的相互转化 direction -true 向透明转换 -false 向不透明转换
    public override void convertState(bool direction) {

        //判断更改材质数组
        Material[] temp = direction ? transparentMaterial : opaqueMaterial;
        for (int i = 0; i < obj.Length; i++) {
            if (i == 0) {
                obj[i].GetComponent<Renderer>().material = temp[0];
            }
            else if (i >= 1 && i <= 5) {
                obj[i].GetComponent<Renderer>().material = temp[1];
            }
            else if (i == 6) {
                obj[i].GetComponent<Renderer>().material = temp[2];
            }
            else if (i >= 7 && i <= 13) {
                obj[i].GetComponent<Renderer>().material = temp[3];
            }
        }
    }

    //是否为非线性元器件
    public override bool nonLinear() {
        return true;
    }
}
