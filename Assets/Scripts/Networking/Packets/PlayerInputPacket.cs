using System.Collections;
using System.Collections.Generic;

using ExitGames.Client.Photon;
using Newtonsoft.Json;

using System.Text;

using UnityEngine;

public class PlayerInputPacket
{
    private static int DataSize = 12;

    public uint PacketId;

    public int PlayerIndex;

    public List<PlayerInputData> InputData;

    public struct PlayerInputData
    {
        [JsonProperty]
        public uint FrameNumber { get; set; }

        [JsonProperty]
        public int PlayerIndex { get; set; }

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
        [JsonProperty]
        public ushort InputPattern { get; set; }
    }

    public static short Serialize(StreamBuffer outstream, object data)
    {
        PlayerInputPacket inputData = (PlayerInputPacket)data;
        string inputDataListJson = JsonConvert.SerializeObject(inputData);

        Debug.LogWarning("Input json: " + inputDataListJson);

        Encoding encoder = Encoding.UTF8;
        byte[] stringToByte = encoder.GetBytes(inputDataListJson);
        int stringSize = stringToByte.Length;

        Debug.LogWarning("String size: " + stringSize);
        

        int byteArrayIndex = 0;
        byte[] obj = new byte[DataSize + stringSize];

        Protocol.Serialize((int)inputData.PacketId, obj, ref byteArrayIndex);
        Protocol.Serialize(inputData.PlayerIndex, obj, ref byteArrayIndex);
        Protocol.Serialize(stringSize, obj, ref byteArrayIndex);
        System.Array.Copy(stringToByte, 0, obj, byteArrayIndex, stringToByte.Length);

        outstream.Write(obj, 0, DataSize + stringSize);

        return (short)(stringSize + DataSize);
    }

    public static object Deserialize(StreamBuffer inStream, short length)
    {
        PlayerInputPacket inputPacket = new PlayerInputPacket();

        byte[] output = new byte[DataSize];

        inStream.Read(output, 0, DataSize);

        int index = 0;

        int packetId;
        int stringSize;

        Protocol.Deserialize(out packetId, output, ref index);
        inputPacket.PacketId = (uint)packetId;

        Protocol.Deserialize(out inputPacket.PlayerIndex, output, ref index);

        Protocol.Deserialize(out stringSize, output, ref index);
        byte[] customString = new byte[4 + stringSize];
        inStream.Read(customString, index, stringSize);

        string jsonString = System.Text.Encoding.UTF8.GetString(customString);
        List<PlayerInputData> inputList = JsonConvert.DeserializeObject<List<PlayerInputData>>(jsonString);
        inputPacket.InputData = inputList;
        return inputPacket;
    }


}
