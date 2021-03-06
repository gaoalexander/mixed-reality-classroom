import socket
import pickle
import numpy as np
import cv2
from cv2 import aruco

#-----------------------------------------------------------------------------------
# SETUP SOCKETS

# CAMERA CLIENT - SERVER SOCKET

'''
#-----------------------------------------------------------------------------------
# INITIALIZE / DECLARE GLOBAL VARIABLES
'''
import socket
import socketserver
import threading
from queue import Queue

import json
import random

state = {}
clients = {}
organelles = [0,1,2,3,4,5,6,7,8,9,10,11,12,19,27,28,34]

# MQ
# tosend = []
tosend = Queue(maxsize=0)

# interval = 0.01
expire = 5 * 60
toflush = []

def sendmessages():
    while True:
        message = tosend.get()

        for client, socket in clients.items():
            try:
                socket.sendall(json.dumps(message).encode('utf-8'))
            except OSError as e:
                toflush.append(client)
                print(str(e) + ":\n" + client)
                # print(client + ":" + str(e))

        tosend.task_done()

        for i in range(0, len(toflush)):
            
            client = toflush[i]
            print("popping" + client)
            clients[client].close()
            clients.pop(client)
            toflush.pop(i)

# skipped = False
# # poll for clients and clear state if none are connected
# def checkAndFlush():
#     global skipped

#     if len(clients) == 0:
#         if not skipped:
#             skipped = True
#             return
#         state = {}
            
# threading.Timer(expire, checkAndFlush).start()
            
# def sendmessages():
    # threading.Timer(interval, sendmessages).start()
spawn_manager = {}

class SpawnPoint:
    def __init__(self, idnum, isFull):
        self.idnum = idnum
        self.isFull = False;

def generateSpawnPoints(n):
    spawn_points = []
    for i in range(0,n):
        spawn_points.append(SpawnPoint(i,False))
    return spawn_points

def findFreeSpawnPoints(detected, spawn_points):
    free_points = []
    all_points = []
    result = []

    #Check for all free spawn points
    for i in range(0, len(spawn_points)):
        if not spawn_points[i].isFull:
            free_points.append(spawn_points[i])

    free_count = 0
    random.shuffle(free_points)
    for id in detected:
        if id not in organelles:
            if((not "sim" in state) and (id == 47 or id == 48 or id == 49)):    
                state["sim"] = id
            result.append(-1)
        elif id in spawn_manager:
            result.append(-1)
        elif(free_count >= len(free_points)):
            result.append(random.randrange(0,len(spawn_points)))
        else:
            result.append(free_points[free_count].idnum)
            spawn_manager[id] = free_points[free_count]
            free_points[free_count].isFull = True
            free_count+=1
    return result

spawn_points = generateSpawnPoints(8)

