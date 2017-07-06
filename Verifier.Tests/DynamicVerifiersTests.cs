using Xunit;

namespace EdlinSoftware.Verifier.Tests
{
    public class DynamicVerifiersTests
    {
        private class TestVerifier : Verifier<string>
        {
            protected override void AddDynamicVerifiers(string instanceUnderTest)
            {
                this
                    .AddCriticalVerifiers(Assert.NotNull)
                    .AddNormalVerifiers(
                        sut => Assert.Equal(5, sut.Length),
                        sut => Assert.StartsWith("h", sut),
                        sut => Assert.EndsWith("o", sut)
                        );
            }
        }

        private readonly TestVerifier _verifier;

        public DynamicVerifiersTests()
        {
            _verifier = new TestVerifier();
        }

        [Fact]
        public void Verify_ExecutesAllDynamicVerifiers()
        {
            var vr = _verifier.Verify("");

            Assert.Equal(3, vr.ErrorMessages.Length);
        }
    }
}