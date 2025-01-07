using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSM_Conversion
{
    public class MSG_VSM
    {

        

        //up-down link가 존재해서 포트가 다름
        private static UInt16 sourcePort = 0;// STANAG : 7000  MAVLINK : 8000 MSP : 9000 
        private static UInt16 destinationPort = 0;//목적지 포트
        private static UInt16 head = 0x53;//STANAG 고유 head
        private static UInt16 messageLength = 0;
        private static UInt16 sourceID = 0; //출발지 ID   
        private static UInt16 destinationID = 0; //목적지 ID
        private static UInt16 messageType = 0; //임시용 1 : 시동 on 2: 시동 off 3: throttle up 4: throttle donw 5: getgps 6: getcontrolState
        private static UInt16[]data = new ushort[3];
        private static byte[] rcvdata;

        public static ArrayList SendBuf;
        private static UInt16 throttle = 0;
        private static double Roll = 0; //x
        private static double Pitch = 0; //y
        private static double Yaw = 0; //z
        private static double Lat = 0; //lat
        private static double Long = 0; //long
        private static UInt16 uaType = 0; 
        public MSG_VSM()
        {

           
        }

        public static void setCurrentUAType(UInt16 UAT)
        {
            uaType = UAT;
        }
        
        private static void setSendWrapper()
        {
            if(uaType == 77) //MSP 이면
            {
                destinationPort = (UInt16)MSP.getUAPort();
                destinationID = (UInt16)MSP.getUAID();
            }
            if(uaType == 0xFE)
            {

            }
        }
        public static void set(UInt16 _sourcePort, UInt16 _destinationPort, UInt16 _messageLength, UInt16 _sourceID,
            UInt16 _destinationID, UInt16 _messageType, byte[] _rcvdata)
        {
            sourcePort = _sourcePort;
            destinationPort = _destinationPort;
            messageLength = _messageLength;
            sourceID = _sourceID;
            destinationID = _destinationID;
            messageType = _messageType;
            rcvdata = _rcvdata;
           
           
            if(messageType == 6)
            {
                double[] doubleArray = new double[3];
                for (int i = 0; i < doubleArray.Length; i++)
                {
                    doubleArray[i] = BitConverter.ToDouble(rcvdata, i * 8);
                }

                Roll = doubleArray[0];
                Pitch = doubleArray[1];
                Yaw = doubleArray[2];
               // Console.WriteLine(Roll + "seqw");
            }
           
            if (messageType == 5)
            {
                double[] doubleArray = new double[2];
                for (int i = 0; i < doubleArray.Length; i++)
                {
                    doubleArray[i] = BitConverter.ToDouble(rcvdata, i * 8);
                }

                Lat = doubleArray[0];
                Long = doubleArray[1];
              
            }

        }
        public static double getLat()
        {
            double gpsData = Lat;
            Console.WriteLine("222222222222222222222222222222222222");
            Console.WriteLine(gpsData);
      
            return gpsData;
        }
        public static double getLong()
        {
            double gpsData = Long;
            Console.WriteLine("222222222222222222222222222222222222");
            Console.WriteLine(gpsData);
           
            return gpsData;
        }
        public static double getRoll()
        {
            return Roll;
        }


        }
        public static void Encoder(ArrayList encodBuf)
        {
            
            List<byte> encodtobyte = new List<byte>();

            foreach (var item in encodBuf)
            {
                if (item is UInt16)
                {
                    encodtobyte.AddRange(BitConverter.GetBytes((UInt16)item));
                }
                else if (item is UInt16[])
                {
                    foreach (var value in (UInt16[])item)
                    {
                        encodtobyte.AddRange(BitConverter.GetBytes(value));
                    }
                }
            }
  
            byte[] encodeData = encodtobyte.ToArray();
            
            VSM.setRevData(encodeData);
        }
        public static ArrayList get()
        {
            ArrayList gets = new ArrayList { sourcePort, destinationPort, head, messageLength, sourceID, destinationID, messageType };
            return gets;
        }
        

        //gcs msg

        public static void throttleUp()
        {
            if (throttle < 800)
                throttle += 100;
            setSendWrapper(); //송신을 위한 wrapper 설정
            sourcePort = 7000;
            messageLength = 0;
            sourceID = 255;
            messageType = 3;
            data[0] = (UInt16)(throttle / 100);
            
            SendBuf = new ArrayList { sourcePort, destinationPort, head, messageLength, sourceID, destinationID, messageType, data };
            Encoder(SendBuf);
        }
        public static void throttleDown()
        {
            if (throttle > 200)
                throttle -= 100;
            setSendWrapper(); //송신을 위한 wrapper 설정
            sourcePort = 7000;
            messageLength = 0;
            sourceID = 255;
            messageType = 4;
            data[0] = (UInt16)(throttle / 100);
            SendBuf = new ArrayList { sourcePort, destinationPort, head, messageLength, sourceID, destinationID, messageType, data };
            Encoder(SendBuf);
        }
        public static void moterOn()
        {
            throttle = 200;
            setSendWrapper(); //송신을 위한 wrapper 설정
            sourcePort = 7000;
            messageLength = 0;
            sourceID = 255;
            messageType = 2;
            SendBuf = new ArrayList { sourcePort, destinationPort, head, messageLength, sourceID, destinationID, messageType };
            Encoder(SendBuf);
        }
        public static void moterOff()
        {
            throttle = 0;
            setSendWrapper(); //송신을 위한 wrapper 설정
            sourcePort = 7000;
            messageLength = 0;
            sourceID = 255;
            messageType = 1;
            SendBuf = new ArrayList { sourcePort, destinationPort, head, messageLength, sourceID, destinationID, messageType };
            Encoder(SendBuf);
        }
        public static void reqGPS()
        {
            setSendWrapper(); //송신을 위한 wrapper 설정
            sourcePort = 7000;
            messageLength = 0;
            sourceID = 255;
            messageType = 7;
            SendBuf = new ArrayList { sourcePort, destinationPort, head, messageLength, sourceID, destinationID, messageType };
            Encoder(SendBuf);
        }
        public static void reqControlState()
        {
            
            setSendWrapper(); //송신을 위한 wrapper 설정
            sourcePort = 7000;
            messageLength = 0;
            sourceID = 255;
            messageType = 8;
            SendBuf = new ArrayList {sourcePort, destinationPort, head, messageLength, sourceID,destinationID,messageType};
            Encoder(SendBuf);
           
        }
    }
}
