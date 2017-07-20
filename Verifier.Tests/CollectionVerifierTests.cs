using EdlinSoftware.Verifier.Tests.Support;
using Xunit;

namespace EdlinSoftware.Verifier.Tests
{
    public class CollectionVerifierTests
    {
        private class StringCollectionVerifier : CollectionVerifier<StringCollectionVerifier, string>
        {}

        private readonly StringCollectionVerifier _verifier;

        public CollectionVerifierTests()
        {
            _verifier = new StringCollectionVerifier();
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

        [Fact]
        public void UseOfItemVerificationFunctions()
        {
            _verifier.AddItemVerifiers(elem => VerificationResult.Normal(elem.StartsWith("a") ? null : "String should start with 'a'"));
            _verifier.AddItemVerifiers(elem => VerificationResult.Normal(elem.StartsWith("b") ? null : "String should start with 'b'"));

            var vr = _verifier.Verify(new[] { "bbb", "aaa" });

            Assert.Equal(2, vr.ErrorMessages.Length);
        }

        [Fact]
        public void UseOfCriticalItemVerificationFunctions()
        {
            _verifier.AddCriticalItemVerifiers(elem => Assert.StartsWith("a", elem));
            _verifier.AddCriticalItemVerifiers(elem => Assert.StartsWith("b", elem));

            var vr = _verifier.Verify(new[] { "bbb", "aaa" });

            Assert.Equal(1, vr.ErrorMessages.Length);
        }

        [Fact]
        public void UseOfItemVerifiers()
        {
            var stringVerifier1 = new StringVerifier()
                .AddNormalVerifiers(elem => Assert.StartsWith("a", elem));
            var stringVerifier2 = new StringVerifier()
                .AddNormalVerifiers(elem => Assert.StartsWith("b", elem));

            _verifier.AddItemVerifiers(stringVerifier1);
            _verifier.AddItemVerifiers(stringVerifier2);

            var vr = _verifier.Verify(new[] { "bbb", "aaa" });

            Assert.Equal(2, vr.ErrorMessages.Length);
        }
    }
}