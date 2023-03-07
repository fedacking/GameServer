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
        cell = line.split(";")[-1].split("!")
        coords = list(map(lambda x: tuple(map(int, x.split("|"))), cell))

        for coord in coords:
            i += 1
            color = float(i) / len(coords) * 512
            color_r = min(255, 512 - color)
            color_g = min(255, color)
            try:
                cells[295-coord[1]][coord[0]] = (color_r, color_g, 0)
            except:
                pass

image_arr = np.array(cells, dtype=np.uint8)
data = Image.fromarray(image_arr)
data.save(f'output/path_and_smooth.png')

