using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//元器件父类
public class CircuitElm : MonoBehaviour {

    public CirSim.TYPES type;											//元器件类型
	public int downHole;												//元器件下方孔数
	public ArrayList buttonColliders = new ArrayList();					//纽扣碰撞列表
    public List<int> buttonCollidersIndex = new List<int>();            //记录纽扣碰撞列表中纽扣对应的point序号
    public ArrayList unitColliders = new ArrayList();                   //元器件碰撞列表
    public Dictionary<int, CircuitElm> interfaceList = 
		new Dictionary<int, CircuitElm>();								//每个元器件的接口
    public int floor;                                                   //层数

    public Material[] opaqueMaterial;                                   //不透明材质数组
    public Material[] transparentMaterial;                              //透明材质数组
    public GameObject[] obj;                                            //元器件部件数组

    public Vector3 originalPos;            	//元器件初始位置
    public Vector3 originalRot;            	//元器件的初始旋转角
    public int state;						//元器件状态 0-放在原位 1-正在移动动画 2-正在拖动 3-安装在板上

    public string[] points;                 //端点数组
    public int[] nodes;                     //结点数组
    public float[] volts;                   //电压数组
    public float current;                   //电流值
    public int[] voltSource;                //电压源序号

    public bool isDamaged;                  //是否破损

    public AudioManager audioManager;       //音效脚本

    void Update() {
        MoveUnit();							//移动元器件
    }

	//移动元器件到初始位置（用于没有成功安装在电路中的器件）
	public void MoveUnit() {
		if (state == 1) {																		//如果正在移动
			transform.position = Vector3.Lerp (transform.position, originalPos, 0.1f);   		//移动元器件位置
			Quaternion targetRot = new Quaternion ();                                    		//目标角度
			targetRot.eulerAngles = originalRot;												//目标角度为初始角度
			transform.rotation = Quaternion.Slerp (transform.rotation, targetRot, 0.1f); 		//旋转元器件角度
			if (Mathf.Abs (transform.position.x - originalPos.x) < 0.05f
			    && Mathf.Abs (transform.position.y - originalPos.y) < 0.05f
			    && Mathf.Abs (transform.position.z - originalPos.z) < 0.05f) {   				//移动到指定位置
				state = 0;																		//更改状态为放在原位
				buttonColliders.Clear ();                                                      	//清空碰撞列表
				Destroy (gameObject);                                                           //移动到原位置后销毁此物体
			}
		}
	}

    public void OnTriggerEnter(Collider collider) {
		//若本元器件正在被触摸，并且碰到了元器件
		if (Utils.holdingName.Equals (transform.name) && (collider.tag == "unit") && state != 1) {
			unitColliders.Add (collider.transform.GetComponent<CircuitElm> ());					//将本元器件脚本添加到元器件列表
			Utils.adjustHeight (transform, unitColliders);										//调整高度
		} 
		//若本元器件碰到了地板
		else if (Utils.holdingName.Equals (transform.name) && (collider.tag == "plate")) {
			floor = 1;																			//调整层数
			transform.position = new Vector3 (transform.position.x, 
				0.064f, transform.position.z);													//调整高度
		}
		//若本元器触碰到其他元器件的纽扣
		else if (collider.tag == "button" && collider.transform.parent.tag == "unit" && (state != 0) && (state != 1)) {
			CircuitElm temp = collider.transform.parent.GetComponent<CircuitElm> ();			//获取正在触摸的元器件引用
			if (temp.interfaceList [int.Parse (collider.name)] != null
			    && collider.transform.parent.parent != null) {									//已经连上的就不能再连了
				return;
			}
			temp.interfaceList [int.Parse (collider.name)] = transform.GetComponent<CircuitElm> ();		//接口连接
			buttonColliders.Add (collider.gameObject);											//添加触碰到的纽扣
		}
		//若本元器触碰到底板上的纽扣
		else if (collider.tag == "point" && (state == 2 || state == 1)) {
			buttonColliders.Add (collider.gameObject);											//添加触碰到的纽扣
		}
	}

