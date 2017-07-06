using Xunit;

namespace EdlinSoftware.Verifier.Tests
{
    public class VerificationResultTests
    {
        [Fact]
        public void DefaultConstructor()
        {
            VerificationResult vr = new VerificationResult();
            Assert.False(vr.IsCritical);
            Assert.Equal(0, vr.ErrorMessages.Length);
        }

        [Fact]
        public void NormalConstructor()
        {
            var vr = VerificationResult.Normal("error");
            Assert.False(vr.IsCritical);
            Assert.Equal(new [] { "error" }, vr.ErrorMessages);
        }

        [Fact]
        public void CriticalConstructor()
        {
            var vr = VerificationResult.Critical("error");
            Assert.True(vr.IsCritical);
            Assert.Equal(new[] { "error" }, vr.ErrorMessages);
        }

        [Fact]
        public void ConvertFromString()
        {
            VerificationResult vr = "error";
            Assert.False(vr.IsCritical);
            Assert.Equal(new[] { "error" }, vr.ErrorMessages);
        }

        [Fact]
        public void ConvertFromStringArray()
        {
            VerificationResult vr = new [] { "error1", "error2" };
            Assert.False(vr.IsCritical);
            Assert.Equal(new[] { "error1", "error2" }, vr.ErrorMessages);
        }

        [Theory]
        [InlineData(false, false, false)]
        [InlineData(true, false, true)]
        [InlineData(false, true, true)]
        [InlineData(true, true, true)]
        public void AddOperator(bool critical1, bool critical2, bool expectedCritical)
        {
            var vr1 = new VerificationResult(critical1,"error1");
            var vr2 = new VerificationResult(critical2, "error2");

            var vr = vr1 + vr2;

            Assert.Equal(expectedCritical, vr.IsCritical);
            Assert.Equal(new[] { "error1", "error2" }, vr.ErrorMessages);
        }
    }
}