import socket
import pickle
import numpy as np
import cv2

TCP_PORT_NO = 25285

serversocket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
serversocket.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
serversocket.bind(('', TCP_PORT_NO))

serversocket.listen(5)
clientsocket, address = serversocket.accept()
buf = []
bytesrecvd = 0

# WIDTH = 320
# HEIGHT = 240
DATALEN = 4096
frame = 1
while True:
        data = clientsocket.recv(DATALEN)
        print(data.decode())
        if not data: break
        bytesrecvd += len(data)
        buf.extend(data)
        clientsocket.send("hello".encode())
        buf = []
        bytesrecvd = 0