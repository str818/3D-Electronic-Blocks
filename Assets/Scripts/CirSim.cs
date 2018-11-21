using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//模拟电路类
public class CirSim : MonoBehaviour {

    public static List<CircuitElm> elmList = new List<CircuitElm>();        //元器件列表 
    public static List<CircuitNode> nodeList = new List<CircuitNode>();     //结点列表 
    public static List<CircuitElm> damageList = new List<CircuitElm>();     //已损坏元器件列表
    public static CircuitElm[] voltageSources;                              //电压源数组
    public static bool circuitNonLinear = false;                            //电路是否是非线性的
    //矩阵数组
    public static float[,] circuitMatrix, origMatrix;
    public static float[] circuitRightSide, origRightSide;
    public static int[] circuitPermute;                                     //电路交换数组
    public static RowInfo[] circuitRowInfo;                                 //矩阵行信息
    
    public static int circuitMatrixSize, circuitMatrixFullSize;             //电路矩阵大小
    public static bool circuitNeedsMap;

    public static bool isNext;                                              //是否第二次计算

    //元器件类型
    public enum TYPES {
        VoltageElm,						//电源
        WireElm,						//导线
        SwitchElm,						//开关
        BulbElm,						//灯泡
        SlidingResistanceElm,   		//滑动变阻器
        EngineElm,              		//发动机
        ResistanceElm,          		//电阻
        DiodeElm,               		//二极管
        AmmeterElm,             		//电流表
        MusicChipElm,           		//音乐集成电路
        HornElm,                 		//喇叭
		PhotosensitiveResistorElm,		//光敏电阻
		CounterElm,						//数码管
        RecorderChipElm,                //录音机
        NPNElm,                         //NPN
        MicrophoneElm                   //话筒
    }

    //获取元器件
    public static CircuitElm getElm(int n) {
		if (n >= elmList.Count) {
			return null;
		}
		return elmList [n];
	}

    //获取电路结点
    public static CircuitNode getCircuitNode(int n) {
		if (n >= nodeList.Count) {
			return null;
		}
		return nodeList [n];
	}

