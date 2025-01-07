#include <Wire.h>  //I2C통신
#include <WiFi.h>
#include <WiFiUdp.h>
#include <SoftwareSerial.h>

void control(int);
void reqCntData(double, double, double);
//메시지의 타입
typedef enum msgtype {

  throttle_up = 1,      //GCS 명령
  throttle_down = 11,
  request_off = 2,  // 시동 off
  request_on = 3,   // 시동 on
  request_control_data = 4,
  request_gps_data = 5,
  response_control_data = 6,  // control_data 요청에 대한 응답
  response_gps_data = 7
};
typedef struct request {
  uint head1 = 0x24;
  uint head2 = 0x4D;
  uint head3 = 0x3E;
  uint size;
  uint type;
  byte* payload;
} request;

TaskHandle_t Task1, Task2;  //Thread

//GPS 변수
char cbuf[80];
char c;
String str = "";
String targetStr = "GPGGA";
SoftwareSerial gps(16, 17);

const char* ssid = "MSP_DRONE";
const char* password = "";
byte buf[] = {
  0,
};  //일반 udp 수신 버퍼

WiFiUDP udp;
IPAddress server(192, 168, 4, 26);
int upPort = 9000;    //수신포트 드론 자체의 고유 포트
int downPort = 7000;  //송신포트
int broadcastPort = 9999;
//드론 관련 변수
const int MOTOR_A = 23;
const int MOTOR_B = 19;
const int MOTOR_C = 18;
const int MOTOR_D = 26;
const int CHANNEL_A = 0;
const int CHANNEL_B = 1;
const int CHANNEL_C = 2;
const int CHANNEL_D = 3;
const int MOTOR_FREQ = 5000;
const int MOTOR_RESOLUTION = 10;


void setup() {
  IPAddress ip(192, 168, 4, 25);      //고정 아이피
  IPAddress gateway(192, 168, 4, 1);  //통신사 접속 게이트 웨이
  IPAddress subnet(255, 255, 255, 0);

  Serial.begin(9600);
  //pinMode(15, OUTPUT);
  Serial.print("Setting AP (Access Point)...");
  WiFi.softAPConfig(ip, gateway, subnet);  //AP 모드로 설정
  Serial.print("Setting AP (Access Point)...");
  WiFi.softAP("MSP", "");

  udp.begin(upPort);
  gps.begin(9600);  // GSP 설정

  //자신의 IP 주소를 출력
  IPAddress IP = WiFi.softAPIP();
  Serial.print("AP IP adderss: ");
  Serial.println(IP);

  //드론제어 초기설정답
  Wire.begin();
  Wire.setClock(400000);

  Wire.beginTransmission(0x68);
  Wire.write(0x6b);
  Wire.write(0x0);
  Wire.endTransmission(true);

  ledcAttachPin(MOTOR_A, CHANNEL_A);
  ledcAttachPin(MOTOR_B, CHANNEL_B);
  ledcAttachPin(MOTOR_C, CHANNEL_C);
  ledcAttachPin(MOTOR_D, CHANNEL_D);

  ledcSetup(CHANNEL_A, MOTOR_FREQ, MOTOR_RESOLUTION);
  ledcSetup(CHANNEL_B, MOTOR_FREQ, MOTOR_RESOLUTION);
  ledcSetup(CHANNEL_C, MOTOR_FREQ, MOTOR_RESOLUTION);
  ledcSetup(CHANNEL_D, MOTOR_FREQ, MOTOR_RESOLUTION);

  ledcWrite(CHANNEL_A, 0);
  ledcWrite(CHANNEL_B, 0);
  ledcWrite(CHANNEL_C, 0);
  ledcWrite(CHANNEL_D, 0);


  xTaskCreatePinnedToCore(
    mspMain,
    "Task1",
    10000,
    NULL,
    1,
    &Task1,
    0);

  xTaskCreatePinnedToCore(
    gpsM,
    "Task2",
    10000,
    NULL,
    1,
    &Task2,
    0);
}


