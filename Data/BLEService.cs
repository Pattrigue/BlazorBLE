using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;

namespace MauiApp2.Data;

public class BLEService
{
    public event Action DevicesChanged;

    public List<IDevice> devices = new();

    private IAdapter adapter;

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

    private void Adapter_DeviceDiscovered(object sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs args)
    {
        devices.Add(args.Device);
        Console.WriteLine($"Found device BLEService: {args.Device.Id} {args.Device.Name}");
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

        adapter.ScanMode = ScanMode.LowPower;
        await adapter.StartScanningForDevicesAsync();
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

