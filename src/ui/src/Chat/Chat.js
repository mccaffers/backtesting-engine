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
                {
                    "x": "2020-01-01T22:01:12.821Z",
                    "y": [
                        1.1216,
                        1.12228,
                        1.12124,
                        1.12185
                    ]
                }
            ]
        }
  
    ]);

    const [ chat, setChat ] = useState([]);
    const latestChat = useRef(null);

    latestChat.current = chat;

    useEffect(() => {
        // Chart.exec("trading",'updateSeries', series);
        console.log(series);
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
                    // const updatedChat = [...latestChat.current];
                    // updatedChat.push(message);

                    // {user: 'test', message: '[{"date":"2020-01-09T00:02:13.383+00:00","open":1.…1063,"high":1.111,"low":1.11037,"complete":true}]'}
                    
                    var OHLCObj = JSON.parse(message.message)[0];


                    // setData([...data, 1])
                    // setData([...data, 1])

                    setSeries((prevState) => {
                        let newArray = Array.from(prevState)[0];
            
                        newArray.data = [...newArray.data, { x: new Date(OHLCObj.date).toISOString(), y: [ OHLCObj.open, OHLCObj.high, OHLCObj.low, OHLCObj.close]} ];
                        // if(newArray.data.length>5){
                        //     newArray.data=newArray.data.slice(Math.max(newArray.data.length - 5, 1));
                        // }
                        let response = [newArray]
                        // console.log(response);
                        return response;
                    });

                    setOptions((prevOptions) => {
                        // prevOptions.chart.xaxis
                        // prevOptions.chart.xaxis.max = new Date(OHLCObj.date).getTime();

                        var currentSeries = series[0];
                        var currentData = currentSeries.data;
                        var high=0;
                        var low=Number.MAX_SAFE_INTEGER;
                    

                        var minDate = null;
                        var range = null;
                        var max = new Date(OHLCObj.date).getTime();
                        if(currentData.length > 50){
                            var slicedData=currentData.slice(currentData.length-50, currentData.length-1);
                            minDate = new Date(slicedData[0].x).getTime();
                            range = max-minDate;

                            slicedData.forEach(function(item){
                                if(item.y[1] > high){
                                    high = item.y[1];
                                }
                            })
                            slicedData.forEach(function(item){
                                if(item.y[2] < low){
                                    low = item.y[2];
                                }
                            })
                        } else {
                            currentData.forEach(function(item){
                                if(item.y[1] > high){
                                    high = item.y[1];
                                }
                            })
                            currentData.forEach(function(item){
                                if(item.y[2] < low){
                                    low = item.y[2];
                                }
                            })
                        }

                        var output = {
                            ...prevOptions,
                            xaxis: {
                                min: minDate,
                                max: new Date(OHLCObj.date).getTime(),
                                range: range

                            },
                            yaxis: {
                                min: low,
                                max: high,
                            }
                          }
                          
                        return output;
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
