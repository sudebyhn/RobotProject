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
using Emgu.CV.UI;
using System.Globalization;
using System.Threading;
using System.Runtime.Remoting.Messaging;
using System.IO.Ports;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;


namespace TestCam
{
    public partial class Form1 : Form
    {
        private Capture capture;
        private Image<Bgr, Byte> IMG;
        private Image<Gray, Byte> R_frame;
        private Image<Gray, Byte> G_frame;
        private Image<Gray, Byte> B_frame;
        private Image<Gray, Byte> GrayImg;
        private Image<Gray, Byte> main_BW;
        static SerialPort _serialPort;
        public byte[] Buff = new byte[3];//changed it to 3 because we will be sending 3 bytes now 
        //here we write the variables
        public double l1, d1, d2, l2;//
        public double th1, th2, d3;//constant values
        public double Xcm, Ycm;// from image processing modifiy from process frame
        public double Px, Py, Pz;
        
        //()()()()()()()()()()()()()()()()()()()()()()()()
        public Form1()
        {
            InitializeComponent();
            _serialPort = new SerialPort();
            _serialPort.PortName = "COM5";//Set your board COM
            _serialPort.BaudRate = 9600;
            _serialPort.Open();

            l1 = 4;
            d1 = 5;
            l2 = 4;
        }
        private void processFrame(object sender, EventArgs e)
        {
            if (capture == null)
            {
                try
                {
                    capture = new Capture();//capture number() 


                }
                catch (NullReferenceException excpt)//show us eror
                {
                    MessageBox.Show(excpt.Message);
                }
            }
           // Application.Idle = processFrame(); if Idle+ loop kepp working background 

            IMG = capture.QueryFrame();//capture =new capture(0) if we want to add new capture
                                       //IMG class for capture images
            R_frame = IMG[2].Copy();
            G_frame = IMG[1].Copy();
            B_frame = IMG[0].Copy();
            GrayImg = IMG.Convert<Gray, Byte>();
            Image<Gray, Byte> BW = GrayImg.ThresholdBinaryInv(new Gray(hScroll_TH.Value), new Gray(255));
            Image<Gray, Byte> main_BW = BW.ThresholdBinary(new Gray(hScroll_TH.Value), new Gray(255));
           /* Image<Gray, Byte> corrodedImage = Corrosion(BW, hScroll_Corro.Value);
            MCvMoments moments = BW.GetMoments(true);


          
            double moment10 = moments.m10;
            double moment01 = moments.m01;
            double area = moments.m00;

            if (area != 0)
            {
                double normalizedMoment10 = moment10 / area;
                double normalizedMoment01 = moment01 / area;

                int x = (int)Math.Round(normalizedMoment10);
                int y = (int)Math.Round(normalizedMoment01);

                // Draw a circle at the accurate centroid on the corrodedImage
                CircleF circle = new CircleF(new PointF(x, y), 5);
                BW.Draw(circle, new Gray(128), 10);//grey and 10 brms thickness
            }*/
            try
            {
                // Display original image
                imageBox1.Image = IMG;

                // Display grayscale image
                imageBox2.Image = GrayImg;

                // Display individual color channels
                imageBox3.Image = main_BW;

                // Display the corroded image in the new ImageBox
                //imageBox4.Image = corrodedImage;

                //  display to find the center of gravity 
                Point cog = CalculateCOG(BW);
                int cogX = cog.X;
                int cogY = cog.Y;

                textBox_COGX.Text = $"COG X: ({cogX})";
                textBox_COGY.Text = $"COG Y: ({cogY})";

                Xcm = cogX;
                Ycm = cogY;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        // Set a fixed value for R

        // Perform corrosion operation

        // Display the corroded image in the new ImageBox
        private Image<Gray, Byte> Corrosion(Image<Gray, Byte> BW_old, int R)
        {
            Image<Gray, Byte> BW_New = BW_old.CopyBlank();

            for (int r = 0; r < R; r++)
            {
                BW_New = BW_old;

                int x, y;
                int count = 0;

                for (int i = 1; i < BW_old.Width - 1; i++)
                {
                    for (int j = 1; j < BW_old.Height - 1; j++)
                    {

                        if (BW_old[j, i].Intensity == 0.0)//if it 1 nothing to do
                        {
                            for (x = i - 1; (x <= i + 1) && (count == 0); x++)
                            {
                                for (y = j - 1; (y <= j + 1) && (count == 0); y++)
                                {
                                    if (BW_old[y, x].Intensity == 255)
                                        count++;
                                }
                            }

                            if (count == 0)
                            {
                                BW_New.Data[j, i, 0] = 0;
                            }

                        }
                    }
                }

                BW_New = BW_old.Copy();
            }
            return BW_New;
        }

        public Point CalculateCOG(Image<Gray, byte> BW)
        {
            double Scale = 70 / 196;
            //double YScale = ...
           
            double sumX = 0.0;
            double sumY = 0.0;
            double totalIntensity = 0.0;

            for (int y = 0; y < BW.Height *4 / 5; y++) //we calculsting here pixel
            {
                for (int x = 0; x < BW.Width * 4/ 5; x++)
                {
                    double intensity = BW[y, x].Intensity;

                    // Assuming the object in the corroded image is represented by non-zero intensity
                    if (intensity > 0)
                    {
                        sumX += x * intensity;
                        sumY += y * intensity;
                        totalIntensity += intensity;
                    }
                }
            }
            int cogX = (int)(sumX / totalIntensity);
            int cogY = (int)(sumY / totalIntensity);

            double Xc = -((IMG.Width / 2) - cogX); //
            double Yc = -(cogY - (IMG.Height / 2));
            double Xcm = Xc * Scale;
            double Ycm = Yc * Scale;   //we set xcm and ycm  for ımage center
            this.Xcm = Xcm;
            this.Ycm = Ycm;
            return new Point((int)Xcm, (int)Ycm); //we returned it as an object of type point 
        }
        private void Button1_Click(object sender, EventArgs e) //start button
        {
            Application.Idle += processFrame; // if add Idle+ loop kepp working background
            button1.Enabled = false;
            button2.Enabled = true;
        }

        private void Button2_Click(object sender, EventArgs e)//stop button 
        {
            Application.Idle -= processFrame;
            button1.Enabled = true;
            button2.Enabled = false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            IMG.Save("Image" + ".jpg"); //record the image  TO SAVE

        }

        private void hScroll_TH_Scroll(object sender, ScrollEventArgs e)
        {
            textBox_TH.Text = hScroll_TH.Value.ToString();
        }

        private void hScroll_Corro_Scroll(object sender, ScrollEventArgs e)
        {
            textBox_Corro.Text = hScroll_Corro.Value.ToString();
        }

        private void textBox_COGX_TextChanged(object sender, EventArgs e)
        {

        }

       

        void Button1Click(object sender, EventArgs e)
        {

            Buff[0] = 1; //Th1 //first eleman 
            Buff[1] = 0;//th2//second eleman
            _serialPort.Write(Buff, 0, 3); //0 means starting point 3 number of element
        }

        void Button2Click(object sender, EventArgs e)
        {

            Buff[0] = 0;//th1 for ardunio
            Buff[1] = 1;//Th2
            _serialPort.Write(Buff, 0, 3);// 0 represents the starting point, and 2 represents the the number of bytes that 
        }

        private void send_Click(object sender, EventArgs e)//send button 
        {

            int text1; //sending the th1 value 
            int.TryParse(textBox_theta1.Text, out text1);
            Buff[0] = (byte)(90 - text1);



            int text2;//sending the th2 value 
            int.TryParse(textBox_theta2.Text, out text2);
            Buff[1] = (byte)(90 - text2);

            if (checkBox1.Checked == false)
            {
                Buff[2] = 1; //run 

            }
            else
            {
                Buff[2] = 0;//stop
            }
            _serialPort.Write(Buff, 0, 3);
        }

        private void timer3_Tick(object sender, EventArgs e)
        {

            timer3.Enabled = false;
            Buff[2] = 0; //buff is sending the value and giving parameter


            _serialPort.Write(Buff, 0, 3);
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            timer2.Enabled = false;//until start timer it stop 
            timer3.Enabled = true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;////until start timer it stop 
            Buff[2] = 1; //3. elemans is 1
            _serialPort.Write(Buff, 0, 3);
            timer2.Enabled = true;
        }

        private void button4_Click(object sender, EventArgs e)//shoot button
        {
            //testing
            Buff[0] = 45;
            //testing
            Buff[1] = 45;

            
            timer1.Enabled = true;//run
        }
        //sude
        private void button5_Click(object sender, EventArgs e)
        {
            //sude
            double ds =3 ;
            double S=16;
            double Px = 160 -ds;//1-2 cm aşağı gir 
            this.Px = Px;
            double Py = this.Xcm;
            this.Py = Py;
            double Pz = this.Ycm +S;
            this.Pz = Pz;
            th1 = Math.Atan2(Px, Py);
            double Px1 = Px - l1 * Math.Cos(th1);
            double Py1 = Py - l1 * Math.Sin(th1);
            double Pz1 = Pz + d1; ;
           
            double temp = Math.Atan2(Px1, Pz1);
            th2 = temp;
            
            double th1_degree = (th1 * 180.0 / Math.PI);//we  converted degree 
            if (th1_degree < 0) th1_degree = 0;

            else if (th1_degree > 180) th1_degree = 180;


            double th2_degree = (th2 * 180 / Math.PI);
            if (th2_degree < 0) th2_degree = 0;

            else if (th2_degree > 180) th1_degree = 180;
            Buff[0] = (byte)(th1_degree);
            Buff[1] = (byte)(th2_degree);
            textBox_Px.Text = Px.ToString();
            textBox_Py.Text = Py.ToString();
            textBox_Pz.Text = Pz.ToString();
            textBox_Th1.Text = th1_degree.ToString();
            textBox_Th2.Text = th2_degree.ToString();
            textBox_Xc.Text = Xcm.ToString();
            textBox_Yc.Text = Ycm.ToString();
            _serialPort.Write(Buff, 0, 3);

            timer1.Enabled = true;
        }
    }

}