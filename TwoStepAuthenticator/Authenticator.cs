﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwoStepAuthenticator
{
    public class Authenticator
    {

        /**
         * The system property to specify the random number generator algorithm to use.
         *
         * @since 0.5.0
         */
        public static sealed String RNG_ALGORITHM = "com.warrenstrange.googleauth.rng.algorithm";

    /**
     * The system property to specify the random number generator provider to use.
     *
     * @since 0.5.0
     */
    public static sealed String RNG_ALGORITHM_PROVIDER = "com.warrenstrange.googleauth.rng.algorithmProvider";

    /**
     * The logger for this class.
     */
    private static sealed Logger LOGGER = Logger.getLogger(GoogleAuthenticator.class.getName());

    /**
     * The number of bits of a secret key in binary form. Since the Base32
     * encoding with 8 bit characters introduces an 160% overhead, we just need
     * 80 bits (10 bytes) to generate a 16 bytes Base32-encoded secret key.
     */
    private static sealed int SECRET_BITS = 80;

        /**
         * Number of scratch codes to generate during the key generation.
         * We are using Google's default of providing 5 scratch codes.
         */
        private static sealed int SCRATCH_CODES = 5;

        /**
         * Number of digits of a scratch code represented as a decimal integer.
         */
        private static sealed int SCRATCH_CODE_LENGTH = 8;

        /**
         * Modulus used to truncate the scratch code.
         */
        public static sealed int SCRATCH_CODE_MODULUS = (int)Math.pow(10, SCRATCH_CODE_LENGTH);

        /**
         * Magic number representing an invalid scratch code.
         */
        private static sealed int SCRATCH_CODE_INVALID = -1;

        /**
         * Length in bytes of each scratch code. We're using Google's default of
         * using 4 bytes per scratch code.
         */
        private static sealed int BYTES_PER_SCRATCH_CODE = 4;

        /**
         * The default SecureRandom algorithm to use if none is specified.
         *
         * @see java.security.SecureRandom#getInstance(String)
         * @since 0.5.0
         */
        @SuppressWarnings("SpellCheckingInspection")
    private static sealed String DEFAULT_RANDOM_NUMBER_ALGORITHM = "SHA1PRNG";

    /**
     * The default random number algorithm provider to use if none is specified.
     *
     * @see java.security.SecureRandom#getInstance(String)
     * @since 0.5.0
     */
    private static sealed String DEFAULT_RANDOM_NUMBER_ALGORITHM_PROVIDER = "SUN";

    /**
     * The configuration used by the current instance.
     */
    private sealed GoogleAuthenticatorConfig config;

    /**
     * The internal SecureRandom instance used by this class.  Since Java 7
     * {@link Random} instances are required to be thread-safe, no synchronisation is
     * required in the methods of this class using this instance.  Thread-safety
     * of this class was a de-facto standard in previous versions of Java so
     * that it is expected to work correctly in previous versions of the Java
     * platform as well.
     */
    private ReseedingSecureRandom secureRandom = new ReseedingSecureRandom(
            getRandomNumberAlgorithm(),
            getRandomNumberAlgorithmProvider());

        private ICredentialRepository credentialRepository;
        private boolean credentialRepositorySearched;

        public GoogleAuthenticator()
        {
            config = new GoogleAuthenticatorConfig();
        }

        public GoogleAuthenticator(GoogleAuthenticatorConfig config)
        {
            if (config == null)
            {
                throw new IllegalArgumentException("Configuration cannot be null.");
            }

            this.config = config;
        }

        /**
         * @return the random number generator algorithm.
         * @since 0.5.0
         */
        private String getRandomNumberAlgorithm()
        {
            return System.getProperty(
                    RNG_ALGORITHM,
                    DEFAULT_RANDOM_NUMBER_ALGORITHM);
        }

        /**
         * @return the random number generator algorithm provider.
         * @since 0.5.0
         */
        private String getRandomNumberAlgorithmProvider()
        {
            return System.getProperty(
                    RNG_ALGORITHM_PROVIDER,
                    DEFAULT_RANDOM_NUMBER_ALGORITHM_PROVIDER);
        }

        /**
         * Calculates the verification code of the provided key at the specified
         * instant of time using the algorithm specified in RFC 6238.
         *
         * @param key the secret key in binary format.
         * @param tm  the instant of time.
         * @return the validation code for the provided key at the specified instant
         * of time.
         */
        int calculateCode(byte[] key, long tm)
        {
            // Allocating an array of bytes to represent the specified instant
            // of time.
            byte[] data = new byte[8];
            long value = tm;

            // Converting the instant of time from the long representation to a
            // big-endian array of bytes (RFC4226, 5.2. Description).
            for (int i = 8; i-- > 0; value >>>= 8)
            {
                data[i] = (byte)value;
            }

            // Building the secret key specification for the HmacSHA1 algorithm.
            SecretKeySpec signKey = new SecretKeySpec(key, config.getHmacHashFunction().toString());

            try
            {
                // Getting an HmacSHA1/HmacSHA256 algorithm implementation from the JCE.
                Mac mac = Mac.getInstance(config.getHmacHashFunction().toString());

                // Initializing the MAC algorithm.
                mac.init(signKey);

                // Processing the instant of time and getting the encrypted data.
                byte[] hash = mac.doFinal(data);

                // Building the validation code performing dynamic truncation
                // (RFC4226, 5.3. Generating an HOTP value)
                int offset = hash[hash.length - 1] & 0xF;

                // We are using a long because Java hasn't got an unsigned integer type
                // and we need 32 unsigned bits).
                long truncatedHash = 0;

                for (int i = 0; i < 4; ++i)
                {
                    truncatedHash <<= 8;

                    // Java bytes are signed but we need an unsigned integer:
                    // cleaning off all but the LSB.
                    truncatedHash |= (hash[offset + i] & 0xFF);
                }

                // Clean bits higher than the 32nd (inclusive) and calculate the
                // module with the maximum validation code value.
                truncatedHash &= 0x7FFFFFFF;
                truncatedHash %= config.getKeyModulus();

                // Returning the validation code to the caller.
                return (int)truncatedHash;
            }
            catch (NoSuchAlgorithmException | InvalidKeyException ex)
        {
                // Logging the exception.
                LOGGER.log(Level.SEVERE, ex.getMessage(), ex);

                // We're not disclosing internal error details to our clients.
                throw new GoogleAuthenticatorException("The operation cannot be "
                        + "performed now.");
            }
            }

            private long getTimeWindowFromTime(long time)
            {
                return time / this.config.getTimeStepSizeInMillis();
            }

            /**
             * This method implements the algorithm specified in RFC 6238 to check if a
             * validation code is valid in a given instant of time for the given secret
             * key.
             *
             * @param secret    the Base32 encoded secret key.
             * @param code      the code to validate.
             * @param timestamp the instant of time to use during the validation process.
             * @param window    the window size to use during the validation process.
             * @return <code>true</code> if the validation code is valid,
             * <code>false</code> otherwise.
             */
            private boolean checkCode(
                    String secret,
                    long code,
                    long timestamp,
                    int window)
            {
                byte[] decodedKey = decodeSecret(secret);

                // convert unix time into a 30 second "window" as specified by the
                // TOTP specification. Using Google's default interval of 30 seconds.
                sealed long timeWindow = getTimeWindowFromTime(timestamp);

                // Calculating the verification code of the given key in each of the
                // time intervals and returning true if the provided code is equal to
                // one of them.
                for (int i = -((window - 1) / 2); i <= window / 2; ++i)
                {
                    // Calculating the verification code for the current time interval.
                    long hash = calculateCode(decodedKey, timeWindow + i);

                    // Checking if the provided code is equal to the calculated one.
                    if (hash == code)
                    {
                        // The verification code is valid.
                        return true;
                    }
                }

                // The verification code is invalid.
                return false;
            }

            private byte[] decodeSecret(String secret)
            {
                // Decoding the secret key to get its raw byte representation.
                switch (config.getKeyRepresentation())
                {
                    case BASE32:
                        Base32 codec32 = new Base32();
                        // See: https://issues.apache.org/jira/browse/CODEC-234
                        // Commons Codec Base32::decode does not support lowercase letters.
                        return codec32.decode(secret.toUpperCase());
                    case BASE64:
                        Base64 codec64 = new Base64();
                        return codec64.decode(secret);
                    default:
                        throw new IllegalArgumentException("Unknown key representation type.");
                }
            }

            @Override
        public GoogleAuthenticatorKey createCredentials()
            {

                // Allocating a buffer sufficiently large to hold the bytes required by
                // the secret key and the scratch codes.
                byte[] buffer =
                        new byte[SECRET_BITS / 8 + SCRATCH_CODES * BYTES_PER_SCRATCH_CODE];

                secureRandom.nextBytes(buffer);

                // Extracting the bytes making up the secret key.
                byte[] secretKey = Arrays.copyOf(buffer, SECRET_BITS / 8);
                String generatedKey = calculateSecretKey(secretKey);

                // Generating the verification code at time = 0.
                int validationCode = calculateValidationCode(secretKey);

                // Calculate scratch codes
                List<Integer> scratchCodes = calculateScratchCodes(buffer);

                return
                        new GoogleAuthenticatorKey
                                .Builder(generatedKey)
                                .setConfig(config)
                                .setVerificationCode(validationCode)
                                .setScratchCodes(scratchCodes)
                                .build();
            }

            @Override
        public GoogleAuthenticatorKey createCredentials(String userName)
            {
                // Further validation will be performed by the configured provider.
                if (userName == null)
                {
                    throw new IllegalArgumentException("User name cannot be null.");
                }

                GoogleAuthenticatorKey key = createCredentials();

                ICredentialRepository repository = getValidCredentialRepository();
                repository.saveUserCredentials(
                        userName,
                        key.getKey(),
                        key.getVerificationCode(),
                        key.getScratchCodes());

                return key;
            }

            private List<Integer> calculateScratchCodes(byte[] buffer)
            {
                List<Integer> scratchCodes = new ArrayList<>();

                while (scratchCodes.size() < SCRATCH_CODES)
                {
                    byte[] scratchCodeBuffer = Arrays.copyOfRange(
                            buffer,
                            SECRET_BITS / 8 + BYTES_PER_SCRATCH_CODE * scratchCodes.size(),
                            SECRET_BITS / 8 + BYTES_PER_SCRATCH_CODE * scratchCodes.size() + BYTES_PER_SCRATCH_CODE);

                    int scratchCode = calculateScratchCode(scratchCodeBuffer);

                    if (scratchCode != SCRATCH_CODE_INVALID)
                    {
                        scratchCodes.add(scratchCode);
                    }
                    else
                    {
                        scratchCodes.add(generateScratchCode());
                    }
                }

                return scratchCodes;
            }

            /**
             * This method calculates a scratch code from a random byte buffer of
             * suitable size <code>#BYTES_PER_SCRATCH_CODE</code>.
             *
             * @param scratchCodeBuffer a random byte buffer whose minimum size is
             *                          <code>#BYTES_PER_SCRATCH_CODE</code>.
             * @return the scratch code.
             */
            private int calculateScratchCode(byte[] scratchCodeBuffer)
            {
                if (scratchCodeBuffer.length < BYTES_PER_SCRATCH_CODE)
                {
                    throw new IllegalArgumentException(
                            String.format(
                                    "The provided random byte buffer is too small: %d.",
                                    scratchCodeBuffer.length));
                }

                int scratchCode = 0;

                for (int i = 0; i < BYTES_PER_SCRATCH_CODE; ++i)
                {
                    scratchCode = (scratchCode << 8) + (scratchCodeBuffer[i] & 0xff);
                }

                scratchCode = (scratchCode & 0x7FFFFFFF) % SCRATCH_CODE_MODULUS;

                // Accept the scratch code only if it has exactly
                // SCRATCH_CODE_LENGTH digits.
                if (validateScratchCode(scratchCode))
                {
                    return scratchCode;
                }
                else
                {
                    return SCRATCH_CODE_INVALID;
                }
            }

            /* package */
            boolean validateScratchCode(int scratchCode)
            {
                return (scratchCode >= SCRATCH_CODE_MODULUS / 10);
            }

            /**
             * This method creates a new random byte buffer from which a new scratch
             * code is generated. This function is invoked if a scratch code generated
             * from the main buffer is invalid because it does not satisfy the scratch
             * code restrictions.
             *
             * @return A valid scratch code.
             */
            private int generateScratchCode()
            {
                while (true)
                {
                    byte[] scratchCodeBuffer = new byte[BYTES_PER_SCRATCH_CODE];
                    secureRandom.nextBytes(scratchCodeBuffer);

                    int scratchCode = calculateScratchCode(scratchCodeBuffer);

                    if (scratchCode != SCRATCH_CODE_INVALID)
                    {
                        return scratchCode;
                    }
                }
            }

            /**
             * This method calculates the validation code at time 0.
             *
             * @param secretKey The secret key to use.
             * @return the validation code at time 0.
             */
            private int calculateValidationCode(byte[] secretKey)
            {
                return calculateCode(secretKey, 0);
            }


            public int getTotpPassword(String secret)
            {
                return getTotpPassword(secret, new Date().getTime());
            }

            public int getTotpPassword(String secret, long time)
            {
                return calculateCode(decodeSecret(secret), getTimeWindowFromTime(time));
            }

            public int getTotpPasswordOfUser(String userName)
            {
                return getTotpPasswordOfUser(userName, new Date().getTime());
            }

            public int getTotpPasswordOfUser(String userName, long time)
            {
                ICredentialRepository repository = getValidCredentialRepository();

                return calculateCode(
                        decodeSecret(repository.getSecretKey(userName)),
                        getTimeWindowFromTime(time));
            }

            /**
             * This method calculates the secret key given a random byte buffer.
             *
             * @param secretKey a random byte buffer.
             * @return the secret key.
             */
            private String calculateSecretKey(byte[] secretKey)
            {
                switch (config.getKeyRepresentation())
                {
                    case BASE32:
                        return new Base32().encodeToString(secretKey);
                    case BASE64:
                        return new Base64().encodeToString(secretKey);
                    default:
                        throw new IllegalArgumentException("Unknown key representation type.");
                }
            }

            @Override
        public boolean authorize(String secret, int verificationCode)
                throws GoogleAuthenticatorException
        {
                return authorize(secret, verificationCode, new Date().getTime());
            }

            @Override
        public boolean authorize(String secret, int verificationCode, long time)
                throws GoogleAuthenticatorException
        {
                // Checking user input and failing if the secret key was not provided.
                if (secret == null)
                {
                    throw new IllegalArgumentException("Secret cannot be null.");
                }

                // Checking if the verification code is between the legal bounds.
                if (verificationCode <= 0 || verificationCode >= this.config.getKeyModulus())
                {
                    return false;
                }

                // Checking the validation code using the current UNIX time.
                return checkCode(
                        secret,
                        verificationCode,
                        time,
                        this.config.getWindowSize());
            }

            @Override
        public boolean authorizeUser(String userName, int verificationCode)
                throws GoogleAuthenticatorException
        {
                return authorizeUser(userName, verificationCode, new Date().getTime());
            }

            @Override
        public boolean authorizeUser(String userName, int verificationCode, long time) throws GoogleAuthenticatorException
        {
                ICredentialRepository repository = getValidCredentialRepository();

                return authorize(repository.getSecretKey(userName), verificationCode, time);
            }

            /**
             * This method loads the first available and valid ICredentialRepository
             * registered using the Java service loader API.
             *
             * @return the first registered ICredentialRepository.
             * @throws java.lang.UnsupportedOperationException if no valid service is
             *                                                 found.
             */
            private ICredentialRepository getValidCredentialRepository()
            {
                ICredentialRepository repository = getCredentialRepository();

                if (repository == null)
                {
                    throw new UnsupportedOperationException(
                            String.format("An instance of the %s service must be " +
                                            "configured in order to use this feature.",
                                    ICredentialRepository.class.getName()
                    )
            );
        }

        return repository;
    }

/**
 * This method loads the first available ICredentialRepository
 * registered using the Java service loader API.
 *
 * @return the first registered ICredentialRepository or <code>null</code>
 * if none is found.
 */
public ICredentialRepository getCredentialRepository()
{
    if (this.credentialRepositorySearched) return this.credentialRepository;

    this.credentialRepositorySearched = true;

    ServiceLoader<ICredentialRepository> loader =
            ServiceLoader.load(ICredentialRepository.class);

        //noinspection LoopStatementThatDoesntLoop
        for (ICredentialRepository repository : loader)
        {
            this.credentialRepository = repository;
            break;
        }

        return this.credentialRepository;
    }

@Override
    public void setCredentialRepository(ICredentialRepository repository)
{
    this.credentialRepository = repository;
    this.credentialRepositorySearched = true;
}
    }
}
