using System;
using System.Windows;
using MyHIDUSBLibrary;

namespace CardioMonitor.Core.Repository.Controller
{
    public static class ControllerConnection
    {
        // public static UsbDevice BedUsbDevice;
        // public static UsbDeviceFinder BedDeviceFinder = new UsbDeviceFinder(0x0483, 0xAB12);

        public static void ExecuteCommand(string commandName)
        {
            try
            {
                HIDDevice.interfaceDetails[] devices = HIDDevice.getConnectedDevices();
                string devicePath = null;
                foreach (var listofDevice in devices)
                {
                    if (listofDevice.product == "MAKET-1_TECT_(XPOM)")
                    {
                        devicePath = listofDevice.devicePath;
                    }
                }
                if (devicePath == null)
                {
                    MessageBox.Show("Device not found!!!");
                }
                else
                {

                    var device = new HIDDevice(devicePath, false);
                    byte[] data = null;
                    switch (commandName)
                    {
                        case "Start":
                            data = new byte[] { 0x29, 0x00, 0x01, 0xff, 0xff, 0xff, 0xff, 0xff };
                            break;
                        case "Pause":
                            data = new byte[] { 0x2a, 0x00, 0x01, 0xff, 0xff, 0xff, 0xff, 0xff };
                            break;
                        case "Reverse":
                            data = new byte[] { 0x2c, 0x00, 0x01, 0xff, 0xff, 0xff, 0xff, 0xff };
                            break;
                        case "EmergencyStop":
                            data = new byte[] { 0x2b, 0x00, 0x01, 0xff, 0xff, 0xff, 0xff, 0xff };
                            break;
                    }
                    device.write(data);
                    device.close();

                    int countIn;
                    /*byte[] data1  = {0x20, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff};
                if (BedUsbDevice.ControlTransfer(ref packet, data1, 1, out countIn) && (countIn !=0))
                {
                    MessageBox.Show("Прочитано значение");
                }*/
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {



            }
        }

        public static void GetStatus()
        {
            HIDDevice.interfaceDetails[] devices = HIDDevice.getConnectedDevices();
            string devicePath = null;
            foreach (var listofDevice in devices)
            {
                if (listofDevice.product == "MAKET-1_TECT_(XPOM)")
                {
                    devicePath = listofDevice.devicePath;
                }
            }
            if (devicePath == null)
            {
                MessageBox.Show("Device not found!!!");
            }
            else
            {
                //MessageBox.Show("Device was found!!!!");

                var device = new HIDDevice(devicePath, false);
                var message = new byte[] { 0x6e, 0x00, 0x01, 0xff, 0xff, 0xff, 0xff, 0xff };
                device.write(message);
                var readData = device.read();
                if (readData != null)
                {
                    //MessageBox.Show(readData.ToString());
                    if (readData[3] == 0)
                    {
                        // BedStatus_0.Checked = true;
                    }
                    if (readData[3] == 1)
                    {
                        //  BedStatus_1.Checked = true;
                    }
                    if (readData[3] == 2)
                    {
                        //BedStatus_2.Checked = true;
                    }
                }
                device.close();
            }
        }
    }
}
