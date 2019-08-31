import socket
import json

host = "127.0.0.1"                                # LAN IP ADDRESS OF SERVER
TCP_PORT_NO = 20380
# ADDRESS = (host, PORT_NO)

clientsocket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
clientsocket.connect((host, TCP_PORT_NO))

print("...client connecting to " + host)
while(True):
    senddata = {}
    senddata["type"] = "active"
    senddata["ids"] = ["1"]
    message = json.dumps(senddata)
    clientsocket.send(message)
