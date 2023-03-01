from random import uniform

text = "\t\t\t"

for i in range(100):
    if (i % 10) == 0:
        text += "\n\t\t\t"
    text += "new Actor(new Vector2(%f" % uniform(0.5, 250) + \
            "f, %f" % uniform(0.5, 250) + \
            "f), %ff), " % uniform(0.3, 1.5)

print(text)