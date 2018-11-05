using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.IO.Ports;

namespace newport_test
{
    public partial class Form1 : Form
    {
        //串口通信声明
        string returnflag;
        bool tryagain; //防止串口不稳定出错

        //波片位置
        int rotnum;


        //波片架0度位置
        double oriposition1=0;
        double toposition;
        double addposition;
        double nowposition;

        public Form1()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
        /// <summary>
        /// 串口连接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            if (myserialPort.IsOpen == true)
                return;
            try
            {
                myserialPort.PortName = comboBox1.Text;
                myserialPort.BaudRate = Convert.ToInt32(comboBox2.Text.Trim());//"57600"
                myserialPort.Parity = Parity.None;
                myserialPort.DataBits = 8;
                myserialPort.StopBits = StopBits.One;

                myserialPort.Open();
            }
            catch (Exception exception)
            {
                System.Windows.Forms.MessageBox.Show(exception.Message);
            }
            finally
            {
                if (myserialPort.IsOpen == true)
                {
                    textBox2.Text = "串口已连接";
                    textBox2.ForeColor = Color.Green;
                }
                else
                {
                    textBox2.Text = "串口未连接";
                    textBox2.ForeColor = Color.Red;
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //初始化串口参数
            string[] myports = SerialPort.GetPortNames();
            for (int i = 0; i <= myports.Length - 1; i++)
            {
                string myport = myports[i];
                comboBox1.Items.Add(myport.Trim());
            }
            comboBox1.SelectedIndex = 0;
            comboBox2.Items.Add("57600");
            comboBox2.SelectedIndex = comboBox2.Items.IndexOf("57600");
            myserialPort = new SerialPort();

            //初始化SerialPort对象  
            myserialPort.NewLine = "/r/n";
            myserialPort.RtsEnable = true; //放着先看看

            //波片初始化
            textBox5.Text = "1";  //默认操作第一个波片

            //添加事件注册  
            myserialPort.DataReceived += myserialPort_DataReceived;
        }

        /// <summary>
        /// 串口数据采集
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void myserialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int spn = myserialPort.BytesToRead;//先记录下来，避免某种原因，人为的原因，操作几次之间时间长，缓存不一致  
            byte[] mybyte = new byte[spn];
            string returnflaghere;
            try
            {
                if (spn > 0)
                {
                    for (int i = 0; i < 1000; i++)
                    {
                        System.Windows.Forms.Application.DoEvents();
                    }
                    myserialPort.Read(mybyte, 0, spn);
                    myserialPort.DiscardInBuffer();
                    returnflaghere = "";
                    foreach (byte inbyte in mybyte)
                    {
                        returnflaghere += Convert.ToChar(inbyte);
                    }
                    returnflag += returnflaghere;
                }
            }
            catch (Exception exception)
            {
                System.Windows.Forms.MessageBox.Show(exception.Message);
            }
        }


        /// <summary>
        /// 串口断开
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            if (myserialPort.IsOpen == false)
                return;

            try
            {
                myserialPort.Close();
            }
            catch (Exception exception)
            {
                System.Windows.Forms.MessageBox.Show(exception.Message);
            }
            finally
            {
                if (myserialPort.IsOpen == true)
                {
                    textBox2.Text = "串口已连接";
                    textBox2.ForeColor = Color.Green;
                }
                else
                {
                    textBox2.Text = "串口未连接";
                    textBox2.ForeColor = Color.Red;
                }
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }



        private void button3_Click(object sender, EventArgs e)
        {
            rotnum =Convert.ToInt32(textBox5.Text.Trim());
            nowposition=rotate(oriposition1, rotnum);
            textBox4.Text =nowposition.ToString();
            System.Windows.Forms.MessageBox.Show("波片已归零！", "消息");
        }



        /// <summary>
        /// 转动波片架
        /// </summary>
        /// <param name="position"></param>//要转动的角度
        /// <param name="rotnumx"></param>//要转动哪个波片架
        private double rotate(double position, int rotnumx)
        {
            int rotnum = rotnumx;
            double myposition;
            tryagain = false;

            if (operation(rotnum, rotnum.ToString() + "PA" + position.ToString(), false) == "")
            {
                return -1;
            }

            do
            {
                for (int i = 0; i <= 1000000; i++)
                {
                    System.Windows.Forms.Application.DoEvents();
                }

                string retval = operation(rotnum, rotnum.ToString() + "TP", true);
                if (tryagain == true)
                {
                    retval = operation(rotnum, rotnum.ToString() + "TP", true);
                }
                if (tryagain == true)
                {
                    retval = operation(rotnum, rotnum.ToString() + "TP", true);
                }


                if (retval == "")
                    return -1;
                  myposition = Convert.ToDouble(retval.Substring((rotnum.ToString() + "TP").Length));
                if (Math.Abs(myposition - position) < 1)
                    break;
            } while (true);
            return myposition;
        }
        private string operation(int myrotnum, string mycmd, bool rtnneeded)
        {
            try
            {
                returnflag = "";
                myserialPort.Write(mycmd.Trim() + "\r" + "\n");
            }
            catch (Exception exception)
            {
                System.Windows.Forms.MessageBox.Show(exception.Message);
            }

            if (rtnneeded == false)
            {
                return "done";
            }

            int pp = 0;
            do
            {
                for (int i = 0; i <= 10000; i++)
                {
                    System.Windows.Forms.Application.DoEvents();
                }
                if (returnflag.Length >= 2)
                {
                    if (returnflag.Substring(returnflag.Length - 2) == "\r" + "\n")
                    {
                        tryagain = false;
                        break;
                    }
                }

                pp++; //防止串口出错，转波片等待10余秒还没结果，自动跳出，再来一次
                if (pp > 2000)
                {
                    tryagain = true;
                    break;
                }

            } while (true);
            return returnflag.Trim();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }


//转动一次
        private void button4_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != "")
                toposition = Convert.ToInt32(textBox1.Text.Trim());
                rotnum = Convert.ToInt32(textBox5.Text.Trim());
                nowposition =rotate(oriposition1+toposition, rotnum);
                textBox4.Text = nowposition.ToString();
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }


//持续转动
        private void button5_Click(object sender, EventArgs e)
        {

            if (textBox1.Text != "")
                addposition = Convert.ToInt32(textBox3.Text.Trim());
                rotnum = Convert.ToInt32(textBox5.Text.Trim());
                nowposition =rotate(nowposition + addposition, rotnum);
                textBox4.Text = nowposition.ToString();

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }
    }
  }
