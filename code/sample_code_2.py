
# set starting cash
set_cash(5000)
set_fee(0.0025)
set_interval("1m")

# define two trading signals
ema26 = ema(period=26, color="red")
ema12 = ema(period=12, color="green")
macd = ema12 - ema26
print("macd: ", macd)

vwap24 = vwap(period=24, color="yellow")
print("vwap: ", vwap24)
