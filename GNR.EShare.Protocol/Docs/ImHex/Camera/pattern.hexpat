
be s8 unkown1  @0x03 [[color("FF0000")]];


be s8 unkown2  @0x0000000F [[color("FF0000")]];

be s8 packetNumber @0x00000017;

be s8 lastPacketNumber @0x0000001B;

be s16 dataSize @0x00000026;

be s48 unixTime @0x0000001E;

be s8 data[dataSize] @0x0000002C;


