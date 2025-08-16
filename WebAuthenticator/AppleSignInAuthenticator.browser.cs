using Microsoft.Maui.ApplicationModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Maui.Authentication
{
    partial class AppleSignInAuthenticatorImplementation : IAppleSignInAuthenticator
    {
        public Task<WebAuthenticatorResult> AuthenticateAsync(AppleSignInAuthenticator.Options options) =>
            throw ExceptionUtils.NotSupportedOrImplementedException;
    }
}
