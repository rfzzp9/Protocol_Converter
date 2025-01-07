using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VSM_Conversion
{
    //gcs id 255
    //vsm id 250
   
    // MSP 2
    public partial class VSM : Form
    {
        private static UInt16 stanagID = 250;
        private static UInt16 stanagPort = 7000;
        DataLink DataLink =  new DataLink();
        MSG_VSM MSG_VSM = new MSG_VSM();
        private ArrayList dbuf = new ArrayList();
        private static byte[] revBytes;
        static TextBox stanagt; //stanagBox
        static TextBox mspt; //MspBox
        static PictureBox transp1;
        static PictureBox transp2;
        static Label transbefor1;
        static Label transbefor2;
        static Label transafter1;
        static Label transafter2;
        
        

        public VSM()
        {
            InitializeComponent();
            stanagt = stanagBox;
            mspt = MspBox;
            transp1 = transP1;
            transp2 = transP2;
            transbefor1 = transbeforL1;
            transbefor2 = transbeforL2;
            transafter1 = transafterL1;
            transafter2 = transafterL2;
        }

      
        //vsm 부분
        public static void setRevData(byte[] bytes)
        {
            revBytes = bytes;
            
            protocolCheck();
        }
        
        private static void protocolCheck() // 수신시 프로토콜 종류를 확인
        {
           
            if(revBytes[4] == 83) //stanag
            {
               // Console.WriteLine("stanag msg");
                if (stanagt.InvokeRequired)
                {
                    stanagt.Invoke(new MethodInvoker(delegate ()
                    {
                        stanagt.Text = "";
                        transp1.Visible = true;
                        transp2.Visible = false;
                        transbefor1.Visible = true;
                        transafter1.Visible = false;
                        transbefor2.Visible = false;
                        transafter2.Visible = true;
                        stanagt.BackColor = Color.Red;
                        mspt.BackColor = Color.White;
                        foreach(byte t in revBytes)
                        {
                            stanagt.Text += t.ToString() + " ";
                        }
                    }));
                }
                else
                {
                    stanagt.Text = "";
                    transp1.Visible = true;
                    transp2.Visible = false;
                    transbefor1.Visible = true;
                    transafter1.Visible = false;
                    transbefor2.Visible = false;
                    transafter2.Visible = true;
                    stanagt.BackColor = Color.Red;
                    mspt.BackColor = Color.White;
                    foreach (byte t in revBytes)
                    {
                        stanagt.Text += t.ToString() + " ";
                    }
                }
                stanagToMSP();
            }
            else if (revBytes[1] == 77)//MSP
            {
                //Console.WriteLine("MSP msg");
                
                if (stanagt.InvokeRequired)
                {
                    stanagt.Invoke(new MethodInvoker(delegate ()
                    {
                        mspt.Text = " ";
                        stanagt.BackColor = Color.White;
                        mspt.BackColor = Color.Red;
                        transp1.Visible = false;
                        transp2.Visible = true;
                        transbefor1.Visible = false;
                        transafter1.Visible = true;
                        transbefor2.Visible = true;
                        transafter2.Visible = false;
                        foreach (byte t in revBytes)
                        {
                            mspt.Text += t.ToString() + " ";

                        }
                    }));
                }
                else
                {
                    mspt.Text = " ";
                    stanagt.BackColor = Color.White;
                    mspt.BackColor = Color.Red;
                    transp1.Visible = false;
                    transp2.Visible = true;
                    transbefor1.Visible = false;
                    transafter1.Visible = true;
                    transbefor2.Visible = true;
                    transafter2.Visible = false;
                    foreach (byte t in revBytes)
                    {
                        mspt.Text += t.ToString() + " ";
                    }
                }
                
                MSPToStanag();
                
            }
           
        }
        
        private static void stanagToMSP()
        {
            try
            {
               
                UInt16[] uint16Array = new UInt16[revBytes.Length / 2];
                uint start = VSM_Conversion.MSP.getStart();
                uint protocol = VSM_Conversion.MSP.getProtocol();
                uint direction = 0x3e;
                uint size = 0;
                uint msgType = 0;
                uint[] payload = new uint[3];
                
                mspt.Invoke(new MethodInvoker(delegate ()
                {
                    mspt.Text = " ";
                }));
                for (int i = 0; i < uint16Array.Length; i++)
                {
                    uint16Array[i] = BitConverter.ToUInt16(revBytes, i * 2);
                    if (uint16Array.Length > 7)
                    {
                        for (int j = 0; j < payload.Length; j++)
                        {
                            payload[j] = uint16Array[j + 7];

                        }
                    }               
                }
                
                size = uint16Array[3];
                mspt.Invoke(new MethodInvoker(delegate ()
                {
                    
                    mspt.Text = start.ToString() + " " + protocol.ToString() 
                            + " " + direction.ToString() + " " + size.ToString() + " " + msgType.ToString() + " ";
                    if (uint16Array.Length > 7)
                    {
                        for (int j = 0; j < payload.Length; j++)
                        {
                            payload[j] = uint16Array[j + 7];
                            mspt.Text += payload[j].ToString() + " ";
                        }

                    }                 
                }));
                

                MSG_VSM.setCurrentUAType(revBytes[1]);


                if (uint16Array[6] == 0) //connectData
                {
                    msgType = 0;
                }
                if (uint16Array[6] == 3) //throttle up
                {
                    msgType = 1;
                }
                if (uint16Array[6] == 4) //throttle down
                {
                    msgType = 11;
                }
                if (uint16Array[6] == 1) //off
                {
                    msgType = 2;
                }
                if (uint16Array[6] == 2) //on
                {
                    msgType = 3;
                }
                if (uint16Array[6] == 8) //req control data
                {
                    msgType = 4;
                }
                if (uint16Array[6] == 7) //req gps data
                {
                    msgType = 5;
                }
                if (uint16Array[6] == 6) //response control
                {
                    msgType = 6;
                }
                if (uint16Array[6] == 5) //response gps
                {
                    msgType = 7;
                }
               
                ArrayList encodBuf = new ArrayList { start, protocol, direction, size, msgType, payload }; 

                VSM_Conversion.MSP.encode(encodBuf);
            }
            catch(Exception e)
            {

            }
            

        }
        private static void MSPToStanag()
        {
            // MSP type  1 : cmd_throttle up 11 :cmd_throttle down  2 : 시동 off 3 : 시동 on 4 : 제어정보요청 5 : gps 정보요청 6 : 응답_controldata
            // STANAG type 임시용 0: connect 1 : 시동 off 2: 시동 on 3: throttle up 4: throttle donw 5: getgps 6: getcontrolState 7: req gps 8:req control
            VSM_Conversion.MSP.deconde(revBytes); //MSP에 맞게 decode 진행           
            
            UInt16 msgType = 0; //STANAG에 맞는 type 번호


            UInt16 uaID = (UInt16)VSM_Conversion.MSP.getUAID(); //현재 UA의 ID를 얻어옴
            UInt16 uaPort = (UInt16)VSM_Conversion.MSP.getUAPort(); //UA Port 번호를 얻어옴
            UInt16 msgLen = (UInt16)VSM_Conversion.MSP.getMsgLen();
            byte []data = VSM_Conversion.MSP.getPayload();

            //STANAG에 맞는 type 번호로 변환
            if (revBytes[4] == 0) //connectData
            {
                msgType = 0;
            }
            if(revBytes[4] == 1) //throttle up
            {
                msgType = 3;
            }
            if (revBytes[4] == 11) //throttle down
            {
                msgType = 4;
            }
            if (revBytes[4] == 2) //off
            {
                msgType = 1;
            }
            if (revBytes[4] == 3) //on
            {
                msgType = 2;
            }
            if (revBytes[4] == 4) //req control data
            {
                msgType = 8;
            }
            if (revBytes[4] == 5) //req gps data
            {
                msgType = 7;
            }
            if (revBytes[4] == 6) //response control
            {
                msgType = 6;
            }
            if (revBytes[4] == 7) //response gps
            {
                msgType = 5;
                
            }

            MSG_VSM.set(uaPort, stanagPort, msgLen, uaID, stanagID, msgType, data);
            
        }

        
    }
}
