# import sys
# sys.stdout = open("python_output.txt", "w", encoding="utf-8")




#####################################################################
# testing expressions
def test_expressions():
    print("Testing basic expression evaluation")

    x = 2 + 3 * 4
    print(x)

    x = 2 * 3 + 4
    print(x)

    x = 1
    print(x)

    x = 3
    x = 2 + 2 * x ** 2 # 20
    print(x)

    x = 3
    x = 1 + 2 * (3 + x) # 13
    print(x)

    x = 3.14
    x = 1 + x # 4.14
    print(x)

    x = -3.14
    print(x)

    x = 5 - 3.14
    print(x)

    x = 5 - -3.14
    print(x)

    x = 5+-3.14
    print(x)

    x = .2
    print(x)

    x = 3 + .2
    print(x)

    x = 100e2
    print(x)

    x = 100 + 3.14159e2
    print(x)

    x = 100 + -3.14e2
    print(x)

    print(1 + 2 - 3 + 4 - 5) # -1
    print(-1 + -2 * (2 + 3)) # -11
    print(1 + 2 * (3 + 4) + 5)
    print(1 + 2.2 * (3.3 - 4.1) / 5)
    print(1 + 2.2 * (3.3 - 4.1) / (5 + 2))
    print(1 + 2.2 * (3.3 - 4.1) / (5 + 2) + 5)
    print(-1 + -2.2 * (-3.3 - 4.1) / (-5 + 2) - 5) # -11.426666666666666
    print(+1 + +2.2 * (+3.3 - 4.1) / (+5 + 2) + 5)

    print(1 + 2.2 * ((3.3 - 4.1) / (5 + 2) + 4 * 6 + 3 ** 3.5) - 3.14e-1)

    # testing operators
    # logical and, or, not
    x = 3
    y = 4
    print(x==3 or y==3)
    print(x==3 and y==3)
    print(not x== 3)

    # bitwise operators, shift operators
    print(1 | 2) 	# 3
    print(1 ^ 3) 	# 2
    print(5 & 3)    # 1
    print(2 << 3)   # 16
    print(16 >> 3)  # 2
    print(~1)       # -2

    # comparison operators
    x = 3
    z = 4 + x
    print(z < 7)
    print(z <= 7)
    print(z > 7)
    print(z >= 7)
    print(z != 7)
    print(z == 7)

    # division
    print(1/3) # 0.33333
    print(10 // 3)
    print(11.1 // 3)
    print(10 % 3)
    print(10 % 3.14)

    # short hands: +=, -=, *=, /=
    x += 3
    print(x)

    x -= 3
    print(x)

    x = 2
    x *= 3
    print(x) # 6
    x /= 4
    print(x) # 1.5

    
test_expressions()

#####################################################################
# testing built-in functions
print("Testing built-in functions")

print("hello", "world", sep="*", end="...\n") # hello*world...
x = float("123.4") + int("456")
print(x) # 579.4

print(int(float("-2.3") - 4)) # -6


#####################################################################
# testing string
def test_strings():
    print("Testing strings")

    s = "中文"
    print(s)

    s = "x" * 3
    print(s)
    print(3 * "z")    

    s2 = "abcd" + "efg" + "hijk"
    b = 'e' in s2 # True
    print(b)

    print("a" not in s2) # False

    print(s2 == "abcdefghijk")
    print(s2 < "abcdefghijz")
    print(s2 <= "abcdefghijz")
    print(s2 > "abcdefghijz")
    print(s2 >= "abcdefghijz")
    print(s2 != "abcdefghijz")

    s = s2[2:5]
    print(s)

    s = s2[1:3] + s2[5:7]
    print(s) # bcfg

    print(s2[1:10:3]) # beh
    print(s2[-1:0:-1]) # kjihgfedcb

    x = 1
    y = 2
    print(s2[x+y-1 : 2*x+2*y-1])
    print(s2[0])
    print(s2[1:3])
    print(s2[1:7:2])
    print(s2[:8:1])
    print(s2[4:])
    print(s2[:-2])
    print(s2[6:0:-1])
    print(s2[6::-1])
    print(s2[::-1])
    print(s2[:-1:-1])
    print(s2[::-2])
    print(s2[::-3])
    print(s2[:])
    print(s2[-2:])
    print(s2[20:])
    print(s2[2:-2])
    print(s2[7:-7])
    print(s2[:20])

    print("hello world"[1:4], "hello world"[9:6:-1]) # ell lro

    x = len(s2)
    print(x)

    print("hEllo wOrld".capitalize())
    print("abc".center(2))
    print("abc".center(10))
    print("abc".center(10, '*'))
    print(s2.count("gh")) # 1
    print(s2.count("gh", 0, 100)) # 1
    print(s2.count("gh", 6)) # 1
    print(s2.count("gh", 7)) # 0
    print(s2.count("gh", 5, 8)) # 1
    print(s2.count("gh", 5, 7)) # 0
    print("aaabbbaaa".count("a")) # 6
    print(s2.endswith("ijk")) # True
    print(s2.endswith("ijk", 7)) # True
    print(s2.endswith("hij", 7)) # False
    print(s2.endswith("hij", 7, 9)) # False
    print(s2.endswith("hij", 7, 10)) # True
    print(s2.endswith("hij", 1, 10)) # True
    print(s2.endswith("hij", 7, 11)) # False
    print(s2.find("cde")) # 2
    print(s2.find("efg", 4)) # 4
    print(s2.find("efg", 5)) # -1
    print(s2.find("efg", 1, 6)) # -1
    print(s2.find("efg", 1, 7)) # 4
    print(s2.find("efgx", 1, 100)) # -1
    print(s2.find("ijk", 1, 100)) # 8
    print("ab123".isalnum())    # True
    print("ab12+3".isalnum())   # False
    print("".isalpha())         # False
    print("abc".isalpha())      # True
    print("abc123".isalpha())   # False
    print("123.4".isdecimal())  # False
    print("1234".isdecimal())   # True
    print("½".isdecimal())      # False
    print("123".islower())      # False
    print("abCd".islower())     # False
    print("abcd".islower())     # True
    print("12cd".islower())     # True
    print("½".isnumeric())      # True
    print("x".isnumeric())      # False
    print(" \t ".isspace())     # True
    print(" tt ".isspace())     # False
    print("12AB".isupper())     # True
    print("12aB".isupper())     # False
    print("12".isupper())       # False
    print("abc".ljust(10))
    print("abc".ljust(10, '*'))     # abc*******
    print("abc  ".ljust(10, '*'))   # abc  *****
    print("aAb".lower())
    print(" cde  ".lstrip())
    print(s2.lstrip("a"))       # bcdefghijk
    print(s2.lstrip("cab"))     # defghijk
    print(s2.replace("ab", "xx"))   # xxcdefghijk
    print("aabbaaccaadd".replace("aa", "xx"))   # xxbbxxccxxdd
    print("aabbaaccaadd".replace("aa", "xxx"))  # xxxbbxxxccxxxdd
    print("aabbaaccaadd".replace("aa", "xx", 2)) # xxbbxxccaadd
    print("aabba".replace("aa", "xx", 2)) # xxbba
    print("aaabbaabbaaa".rfind("aa")) # 10
    print("aaabbaaabbaaa".rfind("aa", 7, 8)) # -1
    print("aaabbaaabbaaa".rfind("aa", 6, 8)) # 6
    print("hjk".rjust(10))
    print("hjk".rjust(10, '*'))
    print("   lmn   ".rstrip())
    print("   lmnopq".rstrip("qop")) # "   lmn"
    print(s2.startswith("abcd"))        # True
    print(s2.startswith("abcd", 4))     # False
    print(s2.startswith("efg", 4, 100)) # True
    print(s2.startswith("efg", 4, 7))   # True
    print(s2.startswith("efg", 4, 6))   # False
    print(s2.startswith("efg", 3, 7))   # False
    print("   abc   ".strip())
    print(s2.strip("abjk"))
    print("ab10efg".upper())
    print("42".zfill(5))
    print("-42".zfill(5)) # -0042
    print("++42".zfill(10)) # +000000+42
    
    # slice method call
    s = "1234" + s2[4:7].strip()
    print(s)    # 1234efg

    
test_strings()

#####################################################################
# Testing multi-line and multi-statement handling
print("a",
    "b",
    "c")

s = """line 1
line 2"""

x = 1 + \
2

print(s,
      "hello world",
      x)

print(s[1:
      3])
      
x = 1; y = 2; print(x)


#####################################################################
# Testing control flow

# if statement testing
print('Testing "if" statement')

def if_test1(x):
    x = 5
    if x<=5: print("x<=5")
    elif x==6: print("x==6")
    else: print("x > 6")
    
    
if_test1(4)
if_test1(5)
if_test1(6)
if_test1(7)


def if_test2(x, y):
    if x<=0:
        if y <= 0:
            print("x<=0, y<=0")
        elif y == 1:
            print("x<=0, y==1")
        elif y >= 2:
            print("x<=0, y>=2")

    elif x==1:
        if y <= 0:
            print("x==1, y<=0")
        elif y == 1:
            print("x==1, y==1")
        elif y >= 2:
            print("x==1, y>=2")

    elif x==2 or x==3:
        if y <= 0:
            print("x==2 or 3, y<=0")
        elif y == 1:
            print("x==2 or 3, y==1")
        elif y >= 2:
            print("x==2 or 3, y>=2")

    else:
        if y <= 0:
            print("x>3, y<=0")
        elif y == 1:
            print("x>3, y==1")
        elif y >= 2:
            print("x>3, y>=2")
       
def run_if_test2():    
    if_test2(-1, -1)
    if_test2(0, -1)
    if_test2(0, 0)
    if_test2(0, 1)
    if_test2(0, 2)
    if_test2(0, 3)
    if_test2(1, 1)
    if_test2(2, -1)
    if_test2(2, 0)
    if_test2(2, 1)
    if_test2(2, 2)
    if_test2(2, 3)
    if_test2(3, 3)
    if_test2(4, -1)

    
run_if_test2()

# while loop
def test_while():
    print("""Testing "while" loop""")
    i = 0
    while i < 10:
        i += 1
        if i==3: 
            continue

        print(i, end=' ')

        if i == 7: 
            break

    print()

    s = "hello world"
    i = 0
    while s[i] != 'w': i += 1
    print(i)

    
test_while()

# for loop
def test_for():
    print('Testing "for" loop')
    x = 11

    for i in range(1, 17, 2):
        if i==5: continue

        print(i, end = ' ')

        if i==x: break
        
    print() 
    
    for i in range(0,5): print(i, end=' ')
    print()
    
    s = "hello world"

    for i in range(0, len(s)):
        print(s[i], end='')
    print()

    for c in s:
        print(c, end=' ')
    print()
        
        
test_for()

#####################################################################
# Test functions
print("Testing functions")

def f1(x1, x2):
    return x1 + x2

x = f1(3, 5)
print(x)

# function nesting
x = f1(f1(1+2, 3+4), 
       f1(5+6, f1(7+8, 9+10)))
print(x)

print(f1(f1(1+2, 3+4), 
         f1(5+6, f1(7+8, 9+10))))
    
# local variable
x1 = 4

def f2():
    x1 = 3
    print(x1)

f2()
print(x1)

# global statement
x = 1
y = 2
print(x, y)

def f3():
    global x, y
    x = 10; y = 20

f3()
print(x, y)

# default arguments
def f4(x, y):
    print(x, y)

f4(x=1, y=2)
f4(y=1, x=2)
    
def f5(x, y=1, z=2):
    print(x, y, z)

f5(1, z=3)
f5(x=3)

# ignore interpreter type hints
def f(s:str, x:int):
    print(s.upper())
    print(x+10)

f("function with type hint", 10)
# FUNCTION WITH TYPE HINT
# 20

#####################################################################
# Test lists
def test_lists():
    print("Testing lists")

    # list expression
    y = 10
    x = [1 + y, 2*y, 3 + 2*y, 4, "hello"]
    print(x) # [11, 20, 23, 4, 'hello']

    x  = [[y-9, 2], [y-7, 4], y-5]
    print(x) # [[1, 2], [3, 4], 5]

    # list operator support
    # in and not in
    x = [1, 2, 3, 4]
    print(2 in x)
    print(-1 in x)
    print(-1 not in x)

    x = [1, [2, 3], 4, "five", "six", None, True]
    print(2 in x)
    print(5 in x)
    print("five" not in x)
    print(None in x)
    print(True not in x)
    y = [2, 3]
    print(y in x)

    # + and *
    x = [1, [2, 3], "four"]
    y = [10, 11, 12]
    print(x + y)
    print(x * 3)
    print(4 * y)

    # ==
    x = [1, 2, 3, 4]
    y = [1, 2, 3]
    print(x == y)       # False
    y = [1, 2, 3, 4]
    print(x == y)       # True
    y = [1, 2, 3, 5]
    print(x == y)       # False

    # list methods support
    # .index() .count()
    x = [1, 2, 2, 2, 3, 4, 1, 2]
    print(x.index(2))       # 1
    print(x.index(2, 3))    # 3
    print(x.index(1, 1, -1))    # 6
    print(x.index(1, -100, 100))    # 0
    # print(x.index(10)) # ValueError not supported
    # print(x.index(3, 0, 3)) # ValueError not supported
    print(x.count(2))       # 4
    print(x.count(10))      # 0

    # .append(), .clear()
    x = [1, 2, 3, 4]
    x.append(5)
    x.append([6, 7])
    print(x)    # [1, 2, 3, 4, 5, [6, 7]]

    x.clear()
    print(x)    # []

    # .copy()
    x = [1, 2, 3, 4, 5, [6, 7, 8], 9]
    y = x.copy()
    print(y)            # [1, 2, 3, 4, 5, [6, 7, 8], 9]
    y[0:4:2] = [0, 0]
    y[5][0] = 106
    y[5][1] = 107
    print(y)        # [0, 2, 0, 4, 5, [106, 107, 8], 9]
    print(x)        # [1, 2, 3, 4, 5, [106, 107, 8], 9]

    # .extend() .insert() .pop() .remove(), .reverse()
    x = [1, 2, 3, 4]
    x.extend([5, 6, 7])
    print(x)    # [1, 2, 3, 4, 5, 6, 7]

    x[len(x):] = [8, 9, 10]
    print(x)    # [1, 2, 3, 4, 5, 6, 7, 8, 9, 10]

    x = []
    x.extend(range(1, 20, 3))
    print(x) # [1, 4, 7, 10, 13, 16, 19]

    x.insert(4, 20)
    x.insert(4, [100, 101, 102])
    print(x)    # [1, 2, 3, 4, [100, 101, 102], 20, 5, 6, 7, 8, 9, 10]

    i = x.pop(4)
    print(i, x) # [100, 101, 102] [1, 2, 3, 4, 20, 5, 6, 7, 8, 9, 10]
    i = x.pop()
    print(i, x) # 10 [1, 2, 3, 4, 20, 5, 6, 7, 8, 9]

    x.remove(20)
    print(x) # [1, 2, 3, 4, 5, 6, 7, 8, 9]

    x.reverse()
    print(x) # [9, 8, 7, 6, 5, 4, 3, 2, 1]

    # .sort()
    x = [3, 1, 2, 6, 5]
    x.sort()
    print(x) # [1, 2, 3, 5, 6]

    x = [3, 1, 2, 6, 5]
    x.sort(reverse=True)
    print(x) # [6, 5, 3, 2, 1]

    # slice notation - read
    x = [1, 2, 3, 4, 5, 6, 7, 8]

    print(x[0])
    print(x[1:3])
    print(x[1:7:2]) # [2, 4, 6]
    print(x[:8:1])
    print(x[4:])
    print(x[:-2]) # [1, 2, 3, 4, 5, 6]
    print(x[6:0:-1])
    print(x[6::-1])
    print(x[::-1]) # [8, 7, 6, 5, 4, 3, 2, 1]
    print(x[:-1:-1])
    print(x[::-2])
    print(x[::-3]) # [8, 5, 2]
    print(x[:])
    print(x[-2:])
    print(x[20:]) # []
    print(x[2:-2])
    print(x[5:-5])
    print(x[:20]) # [1, 2, 3, 4, 5, 6, 7, 8]

    y=1
    print(x[y:y+6:2]) # [2, 4, 6]

    y = [1, 2, 3]
    print(x[y[x[0]]]) # 3

    # slice notation - operator support
    # in and not in
    x = [1, 2, 3, 4, 5, 6, 7]
    print(2 in x[:])    # True
    print(2 in x[::])   # True
    print(2 in x[1:5])  # True
    print(2 in x[3:5])      # False
    print(2 not in x[1:5])  # False

    x = [1, [2, 3], 4, "five", "six", None, True]
    print(2 in x[-100:100])     # False
    print("five" in x[1:6:2])   # True
    print("five" in x[1:100:3]) # False
    print(True not in x[3:-1])  # True
    print(None in x[3:-2:2])    # False
    print(None in x[3:-2:3])    # False
    y = [2, 3]
    print(y in x[0::2])         # False
    print(y in x[0::])          # True

    # + and *
    x = [1, [2, 3], "four"]
    y = [10, 11, 12, 13, 14]

    print(x[0:2] + y[2:])   # [1, [2, 3], 12, 13, 14]
    print(x[0:2] * 2)       # [1, [2, 3], 1, [2, 3]]
    print(x[0:2] + y)       # [1, [2, 3], 10, 11, 12, 13, 14]
    print(3 * y[1:3])       # [11, 12, 11, 12, 11, 12]
    print(3 * y[3:4])       # [13, 13, 13]
    print(y + x[0:2])       # [10, 11, 12, 13, 14, 1, [2, 3]]

    # ==
    x = [1, 2, 3, 4]
    y = [1, 2, 3]
    print(x[0:3] == y)      # True
    print(x[0:2] == y)      # False
    print(x[1:3] == y[1:3]) # True
    print(x[1:3] == y[1:100])   # True
    print(x[0:2] == y[1:100])   # False
    print(x[2::-1] == y[::-1])  # True

    # operator involving individual values
    x = [1, [2, 3], "four"]
    y = [10, 11, 12, 13, 14]
    print(x[1] * 3)         # [2, 3, 2, 3, 2, 3]
    print(x[2] * 2)         # fourfour
    print(3 * y[2])         # 36
    print(x[y[2] - y[0]])   # four
    print(y[4] / y[0])      # 1.4
    print(x[0] + y[3])      # 14

    print(x[0] == y[3] - y[2])  # True
    print(x[0] == y[4] - y[2])  # False
    print(x[0] < y[4] - y[2])   # True
    print(x[0] > y[4] - y[2])   # False

    # sequentially nested slices
    x = [1, [2, 3], "four"]
    y = [10, 11, 12, 13, 14]
    print(x[1][0] * (x[0] + y[3]))  # 28
    print(x[1][1] * x[0] + y[3])    # 16
    print(x[0] + x[1][1] * y[3])    # 40

    # slice notation - methods support 
    # .index() .count()
    x = [1, 2, 2, 2, 3, 4, 1, 2]
    print(x[2:].index(2))       # 0
    print(x[0::2].index(3))     # 2
    print(x[1:].index(2, 3))    # 6
    print(x[1:].index(1, 1, -1))   # 5
    print(x[::-1].index(1))  # 1
    print(x[::-1].index(1, 2, 100))  # 7
    print(x[:0:-1].index(1, -100, 100)) # 1
    print(x[-3::-1].index(1, 2, 100)) # 5
    print(x[2::2].index(1, -100, 100))  # 2
    print(x[0::2].count(2))     # 1

    # slice notation - slice assignment
    x = [0, 1, 2, 3, 4, 5, 6]

    x[1] = 10
    print(x)    # [0, 10, 2, 3, 4, 5, 6]

    x[2:4] = [7, 8, 9, 10]
    print(x)    # [0, 10, 7, 8, 9, 10, 4, 5, 6]

    x[1:3:1] = [11, 12, 13]
    print(x)    # [0, 11, 12, 13, 8, 9, 10, 4, 5, 6]

    x[1:5:2] = [14, 15]
    print(x)    # [0, 14, 12, 15, 8, 9, 10, 4, 5, 6]

    y = 1
    x[y:y+4:2] = [16, 17]
    print(x)    # [0, 16, 12, 17, 8, 9, 10, 4, 5, 6]

    x[2] = [18, 19, 20]
    print(x)    # [0, 16, [18, 19, 20], 17, 8, 9, 10, 4, 5, 6]

    x[2][1] = [21, 22, 23]
    print(x)    # [0, 16, [18, [21, 22, 23], 20], 17, 8, 9, 10, 4, 5, 6]

    x[2][1][1] = [24]
    x[2][1][2] = [25]
    print(x)    # [0, 16, [18, [21, [24], [25]], 20], 17, 8, 9, 10, 4, 5, 6]

    x[7:4:-1] = [26, 27, 28]
    print(x)    # [0, 16, [18, [21, [24], [25]], 20], 17, 8, 28, 27, 26, 5, 6]

    x[4:9] = []
    print(x)    # [0, 16, [18, [21, [24], [25]], 20], 17, 6]

    x = [0, 1, 2, 3, 4, 5]
    x[2:5] = [101, 102, 103]
    print(x)    # [0, 1, 101, 102, 103, 5]

    x = [0, 1, 2, 3, 4, 5]
    x[2:2] = [101, 102, 103]
    print(x)    # [0, 1, 101, 102, 103, 2, 3, 4, 5]

    x = [0, 1, 2, 3, 4, 5]
    x[2:1] = [101, 102, 103]
    print(x)    # [0, 1, 101, 102, 103, 2, 3, 4, 5]

    x = [0, 1, 2, 3, 4, 5]
    x[-3:5] = [101, 102, 103]
    print(x)    # [0, 1, 2, 101, 102, 103, 5]

    x = [0, 1, 2, 3, 4, 5]
    x[-3:] = [101, 102, 103]
    print(x)    # [0, 1, 2, 101, 102, 103]

    x = [0, 1, 2, 3, 4, 5]
    x[-100:4] = [101, 102, 103]
    print(x)    # [101, 102, 103, 4, 5]

    x = [0, 1, 2, 3, 4, 5]
    x[:4] = [101, 102, 103]
    print(x)    # [101, 102, 103, 4, 5]

    x = [0, 1, 2, 3, 4, 5]
    x[1:4] = "abcd"
    print(x)    # [0, 'a', 'b', 'c', 'd', 4, 5]

    x = [0, 1, 2, 3, 4, 5]
    x[1:3] = range(100, 105, 1)
    print(x)    # [0, 100, 101, 102, 103, 104, 3, 4, 5]

    # built in functions
    x = list(range(0, 20, 2)) + [-1, 40]
    print(x)        # [0, 2, 4, 6, 8, 10, 12, 14, 16, 18, -1, 40]
    print(len(x))   # 12
    print(min(x))   # -1
    print(max(x))   # 40

    s = "abcd" + "efg" + "za"
    x = list(s[2:])
    print(x)        # ['c', 'd', 'e', 'f', 'g', 'z', 'a']
    print(len(x))   # 7
    print(min(x))   # a
    print(max(x))   # z

    x = list(range(0, 10, 1))
    y = list(x[0::2])
    y[0] = 100
    print(x, y) # [0, 1, 2, 3, 4, 5, 6, 7, 8, 9] [100, 2, 4, 6, 8]

    print(min([1, 2, 3, -1, 4]))    # -1
    print(min("abcd"))          # a
    print(min("abcd", "abbd", "abzd"))  # abbd
    print(max("abcd"))          # d
    print(max("abcd", "abbd", "abzd"))  # abzd

    # list copy behavior
    x = [1, 2, 3, 4, 5, 6]
    y = x
    y[2] = 100
    print(x, y) # [1, 2, 100, 4, 5, 6] [1, 2, 100, 4, 5, 6]

    y = x[:]
    y[0] = 100
    print(x, y) # [1, 2, 100, 4, 5, 6] [100, 2, 100, 4, 5, 6]

    x = [1, 2, 3, 4, 5, 6]
    y = x[:4]
    y[0] = 30
    print(x, y) # [1, 2, 3, 4, 5, 6] [30, 2, 3, 4]

    x = [[1, 2, 3], 10, 11, 12, 13]
    y = x[0:4]
    y[0][1] = 100
    y[1] = 20
    print(x, y) # [[1, 100, 3], 10, 11, 12, 13] [[1, 100, 3], 20, 11, 12]

    s = "abcd" + "efg"
    s2 = s[:]
    print(s, s2) # abcdefg abcdefg

    s2 = s[2:6]
    print(s, s2) # abcdefg cdef

    # del keyword
    x = [1, 2, 3, 4, 5, 6, 7, 8]
    del x[3]
    print(x)    # [1, 2, 3, 5, 6, 7, 8]

    y = 2
    del x[y:y+8:2]
    print(x, y) # [1, 2, 5, 7] 2

    x = [1, 2, 3, 4, 5, 6, 7, 8]
    del x[1:5]
    print(x)    # [1, 6, 7, 8]

    x = [1, 2, [3, 4, 5], 6]
    del x[2][1]
    print(x)    # [1, 2, [3, 5], 6]

    x = [1, 2, [3, 4, 5], 6]
    del x[2][0:2]
    print(x)    # [1, 2, [5], 6]

    del x[:]
    print(x)    # []

    # for loop support
    x = []
    x.extend(range(1, 19, 3))
    for i in x:
        print(i, end=' ')
    print() # 1 4 7 10 13 16

    x = ['a', 'b', 'c', 'd', 'e', 'f']
    for i in range(0, len(x)):
        print(x[i], end=' ')
    print() # a b c d e f

    for i in range(0, len(x), 2):
        print(x[i], end=' ')
    print() # a c e

    for i in x[1:3]:
        print(i, end=' ')
    print() # b c

    # no copy on write support
    x = [5, 6, 7, 8, 9, 10, 11]
    x_copy = x[1:5] # explicit copy needed
    for i in x_copy:
        x[i - 5 + 1] += 10
        print(i, end = ' ')
    print() # 6 7 8 9

    print(x) # [5, 6, 17, 18, 19, 20, 11]

    x = [5, 6, 7, 8, 9, 10, 11]
    for i in x:
        next_index = i - 5 + 1
        if next_index < len(x):
            x[next_index] += 10
        print(i, end = ' ')
    print() # 5 16 7 18 9 20 11

    # string methods that require list support
    # string::join(), partition(), rpartition()
    print("*".join(["aa", "bb", "cc"])) # aa*bb*cc
    print("*".join(["abc"])) # abc
    print("*".join("abc")) # a*b*c
    print("*".join(["a", str(2), 'c'])) # a*2*c
    print("*".join("abcdefg"[1::2])) # b*d*f

    x = "ab cd efg".partition(" ")
    print(x[0], x[1], x[2]) # ab   cd efg
    x = "ab cd efg".partition("xx")
    print(x[0], x[1], x[2]) # ab cd efg
    x = "ab cd efg".partition("ab")
    print(x[0], x[1], x[2]) #  ab  cd efg

    s2 = "ab**cd*efg***hi"
    x = s2.rpartition('**')
    print(x[0], x[1], x[2]) # ab**cd*efg* ** hi

    # string::split(), rsplit(), splitlines()
    s2 = "ab cd efg hi jk lmn"
    print(s2.split()) # ['ab', 'cd', 'efg', 'hi', 'jk', 'lmn']
    print("a    b".split())     # ['a', 'b']
    print("a    b".split(' '))  # ['a', '', '', '', 'b']
    print("a    b   c".split(maxsplit=1)) # ['a', 'b   c']
    print(s2.split(maxsplit=3)) # ['ab', 'cd', 'efg', 'hi jk lmn']
    print("ab**cd*efg**hi".split('*', 3)) # ['ab', '', 'cd', 'efg**hi']
    print("ab**cd*efg**hi".split(sep='*')) # ['ab', '', 'cd', 'efg', '', 'hi']
    print("ab**cd***efg****hi".split("**")) # ['ab', 'cd', '*efg', '', 'hi']
    print("ab**cd**efg**hi".split(maxsplit=1, sep='**')) # ['ab', 'cd**efg**hi']

    print("ab  cd efg  hi".rsplit(" ", 2))      # ['ab  cd efg', '', 'hi']
    print("ab  cd efg  hi".rsplit(maxsplit=2))  # ['ab  cd', 'efg', 'hi']
    print("ab**cd*efg**hi".rsplit("*", 5))      # ['ab', '', 'cd', 'efg', '', 'hi']
    print("ab**cd*efg**hi".rsplit("*", 6))      # ['ab', '', 'cd', 'efg', '', 'hi']
    print("**ab**cd*efg**hi".rsplit("**", 4))   # ['', 'ab', 'cd*efg', 'hi']
    print("**ab**cd*efg**hi".rsplit("**", 3))   # ['', 'ab', 'cd*efg', 'hi']
    print("**ab**cd*efg**hi".rsplit("**", 2))   # ['**ab', 'cd*efg', 'hi']
    print("ab**cd***efg****hi".rsplit("**", 3))  # ['ab**cd*', 'efg', '', 'hi']
    print("ab**cd*efg**hi".rsplit(maxsplit=3, sep='*')) # ['ab**cd', 'efg', '', 'hi']

    print("ab\ncd\r\nefg\n\rhijk\rlmn".splitlines()) # ['ab', 'cd', 'efg', '', 'hijk', 'lmn']
    x = "ab\ncd\r\nefg\n\rhijk\rlmn".splitlines(True)
    print(x) # ['ab\n', 'cd\r\n', 'efg\n', '\r', 'hijk\r', 'lmn']
    x = "ab\ncd\r\nefg\n\rhijk\rlmn\n\n".splitlines(True)
    print(x) # ['ab\n', 'cd\r\n', 'efg\n', '\r', 'hijk\r', 'lmn\n', '\n']


test_lists()


#####################################################################
# Test dictionary
# dictionary
def test_dictionary():
    print("Testing dictionary")

    y = 10
    d = { "apples": 5, "oranges": 10,
          "stuffed" + " animals": 2 * y}
    print(d["apples"])              # 5
    print(d["stuffed animals"])     # 20

    d2 = {"apples": 5, "oranges": 10,
          "stuffed animals": 20}

    print(d==d2)        # True

    d["toys"] = 100
    print(d["toys"])    # 100

    print(d == d2)      # False
    print(d != d2)      # True


    # .keys()
    d = { "apples": 5 }

    x = list(d.keys())
    print(x)                # ['apples']

    for key in d.keys():
        print(key, d[key])  # apples 5

    # .values
    x = list(d.values())
    print(x)                # [5]

    # when there are more than 1 element, C# and Python might order things differently
    d["oranges"] = 10
    d["stuffed animals"] = d["oranges"] * 2
    d["toys"] = d["stuffed animals"] * 2

    x = list(d.keys())
    x.sort()

    for key in x:
        print(key, d[key])

    # apples 5
    # oranges 10
    # stuffed animals 20
    # toys 40

    x = list(d.values())
    x.sort()
    print(x)

    # [5, 10, 20, 40]

    # len()
    print(len(d))           # 4
    print(len(d.keys()))    # 4
    print(len(d.values()))  # 4

    # print(d) # might not be the same in C# and Python

    # del, in
    d = { "toys": 5 }

    print("toys" in d)      # True
    print("toys" not in d)  # False

    del d["toys"]
    print(d)                # {}

    print("toys" in d)      # False
    print("toys" not in d)  # True

    # dictionary methods
    # .clear()
    d= {1: "a", 2: "b"}
    print(len(d)) # 2
    d.clear()
    print(len(d)) # 0

    # .copy()
    d= {1: "a", 2: "b", 3: [100, 101]}
    d2 = d.copy()
    d2[2] = "c"
    d2[3][1] = 201

    x = list(d.keys())
    x.sort()
    for key in x:
        print(key, d[key])
    # 1 a
    # 2 b
    # 3 [100, 201]

    x = list(d2.keys())
    x.sort()
    for key in x:
        print(key, d2[key])
    # 1 a
    # 2 c
    # 3 [100, 201]

    # .get()
    d = {6: "six", 7: "seven"}
    print(d.get(6))             # six
    print(d.get(6, "default"))  # six
    print(d.get(8))             # None
    print(d.get(8, "default"))  # default

    # .pop()
    one = "one"
    two = "two"
    d = {one: 1, two: 2}
    print(d.pop("three", -1))   # -1
    print(d.pop(two, -1))       # 2
    print(len(d))               # 1

    #. setdefault()
    d = {}
    print(d.setdefault(one))    # None
    print(d.setdefault(two, 2)) # 2
    print(d[one])               # None
    print(len(d))               # 2

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


test_dictionary()



print("\n\n")