import Foundation

extension _MessagePackDecoder {
    final class UnkeyedContainer {
        var codingPath: [CodingKey]
        
        var nestedCodingPath: [CodingKey] {
            return self.codingPath + [AnyCodingKey(intValue: self.count ?? 0)!]
        }
        
        var userInfo: [CodingUserInfoKey: Any]
        
        var data: Data
        var index: Data.Index
        var currentSpec: DataSpec?
        
        lazy var count: Int? = {
            do {
                let format = try self.readByte()
                switch format {
                case 0x90...0x9f:
                    return Int(format & 0x0F)
                case 0xdc:
                    return Int(try read(UInt16.self))
                case 0xdd:
                    return Int(try read(UInt32.self))
                default:
                    return nil
                }
            } catch {
                return nil
            }
        }()
        
        var currentIndex: Int = 0
        
        lazy var nestedContainers: [MessagePackDecodingContainer] = {
            guard let count = self.count else {
                return []
            }
            
            var nestedContainers: [MessagePackDecodingContainer] = []
            
            do {
                for _ in 0..<count {
                    let container = try self.decodeContainer()
                    nestedContainers.append(container)
                }
            } catch {
                fatalError("\(error)") // FIXME
            }
            
            self.currentIndex = 0
            
            return nestedContainers
        }()
        
        init(data: Data, codingPath: [CodingKey], userInfo: [CodingUserInfoKey : Any]) {
            self.codingPath = codingPath
            self.userInfo = userInfo
            self.data = data
            self.index = self.data.startIndex
        }
        
        var isAtEnd: Bool {
            guard let count = self.count else {
                return true
            }
            
            return currentIndex >= count
        }
        
        func checkCanDecodeValue() throws {
            guard !self.isAtEnd else {
                throw DecodingError.dataCorruptedError(in: self, debugDescription: "Unexpected end of data")
            }
        }
    }
}

extension _MessagePackDecoder.UnkeyedContainer: UnkeyedDecodingContainer {
    func decodeNil() throws -> Bool {
        try checkCanDecodeValue()
        defer { self.currentIndex += 1 }

        let nestedContainer = self.nestedContainers[self.currentIndex]

        switch nestedContainer {
        case let singleValueContainer as _MessagePackDecoder.SingleValueContainer:
            return singleValueContainer.decodeNil()
        case is _MessagePackDecoder.UnkeyedContainer,
             is _MessagePackDecoder.KeyedContainer<AnyCodingKey>:
            return false
        default:
            let context = DecodingError.Context(codingPath: self.codingPath, debugDescription: "cannot decode nil for index: \(self.currentIndex)")
                       throw DecodingError.typeMismatch(Any?.self, context)
        }
    }
    
    func decode<T>(_ type: T.Type) throws -> T where T : Decodable {
        try checkCanDecodeValue()
        defer { self.currentIndex += 1 }
        
        if userInfo.keys.contains(MessagePackDecoder.isArrayDataSpecKey) {
            currentSpec = DataSpec("", false, true, (userInfo[MessagePackDecoder.dataSpecKey] as? DataSpecBuilder)?.copy() as? DataSpecBuilder)
        }
        
        let container = self.nestedContainers[self.currentIndex]
        let decoder = MessagePackDecoder()
        
        if userInfo.keys.contains(MessagePackDecoder.dataSpecKey) {
            decoder.userInfo[MessagePackDecoder.dataSpecKey] = (userInfo[MessagePackDecoder.dataSpecKey] as? DataSpecBuilder)?.copy() as? DataSpecBuilder
        }
        
        let value = try decoder.decode(T.self, from: container.data)

        return value
    }
    
    func nestedUnkeyedContainer() throws -> UnkeyedDecodingContainer {
        try checkCanDecodeValue()
        defer { self.currentIndex += 1 }

        let container = self.nestedContainers[self.currentIndex] as! _MessagePackDecoder.UnkeyedContainer
        
        return container
    }
    
    func nestedContainer<NestedKey>(keyedBy type: NestedKey.Type) throws -> KeyedDecodingContainer<NestedKey> where NestedKey : CodingKey {
        try checkCanDecodeValue()
        defer { self.currentIndex += 1 }

        let container = self.nestedContainers[self.currentIndex] as! _MessagePackDecoder.KeyedContainer<NestedKey>
        
        return KeyedDecodingContainer(container)
    }

