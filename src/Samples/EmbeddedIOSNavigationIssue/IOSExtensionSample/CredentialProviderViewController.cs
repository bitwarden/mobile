using System;
using System.Threading.Tasks;
using AuthenticationServices;
using Bit.Core;
using Core;
using CoreFoundation;
using Foundation;
using IOSExtensionSample;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Embedding;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using UIKit;

namespace Bit.iOS.Autofill
{
    public partial class CredentialProviderViewController : ASCredentialProviderViewController
    {
        private Context _context;
        private bool _isInit;

        public CredentialProviderViewController(IntPtr handle)
            : base(handle)
        {
            ModalPresentationStyle = UIModalPresentationStyle.FullScreen;
        }

        private ASCredentialProviderExtensionContext ASExtensionContext => _context?.ExtContext as ASCredentialProviderExtensionContext;

        public override void ViewDidLoad()
        {
            try
            {
                InitAppIfNeeded();

                base.ViewDidLoad();

                _context = new Context
                {
                    ExtContext = ExtensionContext
                };
            }
            catch (Exception ex)
            {
                ClipLogger.Log(ex);
                throw;
            }
        }

        public override async void PrepareCredentialList(ASCredentialServiceIdentifier[] serviceIdentifiers)
        {
            try
            {
                InitAppIfNeeded();
                _context.ServiceIdentifiers = serviceIdentifiers;
                if (serviceIdentifiers.Length > 0)
                {
                    var uri = serviceIdentifiers[0].Identifier;
                    if (serviceIdentifiers[0].Type == ASCredentialServiceIdentifierType.Domain)
                    {
                        uri = string.Concat("https://", uri);
                    }
                    _context.UrlString = uri;
                }
                if (!await IsAuthed())
                {
                    LaunchHomePage();
                }
                else if (await IsLocked())
                {
                    PerformSegue("lockPasswordSegue", this);
                }
                else
                {
                    PerformSegue("loginListSegue", this);
                }
            }
            catch (Exception ex)
            {
                ClipLogger.Log(ex);
                throw;
            }
        }

        public override async void ProvideCredentialWithoutUserInteraction(ASPasswordCredentialIdentity credentialIdentity)
        {
            try
            {
                InitAppIfNeeded();
                if (!await IsAuthed() || await IsLocked())
                {
                    var err = new NSError(new NSString("ASExtensionErrorDomain"),
                        Convert.ToInt32(ASExtensionErrorCode.UserInteractionRequired), null);
                    ExtensionContext.CancelRequest(err);
                    return;
                }
                _context.CredentialIdentity = credentialIdentity;
                await ProvideCredentialAsync(false);
            }
            catch (Exception ex)
            {
                ClipLogger.Log(ex);
                throw;
            }
        }

        public override async void PrepareInterfaceToProvideCredential(ASPasswordCredentialIdentity credentialIdentity)
        {
            try
            {
                InitAppIfNeeded();
                if (!await IsAuthed())
                {
                    LaunchHomePage();
                    return;
                }
                _context.CredentialIdentity = credentialIdentity;
                await CheckLockAsync(async () => await ProvideCredentialAsync());
            }
            catch (Exception ex)
            {
                ClipLogger.Log(ex);
                throw;
            }
        }

        public override async void PrepareInterfaceForExtensionConfiguration()
        {
            try
            {
                InitAppIfNeeded();
                _context.Configuring = true;
                if (!await IsAuthed())
                {
                    LaunchHomePage();
                    return;
                }
                await CheckLockAsync(() => PerformSegue("setupSegue", this));
            }
            catch (Exception ex)
            {
                ClipLogger.Log(ex);
                throw;
            }
        }

        public void CompleteRequest(string id = null, string username = null,
            string password = null, string totp = null)
        {
            if ((_context?.Configuring ?? true) && string.IsNullOrWhiteSpace(password))
            {
                ServiceContainer.Reset();
                ASExtensionContext?.CompleteExtensionConfigurationRequest();
                return;
            }

            if (_context == null || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ServiceContainer.Reset();
                var err = new NSError(new NSString("ASExtensionErrorDomain"),
                    Convert.ToInt32(ASExtensionErrorCode.UserCanceled), null);
                NSRunLoop.Main.BeginInvokeOnMainThread(() => ASExtensionContext?.CancelRequest(err));
                return;
            }

            var cred = new ASPasswordCredential(username, password);
            NSRunLoop.Main.BeginInvokeOnMainThread(() =>
            {
                ServiceContainer.Reset();
                ASExtensionContext?.CompleteRequest(cred, null);
            });
        }

        public override void PrepareForSegue(UIStoryboardSegue segue, NSObject sender)
        {
            try
            {
                if (segue.DestinationViewController is UINavigationController navController)
                {
                    if (navController.TopViewController is LoginListViewController listLoginController)
                    {
                        listLoginController.CPViewController = this;
                        segue.DestinationViewController.PresentationController.Delegate =
                            new CustomPresentationControllerDelegate(listLoginController.DismissModalAction);
                    }
                    else if (navController.TopViewController is LockPasswordViewController passwordViewController)
                    {
                        passwordViewController.CPViewController = this;
                        passwordViewController.LaunchHomePage = () => DismissViewController(false, () => LaunchHomePage());
                        segue.DestinationViewController.PresentationController.Delegate =
                            new CustomPresentationControllerDelegate(passwordViewController.DismissModalAction);
                    }
                    else if (navController.TopViewController is SetupViewController setupViewController)
                    {
                        setupViewController.CPViewController = this;
                        segue.DestinationViewController.PresentationController.Delegate =
                            new CustomPresentationControllerDelegate(setupViewController.DismissModalAction);
                    }
                }

            }
            catch (Exception ex)
            {
                ClipLogger.Log(ex);
                throw;
            }
        }

