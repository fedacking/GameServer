import math
import os
import json

import numpy as np
from PIL import Image

path = "..\\Content\\LeagueSandbox-Default\\Maps\\Map1\\Scene\\"

nexus_path = ["HQ_T1.sco.json", "HQ_T2.sco.json"]
spawns = {
    "red": {
        "C": "__P_Chaos_Spawn_Barracks__C01.sco.json",
        "L": "__P_Chaos_Spawn_Barracks__L01.sco.json",
        "R": "__P_Chaos_Spawn_Barracks__R01.sco.json"
    },
    "blue": {
        "C": "__P_Order_Spawn_Barracks__C01.sco.json",
        "L": "__P_Order_Spawn_Barracks__L01.sco.json",
        "R": "__P_Order_Spawn_Barracks__R01.sco.json"
    }
}



class MapObject:
    radius: int
    x: int
    y: int
    color: tuple[int, int, int]

    def __init__(self, path, radius, color):
        if not os.path.exists(path):
            raise Exception

        with open(path, "r+") as file:
            obj_json = json.load(file)

        self.radius = radius
        self.color = color
        self.x = obj_json["CentralPoint"]["X"]
        self.y = obj_json["CentralPoint"]["Z"]


class Map:
    width: int
    height: int
    map: list[list[tuple[int, int, int]]]

    def __init__(self, width, height):
        self.width = width
        self.height = height
        self.map = []
        for x in range(width):
            self.map.append([])
            for y in range(height):
                self.map[-1].append((255, 255, 255))

    def draw_circle(self, rgb, x, y, radius):
        for x1 in range(math.floor(x-radius), math.ceil(x+radius+1)):
            for y1 in range(math.floor(y-radius), math.ceil(y+radius+1)):
                if math.dist((x1, y1), (x, y)) <= radius:
                    self.map[x1][y1] = rgb

    def draw_map_object(self, map_object: MapObject):
        self.draw_circle(map_object.color, map_object.x, map_object.y, map_object.radius)


def find_mm(data_list, data_fun, comp_fun):
    val_list = [data_fun(x) for x in data_list]
    return comp_fun(x for x in val_list)


objects = []
for n_path in nexus_path:
    objects.append(MapObject(path + n_path, 353, (0, 0, 0)))

objects.append(MapObject(path + spawns["red"]["C"], 36, (255, 0, 0)))
objects.append(MapObject(path + spawns["red"]["L"], 36, (0, 255, 0)))
objects.append(MapObject(path + spawns["red"]["R"], 36, (0, 0, 255)))
objects.append(MapObject(path + spawns["blue"]["C"], 36, (255, 0, 0)))
objects.append(MapObject(path + spawns["blue"]["L"], 36, (0, 255, 0)))
objects.append(MapObject(path + spawns["blue"]["R"], 36, (0, 0, 255)))

min_x = math.floor(find_mm(objects, lambda x: x.x - x.radius, min))
min_y = math.floor(find_mm(objects, lambda x: x.y - x.radius, min))

for obj in objects:
    obj.x -= min_x
    obj.y -= min_y

max_x = math.ceil(find_mm(objects, lambda x: x.x + x.radius, max))
max_y = math.ceil(find_mm(objects, lambda x: x.y + x.radius, max))

lol_map = Map(max_x, max_y)
for obj in objects:
    lol_map.draw_map_object(obj)
    print(obj)

image_arr = np.array(lol_map.map, dtype=np.uint8)

data = Image.fromarray(image_arr)
data.save(f'output/map.png')
