using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class Move : MonoBehaviour {

	public bool MultiTouch = true;														// 是否允许多点触控（控制双指平移和双指缩放）
	public float MaxZoomRatio = 2.0f;													// 最大缩放
	public float MinZoomRatio = 7.5f;													// 最小缩放
	public float RotateHSpeed = 250.0f;													// 水平旋转速度
	public float RotateVSpeed = 225.0f;													// 垂直旋转速度

    public static GameObject gameObj;													// 触摸到的元件
	public static GameObject gameObjGesture;                                            // 连接状态下触碰到的元器件
	public GameObject Magnifier;                                                        // 放大镜
    public GameObject UICamera;															// UI摄像机

    private CircuitElm temp;                                                            // 确认连接后触摸的元件
    private Vector2 offSet;                                                           	// 触摸偏移量
	private Vector2 m_screenpos = new Vector2();                						// 记录手指触屏的位置
	private bool isHoldUnit = false;													// 是否持有元件
	private RaycastHit hit;																// RaycastHit索引

	void Start () {
		if (!MultiTouch)																// 如果不允许多点触控
			Input.multiTouchEnabled = false;               								// 关闭多点触控
	}

	void Update () {
		#if !UNITY_EDITOR && ( UNITY_IOS || UNITY_ANDROID )								// 如果不是unity编辑器运行环境
		MobileInput();																	// 移动平台触屏操作
		#else																			// 如果是unity编辑器运行环境
		DesktopInput();																	// 桌面系统鼠标操作
		#endif																			// 结束假设
	}

	// 桌面系统鼠标操作（由于只是制作移动平台，所以这里省略）
	void DesktopInput() {
		// 记录鼠标左键的移动距离
		float mx = Input.GetAxis("Mouse X");
		float my = Input.GetAxis("Mouse Y");
		if (  mx!= 0 || my !=0 ) {
			// 松开鼠标左键
			if (Input.GetMouseButton(0)) {
				// 摄像机移动
				transform.FindChild ("Main Camera").Translate(new Vector3(mx*Time.deltaTime, my * Time.deltaTime, 0));
			}
		}
	}
		
	// 移动平台触屏操作
	void MobileInput() {

		if (Utils.isIntroduction) {
			return;
		}

		if (Utils.gestureLayer) {//电路连通好后的手势触摸层
			if (Input.touchCount == 1) {
				Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
				LayerMask unitmask = 1 << LayerMask.NameToLayer("unit");//规定触摸层数
                if (Physics.Raycast(ray, out hit, 100, unitmask.value) && Input.touches[0].phase == TouchPhase.Began) {
                    Utils.audioManager.playAudio(1);    //播放音效
                    temp = hit.collider.transform.GetComponent<CircuitElm>();//获取元器件脚本
                    if (temp.type == CirSim.TYPES.SwitchElm) {
                        setUICamera(temp, UICamera);                            //设置UI摄像机位置
                        ((SwitchElm)temp).convert();                            //转换开关状态
                    }
                    else if (temp.type == CirSim.TYPES.SlidingResistanceElm || temp.type == CirSim.TYPES.AmmeterElm) {
                        setUICamera(temp, UICamera);                            //设置UI摄像机位置
                    }
                    gameObjGesture = hit.collider.gameObject;                   //获取触摸物体引用
                    CirSim.analyzeCircuit();        //分析电路
                }
                else if (Input.touches[0].phase == TouchPhase.Moved) {
                    if (gameObjGesture == null) return;
                    CircuitElm elm = gameObjGesture.transform.GetComponent<CircuitElm>();   //获取元器件脚本
                    if (elm.type == CirSim.TYPES.SlidingResistanceElm) {                      //若为滑动变阻器
                        ((SlidingResistanceElm)(elm)).slideSwitch(Input.touches[0].deltaPosition);  //滑动滑块
                        CirSim.analyzeCircuit();        //分析电路
                    }
                }
                else if (Input.touches[0].phase == TouchPhase.Ended) {
                    gameObjGesture = null;               //手持物体引用置null
                }
            }
			return;
		}

        if (gameObj != null) {															// 如果触控物体不为空
			if (!gameObj.tag.Equals ("plate") && Input.touchCount <= 0) {				// 如果触控物体不为地板并且此时没有手指触控事件
				return;																	// 直接返回
			} else if (gameObj.tag.Equals ("plate") && Input.touchCount <= 0) {			// 如果触控物体为地板并且此时没有手指触控事件
				RotatePlate(gameObj);													// 校正旋转地板
			}
		}

		// 1个手指触摸屏幕
		if (Input.touchCount == 1) {
            Ray ray = Camera.main.ScreenPointToRay (Input.GetTouch (0).position);		// 定义从摄像机出发的射线
            LayerMask uimask = 1 << LayerMask.NameToLayer("UI");                            //定义UI层
            LayerMask unitmask = 1 << LayerMask.NameToLayer ("unit");					// 获取unit层
			LayerMask platemask = 1 << LayerMask.NameToLayer ("plate");					// 获取plate层
            LayerMask originalmask = 1 << LayerMask.NameToLayer("original");            // 获取放置初始元器件层

            if (Input.GetMouseButtonDown(0)) {
                if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)) {      //触摸在UI上
                    //Debug.Log("触摸在UI上");
                    gameObj = null;
                    return;
                }
            }

            
            if (Input.touches [0].phase == TouchPhase.Began) {							// 单指触控

				m_screenpos = Input.touches [0].position;								// 记录手指触屏的位置

                if(Physics.Raycast(ray, out hit, 100, uimask.value)) {                  //如果触碰到了UI按钮
                    gameObj = null;
                    return;
                }else if(Physics.Raycast(ray, out hit, 100, originalmask.value)) {      //如果触摸到原始元器件
                    isHoldUnit = true;													//表示持有元件

					GameObject gb = Instantiate (hit.collider.gameObject);
					gb.GetComponent<CircuitElm> ().originalPos = new Vector3 (hit.collider.transform.position.x, 0.0f, hit.collider.transform.position.z);
					gb.transform.SetParent(hit.collider.gameObject.transform.parent);   	//初始化一个元器件

                    gameObj = hit.collider.gameObject;
                    gameObj.tag = "unit";                                               //unit层
                    gameObj.layer = 8;                                                  //unit层
                    gameObj.GetComponent<CircuitElm>().convertState(true);              //转变为透明
                    Utils.holdingName = gameObj.name;                                   //获取手持元器件的名字
                    Vector2 tempPos = Camera.main.WorldToScreenPoint(gameObj.transform.position);	// 将元件的世界坐标转换为屏幕坐标
                    offSet = new Vector2(m_screenpos.x - tempPos.x, m_screenpos.y - tempPos.y);		// 触控时考虑偏移
                }
				else if (Physics.Raycast (ray, out hit, 100, unitmask.value)) {			// 如果触控到元件
					isHoldUnit = true;													// 表示持有元件

                    //若元器件被压在底下，则不能动
                    CircuitElm tempCE = hit.collider.transform.GetComponent<CircuitElm>();
                    if (tempCE.interfaceList[0] != null || tempCE.interfaceList[2] != null) {
                        gameObj = null;
                        return;
                    }

                    gameObj = hit.collider.gameObject;									// 保存持有元件的对象
                    gameObj.GetComponent<CircuitElm>().convertState(true);              //变为透明显示元器件
                    CirSim.elmList.Remove(gameObj.GetComponent<CircuitElm>());          //在元器件列表中删除该元器件
                    Utils.holdingName = gameObj.name;									// 获取手持元器件的名字
                    Vector2 tempPos = Camera.main.WorldToScreenPoint(gameObj.transform.position);	// 将元件的世界坐标转换为屏幕坐标
                    offSet = new Vector2(m_screenpos.x - tempPos.x, m_screenpos.y - tempPos.y);		// 触控时考虑偏移
					//gameObj.transform.SetParent(null);									// 设置父物体为空
				} else {																// 如果没有触控到元件
                    Utils.holdingName = "null";                                         // 手持物体名称
                    isHoldUnit = false;													// 表示未持有元件
					gameObj = Utils.plate;												// 保存地板对象
				} 
			} else if (Input.touches [0].phase == TouchPhase.Moved) {					// 手指移动
                if (gameObj == null) return;                                            // 若手持物体为空，则返回
				Touch touch = Input.touches [0];										// 获取touch对象
				switch (gameObj.tag) {													// 区别操作对象的便签
				case "unit":                                                            // 元件
                    gameObj.transform.GetComponent<CircuitElm>().state = 2;             //切换元器件的状态
                    Vector3 unitPosition = gameObj.transform.position;					// 获取元件世界坐标
					Vector3 screenPosition = Camera.main.WorldToScreenPoint (unitPosition);			// 将元件的世界坐标转为屏幕坐标
					// 计算时考虑偏移量
					Vector3 fingerPosition = new Vector3 (touch.position.x-offSet.x, touch.position.y-offSet.y, screenPosition.z);
					Vector3 worldPosition = Camera.main.ScreenToWorldPoint (fingerPosition);        // 获取触屏点世界坐标
                        gameObj.transform.position = new Vector3(worldPosition.x, gameObj.transform.GetComponent<CircuitElm>().getHeight(), worldPosition.z); 	// 更新元件世界坐标
                    
					break;
				case  "plate":															// 底板
					Vector2 deltaPos = touch.deltaPosition;								// 获取滑屏当前帧改变值
					gameObj.transform.Rotate (Vector3.down * deltaPos.x * 0.6f, Space.World);		// 根据滑屏距离的水平值旋转底板
					break;
				default:
					//Debug.Log ("touch error!");										// 打印错误信息
					break;
				}
			} else if (Input.touches[0].phase == TouchPhase.Ended) {
                //Utils.holdingName = null;//手持元起价置空
                if (gameObj != null && gameObj.tag == "unit") {
                    //恢复为不透明
                    gameObj.GetComponent<CircuitElm>().convertState(false);
                    gameObj.transform.GetComponent<CircuitElm>().putUnit();//将元器件放到指定位置
                }
            }

        } else if (Input.touchCount > 1) {												// 如果是多点触控
			// 记录两个手指的位置
			Vector2 finger1 = new Vector2 ();
			Vector2 finger2 = new Vector2 ();
			// 记录两个手指的移动
			Vector2 mov1 = new Vector2 ();
			Vector2 mov2 = new Vector2 ();

			for (int i = 0; i < 2; i++) {												// 遍历触控点
				Touch touch = Input.touches [i];										// 获取touch对象
				if (touch.phase == TouchPhase.Ended)	{								// 如果有一只手指松开
					break;																// 直接跳出循环
				}
				if (touch.phase == TouchPhase.Moved) {									// 如果行为是移动
					float mov = 0;														// 定义记录移动距离的变量
					if (i == 0) {														// 如果是第一支手指
						finger1 = touch.position;										// 记录触屏位置
						mov1 = touch.deltaPosition;										// 记录移动距离
					} else {															// 如果是第二支手指
						finger2 = touch.position;										// 记录触屏位置
						mov2 = touch.deltaPosition;										// 记录移动距离
						// Debug.Log (" asd " + Vector2.Angle (mov1, mov2));
						// 双指运动方向相同
						if (Vector2.Angle (mov1, mov2) <= 90 && (mov1.magnitude > 0.8f || mov2.magnitude > 0.8f)) {
							if(Vector2.Angle(mov1, new Vector2(1, 0)) < 10) {			// 滑动方向向量如果和x正轴向量夹角小于10度
								// 水平向右旋转
								transform.RotateAround(Utils.plate.transform.position, new Vector3(0, -1, 0), RotateHSpeed * Time.deltaTime);
							} else if(Vector2.Angle(mov1, new Vector2(-1, 0)) < 10) {	// 滑动方向向量如果和x负轴向量夹角小于10度
								// 水平向左旋转
								transform.RotateAround(Utils.plate.transform.position, new Vector3(0, 1, 0), RotateHSpeed * Time.deltaTime);
							} else if(Vector2.Angle(mov1, new Vector2(0, 1)) < 10) {	// 滑动方向向量如果和y负轴向量夹角小于10度
								//垂直向上旋转
								transform.FindChild ("Main Camera").Rotate (-RotateVSpeed * Time.deltaTime, 0, 0);
							} else if(Vector2.Angle(mov1, new Vector2(0, -1)) < 10) {	// 滑动方向向量如果和y负轴向量夹角小于10度
								// 垂直向下旋转
								transform.FindChild ("Main Camera").Rotate (RotateVSpeed * Time.deltaTime, 0, 0);
							}
						} else {														// 缩放
							if (finger1.x > finger2.x) {								// 如果第一根手指的x坐标大于第二根
								mov = mov1.x;											// 总的移动距离为第一根手指的x方向移动距离
							} else {													// 如果第二根手指的x坐标大于第一根
								mov = mov2.x;											// 总的移动距离为第二根手指的x方向移动距离
							}
							if (finger1.y > finger2.y) {								// 如果第一根手指的y坐标大于第二根
								mov += mov1.y;											// 总的移动距离为第一根手指的y方向移动距离
							} else {													// 如果第二根手指的y坐标大于第一根
								mov += mov2.y;											// 总的移动距离为第二根手指的y方向移动距离
							}
							transform.Translate (0, -mov * Time.deltaTime, 0);			// 移动摄像机父物体的y坐标
							if (transform.position.y <= MaxZoomRatio) {					// 如果y坐标小于等于最大放大率
								transform.position = new Vector3(transform.position.x, MaxZoomRatio, transform.position.z);
							} else if(transform.position.y >= MinZoomRatio){			// 如果y坐标大于等于最小放大率
								transform.position = new Vector3(transform.position.x, MinZoomRatio, transform.position.z);
							}
							//Debug.Log (transform.position + "asd" + transform.FindChild("Main Camera").transform.position);
						}
					}
				}
			}
		}

	}

	// 旋转地板的方法（规正非90度倍数角）
	private void RotatePlate(GameObject gameObj) {
		float rotateY = gameObj.transform.rotation.eulerAngles.y;
		if (rotateY > 45 && rotateY < 135) {
			plateRotateTowards (gameObj, 90.0f);
		} else if (rotateY > 135 && rotateY < 225) {
			plateRotateTowards (gameObj, 180.0f);
		} else if (rotateY > 225 && rotateY < 315) {
			plateRotateTowards (gameObj, 270.0f);
		} else if (rotateY > 315 || (rotateY > 0 && rotateY < 45)) {
			plateRotateTowards (gameObj, 0.0f);
		}
	}

	// 旋转至目标值的方法
	private void plateRotateTowards(GameObject gameObj, float aim) {
		gameObj.transform.rotation = Quaternion.RotateTowards (gameObj.transform.rotation, Quaternion.Euler(new Vector3(0, aim, 0)), 5.0f);
	}

    //设置放大镜位置
    private void setMagnifier(GameObject Magnifier) {

    }

    //设置UI摄像机位置
    private void setUICamera(CircuitElm temp, GameObject UICamera) {
        float xx = temp.transform.position.x;
        float yy = temp.transform.position.y;
        float zz = temp.transform.position.z;

        float x = xx;
        float y = yy + 0.5f;
        float z = zz;

        if (temp.type == CirSim.TYPES.AmmeterElm) {             //若是电流表，则UI摄像机向上移动
            AmmeterElm elm = temp.GetComponent<AmmeterElm>();   //获取电流表脚本
            x = elm.camPos.position.x;
            y = elm.camPos.position.y + 0.9f;
            z = elm.camPos.position.z;
        }

        UICamera.transform.position = new Vector3(x, y, z);

        Magnifier.SetActive(true);                              //打开放大镜
        Magnifier.GetComponent<Magnifier>().reset();            //初始化
        Magnifier.transform.FindChild("MagnifierChild").gameObject.SetActive(true);
        Magnifier.transform.FindChild("MagnifierChild").gameObject.GetComponent<Magnifier>().reset();
        setMagnifier(Magnifier);
    }

}