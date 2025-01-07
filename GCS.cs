using GMap.NET;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using System.Threading;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

/*일반 차량 제어 프로세스
• 지도 표시 인터페이스
• 경고표시장치
• 페이로드 제어 프로세스
• 페이로드 비디오 디스플레이 시스템
• 임무선택및적재능력
• 일반 데이터 링크 제어
• CUCS 차량 고유 메커니즘*/


namespace ControlStation
{
    public partial class GCS : Form
    {
        public static bool LeftRepiar = false;
        public static bool RightRepiar = true;
        
        public static int TestVal = 0;
        public int ImgIdx = 0;
        Thread T1;

        
        VSM_Conversion.VSM vsm;
        private int vsmflag= 0;
        public GMapOverlay MarkerOverlay = new GMapOverlay("markers");
        Image[] im;
        public GCS()
        {
            InitializeComponent();
            vsm = new VSM_Conversion.VSM();
            vsm.Show();
            vsm.Visible = false;
            //pictureBox1.Visible = false; //초반에 pictureBox 비활성화
            // msg = new VSM_Conversion.MSG_VSM();

            this.KeyPreview = true;
            Map map = new Map(gMapControl1, pictureBox1);

            Bitmap imgbitmap = new Bitmap(imageList1.Images[17]);
            Image resizedImage = resizeImage(imgbitmap, 600, 360);

            pictureBox1.Image = resizedImage;


            im = new Image[imageList1.Images.Count];
            //kbs
            for(int i = 0; i < imageList1.Images.Count; ++i)
            {
                im[i] = ResizeImage(new Bitmap(imageList1.Images[i]), 600, 360);
            }



            Thread gps = new Thread(gpsStart);
            Thread mpu = new Thread(mpuStart);
            //gps.Start();
            mpu.Start();
            DoTest();
            Maps();
        }
        private void gpsStart()
        {
            while (true)
            {
                VSM_Conversion.MSG_VSM.reqGPS();
                Thread.Sleep(100); 
            }
        }
        private void mpuStart()
        {
            while (true)
            {
                VSM_Conversion.MSG_VSM.reqControlState();               
                Thread.Sleep(300);
            }
        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Left:

                    if (TestVal == -55)
                    {

                    }
                    else
                    {
                        TestVal -= 1;
                    }
                    LeftRepiar = true;
                    RightRepiar = false;

                    return true;

                case Keys.Right:

                    if (TestVal == 52)
                    {

                    }
                    else
                    {
                        TestVal += 1;
                    }
                    LeftRepiar = false;
                    RightRepiar = true;

                    return true;
            }


            return base.ProcessCmdKey(ref msg, keyData);
        }
        private static DateTime Delay(int MS)
        {
            DateTime ThisMoment = DateTime.Now;
            TimeSpan duration = new TimeSpan(0, 0, 0, 0, MS);
            DateTime AfterWards = ThisMoment.Add(duration);

            while (AfterWards >= ThisMoment)
            {
                System.Windows.Forms.Application.DoEvents();
                ThisMoment = DateTime.Now;
            }

            return DateTime.Now;
        }

        

