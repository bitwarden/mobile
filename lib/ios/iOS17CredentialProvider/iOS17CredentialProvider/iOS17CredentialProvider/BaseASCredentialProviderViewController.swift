//
//  BaseASCredentialProviderViewController.swift
//  iOS17CredentialProvider
//
//  Created by Federico Maccaroni on 03/07/2023.
//

import Foundation
import AuthenticationServices

@objc(ASCredentialRequestCompatType)
public enum ASCredentialRequestCompatType : Int, @unchecked Sendable {
    case password = 0

    case passkeyAssertion = 1
}

@objc(ASPasskeyCredentialIdentityCompat)
public class ASPasskeyCredentialIdentityCompat : NSObject {
    
    init(relyingPartyIdentifier: String, userName: String, credentialID: Data, userHandle: Data, recordIdentifier: String? = nil, rank: Int) {
        self.relyingPartyIdentifier = relyingPartyIdentifier
        self.userName = userName
        self.credentialID = credentialID
        self.userHandle = userHandle
        self.recordIdentifier = recordIdentifier
        self.rank = rank
    }
    
    /** The relying party identifier of this passkey credential.
     @discussion This field is reported as the serviceIdentifier property of ASCredentialIdentity.
     */
    @objc
    public var relyingPartyIdentifier: String

    
    /** The user name of this passkey credential.
     @discussion This field is reported as the user property of ASCredentialIdentity.
     */
    @objc
    public var userName: String

    
    /** The credential ID of this passkey credential.
     @discussion This field is used to identify the correct credential to use based on relying party request parameters.
     */
    @objc
    public var credentialID: Data

    
    /** The user handle of this passkey credential.
     @discussion This field is used to identify the correct credential to use based on relying party request parameters.
     */
    @objc
    public var userHandle: Data

    
    /** Get the record identifier.
     @result The record identifier.
     @discussion You can utilize the record identifier to uniquely identify the credential identity in your local database.
     */
    @objc
    public var recordIdentifier: String?

    
    /** Get or set the rank of the credential identity object.
     @discussion The system may utilize the rank to decide which credential identity precedes the other
     if two identities have the same service identifier. A credential identity with a larger rank value
     precedes one with a smaller value if both credential identities have the same service identifier.
     The default value of this property is 0.
     */
    @objc
    public var rank: Int
}

@objc(ASCredentialRequestCompat)
public class ASCredentialRequestCompat : NSObject {

    /** The type of credential used for this request.
     */
    @objc
    public var type: ASCredentialRequestCompatType

    
    /** The credential identity selected by the user to authenticate.
     */
    @objc
    public var credentialIdentity: ASCredentialIdentity
    
    @objc
    public var passwordCredentialIdentity: ASPasswordCredentialIdentity? {
        guard type == .password, let password = credentialIdentity as? ASPasswordCredentialIdentity else {
            return nil
        }
        return password
    }
    
    @objc
    public var passkeyCredentialIdentity: ASPasskeyCredentialIdentityCompat? {
        guard type == .passkeyAssertion, let passkey = credentialIdentity as? ASPasskeyCredentialIdentity else {
            return nil
        }
        return ASPasskeyCredentialIdentityCompat(relyingPartyIdentifier: passkey.relyingPartyIdentifier, userName: passkey.userName, credentialID: passkey.credentialID, userHandle: passkey.userHandle, recordIdentifier: passkey.recordIdentifier, rank: passkey.rank)
    }
    
    init(type: ASCredentialRequestCompatType, credentialIdentity: ASCredentialIdentity) {
        self.type = type
        self.credentialIdentity = credentialIdentity
    }
}

@objc(BaseASCredentialProviderViewController)
open class BaseASCredentialProviderViewController : ASCredentialProviderViewController
{
    // MARK: iOS 17 new methods
    
    override final public func prepareInterfaceToProvideCredential(for credentialRequest: ASCredentialRequest) {
        guard let compatDelegate = compatDelegate else {
            return
        }
        compatDelegate.prepareInterfaceToProvideCredentialCompat(for: convertRequestToCompat(from: credentialRequest))
    }
    
    override final public func provideCredentialWithoutUserInteraction(for credentialRequest: ASCredentialRequest) {
        guard let compatDelegate = compatDelegate else {
            return
        }
        compatDelegate.provideCredentialWithoutUserInteractionCompat(for: convertRequestToCompat(from: credentialRequest))
    }
    
    override final public func prepareInterface(forPasskeyRegistration registrationRequest: ASCredentialRequest) {
        guard let compatDelegate = compatDelegate else {
            return
        }
        compatDelegate.prepareInterfaceCompat(forPasskeyRegistration: convertRequestToCompat(from: registrationRequest))
    }
    
    // MARK: Compat
    
    var compatDelegate: ASCredentialProviderCompatDelegate? = nil;
    
    @objc
    public func SetCompatDelegate(_ delegate: ASCredentialProviderCompatDelegate)
    {
        compatDelegate = delegate
    }
    
    func convertRequestToCompat(from credentialRequest: ASCredentialRequest) -> ASCredentialRequestCompat {
        return ASCredentialRequestCompat(
            type: credentialRequest.type == .password ? .password : .passkeyAssertion,
            credentialIdentity: credentialRequest.credentialIdentity)
    }
}

@objc(ASCredentialProviderCompatDelegate)
public protocol ASCredentialProviderCompatDelegate
{
    @objc
    func prepareInterfaceToProvideCredentialCompat(for credentialRequest: ASCredentialRequestCompat)
    
    @objc
    func provideCredentialWithoutUserInteractionCompat(for credentialRequest: ASCredentialRequestCompat)
    
    @objc
    func prepareInterfaceCompat(forPasskeyRegistration registrationRequest: ASCredentialRequestCompat)
}
