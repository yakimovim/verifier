using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace EdlinSoftware.Verifier
{
    /// <summary>
    /// Represents verifier of complex object.
    /// </summary>
    /// <typeparam name="TVerifier">Type of verifier.</typeparam>
    /// <typeparam name="TUnderTest">Type of object under test.</typeparam>
    public abstract class Verifier<TVerifier, TUnderTest> : IVerifier<TUnderTest>
        where TVerifier : Verifier<TVerifier, TUnderTest>
    {
        private readonly LinkedList<Func<TUnderTest, VerificationResult>> _staticVerifiers = new LinkedList<Func<TUnderTest, VerificationResult>>();

        private LinkedList<Func<TUnderTest, VerificationResult>> _currentVerifiers;

        /// <summary>
        /// Gets if this verification result should be critical or not.
        /// </summary>
        /// <remarks>Critical verification result prevents later verifications, if it contains errors.</remarks>
        public bool IsCritical { get; set; }

        /// <summary>
        /// Initializes instance of <see cref="Verifier{TVerifier, TUnderTest}"/>
        /// </summary>
        [DebuggerStepThrough]
        protected Verifier()
        {
            _currentVerifiers = _staticVerifiers;
        }

        /// <summary>
        /// Adds verifier functions to this verifier.
        /// </summary>
        /// <param name="verifiers">Verifier functions.</param>
        public TVerifier AddVerifiers(params Func<TUnderTest, VerificationResult>[] verifiers)
        {
            _currentVerifiers.AddNotNullRange(verifiers);
            return (TVerifier)this;
        }

        /// <summary>
        /// Adds verifier actions to this verifier. Each action will produce critical verification result.
        /// If there is an exception in an action, the message of this exception will be added to the <see cref="VerificationResult.ErrorMessages"/> collection.
        /// </summary>
        /// <param name="verifiers">Verifier actions</param>
        public TVerifier AddCriticalVerifiers(params Action<TUnderTest>[] verifiers)
        {
            _currentVerifiers.AddCriticalVerifiers(verifiers);
            return (TVerifier)this;
        }

        /// <summary>
        /// Adds verifier actions to this verifier. Each action will produce normal verification result.
        /// If there is an exception in an action, the message of this exception will be added to the <see cref="VerificationResult.ErrorMessages"/> collection.
        /// </summary>
        /// <param name="verifiers">Verifier actions</param>
        public TVerifier AddNormalVerifiers(params Action<TUnderTest>[] verifiers)
        {
            _currentVerifiers.AddNormalVerifiers(verifiers);
            return (TVerifier)this;
        }

        /// <summary>
        /// Adds verifiers to this verifier.
        /// </summary>
        /// <param name="verifiers">Verifiers.</param>
        public TVerifier AddVerifiers(params IVerifier<TUnderTest>[] verifiers)
        {
            _currentVerifiers.AddVerifiers(verifiers);
            return (TVerifier)this;
        }

        /// <summary>
        /// Override this method to set dynamic verifiers based on knowledge of <paramref name="instanceUnderTest"/>.
        /// </summary>
        /// <param name="instanceUnderTest">Instance of object under test.</param>
        protected virtual void AddDynamicVerifiers(TUnderTest instanceUnderTest) { }

        /// <inheritdoc cref="IVerifier{TUnderTest}"/>
        public VerificationResult Verify(TUnderTest instanceUnderTest)
        {
            var result = GetVerificationResultFrom(_staticVerifiers, instanceUnderTest);
            if(!result.AllowContinue())
                return new VerificationResult(IsCritical, result.ErrorMessages);

            var dynamicVerifiers = new LinkedList<Func<TUnderTest, VerificationResult>>();

            try
            {
                _currentVerifiers = dynamicVerifiers;
                AddDynamicVerifiers(instanceUnderTest);
            }
            finally
            {
                _currentVerifiers = _staticVerifiers;
            }

            result += GetVerificationResultFrom(dynamicVerifiers, instanceUnderTest);

            dynamicVerifiers.Clear();

            return new VerificationResult(IsCritical, result.ErrorMessages);
        }

        private VerificationResult GetVerificationResultFrom(
            LinkedList<Func<TUnderTest, VerificationResult>> verificationFunctions, 
            TUnderTest instanceUnderTest)
        {
            if (verificationFunctions == null) throw new ArgumentNullException(nameof(verificationFunctions));

            var result = VerificationResult.Normal();

            foreach (var verifier in verificationFunctions)
            {
                try
                {
                    var verificationResult = verifier(instanceUnderTest);
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

            return result;
        }

        /// <summary>
        /// Throws exception if there are verification errors in <paramref name="instanceUnderTest"/>.
        /// </summary>
        /// <param name="instanceUnderTest">Instance of object under test.</param>
        /// <summary>
        /// If there are verification errors in <paramref name="instanceUnderTest"/>, this method calls 
        /// <see cref="Verifier.AssertionFailed"/> delegate.
        /// </summary>
        public void Check(TUnderTest instanceUnderTest)
        {
            var result = Verify(instanceUnderTest);
            if (result.HasErrors())
                Verifier.AssertionFailed(string.Join(Environment.NewLine, result.ErrorMessages));
        }
    }

    /// <summary>
    /// Provides common functionality for verifiers.
    /// </summary>
    public static class Verifier
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static Action<string> _assertionFailed;

        /// <summary>
        /// Gets or sets action to execute is assertion is failed.
        /// </summary>
        /// <remarks>You can set it to execute failure procedure of your assertion library, if you want.</remarks>
        public static Action<string> AssertionFailed
        {
            [DebuggerStepThrough]
            get => _assertionFailed;
            [DebuggerStepThrough]
            set => _assertionFailed = value ?? throw new ArgumentNullException(nameof(value));
        }

        static Verifier()
        {
            _assertionFailed = errorMessage =>
            {
                if (!string.IsNullOrWhiteSpace(errorMessage))
                    throw new VerificationException(errorMessage);
            };
        }
    }
}