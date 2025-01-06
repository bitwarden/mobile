import Foundation

#if os(Linux)
let NSEC_PER_SEC: UInt64 = 1000000000
#endif

extension _MessagePackDecoder {
    final class SingleValueContainer {
        var codingPath: [CodingKey]
        var userInfo: [CodingUserInfoKey: Any]
        var data: Data
        var index: Data.Index
        var currentSpec: DataSpec?

        init(data: Data, codingPath: [CodingKey], userInfo: [CodingUserInfoKey : Any]) {
            self.codingPath = codingPath
            self.userInfo = userInfo
            self.data = data
            self.index = self.data.startIndex
        }
        
        func checkCanDecode<T>(_ type: T.Type, format: UInt8) throws {
            guard self.index <= self.data.endIndex else {
                throw DecodingError.dataCorruptedError(in: self, debugDescription: "Unexpected end of data")
            }
            
            guard self.data[self.index] == format else {
                let context = DecodingError.Context(codingPath: self.codingPath, debugDescription: "Invalid format: \(format)")
                throw DecodingError.typeMismatch(type, context)
            }
        }

        var nonMatchingFloatDecodingStrategy: MessagePackDecoder.NonMatchingFloatDecodingStrategy {
            return userInfo[MessagePackDecoder.nonMatchingFloatDecodingStrategyKey] as? MessagePackDecoder.NonMatchingFloatDecodingStrategy ?? .strict
        }
    }
}

extension _MessagePackDecoder.SingleValueContainer: SingleValueDecodingContainer {    
    func decodeNil() -> Bool {
        let format = try? readByte()
        return format == 0xc0
    }
    
    func decode(_ type: Bool.Type) throws -> Bool {
        let format = try readByte()
        switch format {
        case 0xc2: return false
        case 0xc3: return true
        default:
            let context = DecodingError.Context(codingPath: self.codingPath, debugDescription: "Invalid format: \(format)")
            throw DecodingError.typeMismatch(Bool.self, context)
        }
    }
    
    func decode(_ type: String.Type) throws -> String {
        let length: Int
        let format = try readByte()
        switch format {
        case 0xa0...0xbf:
            length = Int(format - 0xa0)
        case 0xd9:
            length = Int(try read(UInt8.self))
        case 0xda:
            length = Int(try read(UInt16.self))
        case 0xdb:
            length = Int(try read(UInt32.self))
        default:
            throw DecodingError.dataCorruptedError(in: self, debugDescription: "Invalid format for String length: \(format)")
        }
        
        let data = try read(length)
        guard let string = String(data: data, encoding: .utf8) else {
            let context = DecodingError.Context(codingPath: self.codingPath, debugDescription: "Couldn't decode string with UTF-8 encoding")
            throw DecodingError.dataCorrupted(context)
        }
        
        return string
    }
    
    func decode(_ type: Double.Type) throws -> Double {
        let format = try readByte()
        switch format {
        case 0xca:
            switch nonMatchingFloatDecodingStrategy {
            case .strict:
                break
            case .cast:
                let bitPattern = try read(UInt32.self)
                return Double(Float(bitPattern: bitPattern))
            }
        case 0xcb:
            let bitPattern = try read(UInt64.self)
            return Double(bitPattern: bitPattern)
        default:
            break
        }
        let context = DecodingError.Context(codingPath: self.codingPath, debugDescription: "Invalid format: \(format)")
        throw DecodingError.typeMismatch(Double.self, context)
    }
    
    func decode(_ type: Float.Type) throws -> Float {
        let format = try readByte()
        switch format {
        case 0xca:
            let bitPattern = try read(UInt32.self)
            return Float(bitPattern: bitPattern)
        case 0xcb:
            switch nonMatchingFloatDecodingStrategy {
            case .strict:
                break
            case .cast:
                let bitPattern = try read(UInt64.self)
                return Float(Double(bitPattern: bitPattern))
            }
        default:
            break
        }
        let context = DecodingError.Context(codingPath: self.codingPath, debugDescription: "Invalid format: \(format)")
        throw DecodingError.typeMismatch(Float.self, context)
    }
    
    func decode<T>(_ type: T.Type) throws -> T where T : BinaryInteger & Decodable {
        let format = try readByte()
        var t: T?
        
        switch format {
        case 0x00...0x7f:
            t = T(format)
        case 0xcc:
            t = T(exactly: try read(UInt8.self))
        case 0xcd:
            t = T(exactly: try read(UInt16.self))
        case 0xce:
            t = T(exactly: try read(UInt32.self))
        case 0xcf:
            t = T(exactly: try read(UInt64.self))
        case 0xd0:
            t = T(exactly: try read(Int8.self))
        case 0xd1:
            t = T(exactly: try read(Int16.self))
        case 0xd2:
            t = T(exactly: try read(Int32.self))
        case 0xd3:
            t = T(exactly: try read(Int64.self))
        case 0xe0...0xff:
            t = T(exactly: Int8(bitPattern: format))
        default:
            t = nil
        }
        
        guard let value = t else {
            let context = DecodingError.Context(codingPath: self.codingPath, debugDescription: "Invalid format: \(format)")
            throw DecodingError.typeMismatch(T.self, context)
        }
        
        return value
    }
    
    func decode(_ type: Date.Type) throws -> Date {
        let format = try readByte()
        
        var seconds: TimeInterval
        var nanoseconds: TimeInterval
        
        switch format {
        case 0xd6:
            _ = try read(Int8.self) // -1
            nanoseconds = 0
            seconds = TimeInterval(try read(UInt32.self))
        case 0xd7:
            _ = try read(Int8.self) // -1
            let bitPattern = try read(UInt64.self)
            nanoseconds = TimeInterval(UInt32(bitPattern >> 34))
            seconds = TimeInterval(UInt32(bitPattern & 0x03_FF_FF_FF_FF))
        case 0xc7:
            _ = try read(Int8.self) // 12
            _ = try read(Int8.self) // -1
            nanoseconds = TimeInterval(try read(UInt32.self))
            seconds = TimeInterval(try read(Int64.self))
        default:
            let context = DecodingError.Context(codingPath: self.codingPath, debugDescription: "Invalid format: \(format)")
            throw DecodingError.typeMismatch(Date.self, context)
        }
        
        let timeInterval = TimeInterval(seconds) + nanoseconds / Double(NSEC_PER_SEC)
        
        return Date(timeIntervalSince1970: timeInterval)
    }
    
    func decode(_ type: Data.Type) throws -> Data {
        let length: Int
        let format = try readByte()
        switch format {
        case 0xc4:
            length = Int(try read(UInt8.self))
        case 0xc5:
            length = Int(try read(UInt16.self))
        case 0xc6:
            length = Int(try read(UInt32.self))
        default:
            throw DecodingError.dataCorruptedError(in: self, debugDescription: "Invalid format for Data length: \(format)")
        }
        
        return self.data.subdata(in: self.index..<self.index.advanced(by: length))
    }
    
    func decode<T>(_ type: T.Type) throws -> T where T : Decodable {
        switch type {
        case is Data.Type:
            return try decode(Data.self) as! T
        case is Date.Type:
            return try decode(Date.self) as! T
        default:
            let decoder = _MessagePackDecoder(data: self.data)
            let value = try T(from: decoder)
            if let nextIndex = decoder.container?.index {
                self.index = nextIndex
            }
            
            return value
        }
    }
}

extension _MessagePackDecoder.SingleValueContainer: MessagePackDecodingContainer {}
