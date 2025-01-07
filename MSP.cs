using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSM_Conversion
{
    public class MSP // MSP에 대한 데이터 정의 
    {

        private static ArrayList encodBuf;
        
        private static uint start = 0x24; // 시작
        private static uint protocal = 0x4d; // 프로토콜 종류 (M 은 MSP)
        private static uint direction = 0x3c; // 전송 방향
        private static uint size = 0;
        private static uint type = 0; // 1 : cmd  2 : 시동 off 3 : 시동 on 4 : 제어정보 5 : gps 정보 6 : 응답
        private static uint Aux = 0;
        private static uint[] payload;
        private static double Roll = 0; //x
        private static double Pitch = 0; //y
        private static double Yaw = 0; //z
        private static uint Throttle = 0;
        private static byte[] rcvPayload;
        //private static uint crc = 0;
        private static uint id = 0;
        private static uint port = 9000;
        public MSP()
        {

        }

        public static void encode(ArrayList encodBuf)
        {
            
            try
            {
                byte[] encodedata;
                ArrayList encodtobyte = new ArrayList();

                for (int i = 0; i < encodBuf.Count; ++i)
                {

                    if (encodBuf[i].GetType().ToString().Equals("System.UInt32[]"))
                    {
                        foreach (UInt32 payloadD in (UInt32[])encodBuf[i])  //arraylist의 payload[]을 byte로 변경
                        {
                            if (!(payloadD > 255))
                            {
                                encodtobyte.Add(Convert.ToByte(payloadD));
                            }
                        }
                    }
                    else
                    {
                        encodtobyte.Add(Convert.ToByte(encodBuf[i]));

                    }

                }
                encodedata = new byte[encodtobyte.Count];
                for (int i = 0; i < encodtobyte.Count; ++i)
                {
                    encodedata[i] = Convert.ToByte(encodtobyte[i]);
                    Console.WriteLine(encodedata[i]);
                }

                DataLink.Send(encodedata);
            }
            catch (System.OverflowException e)
            {

            }
            
            
        }

        public static void deconde(byte[] packet)
        {
            byte[] rawPayload = new byte[packet[3]]; //3번째 데이터 size로 배열크기 지정
            double[]  doubleArray = new double[3]; //byte to double 데이터

            size = packet[3];
            type = packet[4];
            rcvPayload = rawPayload;
            for (int i = 0; i < rawPayload.Length; ++i) //payload 부분을 분리
            {
                rawPayload[i] = packet[i + 5];
            }

            for (int i = 0; i < doubleArray.Length; i++)
            {
                doubleArray[i] = BitConverter.ToDouble(packet, i * 8);
            }

            


            if (packet[4] == 0) //연결후 드론 ID 정보를 가져옴
            {
                //resConnect(rawPayload);
            }
            if (packet[4] == 6)
            {
                respControl(rawPayload);
            }
            if (packet[4] == 7)
            {
                respGps(rawPayload);
            }

        }

      
        // MSP msg
 

        private static void respControl(byte[] packet)
        {
            double[] doubleArray = new double[3];


            for (int i = 0; i < doubleArray.Length; i++)
            {
                doubleArray[i] = BitConverter.ToDouble(packet, i * 8);
            }


            Roll = doubleArray[0];
            Pitch = doubleArray[1];
            Yaw = doubleArray[2];
            Console.WriteLine(Roll);
            Console.WriteLine(Pitch);
            Console.WriteLine(Yaw);
        }

        private static void respGps(byte[] packet)
        {

            double[] doubleArray = new double[2];

            Console.WriteLine(doubleArray[0]);
            for (int i = 0; i < doubleArray.Length; i++)
            {
                doubleArray[i] = BitConverter.ToDouble(packet, i * 8);
            }

            foreach (double d in doubleArray)
            {
                Console.WriteLine(d + "ggggg");
            }
        }
       

        private static void resConnect(byte[] packet) //UA와 연결시
        {
            double[] doubleArray = new double[2];

            
            for (int i = 0; i < doubleArray.Length; i++)
            {
                doubleArray[i] = BitConverter.ToDouble(packet, i * 8);
            }

            id = (uint)doubleArray[0];
            port = (uint)doubleArray[1];
            foreach (double d in doubleArray)
            {
                Console.WriteLine(d + "sssss");
            }
        }
        public static uint getStart()
        {
            return start;
        }
        public static uint getProtocol()
        {
            return protocal;
        }
       
        public static uint getUAID()
        {
            return id;
        }
        public static uint getUAPort()
        {
            return port;
        }
        public static uint getMsgLen()
        {
            return size;
        }
        public static byte[] getPayload()
        {
             return rcvPayload;
        }

    }
}
