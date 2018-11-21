using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//电路结点类
public class CircuitNode : MonoBehaviour {

    public string name;										//结点的名称
    public Dictionary<CircuitElm, int> links;				//结点列表（int-元器件的端口 elm-元器件）

    public CircuitNode() {
		links = new Dictionary<CircuitElm, int> ();
	}

}
