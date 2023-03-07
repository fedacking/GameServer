import numpy as np
from PIL import Image

cells = []
for x in range(295):
    cells.append([])
    for y in range(295):
        cells[-1].append((0, 0, 0))

i = 0
with open("input/a_star.txt", "r+") as in_file:
    for line in in_file:
        if not "Final Path" in line:
            continue
        line = line.strip()
        i += 1
        color = i / 29510 * 255
        cell = line.split(";")[-1].split("!")
        coords = list(map(lambda x: tuple(map(int, x.split("|"))), cell))
        for coord in coords:
            cells[coord[0]][295-coord[1]] = (0, 255, 0)

image_arr = np.array(cells, dtype=np.uint8)
data = Image.fromarray(image_arr)
data.save(f'output/path_and_smooth.png')

