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
        if not "Dequed Path" in line:
            continue
        i += 1
        color = i / 29510 * 255
        cell = line.split(";")[-1].split("!")[-1].strip()
        coords = tuple(map(int, cell.split("|")))
        cells[coords[0]][295-coords[1]] = (color, 255-color, 0)

image_arr = np.array(cells, dtype=np.uint8)
data = Image.fromarray(image_arr)
data.save(f'output/pathgrid.png')