    //分析电路
	public static void analyzeCircuit() {
		
		if (elmList.Count == 0) {						//若元器件列表为空
			return;
		}
		nodeList.Clear ();                           	//清空结点列表
		int vscount = 0;                            	//电压源数量（将导线看做电压为0的电压源）

		//步骤一：找到电压源，确定起始元器件
		Debug.Log ("步骤一");
		CircuitElm volt = null;                     	//电源源引用

		for (int i = 0; i < elmList.Count; i++) {    	//遍历元器件列表
			CircuitElm ce = getElm (i);              	//获取元器件
			if (ce.isDamaged) {
				elmList.Remove (ce);                 	//若元器件破损，在元器件列表中删除该元器件
				damageList.Add (ce);                 	//将损坏元器件添加进损坏列表
			}                  
			if (volt == null && ce.type == TYPES.VoltageElm) {   	//若找到了一个电源
				volt = ce;                                      	//为电压源赋值
			}
		}

		if (volt != null) {                                     	//若找到电压源
			CircuitNode cn = new CircuitNode ();                 	//创建结点
			cn.name = volt.getPost (0);                          	//获取电压源负极
			nodeList.Add (cn);                                   	//添加进结点列表
		} else {
			CircuitNode cn = new CircuitNode ();                 	//创建结点
			cn.name = null;                                     	//设置结点为空
			nodeList.Add (cn);                                   	//添加进结点列表
			return;
		}

		//步骤二：分配所有的结点和电压源
		Debug.Log ("步骤二");
		for (int i = 0; i < elmList.Count; i++) {               	//遍历元器件列表
			CircuitElm ce = getElm (i);                          	//获取元器件
			Debug.Log ("元器件" + i + " " + getElm (i).getPost (0));
			int ivs = ce.getVoltageSourceCount ();               	//记录电压源数量
			int posts = ce.getPostCount ();                      	//记录端点数量
			for (int j = 0; j < posts; j++) {                    	//遍历元器件上的端口
				string port = ce.getPost (j);                    	//获取元器件的端口
				int k;
				for (k = 0; k < nodeList.Count; k++) {          	//遍历结点列表
					CircuitNode cn = getCircuitNode (k);         	//获取k结点
					if (cn.name.Equals (port))
						break;            //从结点列表中找到了该结点
				}
				if (k == nodeList.Count) {                      	//结点列表中无此结点
					CircuitNode cn = new CircuitNode ();         	//创建结点
					cn.name = port;                             	//标记结点名称
					cn.links.Add (ce, j);                        	//添加进结点序列
					ce.setNode (j, nodeList.Count);              	//记录元器件的连接结点
					nodeList.Add (cn);                           	//将该结点添加到结点列表
				} else {                                         	//结点列表中有此结点
					getCircuitNode (k).links.Add (ce, j);         	//添加结点链
					ce.setNode (j, k);                           	//记录元器件的连接结点

					if (k == 0) {                               	//若结点为电源的负极
						ce.setNodeVoltage (j, 0);                	//端口j的电压为0
					}
				}
			}
			vscount += ivs;                                     	//电压源数量累加
		}
		voltageSources = new CircuitElm[vscount];               	//创建电压源数组
		vscount = 0;                                            	//电压源数量置0
		circuitNonLinear = false;                               	//电路是否是非线性的

		Debug.Log ("----------结点-----------");
		for (int i = 0; i < nodeList.Count; i++) {
			Debug.Log ("结点" + i + " : " + getCircuitNode (i).name);
		}

		Debug.Log ("---------元器件对应的结点----------");
		//for(int i = 0; i < elmList.Count; i++) {
		//    Debug.Log("元器件"+i+" : "+getElm(i).type+"  "+getElm(i).nodes[0]+"--"+ getElm(i).nodes[1]);
		//}

		//步骤三：初始电路矩阵
		Debug.Log ("步骤三");
		for (int i = 0; i < elmList.Count; i++) {               	//遍历元器件
			CircuitElm ce = getElm (i);                          	//获取元器件
			int ivs = ce.getVoltageSourceCount ();               	//获取电压源数量
			for (int j = 0; j < ivs; j++) {                      	//遍历电压源
				voltageSources [vscount] = ce;                   	//为电压源数组赋值
				ce.setVoltageSource (j, vscount++);              	//设置电压源序号
			}
		}

		int matrixSize = nodeList.Count - 1 + vscount;              //矩阵大小
		circuitMatrix = new float[matrixSize, matrixSize];         	//初始电路矩阵
		circuitRightSide = new float[matrixSize];                  	//初始电路右边矩阵
		origMatrix = new float[matrixSize, matrixSize];
		origRightSide = new float[matrixSize];
		circuitMatrixSize = circuitMatrixFullSize = matrixSize;     //初始电路矩阵大小
		circuitRowInfo = new RowInfo[matrixSize];                   //初始行信息
		circuitPermute = new int[matrixSize];                       //初始化电路交换数组
            
		for (int i = 0; i < matrixSize; i++) {
			circuitRowInfo [i] = new RowInfo ();              		//创建电路矩阵行信息对象
		}
		circuitNeedsMap = false;
		for (int i = 0; i < elmList.Count; i++) {
			CircuitElm ce = getElm (i);                      		//获取元器件
			ce.stamp ();                                     		//标记元器件,初始化电路矩阵
		}

		for (int i = 0; i < CirSim.circuitMatrixSize; i++) {
			string s = i + ": ";
			for (int j = 0; j < CirSim.circuitMatrixSize; j++) {
				s += CirSim.circuitMatrix [i, j] + " ";
			}
			s += " " + CirSim.circuitRightSide [i];
			Debug.Log (s);
		}

		//步骤四：处理图中未连接的结点
		Debug.Log ("步骤四");
		bool[] closure = new bool[nodeList.Count];
		bool[] tempclosure = new bool[nodeList.Count];
		bool changed = true;                                    	//标志位
		closure [0] = true;                                      	//从第0个结点开始
		while (changed) {
			changed = false;                                    	//标志位置false
			for (int i = 0; i < elmList.Count; i++) {            	//遍历所有元器件
				CircuitElm ce = getElm (i);                      	//获取元器件
				for (int j = 0; j < ce.getPostCount (); j++) {    	//遍历元器件的端口
					if (!closure [ce.getNode (j)]) {
						if (ce.hasGroundConnection (j))          	//若该端口连接到y阴极					
                            closure [ce.getNode (j)] = changed = true;
						continue;
					}

					for (int k = 0; k < ce.getPostCount (); k++) {	//再次遍历元器件端口
						if (j == k)
							continue;                   			//若两个端口相等则继续
						int kn = ce.getNode (k);                 	//获取端口对应的结点
						if (ce.getConnection (j, k) && !closure [kn]) { 	//若两端点相连并没有标记
							closure [kn] = true;
							changed = true;
						}
					}
				}
			}
			if (changed)
				continue;

			//连接未连接的结点
			for (int i = 0; i < nodeList.Count; i++) {
				if (!closure [i]) {
					stampResistor (0, i, 1e8f);
					closure [i] = true;
					changed = true;
					break;
				}
			}
		}

		//步骤五：短路等无效连接的判断
		Debug.Log ("步骤五");
		for (int i = 0; i < elmList.Count; i++) {
			CircuitElm ce = getElm (i);                      		//获取元器件
			//若为电压源或导线
			if ((ce.type == TYPES.VoltageElm && ce.getPostCount () == 2)
				|| (ce.type == TYPES.WireElm && ce.getPostCount () != 1)) {
				FindPathInfo fpi = new FindPathInfo (FindPathInfo.VOLTAGE, ce, ce.getNode (1));
				if (fpi.findPath (ce.getNode (0))) {         	 	//判断是否短路
					//短路
					Utils.menuListener.PopTipUI ("短  路");
					Debug.Log ("短路");
					return;
				}
			}
		}

		//步骤六：简化矩阵
		Debug.Log ("步骤六");
		for (int i = 0; i < matrixSize; i++) {      				//遍历矩阵的每一行
			int qm = -1, qp = -1;
			float qv = 0;
			RowInfo re = circuitRowInfo [i];         				//获取每一行的信息
			if (re.lsChanges || re.dropRow || re.rsChanges)
				continue;
			float rsadd = 0;

			//寻找可以删除的行
			int j;
			for (j = 0; j < matrixSize; j++) {       				//遍历矩阵的每一行
				float q = circuitMatrix [i, j];      				//获取矩阵值
				if (circuitRowInfo [j].type == RowInfo.ROW_CONST) {
					rsadd -= circuitRowInfo [j].value * q;
					continue;
				}
				if (q == 0)
					continue;               						//若矩阵值为0
				if (qp == -1) {                     				//记录第一个不为0的数值
					qp = j;                         				//记录第一个不为0的列
					qv = q;                         				//记录该矩阵值
					continue;
				}
				if (qm == -1 && q == -qv) {         				//记录与qv互为相反数的列
					qm = j;                         				//列数
					continue;
				}
				break;
			}
			if (j == matrixSize) {                  				//若遍历完了该行
				if (qp == -1) {
					//矩阵错误
					Debug.Log ("矩阵错误");
					return;
				}
				RowInfo elt = circuitRowInfo [qp];   				//获取行信息
				if (qm == -1) {
					//一行只有一个常数
					int k;
					for (k = 0; elt.type == RowInfo.ROW_EQUAL && k < 100; k++) {
						qp = elt.nodeEq;
						elt = circuitRowInfo [qp];
					}
					if (elt.type == RowInfo.ROW_EQUAL) {

						elt.type = RowInfo.ROW_NORMAL;
						continue;
					}
					if (elt.type != RowInfo.ROW_NORMAL) {
						continue;
					}
					elt.type = RowInfo.ROW_CONST;                   	//标记为常数
					elt.value = (circuitRightSide [i] + rsadd) / qv; 	//记录结点电压
					circuitRowInfo [i].dropRow = true;               	//删除该行
					i = -1;
				} else if (circuitRightSide [i] + rsadd == 0) {
					//若一行中只有两个非0的数值，并且两个数值互为相反数
					if (elt.type != RowInfo.ROW_NORMAL) {
						int qq = qm;
						qm = qp;
						qp = qq;
						elt = circuitRowInfo [qp];
						if (elt.type != RowInfo.ROW_NORMAL) {
							//交换失败
							continue;
						}
					}
					elt.type = RowInfo.ROW_EQUAL;                   	//标记行类型
					elt.nodeEq = qm;                                	//记录相等结点
					circuitRowInfo [i].dropRow = true;               	//删除标志位置true
				}
			}
		}

		//步骤七：确定新矩阵的大小
		Debug.Log ("步骤七");
		int nn = 0;
		for (int i = 0; i != matrixSize; i++) {                     	//遍历矩阵的行
			RowInfo elt = circuitRowInfo [i];                        	//获取电路行信息
			if (elt.type == RowInfo.ROW_NORMAL) {
				elt.mapCol = nn++;
				continue;
			}
			if (elt.type == RowInfo.ROW_EQUAL) {
				RowInfo e2 = null;
				for (int j = 0; j != 100; j++) {
					e2 = circuitRowInfo [elt.nodeEq];
					if (e2.type != RowInfo.ROW_EQUAL)
						break;
					if (i == e2.nodeEq)
						break;
					elt.nodeEq = e2.nodeEq;
				}
			}
			if (elt.type == RowInfo.ROW_CONST)
				elt.mapCol = -1;
		}
		for (int i = 0; i != matrixSize; i++) {
			RowInfo elt = circuitRowInfo [i];
			if (elt.type == RowInfo.ROW_EQUAL) {
				RowInfo e2 = circuitRowInfo [elt.nodeEq];
				if (e2.type == RowInfo.ROW_CONST) {
					elt.type = e2.type;
					elt.value = e2.value;
					elt.mapCol = -1;
				} else {
					elt.mapCol = e2.mapCol;
				}
			}
		}

		//步骤八：创建新的简化矩阵
		Debug.Log ("步骤八");
		Debug.Log ("nn-----------------" + nn);
		int newsize = nn;
		float[,] newmatx = new float[newsize, newsize];
		float[] newrs = new float[newsize];
		int ii = 0;
		for (int i = 0; i != matrixSize; i++) {
			RowInfo rri = circuitRowInfo [i];
			if (rri.dropRow) {
				rri.mapRow = -1;
				continue;
			}
			newrs [ii] = circuitRightSide [i];
			rri.mapRow = ii;
			//System.out.println("Row " + i + " maps to " + ii);
			for (int j = 0; j != matrixSize; j++) {
				RowInfo ri = circuitRowInfo [j];
				if (ri.type == RowInfo.ROW_CONST)
					newrs [ii] -= ri.value * circuitMatrix [i, j];
				else
					newmatx [ii, ri.mapCol] += circuitMatrix [i, j];
			}
			ii++;
		}
	
		circuitMatrix = newmatx;
		circuitRightSide = newrs;
		matrixSize = circuitMatrixSize = newsize;
		for (int i = 0; i != matrixSize; i++)
			origRightSide [i] = circuitRightSide [i];
		for (int i = 0; i != matrixSize; i++)
			for (int j = 0; j != matrixSize; j++)
				origMatrix [i, j] = circuitMatrix [i, j];
		circuitNeedsMap = true;

		Debug.Log ("--------新矩阵----------");
		for (int i = 0; i < CirSim.circuitMatrixSize; i++) {
			string s = i + ": ";
			for (int j = 0; j < CirSim.circuitMatrixSize; j++) {
				s += CirSim.circuitMatrix [i, j] + " ";
			}
			s += " " + CirSim.circuitRightSide [i];
			Debug.Log (s);
		}

		/*for (int i = 0; i != elmList.Count; i++) {
            CircuitElm ce = getElm(i);
            ce.doStep();
        }*/

		if (!lu_factor (circuitMatrix, circuitMatrixSize, circuitPermute)) {
			Debug.Log ("矩阵计算错误");
			for (int i = 0; i < elmList.Count; i++) {
				getElm (i).stop ();//停止所有元器件
			}
			return;
		}

		Debug.Log ("--------LU分解后的矩阵----------");
		for (int i = 0; i < CirSim.circuitMatrixSize; i++) {
			string s = i + ": ";
			for (int j = 0; j < CirSim.circuitMatrixSize; j++) {
				s += CirSim.circuitMatrix [i, j] + " ";
			}
			s += " " + CirSim.circuitRightSide [i];
			Debug.Log (s);
		}

		//矩阵求解
		lu_solve (circuitMatrix, circuitMatrixSize, circuitPermute, circuitRightSide);

		//为每个结点的电压赋值
		for (int i = 0; i < circuitMatrixFullSize; i++) {
			RowInfo ri = circuitRowInfo [i];         							//获取行信息
			float res = 0;
			if (ri.type == RowInfo.ROW_CONST) {
				res = ri.value;
			} else {
				res = circuitRightSide [ri.mapCol];
			}

			if (double.IsNaN (res)) {//若是未知数结束循环
				Debug.Log ("res is NaN");
				break;
			}

			if (i < nodeList.Count - 1) {
				CircuitNode cn = getCircuitNode (i + 1);                   		//获取结点
				foreach (KeyValuePair<CircuitElm, int> kv in cn.links) {    	//遍历结点上的所有端点
					kv.Key.setNodeVoltage (kv.Value, res);                   	//为结点赋予电压
					Debug.Log (kv.Key.type + "--" + kv.Value + "         " + res);
				}
			} else {
				int j = i - (nodeList.Count - 1);
				voltageSources [j].setCurrent (j, res);
			}
		}

		for (int i = 0; i != elmList.Count; i++) {
			CircuitElm ce = getElm (i);
			ce.doStep ();
		}

		if (!isNext) {
			for (int i = 0; i < elmList.Count; i++) {
				CircuitElm ce = getElm (i);                  					//获取元器件脚本
				if (ce.type == TYPES.MusicChipElm || ce.type == TYPES.RecorderChipElm) {  	//包含集成电路
					isNext = true;
					analyzeCircuit ();                       					//重新分析电路
					isNext = false;
					return;
				}
			}
		}

		//运行元器件
		for (int i = 0; i < elmList.Count; i++) {
			CircuitElm ce = getElm (i);              	//获取元器件
			ce.work ();                              	//元器件工作
		}

	}

