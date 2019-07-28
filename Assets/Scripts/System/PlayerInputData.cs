using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputData 
{
    public uint FrameNumber;

    public int PlayerIndex;

    // Pattern: 
    // Bit 0: LP
    // Bit 1: MP
    // Bit 2: HP
    // Bit 3: LK
    // Bit 4: MK
    // Bit 5: HK
    // Bit 6: Left Directional Input
    // Bit 7: Right Directional Input
    // Bit 8: Up Directional Input
    // Bit 9: Down Directional Input
    public ushort InputPattern;

}
