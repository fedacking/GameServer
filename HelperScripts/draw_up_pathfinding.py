import numpy as np
import math
from PIL import Image

nexsus = (12760, 13026)
nexsus_radius = 353

with open("input/pathfinding debug.txt", "r+") as file:
    line = file.readline()
    line = line.split('-')[-1].strip().replace(',', '.')
    position = (float(line.split(';')[0].strip()), float(line.split(';')[1].strip()))
    radius = float(line.split(';')[2].strip())
    new_positions = []
    for line in file:
        line = line.split('-')[-1].strip().replace(',', '.')
        new_positions.append((float(line.split(';')[0].strip()), float(line.split(';')[1].strip())))

print(math.dist(nexsus, new_positions[0]))

min_coords = [nexsus[0] - nexsus_radius, nexsus[1] - nexsus_radius]
max_coords = [nexsus[0] + nexsus_radius, nexsus[1] + nexsus_radius]

min_coords[0] = min(min_coords[0], position[0] - radius)
min_coords[1] = min(min_coords[1], position[1] - radius)
max_coords[0] = max(max_coords[0], position[0] + radius)
max_coords[1] = max(max_coords[1], position[1] + radius)

min_coords[0] = min(min_coords[0], min(x[0] - radius for x in new_positions))
min_coords[1] = min(min_coords[1], min(x[1] - radius for x in new_positions))
max_coords[0] = max(max_coords[0], max(x[0] + radius for x in new_positions))
max_coords[1] = max(max_coords[1], max(x[1] + radius for x in new_positions))

min_coords[0] = math.floor(min_coords[0])
min_coords[1] = math.floor(min_coords[1])
max_coords[0] = math.ceil(max_coords[0])
max_coords[1] = math.ceil(max_coords[1])

print(new_positions)
print(min_coords)
print(max_coords)
print(radius)
print(math.dist(nexsus, position))

images = []
for new_pos in new_positions:
    images.append([])
    for x in range(min_coords[0], max_coords[0] + 1):
        images[-1].append([])
        for y in range(min_coords[1], max_coords[1] + 1):
            if math.dist((x, y), nexsus) <= nexsus_radius:
                images[-1][-1].append([0, 0, 0])
            elif math.dist((x, y), position) <= radius:
                images[-1][-1].append([0, 240, 0])
            elif math.dist((x, y), new_pos) <= radius:
                images[-1][-1].append([240, 0, 0])
            else:
                images[-1][-1].append([255, 255, 255])

for i in range(len(images)):
    image_arr = np.array(images[i], dtype=np.uint8)
    # Make random 28x28 RGB image
    # image = np.random.randint(0, 256, (28, 28, 3), dtype=np.uint8)

    data = Image.fromarray(image_arr)
    data.save(f'output/new_pos{i}.png')