    //矩阵计算
    public static void calMatrix() {
        if (!lu_factor(circuitMatrix, circuitMatrixSize, circuitPermute)) {
            return;
        }

        Debug.Log("--------LU分解后的矩阵----------");
        for (int i = 0; i < CirSim.circuitMatrixSize; i++) {
            string s = i + ": ";
            for (int j = 0; j < CirSim.circuitMatrixSize; j++) {
                s += CirSim.circuitMatrix[i, j] + " ";
            }
            s += " " + CirSim.circuitRightSide[i];
            Debug.Log(s);
        }

        //矩阵求解
        lu_solve(circuitMatrix, circuitMatrixSize, circuitPermute, circuitRightSide);

        //为每个结点的电压赋值
        for (int i = 0; i < circuitMatrixFullSize; i++) {
            RowInfo ri = circuitRowInfo[i];         	//获取行信息
            float res = 0;
            if (ri.type == RowInfo.ROW_CONST) {
                res = ri.value;
            }
            else {
                res = circuitRightSide[ri.mapCol];
            }

            if (double.IsNaN(res)) {//若是未知数结束循环
                Debug.Log("res is NaN");
                break;
            }

            if (i < nodeList.Count - 1) {
                CircuitNode cn = getCircuitNode(i + 1);                    	 		//获取结点
                foreach (KeyValuePair<CircuitElm, int> kv in cn.links) {    		//遍历结点上的所有端点
                    kv.Key.setNodeVoltage(kv.Value, res);                   		//为结点赋予电压
                    Debug.Log(kv.Key.type + "--" + kv.Value + "         " + res);
                }
            }
            else {
                int j = i - (nodeList.Count - 1);
                voltageSources[j].setCurrent(j, res);
            }
        }
    }

