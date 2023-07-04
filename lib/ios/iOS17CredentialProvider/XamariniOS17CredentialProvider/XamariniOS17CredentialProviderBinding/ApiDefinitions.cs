using System;
using AuthenticationServices;
using Foundation;
using ObjCRuntime;

namespace XamariniOS17CredentialProviderBinding
{
	// @interface ASCredentialRequestCompat : NSObject
	[BaseType (typeof(NSObject))]
	[DisableDefaultCtor]
	interface ASCredentialRequestCompat
    {
		// @property (nonatomic) enum ASCredentialRequestCompatType type;
        [Export("type", ArgumentSemantic.Assign)]
        ASCredentialRequestCompatType Type { get; set; }

        // @property (nonatomic, strong) id<ASCredentialIdentity> _Nonnull credentialIdentity;
        //[Export("credentialIdentity", ArgumentSemantic.Strong)]
        //ASCredentialIdentity CredentialIdentity { get; set; }


        // @property (readonly, nonatomic, strong) ASPasswordCredentialIdentity * _Nullable passwordCredentialIdentity;
        [NullAllowed, Export("passwordCredentialIdentity", ArgumentSemantic.Strong)]
        ASPasswordCredentialIdentity PasswordCredentialIdentity { get; }

        // @property (readonly, nonatomic, strong) ASPasskeyCredentialIdentityCompat * _Nullable passkeyCredentialIdentity;
        [NullAllowed, Export("passkeyCredentialIdentity", ArgumentSemantic.Strong)]
        ASPasskeyCredentialIdentityCompat PasskeyCredentialIdentity { get; }
    }

    // @interface ASPasskeyCredentialIdentityCompat : NSObject
    [BaseType(typeof(NSObject))]
    [DisableDefaultCtor]
    interface ASPasskeyCredentialIdentityCompat
    {
        // @property (copy, nonatomic) NSString * _Nonnull relyingPartyIdentifier;
        [Export("relyingPartyIdentifier")]
        string RelyingPartyIdentifier { get; set; }

        // @property (copy, nonatomic) NSString * _Nonnull userName;
        [Export("userName")]
        string UserName { get; set; }

        // @property (copy, nonatomic) NSData * _Nonnull credentialID;
        [Export("credentialID", ArgumentSemantic.Copy)]
        NSData CredentialID { get; set; }

        // @property (copy, nonatomic) NSData * _Nonnull userHandle;
        [Export("userHandle", ArgumentSemantic.Copy)]
        NSData UserHandle { get; set; }

        // @property (copy, nonatomic) NSString * _Nullable recordIdentifier;
        [NullAllowed, Export("recordIdentifier")]
        string RecordIdentifier { get; set; }

        // @property (nonatomic) NSInteger rank;
        [Export("rank")]
        nint Rank { get; set; }
    }

    // @interface BaseASCredentialProviderViewController : ASCredentialProviderViewController
    [BaseType (typeof(ASCredentialProviderViewController))]
	interface BaseASCredentialProviderViewController
	{
		// -(void)prepareInterfaceToProvideCredentialForRequest:(id<ASCredentialRequest> _Nonnull)credentialRequest;
		//[Export ("prepareInterfaceToProvideCredentialForRequest:")]
		//void PrepareInterfaceToProvideCredentialForRequest (ASCredentialRequest credentialRequest);

		//// -(void)provideCredentialWithoutUserInteractionForRequest:(id<ASCredentialRequest> _Nonnull)credentialRequest;
		//[Export ("provideCredentialWithoutUserInteractionForRequest:")]
		//void ProvideCredentialWithoutUserInteractionForRequest (ASCredentialRequest credentialRequest);

		//// -(void)prepareInterfaceForPasskeyRegistration:(id<ASCredentialRequest> _Nonnull)registrationRequest;
		//[Export ("prepareInterfaceForPasskeyRegistration:")]
		//void PrepareInterfaceForPasskeyRegistration (ASCredentialRequest registrationRequest);

		// -(void)prepareInterfaceToProvideCredentialCompatFor:(ASCredentialRequestCompat * _Nonnull)credentialRequest;
		[Export ("prepareInterfaceToProvideCredentialCompatFor:")]
        [Abstract]
        void PrepareInterfaceToProvideCredentialCompatFor (ASCredentialRequestCompat credentialRequest);

		// -(void)provideCredentialWithoutUserInteractionCompatFor:(ASCredentialRequestCompat * _Nonnull)credentialRequest;
		[Export ("provideCredentialWithoutUserInteractionCompatFor:")]
        [Abstract]
        void ProvideCredentialWithoutUserInteractionCompatFor (ASCredentialRequestCompat credentialRequest);

		// -(void)prepareInterfaceCompatForPasskeyRegistration:(ASCredentialRequestCompat * _Nonnull)registrationRequest;
		[Export ("prepareInterfaceCompatForPasskeyRegistration:")]
        [Abstract]
        void PrepareInterfaceCompatForPasskeyRegistration (ASCredentialRequestCompat registrationRequest);

		// -(instancetype _Nonnull)initWithNibName:(NSString * _Nullable)nibNameOrNil bundle:(NSBundle * _Nullable)nibBundleOrNil __attribute__((objc_designated_initializer));
		[Export ("initWithNibName:bundle:")]
		[DesignatedInitializer]
        IntPtr Constructor ([NullAllowed] string nibNameOrNil, [NullAllowed] NSBundle nibBundleOrNil);

		//// -(instancetype _Nullable)initWithCoder:(NSCoder * _Nonnull)coder __attribute__((objc_designated_initializer));
		//[Export ("initWithCoder:")]
		//[DesignatedInitializer]
  //      IntPtr Constructor (NSCoder coder);
	}
}
