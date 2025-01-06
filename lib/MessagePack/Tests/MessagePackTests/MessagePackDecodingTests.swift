import XCTest
@testable import MessagePack

class MessagePackDecodingTests: XCTestCase {
    var decoder: MessagePackDecoder!
    
    override func setUp() {
        self.decoder = MessagePackDecoder()
    }

    func assertTypeMismatch<T>(_ expression: @autoclosure () throws -> T,
                               _ message: @autoclosure () -> String = "",
                               file: StaticString = #file,
                               line: UInt = #line) -> Any.Type? {
        var error: Error?
        XCTAssertThrowsError(expression, message,
                             file: file, line: line) {
            error = $0
        }
        guard case .typeMismatch(let type, _) = error as? DecodingError else {
            XCTFail(file: file, line: line)
            return nil
        }
        return type
    }
    
    func testDecodeNil() {
        let data = Data(bytes: [0xC0])
        let value = try! decoder.decode(Int?.self, from: data)
        XCTAssertNil(value)
    }

    func testDecodeFalse() {
        let data = Data(bytes: [0xc2])
        let value = try! decoder.decode(Bool.self, from: data)
        XCTAssertEqual(value, false)
    }

    func testDecodeTrue() {
        let data = Data(bytes: [0xc3])
        let value = try! decoder.decode(Bool.self, from: data)
        XCTAssertEqual(value, true)
    }

    func testDecodeInt() {
        let data = Data(bytes: [0x2A])
        let value = try! decoder.decode(Int.self, from: data)
        XCTAssertEqual(value, 42)
    }

    func testDecodeNegativeInt() {
        let data = Data(bytes: [0xFF])
        let value = try! decoder.decode(Int.self, from: data)
        XCTAssertEqual(value, -1)
    }

    func testDecodeUInt() {
        let data = Data(bytes: [0xCC, 0x80])
        let value = try! decoder.decode(Int.self, from: data)
        XCTAssertEqual(value, 128)
    }

    func testDecodeFloat() {
        let data = Data(bytes: [0xCA, 0x40, 0x48, 0xF5, 0xC3])
        let value = try! decoder.decode(Float.self, from: data)
        XCTAssertEqual(value, 3.14)
    }

    func testDecodeFloatToDouble() {
        let data = Data(bytes: [0xCA, 0x40, 0x48, 0xF5, 0xC3])
        let type = assertTypeMismatch(try decoder.decode(Double.self, from: data))
        XCTAssertTrue(type is Double.Type)
        decoder.nonMatchingFloatDecodingStrategy = .cast
        let value = try! decoder.decode(Double.self, from: data)
        XCTAssertEqual(value, 3.14, accuracy: 1e-6)
    }

    func testDecodeDouble() {
        let data = Data(bytes: [0xCB, 0x40, 0x09, 0x21, 0xF9, 0xF0, 0x1B, 0x86, 0x6E])
        let value = try! decoder.decode(Double.self, from: data)
        XCTAssertEqual(value, 3.14159)
    }

    func testDecodeDoubleToFloat() {
        let data = Data(bytes: [0xCB, 0x40, 0x09, 0x21, 0xF9, 0xF0, 0x1B, 0x86, 0x6E])
        let type = assertTypeMismatch(try decoder.decode(Float.self, from: data))
        XCTAssertTrue(type is Float.Type)
        decoder.nonMatchingFloatDecodingStrategy = .cast
        let value = try! decoder.decode(Float.self, from: data)
        XCTAssertEqual(value, 3.14159)
    }

    func testDecodeFixedArray() {
        let data = Data(bytes: [0x93, 0x01, 0x02, 0x03])
        let value = try! decoder.decode([Int].self, from: data)
        XCTAssertEqual(value, [1, 2, 3])
    }

    func testDecodeVariableArray() {
        let data = Data(bytes: [0xdc] + [0x00, 0x10] + Array(0x01...0x10))
        let value = try! decoder.decode([Int].self, from: data)
        XCTAssertEqual(value, Array(1...16))
    }

    func testDecodeFixedDictionary() {
        let data = Data(bytes: [0x83, 0xA1, 0x62, 0x02, 0xA1, 0x61, 0x01, 0xA1, 0x63, 0x03])
        let value = try! decoder.decode([String: Int].self, from: data)
        XCTAssertEqual(value, ["a": 1, "b": 2, "c": 3])
    }

