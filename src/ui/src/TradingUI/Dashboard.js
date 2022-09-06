import React, { useState, useEffect, useRef } from 'react';
import { HubConnectionBuilder, HttpTransportType} from '@microsoft/signalr';
import CanvasJSReact from '../libs/canvasjs.react';
import SaveClosedTradeForTable from '../Functions/SaveClosedTrades';
import UpdateChart from '../Functions/DisplayOHLCData';
import AddHedgingTrade from '../Functions/TradesOnLinearXAxis';

var CanvasJS = CanvasJSReact.CanvasJS;
var CanvasJSChart = CanvasJSReact.CanvasJSChart;

// MessagePack is a fast and compact binary serialization format. It's useful when performance and bandwidth are a concern because it creates smaller messages than JSON.
// https://docs.microsoft.com/en-us/aspnet/core/signalr/messagepackhubprotocol?view=aspnetcore-6.0
const signalRMsgPack = require("@microsoft/signalr-protocol-msgpack");

// To fix localhost SSL issues
process.env["NODE_TLS_REJECT_UNAUTHORIZED"] = 0; 

const Chat = () => {

    const [ series, setSeries ] = useState({
            animationEnabled: true,
            theme: "light1", // "light1", "light2", "dark1", "dark2"
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

    const [ series2, setSeries2 ] = useState({
        animationEnabled: true,
        theme: "light1", // "light1", "light2", "dark1", "dark2"
        title: {
            text: ""
        },
        subtitles: [{
            text: ""
        }],
        axisX: {
            minimum: 0,
            maximum: 50,
        },
        axisY: {
            prefix: "",
            title: "",
            includeZero: false,
        },
        data: [{				
            type: "candlestick",
            xValueType: "label",
            risingColor: "#00D100",
            fallingColor: "#FF0000",  
            connectNullData:true,
            dataPoints: []
        }]
    }
);

    const [ account, setAccount ] = useState(0);
    const [ openTrades, setOpenTrades] = useState([]);
    const [ closedTrades, setClosedTrades] = useState([]);
    const inputOpenTrades = useRef([]);
    const inputTrades = useRef([]);
    const hedgeCount = useRef(0);

    var chartRef = useRef();
    var chartRef2 = useRef();
   

    // function SaveClosedTradeForTable(OHLCObj){
    //     setClosedTrades((previousState) => {
    //         previousState.push(OHLCObj);
    //         return previousState;
    //     });
    // }

    

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

            // if(tradeType === "closed" ){
            //     // Does the key exist && is it open
            //     if(newState.data.some(item => item._tradingKey === OHLCObj.key) && 
            //         newState.data.some(item => item._tradingType === "open")){
            //             // console.log("found open trade to remove");
            //             // Remove it
            //             newState.data = newState.data.filter(function(item) {
            //                 return item._tradingKey !== OHLCObj.key
            //             })
            //     }
            // }
            
            // Lets check if the open trade exists, only need to add it once
            // if(tradeType === "open"){
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
                    newState.data[index].color = color;
                    return newState;
                }
            }
            // }

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

                    let parsedContent = JSON.parse(message.Content);
                    
                    if ('priceUpdate' in parsedContent){
                        var OHLCObj = JSON.parse(parsedContent.priceUpdate);
                        UpdateChart(OHLCObj,chartRef, setSeries);
                    } 
                    if ('accountUpdate' in parsedContent){
                        setAccount(parseFloat(parsedContent.accountUpdate[0]).toFixed(2))
                    }
                    if ('openTrades' in parsedContent){
                        for (const i in parsedContent.openTrades) {
                            let item = JSON.parse(parsedContent.openTrades[i]);
                            AddTrade(item, "open");
                            AddHedgingTrade(item, "open", setSeries2, hedgeCount, chartRef2);
                            inputOpenTrades.current.push(item);
                        }   
                        
                        setOpenTrades((previousState) => {
                            if (previousState == null){
                                previousState = [];
                            }
                            previousState.push(...inputTrades.current);
                            inputOpenTrades.current=[];
                        })
                    }


                    if ('trade' in parsedContent){
                        for (const i in parsedContent.trade) {
                            let item = JSON.parse(parsedContent.trade[i]);
                            AddTrade(item);
                            AddHedgingTrade(item, "update", setSeries2, hedgeCount, chartRef2);

                            SaveClosedTradeForTable(item, setClosedTrades);
                        }                      
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

    return (
        
        <div>
            <div class="wrapper">
                <header class="header">Backtesting Engine</header>
                <div class="chartDiv"><CanvasJSChart options = {series}
                    onRef={(ref) => {
                        chartRef.current = ref;
                    }}
                /></div>
                <div class="chartDiv2"><CanvasJSChart options = {series2}
                    onRef={(ref) => {
                        chartRef2.current = ref;
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
