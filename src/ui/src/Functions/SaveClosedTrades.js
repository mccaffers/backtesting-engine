
 function SaveClosedTradeForTable(OHLCObj, setClosedTrades){
    setClosedTrades((previousState) => {
        previousState.push(OHLCObj);
        return previousState;
    });
}

export default SaveClosedTradeForTable;
