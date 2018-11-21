using UnityEngine;
using System.Collections;

//电池逻辑代码
public class VoltageElm : CircuitElm {

    public float maxVoltage;//最大电压
    void Start() {
        type = CirSim.TYPES.VoltageElm;								//声明类型为电池
		downHole = 2;												//声明下方孔数
        for (int i = 0; i < 4; i++) {								//0、1-正极 2、3负极
            interfaceList.Add(i, null);								//为每个纽扣接口赋初值
        }
        floor = 0;
        this.transform.position = originalPos;                      //还原初始位置
        state = 0;                                                  //元器件起始状态
        maxVoltage = 3;                                             //初始电源最大电压为3V
        points = new string[2];                                     //初始端点数组
        nodes = new int[getPostCount()];                            //创建结点数组
        volts = new float[getPostCount()];                          //创建电压数组
        voltSource = new int[getVoltageSourceCount()];              //创建电压源数组
    }

    

    //设置端口点-有正负极之分
    public override void setPosts() {
        if (floor == 1) {                                           //若该元器件直接插在底板上
            Point point = ((GameObject)buttonColliders[0]).GetComponent<Point>();//获取纽扣上的脚本
            if (point.port == 1) {                                  //该纽扣碰撞的是电池的正极
                points[1] = ((GameObject)buttonColliders[0]).name;         //获取结点名称
                points[0] = ((GameObject)buttonColliders[1]).name;
            }else {
                points[0] = ((GameObject)buttonColliders[0]).name;         //获取结点名称
                points[1] = ((GameObject)buttonColliders[1]).name;
            }
        }
        else {                                                     //若元器件插在其他元器件上
            int index;
            CircuitElm thisCE = transform.GetComponent<CircuitElm>();                                       //此元器件
            CircuitElm ce = ((GameObject)buttonColliders[0]).transform.parent.GetComponent<CircuitElm>();   //获取元器件
            if (thisCE.interfaceList[1].Equals(ce)) {                                              //若是电源的正极连接此端口
                ce = ((GameObject)buttonColliders[0]).transform.parent.GetComponent<CircuitElm>();
                index = int.Parse(((GameObject)buttonColliders[0]).name);
                points[1] = ce.getPostFromIndex(index);                                            //point2端口点代表电源的正极
                ce = ((GameObject)buttonColliders[1]).transform.parent.GetComponent<CircuitElm>();
                index = int.Parse(((GameObject)buttonColliders[1]).name);
                points[0] = ce.getPostFromIndex(index);                                            //point1端口点代表电源的负极
            }
            else {                                                                                 //若是电源的正极连接此端口
                ce = ((GameObject)buttonColliders[0]).transform.parent.GetComponent<CircuitElm>();
                index = int.Parse(((GameObject)buttonColliders[0]).name);
                points[0] = ce.getPostFromIndex(index);                                            //point1端口点代表电源的负极
                ce = ((GameObject)buttonColliders[1]).transform.parent.GetComponent<CircuitElm>();
                index = int.Parse(((GameObject)buttonColliders[1]).name);
                points[1] = ce.getPostFromIndex(index);                                            //point2端口点代表电源的正极
            }
        }
    }

    // MaterialArray 0-电池材质 1-外框材质 2-银色接口材质 3-白色条材质 4-红字材质
    // OBJ 0-1 电池 2-13 黑色边框 14-21 红色文字 22 白色条 23-28 银色材质

    //元器件透明与不透明的相互转化 direction -true 向透明转换 -false 向不透明转换
    public override void convertState(bool direction) {

        //判断更改材质数组
        Material[] temp = direction ? transparentMaterial : opaqueMaterial;
        for(int i = 0; i < obj.Length; i++) {
            if(i>=0 && i <= 1) {//电池材质
                obj[i].GetComponent<Renderer>().material = temp[0];
            }else if (i>=2 && i<=13) {//黑色边框
                obj[i].GetComponent<Renderer>().material = temp[1];
            }else if (i>=14 && i<=21) {//红色文字
                obj[i].GetComponent<Renderer>().material = temp[4];
            }else if (i == 22) {//白条材质
                obj[i].GetComponent<Renderer>().material = temp[3];
            }else if (i>=23 && i<=28){//银色材质
                obj[i].GetComponent<Renderer>().material = temp[2];
            }
        }
    }

    //获取电压
    public float getVoltage() {
        return maxVoltage;
    }

    //获取电压源数量
    public override int getVoltageSourceCount() {
        return 1;
    }

    //标记元器件
    public override void stamp() {
        CirSim.stampVoltageSource(nodes[0], nodes[1], voltSource[0], getVoltage());
    }

    //根据纽扣序号返回端口点
    public override string getPostFromIndex(int n) {
        if (n == 0 || n == 1) return getPost(1);
        return getPost(0);
    }
}
