using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace EdlinSoftware.Verifier
{
    /// <summary>
    /// Represents verifier of complex object.
    /// </summary>
    /// <typeparam name="TUnderTest">Type of object under test.</typeparam>
    public interface IVerifier<TUnderTest>
    {
        /// <summary>
        /// Adds verification function to the container.
        /// </summary>
        /// <param name="verifiers">Verification functions.</param>
        IVerifier<TUnderTest> AddVerifiers(params Func<TUnderTest, VerificationResult>[] verifiers);

        /// <summary>
        /// Runs all registered verifiers and return cumulative result.
        /// </summary>
        /// <param name="instanceUnderTest">Instance under test.</param>
        VerificationResult Verify(TUnderTest instanceUnderTest);
    }

    /// <summary>
    /// Represents verifier of complex object.
    /// </summary>
    /// <typeparam name="TUnderTest">Type of object under test.</typeparam>
    public abstract class Verifier<TUnderTest> : IVerifier<TUnderTest>
    {
        private readonly LinkedList<Func<TUnderTest, VerificationResult>> _staticVerifiers = new LinkedList<Func<TUnderTest, VerificationResult>>();
        private readonly LinkedList<Func<TUnderTest, VerificationResult>> _dynamicVerifiers = new LinkedList<Func<TUnderTest, VerificationResult>>();

        private Action<Func<TUnderTest, VerificationResult>[]> _adder;

        private void AddStaticVerifiers(params Func<TUnderTest, VerificationResult>[] verifiers)
        {
            foreach (var verifier in verifiers.Where(v => v != null))
            {
                _staticVerifiers.AddLast(verifier);
            }
        }

        private void AddDynamicVerifiers(params Func<TUnderTest, VerificationResult>[] verifiers)
        {
            foreach (var verifier in verifiers.Where(v => v != null))
            {
                _dynamicVerifiers.AddLast(verifier);
            }
        }

        /// <summary>
        /// Gets if this verification result should be critical or not.
        /// </summary>
        /// <remarks>Critical verification result prevents later verifications, if it contains errors.</remarks>
        public bool IsCritical { get; set; }

        /// <summary>
        /// Initializes instance of <see cref="Verifier{TUnderTest}"/>
        /// </summary>
        [DebuggerStepThrough]
        protected Verifier()
        {
            _adder = AddStaticVerifiers;
        }

        /// <inheritdoc cref="IVerifier{TUnderTest}"/>
        public IVerifier<TUnderTest> AddVerifiers(params Func<TUnderTest, VerificationResult>[] verifiers)
        {
            _adder(verifiers);
            return this;
        }
        
        /// <summary>
        /// Adds dynamic verifiers.
        /// </summary>
        /// <param name="instanceUnderTest">Instance under test.</param>
        protected virtual void AddDynamicVerifiers(TUnderTest instanceUnderTest) { }

        /// <summary>
        /// Runs all registered verifiers and return cumulative result.
        /// </summary>
        /// <param name="instanceUnderTest">Instance under test.</param>
        /// <returns>Result of verification. Value of <see cref="VerificationResult.IsCritical"/> property will be in any case equal to the value of <see cref="IsCritical"/> of this object.</returns>
        public VerificationResult Verify(TUnderTest instanceUnderTest)
        {
            try
            {
                _adder = AddStaticVerifiers;
                AddDynamicVerifiers(instanceUnderTest);
            }
            finally
            {
                _adder = AddDynamicVerifiers;
            }

            var errorMessages = new List<string>();

            foreach (var verifier in _staticVerifiers.Concat(_dynamicVerifiers))
            {
                try
                {
                    var verifierMessages = verifier(instanceUnderTest);
                    errorMessages.AddRange(verifierMessages.ErrorMessages);
                    if(!verifierMessages.AllowContinue())
                    { break; }
                }
                catch (Exception e)
                {
                    errorMessages.Add(e.Message);
                    break;
                }
            }

            return new VerificationResult(IsCritical, errorMessages.ToArray());
        }
    }

    /// <summary>
    /// Provides help methods for verifiers.
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

        /// <summary>
        /// Asserts that <paramref name="instanceUnderTest"/> conforms with the <paramref name="verifier"/>.
        /// </summary>
        /// <typeparam name="TUnderTest">Type of object under test.</typeparam>
        /// <param name="verifier">Verifier.</param>
        /// <param name="instanceUnderTest">Instance under test.</param>
        public static void Assert<TUnderTest>(this Verifier<TUnderTest> verifier, TUnderTest instanceUnderTest)
        {
            if (verifier == null) throw new ArgumentNullException(nameof(verifier));

            var result = verifier.Verify(instanceUnderTest);
            if (result.HasErrors())
                AssertionFailed(string.Join(Environment.NewLine, result.ErrorMessages));
        }

        /// <summary>
        /// Adds actions as critical verifiers. Critical verification result will be returned for each of them. 
        /// If there is an exception in the <paramref name="verifiers"/> action, its message will be in the <see cref="VerificationResult.ErrorMessages"/> of the verification result.
        /// </summary>
        /// <typeparam name="TUnderTest">Type of object under test.</typeparam>
        /// <param name="container">Verifiers container.</param>
        /// <param name="verifiers">Verifying actions.</param>
        public static IVerifier<TUnderTest> AddCriticalVerifiers<TUnderTest>(this IVerifier<TUnderTest> container, params Action<TUnderTest>[] verifiers)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));

            container.AddVerifiers(verifiers.Where(v => v != null).Select(v =>
            {
                return (Func<TUnderTest, VerificationResult>)(instanceUnderTest =>
                {
                    try
                    {
                        v(instanceUnderTest);
                        return VerificationResult.Critical();
                    }
                    catch (Exception e)
                    {
                        return VerificationResult.Critical(e.Message);
                    }
                });
            }).ToArray());

            return container;
        }

        /// <summary>
        /// Adds actions as normal verifiers. Normal verification result will be returned for each of them. 
        /// If there is an exception in the <paramref name="verifiers"/> action, its message will be in the <see cref="VerificationResult.ErrorMessages"/> of the verification result.
        /// </summary>
        /// <typeparam name="TUnderTest">Type of object under test.</typeparam>
        /// <param name="container">Verifiers container.</param>
        /// <param name="verifiers">Verifying actions.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public static IVerifier<TUnderTest> AddNormalVerifiers<TUnderTest>(this IVerifier<TUnderTest> container, params Action<TUnderTest>[] verifiers)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));

            container.AddVerifiers(verifiers.Where(v => v != null).Select(v =>
            {
                return (Func<TUnderTest, VerificationResult>) (instanceUnderTest =>
                {
                    try
                    {
                        v(instanceUnderTest);
                        return VerificationResult.Normal();
                    }
                    catch (Exception e)
                    {
                        return VerificationResult.Normal(e.Message);
                    }
                });
            }).ToArray());

            return container;
        }

        /// <summary>
        /// Adds external verifiers to the verifiers container.
        /// If there is an exception in the <see cref="Verifier{TUnderTest}.Verify"/> method, critical verification result will be returned with message of the exception in the <see cref="VerificationResult.ErrorMessages"/> of the verification result.
        /// </summary>
        /// <typeparam name="TUnderTest">Type of object under test.</typeparam>
        /// <param name="container">Verifiers container.</param>
        /// <param name="verifiers">Verifiers.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public static IVerifier<TUnderTest> AddVerifiers<TUnderTest>(this IVerifier<TUnderTest> container,
            params Verifier<TUnderTest>[] verifiers)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            container.AddVerifiers(verifiers.Where(v => v != null).Select(v => (Func<TUnderTest, VerificationResult>)v.Verify).ToArray());
            return container;
        }
    }
}