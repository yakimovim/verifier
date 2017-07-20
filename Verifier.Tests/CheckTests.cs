using System;
using EdlinSoftware.Verifier.Tests.Support;
using Xunit;

namespace EdlinSoftware.Verifier.Tests
{
    public class CheckTests : IDisposable
    {
        private readonly StringVerifier _verifier;
        private static readonly Action<string> DefaultAssertionFailed = Verifier.AssertionFailed;

        public CheckTests()
        {
            _verifier = new StringVerifier()
                .AddVerifiers(sut => VerificationResult.Critical(sut == "success" ? null : "error"));
        }

        [Fact]
        public void Check_Failure_ByDefault()
        {
            Assert.Throws<VerificationException>(() => _verifier.Check("failure"));
        }

        [Fact]
        public void Check_Success_ByDefault()
        {
            _verifier.Check("success");
        }

        [Fact]
        public void Check_CustomAssert()
        {
            Verifier.AssertionFailed = errorMessage => throw new InvalidOperationException(errorMessage);

            Assert.Equal(
                "error",
                Assert.Throws<InvalidOperationException>(() => _verifier.Check("failure")).Message
                );
        }

        public void Dispose()
        {
            Verifier.AssertionFailed = DefaultAssertionFailed;
        }
    }
}