static int throttle;
static double gBalX, gBalY, gBalZ;
static double gLat, gLong;
//byte* control_data;
request req;

void loop() {
}


void mspMain(void* param) {

  while (1) {
    control(throttle);
    byte packetSize = udp.parsePacket();  //수신 받은 패킷 확인 없으면 0을 받음
    if (packetSize) {
      udp.read(buf, 80);
      Serial.println(buf[4]);
      
      if (buf[4] == throttle_up && throttle >= 200) {  //throttle값 변경
        throttle = (int)buf[5] * 100;
        Serial.println(buf[5]);
        Serial.println(throttle);
      }

      if (buf[4] == throttle_down && throttle > 200) {  //throttle값 변경
        throttle = ((int)buf[5] - 1) * 100;
        //Serial.println(buf[5]);
        Serial.println(throttle);
      }

      if (buf[4] == request_off && throttle > 0) {  //시동끄기
        throttle = 0;
      }
      if (buf[4] == request_on && throttle == 0) {  //시동켜기
        throttle = 200;
      }
      if (buf[4] == request_control_data) {  //roll pitch yaw값 받기
        reqCntData(gBalX, gBalY, gBalZ);
      }
      if (buf[4] == request_gps_data) {
        reqGpsData(gLat, gLong);
      }
      udp.beginPacket(server, downPort);  // write 사용전 필수

      for (byte i = 0; i < 10; ++i) {
        udp.write(*(buf + i));
      }
      udp.endPacket();  // write 사용후 필수
    } else {
      delay(1000);
    }
  }
}

