function UpdateChart(OHLCObj, chartRef, setSeries){
        
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

export default UpdateChart;
