# From Python
# It requires OpenCV installed for Python
import sys
import cv2
import os
from sys import platform
import argparse
import time
import socket
import threading
import math
import numpy as np

def get_keypoint_string(my_arr):
    my_str = str(int(my_arr[0][0])) + ',' + str(int(my_arr[0][1]))
    for i in range(1,25):
        my_str += ',' + str(int(my_arr[i][0])) + ',' + str(int(my_arr[i][1]))
    return my_str

try:
    # Import Openpose (Windows/Ubuntu/OSX)
    dir_path = os.path.dirname(os.path.realpath(__file__))
    try:
        # Windows Import
        if platform == "win32":
            # Change these variables to point to the correct folder (Release/x64 etc.)
            sys.path.append(dir_path + '/../../python/openpose/Release');
            os.environ['PATH']  = os.environ['PATH'] + ';' + dir_path + '/../../x64/Release;' +  dir_path + '/../../bin;'
            import pyopenpose as op
        else:
            # Change these variables to point to the correct folder (Release/x64 etc.)
            sys.path.append('../../python');
            # If you run `make install` (default path is `/usr/local/python` for Ubuntu), you can also access the OpenPose/python module from there. This will install OpenPose and the python library at your desired installation path. Ensure that this is in your python path in order to use it.
            # sys.path.append('/usr/local/python')
            from openpose import pyopenpose as op
    except ImportError as e:
        print('Error: OpenPose library could not be found. Did you enable `BUILD_PYTHON` in CMake and have this Python script in the right folder?')
        raise e

    # Flags
    parser = argparse.ArgumentParser()
    parser.add_argument("--image_dir", default="../../../examples/media/", help="Process a directory of images. Read all standard formats (jpg, png, bmp, etc.).")
    parser.add_argument("--no_display", default=False, help="Enable to disable the visual display.")
    args = parser.parse_known_args()

    # Custom Params (refer to include/openpose/flags.hpp for more parameters)
    params = dict()
    params["model_folder"] = "../../../models/"

    # Add others in path?
    for i in range(0, len(args[1])):
        curr_item = args[1][i]
        if i != len(args[1])-1: next_item = args[1][i+1]
        else: next_item = "1"
        if "--" in curr_item and "--" in next_item:
            key = curr_item.replace('-','')
            if key not in params:  params[key] = "1"
        elif "--" in curr_item and "--" not in next_item:
            key = curr_item.replace('-','')
            if key not in params: params[key] = next_item

    # Construct it from system arguments
    # op.init_argv(args[1])
    # oppython = op.OpenposePython()


    ##################### Kalman filter ##################
    '''
    它有3个输入参数
    dynam_params  ：状态空间的维数，这里为4
    measure_param ：测量值的维数，这里也为2
    control_params：控制向量的维数，默认为0。由于这里该模型中并没有控制变量，因此也为0。
    kalman.processNoiseCov    ：为模型系统的噪声，噪声越大，预测结果越不稳定，越容易接近模型系统预测值，且单步变化越大，相反，若噪声小，则预测结果与上个计算结果相差不大。
    kalman.measurementNoiseCov：为测量系统的协方差矩阵，方差越小，预测结果越接近测量值
    '''
    kalman_RAnkle = cv2.KalmanFilter(4,2)
    kalman_RAnkle.measurementMatrix = np.array([[1,0,0,0],[0,1,0,0]],np.float32)
    kalman_RAnkle.transitionMatrix = np.array([[1,0,1,0],[0,1,0,1],[0,0,1,0],[0,0,0,1]], np.float32)
    kalman_RAnkle.processNoiseCov = np.array([[1,0,0,0],[0,1,0,0],[0,0,1,0],[0,0,0,1]], np.float32) * 1e-4
    kalman_RAnkle.measurementNoiseCov = np.array([[1,0],[0,1]], np.float32) * 0.005
    kalman_RAnkle.errorCovPost = np.array([[1,0],[0,1]], np.float32) * 1

    kalman_RToe = cv2.KalmanFilter(4,2)
    kalman_RToe.measurementMatrix = np.array([[1,0,0,0],[0,1,0,0]],np.float32)
    kalman_RToe.transitionMatrix = np.array([[1,0,1,0],[0,1,0,1],[0,0,1,0],[0,0,0,1]], np.float32)
    kalman_RToe.processNoiseCov = np.array([[1,0,0,0],[0,1,0,0],[0,0,1,0],[0,0,0,1]], np.float32) * 1e-4
    kalman_RToe.measurementNoiseCov = np.array([[1,0],[0,1]], np.float32) * 0.005
    kalman_RToe.errorCovPost = np.array([[1,0],[0,1]], np.float32) * 1

    ori_RAnkle = np.array([[0],[0]],np.float32)
    pre_RAnkle = np.array([[0],[0]],np.float32)
    ori_RToe = np.array([[0],[0]],np.float32)
    pre_RToe = np.array([[0],[0]],np.float32)

    # Starting OpenPose
    opWrapper = op.WrapperPython()
    opWrapper.configure(params)
    opWrapper.start()

    while(True):

        start = time.time()

        cap = cv2.VideoCapture(0)
        cap.set(cv2.CAP_PROP_FRAME_WIDTH, 960)
        cap.set(cv2.CAP_PROP_FRAME_HEIGHT, 540)
        ret, frame = cap.read()

        print(frame.shape)
        if ret == False:
            print('Cannot find camera!')
            break
        elif frame.shape[0] != 540 or frame.shape[1] != 960:
            print('Wrong frame width or height! (need 960 for width and 540 for height)')
            print(frame.shape[0])
            break

        ################################### Socket #######################################
        HOST = '127.0.0.1'
        PORT = 9000

        sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)    # tcp
        sock.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)  # reuse tcp
        sock.bind((HOST, PORT))
        sock.listen(1)
    
        print('Wait for connection...')
        (client, adr) = sock.accept()
        print("Client Info: ", client, adr)

        fps_time = 1.0
        while(True):
            try:
                ret, frame = cap.read()
                #print(frame.shape)
                result, encimg = cv2.imencode('test.jpg', frame, [int(cv2.IMWRITE_JPEG_QUALITY), 70])

                temp = client.recv(1024)
                #print(temp)
                client.send(str(len(encimg.flatten())).encode('utf-8'))  # 告訴server image size有多大
                temp = client.recv(1024)
                #print(temp)
                client.send(encimg)
                temp = client.recv(1024)
                #print(temp)

                frame_crop = frame[:, 210:750, :].copy()

                datum = op.Datum()
                datum.cvInputData = frame_crop
                opWrapper.emplaceAndPop([datum])

                cv2_img = datum.cvOutputData.copy()
                cv2.putText(cv2_img, "FPS: %f" % (1.0/(time.time() - fps_time)), (30, 30),  cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 255, 0), 2)
                cv2.imshow('frame', cv2_img)
                cv2.waitKey(1)
                fps_time = time.time()

                #print("Body keypoints: \n" + str(datum.poseKeypoints))
                #print(datum.poseKeypoints.dtype)               # float32
                #print(datum.poseKeypoints.shape)               # (people_num, 25, 3)

                if (len(datum.poseKeypoints.shape) == 3):       # detect human
                    key_points = datum.poseKeypoints[0]

                    R_deg = '999'
                    L_deg = '999'

                    R_ankel_x = key_points[11][0]
                    R_ankel_y = key_points[11][1]
                    R_toe_x = key_points[22][0]
                    R_toe_y = key_points[22][1]

                    L_ankel_x = key_points[14][0]
                    L_ankel_y = key_points[14][1]
                    L_toe_x = key_points[19][0]
                    L_toe_y = key_points[19][1]


                    R_hip_x = key_points[9][0]
                    R_hip_y = key_points[9][1]
                    R_knee_x = key_points[10][0]
                    R_knee_y = key_points[10][1]

                    L_hip_x = key_points[12][0]
                    L_hip_y = key_points[12][1]
                    L_knee_x = key_points[13][0]
                    L_knee_y = key_points[13][1]
                    
                    R_leg_len = 0
                    L_leg_len = 0

                    ####################### Kalman filter #######################
                    # if(key_points[11][0] != 0 or key_points[11][1] != 0):
                    #     ori_RAnkle = np.array([[key_points[11][0]],[key_points[11][1]]], np.float32)
                    #     kalman_RAnkle.correct(ori_RAnkle)
                    #     pre_RAnkle = kalman_RAnkle.predict()
                    
                    # if(key_points[22][0] != 0 or key_points[22][1] != 0):
                    #     ori_RToe = np.array([[key_points[22][0]],[key_points[22][1]]], np.float32)
                    #     kalman_RToe.correct(ori_RToe)
                    #     pre_RToe = kalman_RToe.predict()

                    # key_points[11][0] = pre_RAnkle[0,0]
                    # key_points[11][1] = pre_RAnkle[1,0]
                    # key_points[22][0] = pre_RToe[0,0]
                    # key_points[22][1] = pre_RToe[1,0]

                    # R_ankel_x = pre_RAnkle[0,0]
                    # R_ankel_y = pre_RAnkle[1,0]
                    # R_toe_x = pre_RToe[0,0]
                    # R_toe_y = pre_RToe[1,0]

                    keypoint_str = get_keypoint_string(datum.poseKeypoints[0])
                    #print(keypoint_str)

                    if ((R_toe_x==0.0 and R_toe_y==0.0) or (R_ankel_x==0.0 and R_ankel_y==0.0)):  # 未偵測到腳踝or大拇指
                        #print('deg = 999')
                        pass
                    else:
                        if (abs(R_toe_x - R_ankel_x) < 1):        # 如果 x1 跟 x2 太接近，代表垂直
                            R_deg = '0'
                            #print('zero!!')
                        elif (abs(R_toe_y - R_ankel_y) < 1):
                            if R_ankel_x > R_toe_x:
                                R_deg = '90'
                            else:
                                R_deg = '-90'
                        else:
                            c = math.sqrt((R_toe_x - R_ankel_x)**2 + ((R_toe_y - R_ankel_y)**2))
                            b = abs((R_toe_y - R_ankel_y))
                            #print(str(c) + ', ' + str(b))
                            R_deg = int(math.degrees(math.acos((b/c))))       # 鄰邊/斜邊
                            if (R_deg > 0) and (R_deg < 90):
                                if R_ankel_x > R_toe_x:                     # 向右轉
                                    R_deg = str(R_deg)
                                else:                                       # 向左轉
                                    R_deg = str(-R_deg)
                            #print(R_deg)

                    if ((L_toe_x==0.0 and L_toe_y==0.0) or (L_ankel_x==0.0 and L_ankel_y==0.0)):  # 未偵測到腳踝or大拇指
                        #print('no L_deg')
                        pass
                    else:
                        if (abs(L_toe_x - L_ankel_x) < 1):        # 如果 x1 跟 x2 太接近，代表垂直
                            L_deg = '0'
                            #print('zero!!')
                        elif (abs(L_toe_y - L_ankel_y) < 1):
                            if L_ankel_x > L_toe_x:
                                L_deg = '90'
                            else:
                                L_deg = '-90'
                        else:
                            c = math.sqrt((L_toe_x - L_ankel_x)**2 + ((L_toe_y - L_ankel_y)**2))
                            b = abs((L_toe_y - L_ankel_y))
                            #print(str(c) + ', ' + str(b))
                            L_deg = int(math.degrees(math.acos((b/c))))       # 鄰邊/斜邊
                            if (L_deg > 0) and (L_deg < 90):
                                if L_ankel_x > L_toe_x:                     # 向右轉
                                    L_deg = str(L_deg)
                                else:                                       # 向左轉
                                    L_deg = str(-L_deg)
                            #print(L_deg)
                    #print(str(R_ankel_x) + ' | ' + str(R_ankel_y) + ' | ' + str(R_toe_x) + ' | ' + str(R_toe_y))

                    R_leg_len = str(int(math.sqrt((R_hip_x - R_knee_x)**2 + (R_hip_y - R_knee_y)**2) + math.sqrt((R_knee_x - R_ankel_x)**2 + (R_knee_y - R_ankel_y)**2)))
                    L_leg_len = str(int(math.sqrt((L_hip_x - L_knee_x)**2 + (L_hip_y - L_knee_y)**2) + math.sqrt((L_knee_x - L_ankel_x)**2 + (L_knee_y - L_ankel_y)**2)))

                    with open('shoe_index.txt', 'r') as f:
                        temp = f.readline()
                    #print(temp)
                    keypoint_str_byte = bytes(keypoint_str, 'ascii') + b',' + bytes(R_deg, 'ascii') + b',' + bytes(L_deg, 'ascii') + b',' + bytes(R_leg_len, 'ascii') + b',' + bytes(L_leg_len, 'ascii') + b',' + bytes(temp.replace('\n', ''), 'ascii')
                    #keypoint_str_byte = bytes(keypoint_str, 'ascii') + b',' + bytes(R_deg, 'ascii') + b',' + bytes(L_deg, 'ascii') + b',' + bytes(R_leg_len, 'ascii') + b',' + bytes(L_leg_len, 'ascii')
                    client.send(keypoint_str_byte)
                else:
                    client.send(b'0' + b',0'*49 + b',999,999,100,100,-1')        # total : 25個(x,y)，所以一共50個座標值
            except ConnectionResetError:
                print('socket close')
                client.close()
                break
            except ConnectionAbortedError:
                print('socket close')
                client.close()
                break

        cap.release()
        cv2.destroyAllWindows()
except Exception as e:
    print(e)
    sys.exit(-1)