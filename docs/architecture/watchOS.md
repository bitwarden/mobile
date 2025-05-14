# watchOS

:::warning Legacy

This represents the **legacy** watchOS app architecture done in .NET MAUI.

:::

## Overall architecture

The watchOS application is organized as follows:

- `src/watchOS`: All the code specific to the watchOS platform
  - `bitwarden`: Stub iOS app so that the watchOS app has a companion app on Xcode
  - `bitwarden WatchKit App`: Main Watch app where we set assets.
  - `bitwarden WatchKit Extension`: All the logic and presentation logic for the Watch app is here

So almost all the things related to the watch app will be in the **WatchKit Extension**, the
WatchKit App one will be only for assets and some configs.

Then in the Extension we have a layered architecture:

- State (it's a really simplified version of the iOS state)
- Persistence (here we use `CoreData` to interact with the Database)
- Services (totp generation, crypto services and business logic)
- Presentation (use `SwiftUI` for the UI with an MVVM pattern)

## Integration with iOS

The watchOS app is developed using `Xcode` and `Swift` and we need to integrate it to the .NET MAUI
iOS application.

For this, the `iOS.csproj` has been adapted taking a
[solution](https://github.com/xamarin/xamarin-macios/issues/10070#issuecomment-1033428823) provided
in the `Xamarin.Forms` GitHub repository and modified to our needs:

```xml
<PropertyGroup>
    <WatchAppBuildPath Condition=" '$(Configuration)' == 'Debug' ">$(Home)/Library/Developer/Xcode/DerivedData/bitwarden-cbtqsueryycvflfzbsoteofskiyr/Build/Products</WatchAppBuildPath>
    <WatchAppBuildPath Condition=" '$(Configuration)' != 'Debug' ">$([System.IO.Path]::GetFullPath('$(MSBuildProjectDirectory)\..'))/watchOS/bitwarden.xcarchive/Products/Applications/bitwarden.app/Watch</WatchAppBuildPath>
    <WatchAppBundle>Bitwarden.app</WatchAppBundle>
    <WatchAppConfiguration Condition=" '$(Platform)' == 'iPhoneSimulator' ">watchsimulator</WatchAppConfiguration>
    <WatchAppConfiguration Condition=" '$(Platform)' == 'iPhone' ">watchos</WatchAppConfiguration>
    <WatchAppBundleFullPath Condition=" '$(Configuration)' == 'Debug' ">$(WatchAppBuildPath)/$(Configuration)-$(WatchAppConfiguration)/$(WatchAppBundle)</WatchAppBundleFullPath>
    <WatchAppBundleFullPath Condition=" '$(Configuration)' != 'Debug' ">$(WatchAppBuildPath)/$(WatchAppBundle)</WatchAppBundleFullPath>
</PropertyGroup>

...

<ItemGroup Condition=" '$(Configuration)' == 'Debug' AND Exists('$(WatchAppBundleFullPath)') ">
    <_ResolvedWatchAppReferences Include="$(WatchAppBundleFullPath)" />
</ItemGroup>
<ItemGroup Condition=" '$(Configuration)' != 'Debug' ">
    <_ResolvedWatchAppReferences Include="$(WatchAppBundleFullPath)" />
</ItemGroup>
<PropertyGroup Condition=" '$(_ResolvedWatchAppReferences)' != '' ">
    <CodesignExtraArgs>--deep</CodesignExtraArgs>
</PropertyGroup>
<Target Name="PrintWatchAppBundleStatus" BeforeTargets="Build">
    <Message Text="WatchAppBundleFullPath: '$(WatchAppBundleFullPath)' exists" Condition=" Exists('$(WatchAppBundleFullPath)') " />
    <Message Text="WatchAppBundleFullPath: '$(WatchAppBundleFullPath)' does NOT exist" Condition=" !Exists('$(WatchAppBundleFullPath)') " />
</Target>
```

So on the `PropertyGroup` the `WatchAppBundleFullPath` is assembled together depending on the
Configuration and the Platform taking the output of the Xcode watchOS app build. Then there are some
`ItemGroup` to include the watch app depending on if it exists and the Configuration. The task
`_ResolvedWatchAppReferences` is the one responsible to peek into the `Bitwarden.app` built by Xcode
and if it finds a Watch app, it will just bundle it to the Xamarin iOS application. Finally, if the
Watch app is bundled, deep signing is enabled and the build path is printed.

:::caution

As one can see in the csproj, to bundle the watchOS app into the iOS app one needs to target the
correct platform. So if one is going to use a device, target the device on Xcode to build the
watchOS app and after the build is done one can go to VS4M to build the iOS app (which will bundle
the watchOS one) and run it on the device.

:::

## Synchronization between iPhone and Watch

In order to sync data between the iPhone and the Watch apps the
[Watch Connectivity Framework](https://developer.apple.com/documentation/watchconnectivity) is used.

So there is a Watch Connectivity Manager on each side that is the interface used for the services on
each platform to communicate.

For the sync communication, mainly
[updateApplicationContext](https://developer.apple.com/documentation/watchconnectivity/wcsession/1615621-updateapplicationcontext)
is used given that it always have the latest data sent available, it's sent in the background and
the counterpart device doesn't necessarily needs to be in range (so it's cached until it can be
delivered). Additionally,
[sendMessage](https://developer.apple.com/documentation/watchconnectivity/wcsession/1615687-sendmessage)
is also used to signal the counterpart of some action to take quickly (like triggering a sync from
the Watch).

The `WatchDTO` is the object that is sent in the synchronization that has all the information for
the Watch.

```kroki type=plantuml
title= iOS part
@startuml

title iOS

participant C as "Caller"
participant BWDS as "BaseWatchDeviceService"
participant WDS as "WatchDeviceService"
participant WCSM as "WCSessionManager"
boundary WCF as "Watch Connectivity Framework"

group Sync
C->>BWDS: SyncDataToWatchAsync(...)
BWDS->BWDS: GetStateAsync(...)
BWDS->>WDS: SendDataToWatchAsync(...)
WDS->>WCSM: SendBackgroundHighPriorityMessage(...)
WCSM->>WCF: UpdateApplicationContext(...)
end
@enduml
```

```kroki type=plantuml
title= iOS part
@startuml

title watchOS

boundary WCF as "Watch Connectivity Framework"
participant WCM as "WatchConnectivityManager"
participant SS as "StateService"
participant ES as "EnvironmentService"
participant CS as "CipherService"
participant WCS as "watchConnectivitySubject"

group Sync
WCF->>WCM: didReceiveApplicationContext(...)
WCM->>SS: update state
WCM->>ES: update environment
WCM->>CS: saveCiphers(...)
WCM->>WCS: fire notification change to subscribers
end
@enduml
```

## States

The next ones are the states in which the Watch application can be at a given time:

- **Valid:** Everything it's ok and the user can see the vault ciphers with TOTP
- **Need Login:** The user needs to log in using the iPhone
- **Need Setup:** The user needs to set up an account with "Connect to Watch" enabled on their
  iPhone
- **Need Premium:** The current account is not a premium account
- **Need 2FA item:** The current account doesn't have any cipher with TOTP set up
- **Syncing:** Displayed when changing accounts and syncing the new vault TOTPs
- **Need Device Owner Auth:** The user needs to set up an Apple Watch Passcode in order to use the
  app

## Persistence and encryption

On the Watch [CoreData](https://developer.apple.com/documentation/coredata) is used as persistence
for the ciphers. So in order to encrypt the data in them a Value Transformer in each encrypted
attribute is used: `StringEncryptionTransformer`.

Inside the transformer a call to the `CryptoService` is used that ends up using
[AES.GCM](https://developer.apple.com/documentation/cryptokit/aes/gcm) to encrypt the data with a
256 bits [SymmetricKey](https://developer.apple.com/documentation/cryptokit/symmetrickey). The key
is generated/loaded the first time something needs to be encrypted and stored in the device
Keychain.

## Crash reporting

On all the other mobile applications, [AppCenter](https://appcenter.ms/) is being used as Crash
reporting tool. However, it doesn't have support for watchOS (nor its internal library to handle
crashes).

So, on the watchOS app [Firebase Crashlytics](https://firebase.google.com/docs/crashlytics) is used
with basic crash reporting enabled (there is no handled error logging here yet). For this to work a
`GoogleService-Info.plist` file is needed which is injected on the CI.

At the moment of writing this document, no plist is configured for dev environment so `Crashlytics`
is enabled on **non-DEBUG** configurations.

There is a `Log` class to log errors happened in the app, but it's only enabled in **DEBUG**
configuration.
