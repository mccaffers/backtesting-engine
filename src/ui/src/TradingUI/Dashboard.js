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

    const [ series, setSeries ] = useState({
            animationEnabled: true,
            theme: "light2", // "light1", "light2", "dark1", "dark2"
            exportEnabled: true,
            title: {
                text: ""
            },
            subtitles: [{
                text: ""
            }],
            axisX: {
            },
            axisY: {
                prefix: "",
                title: "",
                includeZero: false,
            },
            dataPointMinWidth: 5,
            data: [{				
                type: "candlestick",
                xValueType: "dateTime",
                risingColor: "#00D100",
                fallingColor: "#FF0000",  
                dataPoints: []
            }]
        }
    );

    const [ account, setAccount ] = useState(0);

    var chartRef = useRef();
    function UpdateChart(OHLCObj){
        
        const eventDate = +new Date(OHLCObj.d);

        setSeries((previousState) => {
            let previousDataPoints = previousState.data[0].dataPoints;

            let indexValue = previousDataPoints.findIndex((obj => obj.x===eventDate));
            // -1 means it doesn't exist, lets start a new element
            let priceEvent = [ OHLCObj.o, OHLCObj.h, OHLCObj.l, OHLCObj.c];
            if(indexValue==-1){
                const keepAmount = 40;
                if(previousDataPoints.length>keepAmount){
                    previousDataPoints = previousDataPoints.slice(previousDataPoints.length-keepAmount);
                }
                previousDataPoints = [...previousDataPoints, { y: priceEvent, x: eventDate} ];
                indexValue = previousDataPoints.length-1;
            } else {
                // dataPoints.current[indexValue.current].x = dataPoints.current[indexValue.current].x;
                previousDataPoints[indexValue].y = priceEvent;
            }
            let color = "#FF0000"; // red
            if(previousDataPoints[indexValue].y[3]>previousDataPoints[indexValue].y[0]){
                color="#00D100"; // green
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

            let color = '#007500'; // green
            if(OHLCObj.profit < 0){
                color ='#750000';
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

    function connectToSignalR(){
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
                        setAccount(parseFloat(message.Content).toFixed(2))
                    } else if(message.Activity == "trade"){
                        var OHLCObj = JSON.parse(message.Content);
                        AddTrade(OHLCObj);
                    }

                });
            })
            .catch(e => {
                console.log('Connection failed: ', e)
                setTimeout(connectToSignalR, 1000);
            });
    }

    // Initiate Functions
    useEffect(() => {
        connectToSignalR();
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
            {/* <TradeInput sendMessage={sendMessage} /> */}
            <div></div>
            <div class="wrapper">
                <header class="header">Backtesting</header>
                <article class="main">
                <CanvasJSChart options = {series}
                    onRef={(ref) => {
                        chartRef.current = ref;
                    }}
                />
                </article>
                <aside class="aside aside-1">
                    <div>Open Trades</div>
                    <div class="accountLabel">0</div></aside>
                <aside class="aside aside-2">
                    <div>Account</div>
                    <div class="accountLabel">{account}</div></aside>
                <footer class="footer">Trade History</footer>
            </div>
        </div>
    );
};

export default Chat;
