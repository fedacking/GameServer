

with open('input/path_debug_2.txt', 'r+') as file:
    for line in file:
        print(line.strip().split(';')[1:])