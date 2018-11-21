using UnityEngine;
using System.Collections;

//矩阵行信息
public class RowInfo : MonoBehaviour {
    public const int ROW_NORMAL = 0;
    public const int ROW_CONST = 1;
    public const int ROW_EQUAL = 2;
    public int nodeEq, type, mapCol, mapRow;
    public float value;
    public bool rsChanges;
    public bool lsChanges;
    public bool dropRow;

    public RowInfo() {
        type = ROW_NORMAL;
    }
}
