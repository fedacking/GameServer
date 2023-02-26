import json
import math
from pathlib import Path
from typing import List
from shapely.geometry import Point, Polygon
import pyprog
import numpy as np
from PIL import Image


all_turrets: List[List[Polygon]] = []
for map_content in Path("../Content/LeagueSandbox-Default/Maps/Map1/Scene").iterdir():
    if "Turret_T" not in map_content.name:
        continue
    with map_content.open("r+") as file:
        turret_json = json.load(file)
        vertices = [(float(x['X']/50), float(x['Z']/50)) for x in turret_json["Vertices"]]
        vertices[2], vertices[3] = vertices[3], vertices[2]
        all_turrets.append(Polygon(vertices))

x_max = 400
y_max = 400

img_array = []


# Create a PyProg ProgressBar Object
prog = pyprog.ProgressIndicatorFraction(":-) ", " OK!", x_max*y_max)

# Show the initial status
prog.update()
i = 0

for x in range(0, x_max):
    img_array.append([])
    for y in range(0, y_max):
        flag_inside = False
        for tur in all_turrets:
            if Point(x, y).within(tur):
                flag_inside = True
                break
        if flag_inside:
            img_array[-1].append((0, 0, 0))
        else:
            img_array[-1].append((255, 255, 255))

        # Update status
        i += 1
        prog.set_stat(i)

        # Show (Update) the current status
        prog.update()

image_arr = np.array(img_array, dtype=np.uint8)
data = Image.fromarray(image_arr)
data.save(f'output/turrets.png')
