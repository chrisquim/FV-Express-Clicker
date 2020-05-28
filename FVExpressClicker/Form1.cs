using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.Threading;
using System.Media;

namespace ExpressClicker
{

    public partial class Form1 : Form
    {
        #region DLL Import
        [STAThread]
        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);
        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        const int hotkeyF1 = 1;
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);
        [DllImport("user32.dll")]
        static extern bool GetCursorPos(ref Point lpPoint);
        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int BitBlt(IntPtr hDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, int dwRop);

        #endregion

        #region Variables
        //Mouse actions
        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;
        
        Bitmap screenPixel = new Bitmap(1, 1, PixelFormat.Format32bppArgb);
        DateTime timeA;
        Thread clickThread;
        Thread timerThread;
        Thread colorThread;
        Form2 screenshotOverlay = new Form2();

        bool isF1active = false;
        bool isCounterActive = false;
        int countClick = 0;
        int millisec = 2;

        string stringA = "Position your cursor on the button to be repeatedly click then press F1 to begin.";
        string stringB = "Clicker activated. \rPress F1 to stop.";
        string stringC = "Select pixel on the screenshot to monitor.\rPress ESC to cancel.";
        string stringD = "Monitoring pixel";
        #endregion

        public void ClickerMethod()
        {
            Point cursor = new Point();
            GetCursorPos(ref cursor);
            var startColor = GetColorAt(cursor);
            var atmColor = startColor;
            while (startColor == atmColor  && isF1active)
            {
                Cursor.Position = cursor;
                DoMouseClick();
                textclick.Invoke((MethodInvoker)(() =>
                {
                    textclick.Text = countClick + " clicks";
                }));
                atmColor = GetColorAt(cursor);
                Thread.Sleep(millisec);
                Cursor.Position = cursor;
            }
            SystemSounds.Beep.Play();
            isF1active = false;
            label1.Invoke((MethodInvoker)(() =>
            {
                label1.Text = stringA;
            }));

        }
        public void TimerMethod()
        {
            while (isCounterActive)
            {
            TimeSpan duration;
            string stringduration;
            duration = DateTime.Now - timeA;
            stringduration = duration.ToString(@"hh\:mm\:ss");
            texttimer.Invoke((MethodInvoker)(() =>
            {
                texttimer.Text = stringduration;
            }));

            }

        }
        public void colorMethod()
        {

            while (GetColorAt(screenshotOverlay.monitorPoint) == screenshotOverlay.pinColor)
            {
                Thread.Sleep(500);
            }
            string clipdate = DateTime.Now.ToString();
            RunAsSTAThread(
            () =>
            {
                Clipboard.SetText(clipdate);
            });
            SoundPlayer player = new SoundPlayer(@"C:\Windows\Media\Alarm10.wav");
            player.PlayLooping();
            if (MessageBox.Show(clipdate).ToString() == "OK")
            {
                player.Stop();
            }

        }

        public void DoMouseClick()
        {
            //Call the imported function with the cursor's current position
            uint X = (uint)Cursor.Position.X;
            uint Y = (uint)Cursor.Position.Y;
            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, X, Y, 0, 0);
            countClick += 1;
        }
        public void DeactivateF1()
        {
            label1.Text = stringA;
        }
        public void PixelSensor()
        {
            label1.Text = stringC;
            label1.Refresh();
            screenshotOverlay.ShowDialog();
            label1.Text = stringD + screenshotOverlay.pinColor.ToString() ;
            colorThread = new Thread(colorMethod);
            colorThread.Start();
            //screenshotOverlay.pinColor
        }


        public Color GetColorAt(Point location)
        {
            using (Graphics gdest = Graphics.FromImage(screenPixel))
            {
                using (Graphics gsrc = Graphics.FromHwnd(IntPtr.Zero))
                {
                    IntPtr hSrcDC = gsrc.GetHdc();
                    IntPtr hDC = gdest.GetHdc();
                    int retval = BitBlt(hDC, 0, 0, 1, 1, hSrcDC, location.X, location.Y, (int)CopyPixelOperation.SourceCopy);
                    gdest.ReleaseHdc();
                    gsrc.ReleaseHdc();
                }
            }

            return screenPixel.GetPixel(0, 0);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x0312 && m.WParam.ToInt32() == hotkeyF1)
            {
                isF1active = !isF1active;
                if (isF1active)
                {
                    label1.Text = stringB;
                    timeA = DateTime.Now;
                    isCounterActive = true;
                    clickThread = new Thread(ClickerMethod);
                    clickThread.Start();
                   // MessageBox.Show(timerThread.ToString());
                    if (timerThread == null)
                    {
                        timerThread = new Thread(TimerMethod);
                        timerThread.Start();
                    }

                }
                else
                {
                    clickThread.Abort();
                    DeactivateF1();

                }
            }
            base.WndProc(ref m);
        }
        static void RunAsSTAThread(Action goForIt)
        {
            AutoResetEvent @event = new AutoResetEvent(false);
            Thread thread = new Thread(
                () =>
                {
                    goForIt();
                    @event.Set();
                });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            @event.WaitOne();
        }
        public Form1()
        {

            InitializeComponent();
            label1.Text = stringA;
            numericUpDown1.Controls[0].Visible = false;
            RegisterHotKey(this.Handle, hotkeyF1, 0, (int)Keys.F1);
            //Thread enddetectorThread = new Thread(enddetectorMethod);
            //enddetectorThread.Start();
        }
        private void ResetClickCounter(object sender, EventArgs e)
        {
            countClick = 0;
            textclick.Text = countClick + " clicks";
            textclick.Refresh();
        }
        private void ResetStopwatch(object sender, EventArgs e)
        {
            isCounterActive = false;            
            texttimer.Text = "00:00:00";
            //MessageBox.Show("inside");
            if (timerThread != null) { timerThread.Abort(); }
            texttimer.Refresh();
            texttimer.Text = "00:00:00";
            texttimer.Refresh();
            texttimer.Hide();
            texttimer.Show();
            //this.Refresh();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            PixelSensor();
            
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.LastWindowLocation = this.Location;
            Properties.Settings.Default.Save();
            if (timerThread != null){timerThread.Abort();}
            if (colorThread != null){colorThread.Abort();}
            screenshotOverlay.Close();
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                Hide();
            }
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.LastWindowLocation != null)
            {
                this.Location = Properties.Settings.Default.LastWindowLocation;
            }
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            Show();
            this.WindowState = FormWindowState.Normal;

        }
    }
}
