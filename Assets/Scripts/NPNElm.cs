using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//NPN三极管
public class NPNElm : CircuitElm {

    int pnp;        //默认1为NPN（便于扩展）
    float beta;     //偏移量
    float fgain;
    float gmin;
    const float leakage = 1e-13f;
    const float vt = 0.025f;
    const float vdcoef = 1f / vt;
    const float rgain = 0.5f;
    float vcrit, lastvbc, lastvbe, ic, ie, ib, curcount_c, curcount_e, curcount_b;

    void Start () {
        type = CirSim.TYPES.NPNElm;                                     //声明类型为NPN
        downHole = 3;                                                   //声明下方孔数
        for (int i = 0; i < 6; i++) {
            interfaceList.Add(i, null);                                 //为每个纽扣接口赋初值
        }
        floor = 0;                                                      //设置层数
        state = 0;                                                      //元器件起始状态
        points = new string[3];                                         //初始端点数组
        nodes = new int[getPostCount()];                                //创建结点数组
        volts = new float[getPostCount()];                              //创建电压数组
        voltSource = new int[getVoltageSourceCount()];                  //创建电压源数组
        transform.position = originalPos;                               //还原初始位置

        pnp = 1;                //声明为NPN
        beta = 100;             //偏移量置100
        vcrit = vt * Mathf.Log(vt / (Mathf.Sqrt(2) * leakage));
        fgain = beta / (beta + 1);
    }

    float limitStep(float vnew, float vold) {
        float arg;
        float oo = vnew;

        if (vnew > vcrit && Mathf.Abs(vnew - vold) > (vt + vt)) {
            if (vold > 0) {
                arg = 1 + (vnew - vold) / vt;
                if (arg > 0) {
                    vnew = vold + vt * Mathf.Log(arg);
                }
                else {
                    vnew = vcrit;
                }
            }
            else {
                vnew = vt * Mathf.Log(vnew / vt);
            }
            //sim.converged = false;
        }
        return vnew;
    }

    //标记矩阵
    public override void stamp() {
        CirSim.stampNonLinear(nodes[0]);
        CirSim.stampNonLinear(nodes[1]);
        CirSim.stampNonLinear(nodes[2]);
        doStep();
    }

    //获取端点数量
    public override int getPostCount() {
        return 3;
    }

    //进行下一步
    public override void doStep() {
        float vbc = volts[0] - volts[1]; // typically negative
        float vbe = volts[0] - volts[2]; // typically positive
        if (Mathf.Abs(vbc - lastvbc) > .01 || // .01
        Mathf.Abs(vbe - lastvbe) > .01)
        gmin = 0;
        //System.out.print("T " + vbc + " " + vbe + "\n");
        vbc = pnp * limitStep(pnp * vbc, pnp * lastvbc);
        vbe = pnp * limitStep(pnp * vbe, pnp * lastvbe);
        lastvbc = vbc;
        lastvbe = vbe;
        float pcoef = vdcoef * pnp;
        float expbc = Mathf.Exp(vbc * pcoef);
        /*if (expbc > 1e13 || Double.isInfinite(expbc))
	      expbc = 1e13;*/
        double expbe = Mathf.Exp(vbe * pcoef);
        if (expbe < 1)
            expbe = 1;
        /*if (expbe > 1e13 || Double.isInfinite(expbe))
	      expbe = 1e13;*/
        ie = (float)(pnp * leakage * (-(expbe - 1) + rgain * (expbc - 1)));
        ic = (float)(pnp * leakage * (fgain * (expbe - 1) - (expbc - 1)));
        ib = -(ie + ic);
        //System.out.println("gain " + ic/ib);
        //System.out.print("T " + vbc + " " + vbe + " " + ie + " " + ic + "\n");
        float gee = (float)(-leakage * vdcoef * expbe);
        float gec = rgain * leakage * vdcoef * expbc;
        float gce = -gee * fgain;
        float gcc = -gec * (1 / rgain);

        // stamps from page 302 of Pillage.  Node 0 is the base,
        // node 1 the collector, node 2 the emitter.  Also stamp
        // minimum conductance (gmin) between b,e and b,c
        CirSim.stampMatrix(nodes[0], nodes[0], -gee - gec - gce - gcc + gmin * 2);
        CirSim.stampMatrix(nodes[0], nodes[1], gec + gcc - gmin);
        CirSim.stampMatrix(nodes[0], nodes[2], gee + gce - gmin);
        CirSim.stampMatrix(nodes[1], nodes[0], gce + gcc - gmin);
        CirSim.stampMatrix(nodes[1], nodes[1], -gcc + gmin);
        CirSim.stampMatrix(nodes[1], nodes[2], -gce);
        CirSim.stampMatrix(nodes[2], nodes[0], gee + gec - gmin);
        CirSim.stampMatrix(nodes[2], nodes[1], -gec);
        CirSim.stampMatrix(nodes[2], nodes[2], -gee + gmin);

        // we are solving for v(k+1), not delta v, so we use formula
        // 10.5.13, multiplying J by v(k)
        CirSim.stampRightSide(nodes[0], -ib - (gec + gcc) * vbc - (gee + gce) * vbe);
        CirSim.stampRightSide(nodes[1], -ic + gce * vbe + gcc * vbc);
        CirSim.stampRightSide(nodes[2], -ie + gee * vbe + gec * vbc);
    }

    //是否为非线性元器件
    public override bool nonLinear() {
        return true;
    }
}
