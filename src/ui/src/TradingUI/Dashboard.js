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
            theme: "light1", // "light1", "light2", "dark1", "dark2"
            exportEnabled: true,
            title: {
                text: ""
            },
            subtitles: [{
                text: ""
            }],
            axisX: {
                margin: 15,
            },
            axisY: {
                prefix: "",
                title: "",
                includeZero: false,
            },
            dataPointMinWidth: 10,
            data: [{				
                type: "candlestick",
                xValueType: "dateTime",
                risingColor: "#00D100",
                fallingColor: "#FF0000",  
                connectNullData:true,
                dataPoints: []
            }]
        }
    );

    const [ account, setAccount ] = useState(0);
    const [ openTrades, setOpenTrades] = useState();
    const [ closedTrades, setClosedTrades] = useState([]);

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
                            _tradingType: item._tradingType,
                            _tradingKey: item._tradingKey,
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

    function SaveClosedTradeForTable(OHLCObj){
        setClosedTrades((previousState) => {
            previousState.push(OHLCObj);
            return previousState;
        });
    }

    function AddTrade(OHLCObj, tradeType = "closed"){
        
        setSeries((previousState) => {

            let color = '#007500'; // green
            if(OHLCObj.profit < 0){
                color ='#750000';
            }
            let direction = "BUY";
            if(OHLCObj.direction === 1){
                direction="SELL";
            }

            let newState = previousState;

            if(tradeType === "closed" ){
                // Does the key exist && is it open
                if(newState.data.some(item => item._tradingKey === OHLCObj.key) && 
                    newState.data.some(item => item._tradingType === "open")){
                        console.log("found open trade to remove");
                        // Remove it
                        newState.data = newState.data.filter(function(item) {
                            return item._tradingKey !== OHLCObj.key
                        })
                }
            }
            
            // Lets check if the open trade exists, only need to add it once
            if(tradeType === "open"){
                if(newState.data.some(item => item._tradingKey === OHLCObj.key)){
                    const index = newState.data.findIndex((element, index) => {
                        if (element._tradingKey === OHLCObj.key) {
                          return true
                        }
                    });

                    if(index!==-1){
                        if(newState.data[index].dataPoints.length == 1){
                            newState.data[index].dataPoints[0].x = +new Date(OHLCObj.openDate);
                            newState.data[index].dataPoints[0].y = OHLCObj.level;

                            newState.data[index].dataPoints.push([]);
                        }
                        newState.data[index].dataPoints[1].x = +new Date(OHLCObj.closeDateTime);
                        newState.data[index].dataPoints[1].y = OHLCObj.closeLevel;
                        return newState;
                    }
                }
            }

            // Otherwise, add the open trades that doesn't exist, or a new closed trade
            newState.data.push({
                _tradingType: tradeType,
                _tradingKey: OHLCObj.key,
                type: "line",
                color:color,
                dataPoints: [
                    { x: +new Date(OHLCObj.openDate), y: OHLCObj.level, indexLabel: direction },
                    { x: +new Date(OHLCObj.closeDateTime), y: OHLCObj.closeLevel }
                ]
            });
        
            return newState;

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
                        SaveClosedTradeForTable(OHLCObj);
                    } else if(message.Activity == "openTrades"){
                        var parsedOpenTrades = JSON.parse(message.Content);
                        setOpenTrades(parsedOpenTrades)

                        parsedOpenTrades.map( item => {
                            AddTrade(item.Value, "open");
                        })

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
                <header class="header">Backtesting Engine</header>
                <div class="chartDiv"><CanvasJSChart options = {series}
                    onRef={(ref) => {
                        chartRef.current = ref;
                    }}
                /></div>
                <article class="main">
                <table>
                    <thead>
                        <tr><td>Date</td><td>Direction</td><td>Profit</td></tr>
                    </thead>
                    <tbody>
                        {openTrades?.map( item => (
                                    <tr>                                   
                                    <td>
                                    { new Date(item.Value.openDate).toLocaleString() }
                                    </td>
                                    <td>
                                    { item.Value.direction == 0 ? "BUY" : "SELL" }
                                    </td>
                                    <td>
                                    { item.Value.profit }
                                    </td>
                                    </tr>
                                    
                        ))}
                        {closedTrades?.map( item => (
                                    <tr>                                 
                                    <td>
                                    { new Date(item.openDate).toLocaleString() }
                                    </td>
                                    <td>
                                    { item.direction == 0 ? "BUY" : "SELL" }
                                    </td>
                                    <td>
                                    { item.profit }
                                    </td>
                                    </tr>
                                    
                        ))}
                        </tbody>
                    </table>
                </article>
                <aside class="aside aside-1">
                    <div>Open Trades</div>
                    <div class="accountLabel">{openTrades?.length}</div>
                    <div className="openTradesDiv">Closed Trades</div>
                    <div class="accountLabel">{closedTrades?.length}</div>
                </aside>
                <aside class="aside aside-2">
                    <div>Account</div>
                    <div class="accountLabel">{account}</div>
                </aside>
             
            </div>
        </div>
    );
};

export default Chat;
