#import <Cocoa/Cocoa.h>

int main(int argc, const char * argv[]) {
    @autoreleasepool {
        NSString *outputPath = argc > 1
            ? [NSString stringWithUTF8String:argv[1]]
            : @"GuitarTUIIcon.png";
        NSSize size = NSMakeSize(1024, 1024);
        NSImage *image = [[NSImage alloc] initWithSize:size];

        [image lockFocus];

        NSRect bounds = NSMakeRect(0, 0, 1024, 1024);
        [[NSColor colorWithCalibratedRed:0.08 green:0.10 blue:0.13 alpha:1.0] setFill];
        [[NSBezierPath bezierPathWithRoundedRect:bounds xRadius:210 yRadius:210] fill];

        NSRect terminalRect = NSMakeRect(150, 235, 724, 555);
        [[NSColor colorWithCalibratedRed:0.14 green:0.16 blue:0.20 alpha:1.0] setFill];
        [[NSBezierPath bezierPathWithRoundedRect:terminalRect xRadius:42 yRadius:42] fill];

        NSRect titleBar = NSMakeRect(NSMinX(terminalRect), NSMaxY(terminalRect) - 88, NSWidth(terminalRect), 88);
        [[NSColor colorWithCalibratedRed:0.20 green:0.22 blue:0.27 alpha:1.0] setFill];
        [[NSBezierPath bezierPathWithRoundedRect:titleBar xRadius:42 yRadius:42] fill];

        NSArray<NSColor *> *buttons = @[
            [NSColor colorWithCalibratedRed:1.00 green:0.36 blue:0.32 alpha:1.0],
            [NSColor colorWithCalibratedRed:1.00 green:0.80 blue:0.28 alpha:1.0],
            [NSColor colorWithCalibratedRed:0.30 green:0.82 blue:0.42 alpha:1.0]
        ];

        for (NSInteger index = 0; index < buttons.count; index++) {
            [buttons[index] setFill];
            [[NSBezierPath bezierPathWithOvalInRect:NSMakeRect(205 + index * 48, 725, 24, 24)] fill];
        }

        NSDictionary *promptAttributes = @{
            NSFontAttributeName: [NSFont monospacedSystemFontOfSize:110 weight:NSFontWeightBold],
            NSForegroundColorAttributeName: [NSColor colorWithCalibratedRed:0.45 green:1.00 blue:0.62 alpha:1.0]
        };
        [@">" drawAtPoint:NSMakePoint(220, 515) withAttributes:promptAttributes];

        NSDictionary *textAttributes = @{
            NSFontAttributeName: [NSFont monospacedSystemFontOfSize:82 weight:NSFontWeightSemibold],
            NSForegroundColorAttributeName: [NSColor colorWithCalibratedRed:0.86 green:0.90 blue:0.96 alpha:1.0]
        };
        [@"TUI" drawAtPoint:NSMakePoint(325, 535) withAttributes:textAttributes];

        NSDictionary *guitarAttributes = @{
            NSFontAttributeName: [NSFont systemFontOfSize:280],
            NSForegroundColorAttributeName: [NSColor whiteColor]
        };
        [@"🎸" drawAtPoint:NSMakePoint(410, 245) withAttributes:guitarAttributes];

        [image unlockFocus];

        NSData *tiff = [image TIFFRepresentation];
        NSBitmapImageRep *bitmap = [[NSBitmapImageRep alloc] initWithData:tiff];
        NSData *png = [bitmap representationUsingType:NSBitmapImageFileTypePNG properties:@{}];
        [png writeToFile:outputPath atomically:YES];
    }

    return 0;
}
