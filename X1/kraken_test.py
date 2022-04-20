#!/usr/bin/env python3

# Import WebSocket client library (and others)
import sys
import json
import time
import _thread
# import signal
import websocket


api_symbol = "XBT/USD"
api_depth = 25
ui_depth = 25

# Define order book variables
api_book = {'bid': {}, 'ask': {}}

# Define order book update functions


def dicttofloat(data):
    return float(data[0])


def api_book_update(api_book_side, api_book_data):
    for data in api_book_data:
        price_level = data[0]
        volume = data[1]

        if float(volume) > 0.0:
            api_book[api_book_side][price_level] = volume
        else:
            api_book[api_book_side].pop(price_level)

        # print(len(api_book['bid']))
        # print(len(api_book['ask']))
        # print(api_book['ask'][0])
        # print(api_book['bid'][0])
        if api_book_side == 'bid':
            api_book['bid'] = dict(
                sorted(api_book['bid'].items(), key=dicttofloat, reverse=True)[:api_depth])
        elif api_book_side == 'ask':
            api_book['ask'] = dict(
                sorted(api_book['ask'].items(), key=dicttofloat)[:api_depth])

# Define WebSocket callback functions


def ws_thread(*args):
    ws = websocket.WebSocketApp(
        'wss://ws.kraken.com/', on_open=ws_open, on_message=ws_message)
    ws.run_forever()


def ws_open(ws):
    ws.send('{"event":"subscribe", "subscription":{"name":"book", "depth":%(api_depth)d}, "pair":["%(api_symbol)s"]}' % {
            'api_depth': api_depth, 'api_symbol': api_symbol})
    # ws.send('{"event":"subscribe", "subscription":{"name":"trade"}, "pair":["%(api_symbol)s"]}' % {'api_symbol':api_symbol})


def ws_message(ws, ws_data):
    api_data = json.loads(ws_data)
    # print(api_data)
    if 'event' in api_data:
        return
    else:
        # process snapshot
        if 'as' in api_data[1]:
            api_book_update('ask', api_data[1]['as'])
            api_book_update('bid', api_data[1]['bs'])
        else:
            # normal update
            for data in api_data[1:len(api_data)-2]:
                # print("data: ", data)
                if 'a' in data:
                    api_book_update('ask', data['a'])

                elif 'b' in data:
                    api_book_update('bid', data['b'])


# Start new thread for WebSocket interface
_thread.start_new_thread(ws_thread, ())

# Output order book (once per second) in main thread
try:
    while True:
        # print("test")
        if len(api_book['bid']) < api_depth or len(api_book['ask']) < api_depth:
            time.sleep(1)
        else:
            bid = sorted(api_book['bid'].items(),key=dicttofloat, reverse=True)
            ask = sorted(api_book['ask'].items(), key=dicttofloat, reverse=True)
            
            print("book")

            for i in range(api_depth):
                print("ASK", ask[i][0], ask[i][1])
            print("--------")
            for j in  range(api_depth):
                print("BID", bid[j][0], bid[j][1])
            print()
            
            time.sleep(1)
except KeyboardInterrupt:
    sys.exit(0)
