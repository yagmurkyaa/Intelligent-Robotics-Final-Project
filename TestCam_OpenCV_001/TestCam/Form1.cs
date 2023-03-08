using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.Util;
using Emgu.CV.Structure;
using System.IO.Ports;

namespace TestCam
{
    public partial class Form1 : Form
    {
        private Capture capture;
        private Image<Bgr, Byte> IMG;
        private Image<Gray, Byte> GrayImg;
        private Image<Gray, Byte> BWImg;
        private double myScale= 128/640;
        private int Xpx, Ypx, N;
        private double Xcm, Ycm; //Px, Py
        double Zcm = 100.0;

        static SerialPort _serialPort;
		public byte []Buff = new byte[2];
        

//(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)
//(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)
        
        
        public Form1()
        {
            InitializeComponent();
            myScale= 128/640; // cm/px


            _serialPort = new SerialPort();
            _serialPort.PortName = "COM3";//Set your board COM
            _serialPort.BaudRate = 9600;
            _serialPort.Open();	
        }
        
//(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)
//(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)
        private void processFrame(object sender, EventArgs e)
        {
            if (capture == null)//very important to handel excption
            {
                try
                {
                    capture = new Capture(0); 
                }
                catch (NullReferenceException excpt)
                {
                    MessageBox.Show(excpt.Message);
                }
            }

            IMG = capture.QueryFrame();         
            GrayImg = IMG.Convert<Gray, Byte>();
            BWImg = GrayImg.ThresholdBinaryInv(new Gray(20), new Gray(255));
            
            Xpx = 0;                              //How to specify center of image
            Ypx = 0;
            N = 0;
            for (int i=0;i<BWImg.Width;i++)
            {for (int j=0;j<BWImg.Height; j++)
            {
                 if(BWImg[j,i].Intensity>128)
                 {
                     N++;
                     Xpx+=i;
                     Ypx+=j;
                 }
            	}  
            }                                   


            if (N > 0)
            {
                Xpx = Xpx/N;
                Ypx = Ypx/N;
                
                Xpx =Xpx- BWImg.Width / 2 ;    
                Ypx =  BWImg.Height / 2- Ypx;
                
                Xcm = Xpx * myScale;  
                Ycm = Ypx * myScale;

                textBox1.Text = Xcm.ToString();
                textBox2.Text = Ycm.ToString();
                textBox3.Text = N.ToString();
                
                //INVERSE Kınematic Model
                double d1 = 5.0;

                double Px = -Zcm, Py = -Xcm, Pz = -Ycm;

                double Th1 = Math.Atan(Py / Px);  //Real Domain
                double Th2 = Math.Atan(Math.Sin(Th1) * (Pz - d1) / Py);

                //double d3 = ((Zcm - d1) / Math.Sin(Th2)) - l2;

                Th1 = (Th1 * (180 / Math.PI));
                Th2 = (Th2 * (180 / Math.PI));
                Th1 = Th1 + 90; //ServoMotor Domain
                Th2 = Th2 + 90; ;


                Buff[0] = (byte)Th1;  //Th1
                Buff[1] = (byte)Th2;  //Th2 
                _serialPort.Write(Buff, 0, 2);

            }
            else
            {

                textBox1.Text = "";
                textBox2.Text = "";
                textBox3.Text = N.ToString();
               
                Buff[0] = 90;  //Th1
                Buff[1] = 90;  //Th2 
                _serialPort.Write(Buff,0,2);
            }
      
            try
            {
                
                imageBox1.Image = IMG;
                imageBox2.Image = GrayImg;
                imageBox3.Image = BWImg;
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

//(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)
//(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)
		void Timer1Tick(object sender, EventArgs e)
		{
			processFrame( sender, e);
		} 
		
        private void button1_Click(object sender, EventArgs e)
        {
            //Application.Idle += processFrame;
            timer1.Enabled = true;
            button1.Enabled = false;
            button2.Enabled = true;
        }
//(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)
//(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)
        private void button2_Click(object sender, EventArgs e)
        {
            //Application.Idle -= processFrame;
            timer1.Enabled = false;
            button1.Enabled = true;
            button2.Enabled = false;
        }    
//(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)
//(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)
        private void button3_Click(object sender, EventArgs e)
        {
            IMG.Save("\\0000\\Image" +  ".jpg");
        }
      
//(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)
//(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)
        
    }
}