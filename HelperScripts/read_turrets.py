import json
from typing import List, Tuple
from pathlib import Path

all_turrets: List[List[Tuple[float, float]]] = []
for map_content in Path("../Content/LeagueSandbox-Default/Maps/Map1/Scene").iterdir():
    if "Turret_T" not in map_content.name:
        continue
    with map_content.open("r+") as file:
        turret_json = json.load(file)
        vertices = [(int((x['X']+328.903)/50), int((x['Z']+110.193)/50)) for x in turret_json["Vertices"]]
        vertices[2], vertices[3] = vertices[3], vertices[2]
        all_turrets.append(vertices)


for tur in all_turrets:
    print(tur)
