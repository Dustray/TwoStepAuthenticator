using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwoStepAuthenticator
{
    class ReseedingSecureRandom
    {
        private const int MAX_OPERATIONS = 1_000_000;
        private  String provider;
    private  String algorithm;
    private  AtomicInteger count = new AtomicInteger(0);
        private SecureRandom secureRandom;
        
    ReseedingSecureRandom()
        {
            this.algorithm = null;
            this.provider = null;

            buildSecureRandom();
        }
        
    ReseedingSecureRandom(String algorithm)
        {
            if (algorithm == null)
            {
                //throw new IllegalArgumentException("Algorithm cannot be null.");
            }

            this.algorithm = algorithm;
            this.provider = null;

            buildSecureRandom();
        }

        ReseedingSecureRandom(String algorithm, String provider)
        {
            if (algorithm == null)
            {
                //throw new IllegalArgumentException("Algorithm cannot be null.");
            }

            if (provider == null)
            {
                //throw new IllegalArgumentException("Provider cannot be null.");
            }

            this.algorithm = algorithm;
            this.provider = provider;

            buildSecureRandom();
        }

        private void buildSecureRandom()
        {
            try
            {
                if (this.algorithm == null && this.provider == null)
                {
                    this.secureRandom = new SecureRandom();
                }
                else if (this.provider == null)
                {
                    this.secureRandom = SecureRandom.getInstance(this.algorithm);
                }
                else
                {
                    this.secureRandom = SecureRandom.getInstance(this.algorithm, this.provider);
                }
            }
            catch (NoSuchAlgorithmException e)
            {
                throw new GoogleAuthenticatorException(
                        String.format(
                                "Could not initialise SecureRandom with the specified algorithm: %s. " +
                                        "Another provider can be chosen setting the %s system property.",
                                this.algorithm,
                                GoogleAuthenticator.RNG_ALGORITHM
                        ), e
                );
            }
            catch (NoSuchProviderException e)
            {
                throw new GoogleAuthenticatorException(
                        String.format(
                                "Could not initialise SecureRandom with the specified provider: %s. " +
                                        "Another provider can be chosen setting the %s system property.",
                                this.provider,
                                GoogleAuthenticator.RNG_ALGORITHM_PROVIDER
                        ), e
                );
            }
        }

        void nextBytes(byte[] bytes)
        {
            if (count.incrementAndGet() > MAX_OPERATIONS)
            {
                synchronized(this) {
                    if (count.get() > MAX_OPERATIONS)
                    {
                        buildSecureRandom();
                        count.set(0);
                    }
                }
            }

            this.secureRandom.nextBytes(bytes);
        }
    }
}
