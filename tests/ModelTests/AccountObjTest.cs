using System.Threading.Tasks;
using backtesting_engine;
using Moq;
using Xunit;

namespace Tests;

[Collection("Sequential")]
public class AccountObjTests
{

    [Fact]
    public void PopulateAccountObjTest(){

        // var programMock = new Mock<Program>(){
        //     CallBase = false
        // };

        // // programMock.Setup(x=>x.StartEngine()).Returns(It.IsAny<Task>());

        // AccountObj accountObj = new AccountObj(){
        //     openingEquity=500,
        //     maximumDrawndownPercentage=50
        // };

        // Assert.False(accountObj.hasAccountExceededDrawdownThreshold());

    }
}