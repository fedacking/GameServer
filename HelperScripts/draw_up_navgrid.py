import numpy as np
from PIL import Image

boolean = {
    'False': (0, 0, 0),
    'True': (255, 255, 255)
}

with open("input/navgrid.txt", "r+") as file:
    x_count = int(file.readline())
    y_count = int(file.readline())
    cell_size = int(file.readline())
    cells = []

    for x in range(0, y_count):
        cells.append([])
        for y in range(0, x_count):
            line = file.readline()
            cells[-1].insert(0, boolean[line.split(';')[-1].strip()])

image_arr = np.array(cells, dtype=np.uint8)
data = Image.fromarray(image_arr)
data.save(f'output/navgrid.png')
