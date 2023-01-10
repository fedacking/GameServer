import numpy as np
from PIL import Image

with open("input/map debug.txt", "r+") as file:
    line = file.readline()
    x = int(line.split(';')[0].split(':')[1].strip())
    y = int(line.split(';')[1].split(':')[1].strip())
    i = 0
    cells = [[]]
    for line in file:
        result = np.array([255, 0, 0]) if 'NOT_PASSABLE' in line else [0, 255, 0]
        cells[-1].append(result)
        i += 1
        if i >= x and len(cells) < y:
            cells.append([])
            i = 0

image = np.array(cells, dtype=np.uint8)
# Make random 28x28 RGB image
# image = np.random.randint(0, 256, (28, 28, 3), dtype=np.uint8)

data = Image.fromarray(image)
data.save('output/gfg_dummy_pic.png')
