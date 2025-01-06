Pod::Spec.new do |s|
    s.name = 'MessagePack-FlightSchool'
    s.module_name  = 'MessagePack'
    s.version      = '1.2.4'
    s.summary      = 'A MessagePack encoder and decoder for Codable types.'

    s.description  = <<-DESC
      This functionality is discussed in Chapter 7 of
      Flight School Guide to Swift Codable.
    DESC

    s.homepage     = 'https://flight.school/books/codable/'

    s.license      = { type: 'MIT', file: 'LICENSE.md' }

    s.author = { 'Mattt' => 'mattt@flight.school' }

    s.social_media_url   = 'https://twitter.com/mattt'

    s.ios.deployment_target = '8.0'
    s.osx.deployment_target = '10.10'
    s.watchos.deployment_target = '2.0'
    s.tvos.deployment_target = '9.0'

    s.source = { git: 'https://github.com/Flight-School/MessagePack.git',
                 tag: s.version.to_s }

    s.source_files = 'Sources/**/*.swift'

    s.swift_version = '4.2'
    s.static_framework = true
end
