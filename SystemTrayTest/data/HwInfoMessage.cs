public class HwInfoMessage {
    public String t { get; set; }   // Message type
    public String c { get; set; }   // CPU Temperature
    public String g { get; set; }   // GPU Temperature
    public String p { get; set; }   // Power

    public HwInfoMessage(float cpuTemp, float gpuTemp, float power) {
        this.t = "hwinfo";
        this.c = cpuTemp.ToString("0.00");
        this.g = gpuTemp.ToString("0.00");
        this.p = power.ToString("0.00");
    }

    public override string ToString() {
        return "HwInfoMessage: {t:" + t + ", c:" + c + ", g:" + g + ", p:" + p + "}";
    }
}

