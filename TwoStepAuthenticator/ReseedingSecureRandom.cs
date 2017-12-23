using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TwoStepAuthenticator
{
    class ReseedingSecureRandom
    {
        private const int MAX_OPERATIONS = 1_000_000;
        private  String provider;
        private  String algorithm;
        private  int count = new int();
        private RNGCryptoServiceProvider secureRandom;
        
       public  ReseedingSecureRandom()
        {
            this.algorithm = null;
            this.provider = null;

            buildSecureRandom();
        }
        
    public ReseedingSecureRandom(String algorithm)
        {
            if (algorithm == null)
            {
                //throw new IllegalArgumentException("Algorithm cannot be null.");
            }

            this.algorithm = algorithm;
            this.provider = null;

            buildSecureRandom();
        }

        public ReseedingSecureRandom(String algorithm, String provider)
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
                    this.secureRandom = new RNGCryptoServiceProvider();
                }
                else if (this.provider == null)
                {
                    this.secureRandom = new RNGCryptoServiceProvider(this.algorithm);
                }
                else
                {
                    this.secureRandom = new RNGCryptoServiceProvider(this.provider);
                }
            }
            catch (Exception e)
            {
                throw new AuthenticatorException(
                        String.Format(
                                "Could not initialise SecureRandom with the specified algorithm: %s. " +
                                        "Another provider can be chosen setting the %s system property.",
                                this.algorithm,
                                 Authenticator.RNG_ALGORITHM
                        ), e
                );
            }
           
        }

        void nextBytes(byte[] bytes)
        {
            if (++count > MAX_OPERATIONS)
            {
                lock(this) {
                    if (count > MAX_OPERATIONS)
                    {
                        buildSecureRandom();

                    }
                }
            }

            this.secureRandom.GetNonZeroBytes(bytes);
        }
    }
}