    //标记电阻
    public static void stampResistor(int n1,int n2, float r) {
        float r0 = 1 / r;
        stampMatrix(n1, n1, r0);
        stampMatrix(n2, n2, r0);
        stampMatrix(n1, n2, -r0);
        stampMatrix(n2, n1, -r0);
    }

    //标记独立电压源
    public static void stampVoltageSource(int n1,int n2,int vs, float v) {
        //开始标记电压源
        int vn = nodeList.Count + vs;						//结点数量+电压源数量
        stampMatrix(vn, n1, -1);
        stampMatrix(vn, n2, 1);
        stampRightSide(vn, v);
        stampMatrix(n1, vn, 1);
        stampMatrix(n2, vn, -1);
    }

    //标记电流源
    public static void stampCurrentSource(int n1, int n2, float i) {
        stampRightSide(n1, -i);
        stampRightSide(n2, i);
    }

    //标记电导
    public static void stampConductance(int n1, int n2, float r0) {
        stampMatrix(n1, n1, r0);
        stampMatrix(n2, n2, r0);
        stampMatrix(n1, n2, -r0);
        stampMatrix(n2, n1, -r0);
    }

    //标记右边矩阵
    public static void stampRightSide(int i, float x) {
        if (i > 0) {
            if (circuitNeedsMap) {
                i = circuitRowInfo[i - 1].mapRow;
            }else {
                i--;
            }
            circuitRightSide[i] += x;
        }
    }

