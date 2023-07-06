echo "Building framework"

xcodebuild -sdk iphonesimulator17.0 -project "iOS17CredentialProvider.xcodeproj" -configuration Release -arch x86_64
xcodebuild -sdk iphoneos17.0 -project "iOS17CredentialProvider.xcodeproj" -configuration Release
cd build
cp -R "Release-iphoneos" "Release-fat"
cp -R "Release-iphonesimulator/iOS17CredentialProvider.framework/Modules/iOS17CredentialProvider.swiftmodule/" "Release-fat/iOS17CredentialProvider.framework/Modules/iOS17CredentialProvider.swiftmodule/"
lipo -create -output "Release-fat/iOS17CredentialProvider.framework/iOS17CredentialProvider" "Release-iphoneos/iOS17CredentialProvider.framework/iOS17CredentialProvider" "Release-iphonesimulator/iOS17CredentialProvider.framework/iOS17CredentialProvider"

echo "Sharpie creating binding definitions"

sharpie bind --sdk=iphoneos17.0 --output="XamarinApiDef" --namespace="Binding" --scope="Release-iphoneos/iOS17CredentialProvider.framework/Headers/" "Release-iphoneos/iOS17CredentialProvider.framework/Headers/iOS17CredentialProvider-Swift.h"

echo "Done!"