import numpy as np
from PIL import Image

boolean = {
    'False': False,
    'True': True,
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
            bools = [boolean[x.strip()] for x in line.split(';')[2:-1]]
            if not bools[1]:
                cells[-1].insert(0, (0, 0, 0))
            elif not bools[2]:
                cells[-1].insert(0, (255, 0, 0))
            elif not bools[3]:
                cells[-1].insert(0, (0, 255, 0))
            elif not bools[4]:
                cells[-1].insert(0, (0, 0, 255))
            else:
                cells[-1].insert(0, (255, 255, 255))

image_arr = np.array(cells, dtype=np.uint8)
data = Image.fromarray(image_arr)
data.save(f'output/navgrid.png')
