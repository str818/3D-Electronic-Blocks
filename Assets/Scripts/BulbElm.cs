using UnityEngine;
using System.Collections;

//灯泡逻辑代码
public class BulbElm : CircuitElm {

    public GameObject point;                                            //光源
    public float resistance;                                            //电阻值
    public float maxCurrent;                                            //最大电流
    public float delat;                                                 //最大电流偏移量
    public ParticleSystem smoke;                                        //烟雾粒子系统
    public GameObject bulbObj;                                          //灯泡对象
    public Color maxEmission;                                           //最强颜色反射值
    public GameObject Flash;                                            //闪光物体
	public bool isFlicker;

    void Start() {
        type = CirSim.TYPES.BulbElm;									//声明类型为灯泡
		downHole = 2;													//声明下方孔数
        for (int i = 0; i < 4; i++) {
            interfaceList.Add(i, null);									//为每个纽扣接口赋初值
        }
        this.transform.position = originalPos;                          //还原初始位置
        floor = 0;                                                      //设置层数
        state = 0;                                                      //元器件起始状态
        resistance = 10;                                                //初始电阻为10欧姆
        maxCurrent = 0.3f;                                              //最大电流为0.3A
        delat = 0.1f;                                                   //最大电流偏移量
        points = new string[2];                                         //初始端点数组
        nodes = new int[getPostCount()];                                //创建结点数组
        volts = new float[getPostCount()];                              //创建电压数组
        voltSource = new int[getVoltageSourceCount()];                  //创建电压源数组
        maxEmission = new Color(0.465f, 0.47f, 0.14f);                  //最大反射颜色
    }

    //器件开始工作——灯泡亮
    public override void work() {
        //亮灯泡逻辑
        Debug.Log(current);
        float tempCurrent = Mathf.Abs(current);                 		//电流绝对值

		//与音乐集成电路连接
		if (volts[0] == 999 || volts[1] == 999) {
			isFlicker = true;
			StartCoroutine (Flicker());
			return;
		}
		//电流小于等于0
        if (tempCurrent <= 0) {                                 
			isFlicker = false;
            stop();                                            			 //熄灭灯泡
        } else {
            float calAnswer = (tempCurrent / maxCurrent) + (tempCurrent % maxCurrent);
            point.GetComponent<Light>().intensity = calAnswer * 8;  	//根据电流大小设置发光强弱
            bulbObj.GetComponent<Renderer>().material.SetColor(
				"_EmissionColor", calAnswer * maxEmission);				//更改反射颜色
            point.SetActive(true);                                  	//开启点光源
            if (tempCurrent > maxCurrent + delat) {                 	//若大于小灯泡额定电流
                float delayTime = (0.6f + delat - tempCurrent) * 10;	//等待时长
                StartCoroutine(Damage(delayTime));                  	//触发损坏元器件效果                  
            }
		}
	}

    //器件停止工作——灭灯泡
    public override void stop() {
		isFlicker = false;
        bulbObj.GetComponent<Renderer>().material.SetColor("_EmissionColor", new Color(0, 0, 0));//更改反射颜色
        point.gameObject.SetActive(false);                      //关闭点光源
	}

    // MaterialArray 0-黑线材质 1-灯泡透明材质 2-外部蓝色材质 3-红字材质 4-银色材质 5-不锈钢材质
    // OBJ 0-3 黑线 4-5 透明玻璃材质 6 外部蓝色材质 7-14 红字材质 15-20 银色材质 21-25 不锈钢材质

    //元器件透明与不透明的相互转化 direction -true 向透明转换 -false 向不透明转换
    public override void convertState(bool direction) {
		//判断更改材质数组
		Material[] temp = direction ? transparentMaterial : opaqueMaterial;
		for (int i = 0; i < obj.Length; i++) {
			if (i >= 0 && i <= 3) {
				obj [i].GetComponent<Renderer> ().material = temp [0];
			} else if (i >= 4 && i <= 5) {
				obj [i].GetComponent<Renderer> ().material = temp [1];
			} else if (i == 6) {
				obj [i].GetComponent<Renderer> ().material = temp [2];
			} else if (i >= 7 && i <= 14) {
				obj [i].GetComponent<Renderer> ().material = temp [3];
			} else if (i >= 15 && i <= 20) {
				obj [i].GetComponent<Renderer> ().material = temp [4];
			} else if (i >= 21 && i <= 25) {
				obj [i].GetComponent<Renderer> ().material = temp [5];
			}
		}
	}

    //计算电流值
    public override void calculateCurrent() {
		current = (volts [0] - volts [1]) / resistance;
	}

    //标记元器件
    public override void stamp() {
		CirSim.stampResistor (nodes [0], nodes [1], resistance);
	}

    //灯泡损坏协程
    IEnumerator Damage(float delayTime) {
        yield return new WaitForSeconds(delayTime);     					//等待delayTime秒
        Flash.SetActive(true);                         	 					//闪光效果
        Utils.audioManager.playAudio(2);                					//播放烧毁音效
        stop();                                         					//小灯泡停止工作
        isDamaged = true;                               					//元器件破损
        CirSim.damageList.Add(transform.GetComponent<CircuitElm>());    	//添加进损坏列表
        CirSim.analyzeCircuit();                        					//重新分析电路
        yield return new WaitForSeconds(0.5f);          					//等待音效播放完
        smoke.Play();                                   					//触发粒子系统
        Utils.menuListener.PopTipUI("灯泡烧毁");         					//弹出提示界面
	}

	//灯泡闪烁协程
	IEnumerator Flicker() {
		while (isFlicker) {
			float calAnswer = 1.0f;
			bulbObj.GetComponent<Renderer>().material.SetColor("_EmissionColor", calAnswer * maxEmission);//更改反射颜色
			yield return new WaitForSeconds(2.0f);     		//等待delayTime秒
			while(calAnswer>0){
				bulbObj.GetComponent<Renderer>().material.SetColor("_EmissionColor", calAnswer * maxEmission);//更改反射颜色
				calAnswer -= 0.01f;
				yield return 0;
			}
		}
	}
	
}