    public void OnTriggerExit(Collider collider) {
		//若本元器件正在被触摸，并且碰到了元器件
		if (Utils.holdingName.Equals (transform.name) && collider.tag == "unit" && state != 1) {
			unitColliders.Remove (collider.transform.GetComponent<CircuitElm> ());				//将本元器件脚本在元器件列表中移除
			Utils.adjustHeight (transform, unitColliders);										//调整高度
		}
		//若本元器件碰到了地板
		else if (Utils.holdingName.Equals (transform.name) && (collider.tag == "plate")) {
			floor = 0;																			//调整层数
			transform.position = new Vector3 (transform.position.x,
				0f, transform.position.z);														//调整高度
		}
		//若本元器触碰到其他元器件的纽扣
		else if (collider.tag == "button" && collider.transform.parent.tag == "unit" && (state != 0) && (state != 1)) {
			CircuitElm temp = collider.transform.parent.GetComponent<CircuitElm> ();			//获取正在触摸的元器件引用
			//若不是正常离开纽扣
			if (temp.interfaceList [int.Parse (collider.name)] != null &&
			             !temp.interfaceList [int.Parse (collider.name)].Equals (transform.GetComponent<CircuitElm> ())) {
				return;
			}
			temp.interfaceList [int.Parse (collider.name)] = null;
			buttonColliders.Remove (collider.gameObject);										//删除纽扣
		}
		//若本元器触碰到底板上的纽扣
		else if (collider.tag == "point" && (state == 2 || state == 1)) {
			buttonColliders.Remove (collider.gameObject);										//删除纽扣
		}
	}

    //返回端口点
    public virtual string getPost(int n) {
		return points [n];
    }

    //根据纽扣序号返回端口点
    public virtual string getPostFromIndex(int n) {
        return getPost(n / 2);
    }

	//设置端口点 0、1纽扣代表端口1（point2） 2、3纽扣代表端口0（point1)
	public virtual void setPosts() {
		buttonCollidersIndex.Clear ();                                                   							//清空纽扣对应端口的列表
		if (floor == 1) {                                                               							//若该元器件直接插在底板上
			if (getPostCount () == 2 || type == CirSim.TYPES.WireElm) {                       						//若端口数为两个或者是导线
				if (((GameObject)buttonColliders [0]).GetComponent<Point> ().port == 1) { 							//若碰到的时1纽扣（端口1）
					points [0] = ((GameObject)buttonColliders [0]).name;                  							//获取结点名称
					points [1] = ((GameObject)buttonColliders [1]).name;
				} else {                                                                 							//碰到的是3纽扣（端口0）
					points [1] = ((GameObject)buttonColliders [0]).name;
					points [0] = ((GameObject)buttonColliders [1]).name;
				}
			} else {                                                                     							//端口数大于2并且不是导线
				for (int i = 0; i < downHole; i++) {
					int port = ((GameObject)buttonColliders [i]).GetComponent<Point> ().port; 						//获取连接端点数
					buttonCollidersIndex.Add (port / 2);                                     						//记录纽扣对应的端口
					points [port / 2] = ((GameObject)buttonColliders [i]).name;                  					//设置端点名称
				}
			}           
		} else if (floor > 1) {                                                                         			//若元器件插在其他元器件上
			for (int i = 0; i < downHole; i++) {
				CircuitElm ce = ((GameObject)buttonColliders [i]).transform.parent.GetComponent<CircuitElm> ();   	//获取元器件
				int port = calNextPort (transform.GetComponent<CircuitElm> (), ce);                           		//获取本元器件的连接纽扣
				int index = int.Parse (((GameObject)buttonColliders [i]).name);                               		//被碰撞元器件连接纽扣
				buttonCollidersIndex.Add (port / 2);                                                         		//记录纽扣对应的端口
				points [port / 2] = ce.getPostFromIndex (index);                                              		//获取结点名称
			}
		}
	}

    //计算本元器件哪个端口连接下一个元器件
    //root--此元器件 next--下一个元器件
    public static int calNextPort(CircuitElm root, CircuitElm next) {
        for (int i = 0; i < root.interfaceList.Count; i++) {
            if (root.interfaceList[i] != null && root.interfaceList[i].Equals(next)) {			//若该端口连接为此元器件
                return i;																		//返回连接端口号
            }
        }
        return -1;																				//连接错误
    }

