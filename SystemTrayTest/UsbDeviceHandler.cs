using HidLibrary;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SystemTrayTest {
    internal class UsbDeviceHandler {
        private HidDevice? teensy;
        private short inputReportByteLength; // should be 65
        public UsbDeviceHandler() {
            //Console.WriteLine("[INFO] Connecting to Teensy... ");
            teensy = getTeensy();
            while (teensy == null || !teensy.IsConnected) {
                teensy = getTeensy();
                Thread.Sleep(1000);
                //Console.Write(".");
            }

            inputReportByteLength = teensy.Capabilities.InputReportByteLength;
            //Console.WriteLine();
            //Console.WriteLine("[INFO] Connected: usage:" + teensy.Capabilities.Usage + ", InputReportByteLength:" + teensy.Capabilities.InputReportByteLength);

        }

        public void sendHwInfo(HwInfoMessage message) { //TODO use just first,second,third names?
            string json = JsonSerializer.Serialize(message); // TODO why

            if (validateString(json)) {

                HidReport report = teensy.CreateReport();
                for (int i = 0; i < json.Length; i++) {
                    report.Data[i] = (byte)json[i];
                }
                teensy.WriteReport(report);
                Console.WriteLine("[DEBUG] sent: " + json);
            }
        }

        public void sendString(String data) {
            HidReport report = teensy.CreateReport();
            for (int i = 0; i < data.Take(63).Count(); i++) {
                report.Data[i] = (byte)data[i];
            }
            teensy.WriteReport(report);
        }

        private bool validateString(String message) {
            bool valid = message.Length < inputReportByteLength;
            if (!valid) {
                Console.WriteLine("[ERROR] String longer than " + inputReportByteLength + ": " + message);
            }
            return valid;
        }

        private static HidDevice? getTeensy() {
            var teensy = HidDevices.Enumerate(0x16C0, 0x0486)       // 0x486 -> usb type raw hid
                        .Where(d => d.Capabilities.Usage == 0x200)  // usage 0x200 -> RawHID usage
                        .FirstOrDefault();                          // take the first one we find on the USB bus
            return teensy;
        }

        [Obsolete]
        public void sendTestData() {
            for (int loops = 0; loops < 10; loops++)               // don't read too much data from the web page...
            {
                String slip = new WebClient().DownloadString(@"https://api.adviceslip.com/advice");  // read a slip from adviceslip.com
                String advice = (string)JsonValue.Parse(slip)["slip"]["advice"];                             // extract the advice from the json result

                HidReport report = teensy.CreateReport();        // Generate an empty report
                for (int i = 0; i < advice.Take(63).Count(); i++) // and copy the characters into it.
                {                                                // limit to 63 bytes to ensure the report always has a EOS (\0) at the end
                    report.Data[i] = (byte)advice[i];
                }
                teensy.WriteReport(report);                      // send report to teensy
                //Console.WriteLine(advice);                       // echo on console
            }
        }


        [Obsolete]
        public void sendHwInfo(float cpuTemp, float gputemp, float power) { //TODO use just first,second,third names?
            HwInfoMessage message = new HwInfoMessage(cpuTemp, gputemp, power);
            string json = JsonSerializer.Serialize(message); // TODO why
            //Console.WriteLine(json);

            if (validateString(json)) {

                HidReport report = teensy.CreateReport();
                for (int i = 0; i < json.Length; i++) {
                    report.Data[i] = (byte)json[i];
                }
                teensy.WriteReport(report);
                //Console.WriteLine("[DEBUG] sent: " + json);
            }
        }


    }

}
