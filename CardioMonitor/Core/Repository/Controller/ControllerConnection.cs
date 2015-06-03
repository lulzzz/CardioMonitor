using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibUsbDotNet;
using LibUsbDotNet.Main;
using System.Windows;

namespace CardioMonitor.Core.Repository.Controller
{
    public static class ControllerConnection
    {
        public static UsbDevice BedUsbDevice;
        public static UsbDeviceFinder BedDeviceFinder = new UsbDeviceFinder(0x0483, 0xAB12);

        public static void Connection(string CommandName)
        {
            try
            {
                BedUsbDevice = UsbDevice.OpenUsbDevice(BedDeviceFinder); //контроллер кровати
                var x = BedUsbDevice.Info;
                var d = x.ProductString;
                // MessageBox.Show(d);
                var l = BedUsbDevice.ActiveEndpoints;
                if (BedUsbDevice != null)
                {
                    //MessageBox.Show("МЫ НАШЛИ ТЕБЯ");
                }
                //TestTest();
                var usbDevices = new UsbRegDeviceList();
                usbDevices = UsbDevice.AllDevices;
                usbDevices.FindAll(BedDeviceFinder);
                usbDevices.Count();
                IUsbDevice wholeUsbDevice;
                wholeUsbDevice = BedUsbDevice as IUsbDevice;
                if (!ReferenceEquals(wholeUsbDevice, null))
                {
                    wholeUsbDevice.SetConfiguration(1);
                    wholeUsbDevice.ClaimInterface(0);
                }
                UsbEndpointReader reader = BedUsbDevice.OpenEndpointReader(ReadEndpointID.Ep01);
                var writer = BedUsbDevice.OpenEndpointWriter(WriteEndpointID.Ep01);

                ErrorCode ec = ErrorCode.None;
                int bytesWritten;
                byte[] data = null;
                switch (CommandName)
                {
                    case "Start":
                        data = new byte[] { 0x20, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff };
                        break;
                    case "Pause":
                        data = new byte[] { 0x20, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff };
                        break;
                    case "Reverse":
                        data = new byte[] { 0x20, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff };
                        break;
                    case "EmergencyStop":
                        data = new byte[] { 0x20, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff };
                        break;
                }
                //byte[] data = {0x20, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff};
                ec = writer.Write(data, 2000, out bytesWritten);
                byte[] readBuffer = new byte[1024];
                String s = null;
                int bytesRead;
                // ec = reader.Read(readBuffer, 100, out bytesRead);
                // MessageBox.Show(ec.ToString());
                // ec = ErrorCode.None;
                // while (ec == ErrorCode.None)
                //  {

                // byte[] data = {0x10, 0x00, 0x01, 0xff, 0xff, 0xff, 0xff, 0xff};
                // ec = writer.Write(data, 2000, out bytesWritten);
                wholeUsbDevice.ClaimInterface(1);
                // If the device hasn't sent data in the last 100 milliseconds,
                // a timeout error (ec = IoTimedOut) will occur. 
                //ec = reader.Read(readBuffer, 2000, out bytesRead);
                //MessageBox.Show(ec.ToString());
                //if (bytesRead == 0) throw new Exception("No more bytes!");

                // Write that output to the console.
                // s = (Encoding.Default.GetString(readBuffer, 0, bytesRead));

                //    }
                // MessageBox.Show(s);
                // UsbSetupPacket packet = new UsbSetupPacket((byte)(UsbCtrlFlags.RequestType_Vendor |
                //  UsbCtrlFlags.Recipient_Device | UsbCtrlFlags.Direction_In), 2, (short)0, (short)0, (short)0);
                int countIn;
                /*byte[] data1  = {0x20, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff};
                if (BedUsbDevice.ControlTransfer(ref packet, data1, 1, out countIn) && (countIn !=0))
                {
                    MessageBox.Show("Прочитано значение");
                }*/
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                if (BedUsbDevice != null)
                {
                    if (BedUsbDevice.IsOpen)
                    {

                        IUsbDevice wholeUsbDevice = BedUsbDevice as IUsbDevice;
                        if (!ReferenceEquals(wholeUsbDevice, null))
                        {
                            // Release interface #0.
                            wholeUsbDevice.ReleaseInterface(0);
                        }

                        BedUsbDevice.Close();
                    }
                    BedUsbDevice = null;

                    // Free usb resources
                    UsbDevice.Exit();

                }
            }
        }
    }
}
