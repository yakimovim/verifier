using System;
using System.Collections.Generic;

namespace EdlinSoftware.Verifier
{
    /// <summary>
    /// Represents verifier, which does not use any particular object.
    /// </summary>
    public abstract class ActionVerifier<TVerifier>
        where TVerifier : ActionVerifier<TVerifier>
    {
        private readonly LinkedList<Func<VerificationResult>> _staticVerifiers = new LinkedList<Func<VerificationResult>>();

        /// <summary>
        /// Gets if this verification result should be critical or not.
        /// </summary>
        /// <remarks>Critical verification result prevents later verifications, if it contains errors.</remarks>
        public bool IsCritical { get; set; }

        /// <summary>
        /// Adds verifier functions to this verifier.
        /// </summary>
        /// <param name="verifiers">Verifier functions.</param>
        public TVerifier AddVerifiers(params Func<VerificationResult>[] verifiers)
        {
            _staticVerifiers.AddNotNullRange(verifiers);
            return (TVerifier)this;
        }

        /// <summary>
        /// Adds verifier actions to this verifier. Each action will produce critical verification result.
        /// If there is an exception in an action, the message of this exception will be added to the <see cref="VerificationResult.ErrorMessages"/> collection.
        /// </summary>
        /// <param name="verifiers">Verifier actions</param>
        public TVerifier AddCriticalVerifiers(params Action[] verifiers)
        {
            _staticVerifiers.AddCriticalVerifiers(verifiers);
            return (TVerifier)this;
        }

        /// <summary>
        /// Adds verifier actions to this verifier. Each action will produce normal verification result.
        /// If there is an exception in an action, the message of this exception will be added to the <see cref="VerificationResult.ErrorMessages"/> collection.
        /// </summary>
        /// <param name="verifiers">Verifier actions</param>
        public TVerifier AddNormalVerifiers(params Action[] verifiers)
        {
            _staticVerifiers.AddNormalVerifiers(verifiers);
            return (TVerifier)this;
        }

        /// <summary>
        /// Runs all registered verifiers and return cumulative result.
        /// </summary>
        public VerificationResult Verify()
        {
            var result = VerificationResult.Normal();

            foreach (var verifier in _staticVerifiers)
            {
                try
                {
                    var verificationResult = verifier();
                    result += verificationResult;
                    if (!verificationResult.AllowContinue())
                    { break; }
                }
                catch (Exception e)
                {
                    result += VerificationResult.Critical(e.Message);
                    break;
                }
            }

            return new VerificationResult(IsCritical, result.ErrorMessages);
        }

        /// <summary>
        /// Throws exception if there are verification errors in verification functions.
        /// </summary>
        /// <summary>
        /// If there are verification errors, this method calls 
        /// <see cref="Verifier.AssertionFailed"/> delegate.
        /// </summary>
        public void Check()
        {
            var result = Verify();
            if (result.HasErrors())
                Verifier.AssertionFailed(string.Join(Environment.NewLine, result.ErrorMessages));
        }
    }
}