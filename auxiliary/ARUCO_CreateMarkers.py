import numpy as np
import cv2, PIL
from cv2 import aruco
import matplotlib.pyplot as plt
import matplotlib as mpl
import pandas as pd

aruco_dictionary = aruco.Dictionary_get(aruco.DICT_4X4_50)

fig = plt.figure()
nx = 4
ny = 3
for i in range(8):
    # ax = fig.add_subplot(ny,nx, i)
    print(i)
    img = aruco.drawMarker(aruco_dictionary,i, 400)
    cv2.imwrite("marker%d.bmp"%i, img)


    # plt.imshow(img, cmap = mpl.cm.gray, interpolation = "nearest")
    # ax.axis("off")

# plt.savefig("_data/markers.pdf")
# plt.show()