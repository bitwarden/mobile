#if !FDROID
using System.Collections.Generic;
using Android.Gms.Fido.Common;
using Android.Gms.Fido.Fido2.Api.Common;
using Bit.Core.Models.Data;
using Bit.Core.Models.Response;
using Bit.Core.Utilities;
using Java.Lang;
using Newtonsoft.Json.Linq;

namespace Bit.Droid.Fido2System
{
    class Fido2BuilderObject
    {
        public static PublicKeyCredentialRequestOptions ParsePublicKeyCredentialRequestOptions(
            Fido2AuthenticationChallengeResponse data)
        {
            if (data == null)
            {
                return null;
            }

            var builder = new PublicKeyCredentialRequestOptions.Builder();

            if (!string.IsNullOrEmpty(data.Challenge))
            {
                builder.SetChallenge(CoreHelpers.Base64UrlDecode(data.Challenge));
            }
            if (data.AllowCredentials != null && data.AllowCredentials.Count > 0)
            {
                builder.SetAllowList(ParseCredentialDescriptors(data.AllowCredentials));
            }
            if (!string.IsNullOrEmpty(data.RpId))
            {
                builder.SetRpId(data.RpId);
            }
            if (data.Timeout > 0)
            {
                builder.SetTimeoutSeconds((Double)(data.Timeout / 1000));
            }
            if (data.Extensions != null)
            {
                builder.SetAuthenticationExtensions(ParseExtensions((JObject)data.Extensions));
            }
            return builder.Build();
        }

        private static List<PublicKeyCredentialDescriptor> ParseCredentialDescriptors(
            List<Fido2CredentialDescriptor> listData)
        {
            if (listData == null || listData.Count == 0)
            {
                return new List<PublicKeyCredentialDescriptor>();
            }

            var credentials = new List<PublicKeyCredentialDescriptor>();

            foreach (var data in listData)
            {
                string id = null;
                string type = null;
                var transports = new List<Transport>();

                if (!string.IsNullOrEmpty(data.Id))
                {
                    id = data.Id;
                }
                if (!string.IsNullOrEmpty(data.Type))
                {
                    type = data.Type;
                }
                if (data.Transports != null && data.Transports.Count > 0)
                {
                    foreach (var transport in data.Transports)
                    {
                        transports.Add(Transport.FromString(transport));
                    }
                }

                credentials.Add(new PublicKeyCredentialDescriptor(type, CoreHelpers.Base64UrlDecode(id), transports));
            }

            return credentials;
        }

        private static AuthenticationExtensions ParseExtensions(JObject extensions)
        {
            var builder = new AuthenticationExtensions.Builder();

            if (extensions.ContainsKey("appid"))
            {
                var appId = new FidoAppIdExtension((string)extensions.GetValue("appid"));
                builder.SetFido2Extension(appId);
            }

            if (extensions.ContainsKey("uvm"))
            {
                var uvm = new UserVerificationMethodExtension((bool)extensions.GetValue("uvm"));
                builder.SetUserVerificationMethodExtension(uvm);
            }

            return builder.Build();
        }
    }
}
#endif
