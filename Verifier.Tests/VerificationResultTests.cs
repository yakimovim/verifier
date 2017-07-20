using System.Linq;
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
        [InlineData(false, "error1", false, "error2", false)]
        [InlineData(true, "error1", false, "error2", true)]
        [InlineData(false, "error1", true, "error2", true)]
        [InlineData(true, "error1", true, "error2", true)]
        [InlineData(false, null, false, "error2", false)]
        [InlineData(false, "error1", false, null, false)]
        [InlineData(false, null, false, null, false)]
        [InlineData(true, "error1", false, null, true)]
        [InlineData(true, null, false, "error2", false)]
        [InlineData(true, null, false, null, false)]
        [InlineData(false, "error1", true, null, false)]
        [InlineData(false, null, true, "error2", true)]
        [InlineData(false, null, true, null, false)]
        [InlineData(true, "error1", true, null, true)]
        [InlineData(true, null, true, "error2", true)]
        [InlineData(true, null, true, null, false)]
        public void AddOperator(bool critical1, string error1, bool critical2, string error2, bool expectedCritical)
        {
            var vr1 = new VerificationResult(critical1, error1);
            var vr2 = new VerificationResult(critical2, error2);

            var vr = vr1 + vr2;

            Assert.Equal(expectedCritical, vr.IsCritical);
            Assert.Equal(new[] { error1, error2 }.Where(e => e != null).ToArray(), vr.ErrorMessages);
        }
    }
}