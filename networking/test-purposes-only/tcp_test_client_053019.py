import socket
import cv2
import pickle
import math
import sys
import numpy as np
import time

# cap = cv2.VideoCapture(0)
# cap.release()
# exit()

# print("FPS:",cap.get(cv2.CAP_PROP_FPS))
# print("RES:",cap.get(cv2.CAP_PROP_FRAME_WIDTH),cap.get(cv2.CAP_PROP_FRAME_HEIGHT))
TCP_IP_ADDRESS = "34.73.132.190"                                # LAN IP ADDRESS OF SERVER
TCP_PORT_NO = 25290

clientsocket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
clientsocket.connect((TCP_IP_ADDRESS, TCP_PORT_NO))


frame_number = 1
t = 0.0
while(True):

        t = time.time()
        clientsocket.send("hello outgoing".encode())
                
        msg = clientsocket.recv(5)
        print(msg.decode())
        # break
        # dt = time.time() - t
        # print(dt)
        #print(frame_number)
        frame_number += 1
