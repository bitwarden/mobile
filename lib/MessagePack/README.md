# MessagePack

[![Build Status][build status badge]][build status]

A [MessagePack](https://msgpack.org/) encoder and decoder for `Codable` types.

This functionality is discussed in Chapter 7 of
[Flight School Guide to Swift Codable](https://flight.school/books/codable).

## Requirements

- Swift 4.2+

## Usage

### Encoding Messages

```swift
import MessagePack

let encoder = MessagePackEncoder()
let value = try! encoder.encode(["a": 1, "b": 2, "c": 3])
// [0x83, 0xA1, 0x62, 0x02, 0xA1, 0x61, 0x01, 0xA1, 0x63, 0x03]
```

### Decoding Messages

```swift
import MessagePack

let decoder = MessagePackDecoder()
let data = Data(bytes: [0xCB, 0x40, 0x09, 0x21, 0xF9, 0xF0, 0x1B, 0x86, 0x6E])
let value = try! decoder.decode(Double.self, from: data)
// 3.14159
```

## Installation

### Swift Package Manager

Add the MessagePack package to your target dependencies in `Package.swift`:

```swift
import PackageDescription

let package = Package(
  name: "YourProject",
  dependencies: [
    .package(
        url: "https://github.com/Flight-School/MessagePack",
        from: "1.2.3"
    ),
  ]
)
```

Then run the `swift build` command to build your project.

### CocoaPods

You can install `MessagePack` via CocoaPods,
by adding the following line to your `Podfile`:

```ruby
pod 'MessagePack-FlightSchool', '~> 1.2.4'
```

Run the `pod install` command to download the library
and integrate it into your Xcode project.

> **Note**
> The module name for this library is "MessagePack" ---
> that is, to use it, you add `import MessagePack` to the top of your Swift code
> just as you would by any other installation method.
> The pod is called "MessagePack-FlightSchool"
> because there's an existing pod with the name "MessagePack".

## License

MIT

## Contact

Mattt ([@mattt](https://twitter.com/mattt))

[build status]: https://github.com/Flight-School/MessagePack/actions?query=workflow%3ACI
[build status badge]: https://github.com/Flight-School/MessagePack/workflows/CI/badge.svg
