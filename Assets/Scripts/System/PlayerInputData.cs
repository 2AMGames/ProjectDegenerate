using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputData 
{
    public ushort FrameNumber;

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

    public override bool Equals(object ob)
    {
        PlayerInputData frameData = ob as PlayerInputData;
        // Leave frame data out of the comparison, since we only want to check data that is concerned with player input.
        return frameData != null && InputPattern == frameData.InputPattern;
    }

}
