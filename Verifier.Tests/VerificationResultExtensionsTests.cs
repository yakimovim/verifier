using Xunit;

namespace EdlinSoftware.Verifier.Tests
{
    public class VerificationResultExtensionsTests
    {
        [Theory]
        [InlineData("error", true)]
        [InlineData(null, false)]
        public void HasErrors(string errorMessage, bool hasErrors)
        {
            Assert.Equal(hasErrors, VerificationResult.Normal(errorMessage).HasErrors());
        }
    }
}