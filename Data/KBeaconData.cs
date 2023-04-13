using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions;

namespace BlazorBLE.Data
{
    internal class KBeaconData
    {
        private const int ByteCount = 25;

        public Guid Uuid { get; }

        public byte[] CompanyId { get; }

        public ushort Major { get; }
        public ushort Minor { get; }
        
        public sbyte TxPower { get; }

        /// <summary>
        /// Wrapper class for the KBeacon advertising packet.
        /// </summary>
        /// <param name="data">The advertising packet for the Proximity beacon.</param>
        /// <exception cref="ArgumentException"></exception>
        public KBeaconData(byte[] data)
        {
            if (data.Length != ByteCount)
            {
                throw new ArgumentException($"Number of bytes was {data.Length}, expected {ByteCount}.");
            }

            if (BitConverter.IsLittleEndian)
            {
                CompanyId = data.Take(2).ToArray();
                Uuid = new Guid(
                    BitConverter.ToInt32(data.Skip(4).Take(4).ToArray()),
                    BitConverter.ToInt16(data.Skip(8).Take(2).ToArray()),
                    BitConverter.ToInt16(data.Skip(10).Take(2).ToArray()),
                    data.Skip(12).Take(8).ToArray()
                );
                Major = BitConverter.ToUInt16(data.Skip(20).Take(2).ToArray());
                Minor = BitConverter.ToUInt16(data.Skip(22).Take(2).ToArray());
            }
            else
            {
                CompanyId = data.Skip(1).Take(2).ToArray();
                Uuid = new Guid(
                    BitConverter.ToInt32(data.Skip(4).Take(4).Reverse().ToArray()),
                    BitConverter.ToInt16(data.Skip(8).Take(2).Reverse().ToArray()),
                    BitConverter.ToInt16(data.Skip(10).Take(2).Reverse().ToArray()),
                    data.Skip(12).Take(8).ToArray()
                );
                Major = BitConverter.ToUInt16(data.Skip(20).Take(2).Reverse().ToArray());
                Minor = BitConverter.ToUInt16(data.Skip(22).Take(2).Reverse().ToArray());
            }

            TxPower = (sbyte)data[data.Length - 1];
        }

        public override string ToString()
        {
            return $"Uuid = {Uuid}, Company ID = {CompanyId}, Major = {Major}, Minor = {Minor}, TxPower = {TxPower}";
        }

        public static bool IsProximityBeacon(IDevice device)
        {
            foreach (AdvertisementRecord record in device.AdvertisementRecords)
            {
                if (record.Type == AdvertisementRecordType.ManufacturerSpecificData)
                {
                    return IsProximityBeacon(record.Data);
                }
            }

            return false;
        }

        public static bool IsProximityBeacon(byte[] data)
        {
            if (data.Length < 4)
            {
                throw new ArgumentException($"Expected 4 bytes or more, got {data.Length} bytes.");
            }

            bool isCompanyApple = data[0] == 0x4C && data[1] == 0x00;
            bool isProximityBeacon = data[2] == 0x02 && data[3] == 0x15;

            return isCompanyApple && isProximityBeacon;
        }
    }
}