        public async Task OnLockDismissedAsync()
        {
            try
            {
                if (_context.CredentialIdentity != null)
                {
                    await MainThread.InvokeOnMainThreadAsync(() => ProvideCredentialAsync());
                    return;
                }
                if (_context.Configuring)
                {
                    await MainThread.InvokeOnMainThreadAsync(() => PerformSegue("setupSegue", this));
                    return;
                }

                await MainThread.InvokeOnMainThreadAsync(() => PerformSegue("loginListSegue", this));
            }
            catch (Exception ex)
            {
                ClipLogger.Log(ex);
                throw;
            }
        }

        private async Task ProvideCredentialAsync(bool userInteraction = true)
        {
            try
            {
                await Task.Delay(200);

                CompleteRequest("asdfa", "myUser", "myPass", "someTotp");
            }
            catch (Exception ex)
            {
                ClipLogger.Log(ex);
                throw;
            }
        }

        private async Task CheckLockAsync(Action notLockedAction)
        {
            if (await IsLocked())
            {
                DispatchQueue.MainQueue.DispatchAsync(() => PerformSegue("lockPasswordSegue", this));
            }
            else
            {
                notLockedAction();
            }
        }

        private Task<bool> IsLocked()
        {
            return Task.FromResult(State.IsLocked);
        }

        private Task<bool> IsAuthed()
        {
            return Task.FromResult(State.IsAuthed);
        }

        private void InitAppIfNeeded()
        {
            if (_isInit)
                return;

            var builder = Bit.Core.MauiProgram.ConfigureMauiAppBuilder(false, handlers =>
            {
                // WORKAROUND: This is needed to make TapGestureRecognizer work on extensions.
                handlers.AddHandler(typeof(Window), typeof(CustomWindowHandler));
            })
                .UseMauiEmbedding<Microsoft.Maui.Controls.Application>();
            // Register the Window
            builder.Services.Add(new ServiceDescriptor(typeof(UIWindow), _ => UIApplication.SharedApplication.KeyWindow, ServiceLifetime.Singleton));
            var mauiApp = builder.Build();

            MauiContextSingleton.Instance.Init(new MauiContext(mauiApp.Services));

            _isInit = true;
        }

        private void LaunchHomePage()
        {
            var appOptions = new AppOptions { IosExtension = true };
            var homePage = new HomePage(appOptions);
            var app = new App(appOptions);

            homePage.StartLoginAction = () => DismissViewController(false, () => LaunchLoginFlow());
            homePage.CloseAction = () => CompleteRequest();

            NavigateToPage(homePage);
        }

        private void LaunchLoginFlow()
        {
            var appOptions = new AppOptions { IosExtension = true };
            var app = new App(appOptions);
            var loginPage = new LoginPage(appOptions);

            loginPage.LogInSuccessAction = () => DismissLockAndContinue();
            loginPage.CloseAction = () => DismissViewController(false, () => LaunchHomePage());

            NavigateToPage(loginPage);
        }

#if !ENABLED_TAP_GESTURE_RECOGNIZER_MAUI_EMBEDDED_WORKAROUND
        public async void DismissLockAndContinue()
        {
            DismissViewController(false, async () => await OnLockDismissedAsync());
        }

        private void NavigateToPage(ContentPage page)
        {
            var navigationPage = new NavigationPage(page);
            var uiController = navigationPage.ToUIViewController(MauiContextSingleton.Instance.MauiContext);
            uiController.ModalPresentationStyle = UIModalPresentationStyle.FullScreen;

            PresentViewController(uiController, true, null);
        }
#else
        const string STORYBOARD_NAME = "MainInterface";
        Lazy<UIStoryboard> _storyboard = new Lazy<UIStoryboard>(() => UIStoryboard.FromName(STORYBOARD_NAME, null));

        public void InitWithContext(Context context)
        {
            _context = context;
        }

        public void DismissLockAndContinue()
        {
            if (UIApplication.SharedApplication.KeyWindow is null)
            {
                return;
            }

            UIApplication.SharedApplication.KeyWindow.RootViewController = _storyboard.Value.InstantiateInitialViewController();

            if (UIApplication.SharedApplication.KeyWindow?.RootViewController is CredentialProviderViewController cpvc)
            {
                cpvc.InitWithContext(_context);
                cpvc.OnLockDismissedAsync().FireAndForget();
            }
        }

        private void NavigateToPage(ContentPage page)
        {
            var navigationPage = new NavigationPage(page);

            var window = new Window(navigationPage);
            window.ToHandler(MauiContextSingleton.Instance.MauiContext);
        }
#endif
    }
}
