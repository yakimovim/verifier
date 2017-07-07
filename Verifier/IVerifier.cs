namespace EdlinSoftware.Verifier
{
    /// <summary>
    /// Represents verifier of complex object.
    /// </summary>
    /// <typeparam name="TUnderTest">Type of object under test.</typeparam>
    public interface IVerifier<in TUnderTest>
    {
        /// <summary>
        /// Runs all registered verifiers and return cumulative result.
        /// </summary>
        /// <param name="instanceUnderTest">Instance under test.</param>
        VerificationResult Verify(TUnderTest instanceUnderTest);
    }
}