    //标记非线性
    public static void stampNonLinear(int i) {
        if (i > 0) circuitRowInfo[i - 1].lsChanges = true;
    }

    //标记矩阵
    public static void stampMatrix(int i,int j, float x) {
        if(i>0 && j > 0) {
            if (circuitNeedsMap) {
                i = circuitRowInfo[i - 1].mapRow;
                RowInfo ri = circuitRowInfo[j - 1];
                if (ri.type == RowInfo.ROW_CONST) {
                    circuitRightSide[i] -= x * ri.value;
                    return;
                }
                j = ri.mapCol;
            }
            else {
                i--;
                j--;
            }
            circuitMatrix[i,j] += x;//为矩阵赋值
        }
    }

    //LU分解
    static bool lu_factor(float[,] a, int n, int[] ipvt) {
        float[] scaleFactors;
        int i, j, k;

        scaleFactors = new float[n];

        for (i = 0; i != n; i++) {
            float largest = 0;
            for (j = 0; j != n; j++) {
                float x = Mathf.Abs((float)a[i,j]);
                if (x > largest)
                    largest = x;
            }
            // if all zeros, it's a singular matrix
            if (largest == 0)
                return false;
            scaleFactors[i] = 1.0f / largest;
        }

        // use Crout's method; loop through the columns
        for (j = 0; j != n; j++) {

            // calculate upper triangular elements for this column
            for (i = 0; i != j; i++) {
                float q = a[i,j];
                for (k = 0; k != i; k++)
                    q -= a[i,k] * a[k,j];
                a[i,j] = q;
            }

            // calculate lower triangular elements for this column
            float largest = 0;
            int largestRow = -1;
            for (i = j; i != n; i++) {
                float q = a[i,j];
                for (k = 0; k != j; k++)
                    q -= a[i,k] * a[k,j];
                a[i,j] = q;
                float x = Mathf.Abs((float)q);
                if (x >= largest) {
                    largest = x;
                    largestRow = i;
                }
            }

            // pivoting
            if (j != largestRow) {
                float x;
                for (k = 0; k != n; k++) {
                    x = a[largestRow,k];
                    a[largestRow,k] = a[j,k];
                    a[j,k] = x;
                }
                scaleFactors[largestRow] = scaleFactors[j];
            }

            // keep track of row interchanges
            ipvt[j] = largestRow;

            // avoid zeros
            if (a[j,j] == 0.0) {
                a[j,j] = 1e-18f;
            }

            if (j != n - 1) {
                float mult = 1.0f / a[j,j];
                for (i = j + 1; i != n; i++)
                    a[i,j] *= mult;
            }
        }
        return true;
    }

