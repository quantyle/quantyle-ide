
# set starting cash
set_cash(15000)
set_fee(0.0025)
set_interval("1m")

# define two trading signals
v1 = sma(period=10, color="red")
v2 = ema(period=25, color="green")

# set buy rule
if v1 < v2:
    buy_market(size=25)

# set sell rule
if v1 > v2:
    sell_market(size=25)

print(get_cash())