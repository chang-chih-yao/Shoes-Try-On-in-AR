using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class TCP : MonoBehaviour
{
    string editString = "hello wolrd"; //编辑框文字

    public Text Recv;
    public RawImage my_img;
    public GameObject R_shoe;
    public GameObject L_shoe;
    public GameObject R_shoe_1;
    public GameObject L_shoe_1;
    public GameObject R_shoe_2;
    public GameObject L_shoe_2;
    public GameObject R_shoe_3;
    public GameObject L_shoe_3;
    public GameObject R_shoe_4;
    public GameObject L_shoe_4;

    public GameObject Zoom;
    private float[] shoe_arr = new float[] { -195f, -93f, 7.5f, 108f, 206f };

    public float shoe_scale = 350f;

    int R_deg = 0;
    int L_deg = 0;
    int R_leg_len = 0;
    int L_leg_len = 0;

    Socket serverSocket; //服务器端socket
    IPAddress ip; //主机ip
    IPEndPoint ipEnd;
    string recvStr; //接收的字符串
    string recv_keypoint_str;
    string sendStr; //发送的字符串
    byte[] recvData = new byte[1024]; //接收的数据，必须为字节
    byte[] sendData = new byte[1024]; //发送的数据，必须为字节
    byte[] recv_keypoint = new byte[1024];
    byte[] recv_img;
    int recvLen; //接收的数据长度
    int flag = 0;

    float R_yRotation = 5.0f;
    float R_center_x = 0f;
    float R_center_y = 0f;

    float L_yRotation = 5.0f;
    float L_center_x = 0f;
    float L_center_y = 0f;

    Thread connectThread; //连接线程

    //初始化
    void InitSocket()
    {
        //定义服务器的IP和端口，端口与服务器对应
        ip = IPAddress.Parse("127.0.0.1"); //可以是局域网或互联网ip，此处是本机
        ipEnd = new IPEndPoint(ip, 9000);


        //开启一个线程连接，必须的，否则主线程卡死
        connectThread = new Thread(new ThreadStart(SocketReceive));
        connectThread.Start();
    }

    void SocketConnet()
    {
        if (serverSocket != null)
            serverSocket.Close();
        //定义套接字类型,必须在子线程中定义
        serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        print("ready to connect");
        //连接
        serverSocket.Connect(ipEnd);

        //输出初次连接收到的字符串
        //recvLen = serverSocket.Receive(recvData);
        //recvStr = Encoding.ASCII.GetString(recvData, 0, recvLen);
        //print(recvStr);
    }

    void SocketSend(string sendStr)
    {
        //清空发送缓存
        sendData = new byte[1024];
        //数据类型转换
        sendData = Encoding.ASCII.GetBytes(sendStr);
        //发送
        serverSocket.Send(sendData, sendData.Length, SocketFlags.None);
    }

    void SocketReceive()
    {
        SocketConnet();
        //不断接收服务器发来的数据
        while (true)
        {
            SocketSend("OK_1");

            recvData = new byte[1024];
            recvLen = serverSocket.Receive(recvData);
            if (recvLen == 0)
            {
                SocketConnet();
                continue;
            }

            SocketSend("OK_2");

            recvStr = Encoding.ASCII.GetString(recvData, 0, recvLen);
            int img_size = Convert.ToInt32(recvStr);
            //int cou = 0;
            flag = 0;
            recv_img = new byte[img_size];
            int recv_img_len = serverSocket.Receive(recv_img);
            if (img_size != recv_img_len)
            {
                print("error");
            }
            else
            {
                flag = 1;

                SocketSend("OK_3");

                recv_keypoint = new byte[1024];
                int recv_keypoint_len = serverSocket.Receive(recv_keypoint);
                recv_keypoint_str = Encoding.ASCII.GetString(recv_keypoint, 0, recv_keypoint_len);
                //print(recv_keypoint_str);
                string[] arr = recv_keypoint_str.Split(',');
                R_deg = int.Parse(arr[50]);
                L_deg = int.Parse(arr[51]);
                R_leg_len = int.Parse(arr[52]);
                L_leg_len = int.Parse(arr[53]);
                
                if (int.Parse(arr[54]) != -1)
                {
                    if (change_shoe.index != int.Parse(arr[54]))
                    {
                        change_shoe.animation_flag = true;
                        //print("Change");
                    }

                    change_shoe.index = int.Parse(arr[54]);
                }
                


                if (R_deg != 999)
                {
                    R_yRotation = R_deg;


                    int R_ankel_x = int.Parse(arr[22]);
                    int R_ankel_y = int.Parse(arr[23]);
                    int R_Toe_x = int.Parse(arr[44]);
                    int R_Toe_y = int.Parse(arr[45]);
                    R_center_x = (R_ankel_x + R_Toe_x) / 2f;
                    R_center_y = (R_ankel_y + R_Toe_y) / 2f;
                }
                if (L_deg != 999)
                {
                    L_yRotation = L_deg;

                    int L_ankel_x = int.Parse(arr[28]);
                    int L_ankel_y = int.Parse(arr[29]);
                    int L_Toe_x = int.Parse(arr[38]);
                    int L_Toe_y = int.Parse(arr[39]);
                    L_center_x = (L_ankel_x + L_Toe_x) / 2f;
                    L_center_y = (L_ankel_y + L_Toe_y) / 2f;
                }
            }
        }
    }

    void SocketQuit()
    {
        //关闭线程
        if (connectThread != null)
        {
            connectThread.Interrupt();
            connectThread.Abort();
        }
        //最后关闭服务器
        if (serverSocket != null)
            serverSocket.Close();
        print("diconnect");
    }

    // Use this for initialization
    void Start()
    {
        my_img.texture = new Texture2D(960, 540, TextureFormat.RGB24, false);
        InitSocket();

        R_yRotation = 0f;
        //R_shoe.transform.eulerAngles = new Vector3(-110f, 0f, R_yRotation);

        L_yRotation = 0f;
        //L_shoe.transform.eulerAngles = new Vector3(-110f, 0f, L_yRotation);

        //Zoom.GetComponent<Transform>().localPosition = new Vector3(shoe_arr[change_shoe.index], 132f, 5f);

    }

    /*void OnGUI()
    {
        editString = GUI.TextField(new Rect(10, 10, 100, 20), editString);
        if (GUI.Button(new Rect(10, 30, 60, 20), "send"))
            SocketSend(editString);
    }*/

    // Update is called once per frame
    void Update()
    {
        //Recv.text = recvStr;
        Recv.text = R_leg_len.ToString();

        if (change_shoe.animation_flag)
        {
            //Zoom.GetComponent<Transform>().localPosition = new Vector3(shoe_arr[index], 132f, 5f);
            Zoom.GetComponent<Transform>().localPosition = Vector3.Lerp(Zoom.GetComponent<Transform>().localPosition, new Vector3(shoe_arr[change_shoe.index], 132f, 5f), 0.25f);

            if (Mathf.Abs(shoe_arr[change_shoe.index] - Zoom.GetComponent<Transform>().localPosition.x) < 1f)
                change_shoe.animation_flag = false;
        }

        if (flag == 1)
        {
            var texture = my_img.texture as Texture2D;
            texture.LoadImage(recv_img);
            texture.Apply();

            if (R_deg != 999)
            {
                if (change_shoe.index == 0)
                {
                    R_shoe.SetActive(true);
                    R_shoe_1.SetActive(false);
                    R_shoe_2.SetActive(false);
                    R_shoe_3.SetActive(false);
                    R_shoe_4.SetActive(false);

                    //R_shoe.transform.eulerAngles = new Vector3(-10f, R_yRotation, 0f);
                    R_shoe.transform.eulerAngles = new Vector3(-20f, 0f, 0f);
                    R_shoe.transform.Rotate(new Vector3(0, R_yRotation, 0), Space.Self);
                    R_shoe.GetComponent<Transform>().localPosition = new Vector3(R_center_x - 270f, -(R_center_y - 270f), 190f);
                    R_shoe.transform.localScale = new Vector3(R_leg_len / shoe_scale, R_leg_len / shoe_scale, R_leg_len / shoe_scale);
                }
                else if (change_shoe.index == 1)
                {
                    R_shoe.SetActive(false);
                    R_shoe_1.SetActive(true);
                    R_shoe_2.SetActive(false);
                    R_shoe_3.SetActive(false);
                    R_shoe_4.SetActive(false);

                    R_shoe_1.transform.eulerAngles = new Vector3(-20f, 0f, 0f);
                    R_shoe_1.transform.Rotate(new Vector3(0, R_yRotation, 0), Space.Self);
                    R_shoe_1.GetComponent<Transform>().localPosition = new Vector3(R_center_x - 270f, -(R_center_y - 270f), 185f);
                    R_shoe_1.transform.localScale = new Vector3(R_leg_len / shoe_scale, R_leg_len / shoe_scale, R_leg_len / shoe_scale);
                }
                else if (change_shoe.index == 2)
                {
                    R_shoe.SetActive(false);
                    R_shoe_1.SetActive(false);
                    R_shoe_2.SetActive(true);
                    R_shoe_3.SetActive(false);
                    R_shoe_4.SetActive(false);

                    R_shoe_2.transform.eulerAngles = new Vector3(-20f, 0f, 0f);
                    R_shoe_2.transform.Rotate(new Vector3(0, R_yRotation, 0), Space.Self);
                    R_shoe_2.GetComponent<Transform>().localPosition = new Vector3(R_center_x - 270f, -(R_center_y - 270f), 195f);
                    R_shoe_2.transform.localScale = new Vector3(R_leg_len / shoe_scale, R_leg_len / shoe_scale, R_leg_len / shoe_scale);
                }
                else if (change_shoe.index == 3)
                {
                    R_shoe.SetActive(false);
                    R_shoe_1.SetActive(false);
                    R_shoe_2.SetActive(false);
                    R_shoe_3.SetActive(true);
                    R_shoe_4.SetActive(false);

                    R_shoe_3.transform.eulerAngles = new Vector3(-20f, 0f, 0f);
                    R_shoe_3.transform.Rotate(new Vector3(0, R_yRotation, 0), Space.Self);
                    R_shoe_3.GetComponent<Transform>().localPosition = new Vector3(R_center_x - 270f, -(R_center_y - 270f), 190f);
                    R_shoe_3.transform.localScale = new Vector3(R_leg_len / shoe_scale, R_leg_len / shoe_scale, R_leg_len / shoe_scale);
                }
                else if (change_shoe.index == 4)
                {
                    R_shoe.SetActive(false);
                    R_shoe_1.SetActive(false);
                    R_shoe_2.SetActive(false);
                    R_shoe_3.SetActive(false);
                    R_shoe_4.SetActive(true);

                    R_shoe_4.transform.eulerAngles = new Vector3(-20f, 0f, 0f);
                    R_shoe_4.transform.Rotate(new Vector3(0, R_yRotation, 0), Space.Self);
                    R_shoe_4.GetComponent<Transform>().localPosition = new Vector3(R_center_x - 270f, -(R_center_y - 270f), 190f);
                    R_shoe_4.transform.localScale = new Vector3(R_leg_len / shoe_scale, R_leg_len / shoe_scale, R_leg_len / shoe_scale);
                }
            }
            if (L_deg != 999)
            {
                //print(change_shoe.index);
                if (change_shoe.index == 0)
                {
                    L_shoe.SetActive(true);
                    L_shoe_1.SetActive(false);
                    L_shoe_2.SetActive(false);
                    L_shoe_3.SetActive(false);
                    L_shoe_4.SetActive(false);

                    //L_shoe.transform.eulerAngles = new Vector3(-10f, L_yRotation, 0f);
                    L_shoe.transform.eulerAngles = new Vector3(-20f, 0f, 0f);
                    L_shoe.transform.Rotate(new Vector3(0, L_yRotation, 0), Space.Self);
                    L_shoe.GetComponent<Transform>().localPosition = new Vector3(L_center_x - 270f, -(L_center_y - 270f), 190f);
                    L_shoe.transform.localScale = new Vector3(L_leg_len / shoe_scale, L_leg_len / shoe_scale, L_leg_len / shoe_scale);
                }
                else if (change_shoe.index == 1)
                {
                    L_shoe.SetActive(false);
                    L_shoe_1.SetActive(true);
                    L_shoe_2.SetActive(false);
                    L_shoe_3.SetActive(false);
                    L_shoe_4.SetActive(false);

                    L_shoe_1.transform.eulerAngles = new Vector3(-20f, 0f, 0f);
                    L_shoe_1.transform.Rotate(new Vector3(0, L_yRotation, 0), Space.Self);
                    L_shoe_1.GetComponent<Transform>().localPosition = new Vector3(L_center_x - 270f, -(L_center_y - 270f), 190f);
                    L_shoe_1.transform.localScale = new Vector3(L_leg_len / shoe_scale, L_leg_len / shoe_scale, L_leg_len / shoe_scale);
                }
                else if (change_shoe.index == 2)
                {
                    L_shoe.SetActive(false);
                    L_shoe_1.SetActive(false);
                    L_shoe_2.SetActive(true);
                    L_shoe_3.SetActive(false);
                    L_shoe_4.SetActive(false);

                    L_shoe_2.transform.eulerAngles = new Vector3(-20f, 0f, 0f);
                    L_shoe_2.transform.Rotate(new Vector3(0, L_yRotation, 0), Space.Self);
                    L_shoe_2.GetComponent<Transform>().localPosition = new Vector3(L_center_x - 270f, -(L_center_y - 270f), 195f);
                    L_shoe_2.transform.localScale = new Vector3(L_leg_len / shoe_scale, L_leg_len / shoe_scale, L_leg_len / shoe_scale);
                }
                else if (change_shoe.index == 3)
                {
                    L_shoe.SetActive(false);
                    L_shoe_1.SetActive(false);
                    L_shoe_2.SetActive(false);
                    L_shoe_3.SetActive(true);
                    L_shoe_4.SetActive(false);

                    L_shoe_3.transform.eulerAngles = new Vector3(-20f, 0f, 0f);
                    L_shoe_3.transform.Rotate(new Vector3(0, L_yRotation, 0), Space.Self);
                    L_shoe_3.GetComponent<Transform>().localPosition = new Vector3(L_center_x - 270f, -(L_center_y - 270f), 190f);
                    L_shoe_3.transform.localScale = new Vector3(L_leg_len / shoe_scale, L_leg_len / shoe_scale, L_leg_len / shoe_scale);
                }
                else if (change_shoe.index == 4)
                {
                    L_shoe.SetActive(false);
                    L_shoe_1.SetActive(false);
                    L_shoe_2.SetActive(false);
                    L_shoe_3.SetActive(false);
                    L_shoe_4.SetActive(true);

                    L_shoe_4.transform.eulerAngles = new Vector3(-20f, 0f, 0f);
                    L_shoe_4.transform.Rotate(new Vector3(0, L_yRotation, 0), Space.Self);
                    L_shoe_4.GetComponent<Transform>().localPosition = new Vector3(L_center_x - 270f, -(L_center_y - 270f), 190f);
                    L_shoe_4.transform.localScale = new Vector3(L_leg_len / shoe_scale, L_leg_len / shoe_scale, L_leg_len / shoe_scale);
                }
            }
        }
    }

    //程序退出则关闭连接
    void OnApplicationQuit()
    {
        SocketQuit();
    }
}
