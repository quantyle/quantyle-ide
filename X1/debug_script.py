# .update()
d = {"five" : 5}
d2 = {"five" : 55, "six" : 66, "seven" : [7, 77]}
d.update(d2)

x = list(d.keys())
x.sort()
for key in x:
    print(key, d[key])

# five 55
# seven [7, 77]
# six 66

d["seven"][1] = 100
d["six"] = 6

x = list(d2.keys())
x.sort()
for key in x:
    print(key, d2[key])
# five 55
# seven [7, 100]
# six 66