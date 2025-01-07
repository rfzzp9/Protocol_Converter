using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VSM_Conversion
{
    class DataLink
    { // STANAG : 7000  MAVLINK : 8000 MSP : 9000 
        private static char types='n';
        private static string ip = "192.168.4.25";//"192.168.4.1"; // 드론 아이피 // 드론 연결후 컴퓨터 아이피 받아오기
        private static int upPort = 9000; // UA에 보내기 위한 포트 번호 
        private static int downPort = 7000; // UA에서 데이터를 받기위한 포트 번호
        //ArrayList adata = new Array { '$', 'M', '<', size, types, data };
        // private char[] testdata = {'$', 'M', '<', '0', types, '0' };
        private int[] tdata ;
        private byte[] encoding;
        Thread test;
        private static UdpClient client;
        private static IPEndPoint endPoint;
        IAsyncResult asyncResult;
        
        //GCS gsc;
        public DataLink()
        {
            // new GCS();
           
            test = new Thread(Start);

            test.Start();
        }
        public  static void Send(byte[] strb)
        {

            Console.WriteLine("send button\n");
            try
            {
                using (UdpClient client = new UdpClient())
                {
                    IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(ip), upPort); 
                    client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                    client.Send(strb, strb.Length, endPoint);
                    
                    Console.WriteLine("send 성공");
                    client.Close();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            
        }
        
        // recive

        private void Start()
        {

            if (client != null)
            {
                Debug.WriteLine("이미 UDP 소켓이 생성되어있음..");
            }

            client = new UdpClient(); 
            endPoint = new IPEndPoint(IPAddress.Any, downPort);

            //client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            //client.ExclusiveAddressUse = false;
            client.Client.Bind(endPoint); //특정 IP에게 수신시 bind 사용

            while (true)
            {           
               Receive();
            }
            //StartListening();
        }


        private static void Receive()
        {
            if (client == null) { return; }

            try
            {

                byte[] bytes = client.Receive(ref endPoint);
                string message = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
                string[] gpsS = message.Split('/');
                
                Console.WriteLine("Receive\n");
                
                VSM.setRevData(bytes);
                //MSP.set(bytes);

                foreach (byte data in bytes)
                {
                    Console.WriteLine("-----------" + data + "-----------------");
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            
        }
        //여기서 데이터를 비교후 MSP면 MSP_memory에 데이터 업데이트 

    }

    
}

