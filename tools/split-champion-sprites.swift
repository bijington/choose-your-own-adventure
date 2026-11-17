// Run from the repository root:
// swift -module-cache-path /tmp/champion-swift-cache tools/split-champion-sprites.swift
import Foundation
import CoreGraphics
import ImageIO
import UniformTypeIdentifiers

let output = "src/ChooseYourOwnAdventure.Presenter/Resources/Images/Champions"
try FileManager.default.createDirectory(atPath: output, withIntermediateDirectories: true)
let sheets: [(String, [(String, CGRect)])] = [
    ("exec-7404ea89-61db-4791-8e61-367db3486116.png", [
        ("driver_character", CGRect(x:50,y:20,width:420,height:585)),
        ("charioteer_character", CGRect(x:490,y:10,width:450,height:600)),
        ("pilot_character", CGRect(x:1110,y:40,width:350,height:565)),
        ("driver_side", CGRect(x:0,y:740,width:470,height:284)),
        ("charioteer_side", CGRect(x:475,y:615,width:555,height:409)),
        ("pilot_side", CGRect(x:1045,y:710,width:491,height:314))
    ]),
    ("exec-cd28ec4a-3fcc-456d-9404-0ed77f180872.png", [
        ("driver_top", CGRect(x:0,y:0,width:590,height:887)),
        ("charioteer_top", CGRect(x:590,y:0,width:590,height:887)),
        ("pilot_top", CGRect(x:1180,y:0,width:594,height:887))
    ])
]
for (file, regions) in sheets {
    let url = URL(fileURLWithPath: "generated_images/" + file)
    guard let source = CGImageSourceCreateWithURL(url as CFURL, nil),
          let sheet = CGImageSourceCreateImageAtIndex(source, 0, nil) else {
        fatalError("Cannot read \(url.path)")
    }
    for (name, region) in regions {
        let crop = sheet.cropping(to: region)!
        let w = crop.width, h = crop.height
        var rgba = [UInt8](repeating: 0, count: w*h*4)
        let context = CGContext(data: &rgba, width:w, height:h, bitsPerComponent:8,
            bytesPerRow:w*4, space:CGColorSpaceCreateDeviceRGB(),
            bitmapInfo:CGImageAlphaInfo.premultipliedLast.rawValue)!
        context.draw(crop, in:CGRect(x:0,y:0,width:w,height:h))
        var minX=w, minY=h, maxX=0, maxY=0
        for y in 0..<h { for x in 0..<w {
            let i=(y*w+x)*4
            // Remove the generator's faint translucent haze outside the pixel outlines.
            if rgba[i+3] < 32 {
                for c in 0..<4 { rgba[i+c]=0 }
            } else {
                minX=min(minX,x); maxX=max(maxX,x)
                minY=min(minY,y); maxY=max(maxY,y)
            }
        }}
        precondition(minX < maxX && minY < maxY)
        // Keep a small transparent gutter for rotation and texture filtering.
        minX=max(0,minX-3); minY=max(0,minY-3)
        maxX=min(w-1,maxX+3); maxY=min(h-1,maxY+3)
        let cleaned=context.makeImage()!.cropping(to:CGRect(x:minX,y:minY,width:maxX-minX+1,height:maxY-minY+1))!
        let target=URL(fileURLWithPath:output+"/"+name+".png")
        let dest=CGImageDestinationCreateWithURL(target as CFURL, UTType.png.identifier as CFString,1,nil)!
        CGImageDestinationAddImage(dest,cleaned,nil)
        precondition(CGImageDestinationFinalize(dest))
        print("\(name): \(cleaned.width)×\(cleaned.height)")
    }
}