    func testDecodeData() {
        let data = Data(bytes: [0xC4, 0x05, 0x68, 0x65, 0x6C, 0x6C, 0x6F])
        let value = try! decoder.decode(Data.self, from: data)
        XCTAssertEqual(value, "hello".data(using: .utf8))
    }

    func testDecodeDate() {
        let data = Data(bytes: [0xD6, 0xFF, 0x00, 0x00, 0x00, 0x01])
        let date = Date(timeIntervalSince1970: 1)
        let value = try! decoder.decode(Date.self, from: data)
        XCTAssertEqual(value, date)
    }

    func testDecodeDistantPast() {
        let data = Data(bytes: [0xC7, 0x0C, 0xFF, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xF1, 0x88, 0x6B, 0x66, 0x00])
        let date = Date.distantPast
        let value = try! decoder.decode(Date.self, from: data)
        XCTAssertEqual(value, date)
    }

    func testDecodeDistantFuture() {
        let data = Data(bytes: [0xC7, 0x0C, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0E, 0xEC, 0x31, 0x88, 0x00])
        let date = Date.distantFuture
        let value = try! decoder.decode(Date.self, from: data)
        XCTAssertEqual(value, date)
    }

    func testDecodeArrayWithDate() {
        let data = Data(bytes: [0x91, 0xD6, 0xFF, 0x00, 0x00, 0x00, 0x01])
        let date = Date(timeIntervalSince1970: 1)
        let value = try! decoder.decode([Date].self, from: data)
        XCTAssertEqual(value, [date])
    }

    func testDecodeDictionaryWithDate() {
        let data = Data(bytes: [0x81, 0xA1, 0x31, 0xD6, 0xFF, 0x00, 0x00, 0x00, 0x01])
        let date = Date(timeIntervalSince1970: 1)
        let value = try! decoder.decode([String: Date].self, from: data)
        XCTAssertEqual(value, ["1": date])
    }
    
    func testDecodeBv() {
        let b64 = "lK50ZXN0aW5nIHN0cmluZyeSpnF3ZXF3ZagxMjNpY29uc5OU2SRlMTZkYTYwMi0zMjE1LTRiZDYtYjY5MC00Y2Q4NmEwZmU3NjSoQ2lwaGVyIDEBk61jaXBodXNlcm5hbWUxrWFkZmFmZHcyMzQxMzGSkblodHRwczovL3d3dy5nb29nbGUuY29tLmFykbVodHRwczovL3d3dy5hcHBsZS5jb22U2SRhNjExMWU2Ny1hMTMwLTRiM2ItODM5NS0xZjIzMDFjNjk3ZjeoQ2lwaGVyIDIBk6g0MzEzMjEzMatqbGpsbHl1bHVpecCU2SRiOGIwODM3MC0xNGU0LTQzZmUtYjBkOS04ZjJlMDlmODJkYzWoQ2lwaGVyIDMBk6twaW9waW9waXBpb6x6eGN6eHZ6eHZ4enaSkbdodHRwczovL3d3dy52aXNhLmNvbS5hcpG1aHR0cHM6Ly93d3cuZG9ja3MuY29t" // array mode with envData and ciphers
        
//        let b64 = "hKFirnRlc3Rpbmcgc3RyaW5noWMnp2VudkRhdGGCpGJhc2WmcXdlcXdlpWljb25zqDEyM2ljb25zp2NpcGhlcnOThKJpZNkkMDA4YmE0NDctZjU0Mi00OWVjLWJjYTktMDMzZTQ2OTU0YTBipG5hbWWoQ2lwaGVyIDGkdHlwZQGlbG9naW6DqHVzZXJuYW1lrWNpcGh1c2VybmFtZTGkdG90cK1hZGZhZmR3MjM0MTMxpHVyaXOSgaN1cmm5aHR0cHM6Ly93d3cuZ29vZ2xlLmNvbS5hcoGjdXJptWh0dHBzOi8vd3d3LmFwcGxlLmNvbYSiaWTZJDQ1ZTBhODJiLTgyZGQtNDJiZi05ODhhLTAyYTkyNGM4Yzg5M6RuYW1lqENpcGhlciAypHR5cGUBpWxvZ2lug6h1c2VybmFtZag0MzEzMjEzMaR0b3Rwq2psamxseXVsdWl5pHVyaXPAhKJpZNkkZTBjZWU5NDEtZDI1Ni00MjdiLWJkNWUtNDMxMmMwN2U1NDI5pG5hbWWoQ2lwaGVyIDOkdHlwZQGlbG9naW6DqHVzZXJuYW1lq3Bpb3Bpb3BpcGlvpHRvdHCsenhjenh2enh2eHp2pHVyaXOSgaN1cmm3aHR0cHM6Ly93d3cudmlzYS5jb20uYXKBo3VyabVodHRwczovL3d3dy5kb2Nrcy5jb20=" // dict mode with envData and ciphers
        
        do {
            if let d = Data(base64Encoded: b64) {
                let decoder = MessagePackDecoder()
                decoder.userInfo[MessagePackDecoder.dataSpecKey] = DataSpecBuilder()
                    .append("b")
                    .append("c")
                    .appendObj("envData", DataSpecBuilder()
                        .append("base")
                        .append("icons")
                        .build())
                    .appendArray("ciphers", DataSpecBuilder()
                        .append("id")
                        .append("name")
                        .append("type")
                        .appendObj("login", DataSpecBuilder()
                            .append("username")
                            .append("totp")
                            .appendArray("uris", DataSpecBuilder()
                                .append("uri")
                                .build())
                            .build())
                        .build())
                    .build()

                let codTest = try decoder.decode(CodableTest.self, from: d)
                
                XCTAssertEqual(codTest.b, "testing string")
                XCTAssertEqual(codTest.envData.base, "qweqwe")
                XCTAssertEqual(codTest.envData.icons, "123icons")
                XCTAssertTrue(codTest.ciphers!.count > 1)
            } else {
                XCTAssertEqual(1, 0)
            }
        } catch let error {
            XCTFail("E: \(error)")
        }
    }

