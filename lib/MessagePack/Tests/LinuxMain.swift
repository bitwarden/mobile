import XCTest
@testable import MessagePackTests

XCTMain([
    testCase(MessagePackDecodingTests.allTests),
    testCase(MessagePackEncodingTests.allTests),
    testCase(MessagePackRoundTripTests.allTests),
])
