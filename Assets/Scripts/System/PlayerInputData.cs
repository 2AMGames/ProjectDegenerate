using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

using ExitGames.Client.Photon;
using UnityEngine;

public class PlayerInputData 
{
    private const int DataSize = 12;

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

    public static short Serialize(StreamBuffer outstream, object data)
    {
        PlayerInputData inputData = (PlayerInputData)data;

        byte[] obj = new byte[DataSize];
        int byteArrayIndex = 0;

        Protocol.Serialize((int)inputData.FrameNumber, obj, ref byteArrayIndex);
        Protocol.Serialize((int)inputData.PlayerIndex, obj, ref byteArrayIndex);
        Protocol.Serialize((short)inputData.InputPattern, obj, ref byteArrayIndex);

        outstream.Write(obj, 0, DataSize);

        return DataSize;
    }

    public static object Deserialize(StreamBuffer inStream, short length)
    {
        PlayerInputData inputData = new PlayerInputData();

        byte[] output = new byte[DataSize];

        inStream.Read(output, 0, DataSize);

        int index = 0;
        int frameNumber;
        short inputPattern;

        Protocol.Deserialize(out frameNumber, output, ref index);
        inputData.FrameNumber = (uint)frameNumber;

        Protocol.Deserialize(out inputData.PlayerIndex, output, ref index);

        Protocol.Deserialize(out inputPattern, output, ref index);
        inputData.InputPattern = (ushort)inputPattern;

        return inputData;
    }

}