    //设置端口 p 对应的结点是 nodeList[n]
    public void setNode(int p ,int n) {
        nodes[p] = n;
    }

    //获取端口对应的结点
    public int getNode(int n) {
        return nodes[n];
    }

    //设置结点电压
    public void setNodeVoltage(int n,float c) {
        volts[n] = c;       																	//记录端点电压
        calculateCurrent(); 																	//计算电流值
    }

    //计算电流值
    public virtual void calculateCurrent() {

    }

    //执行下一步
    public virtual void doStep() {

    }

    //设置电流值
    public virtual void setCurrent(int x, float c) {
        current = c;
    }

	//放置元器件
	public void putUnit() {
		//碰撞列表中的碰撞物体等于此元器件的安装孔
		if(buttonColliders.Count == downHole) {
			//元器件的接孔不能全插在另一个元器件上
			if (buttonColliders.Count == 2 && ((GameObject)(buttonColliders[0])).transform.parent
				.Equals(((GameObject)(buttonColliders[1])).transform.parent) &&
				((GameObject)(buttonColliders[0])).transform.parent.tag=="unit") {
				transform.GetComponent<CircuitElm>().state = 1; 							//切换元器件的状态-恢复原位过程
				transform.SetParent(null);                          						//设置父对象为空
				CirSim.elmList.Remove(transform.GetComponent<CircuitElm>());				//删除元器件引用
				return;
			}
			setPosts();                                                 					//设置元器件端口点
            Vector3 core = new Vector3(0, 0, 0);
            if (downHole == 1) {                                        					//若只有一个端口
                core = ((GameObject)(buttonColliders[0])).transform.position;
            }else {
                core = Utils.calCore(buttonColliders, buttonCollidersIndex, downHole);     	//计算放置位置
            }
			transform.position = new Vector3(core.x, transform.position.y, core.z); 		//设置元器件位置
			transform.SetParent(Utils.plate.transform);                             		//设置父对象
			CirSim.elmList.Add(transform.GetComponent<CircuitElm>());   					//向元器件列表中添加此元器件引用
			Utils.audioManager.playAudio(0);                                  				//播放音效
			transform.GetComponent<CircuitElm>().state = 3;             					//切换元器件的状态-安装在板上

			if (type == CirSim.TYPES.SlidingResistanceElm) {            					//若是滑动变阻器
				((SlidingResistanceElm)(this)).initialRot = 
					Utils.plate.transform.rotation.eulerAngles.y;							//记录安装时滑动变阻器的初始旋转角
			}
		} else {
			transform.GetComponent<CircuitElm>().state = 1;             					//切换元器件的状态-恢复原位过程
			transform.SetParent(null);                                  					//设置父对象为空
			CirSim.elmList.Remove(transform.GetComponent<CircuitElm>());					//删除元器件引用
		}
	}

	//获取元器件的高度
	public float getHeight() {
		float height = 0;
		switch (floor) {
		case 0:
			height = 0f;
			break;
		case 1:
			height = 0.064f;
			break;
		case 2:
			height = 0.157f;
			break;
		case 3:
			height = 0.250f;
			break;
		case 4:
			height = 0.343f;
			break;
		}
		return height;
	}

    //转换为隐藏显示与非隐藏显示的方法，在子类中实现 direction -true 向透明转换 -false 向不透明转换
    public virtual void convertState(bool direction) {

    }

    //是否为非线性元器件
    public virtual bool nonLinear() {
        return false;
    }

    //获取电压源数量
    public virtual int getVoltageSourceCount() {
        return 0;
    }

    //获取端点数量
    public virtual int getPostCount() {
        return 2;
    }

    //设置电压源序号
    public void setVoltageSource(int n,int v) {
        voltSource[n] = v;
    }

    //标记矩阵
    public virtual void stamp() {

    }

    //判断是否接地
    public virtual bool hasGroundConnection(int n) {
        return false;
    }

    //判断两端口是否相连
    public virtual bool getConnection(int n1,int n2) {
        return true;
    }

    //判断是否是导线
    public virtual bool isWire() {
        return false;
    }

    //元器件工作
    public virtual void work() {

    }

    //元器件停止工作
    public virtual void stop() {

    }

}
