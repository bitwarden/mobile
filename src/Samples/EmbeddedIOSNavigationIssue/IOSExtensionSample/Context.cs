using System;
using AuthenticationServices;
using Foundation;

namespace Bit.iOS.Autofill
{
    public class Context
    {
        public NSExtensionContext ExtContext { get; set; }
        public ASCredentialServiceIdentifier[] ServiceIdentifiers { get; set; }
        public ASPasswordCredentialIdentity CredentialIdentity { get; set; }
        public bool Configuring { get; set; }

        // ---

        private const string iOSProtocol = "iosapp://";

        private string _uriString;

        public Uri Uri
        {
            get
            {
                if (string.IsNullOrWhiteSpace(UrlString) || !Uri.TryCreate(UrlString, UriKind.Absolute, out Uri uri))
                {
                    return null;
                }
                return uri;
            }
        }

        public string UrlString
        {
            get
            {
                return _uriString;
            }
            set
            {
                _uriString = value;
                if (string.IsNullOrWhiteSpace(_uriString))
                {
                    return;
                }
                if (!_uriString.StartsWith(iOSProtocol) && _uriString.Contains("."))
                {
                    if (!_uriString.Contains("://") && !_uriString.Contains(" "))
                    {
                        _uriString = string.Concat("http://", _uriString);
                    }
                }
                if (!_uriString.StartsWith("http") && !_uriString.StartsWith(iOSProtocol))
                {
                    _uriString = string.Concat(iOSProtocol, _uriString);
                }
            }
        }
    }
}
