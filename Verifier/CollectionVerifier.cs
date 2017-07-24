using System;
using System.Collections.Generic;

namespace EdlinSoftware.Verifier
{
    /// <summary>
    /// Represents verifier of collection of objects.
    /// </summary>
    /// <typeparam name="TVerifier">Type of verifier.</typeparam>
    /// <typeparam name="TElement">Type of elements of the collection.</typeparam>
    public abstract class CollectionVerifier<TVerifier, TElement> : Verifier<TVerifier, IEnumerable<TElement>>
        where TVerifier : CollectionVerifier<TVerifier, TElement>
    {
        private readonly LinkedList<Func<TElement, VerificationResult>> _itemVerifiers = new LinkedList<Func<TElement, VerificationResult>>();

        /// <summary>
        /// Initializes instance of <see cref="CollectionVerifier{TVerifier, TUnderTest}"/>
        /// </summary>
        protected CollectionVerifier()
        {
            AddVerifiers(instanceUnderTest => VerificationResult.Critical(instanceUnderTest == null ? "Collection should not be null" : null));
        }

        /// <summary>
        /// Adds verifier functions for elements of the collection under test to this verifier.
        /// </summary>
        /// <param name="verifiers">Verifier functions.</param>
        public TVerifier AddItemVerifiers(params Func<TElement, VerificationResult>[] verifiers)
        {
            _itemVerifiers.AddNotNullRange(verifiers);
            return (TVerifier)this;
        }

        /// <summary>
        /// Adds verifier actions for elements of the collection under test to this verifier. Each action will produce critical verification result.
        /// If there is an exception in an action, the message of this exception will be added to the <see cref="VerificationResult.ErrorMessages"/> collection.
        /// </summary>
        /// <param name="verifiers">Verifier actions</param>
        public TVerifier AddCriticalItemVerifiers(params Action<TElement>[] verifiers)
        {
            _itemVerifiers.AddCriticalVerifiers(verifiers);
            return (TVerifier)this;
        }

        /// <summary>
        /// Adds verifier actions for elements of the collection under test to this verifier. Each action will produce normal verification result.
        /// If there is an exception in an action, the message of this exception will be added to the <see cref="VerificationResult.ErrorMessages"/> collection.
        /// </summary>
        /// <param name="verifiers">Verifier actions</param>
        public TVerifier AddNormalItemVerifiers(params Action<TElement>[] verifiers)
        {
            _itemVerifiers.AddNormalVerifiers(verifiers);
            return (TVerifier)this;
        }

        /// <summary>
        /// Adds verifiers for elements of the collection under test to this verifier.
        /// </summary>
        /// <param name="verifiers">Verifiers.</param>
        public TVerifier AddItemVerifiers(params IVerifier<TElement>[] verifiers)
        {
            _itemVerifiers.AddVerifiers(verifiers);
            return (TVerifier)this;
        }

        /// <inheritdoc />
        protected override void AddDynamicVerifiers(IEnumerable<TElement> instanceUnderTest)
        {
            // ReSharper disable PossibleMultipleEnumeration
            base.AddDynamicVerifiers(instanceUnderTest);

            if(instanceUnderTest == null)
                return;

            using (var enumerator = instanceUnderTest.GetEnumerator())
            {
                var elementIndex = 0;
                foreach (var itemVerifier in _itemVerifiers)
                {
                    if (!enumerator.MoveNext())
                    {
                        AddVerifiers(
                            iut => VerificationResult.Normal(
                                $"{_itemVerifiers.Count} elements were expected, but there are only {elementIndex} elements."));
                        return;
                    }
                    var element = enumerator.Current;
                    AddVerifiers(iut => itemVerifier(element));
                    elementIndex++;
                }

                if (enumerator.MoveNext())
                {
                    AddVerifiers(
                        iut => VerificationResult.Normal(
                            $"{_itemVerifiers.Count} elements were expected, but there are more elements."));
                }
            }
            // ReSharper restore PossibleMultipleEnumeration
        }
    }
}