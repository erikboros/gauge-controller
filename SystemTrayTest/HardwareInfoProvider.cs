using LibreHardwareMonitor.Hardware;

namespace SystemTrayTest {
    public class UpdateVisitor : IVisitor {
        public void VisitComputer(IComputer computer) {
            computer.Traverse(this);
        }
        public void VisitHardware(IHardware hardware) {
            hardware.Update();
            foreach (IHardware subHardware in hardware.SubHardware) subHardware.Accept(this);
        }
        public void VisitSensor(ISensor sensor) { }
        public void VisitParameter(IParameter parameter) { }
    }

    public class HardwareInfoProvider {
        private Computer computer;
        private HardwareType gpuType;
        public HardwareInfoProvider() {
            Console.WriteLine("[INFO] Init HardwareMonitor...");

            computer = new Computer();
            computer.IsCpuEnabled = true;
            computer.IsGpuEnabled = true;

            //init GPU type
            if (getFirstMatchingHwInfo(HardwareType.GpuNvidia, SensorType.Temperature) != 0.0f) {
                gpuType = HardwareType.GpuNvidia;
            }
            else if (getFirstMatchingHwInfo(HardwareType.GpuAmd, SensorType.Temperature) != 0.0f) {
                gpuType = HardwareType.GpuAmd;
            }
            else if (getFirstMatchingHwInfo(HardwareType.GpuIntel, SensorType.Temperature) != 0.0f) {
                gpuType = HardwareType.GpuIntel;
            } //TODO none of these error handling
            Console.WriteLine("[INFO] GPU found: " + gpuType.ToString());

        }
        public HwInfoMessage getHwInfoMessage() {
            try {
                computer.Open();
            }
            catch {
                Console.WriteLine("[ERROR] failed to open hardware monitor");
                return null;
            }
            computer.Accept(new UpdateVisitor());

            float cpuTemp = 0.0f;
            float gpuTemp = 0.0f;
            float cpuPower = 0.0f;
            float gpuPower = 0.0f;
            bool cpuTempFound = false;
            bool gpuTempFound = false;
            bool cpuPowerFound = false;
            bool gpuPowerFound = false;

            foreach (IHardware hardware in computer.Hardware) {
                if (hardware.HardwareType == HardwareType.Cpu) {
                    foreach (ISensor sensor in hardware.Sensors) {
                        if (sensor.SensorType == SensorType.Temperature && sensor.Name == "Core (Tctl/Tdie)") {
                            cpuTemp = sensor.Value.Value;
                            cpuTempFound = true;
                        } else if (sensor.SensorType == SensorType.Power && sensor.Name == "Package") {
                            cpuPower = sensor.Value.Value;
                            cpuPowerFound = true;
                        }
                        if (cpuTempFound && cpuPowerFound) {
                            break;
                        }
                    }
                } else if (hardware.HardwareType == gpuType) {
                    foreach (ISensor sensor in hardware.Sensors) {
                        if (sensor.SensorType == SensorType.Temperature && sensor.Name == "GPU Core") {
                            gpuTemp = sensor.Value.Value;
                            gpuTempFound = true;
                        } else if (sensor.SensorType == SensorType.Power && sensor.Name == "GPU Package") {
                            gpuPower = sensor.Value.Value;
                            gpuPowerFound = true;
                        }
                        if (gpuTempFound && gpuPowerFound) {
                            break;
                        }
                    }
                }
            }
            computer.Close();

            return new HwInfoMessage(cpuTemp, gpuTemp, cpuPower + gpuPower);    //TODO Eh //throw HwNotfoundException;?
        }
        
        [Obsolete]
        public float getCPUTemperature() {
            return getFirstMatchingHwInfo(HardwareType.Cpu, SensorType.Temperature);
        }
        
        [Obsolete]
        public float getGPUTemperature() {
            return getFirstMatchingHwInfo(gpuType, SensorType.Temperature);
        }
        
        [Obsolete]
        public float getCPUPower() {
            return getFirstMatchingHwInfo(HardwareType.Cpu, SensorType.Power);
        }
        
        [Obsolete]
        public float getGPUPower() {
            return getFirstMatchingHwInfo(gpuType, SensorType.Power);
        }
        
        private float getFirstMatchingHwInfo(HardwareType hwType, SensorType sensorType) {
            try {
                computer.Open();
            }
            catch {
                Console.WriteLine("[ERROR] failed to open hardware monitor");
                return 0.0f;
            }
            computer.Accept(new UpdateVisitor());

            foreach (IHardware hardware in computer.Hardware) {
                if (hardware.HardwareType == hwType) {
                    foreach (ISensor sensor in hardware.Sensors) {
                        if (sensor.SensorType == sensorType) {
                            //Console.WriteLine("\t\tSensor: {0}, value: {1}", sensor.Name, sensor.Value);
                            return sensor.Value.Value;
                        }
                    }
                }
            }
            computer.Close();
            return 0.0f;    //TODO Eh //throw HwNotfoundException;

        }


        public void listAll() {
            try {
                computer.Open();
            }
            catch {
                Console.WriteLine("### fail to open hardware monitor ###");
                return;
            }
            computer.Accept(new UpdateVisitor());

            foreach (IHardware hardware in computer.Hardware) {
                Console.WriteLine("Hardware: {0}", hardware.Name);

                foreach (IHardware subhardware in hardware.SubHardware) {
                    Console.WriteLine("\tSubhardware: {0}", subhardware.Name);

                    foreach (ISensor sensor in subhardware.Sensors) {
                        Console.WriteLine("\t\tSensor: {0}, value: {1}", sensor.Name, sensor.Value);
                    }
                }

                foreach (ISensor sensor in hardware.Sensors) {
                    Console.WriteLine("\tSensor: {0}, value: {1}", sensor.Name, sensor.Value);
                }
            }

            computer.Close();
        }

    }
}
