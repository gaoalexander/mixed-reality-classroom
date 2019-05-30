import socket
import pickle
import numpy as np
import cv2

TCP_PORT_NO = 25230

serversocket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
serversocket.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
serversocket.bind(('', TCP_PORT_NO))

serversocket.listen(5)
clientsocket, address = serversocket.accept()
buf = []
bytesrecvd = 0

WIDTH = 320
HEIGHT = 240
DATALEN = WIDTH * HEIGHT
frame = 1
while True:
        data = clientsocket.recv(DATALEN)
        if not data: break
        #print(len(data))
        bytesrecvd += len(data)
        buf.extend(data)
        if bytesrecvd >= DATALEN:
                #print("buffer complete!")
                flattened = buf[0:DATALEN]
                #print("flattened size: ", len(flattened))
                imarr = np.array(flattened, dtype=np.uint8)
                imarr = np.reshape(imarr, (HEIGHT, WIDTH))
                cv2.imshow("remote", imarr)
                cv2.waitKey(1)
                clientsocket.send("hello".encode())
                frame += 1

                buf = []
                bytesrecvd = 0
        #data1 = pickle.loads(data)
        #print("Frame #: ", data1[2], "Packet #: ", data1[3], " / ", data1[4])
        # clientsocket.send("Received!")
