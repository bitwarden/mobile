struct Airport: Codable, Equatable {
    let name: String
    let iata: String
    let icao: String
    let coordinates: [Double]

    struct Runway: Codable, Equatable {
        enum Surface: String, Codable, Equatable {
            case rigid, flexible, gravel, sealed, unpaved, other
        }
        
        let direction: String
        let distance: Int
        let surface: Surface
    }
    
    let runways: [Runway]

    let instrumentApproachProcedures: [String]
    
    static var example: Airport {
        return Airport(
            name: "Portland International Airport",
            iata: "PDX",
            icao: "KPDX",
            coordinates: [-122.5975,
                          45.5886111111111],
            runways: [
                Airport.Runway(
                    direction: "3/21",
                    distance: 1829,
                    surface: .flexible
                )
            ],
            instrumentApproachProcedures: [
                "HI-ILS OR LOC RWY 28",
                "HI-ILS OR LOC/DME RWY 10",
                "ILS OR LOC RWY 10L",
                "ILS OR LOC RWY 10R",
                "ILS OR LOC RWY 28L",
                "ILS OR LOC RWY 28R",
                "ILS RWY 10R (SA CAT I)",
                "ILS RWY 10R (CAT II - III)",
                "RNAV (RNP) Y RWY 28L",
                "RNAV (RNP) Y RWY 28R",
                "RNAV (RNP) Z RWY 10L",
                "RNAV (RNP) Z RWY 10R",
                "RNAV (RNP) Z RWY 28L",
                "RNAV (RNP) Z RWY 28R",
                "RNAV (GPS) X RWY 28L",
                "RNAV (GPS) X RWY 28R",
                "RNAV (GPS) Y RWY 10L",
                "RNAV (GPS) Y RWY 10R",
                "LOC/DME RWY 21",
                "VOR-A",
                "HI-TACAN RWY 10",
                "TACAN RWY 28",
                "COLUMBIA VISUAL RWY 10L/",
                "MILL VISUAL RWY 28L/R"
            ]
        )
    }
}