    //矩阵求解
    static void lu_solve(float[,] a, int n, int[] ipvt, float[] b) {
        int i;

        // find first nonzero b element
        for (i = 0; i != n; i++) {
            int row = ipvt[i];

            float swap = b[row];
            b[row] = b[i];
            b[i] = swap;
            if (swap != 0)
                break;
        }

        int bi = i++;
        for (; i < n; i++) {
            int row = ipvt[i];
            int j;
            float tot = b[row];

            b[row] = b[i];
            // forward substitution using the lower triangular matrix
            for (j = bi; j < i; j++)
                tot -= a[i,j] * b[j];
            b[i] = tot;
        }
        for (i = n - 1; i >= 0; i--) {
            float tot = b[i];

            // back-substitution using the upper triangular matrix
            int j;
            for (j = i + 1; j != n; j++)
                tot -= a[i,j] * b[j];
            b[i] = tot / a[i,i];
        }
    }

    //寻找路径类
    class FindPathInfo {
        public const int INDUCT = 1;
        public const int VOLTAGE = 2;
        public const int SHORT = 3;
        public const int CAP_V = 4;
        bool[] used;            //标记数组
        int dest;               //目的节点
        CircuitElm firstElm;    //首个元器件
        int type;               //类型

        //构造器进行初始化
        public FindPathInfo(int t, CircuitElm e, int d) {
            dest = d;
            type = t;
            firstElm = e;
            used = new bool[nodeList.Count];
        }