    static var allTests = [
        ("testDecodeNil", testDecodeNil),
        ("testDecodeFalse", testDecodeFalse),
        ("testDecodeTrue", testDecodeTrue),
        ("testDecodeInt", testDecodeInt),
        ("testDecodeUInt", testDecodeUInt),
        ("testDecodeFloat", testDecodeFloat),
        ("testDecodeFloatToDouble", testDecodeFloatToDouble),
        ("testDecodeDouble", testDecodeDouble),
        ("testDecodeDoubleToFloat", testDecodeDoubleToFloat),
        ("testDecodeFixedArray", testDecodeFixedArray),
        ("testDecodeFixedDictionary", testDecodeFixedDictionary),
        ("testDecodeData", testDecodeData),
        ("testDecodeDistantPast", testDecodeDistantPast),
        ("testDecodeDistantFuture", testDecodeDistantFuture),
        ("testDecodeArrayWithDate", testDecodeArrayWithDate),
        ("testDecodeDictionaryWithDate", testDecodeDictionaryWithDate),
        ("testDecodeBv", testDecodeBv)
    ]
}

struct CodableTest : Codable {
    enum CodingKeys: Int, CodingKey {
        case b
        case c
        case envData
        case ciphers
    }
    
    var b: String
    var c: Int
    var envData: EnvironmentUrlDataDto
    var ciphers: [Cipher]?
    
    func printt() {
        print("B: \(b)")
        print("C: \(c)")
        print("ENVDATA")
        envData.printt()
        
        if let cs = ciphers {
            print("CIPHERS")
            for c in cs {
                c.printt()
                print("----------------------------")
            }
        }
        
        print("###########################")
    }
}

struct EnvironmentUrlDataDto : Codable {
    var base: String?
    var icons: String?
    
    func printt() {
        print("Base: \(base ?? "")")
        print("Icons: \(icons ?? "")")
    }
}

struct Cipher:Identifiable,Codable{
    enum CodingKeys: Int, CodingKey {
        case id
        case name
        case login
    }

    var id:String
    var name:String?
    var userId:String?
    var login:Login
    
    func printt() {
        print("id: \(id)")
        print("name: \(name ?? "")")
        print("LOGIN")
        login.printt()
    }
}

struct Login:Codable{
    var username:String?
    var totp:String?
    var uris:[LoginUri]?
    
    func printt() {
        print("username: \(username ?? "")")
        print("totp: \(totp ?? "")")
        print("URIS")
        if let us = uris {
            for u in us {
                u.printt()
                print("----------------------------")
            }
        }
    }
}

struct LoginUri:Codable{
    var uri:String?
    
    func printt() {
        print("Uri: \(uri ?? "")")
    }
}
