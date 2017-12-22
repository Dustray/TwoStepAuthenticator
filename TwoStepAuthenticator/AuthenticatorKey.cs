using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwoStepAuthenticator
{
    public class AuthenticatorKey
    {
        /**
         * The configuration of this key.
         */
        private static AuthenticatorConfig config;

        /**
         * The secret key in Base32 encoding.
         */
        private static String key;

        /**
         * The verification code at time = 0 (the UNIX epoch).
         */
        private static int verificationCode;

        /**
         * The list of scratch codes.
         */
        private static List<int> scratchCodes;


        /**
         * The private constructor of this class.
         *
         * @param config           the configuration of the TOTP algorithm.
         * @param key              the secret key in Base32 encoding.
         * @param verificationCode the verification code at time = 0 (the UNIX epoch).
         * @param scratchCodes     the list of scratch codes.
         */
        private AuthenticatorKey(AuthenticatorConfig _config,
                                       String _key,
                                       int _verificationCode,
                                       List<int> _scratchCodes)
        {
            if (key == null)
            {
                // throw new IllegalArgumentException("Key cannot be null");
            }

            if (config == null)
            {
                //throw new IllegalArgumentException("Configuration cannot be null");
            }

            if (scratchCodes == null)
            {
                //throw new IllegalArgumentException("Scratch codes cannot be null");
            }

            config = _config;
            key = _key;
            verificationCode = _verificationCode;
            scratchCodes = new List<int>(_scratchCodes);
        }

        /**
         * Get the list of scratch codes.
         *
         * @return the list of scratch codes.
         */
        public List<int> getScratchCodes()
        {
            return scratchCodes;
        }

        /**
         * Get the config of this key.
         *
         * @return the config of this key.
         */
        public AuthenticatorConfig getConfig()
        {
            return config;
        }

        /**
         * Returns the secret key in Base32 encoding.
         *
         * @return the secret key in Base32 encoding.
         */
        public String getKey()
        {
            return key;
        }

        /**
         * Returns the verification code at time = 0 (the UNIX epoch).
         *
         * @return the verificationCode at time = 0 (the UNIX epoch).
         */
        public int getVerificationCode()
        {
            return verificationCode;
        }

        /**
         * This class is a builder to create instances of the {@link AuthenticatorKey} class.
         */
        public class Builder
        {
            private AuthenticatorConfig config = new AuthenticatorConfig();
            private String key;
            private int verificationCode;
            private List<int> scratchCodes = new List<int>();

            /**
             * Creates an instance of the builder.
             *
             * @param key the secret key in Base32 encoding.
             * @see AuthenticatorKey#AuthenticatorKey(AuthenticatorConfig, String, int, List)
             */
            public Builder(String key)
            {
                this.key = key;
            }

            /**
             * Creates an instance of the {@link AuthenticatorKey} class.
             *
             * @return an instance of the {@link AuthenticatorKey} class initialized with the properties set in this builder.
             * @see AuthenticatorKey#AuthenticatorKey(AuthenticatorConfig, String, int, List)
             */
            public AuthenticatorKey build()
            {
                return new AuthenticatorKey(config, key, verificationCode, scratchCodes);
            }

            /**
             * Sets the config of the TOTP algorithm for this key.
             *
             * @param config the config of the TOTP algorithm for this key.
             * @see AuthenticatorKey#AuthenticatorKey(AuthenticatorConfig, String, int, List)
             */
            public Builder setConfig(AuthenticatorConfig config)
            {
                this.config = config;
                return this;
            }

            /**
             * Sets the secret key.
             *
             * @param key the secret key.
             * @see AuthenticatorKey#AuthenticatorKey(AuthenticatorConfig, String, int, List)
             */
            public Builder setKey(String key)
            {
                this.key = key;
                return this;
            }

            /**
             * Sets the verification code.
             *
             * @param verificationCode the verification code.
             * @see AuthenticatorKey#AuthenticatorKey(AuthenticatorConfig, String, int, List)
             */
            public Builder setVerificationCode(int verificationCode)
            {
                this.verificationCode = verificationCode;
                return this;
            }

            /**
             * Sets the scratch codes.
             *
             * @param scratchCodes the scratch codes.
             * @see AuthenticatorKey#AuthenticatorKey(AuthenticatorConfig, String, int, List)
             */
            public Builder setScratchCodes(List<int> scratchCodes)
            {
                this.scratchCodes = scratchCodes;
                return this;
            }
        }
    }
}
