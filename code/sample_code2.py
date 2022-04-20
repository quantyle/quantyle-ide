# sample_code2

# set starting cash
set_cash(5000)
set_fee(0.0025)

# define two trading signals
v1 = ema(period=20, color="red")
v2 = ema(period=35, color="green")

# set buy rule
if v1 < v2:
    buy_market(size=5)

# set sell rule
if v1 > v2:
    sell_market(size=5)
