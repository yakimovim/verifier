using System;
using Xunit;

namespace EdlinSoftware.Verifier.Tests
{
    public class ActionVerifierTests
    {
        private class StaticVerifier : ActionVerifier<StaticVerifier>
        {}

        private readonly StaticVerifier _verifier;

        public ActionVerifierTests()
        {
            _verifier = new StaticVerifier();
        }

        [Fact]
        public void Verify_Normal_WithNoVerifiers()
        {
            var vr =  _verifier.Verify();

            Assert.False(vr.IsCritical);
            Assert.Equal(0, vr.ErrorMessages.Length);
        }

        [Fact]
        public void Verify_Critical_WithNoVerifiers()
        {
            _verifier.IsCritical = true;

            var vr = _verifier.Verify();

            Assert.True(vr.IsCritical);
            Assert.Equal(0, vr.ErrorMessages.Length);
        }

        [Fact]
        public void Verify_ExecutesAllStaticVerifiers()
        {
            int verifiersCounter = 0;

            VerificationResult Verifier()
            {
                verifiersCounter++;
                return VerificationResult.Normal();
            }

            _verifier
                .AddVerifiers(Verifier, Verifier, Verifier)
                .Verify();

            Assert.Equal(3, verifiersCounter);
        }

        [Fact]
        public void Verify_GathersAllErrorMessages()
        {
            int verifiersCounter = 0;

            VerificationResult Verifier()
            {
                verifiersCounter++;
                return VerificationResult.Normal($"error{verifiersCounter}");
            }

            var vr = _verifier
                .AddVerifiers(Verifier, Verifier, Verifier)
                .Verify();

            Assert.Equal(new[] { "error1", "error2", "error3" }, vr.ErrorMessages);
        }

        [Fact]
        public void Verify_DoesntVerifyAfterCriticalError()
        {
            var vr = _verifier
                .AddVerifiers(
                    () => VerificationResult.Normal("error1"),
                    () => VerificationResult.Critical("error2"),
                    () => VerificationResult.Normal("error3")
                )
                .Verify();

            Assert.Equal(new[] { "error1", "error2" }, vr.ErrorMessages);
        }

        [Fact]
        public void Verify_StopsOnUnhandledException()
        {
            var vr = _verifier
                .AddVerifiers(
                    () => VerificationResult.Normal("error1"),
                    () => throw new InvalidOperationException("error2"),
                    () => VerificationResult.Normal("error3")
                )
                .Verify();

            Assert.Equal(new[] { "error1", "error2" }, vr.ErrorMessages);
        }

        [Fact]
        public void AddNormalVerifiers_Actions_ExecutesAllStaticVerifiers()
        {
            int verifiersCounter = 0;

            void Verifier()
            {
                verifiersCounter++;
            }

            _verifier
                .AddNormalVerifiers(Verifier, Verifier, Verifier)
                .Verify();

            Assert.Equal(3, verifiersCounter);
        }

        [Fact]
        public void AddNormalVerifiers_Actions_GathersAllErrorMessages()
        {
            var vr = _verifier
                .AddNormalVerifiers(
                    () => throw new InvalidOperationException("error1"),
                    () => throw new InvalidOperationException("error2"),
                    () => throw new InvalidOperationException("error3")
                )
                .Verify();

            Assert.Equal(new[] { "error1", "error2", "error3" }, vr.ErrorMessages);
        }

        [Fact]
        public void AddCriticalVerifiers_Actions_ExecutesAllStaticVerifiers()
        {
            int verifiersCounter = 0;

            void Verifier()
            {
                verifiersCounter++;
            }

            _verifier
                .AddCriticalVerifiers(Verifier, Verifier, Verifier)
                .Verify();

            Assert.Equal(3, verifiersCounter);
        }

        [Fact]
        public void AddCriticalVerifiers_Actions_DoesntVerifyAfterCriticalError()
        {
            var vr = _verifier
                .AddCriticalVerifiers(() => throw new InvalidOperationException("error1"))
                .AddNormalVerifiers(
                    () => throw new InvalidOperationException("error2"),
                    () => throw new InvalidOperationException("error3"),
                    () => throw new InvalidOperationException("error4")
                )
                .Verify();

            Assert.Equal(new [] { "error1" }, vr.ErrorMessages);
        }

        [Fact]
        public void Verify_ReusesStaticVerifiers()
        {
            var vr = _verifier
                .AddNormalVerifiers(
                    () => throw new InvalidOperationException("error1"),
                    () => throw new InvalidOperationException("error2"),
                    () => throw new InvalidOperationException("error3")
                )
                .Verify();

            Assert.Equal(new[] { "error1", "error2", "error3" }, vr.ErrorMessages);

            vr = _verifier.Verify();

            Assert.Equal(new[] { "error1", "error2", "error3" }, vr.ErrorMessages);
        }
    }
}