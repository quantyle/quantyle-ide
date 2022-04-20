# # Variable Initialization
# # "a" and "b" have been initialized on the compiled side
# print(a)    # 1
# print(b)    # 2
# print(a + b)  # 3

# # user function "user_add(...)" defined on the compiled side
# x = user_add(1.3, 2.4)
# print(x)    # 3.7

# # class "UserSum" is defined on the compiled side
# user_sum = UserSum(15)
# user_sum.add(5)

# x = user_sum.get()
# print(x)    # 20


# Variable Initialization
# "a" and "b" have been initialized on the compiled side
# print(a)    # 1
# print(b)    # 2
# print(a + b)  # 3

# user function "user_add(...)" defined on the compiled side
v1 = moving_average(period=15, color="red")
print(v1)    # 3.7
