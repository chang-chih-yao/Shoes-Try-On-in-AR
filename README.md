# Shoes-Try-On-in-AR

- 開發時間：2019/08/01 ~ now
- 開發平台：Win 10, GTX 2070, i7-8700k
- 開發工具：Python 3.7, Unity 2019.2.1, CMake 3.16.0
- 開發環境：CUDA 10.0, cuDNN 7.5.0

***

本系統採用Server跟Client的方式互相傳遞訊息。  
Server端(Python)運行OpenPose不斷的把骨架資訊以及影像傳遞給Client端(Unity)，Client端接收到骨架資訊以及影像後，把影像顯示在一個平面上，並且把鞋子的3D模型套到腳掌上，讓使用者站在鏡頭前面就可以自由試鞋，還能一鍵換成其他雙鞋子試穿。

***

## Demo

[DEMO VIDEO-1](https://youtu.be/ThBjJR3uZzg)

[DEMO VIDEO-2](https://youtu.be/yD5ZPH573LY)

***

## Quick Start

1. Install OpenPose Python API. Check [here](./OpenPose(Server)/README.md)
2. Connect the webcam to your computer through USB.
3. Run the Server. Navigate to `openpose(clone root)/build(build name)/examples/tutorial_api_python` folder, run `python webcam.py`.
4. Run the Untiy project.