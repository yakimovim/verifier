using System;
using System.Collections.Generic;
using System.Linq;

namespace EdlinSoftware.Verifier
{
    internal static class ListOfVerifiersExtensions
    {
        public static LinkedList<Func<TUnderTest, VerificationResult>> AddVerifiers<TUnderTest>(
            this LinkedList<Func<TUnderTest, VerificationResult>> verifiersList,
            params Func<TUnderTest, VerificationResult>[] verifiers)
        {
            foreach (var verifier in verifiers.Where(v => v != null))
            {
                verifiersList.AddLast(verifier);
            }
            return verifiersList;
        }

        public static LinkedList<Func<TUnderTest, VerificationResult>> AddVerifiers<TUnderTest>(
            this LinkedList<Func<TUnderTest, VerificationResult>> verifiersList,
            params IVerifier<TUnderTest>[] verifiers)
        {
            verifiersList.AddVerifiers(verifiers.Where(v => v != null).Select(v => (Func<TUnderTest, VerificationResult>)v.Verify).ToArray());
            return verifiersList;
        }

        public static LinkedList<Func<TUnderTest, VerificationResult>> AddCriticalVerifiers<TUnderTest>(
            this LinkedList<Func<TUnderTest, VerificationResult>> verifiersList,
            params Action<TUnderTest>[] verifiers)
        {
            verifiersList.AddVerifiers(verifiers.Where(v => v != null).Select(v =>
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
            return verifiersList;
        }

        public static LinkedList<Func<TUnderTest, VerificationResult>> AddNormalVerifiers<TUnderTest>(
            this LinkedList<Func<TUnderTest, VerificationResult>> verifiersList,
            params Action<TUnderTest>[] verifiers)
        {
            verifiersList.AddVerifiers(verifiers.Where(v => v != null).Select(v =>
            {
                return (Func<TUnderTest, VerificationResult>)(instanceUnderTest =>
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
            return verifiersList;
        }
    }
}