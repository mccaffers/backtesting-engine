import React, { useState, useEffect, useRef } from 'react';
import { HubConnectionBuilder, HttpTransportType} from '@microsoft/signalr';
import { flushSync } from 'react-dom'; 

import TradeWindow from './TradeWindow/TradeWindow';
import TradeInput from './TradeInput/TradeInput';
import TradeUpdate from './TradeUpdate';

import CanvasJSReact from '../libs/canvasjs.react';
var CanvasJS = CanvasJSReact.CanvasJS;
var CanvasJSChart = CanvasJSReact.CanvasJSChart;

// import Chart from "react-apexcharts";
const signalRMsgPack = require("@microsoft/signalr-protocol-msgpack");

process.env["NODE_TLS_REJECT_UNAUTHORIZED"] = 0; 

const Chat = () => {

    var min= new Date("2020-01-01T22:01:12.821Z").getTime();
    var max= new Date("2020-01-05T22:04:12.821Z").getTime();
    var range = max - min

    
    const [ series, setSeries ] = useState(
        {
            animationEnabled: true,
            theme: "light2", // "light1", "light2", "dark1", "dark2"
            exportEnabled: true,
            dataPointWidth: 20,
            title: {
                text: ""
            },
            subtitles: [{
                text: ""
            }],
            axisX: {
                interval: 30,
                intervalType: "minute",
            },
            axisY: {
                prefix: "",
                title: "",
                includeZero: false,
            },
            data: [{				
                type: "candlestick",
                xValueType: "dateTime",
                risingColor: "green",
                fallingColor: "red",  
                dataPoints: []
            }]
        }
  
    );

    const [ chat, setChat ] = useState([]);
    const [ account, setAccount ] = useState(0);
    const count = useRef(0);

    const latestChat = useRef(null);

    latestChat.current = chat;

    useEffect(() => {
    });

    // const indexValue = useRef(0);
    const timeValue = useRef(0);

    var chartRef = useRef();

    function UpdateChart(OHLCObj){
        
        const eventDate = +new Date(OHLCObj.d);

        setSeries((previousState) => {
            let previousDataPoints = previousState.data[0].dataPoints;

            let indexValue = previousDataPoints.findIndex((obj => obj.x===eventDate));
            // -1 means it doesn't exist, lets start a new element
            let priceEvent = [ OHLCObj.o, OHLCObj.h, OHLCObj.l, OHLCObj.c];
            if(indexValue==-1){
                const keepAmount = 20;
                if(previousDataPoints.length>keepAmount){
                    previousDataPoints = previousDataPoints.slice(previousDataPoints.length-keepAmount);
                }
                previousDataPoints = [...previousDataPoints, { y: priceEvent, x: eventDate} ];
                indexValue = previousDataPoints.length-1;
            } else {
                // dataPoints.current[indexValue.current].x = dataPoints.current[indexValue.current].x;
                previousDataPoints[indexValue].y = priceEvent;
            }
            let color = "red";
            if(previousDataPoints[indexValue].y[3]>previousDataPoints[indexValue].y[0]){
                color="green";
            }
            previousDataPoints[indexValue].color = color;

            previousState.data[0].dataPoints = previousDataPoints;
            let candleSticks = previousState.data[0];



            // Remove old trades from the UI
            let alignedState = [];

            for(const item of previousState.data){
                if(item.type === "line"){
                    alignedState.push({
                            type: "line",
                            color:item.color,
                            dataPoints: item.dataPoints.filter(e => e.x > candleSticks.dataPoints[0].x)});
                }
            }
            previousState.data = [candleSticks, ...alignedState]

            return previousState;
        });

        chartRef.current.render();
        
    }

    function AddTrade(OHLCObj){
        
        setSeries((previousState) => {


            let color = 'green'; // green
            if(OHLCObj.profit < 0){
                color ='red';
            }
            let direction = "BUY";
            if(OHLCObj.direction === 1){
                direction="SELL";
            }
           
            previousState.data.push({
                type: "line",
                color:color,
                dataPoints: [
                    { x: +new Date(OHLCObj.openDate), y: OHLCObj.level, indexLabel: direction },
                    { x: +new Date(OHLCObj.closeDateTime), y: OHLCObj.closeLevel }
                ]
            });


            return previousState;

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
                    
                    if(message.Activity === "price"){
                        var OHLCObj = JSON.parse(message.Content);
                        UpdateChart(OHLCObj);
                    } else if(message.Activity === "account"){
                        setAccount(message.Content)
                    } else if(message.Activity == "trade"){
                        var OHLCObj = JSON.parse(message.Content);
                        AddTrade(OHLCObj);
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
            {/* <Chart
              id="trading"
              options={series.options}
              series={series.series}
              width={series.width}
            /> */}

            <CanvasJSChart options = {series}
            onRef={(ref) => {
                chartRef.current = ref;
              }}
            />
        </div>
    );
};

export default Chat;
