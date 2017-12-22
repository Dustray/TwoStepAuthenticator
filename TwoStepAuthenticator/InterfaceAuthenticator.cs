﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwoStepAuthenticator
{
    interface InterfaceAuthenticator
    {
        /**
   * This method generates a new set of credentials including:
   * <ol>
   * <li>Secret key.</li>
   * <li>Validation code.</li>
   * <li>A list of scratch codes.</li>
   * </ol>
   * <p/>
   * The user must register this secret on their device.
   *
   * @return secret key
   */
         AuthenticatorKey createCredentials();

        /**
         * This method generates a new set of credentials invoking the
         * <code>#createCredentials</code> method with no arguments. The generated
         * credentials are then saved using the configured
         * <code>#ICredentialRepository</code> service.
         * <p/>
         * The user must register this secret on their device.
         *
         * @param userName the user name.
         * @return secret key
         */
         AuthenticatorKey createCredentials(String userName);

        /**
         * This method generates the current TOTP password.
         *
         * @param secret the encoded secret key.
         * @return the current TOTP password.
         * @since 1.1.0
         */
        int getTotpPassword(String secret);

        /**
         * This method generates the TOTP password at the specified time.
         *
         * @param secret The encoded secret key.
         * @param time   The time to use to calculate the password.
         * @return the TOTP password at the specified time.
         * @since 1.1.0
         */
        int getTotpPassword(String secret, long time);

        /**
         * This method generates the current TOTP password.
         *
         * @param userName The user whose password must be created.
         * @return the current TOTP password.
         * @since 1.1.0
         */
        int getTotpPasswordOfUser(String userName);

        /**
         * This method generates the TOTP password at the specified time.
         *
         * @param userName The user whose password must be created.
         * @param time     The time to use to calculate the password.
         * @return the TOTP password at the specified time.
         * @since 1.1.0
         */
        int getTotpPasswordOfUser(String userName, long time);

        /**
         * Checks a verification code against a secret key using the current time.
         *
         * @param secret           the encoded secret key.
         * @param verificationCode the verification code.
         * @return <code>true</code> if the validation code is valid,
         * <code>false</code> otherwise.
         * @throws  AuthenticatorException if a failure occurs during the
         *                                      calculation of the validation code.
         *                                      The only failures that should occur
         *                                      are related with the cryptographic
         *                                      functions provided by the JCE.
         * @see #authorize(String, int, long)
         */
        bool authorize(String secret, int verificationCode);

        /**
         * Checks a verification code against a secret key using the specified time.
         * The algorithm also checks in a time window whose size determined by the
         * {@code windowSize} property of this class.
         * <p/>
         * The default value of 30 seconds recommended by RFC 6238 is used for the
         * interval size.
         *
         * @param secret           The encoded secret key.
         * @param verificationCode The verification code.
         * @param time             The time to use to calculate the TOTP password..
         * @return {@code true} if the validation code is valid, {@code false}
         * otherwise.
         * @throws  AuthenticatorException if a failure occurs during the
         *                                      calculation of the validation code.
         *                                      The only failures that should occur
         *                                      are related with the cryptographic
         *                                      functions provided by the JCE.
         * @since 0.6.0
         */
        bool authorize(String secret, int verificationCode, long time);

        /**
         * This method validates a verification code of the specified user whose
         * private key is retrieved from the configured credential repository using
         * the current time.  This method delegates the validation to the
         * {@link #authorizeUser(String, int, long)}.
         *
         * @param userName         The user whose verification code is to be
         *                         validated.
         * @param verificationCode The validation code.
         * @return <code>true</code> if the validation code is valid,
         * <code>false</code> otherwise.
         * @throws  AuthenticatorException if an unexpected error occurs.
         * @see #authorize(String, int)
         */
        bool authorizeUser(String userName, int verificationCode);

        /**
         * This method validates a verification code of the specified user whose
         * private key is retrieved from the configured credential repository.  This
         * method delegates the validation to the
         * {@link #authorize(String, int, long)} method.
         *
         * @param userName         The user whose verification code is to be
         *                         validated.
         * @param verificationCode The validation code.
         * @param time             The time to use to calculate the TOTP password.
         * @return <code>true</code> if the validation code is valid,
         * <code>false</code> otherwise.
         * @throws  AuthenticatorException if an unexpected error occurs.
         * @see #authorize(String, int)
         * @since 0.6.0
         */
        bool authorizeUser(String userName, int verificationCode, long time);

        /**
         * This method returns the credential repository used by this instance, or
         * {@code null} if none is set or none can be found using the ServiceLoader
         * API.
         *
         * @return the credential repository used by this instance.
         * @since 1.0.0
         */
        InterfaceCredentialRepository getCredentialRepository();

        /**
         * This method sets the credential repository used by this instance.  If
         * {@code null} is passed to this method, no credential repository will be
         * used, nor discovered using the ServiceLoader API.
         *
         * @param repository The credential repository to use, or {@code null} to
         *                   disable this feature.
         * @since 1.0.0
         */
        void setCredentialRepository(InterfaceCredentialRepository repository);
    }
}
