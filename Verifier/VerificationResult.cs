using System.Diagnostics;
using System.Linq;

namespace EdlinSoftware.Verifier
{
    /// <summary>
    /// Represents verification result.
    /// </summary>
    public struct VerificationResult
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly string[] NoErrorMessages = new string[0];

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly string[] _errorMessages;

        /// <summary>
        /// Gets if this verification result is critical or not.
        /// </summary>
        /// <remarks>Critical verification result prevents later verifications, if it contains errors.</remarks>
        public bool IsCritical { get; }

        /// <summary>
        /// Gets error messages for current verification.
        /// </summary>
        public string[] ErrorMessages => _errorMessages ?? NoErrorMessages;

        /// <summary>
        /// Initializes instance of <see cref="VerificationResult"/>.
        /// </summary>
        /// <param name="isCritical">Is this verification result critical or not.</param>
        /// <param name="errorMessages">Error messages.</param>
        [DebuggerStepThrough]
        public VerificationResult(bool isCritical, params string[] errorMessages)
        {
            _errorMessages = errorMessages.Where(m => !string.IsNullOrWhiteSpace(m)).ToArray();
            IsCritical = isCritical;
        }

        /// <summary>
        /// Creates normal verification result.
        /// </summary>
        /// <param name="errorMessages">Error messages.</param>
        [DebuggerStepThrough]
        public static VerificationResult Normal(params string[] errorMessages)
        {
            return new VerificationResult(false, errorMessages);
        }

        /// <summary>
        /// Creates critical verification result.
        /// </summary>
        /// <param name="errorMessages">Error messages.</param>
        [DebuggerStepThrough]
        public static VerificationResult Critical(params string[] errorMessages)
        {
            return new VerificationResult(true, errorMessages);
        }
        
        /// <inheritdoc />
        public static implicit operator VerificationResult(string value)
        {
            return Normal(value);
        }

        /// <inheritdoc />
        public static implicit operator VerificationResult(string[] value)
        {
            return Normal(value);
        }

        /// <inheritdoc />
        public static VerificationResult operator +(VerificationResult vr1, VerificationResult vr2)
        {
            var isCritical = !vr1.AllowContinue() || !vr2.AllowContinue();

            return new VerificationResult(isCritical, vr1.ErrorMessages.Concat(vr2.ErrorMessages).ToArray());
        }
    }

    /// <summary>
    /// Contains extension methods for <see cref="VerificationResult"/> instances.
    /// </summary>
    internal static class VerificationResultExtensions
    {
        /// <summary>
        /// Check if verification result has error descriptions.
        /// </summary>
        /// <param name="vr">Verification result.</param>
        public static bool HasErrors(this VerificationResult vr)
        {
            return vr.ErrorMessages.Length > 0;
        }

        /// <summary>
        /// Does verification result allows continuation after error.
        /// </summary>
        /// <param name="vr">Verification result.</param>
        public static bool AllowContinue(this VerificationResult vr)
        {
            return !vr.HasErrors() || !vr.IsCritical;
        }
    }
}