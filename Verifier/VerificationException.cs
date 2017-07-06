using System;

namespace EdlinSoftware.Verifier
{
    /// <summary>
    /// Represents exception on verification failure.
    /// </summary>
    public class VerificationException : Exception
    {
        /// <summary>
        /// Initializes instance of <see cref="VerificationException"/>.
        /// </summary>
        /// <param name="errorMessage">Verification error message.</param>
        public VerificationException(string errorMessage) : base(errorMessage) {}
    }
}