    func superDecoder() throws -> Decoder {
        return _MessagePackDecoder(data: self.data)
    }
}

extension _MessagePackDecoder.UnkeyedContainer {
    func decodeContainer() throws -> MessagePackDecodingContainer {
        try checkCanDecodeValue()
        defer { self.currentIndex += 1 }
        
        let startIndex = self.index
        
        var currDataSpec: DataSpec? = nil
        if currentSpec != nil && currentSpec!.isArray && currentSpec!.dataSpecBuilder != nil {
            currDataSpec = DataSpec("", true, false, currentSpec!.dataSpecBuilder!.copy() as? DataSpecBuilder)
        } else {
            let dataSpec = self.userInfo[MessagePackDecoder.dataSpecKey] as? DataSpecBuilder
            if let currDS = dataSpec?.next() {
                currDataSpec = DataSpec(currDS.name, currDS.isObj, currDS.isArray, currDS.dataSpecBuilder?.copy() as? DataSpecBuilder)
            }
        }
        
        let length: Int
        let format = try self.readByte()
        switch format {
        case 0x00...0x7f,
             0xc0, 0xc2, 0xc3,
             0xe0...0xff:
            length = 0
        case 0xcc, 0xd0, 0xd4:
            length = 1
        case 0xcd, 0xd1, 0xd5:
            length = 2
        case 0xca, 0xce, 0xd2:
            length = 4
        case 0xcb, 0xcf, 0xd3:
            length = 8
        case 0xd6:
            length = 5
        case 0xd7:
            length = 9
        case 0xd8:
            length = 16
        case 0xa0...0xbf:
            length = Int(format - 0xa0)
        case 0xc4, 0xc7, 0xd9:
            length = Int(try read(UInt8.self))
        case 0xc5, 0xc8, 0xda:
            length = Int(try read(UInt16.self))
        case 0xc6, 0xc9, 0xdb:
            length = Int(try read(UInt32.self))
        case 0x80...0x8f, 0xde, 0xdf:
            let container = _MessagePackDecoder.KeyedContainer<AnyCodingKey>(data: self.data.suffix(from: startIndex), codingPath: self.nestedCodingPath, userInfo: self.userInfo)
            container.currentSpec = currDataSpec
            _ = container.nestedContainers // FIXME
            self.index = container.index
            
            return container
        case 0x90...0x9f, 0xdc, 0xdd:
            if currDataSpec != nil && currDataSpec!.isObj {
                var objUserInfo = self.userInfo
                objUserInfo[MessagePackDecoder.dataSpecKey] = currDataSpec!.dataSpecBuilder!
                
                let container = _MessagePackDecoder.KeyedContainer<AnyCodingKey>(data: self.data.suffix(from: startIndex), codingPath: self.nestedCodingPath, userInfo: objUserInfo)
                container.currentSpec = currDataSpec
                _ = container.nestedContainers // FIXME
                self.index = container.index
                
                return container
            }
            
            var arrUserInfo = self.userInfo
            if currDataSpec != nil && currDataSpec!.isArray {
                arrUserInfo[MessagePackDecoder.dataSpecKey] = currDataSpec!.dataSpecBuilder!
            }

            let container = _MessagePackDecoder.UnkeyedContainer(data: self.data.suffix(from: startIndex), codingPath: self.nestedCodingPath, userInfo: arrUserInfo)
            container.currentSpec = currDataSpec
            _ = container.nestedContainers // FIXME

            self.index = container.index
            
            return container
        default:
            throw DecodingError.dataCorruptedError(in: self, debugDescription: "Invalid format: \(format)")
        }
        
        let range: Range<Data.Index> = startIndex..<self.index.advanced(by: length)
        self.index = range.upperBound
        
        let container = _MessagePackDecoder.SingleValueContainer(data: self.data.subdata(in: range), codingPath: self.codingPath, userInfo: self.userInfo)
        container.currentSpec = currDataSpec
        
        return container
    }
}

extension _MessagePackDecoder.UnkeyedContainer: MessagePackDecodingContainer {}
