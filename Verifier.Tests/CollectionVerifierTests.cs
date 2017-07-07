using Xunit;

namespace EdlinSoftware.Verifier.Tests
{
    public class CollectionVerifierTests
    {
        private class TestVerifier : CollectionVerifier<TestVerifier, string>
        {}

        private readonly TestVerifier _verifier;

        public CollectionVerifierTests()
        {
            _verifier = new TestVerifier();
        }

        [Fact]
        public void NullCollection()
        {
            _verifier.AddNormalItemVerifiers(elem => Assert.StartsWith("a", elem));
            _verifier.AddNormalItemVerifiers(elem => Assert.StartsWith("b", elem));

            var vr = _verifier.Verify(null);

            Assert.Equal(1, vr.ErrorMessages.Length);
        }

        [Fact]
        public void CollectionContainsLessElementsThanExpected()
        {
            _verifier.AddNormalItemVerifiers(elem => Assert.StartsWith("a", elem));
            _verifier.AddNormalItemVerifiers(elem => Assert.StartsWith("b", elem));

            var vr = _verifier.Verify(new [] { "aaa" });

            Assert.Equal(1, vr.ErrorMessages.Length);
            Assert.Equal("2 elements were expected, but there are only 1 elements.", vr.ErrorMessages[0]);
        }

        [Fact]
        public void CollectionContainsMoreElementsThanExpected()
        {
            _verifier.AddNormalItemVerifiers(elem => Assert.StartsWith("a", elem));
            _verifier.AddNormalItemVerifiers(elem => Assert.StartsWith("b", elem));

            var vr = _verifier.Verify(new[] { "aaa", "bbb", "ccc" });

            Assert.Equal(1, vr.ErrorMessages.Length);
            Assert.Equal("2 elements were expected, but there are more elements.", vr.ErrorMessages[0]);
        }

        [Fact]
        public void ItemVerifiersAreExecuted()
        {
            _verifier.AddNormalItemVerifiers(elem => Assert.StartsWith("a", elem));
            _verifier.AddNormalItemVerifiers(elem => Assert.StartsWith("b", elem));

            var vr = _verifier.Verify(new[] { "bbb", "aaa" });

            Assert.Equal(2, vr.ErrorMessages.Length);
        }

        [Fact]
        public void ItemVerifiersAreReusable()
        {
            _verifier.AddNormalItemVerifiers(elem => Assert.StartsWith("a", elem));
            _verifier.AddNormalItemVerifiers(elem => Assert.StartsWith("b", elem));

            var vr = _verifier.Verify(new[] { "bbb", "aaa" });

            Assert.Equal(2, vr.ErrorMessages.Length);

            vr = _verifier.Verify(new[] { "aaa", "bbb" });

            Assert.Equal(0, vr.ErrorMessages.Length);
        }
    }
}