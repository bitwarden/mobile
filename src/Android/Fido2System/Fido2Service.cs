#if !FDROID
using Android.App;
using Android.Content;
using Android.Gms.Fido;
using Android.Gms.Fido.Fido2;
using Android.Gms.Fido.Fido2.Api.Common;
using Android.Gms.Tasks;
using Android.Util;
using AndroidX.AppCompat.App;
using Bit.App.Services;
using Bit.Core.Abstractions;
using Bit.Core.Enums;
using Bit.Core.Models.Data;
using Bit.Core.Models.Request;
using Bit.Core.Models.Response;
using Bit.Core.Utilities;
using Java.Lang;
using Newtonsoft.Json;
using Xamarin.Forms;
using Enum = System.Enum;

namespace Bit.Droid.Fido2System
{
    public class Fido2Service
    {
        public static readonly string _tag_log = "Fido2Service";

        public static Fido2Service INSTANCE = new Fido2Service();

        private readonly MobileI18nService _i18nService;
        private readonly IPlatformUtilsService _platformUtilsService;

        private AppCompatActivity _activity;
        private Fido2ApiClient _fido2ApiClient;
        private Fido2CodesTypes _fido2CodesType;

        public Fido2Service()
        {
            _i18nService = ServiceContainer.Resolve<II18nService>("i18nService") as MobileI18nService;
            _platformUtilsService = ServiceContainer.Resolve<IPlatformUtilsService>("platformUtilsService");
        }

        public void Start(AppCompatActivity activity)
        {
            _activity = activity;
            _fido2ApiClient = Fido.GetFido2ApiClient(_activity);
        }

        public void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if (resultCode == Result.Ok && Enum.IsDefined(typeof(Fido2CodesTypes), requestCode))
            {
                switch ((Fido2CodesTypes)requestCode)
                {
                    case Fido2CodesTypes.RequestSignInUser:
                        var errorExtra = data?.GetByteArrayExtra(Fido.Fido2KeyErrorExtra);
                        if (errorExtra != null)
                        {
                            HandleErrorCode(errorExtra);
                        }
                        else
                        {
                            if (data != null)
                            {
                                SignInUserResponse(data);
                            }
                        }
                        break;
                    // TODO: Key registration, should we ever choose to implement client-side
                    /*case Fido2CodesTypes.RequestRegisterNewKey:
                        errorExtra = data?.GetByteArrayExtra(Fido.Fido2KeyErrorExtra);
                        if (errorExtra != null)
                        {
                            HandleErrorCode(errorExtra);
                        }
                        else
                        {
                            if (data != null)
                            {
                                // begin registration flow
                            }
                        }
                        break;*/
                }
            }
            else if (resultCode == Result.Canceled && Enum.IsDefined(typeof(Fido2CodesTypes), requestCode))
            {
                Log.Info(_tag_log, "cancelled");
                _platformUtilsService.ShowDialogAsync(_i18nService.T("Fido2AbortError"),
                    _i18nService.T("Fido2Title"));
            }
        }

        public void OnSuccess(Object result)
        {
            if (result != null && Enum.IsDefined(typeof(Fido2CodesTypes), _fido2CodesType))
            {
                try
                {
                    _activity.StartIntentSenderForResult(((PendingIntent)result).IntentSender, (int)_fido2CodesType,
                        null, 0, 0, 0);
                }
                catch (System.Exception e)
                {
                    Log.Error(_tag_log, e.Message);
                    _platformUtilsService.ShowDialogAsync(_i18nService.T("Fido2SomethingWentWrong"),
                        _i18nService.T("Fido2Title"));
                }
            }
        }

        public void OnFailure(Exception e)
        {
            Log.Error(_tag_log, e.Message ?? "OnFailure: No error message returned");
            _platformUtilsService.ShowDialogAsync(_i18nService.T("Fido2SomethingWentWrong"),
                _i18nService.T("Fido2Title"));
        }

        public void OnComplete(Task task)
        {
            Log.Debug(_tag_log, "OnComplete");
        }

