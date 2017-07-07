using Xunit;

namespace EdlinSoftware.Verifier.Tests
{
    public class DynamicVerifiersTests
    {
        private class TestVerifier : Verifier<TestVerifier, string>
        {
            protected override void AddDynamicVerifiers(string instanceUnderTest)
            {
                AddCriticalVerifiers(Assert.NotNull)
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

        [Fact]
        public void Verify_DontStoryDynamicVerifiers()
        {
            var vr = _verifier.Verify("");

            Assert.Equal(3, vr.ErrorMessages.Length);

            vr = _verifier.Verify("");

            Assert.Equal(3, vr.ErrorMessages.Length);
        }
    }
}