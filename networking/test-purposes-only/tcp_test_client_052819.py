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



cap = cv2.VideoCapture(0)
cap.set(cv2.CAP_PROP_FRAME_WIDTH, 320)
cap.set(cv2.CAP_PROP_FRAME_HEIGHT, 240)
print("FPS:",cap.get(cv2.CAP_PROP_FPS))
print("RES:",cap.get(cv2.CAP_PROP_FRAME_WIDTH),cap.get(cv2.CAP_PROP_FRAME_HEIGHT))

#TCP_IP_ADDRESS = "172.24.71.205"                                # LAN IP ADDRESS OF SERVER
#TCP_IP_ADDRESS = "localhost"                                # LAN IP ADDRESS OF SERVER
# TCP_IP_ADDRESS = "10.19.19.239"                                # LAN IP ADDRESS OF SERVER
TCP_IP_ADDRESS = "10.19.19.239"                                # LAN IP ADDRESS OF SERVER
TCP_PORT_NO = 25265
# ADDRESS = (TCP_IP_ADDRESS, PORT_NO)

clientsocket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
clientsocket.connect((TCP_IP_ADDRESS, TCP_PORT_NO))

# cap.set(cv2.CAP_PROP_FPS, 1)
# print("CAP.GET(5)", cap.get(5))
# FPS = cap.get(5)
# setFPS = 10
# ratio = int(FPS)/setFPS

frame_number = 1
t = 0.0
while(True):

        # Capture frame-by-frame
        ret,frame=cap.read()
        if frame_number % 1 == 0:   #LIMIT FRAMERATE BY ONLY PASSING CERTAIN FRAMES TO SEND, INDENT BLOCK OF CODE BENEATH HERE
                grayscale = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
                #cv2.imshow('local', grayscale)
                #cv2.waitKey(1)
                flattened = grayscale.flatten()

                hsize = 14              # Bytes
                dsize = flattened.shape[0]      # Bytes
                frame_w = grayscale.shape[1]
                frame_h = grayscale.shape[0]

                t = time.time()
                clientsocket.send("hello outgoing".encode())
                
        msg = clientsocket.recv(5)
        dt = time.time() - t
        print(dt)
        #print(frame_number)
        frame_number += 1