class ThreadedTCPHandler(socketserver.BaseRequestHandler):
    """
    The RequestHandler class for our server.
    It is instantiated once per connection to the server, and must
    override the handle() method to implement communication to the
    client.
    """
    
    def handle(self):
        clients[self.request.getpeername()[0] + ":" + str(self.request.getpeername()[1])] = self.request
        self.request.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
        print("new connection:")
        print(self.request.getpeername()[0])

        global state

        #send everything once someone connects
        senddata = state
        senddata["type"] = "initialize"
        
        try:
            self.request.sendall(json.dumps(senddata).encode('utf-8'))
        except OSError as e:
            print(str(e) + ":\n" + self.request.getpeername())

        while(True):
            # print(clients)
            try:
                # self.request is the TCP socket connected to the client
                self.data = self.request.recv(4096).strip()
            except:
                print ("cannot receive data")
                break
            if not self.data:
                break
            #print "{} wrote:".format(self.client_address[0])
            # print (self.data)
            senddata = {}
            array = self.data.decode('utf-8').split('`')

            try:
                data = json.loads(array[-2])
            except ValueError as e:
                print("invalid message, ignoring: ", array)
                print(e)
                continue

            if (data["type"] == "object"):
                # print("state: ", state)
                # print("data: ", data)
                if (data['uid'] in state and state[data['uid']]['lockid'] != data['lockid'] and state[data['uid']]['lockid'] != ""):
                    print("object in use")
                else:
                    state[data['uid']] = data
                    if(data['uid'] in spawn_manager.keys()):
                        spawn_manager[data['uid']].isFull = False
                        del spawn_manager[data['uid']]
                senddata = state
                senddata["type"] = "object"

                tosend.put(senddata)
            elif (data["type"] == "spawn"):
                state[data['uid']] = {} 
                state[data['uid']]['lockid'] = ''
                state[data['uid']]['active'] = True
                state[data['uid']] = data
            elif (data["type"] == "check"):
                senddata["type"] = "check"
                senddata["success"] = np.array_equal(combination_ids,target)
                senddata["ids"] =  data["ids"].split(',')
            elif(data["type"] == "active"):
                senddata["type"] = "active"
                data["ids"] = data["ids"].replace("[", "")
                data["ids"] = data["ids"].replace("]", "")
                senddata["ids"] = [int(i) for i in senddata["ids"]] 
                senddata["spawn"] = findFreeSpawnPoints(senddata["ids"],spawn_points)   
                tosend.put(senddata)
            elif(data["type"] == "release"):
                if (data['uid'] in state):
                    state[data['uid']]['lockid'] = ""
                    print("release object")
                    senddata = state
                    senddata['type'] = "release"
                    senddata['eventid'] = data['uid']
                    
                    tosend.put(senddata)
            elif(data["type"] == "deactivate"):
                if (data['uid'] in state):
                    state[data['uid']]['active'] = False
                    print("deactivate object")
                    senddata = state
                    senddata['type'] = "deactivate"
                    senddata['eventid'] = data['uid']

                    tosend.put(senddata)
            elif(data["type"] == "restart"):
                state = {}
                senddata['type'] = 'restart'
                tosend.put(senddata)
        
        print("killing connection" + self.request.getpeername()[0] + ":" + str(self.request.getpeername()[1]))
        del clients[self.request.getpeername()[0] + ":" + str(self.request.getpeername()[1])]

        if len(clients) == 0:
            state = {}
        
class ThreadedTCPServer(socketserver.ThreadingMixIn, socketserver.TCPServer):
    allow_reuse_address = True

HOST, PORT = "", 20391

# Create the server, binding to localhost on port 9999
server = ThreadedTCPServer((HOST, PORT), ThreadedTCPHandler)
# server.allow_reuse_address = True
# server.server_bind()     # Manually bind, to support allow_reuse_address
# server.server_activate()

# Activate the server; this will keep running until you
# interrupt the program with Ctrl-C
t = threading.Thread(target=server.serve_forever)
t.setDaemon(True) # don't hang on exit
t.start()

senderThread = threading.Thread(target=sendmessages)
senderThread.setDaemon(True)
senderThread.start()

#while(True):
#    pass

#server.serve_forever()
#-----------------------------------------------------------------------------------

TCP_IP_ADDRESS = ""                                # LAN IP ADDRESS OF SERVER
TCP_PORT_NO = 20380
serversocket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
serversocket.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
serversocket.bind(('', TCP_PORT_NO))
serversocket.listen(5)
clientsocket, address = serversocket.accept()

buf = []
bytesrecvd = 0

WIDTH = 1280
HEIGHT = 720
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
        return None
    else:
        combination_ids = np.reshape(ids, ids.shape[0])
    combination_ids.sort()
    # combination_ids = np.array([0,1,2])
    if np.array_equal(combination_ids,target):
        print("YOU PICKED THE RIGHT PIECES.")
    #else: print("KEEP TRYING.")
    gray = aruco.drawDetectedMarkers(image_array, corners)
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
        #cv2.imshow('local', imarr)
        #cv2.waitKey(1)
        detected = detectMarkers(imarr)
        
        if(detected is not None):
            print(detected)
            senddata = {}
            senddata["type"] = "active"
            senddata["ids"] = detected.tolist()
            senddata["spawn"] = findFreeSpawnPoints(detected.tolist(),spawn_points)
            #senddata["spawn"] = random.sample(range(0,len(spawn_points)), len(senddata["ids"]))

            for id in senddata["ids"]:
                state[id] = {}
                state[id]['lockid'] = ''
                state[id]['active'] = True
            #     print("activate object")

            tosend.put(senddata)
        #     for client in clients.values():
        #         client.sendall(json.dumps(senddata).encode('utf-8'))
        buf = []                # CLEAR BUFFER
        bytesrecvd = 0          # RESET BYTES RECEIVED COUNTER
