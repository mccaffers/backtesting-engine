import React, { useState, useEffect, useRef } from 'react';
import { HubConnectionBuilder } from '@microsoft/signalr';

import ChatWindow from './ChatWindow/ChatWindow';
import ChatInput from './ChatInput/ChatInput';
import Chart from "react-apexcharts";

process.env["NODE_TLS_REJECT_UNAUTHORIZED"] = 0; 

const Chat = () => {

    var min= new Date("2020-01-01T22:01:12.821Z").getTime();
    var max= new Date("2020-01-05T22:04:12.821Z").getTime();
    var range = max - min

    const [ options, setOptions ] = useState({
        chart: {
            animations: {
                enabled:false
            },
            events: {
                beforeZoom: function(ctx) {
                  // we need to clear the range as we only need it on the iniital load.
                  ctx.w.config.xaxis.range = undefined
                }
              }
          },
          title: {
            text: 'CandleStick Chart',
            align: 'left'
          },
          xaxis: {
            type: 'datetime',
            min: min,
            max: max,
            range:range
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
    });

    const [ series, setSeries ] = useState([
        {
            "name": "series-1",
            "data": [
            ]
        }
  
    ]);

    const [ chat, setChat ] = useState([]);
    const count = useRef(0);

    const latestChat = useRef(null);

    latestChat.current = chat;

    useEffect(() => {
        // Chart.exec("trading",'updateSeries', series);
        // console.log(series);
        // Chart.exec("trading",'updateSeries', series);
    });


    useEffect(() => {

        const connection = new HubConnectionBuilder()
            .withUrl('https://localhost:5001/hubs/chat')
            .withAutomaticReconnect()
            .build();

        connection.start()
            .then(result => {
                console.log('Connected!');

                connection.on('ReceiveMessage', message => {
                    
                    var OHLCObj = JSON.parse(message.message);

                    setSeries((prevState) => {
                        let newArray = prevState[0];
                        const eventDate = new Date(OHLCObj.date).getTime();
                        const index = newArray.data.findIndex((obj => obj.x===eventDate));
                        
                        // -1 means it doesn't exist, lets start a new element
                        let priceEvent = [ OHLCObj.open, OHLCObj.high, OHLCObj.low, OHLCObj.close];
                        if(index==-1){
                            const keepAmount = 40;
                            if(newArray.data.length>keepAmount){
                                newArray.data = newArray.data.slice(keepAmount);
                            }
                            newArray.data = [...newArray.data, { x: eventDate, y: priceEvent} ];
                        } else {
                            newArray.data[index].y = priceEvent;
                        }

                        count.current++;
                        if(count.current > 200){
                            count.current=0;
                            let high=0
                            let low=Number.MAX_SAFE_INTEGER;
                        
                            let minDate = null;
                            let range = null;
                            let max = eventDate;
                            let slicedData = newArray.data;
                            
                            minDate = new Date(slicedData[0].x).getTime();
                            range = max-minDate;

                            slicedData.forEach(function(item){
                                if(item.y[1] > high){
                                    high = item.y[1];
                                }
                                if(item.y[2] < low){
                                    low = item.y[2];
                                }
                            })

                            setOptions((prevOptions) => {
                                return {
                                    ...prevOptions,
                                    xaxis: {
                                        min: minDate,
                                        max: eventDate,
                                        range: range
                                    },
                                    yaxis: {
                                        min: low,
                                        max: high,
                                    }
                                }
                            });
                        }

                        return [newArray];
                    });

                    

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
            <ChatInput sendMessage={sendMessage} />
            <hr />
            {/* <ChatWindow chat={chat}/> */}
            <Chart
              id="trading"
              options={options}
              series={series}
              type="candlestick"
              width="1000"
            />
        </div>
    );
};

export default Chat;