void control(int throttle) {

  Wire.beginTransmission(0x68);
  Wire.write(0x3b);
  Wire.endTransmission(false);
  Wire.requestFrom((uint16_t)0x68, (uint8_t)14, true);

  int16_t AcXH = Wire.read();
  int16_t AcXL = Wire.read();
  int16_t AcYH = Wire.read();
  int16_t AcYL = Wire.read();
  int16_t AcZH = Wire.read();
  int16_t AcZL = Wire.read();
  int16_t TmpH = Wire.read();
  int16_t TmpL = Wire.read();
  int16_t GyXH = Wire.read();
  int16_t GyXL = Wire.read();
  int16_t GyYH = Wire.read();
  int16_t GyYL = Wire.read();
  int16_t GyZH = Wire.read();
  int16_t GyZL = Wire.read();

  int16_t AcX = AcXH << 8 | AcXL;
  int16_t AcY = AcYH << 8 | AcYL;
  int16_t AcZ = AcZH << 8 | AcZL;
  int16_t GyX = GyXH << 8 | GyXL;
  int16_t GyY = GyYH << 8 | GyYL;
  int16_t GyZ = GyZH << 8 | GyZL;

  //센서값 보정
  static int32_t AcXSum = 0, AcYSum = 0, AcZSum = 0;
  static int32_t GyXSum = 0, GyYSum = 0, GyZSum = 0;
  static int32_t AcXOff = 0.0, AcYOff = 0.0, AcZOff = 0.0;
  static int32_t GyXOff = 0.0, GyYOff = 0.0, GyZOff = 0.0;


  double AcXD = AcX - AcXOff;
  double AcYD = AcY - AcYOff;
  double AcZD = AcZ - AcZOff + 16384;

  double GyXD = GyX - GyXOff;
  double GyYD = GyY - GyYOff;
  double GyZD = GyZ - GyZOff;

  //주기시간 구하기
  static unsigned long t_prev = 0;
  unsigned long t_now = micros();
  double dt = (t_now - t_prev) / 1000000.0;
  t_prev = t_now;

  //자이로센서로 각도 구하기
  const float GYROXYZ_TO_DEGREES_PER_SEC = 131;     // 1도/s = 131
  double GyXR = GyXD / GYROXYZ_TO_DEGREES_PER_SEC;  //X에 대한 각속도
  double GyYR = GyYD / GYROXYZ_TO_DEGREES_PER_SEC;
  double GyZR = GyZD / GYROXYZ_TO_DEGREES_PER_SEC;

  static double gyAngleX = 0.0, gyAngleY = 0.0, gyAngleZ = 0.0;
  gyAngleX += GyXR * dt;
  gyAngleY += GyYR * dt;
  gyAngleZ += GyZR * dt;

  //각속도 센서로 각도 구하기
  const float RADIANS_TO_DEGREES = 180 / 3.14159;
  double AcYZD = sqrt(pow(AcY, 2) + pow(AcZ, 2));
  double AcXZD = sqrt(pow(AcX, 2) + pow(AcZ, 2));
  double acAngleY = atan(-AcXD / AcYZD) * RADIANS_TO_DEGREES;
  double acAngleX = atan(AcYD / AcXZD) * RADIANS_TO_DEGREES;
  double acAngleZ = 0;

  const double ALPHA = 0.96;
  static double cmAngleX = 0.0, cmAngleY = 0.0, cmAngleZ = 0.0;
  cmAngleX = ALPHA * (cmAngleX + GyXR * dt) + (1.0 - ALPHA) * acAngleX;
  cmAngleY = ALPHA * (cmAngleY + GyYR * dt) + (1.0 - ALPHA) * acAngleY;
  cmAngleZ = gyAngleZ;

  static double tAngleX = 0.0, tAngleY = 0.0, tAngleZ = 0.0;
  double eAngleX = tAngleX - cmAngleX;  //오차각도 = 목표각도 - 현재각도
  double eAngleY = tAngleY - cmAngleY;
  double eAngleZ = tAngleZ - cmAngleZ;

  //비례항 P
  double Kp = 1.0;
  double BalX = Kp * eAngleX;
  double BalY = Kp * eAngleY;
  double BalZ = Kp * eAngleZ;

  //미분항 D
  double Kd = 1.0;
  BalX += Kd * -GyXR;
  BalY += Kd * -GyYR;
  BalZ += Kd * -GyZR;
  if (throttle == 0) BalX = BalY = BalZ = 0.0;

  //적분항 I
  double Ki = 1.0;
  double ResX = 0.0, ResY = 0.0, ResZ = 0.0;
  ResX += Ki * eAngleX * dt;
  ResY += Ki * eAngleY * dt;
  ResZ += Ki * eAngleZ * dt;
  if (throttle == 0) ResX = ResY = ResZ = 0.0;
  BalX += ResX;
  BalY += ResY;
  BalZ += ResZ;

  double speedA = throttle + BalX - BalY + BalZ;
  double speedB = throttle - BalX - BalY - BalZ;
  double speedC = throttle - BalX + BalY + BalZ;
  double speedD = throttle + BalX + BalY - BalZ;

  int iSpeedA = constrain((int)speedA, 0, 1000);
  int iSpeedB = constrain((int)speedB, 0, 1000);
  int iSpeedC = constrain((int)speedC, 0, 1000);
  int iSpeedD = constrain((int)speedD, 0, 1000);

  ledcWrite(CHANNEL_A, iSpeedA);
  ledcWrite(CHANNEL_B, iSpeedB);
  ledcWrite(CHANNEL_C, iSpeedC);
  ledcWrite(CHANNEL_D, iSpeedD);

  // Serial.printf(" | BalX = %6.1f ", BalX);
  // Serial.printf(" | BalY = %6.1f ", BalY);
  // Serial.printf(" | BalZ = %6.1f ", BalZ);
  // Serial.println();
  gBalX = BalX;
  gBalY = BalY;
  gBalZ = BalZ;
}

