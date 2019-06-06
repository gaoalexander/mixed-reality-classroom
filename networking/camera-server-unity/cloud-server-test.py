import socket
import pickle
import numpy as np
import cv2
from cv2 import aruco

#-----------------------------------------------------------------------------------
# SETUP SOCKETS

# CAMERA CLIENT - SERVER SOCKET
'''


 I don't think we need this anymore, given that the SocketServer below is connecting to unity clients
# SERVER - UITY CLIENT SOCKET
UDP_IP_ADDRESS = "127.0.0.1"                                # LAN IP ADDRESS OF SERVER
UDP_PORT_NO = 52380
serversocket2 = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
serversocket2.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
serversocket2.bind(('', UDP_PORT_NO))
serversocket2.listen(5)
clientsocket2, address = serversocket2.accept()
clientsocket2.setblocking(False)
'''
'''
#-----------------------------------------------------------------------------------
# INITIALIZE / DECLARE GLOBAL VARIABLES


'''
import socket
import socketserver
import threading
import json

state = {}
clients = {}

class ThreadedTCPHandler(socketserver.BaseRequestHandler):
    """
    The RequestHandler class for our server.
    It is instantiated once per connection to the server, and must
    override the handle() method to implement communication to the
    client.
    """

    '''
    def recv_loop(self):
        while(True):
            print(clients)
            try:
                # self.request is the TCP socket connected to the client
                self.data = self.request.recv(4096).strip()
            except:
                print ("cannot receive data")
                return
            if not self.data:
                return
            #print "{} wrote:".format(self.client_address[0])
            print (self.data)
            senddata = {}
            array = self.data.decode('utf-8').split('`')
            data = json.loads(array[-2])
            if (data["type"] == "object"):
                    if (data['uid'] in state and state[data['uid']]['lockid'] != data['lockid'] and state[data['uid']]['lockid'] != ""):
                            print("object in use")
                    else:
                            state[data['uid']] = data
                    senddata = state
                    senddata["type"] = "object"
            elif (data["type"] == "check"):
                    senddata["type"] = "check"
                    senddata["success"] = np.array_equal(combination_ids,target)
            for client in clients.values():
                    client.sendall(json.dumps(senddata).encode('utf-8'))
                    '''

    def handle(self):
        clients[self.request.getpeername()[0] + ":" + str(self.request.getpeername()[1])] = self.request
        self.request.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
        while(True):
            print(clients)
            try:
                # self.request is the TCP socket connected to the client
                self.data = self.request.recv(4096).strip()
            except:
                print ("cannot receive data")
                return
            if not self.data:
                return
            #print "{} wrote:".format(self.client_address[0])
            print (self.data)
            senddata = {}
            array = self.data.decode('utf-8').split('`')
            data = json.loads(array[-2])
            if (data["type"] == "object"):
                    if (data['uid'] in state and state[data['uid']]['lockid'] != data['lockid'] and state[data['uid']]['lockid'] != ""):
                            print("object in use")
                    else:
                            state[data['uid']] = data
                    senddata = state
                    senddata["type"] = "object"
            elif (data["type"] == "check"):
                    senddata["type"] = "check"
                    senddata["success"] = np.array_equal(combination_ids,target)
            for client in clients.values():
                    client.sendall(json.dumps(senddata).encode('utf-8'))
                    

class ThreadedTCPServer(socketserver.ThreadingMixIn, socketserver.TCPServer):
    pass

HOST, PORT = "", 20391

# Create the server, binding to localhost on port 9999
server = ThreadedTCPServer((HOST, PORT), ThreadedTCPHandler)

# Activate the server; this will keep running until you
# interrupt the program with Ctrl-C
t = threading.Thread(target=server.serve_forever)
t.setDaemon(True) # don't hang on exit
t.start()

#while(True):
#    pass

#server.serve_forever()
#-----------------------------------------------------------------------------------

TCP_IP_ADDRESS = "127.0.0.1"                                # LAN IP ADDRESS OF SERVER
TCP_PORT_NO = 20380
serversocket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
serversocket.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
serversocket.bind(('', TCP_PORT_NO))
serversocket.listen(5)
clientsocket, address = serversocket.accept()

buf = []
bytesrecvd = 0

WIDTH = 640
HEIGHT = 480
DATALEN = WIDTH * HEIGHT
frame = 1

combination_ids = []
target = [0,1,2]

aruco_dict = aruco.Dictionary_get(aruco.DICT_4X4_50)
parameters =  aruco.DetectorParameters_create()

#-----------------------------------------------------------------------------------

def detectMarkers(image_array):
        corners, ids, rejectedImgPoints = aruco.detectMarkers(image_array, aruco_dict, parameters=parameters)
        if ids is None:
                combination_ids = [4]
        else:
                combination_ids = np.reshape(ids, ids.shape[0])
        combination_ids.sort()
        # combination_ids = np.array([0,1,2])
        if np.array_equal(combination_ids,target):
                print("YOU PICKED THE RIGHT PIECES.")
        else: print("KEEP TRYING.")
        gray = aruco.drawDetectedMarkers(imarr, corners)
        return combination_ids

#-----------------------------------------------------------------------------------

while True:
        data = clientsocket.recv(DATALEN)
        if not data: break
        bytesrecvd += len(data)
        buf.extend(data)

        if bytesrecvd >= DATALEN:
                # print("buffer complete!")
                flattened = buf[0:DATALEN]
                imarr = np.array(flattened, dtype=np.uint8)
                imarr = np.reshape(imarr, (HEIGHT, WIDTH))

                detected = detectMarkers(imarr)
                print(detected)
                cv2.imshow('local', imarr)
                cv2.waitKey(1)
                
'''

                b_array = bytearray()
                size = len(detected)
                sbytes = size.to_bytes(2, 'little')
                b_array.append(sbytes[0])
                b_array.append(sbytes[1])
                for marker in detected:
                        b_array.append(marker)
                try:
                        clientsocket2.send(b_array)
                except:
                        print("client closed")
                buf = []                # CLEAR BUFFER
                bytesrecvd = 0          # RESET BYTES RECEIVED COUNTER
             
'''
