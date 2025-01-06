import XCTest
@testable import MessagePack

class MessagePackRoundTripTests: XCTestCase {
    var encoder: MessagePackEncoder!
    var decoder: MessagePackDecoder!
    
    override func setUp() {
        self.encoder = MessagePackEncoder()
        self.decoder = MessagePackDecoder()
    }

    func testRoundTripAirport() {
        let value = Airport.example
        let encoded = try! encoder.encode(value)
        let decoded = try! decoder.decode(Airport.self, from: encoded)

        XCTAssertEqual(value.name, decoded.name)
        XCTAssertEqual(value.iata, decoded.iata)
        XCTAssertEqual(value.icao, decoded.icao)
        XCTAssertEqual(value.coordinates[0], decoded.coordinates[0], accuracy: 0.01)
        XCTAssertEqual(value.coordinates[1], decoded.coordinates[1], accuracy: 0.01)
        XCTAssertEqual(value.runways[0].direction, decoded.runways[0].direction)
        XCTAssertEqual(value.runways[0].distance, decoded.runways[0].distance)
        XCTAssertEqual(value.runways[0].surface, decoded.runways[0].surface)
    }

    func testRoundTripParachutePack() {
        struct Parachute: Codable, Equatable {
            enum Canopy: String, Codable, Equatable {
                case round, cruciform, rogalloWing, annular, ramAir
            }

            let canpoy: Canopy
            let surfaceArea: Double
        }

        struct ParachutePack: Codable, Equatable {
            let main: Parachute?
            let reserve: Parachute?
        }

        let value = ParachutePack(main: Parachute(canpoy: .ramAir, surfaceArea: 200), reserve: nil)
        let encoded = try! encoder.encode(value)
        let decoded = try! decoder.decode(ParachutePack.self, from: encoded)

        XCTAssertEqual(value, decoded)
    }

    func testRoundTripArray() {
        let count: UInt8 = 100
        var bytes: [UInt8] = [0xdc, 0x00, count]
        var encoded: [Int] = []
        for n in 1...count {
            bytes.append(n)
            encoded.append(Int(n))
        }

        let data = Data(bytes: bytes)
        let decoded = try! decoder.decode([Int].self, from: data)
        XCTAssertEqual(encoded, decoded)
    }

    func testRoundTripDictionary() {
        let (a, z): (UInt8, UInt8) = (0x61, 0x7a)
        var bytes: [UInt8] = [0xde, 0x00, 0x1A]
        var encoded: [String: Int] = [:]
        for n in a...z {
            bytes.append(contentsOf: [0xA1, n, n])
            encoded[String(Unicode.Scalar(n))] = Int(n)
        }

        let data = Data(bytes: bytes)
        let decoded = try! decoder.decode([String: Int].self, from: data)
        XCTAssertEqual(encoded, decoded)
    }
    
    func testRoundTripDate() {
        var bytes: [UInt8] = [0xD6, 0xFF]

        let dateComponents = DateComponents(year: 2018, month: 4, day: 20)
        let encoded = Calendar.current.date(from: dateComponents)!
        
        let secondsSince1970 = UInt32(encoded.timeIntervalSince1970)
        bytes.append(contentsOf: secondsSince1970.bytes)
        
        let data = Data(bytes: bytes)
        let decoded = try! decoder.decode(Date.self, from: data)
        XCTAssertEqual(encoded, decoded)
    }

    func testRoundTripDateWithNanoseconds() {
        let encoded = Date()
        let data = try! self.encoder.encode(encoded)
        let decoded = try! self.decoder.decode(Date.self, from: data)
        XCTAssertEqual(encoded.timeIntervalSinceReferenceDate, decoded.timeIntervalSinceReferenceDate, accuracy: 0.0001)
    }
    
    static var allTests = [
        ("testRoundTripAirport", testRoundTripAirport),
        ("testRoundTripArray", testRoundTripArray),
        ("testRoundTripDictionary", testRoundTripDictionary),
        ("testRoundTripDate", testRoundTripDate),
        ("testRoundTripDateWithNanoseconds", testRoundTripDateWithNanoseconds)
    ]
}