        private Bitmap RotateImage(Bitmap bmp, double angle)
        {
            Bitmap rotatedImage = new Bitmap(bmp.Width, bmp.Height);
            using (Graphics g = Graphics.FromImage(rotatedImage))
            {
                g.TranslateTransform((float)bmp.Width / 2, (float)bmp.Height / 2);
                g.RotateTransform((float)angle);
                g.TranslateTransform(-(float)bmp.Width / 2, -(float)bmp.Height / 2);
                g.DrawImage(bmp, new Point(0, 0));
            }
            return rotatedImage;
        }
        public void imgViewMethod() //UA 자세 표시
        {
            /*
            VSM_Conversion.MSG_VSM.reqControlState();
            int old_max = 80;
            int old_min = -100;
            int new_max = 50;
            int new_min = -50;
            double gyroValue = ((double)(new_max - new_min) / (old_max - old_min)) * (VSM_Conversion.MSG_VSM.getRoll() - old_min) + new_min;

            Console.WriteLine("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ " + gyroValue);
            int[] gyroRanges = { -52, -49, -46, -43, -40, -37, -34, -31, -28, -25, -22, -19, -16, -13, -10, -7, -4, -1, 2, 5, 8, 11, 14, 17, 20, 23, 26, 29, 32, 48 };
            int index = -1;

            for (int i = 0; i < gyroRanges.Length; i++)
            {
                if (gyroValue >= gyroRanges[i] && gyroValue <= gyroRanges[i] + 2)
                {
                    index = i;
                    break;
                }
            }

            if (index != -1)
            {
                pictureBox1.Image = im[index];
                pictureBox1.Refresh();
            }*/


            try
            {
                double angle = (double)(VSM_Conversion.MSG_VSM.getRoll());
                int index3 = getImageIndex(angle);
                Bitmap imgbitmap = new Bitmap(imageList1.Images[index3]);
                Image resizedImage = ResizeImage(imgbitmap, 600, 360);
                pictureBox1.Image = resizedImage;

            }
            catch (Exception e7)
            {
                Console.WriteLine("error");
            }


        }

        public static int getImageIndex(double angle)
        {
            int index = 17 - (int)(angle / 10.0 * 1.8);

            if (index < 0)
            {
                index += 36;
            }
            if (index >= 36)
            {
                index -= 36;
            }

            return index;
        }

        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        public void Run()
        {
            try
            {
                while (true)
                {
                    if (this.InvokeRequired)
                    {
                        Delay(500);
                        this.Invoke(new MethodInvoker(delegate ()
                        {
                            imgViewMethod();
                        }));
                    }

                }
            }
            catch(Exception e3)
            {
                Console.WriteLine("error");
            }
            

        }
        public void DoTest()
        {
            T1 = new Thread(new ThreadStart(Run));
            T1.Start();
        }

        public Image resizeImage(Image image, int width, int height)
        {
            var destinationRect = new Rectangle(0, 0, width, height);
            var destinationImage = new Bitmap(width, height);

            destinationImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destinationImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destinationRect, 0, 0, image.Width, image.Height,
                                       GraphicsUnit.Pixel, wrapMode);
                }
            }

            return (Image)destinationImage;
        }

       

        public void Maps()
        {
            PointLatLng p = new PointLatLng(36.107545500000000, 128.33299255371093);

            gMapControl1.MapProvider = GMapProviders.GoogleMap;
            gMapControl1.Overlays.Add(MarkerOverlay);
            gMapControl1.Position = p; // 드론 현재 위치 값 받아와 넣기
            gMapControl1.MinZoom = 2;
            gMapControl1.MaxZoom = 20;
            gMapControl1.Zoom = 15;

        }

       
        private void netWorkSet_Click(object sender, EventArgs e)
        {
            drone_control_test.NetWorkSetting s =  new drone_control_test.NetWorkSetting();
            s.Owner = this;
            s.Show();
        }

        private void on_Click(object sender, EventArgs e)
        {
            VSM_Conversion.MSG_VSM.moterOn();
        }

        private void off_Click(object sender, EventArgs e)
        {
            VSM_Conversion.MSG_VSM.moterOff();
        }

        private void throttle_up_Click(object sender, EventArgs e)
        {
            VSM_Conversion.MSG_VSM.throttleUp();
        }

        private void throttle_down_Click(object sender, EventArgs e)
        {
            VSM_Conversion.MSG_VSM.throttleDown();
        }

        private void VSMWindow_Click(object sender, EventArgs e)
        {
            if(vsmflag == 0)
            {
                vsm.Visible = true;
                vsmflag = 1;
                VSMWindow.Text = "VSM Display Off";
                
            }
            else
            {
                vsm.Visible = false;
                vsmflag = 0;
                VSMWindow.Text = "VSM Display On";
            }
                   
        }
    }
}
