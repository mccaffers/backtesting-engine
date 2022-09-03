function AddHedgingTrade(OHLCObj, tradeType, setSeries2, hedgeCount, chartRef2){

    hedgeCount.current++;
    if(hedgeCount.current>100){
        hedgeCount.current=0;

        setSeries2((previousState) => {
            previousState.data =  [];
            return previousState;
        });
    }

    setSeries2((previousState) => {

        let color = '#007500'; // green
        if(OHLCObj.profit < 0){
            color ='#750000';
        }
        let direction = "BUY";
        if(OHLCObj.direction === 1){
            direction="SELL";
        }

        let newState = previousState;


        // Loop all data obj's, if the _date is older than 3 seconds, delete it
        // newState.data = newState.data.filter(function(item) {
        //     if(item._date == null){
        //         return true;
        //     }
        //     if(item._tradingType === "closed")
        //     {
        //         return false;
        //     }
        //     let localDate=new Date(item._date.getTime());
        //     localDate.setSeconds(localDate.getSeconds()+60);
        //     return localDate.getTime() > new Date().getTime();
        // })

        if(OHLCObj.closeLevel === 0){
            OHLCObj.closeLevel = OHLCObj.level;
        }


        // Lets check if the open trade exists, only need to add it once
        if(newState.data.some(item => item._tradingKey === OHLCObj.key)){

            const index = newState.data.findIndex((element, index) => {
                if (element._tradingKey === OHLCObj.key) {
                    return true
                }
            });

            if(index!==-1){

                let color = '#007500'; // green
                if(OHLCObj.profit < 0){
                    color ='#750000';
                }
                // if(newState.data[index].dataPoints.length == 1){
                //     newState.data[index].dataPoints[0].x = hedgeCount.current;
                //     newState.data[index].dataPoints[0].y = OHLCObj.level;

                //     newState.data[index].dataPoints.push([]);
                // }
                newState.data[index].color = color;
                newState.data[index].dataPoints[1].x = hedgeCount.current;
                newState.data[index].dataPoints[1].y = OHLCObj.closeLevel;
                return newState;
            }
        }
    


        // Otherwise, add the open trades that doesn't exist, or a new closed trade
        newState.data.push({
            _tradingType: tradeType,
            _date : new Date(),
            _tradingKey: OHLCObj.key,
            type: "line",
            color:color,
            dataPoints: [
                { x: hedgeCount.current, y: OHLCObj.level, indexLabel: direction },
                { x: hedgeCount.current, y: OHLCObj.closeLevel }
            ]
        });
    
        return newState;

    });
    chartRef2.current.render();
}

export default AddHedgingTrade;
