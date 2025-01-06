import MessagePack

let encoder = MessagePackEncoder()

let value: String = "hello"
let encodedData = try encoder.encode(value)

print("Bytes: ", encodedData.map{ String($0, radix: 16, uppercase: true) })

let decoder = MessagePackDecoder()
let decodedValue = try decoder.decode(String.self, from: encodedData)

decodedValue == value
