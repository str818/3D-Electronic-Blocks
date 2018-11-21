using UnityEngine;
using System.Collections;

public class Point : MonoBehaviour {

    public int port;//元器件的端口号

    public void OnTriggerEnter(Collider collider) {
        if (collider.tag == "button") {
            port = int.Parse(collider.gameObject.name);//记录碰撞的端口号
        }
    }
}
