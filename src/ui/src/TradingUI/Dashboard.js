import React, { useState, useEffect, useRef } from 'react';
import { HubConnectionBuilder, HttpTransportType} from '@microsoft/signalr';
import { flushSync } from 'react-dom'; 

import TradeWindow from './TradeWindow/TradeWindow';
import TradeInput from './TradeInput/TradeInput';
import TradeUpdate from './TradeUpdate';

import Chart from "react-apexcharts";
const signalRMsgPack = require("@microsoft/signalr-protocol-msgpack");

process.env["NODE_TLS_REJECT_UNAUTHORIZED"] = 0; 

const Chat = () => {

    var min= new Date("2020-01-01T22:01:12.821Z").getTime();
    var max= new Date("2020-01-05T22:04:12.821Z").getTime();
    var range = max - min

 
    const [ series, setSeries ] = useState(
        {
            "width": 1000,
            "series": [{
                "name": "series-1",
                "data": [
                ]}
            ],
            "options": {
                chart: {
                    animations: {
                        enabled:false
                    }
                  },
                  title: {
                    text: '',
                    align: 'left'
                  },
                  xaxis: {
                    type: 'datetime',
                  },
                  yaxis: {
                    tooltip: {
                      enabled: true
                    }
                },
                candlestick: {
                    wick: {
                      useFillColor: true,
                    }
                  }
            }
        }
  
    );

    const [ chat, setChat ] = useState([]);
    const [ account, setAccount ] = useState(0);
    const count = useRef(0);

    const latestChat = useRef(null);

    latestChat.current = chat;

    useEffect(() => {
        // Chart.exec("trading",'updateSeries', series);
        // console.log(series);
        // Chart.exec("trading",'updateSeries', series);
    });

    const indexValue = useRef(0);
    const timeValue = useRef(0);

    function UpdateChart(OHLCObj){
        
        setSeries((prevState) => {

            let newState = prevState;
            let newArray = prevState.series[0];
            const eventDate = new Date(OHLCObj.d).getTime();

            // Track current index to prevent a findIndex search on every tick
            if(indexValue.current === 0){
                indexValue.current = newArray.data.findIndex((obj => obj.x===eventDate));
            }

            // Save the current date
            if(timeValue.current === 0){
                timeValue.current = eventDate;
            }

            // If the date has changed lets update the current one and refresh the index
            if(timeValue.current!==eventDate){
                timeValue.current = eventDate;
                indexValue.current = newArray.data.findIndex((obj => obj.x===eventDate));
            }

            // const index = newArray.data.findIndex((obj => obj.x===eventDate));
            
            // -1 means it doesn't exist, lets start a new element
            let priceEvent = [ OHLCObj.o, OHLCObj.h, OHLCObj.l, OHLCObj.c];
            if(indexValue.current==-1){
                const keepAmount = 40;
                if(newArray.data.length>keepAmount){
                    newArray.data = newArray.data.slice(newArray.data.length-keepAmount);
                }
                newArray.data = [...newArray.data, { x: eventDate, y: priceEvent} ];
                indexValue.current = newArray.data.length-1;
            } else {
                newArray.data[indexValue.current].x = newArray.data[indexValue.current].x;
                newArray.data[indexValue.current].y = priceEvent;
            }

            newArray.name="candlestick";
            newArray.type="candlestick";
            // if(newArray.width === 1000){
            //     newArray.width = 999;
            // } else {
            //     newArray.width = 1000;
            // }

            let high=0
            let low=Number.MAX_SAFE_INTEGER;
        
            let minDate = null;
            let range = null;
            let max = eventDate;
            
            minDate = new Date(newArray.data[0].x).getTime();
            range = max-minDate;

            newArray.data.forEach(function(item){
                if(item.y[1] > high){
                    high = item.y[1];
                }
                if(item.y[2] < low){
                    low = item.y[2];
                }
            })

            newState.options = {
                ...newState.options,
                xaxis: {
                    min: minDate,
                    max: eventDate,
                    range: range
                },
                yaxis: {
                    min: low,
                    max: high+(Math.random()/10000000),
                }
            };

            newState.series = [
                newArray,{
                    name: 'line',
                    type: 'line',
                    data: [
                      {
                        x: minDate,
                        y: high
                      }, {
                        x: max,
                        y: low
                      }
                    ]
                  }
            ];
            return newState;

        });
    }
    
    useEffect(() => {

        
        const connection = new HubConnectionBuilder()
            .withUrl('https://localhost:5001/hubs/chat', { transport: HttpTransportType.WebSockets | HttpTransportType.LongPolling })
            .withHubProtocol(new signalRMsgPack.MessagePackHubProtocol())
            .withAutomaticReconnect()
            .build();

        connection.start()
            .then(result => {
                console.log('Connected!');

                connection.on('ReceiveMessage', message => {
                    
                    if(message.Activity === "trade"){
                        var OHLCObj = JSON.parse(message.Content);
                        UpdateChart(OHLCObj);
                    } else if(message.Activity === "account"){
                        setAccount(message.Content)
                    }

                    // Chart.exec('trading', "updateSeries");
                    // console.log(data);
            
                    // setChat(updatedChat);
                });
            })
            .catch(e => console.log('Connection failed: ', e));

    }, []);

    const sendMessage = async (user, message) => {
       
        const chatMessage = {
            user: user,
            message: message
        };

        console.log("sendMessage message: " + message);

        try {
            await  fetch('https://localhost:5001/chat/messages', { 
                method: 'POST', 
                body: JSON.stringify(chatMessage),
                headers: {
                    'Content-Type': 'application/json'
                }
            });
        }
        catch(e) {
            console.log('Sending message failed.', e);
        }
    }

    return (
        
        <div>
            <TradeInput sendMessage={sendMessage} />
            <hr />
            {/* <ChatWindow chat={chat}/> */}
            <div>Account: {account}</div>
            <Chart
              id="trading"
              options={series.options}
              series={series.series}
              width={series.width}
            />
        </div>
    );
};

export default Chat;