        public async System.Threading.Tasks.Task SignInUserRequestAsync(string dataJson)
        {
            try
            {
                var dataObject = JsonConvert.DeserializeObject<Fido2AuthenticationChallengeResponse>(dataJson);
                _fido2CodesType = Fido2CodesTypes.RequestSignInUser;
                var options = Fido2BuilderObject.ParsePublicKeyCredentialRequestOptions(dataObject);
                var task = _fido2ApiClient.GetSignPendingIntent(options);
                task.AddOnSuccessListener((IOnSuccessListener)_activity)
                    .AddOnFailureListener((IOnFailureListener)_activity)
                    .AddOnCompleteListener((IOnCompleteListener)_activity);
            }
            catch (System.Exception e)
            {
                Log.Error(_tag_log, e.StackTrace);
                await _platformUtilsService.ShowDialogAsync(_i18nService.T("Fido2SomethingWentWrong"),
                    _i18nService.T("Fido2Title"));
            }
            finally
            {
                Log.Info(_tag_log, "SignInUserRequest() -> finally()");
            }
        }

        private void SignInUserResponse(Intent data)
        {
            try
            {
                var response =
                    AuthenticatorAssertionResponse.DeserializeFromBytes(
                        data.GetByteArrayExtra(Fido.Fido2KeyResponseExtra));
                var responseJson = JsonConvert.SerializeObject(new Fido2AuthenticationChallengeRequest
                    {
                        Id = CoreHelpers.Base64UrlEncode(response.GetKeyHandle()),
                        RawId = CoreHelpers.Base64UrlEncode(response.GetKeyHandle()),
                        Type = "public-key",
                        Response = new Fido2AssertionResponse
                        {
                            AuthenticatorData = CoreHelpers.Base64UrlEncode(response.GetAuthenticatorData()),
                            ClientDataJson = CoreHelpers.Base64UrlEncode(response.GetClientDataJSON()),
                            Signature = CoreHelpers.Base64UrlEncode(response.GetSignature()),
                            UserHandle = (response.GetUserHandle() != null
                                ? CoreHelpers.Base64UrlEncode(response.GetUserHandle()) : null),
                        },
                        Extensions = null
                    }
                );
                Device.BeginInvokeOnMainThread(() => ((MainActivity)_activity).Fido2Submission(responseJson));
            }
            catch (System.Exception e)
            {
                Log.Error(_tag_log, e.Message);
                _platformUtilsService.ShowDialogAsync(_i18nService.T("Fido2SomethingWentWrong"),
                    _i18nService.T("Fido2Title"));
            }
            finally
            {
                Log.Info(_tag_log, "SignInUserResponse() -> finally()");
            }
        }

        public void HandleErrorCode(byte[] errorExtra)
        {
            var error = AuthenticatorErrorResponse.DeserializeFromBytes(errorExtra);
            if (error.ErrorMessage.Length > 0)
            {
                Log.Info(_tag_log, error.ErrorMessage);
            }
            string message = "";
            if (error.ErrorCode == ErrorCode.AbortErr)
            {
                message = "Fido2AbortError";
            }
            else if (error.ErrorCode == ErrorCode.TimeoutErr)
            {
                message = "Fido2TimeoutError";
            }
            else if (error.ErrorCode == ErrorCode.AttestationNotPrivateErr)
            {
                message = "Fido2PrivacyError";
            }
            else if (error.ErrorCode == ErrorCode.ConstraintErr)
            {
                message = "Fido2SomethingWentWrong";
            }
            else if (error.ErrorCode == ErrorCode.DataErr)
            {
                message = "Fido2ServerDataFail";
            }
            else if (error.ErrorCode == ErrorCode.EncodingErr)
            {
                message = "Fido2SomethingWentWrong";
            }
            else if (error.ErrorCode == ErrorCode.InvalidStateErr)
            {
                message = "Fido2SomethingWentWrong";
            }
            else if (error.ErrorCode == ErrorCode.NetworkErr)
            {
                message = "Fido2NetworkFail";
            }
            else if (error.ErrorCode == ErrorCode.NotAllowedErr)
            {
                message = "Fido2NoPermission";
            }
            else if (error.ErrorCode == ErrorCode.NotSupportedErr)
            {
                message = "Fido2NotSupportedError";
            }
            else if (error.ErrorCode == ErrorCode.SecurityErr)
            {
                message = "Fido2SecurityError";
            }
            else if (error.ErrorCode == ErrorCode.UnknownErr)
            {
                message = "Fido2SomethingWentWrong";
            }
            else
            {
                message = "Fido2SomethingWentWrong";
            }
            _platformUtilsService.ShowDialogAsync(_i18nService.T(message), _i18nService.T("Fido2Title"));
        }
    }
}
#endif
