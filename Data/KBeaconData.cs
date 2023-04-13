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
    }
}