        public bool findPath(int n1) {
            return findPath(n1, -1);
        }

        public bool findPath(int n1,int depth) {
            if (n1 == dest) return true;        //找到目的元器件端口
            if (depth-- == 0) return false;     //超过搜索深度
            if (used[n1]) return false;         //已经遍历过此结点
            used[n1] = true;                    //标志位置true
            for(int i = 0; i < elmList.Count; i++) {
                CircuitElm ce = getElm(i);      //获取元器件
                if (ce.Equals(firstElm)) continue;   //若是第一个元器件 pass

                if (type == VOLTAGE) {          //若既不是导线也不是电源则继续
                    if(!(ce.isWire() || ce.type == TYPES.VoltageElm)) {
                        continue;
                    }
                }

                //若当前元器件没有与此结点相连
                int j;
                for(j = 0; j < ce.getPostCount(); j++) {
                    if (ce.getNode(j) == n1) {
                        break;
                    }
                }
                if (j == ce.getPostCount()) continue;

                for(int k = 0; k < ce.getPostCount(); k++) {
                    if (j == k) continue;
                    if(ce.getConnection(j,k) && findPath(ce.getNode(k), depth)) {
                        used[n1] = false;
                        return true;
                    }
                }
            }
            used[n1] = false;
            return false;
        }
    }

}