void gpsM(void* param) {
  while (1) {
    if (gps.available()) {
      c = gps.read();
      if (c == '\n') {
        //Serial.println(str + "SS");                   // 한 줄의 끝부분에서
        if (targetStr.equals(str.substring(1, 6))) {  //GPGGA이면
          Serial.println(str);                        //데이터를 출력한다.

          int first = str.indexOf(",");
          int two = str.indexOf(",", first + 1);
          int three = str.indexOf(",", two + 1);
          int four = str.indexOf(",", three + 1);
          int five = str.indexOf(",", four + 1);
          //Lat와 Long 위치에 있는 값들의 index 추출
          String Lat = str.substring(two + 1, three);
          String Long = str.substring(four + 1, five);
          //앞에값과 뒤에 값 구분
          String Lat1 = Lat.substring(0, 2);
          String Lat2 = Lat.substring(2);

          String Long1 = Long.substring(0, 3);
          String Long2 = Long.substring(3);

          //좌표값 계산하기
          double LatF = Lat1.toDouble() + Lat2.toDouble() / 60;
          double LongF = Long1.toDouble() + Long2.toDouble() / 60;
          String gpsReturnValue = String(LatF, 20) + "/" + String(LongF, 20);
          Serial.println(LatF);
          Serial.println(LongF);
          Serial.print("Lat: ");
          Serial.println(LatF, 15);
          Serial.print("Long: ");
          Serial.println(LongF, 15);
          Serial.println(gpsReturnValue);
          //delay(500);
          //udp.beginPacket(server, 9000);  // write 사용전 필수
          //udp.parsePacket();              //read 사용전 필수
          //gpsReturnValue.toCharArray(cbuf, 80);
          //gpsReturnValue.getBytes(cbuf, 80);
          //udp.read(buf, 100);
          // for (int i = 0; i < 80; ++i) {
          //   udp.write(*(cbuf + i));
          // }
          //udp.endPacket();  // write 사용후 필수
          gLat = LatF;
          gLong = LongF;
        }
        str = "";
      } else {  //한 줄이 끝날때 까지 내용을 가져옴
        str += c;
      }
    }
  }
}


void reqCntData(double X, double Y, double Z) {
  double doubleArray[] = { X, Y, Z };
  byte byteArray[sizeof(doubleArray)];

  for (int i = 0; i < sizeof(doubleArray) / sizeof(double); i++) {
    double value = doubleArray[i];
    byte* ptr = (byte*)&value;
    for (int j = 0; j < sizeof(double); j++) {
      byteArray[i * sizeof(double) + j] = *(ptr + j);
    }
  }
  req.size = sizeof(byteArray);
  req.type = 6;
  req.payload = byteArray;
  encode();
}
void reqGpsData(double Lat, double Long) {
  double doubleArray[] = { Lat, Long };
  byte byteArray[sizeof(doubleArray)];

  for (int i = 0; i < sizeof(doubleArray) / sizeof(double); i++) {
    double value = doubleArray[i];
    byte* ptr = (byte*)&value;
    for (int j = 0; j < sizeof(double); j++) {
      byteArray[i * sizeof(double) + j] = *(ptr + j);
    }
  }
  req.size = sizeof(byteArray);
  req.type = 7;
  req.payload = byteArray;
  encode();
}
void encode() {
  byte bytearr[50] = {
    0,
  };

  bytearr[0] = req.head1;
  bytearr[1] = req.head2;
  bytearr[2] = req.head3;
  bytearr[3] = req.size;
  bytearr[4] = req.type;
  for (byte i = 0; i < req.size; ++i) {
    Serial.println(*(req.payload + i));
  }
  Serial.println("-----------------");
  for (byte i = 0; i < req.size; ++i) {
    bytearr[i + 5] = *(req.payload + i);
    Serial.println(bytearr[i]);
  }

  udp.beginPacket(server, downPort);  // write 사용전 필수
  udp.write(bytearr, 50);
  Serial.println("send encoder");
  udp.endPacket();  // write 사용후 필수
}
