using UnityEngine;
using System.IO;
using System.Text;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

//下载资源包列表
public class ListEvent : MonoBehaviour {

    private string localPath = Application.persistentDataPath + "/Ex";                  //本地实验资源包位置
    private string loadPath = Utils.IP + "/dianzijimu";									//下载实验资源包位置

    //点击资源包，下载并解压
    public void OnClick() {
        string name = transform.name;		                                            //获取资源包名称
        Debug.Log(name);																//打印资源包名称
        foreach (string[] ss in Utils.loadEXList) {                                     //遍历服务器上的资源包列表
            if (ss[0].Equals(name)) {                                                   //若点击按钮名称为该资源包
                string constent = ss[0];                                                //内容
                for (int i = 1; i < ss.Length; i++) {                                   //读取资源包中的内容
                    constent += " " + ss[i];
                }
                writeFile(constent);                                                    //将内容写到本地txt文件
                Utils.localEXList.Add(ss);                                              //向本地列表中加入记录
            }
        }
		transform.parent.parent.FindChild ("Panel").gameObject.SetActive(true);
		transform.parent.parent.FindChild ("Button").gameObject.SetActive(false);
        StartCoroutine(downAsFile(loadPath + "/" + name + ".zip", 
			Application.persistentDataPath + "/EX/" + name + ".zip"));					//下载实验资源包
    }

    //向本地txt中写入数据
    public void writeFile(string content) {
        //StreamWriter一个参数默认覆盖
        //StreamWriter第二个参数为false覆盖现有文件，为true则把文本追加到文件末尾
        using (System.IO.StreamWriter file = new System.IO.StreamWriter(
			localPath + "/EX_List.txt", true)) {
            file.WriteLine(content);                                                    //直接追加文件末尾，换行
        }
    }

    //从服务器上下载文件并保存到本地
    IEnumerator downAsFile(string path, string filename) {                              //path-服务器上的下载路径 filename-下载到的本地路径 
        WWW www = new WWW(path);                                                        //定义WWW类
        yield return www;                                                               //返回www
        if (www.error == null) {                                                        //如果解压完成
            byte[] stream = www.bytes;                                                  //定义字节数组
            if (!File.Exists(filename)) {
                FileStream fs = new FileStream(filename, FileMode.CreateNew);           //定义FileStream
                BinaryWriter w = new BinaryWriter(fs);                                  //定义BinaryWriter
                w.Write(stream);                                                        //写入字节数组
                fs.Close();                                                             //关闭FileStream
                w.Close();                                                              //关闭BinaryWriter										
            }
            Utils.UnZipFile(localPath + "/" + transform.name + ".zip", localPath);      //解压下载的资源包
			transform.parent.parent.FindChild ("Over").gameObject.SetActive(true);		//打开完成提示
			Debug.Log("结束下载资源包");
			gameObject.GetComponent<Image> ().color = new Color (0.67f, 0.55f, 0.55f, 1.0f);
			transform.FindChild ("Image").GetComponent<Image>().color = new Color (0.67f, 0.55f, 0.55f, 1.0f);
			transform.FindChild ("Text").GetComponent<Text> ().color = new Color (0.56f, 0.49f, 0.49f, 1.0f);
			transform.FindChild ("Over").gameObject.SetActive (true);
			gameObject.GetComponent<Button> ().enabled = false;
            yield return new WaitForSeconds(1.0f);                                      //等待1秒
        }
    }

}
