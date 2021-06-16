using Amazon.Lambda.TestUtilities;
using Moq;
using TTT.Seasons.Repository;

namespace PatchDeciderMatchFunction.Tests
{
    public class FunctionTest
    {
        private readonly Mock<ISeasonRepository> _seasonRepositoryMock;
        private readonly Function _sutFunction;
        private readonly TestLambdaContext _testContext;

        public FunctionTest()
        {
            _seasonRepositoryMock = new Mock<ISeasonRepository>();

            _sutFunction = new Function(_seasonRepositoryMock.Object);
            _testContext = new TestLambdaContext();
        }
    }
}
