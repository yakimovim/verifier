using System;
using EdlinSoftware.Verifier.Tests.Support;
using Xunit;

namespace EdlinSoftware.Verifier.Tests
{
    public class StaticVerifiersTests
    {
        private readonly StringVerifier _verifier;

        public StaticVerifiersTests()
        {
            _verifier = new StringVerifier();
        }

        [Fact]
        public void Verify_Normal_WithNoVerifiers()
        {
            var vr = _verifier.Verify("hello");

            Assert.False(vr.IsCritical);
            Assert.Equal(0, vr.ErrorMessages.Length);
        }

        [Fact]
        public void Verify_Critical_WithNoVerifiers()
        {
            _verifier.IsCritical = true;

            var vr = _verifier.Verify("hello");

            Assert.True(vr.IsCritical);
            Assert.Equal(0, vr.ErrorMessages.Length);
        }

        [Fact]
        public void Verify_ExecutesAllStaticVerifiers()
        {
            int verifiersCounter = 0;

            VerificationResult Verifier(string sut)
            {
                verifiersCounter++;
                return VerificationResult.Normal();
            }

            _verifier
                .AddVerifiers(Verifier, Verifier, Verifier)
                .Verify("hello");

            Assert.Equal(3, verifiersCounter);
        }

        [Fact]
        public void Verify_GathersAllErrorMessages()
        {
            int verifiersCounter = 0;

            VerificationResult Verifier(string sut)
            {
                verifiersCounter++;
                return VerificationResult.Normal($"error{verifiersCounter}");
            }

            var vr = _verifier
                .AddVerifiers(Verifier, Verifier, Verifier)
                .Verify("hello");

            Assert.Equal(new[] { "error1", "error2", "error3" }, vr.ErrorMessages);
        }

        [Fact]
        public void Verify_DoesntVerifyAfterCriticalError()
        {
            var vr = _verifier
                .AddVerifiers(
                    sut => VerificationResult.Normal("error1"),
                    sut => VerificationResult.Critical("error2"),
                    sut => VerificationResult.Normal("error3")
                )
                .Verify("hello");

            Assert.Equal(new[] { "error1", "error2" }, vr.ErrorMessages);
        }

        [Fact]
        public void Verify_StopsOnUnhandledException()
        {
            var vr = _verifier
                .AddVerifiers(
                    sut => VerificationResult.Normal("error1"),
                    sut => throw new InvalidOperationException("error2"),
                    sut => VerificationResult.Normal("error3")
                )
                .Verify("hello");

            Assert.Equal(new[] { "error1", "error2" }, vr.ErrorMessages);
        }

        [Fact]
        public void AddNormalVerifiers_Actions_ExecutesAllStaticVerifiers()
        {
            int verifiersCounter = 0;

            void Verifier(string sut)
            {
                verifiersCounter++;
            }

            _verifier
                .AddNormalVerifiers(Verifier, Verifier, Verifier)
                .Verify("hello");

            Assert.Equal(3, verifiersCounter);
        }

        [Fact]
        public void AddNormalVerifiers_Actions_GathersAllErrorMessages()
        {
            var vr = _verifier
                .AddNormalVerifiers(
                    sut => throw new InvalidOperationException("error1"),
                    sut => throw new InvalidOperationException("error2"),
                    sut => throw new InvalidOperationException("error3")
                )
                .Verify("hello");

            Assert.Equal(new [] { "error1", "error2", "error3" }, vr.ErrorMessages);
        }

        [Fact]
        public void AddCriticalVerifiers_Actions_ExecutesAllStaticVerifiers()
        {
            int verifiersCounter = 0;

            void Verifier(string sut)
            {
                verifiersCounter++;
            }

            _verifier
                .AddCriticalVerifiers(Verifier, Verifier, Verifier)
                .Verify("hello");

            Assert.Equal(3, verifiersCounter);
        }

        [Fact]
        public void AddCriticalVerifiers_Actions_DoesntVerifyAfterCriticalError()
        {
            var vr = _verifier
                .AddCriticalVerifiers(Assert.NotNull)
                .AddNormalVerifiers(
                    sut => Assert.Equal(6, sut.Length),
                    sut => Assert.True(sut.StartsWith("k")),
                    sut => Assert.True(sut.EndsWith("p"))
                )
                .Verify(null);

            Assert.Equal(1, vr.ErrorMessages.Length);
        }

        [Fact]
        public void AddVerifiers_Verifiers_ExecutesAllStaticVerifiers()
        {
            var v1 = new StringVerifier();
            v1.AddVerifiers(sut => throw new Exception("error1"));
            var v2 = new StringVerifier();
            v2.AddVerifiers(sut => throw new Exception("error2"));
            var v3 = new StringVerifier();
            v3.AddVerifiers(sut => throw new Exception("error3"));

            var vr = _verifier
                .AddVerifiers(v1, v2, v3)
                .Verify("hello");

            Assert.Equal(new[] { "error1", "error2", "error3" }, vr.ErrorMessages);
        }

        [Fact]
        public void AddVerifiers_Verifiers_DoesntVerifyAfterCriticalError()
        {
            var v1 = new StringVerifier();
            v1.AddVerifiers(sut => throw new Exception("error1"));
            var v2 = new StringVerifier { IsCritical = true };
            v2.AddVerifiers(sut => throw new Exception("error2"));
            var v3 = new StringVerifier();
            v3.AddVerifiers(sut => throw new Exception("error3"));

            var vr = _verifier
                .AddVerifiers(v1, v2, v3)
                .Verify("hello");

            Assert.Equal(new[] { "error1", "error2" }, vr.ErrorMessages);
        }

        [Fact]
        public void Verify_ReusesStaticVerifiers()
        {
            var v1 = new StringVerifier();
            v1.AddVerifiers(sut => throw new Exception("error1"));
            var v2 = new StringVerifier();
            v2.AddVerifiers(sut => throw new Exception("error2"));
            var v3 = new StringVerifier();
            v3.AddVerifiers(sut => throw new Exception("error3"));

            var vr = _verifier
                .AddVerifiers(v1, v2, v3)
                .Verify("hello");

            Assert.Equal(new[] { "error1", "error2", "error3" }, vr.ErrorMessages);

            vr = _verifier.Verify("hello");

            Assert.Equal(new[] { "error1", "error2", "error3" }, vr.ErrorMessages);
        }
    }
}