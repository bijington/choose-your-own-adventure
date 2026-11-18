// Run from the repository root. Keeps fixed cells so animation doesn't jitter
// from per-frame trimming. Source sheets are the approved generated artwork.
// swift -module-cache-path /tmp/champion-swift-cache tools/prepare-celebration-sprites.swift
import Foundation
import CoreGraphics
import ImageIO
import UniformTypeIdentifiers

let output = "src/ChooseYourOwnAdventure.Presenter/Resources/Images/Champions"
let sources = [
    ("driver", "exec-b347f74b-95fd-4b8f-b2df-b8a2785bbf91.png"),
    ("charioteer", "exec-22ba08d0-f9d8-4929-8b23-5243fd17af65.png"),
    ("pilot", "exec-fda8887f-eb0c-403f-a96d-891e16864c6b.png")
]
func save(_ image: CGImage, _ path: String) {
    let destination = CGImageDestinationCreateWithURL(URL(fileURLWithPath:path) as CFURL,
        UTType.png.identifier as CFString, 1, nil)!
    CGImageDestinationAddImage(destination, image, nil)
    precondition(CGImageDestinationFinalize(destination))
}
for (name, file) in sources {
    let source = CGImageSourceCreateWithURL(URL(fileURLWithPath:"generated_images/"+file) as CFURL,nil)!
    let image = CGImageSourceCreateImageAtIndex(source,0,nil)!
    precondition(image.width == 1536 && image.height == 1024, "Unexpected source grid")
    let w=image.width, h=image.height
    var pixels=[UInt8](repeating:0,count:w*h*4)
    let context=CGContext(data:&pixels,width:w,height:h,bitsPerComponent:8,bytesPerRow:w*4,
        space:CGColorSpaceCreateDeviceRGB(),bitmapInfo:CGImageAlphaInfo.premultipliedLast.rawValue)!
    context.draw(image,in:CGRect(x:0,y:0,width:w,height:h))
    for i in stride(from:0,to:pixels.count,by:4) {
        if pixels[i+3] < 32 { for channel in 0..<4 { pixels[i+channel]=0 } }
    }
    let sheet=context.makeImage()!
    save(sheet,output+"/\(name)_celebrate.png")
    var frames=[CGImage]()
    for index in 0..<8 {
        let frame=sheet.cropping(to:CGRect(x:(index%4)*384,y:(index/4)*512,width:384,height:512))!
        save(frame,output+String(format:"/%@_celebrate_%02d.png",name,index))
        frames.append(frame)
    }
    // Temporary animated preview for checking the row transition and loop.
    let gif=CGImageDestinationCreateWithURL(URL(fileURLWithPath:"/tmp/\(name)-celebration.gif") as CFURL,
        UTType.gif.identifier as CFString,frames.count,nil)!
    CGImageDestinationSetProperties(gif,[kCGImagePropertyGIFDictionary:[kCGImagePropertyGIFLoopCount:0]] as CFDictionary)
    for frame in frames {
        CGImageDestinationAddImage(gif,frame,[kCGImagePropertyGIFDictionary:[kCGImagePropertyGIFDelayTime:0.125]] as CFDictionary)
    }
    precondition(CGImageDestinationFinalize(gif))
    print("\(name): 1536×1024 transparent sheet, 8 frames at 384×512, row-major")
}
