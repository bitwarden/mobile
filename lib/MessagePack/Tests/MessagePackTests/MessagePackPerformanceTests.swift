import XCTest
@testable import MessagePack

class MessagePackPerformanceTests: XCTestCase {
    var encoder: MessagePackEncoder!
    var decoder: MessagePackDecoder!
    
    override func setUp() {
        self.encoder = MessagePackEncoder()
        self.decoder = MessagePackDecoder()
    }
    
    func testPerformance() {
        let count = 100
        let values = [Airport](repeating: .example, count: count)
        
        self.measure {
            let encoded = try! encoder.encode(values)
            let decoded = try! decoder.decode([Airport].self, from: encoded)
            XCTAssertEqual(decoded.count, count)
        }
    }
}
