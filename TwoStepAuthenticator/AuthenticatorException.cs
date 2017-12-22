using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwoStepAuthenticator
{
    class AuthenticatorException: Exception
    {

        private string msg;
        /**
    * Builds an exception with the provided error message.
    *
    * @param message the error message.
    */
        public  AuthenticatorException(String message) : base(message)
        {
            msg = message;
        }

        /**
         * Builds an exception with the provided error mesasge and
         * the provided cuase.
         *
         * @param message the error message.
         * @param cause   the cause.
         */
        public  AuthenticatorException(String message, Exception inner) : base(message, inner)
        {
            msg = message;
        }
        public override string ToString() // 重写ToString方法
        {
            return msg;
        }
    }
}
