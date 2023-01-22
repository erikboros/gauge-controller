using System.ComponentModel;

namespace SystemTrayTest {
    public partial class Form1 : Form {

        private const int MillisecondsDelay = 1000;
        private bool runningUsbJobState = false;

        public Form1() {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e) {
            notifyIcon1.BalloonTipTitle= "GaugeController";
            notifyIcon1.BalloonTipText= "The application is running in the Notification Tray.";
            notifyIcon1.Text= "GaugeController";
            //this.WindowState = FormWindowState.Minimized;

            toggleJobState();
            //if (runningUsbJobState) {
            //    button1.Text = "Stop";
            //} else {
            //    button1.Text = "Start";
            //}
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e) {
            // Do not access the form's BackgroundWorker reference directly.
            // Instead, use the reference provided by the sender parameter.
            BackgroundWorker bw = sender as BackgroundWorker;

            bw.ReportProgress(1, "[INFO] Initializing hardware info...");
            HardwareInfoProvider infoProvider = new HardwareInfoProvider();
            bw.ReportProgress(1, "[INFO] hardware info ready");
            bw.ReportProgress(1, "[INFO] Initializing usb device...");
            UsbDeviceHandler usb = new UsbDeviceHandler();
            bw.ReportProgress(1, "[INFO] usb device ready");

            while (!bw.CancellationPending) {
                HwInfoMessage infoMessage = infoProvider.getHwInfoMessage();
                usb.sendHwInfo(infoMessage);

                bw.ReportProgress(2, infoMessage.ToString());

                Thread.Sleep(MillisecondsDelay);
            }
        }

        private void BackgroundWorkerOnProgressChanged(object sender, ProgressChangedEventArgs e) {
            object userObject = (String) e.UserState;
            int percentage = e.ProgressPercentage;

            //if (percentage == 1) {
            //    label3.Text = "Starting...";
            //} else if (percentage == 2) {
            //    label3.Text = "Running...";
            //}

            listBox1.Items.Add(userObject);
            int visibleItems = listBox1.ClientSize.Height / listBox1.ItemHeight;
            listBox1.TopIndex = Math.Max(listBox1.Items.Count - visibleItems + 1, 0);
        }

        private void showToolStripMenuItem_Click(object sender, EventArgs e) {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            notifyIcon1.Visible = false;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e) {
            notifyIcon1.Visible = false;
            Application.Exit();
        }

        private void Form1_Resize(object sender, EventArgs e) {
            if (this.WindowState == FormWindowState.Minimized) {
                this.Hide();
                notifyIcon1.Visible = true;
                //notifyIcon1.ShowBalloonTip(1000);
            } else if(this.WindowState == FormWindowState.Normal) {
                notifyIcon1.Visible = false;
            }
        }

        private void notifyIcon1_MouseDoubleClick_1(object sender, MouseEventArgs e) {
            this.Show();
            this.WindowState= FormWindowState.Normal;
            notifyIcon1.Visible = false;
        }

        private void button1_Click(object sender, EventArgs e) {
            toggleJobState();
        }

        private void toggleJobState() {
            if (!runningUsbJobState) {
                this.backgroundWorker1.RunWorkerAsync(); //TODO crashes if stopped when usb device has not yet been found.
                button1.Text= "Stop";
                label3.Text = "Started...";
                runningUsbJobState = true;
            } else {
                this.backgroundWorker1.CancelAsync();
                button1.Text = "Start";
                label3.Text = "Stopped...";
                runningUsbJobState = false;
            }
        }
    }
}