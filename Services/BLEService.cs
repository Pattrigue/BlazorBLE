using BlazorBLE.Data;
using Plugin.BLE;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Plugin.BLE.Abstractions.Exceptions;
using Plugin.BLE.Abstractions.Extensions;

namespace BlazorBLE.Services;

public class BLEService
{
    public event Action DevicesChanged;

    public List<IDevice> devices = new();

    private readonly IAdapter adapter;

    public BLEService()
    {
        adapter = CrossBluetoothLE.Current.Adapter;
        adapter.DeviceDiscovered += Adapter_DeviceDiscovered;
    }

    ~BLEService()
    {
        adapter.DeviceDiscovered -= Adapter_DeviceDiscovered;
        adapter.StopScanningForDevicesAsync();
    }

    private void Adapter_DeviceDiscovered(object sender, DeviceEventArgs args)
    {
        IDevice device = args.Device;

        if (!KBeaconData.IsProximityBeacon(device)) return;

        Console.WriteLine($"Found device: {device.Id} {device.Name}");
        devices.Add(device);
        devices.Sort((deviceA, deviceB) => deviceB.Rssi - deviceA.Rssi);

        DevicesChanged?.Invoke();
    }

    public async void BeginScan()
    {
        PermissionStatus locationstatus = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

        await Console.Out.WriteLineAsync("Begin scan");

        await Console.Out.WriteLineAsync(locationstatus.ToString());

        if (locationstatus != PermissionStatus.Granted)
        {
            PermissionStatus permissionStatus = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

            if (permissionStatus != PermissionStatus.Granted)
            {
                await Console.Out.WriteLineAsync("Location permission not granted.");
                return;
            }
        }

        bool hasBluetoothPermissions = await CheckBluetoothStatus();

        await Console.Out.WriteLineAsync(hasBluetoothPermissions.ToString());

        if (!hasBluetoothPermissions)
        {
            bool gotPermission = await RequestBluetoothAccess();

            if (!gotPermission)
            {
                await Console.Out.WriteLineAsync("Bluetooth permissions not granted.");
                return;
            }
        }

        await Console.Out.WriteLineAsync("Scanning now");

        adapter.ScanMode = ScanMode.LowLatency;
        await adapter.StartScanningForDevicesAsync();
    }

    public IReadOnlyList<IDevice> GetKnownDevices() => adapter.GetSystemConnectedOrPairedDevices();

    public void ConnectToDevice(IDevice device, Action<bool, string> onComplete)
    {
        Task.Run(async () =>
        {
            try
            {
                if (adapter.IsScanning)
                {
                    await adapter.StopScanningForDevicesAsync();
                }

                await Console.Out.WriteLineAsync("BLEService: Connecting to device...");

                await adapter.ConnectToDeviceAsync(device);
                await PrintDeviceServicesAndCharacteristics(device);

                await Console.Out.WriteLineAsync("BLEService: Connected to device.");
                onComplete(true, null);
            }
            catch (DeviceConnectionException ex)
            {
                // specific
                await Console.Out.WriteLineAsync($"BLESERVICE DeviceConnectionException: {ex.Message}");
                onComplete(false, ex.Message);
            }
            catch (Exception ex)
            {
                // generic
                await Console.Out.WriteLineAsync($"BLESERVICE Generic exception: {ex.Message}");
                onComplete(false, ex.Message);
            }
        });
    }

    private static async Task PrintDeviceServicesAndCharacteristics(IDevice device)
    {
        await Console.Out.WriteLineAsync("AdvertisementRecords:");

        foreach (var advertisement in device.AdvertisementRecords)
        {
            var data = advertisement.Data;

            await Console.Out.WriteLineAsync(advertisement.ToString());
            await Console.Out.WriteLineAsync(advertisement.Data.Length.ToString());
            await Console.Out.WriteLineAsync(System.Text.Encoding.Default.GetString(data));

            if (advertisement.Type.HasFlag(AdvertisementRecordType.ManufacturerSpecificData))
            {
                KBeaconData beaconData = new KBeaconData(advertisement.Data);
                await Console.Out.WriteLineAsync($"BeaconData = {beaconData}");
            }
        }

        IReadOnlyList<IService> services = await device.GetServicesAsync();

        foreach (IService service in services)
        {
            await Console.Out.WriteLineAsync($"BLEService: Found service {service.Name} with ID {service.Id}.");

            IReadOnlyList<ICharacteristic> chararcteristics = await service.GetCharacteristicsAsync();

            if (chararcteristics != null)
            {
                await Console.Out.WriteLineAsync($"BLEService: Found characteristics: ");

                foreach (var characteristic in chararcteristics)
                {
                    await Console.Out.WriteLineAsync($"- Name: {characteristic.Name}");

                    if (characteristic.Value != null)
                    {
                        await Console.Out.WriteLineAsync($"- Uuid: {characteristic.Uuid}");
                        await Console.Out.WriteLineAsync($"- Property type: {characteristic.Properties}");
                        await Console.Out.WriteLineAsync($"- Write type: {characteristic.WriteType}");

                        if (characteristic.CanRead)
                        {
                            var bytes = await characteristic.ReadAsync();
                            await Console.Out.WriteLineAsync($"- ReadAsync bytes: {bytes.ToHexString()}");
                        }
                    }

                    var descriptors = await characteristic.GetDescriptorsAsync();

                    if (descriptors.Count > 0) 
                    {
                        await Console.Out.WriteLineAsync($"BLEService: Found descriptors: ");

                        foreach (var descriptor in descriptors)
                        {
                            await Console.Out.WriteLineAsync($"Name: {descriptor.Name}");
                            await Console.Out.WriteLineAsync($"Id: {descriptor.Id}");
                        }
                    }
                }

                await Console.Out.WriteLineAsync("");
            }
        }
    }

    private async Task<bool> CheckBluetoothStatus()
    {
        try
        {
            var requestStatus = await new BluetoothPermissions().CheckStatusAsync();
            return requestStatus == PermissionStatus.Granted;
        }
        catch (Exception ex)
        {
            // logger.LogError(ex);
            return false;
        }
    }

    public async Task<bool> RequestBluetoothAccess()
    {
        try
        {
            var requestStatus = await new BluetoothPermissions().RequestAsync();
            return requestStatus == PermissionStatus.Granted;
        }
        catch (Exception ex)
        {
            // logger.LogError(ex);
            return false;
        }
    }
}

