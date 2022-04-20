# set starting cash
set_cash(15000)

# define two trading signals
v1 = sma(id="sma1", period=10, color="red")
v2 = sma(id="sma2", period=25, color="green")

# set buy rule
if v1 < v2:
    buy_market(size=25)

# set sell rule
if v1 > v2:
    sell_market(size=25)

print(get_cash())