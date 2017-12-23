using DateTimeSpace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwoStepAuthenticator
{
    public class AuthenticatorConfig
    {
        private long timeStepSizeInMillis = DateTimeUnit.Seconds.ToMillis(30);//秒转换为毫秒
        private int windowSize = 3;
        private static int codeDigits = 6;
        private int keyModulus = (int)Math.Pow(10, codeDigits);
        private KeyRepresentation keyRepresentation = KeyRepresentation.BASE32;
        private HmacHashFunction hmacHashFunction = HmacHashFunction.HmacSHA1;

        /**
         * Returns the key module.
         *
         * @return the key module.
         */
        public int getKeyModulus()
        {
            return keyModulus;
        }

        /**
         * Returns the key representation.
         *
         * @return the key representation.
         */
        public KeyRepresentation getKeyRepresentation()
        {
            return keyRepresentation;
        }

        /**
         * Returns the number of digits in the generated code.
         *
         * @return the number of digits in the generated code.
         */
    public int getCodeDigits()
        {
            return codeDigits;
        }

        /**
         * Returns the time step size, in milliseconds, as specified by RFC 6238.
         * The default value is 30.000.
         *
         * @return the time step size in milliseconds.
         */
        public long getTimeStepSizeInMillis()
        {
            return timeStepSizeInMillis;
        }

        /**
         * Returns an int value representing the number of windows of size
         * timeStepSizeInMillis that are checked during the validation process,
         * to account for differences between the server and the client clocks.
         * The bigger the window, the more tolerant the library code is about
         * clock skews.
         * <p/>
         * We are using  's default behaviour of using a window size equal
         * to 3.  The limit on the maximum window size, present in older
         * versions of this library, has been removed.
         *
         * @return the window size.
         * @see #timeStepSizeInMillis
         */
        public int getWindowSize()
        {
            return windowSize;
        }

        /**
         * Returns the cryptographic hash function used to calculate the HMAC (Hash-based
         * Message Authentication Code). This implementation uses the SHA1 hash
         * function by default.
         * <p/>
         *
         * @return the HMAC hash function.
         */
        public HmacHashFunction getHmacHashFunction()
        {
            return hmacHashFunction;
        }

        public class  AuthenticatorConfigBuilder
        {
            private AuthenticatorConfig config = new  AuthenticatorConfig();

            public  AuthenticatorConfig build()
            {
                return config;
            }

            public  AuthenticatorConfigBuilder setCodeDigits(int codeDigits)
            {
                if (codeDigits <= 0)
                {
                    //throw new IllegalArgumentException("Code digits must be positive.");
                }

                if (codeDigits < 6)
                {
                    //throw new IllegalArgumentException("The minimum number of digits is 6.");
                }

                if (codeDigits > 8)
                {
                    //throw new IllegalArgumentException("The maximum number of digits is 8.");
                }

                AuthenticatorConfig.codeDigits = codeDigits;
                config.keyModulus = (int)Math.Pow(10, codeDigits);
                return this;
            }

            public  AuthenticatorConfigBuilder setTimeStepSizeInMillis(long timeStepSizeInMillis)
            {
                if (timeStepSizeInMillis <= 0)
                {
                    //throw new IllegalArgumentException("Time step size must be positive.");
                }

                config.timeStepSizeInMillis = timeStepSizeInMillis;
                return this;
            }

            public  AuthenticatorConfigBuilder setWindowSize(int windowSize)
            {
                if (windowSize <= 0)
                {
                   // throw new IllegalArgumentException("Window number must be positive.");
                }

                config.windowSize = windowSize;
                return this;
            }

            public  AuthenticatorConfigBuilder setKeyRepresentation(KeyRepresentation keyRepresentation)
            {
                if (keyRepresentation == null)
                {
                    //throw new IllegalArgumentException("Key representation cannot be null.");
                }

                config.keyRepresentation = keyRepresentation;
                return this;
            }

            public  AuthenticatorConfigBuilder setHmacHashFunction(HmacHashFunction hmacHashFunction)
            {
                if (hmacHashFunction == null)
                {
                    //throw new IllegalArgumentException("HMAC Hash Function cannot be null.");
                }

                config.hmacHashFunction = hmacHashFunction;
                return this;
            }
        }
    }
}
