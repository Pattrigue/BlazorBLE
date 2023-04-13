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

        public KBeaconData(byte[] advertisementData)
        {
            if (advertisementData.Length != ByteCount)
            {
                throw new ArgumentException($"Number of bytes was {advertisementData.Length}, expected {ByteCount}.");
            }

            if (BitConverter.IsLittleEndian)
            {
                CompanyId = advertisementData.Take(2).ToArray();
                Uuid = new Guid(
                    BitConverter.ToInt32(advertisementData.Skip(4).Take(4).ToArray()),
                    BitConverter.ToInt16(advertisementData.Skip(8).Take(2).ToArray()),
                    BitConverter.ToInt16(advertisementData.Skip(10).Take(2).ToArray()),
                    advertisementData.Skip(12).Take(8).ToArray()
                );
                Major = BitConverter.ToUInt16(advertisementData.Skip(20).Take(2).ToArray());
                Minor = BitConverter.ToUInt16(advertisementData.Skip(22).Take(2).ToArray());
            }
            else
            {
                CompanyId = advertisementData.Skip(1).Take(2).ToArray();
                Uuid = new Guid(
                    BitConverter.ToInt32(advertisementData.Skip(4).Take(4).Reverse().ToArray()),
                    BitConverter.ToInt16(advertisementData.Skip(8).Take(2).Reverse().ToArray()),
                    BitConverter.ToInt16(advertisementData.Skip(10).Take(2).Reverse().ToArray()),
                    advertisementData.Skip(12).Take(8).ToArray()
                );
                Major = BitConverter.ToUInt16(advertisementData.Skip(20).Take(2).Reverse().ToArray());
                Minor = BitConverter.ToUInt16(advertisementData.Skip(22).Take(2).Reverse().ToArray());
            }

            TxPower = (sbyte)advertisementData[advertisementData.Length - 1];
        }


        public override string ToString()
        {
            return $"Uuid = {Uuid}, Company ID = {CompanyId}, Major = {Major}, Minor = {Minor}, TxPower = {TxPower}";
        }
    }
}
