# 드론 통합 통신 시스템



드론 통신 프로토콜의 상이함으로 인해 관제탑(GCS)과 드론 간 일대일 통신만 가능하며, 서로 다른 프로토콜을 사용하는 드론 10대를 운용하려면 10개의 GCS가 필요합니다.

이는 상호운용성을 저해해 군집비행 등 전략적 운용이 불가능하고, 고비용 문제를 초래합니다. 이를 해결하기 위해 **1개의 GCS로 다양한 드론을 상호운용할 수 있는 국제표준 STANAG 4586 기반의 프로토콜 변환기**를 제안합니다.


## 📌 주요 기능


- 다양한 드론 프로토콜 통합 및 실시간 변환
- 드론 위치 및 자세 모니터링


## 🛠 기술 스택


- FRONT-END PART


|구분|Skill|
|------|---|
|Platform|Visual Studio Code|
|Language|C#|
|Networking|Socket|


## 📌 개발 내용


- GPS 맵 개발 (*Map.cs, DataLink.cs*)
    - GPS를 통해 드론 위치 표시 
- 드론 자세 모니터 개발 (*GCS.cs*)
    - 드론의 자세 모니터링 등 자세 제어

<div align="center">
    
![image](https://github.com/user-attachments/assets/68cacf40-363f-4b12-ba21-bc01793a0843)

</div>
######GPS맵 (오른쪽), 드론 자세 모니터 (왼쪽)


## 📌 개발 환경


### **SW Architecture**

<div align="center">
    
![image](https://github.com/user-attachments/assets/435f8f3b-cdae-4133-ac51-63ffeea3b6b0)

</div>


## 📌 성장 경험


### *자이로센서 값 오차로 인한 이미지 변동 문제 해결*

드론의 자이로센서 값에서 발생하는 오차범위가 커서, 작은 기울기 변화에도 이미지가 크게 변동되는 문제가 발생했습니다.

자이로센서 값이 민감하게 반응하면서 실제 드론의 기울기보다 과도하게 이미지가 흔들리거나 변화율이 비정상적으로 커지는 현상이 나타난 것입니다.

이를 해결하기 위해 자이로센서의 값을 일정 시간 동안 수집하여 평균값을 계산하고, 이를 기반으로 드론의 기울기에 맞게 이미지를 변화시키는 방식을 도입했습니다.

이를 통해 이미지 변화율을 안정화시켜, 불필요한 큰 변동을 줄이고 더 정확하고 자연스러운 기울기 변화 표현이 가능해졌습니다.


## 📌 서비스 화면


(시연 영상 - *https://www.youtube.com/watch?v=-fsmM3SVgms*)

GCS의 STANAG4586와 드론의 MSP 간에 데이터 통신 성공 화면

<div align="center">

![image](https://github.com/user-attachments/assets/a4c36bec-e8ef-436b-b713-76161da37c12)

</div>


