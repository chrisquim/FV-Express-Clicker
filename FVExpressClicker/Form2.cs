using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace ExpressClicker
{
    public partial class Form2 : Form
    {
        [DllImport("user32.dll")]
        static extern bool GetCursorPos(ref Point lpPoint);
        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int BitBlt(IntPtr hDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, int dwRop);

        Bitmap captureBitmap = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, PixelFormat.Format32bppArgb);
        Bitmap screenPixel = new Bitmap(1, 1, PixelFormat.Format32bppArgb);
        public Color pinColor = new Color();
        public Point monitorPoint = new Point();
        public Form2()
        {
            //GlobalMouseHandler gmh = new GlobalMouseHandler();
            //gmh.TheMouseMoved += new MouseMovedEvent(gmh_TheMouseMoved);
            //Application.AddMessageFilter(gmh);
            this.BackgroundImage = captureBitmap;
            InitializeComponent();
            this.Width = Screen.PrimaryScreen.Bounds.Width;
            this.Height = Screen.PrimaryScreen.Bounds.Height;

        }
        //void gmh_TheMouseMoved()
        //{
        //    Point cur_pos = System.Windows.Forms.Cursor.Position;
        //    Point curbox = new Point(Cursor.Position.X+5,Cursor.Position.Y+5);
        //    pinColor = GetColorAt(cur_pos);
        //    label1.BackColor = pinColor;
        //    label1.ForeColor = ContrastColor(pinColor);
        //    button1.Location = curbox;
        //}
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                pinColor = Color.Empty;
                this.Close();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
        Color ContrastColor(Color color)
        {
            int d = 0;

            // Counting the perceptive luminance - human eye favors green color... 
            double luminance = (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255;

            if (luminance > 0.5)
                d = 0; // bright colors - black font
            else
                d = 255; // dark colors - white font

            return Color.FromArgb(d, d, d);
        }
        private void CaptureMyScreen()

        {

            try

            {

                //Creating a new Bitmap object



                //Bitmap captureBitmap = new Bitmap(int width, int height, PixelFormat);

                //Creating a Rectangle object which will  

                //capture our Current Screen

                Rectangle captureRectangle = Screen.AllScreens[0].Bounds;



                //Creating a New Graphics Object

                Graphics captureGraphics = Graphics.FromImage(captureBitmap);



                //Copying Image from The Screen

                captureGraphics.CopyFromScreen(captureRectangle.Left, captureRectangle.Top, 0, 0, captureRectangle.Size);



                //Saving the Image File (I am here Saving it in My E drive).

                //captureBitmap.Save(@"E:\Capture.jpg", ImageFormat.Jpeg);



                //Displaying the Successfull Result



                //MessageBox.Show("Screen Captured");

            }

            catch (Exception ex)

            {

                MessageBox.Show(ex.Message);

            }

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

        private void Form2_MouseClick(object sender, MouseEventArgs e)
        {
            Point cur_pos = System.Windows.Forms.Cursor.Position;
            pinColor = GetColorAt(cur_pos);
            monitorPoint = cur_pos;
            this.Hide();
        }

        private void Form2_MouseMove(object sender, MouseEventArgs e)
        {
            Point cur_pos = System.Windows.Forms.Cursor.Position;
            Point curbox = new Point(Cursor.Position.X + 5, Cursor.Position.Y + 5);
            pinColor = GetColorAt(cur_pos);
            //label1.BackColor = pinColor;
            //label1.ForeColor = ContrastColor(pinColor);
            //label1.Location = curbox;
            //toolTip1.Show("", this, curbox);

            //label1.Location = curbox;
            //Graphics g = CreateGraphics();
            //Pen selPen = new Pen(Color.Blue);
            //this.BackgroundImage = captureBitmap;
            //g.DrawRectangle(selPen, curbox.X, curbox.Y, 10, 10);
            //g.Dispose();
        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }

        private void Form2_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible)
            {
                CaptureMyScreen();
            }
        }
    }
    //public delegate void MouseMovedEvent();
    //public class GlobalMouseHandler : IMessageFilter
    //{
    //    private const int WM_MOUSEMOVE = 0x0200;

    //    public event MouseMovedEvent TheMouseMoved;

    //    #region IMessageFilter Members

    //    public bool PreFilterMessage(ref Message m)
    //    {
    //        if (m.Msg == WM_MOUSEMOVE)
    //        {
    //            if (TheMouseMoved != null)
    //            {
    //                TheMouseMoved();
    //            }
    //        }
    //        // Always allow message to continue to the next filter control
    //        return false;
    //    }

    //    #endregion
    //}
}
