using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.Exceptions;

namespace MauiApp2.Services;

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

    private void Adapter_DeviceDiscovered(object sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs a)
    {
        IDevice device = a.Device;

        Console.WriteLine($"Found device: {device.Id} {device.Name}");
        devices.Add(device);
        devices.Sort((deviceA, deviceB) => deviceB.Rssi - deviceA.Rssi);

        DevicesChanged?.Invoke();
    }

    public async void BeginScan()
	{
        PermissionStatus locationstatus = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

        await Console.Out.WriteLineAsync("Begin scan");

        if (locationstatus != PermissionStatus.Granted)
        {
            PermissionStatus permissionStatus = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

            if (permissionStatus != PermissionStatus.Granted)
            {
                await Console.Out.WriteLineAsync("No permissions");
                return;
            }
        }

        bool hasBluetoothPermissions = await CheckBluetoothStatus();

        if (!hasBluetoothPermissions)
        {
            bool gotPermission = await RequestBluetoothAccess();

            if (!gotPermission)
            {
                await Console.Out.WriteLineAsync("no bluetooth?");
                return;
            }
        }

        await Console.Out.WriteLineAsync("Scanning now");

        adapter.ScanMode = ScanMode.LowLatency;
        await adapter.StartScanningForDevicesAsync();
    }

    public void ConnectToDevice(IDevice device, Action<bool, string> onComplete)
    {
        Task.Run(async () =>
        { 
            try
            {
                await Console.Out.WriteLineAsync("BLEService: Connecting to device...");
                await adapter.ConnectToDeviceAsync(device);
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
