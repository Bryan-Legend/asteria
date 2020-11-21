using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace HD
{
    [DataContract]
    public class MapUpdate
    {
        [DataMember(Order = 1, IsRequired = true)]
        public int X { get; set; }

        [DataMember(Order = 2, IsRequired = true)]
        public int Y { get; set; }

        [DataMember(Order = 3)]
        public Material Material { get; set; }

        [DataMember(Order = 4)]
        public int Width { get; set; }

        [DataMember(Order = 5)]
        public int Height { get; set; }

        [DataMember(Order = 6)]
        public byte[] MapData { get; set; }

        public void LoadData(byte[] sourceData, int sourceDataWidth)
        {
            MapData = new Byte[Width * Height];

            for (var y = 0; y < Height; y++) {
                var offset = (Y + y) * sourceDataWidth + X;
                var length = Width;

                if (offset > sourceData.Length || offset + length < 0)
                    continue;
                if (offset + length >= sourceData.Length)
                    length = sourceData.Length - offset;
                else if (offset < 0) {
                    length += offset;
                    offset = 0;
                }

                Array.Copy(sourceData, offset, MapData, y * Width, length);
            }
        }

        public void SetData(byte[] destData, int destDataWidth)
        {
            if (MapData != null) {
                for (var y = 0; y < Height; y++) {
                    var offset = (Y + y) * destDataWidth + X;
                    var length = Width;

                    if (offset > destData.Length || offset + length < 0)
                        continue;
                    if (offset + length >= destData.Length)
                        length = destData.Length - offset;
                    else if (offset < 0) {
                        length += offset;
                        offset = 0;
                    }

                    Array.Copy(MapData, y * Width, destData, offset, length);
                }
            } else
                destData[(Y * destDataWidth) + X] = (byte)Material;
        }
    }
}
