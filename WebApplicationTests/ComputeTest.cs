using WebApplication;

namespace WebApplicationTests
{
    public class ComputeTest
    {
        [Fact]
        public void ComputeMinAreaTest()
        {
            const double expectedMeanArea = 30.4167;

            var areas = new List<double> { 2.5, 10, 15, 25, 30, 100 };
            var meanArea = Utils.ComputeMeanArea(areas);

            Assert.Equal(expectedMeanArea, meanArea, 1e-4);
        }
    }
}