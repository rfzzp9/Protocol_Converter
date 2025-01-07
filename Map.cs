using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;

namespace ControlStation
{
    class Map
    {
        PictureBox pictureBox1;

        public GMapControl App;
        public Dictionary<string, GMapOverlay> DroneOverlays = new Dictionary<string, GMapOverlay>();
        public Dictionary<string, List<PointLatLng>> DroneMarkerPoints = new Dictionary<string, List<PointLatLng>>();
        private Button infoButton;
        private Button addButton;
        private Button startButton;
        private Button drone01;
       // private Button drone02;
        private Button currentPosButton;
        
        private Label drone01Label;
        private Label drone02Label;

        private string currentDrone; // 현재 선택한 드론
        private Dictionary<string, bool> droneInitialized = new Dictionary<string, bool>(); // 드론 초기화 상태
        
        public Map(GMapControl app, PictureBox _pictureBox1)
        {
            pictureBox1 = _pictureBox1;
           
            this.App = app;
            PointLatLng p = new PointLatLng(36.107545500000000, 128.33299255371093);
            App.MapProvider = GMapProviders.GoogleMap;
            App.Position = p; // 드론 현재 위치 값 받아와 넣기
            App.MinZoom = 2;
            App.MaxZoom = 20;
            App.Zoom = 15;
            this.App.MouseDown += MouseDown;
            this.App.OnMarkerClick += MarkerClick; // 마커 클릭 이벤트 핸들러 추가

            
           

            infoButton = new Button();
            infoButton.Text = "Get Marker Info";
            infoButton.Location = new Point(10, 10);
            infoButton.Click += InfoButton_Click;
            this.App.Controls.Add(infoButton);

            addButton = new Button();
            addButton.Text = "Add New Marker";
            addButton.Location = new Point(10, 40);
            addButton.Click += AddButton_Click;
            this.App.Controls.Add(addButton);

            startButton = new Button();
            startButton.Text = "Start";
            startButton.Location = new Point(10, 70);
            startButton.Click += StartButton_Click;
            this.App.Controls.Add(startButton);

            currentPosButton = new Button();
            currentPosButton.Text = "현재위치";
            currentPosButton.Location = new Point(10, 90);
            currentPosButton.Click += CurrentPOSButton_Click;
            this.App.Controls.Add(currentPosButton);

            drone01 = CreateDroneButton("드론1", 10, out drone01Label);
           // drone02 = CreateDroneButton("드론2", 160, out drone02Label);

            // 최초 실행 시에 각 드론 버튼에 마커 추가
            AddMarker(GetRandomCoordinate(36.110, 128.34, 0.02), "드론1"); //repair
           // AddMarker(GetRandomCoordinate(36.105, 128.33, 0.02), "드론2");

            drone01.Click += DroneButton_Click;
            //drone02.Click += DroneButton_Click;

            // 드론 초기화 상태 설정
            droneInitialized.Add("드론1", true);
            //droneInitialized.Add("드론2", true);
        }

        private Button CreateDroneButton(string buttonText, int locationX, out Label label)
        {
            Button droneButton = new Button();
            droneButton.Text = buttonText;
            droneButton.Location = new Point(locationX, 570);
            droneButton.Click += DroneButton_Click;
            //this.App.Controls.Add(droneButton);

            label = new Label();
            label.Text = "Waiting for coordinates...";
            label.AutoSize = true;
            label.Location = new Point(locationX, 600);
            this.App.Controls.Add(label);

            return droneButton;
        }

        private void DroneButton_Click(object sender, EventArgs e) //repair
        {
            Button clickedButton = (Button)sender;
            currentDrone = clickedButton.Text;

            //최초 클릭일 경우에만 구미 좌표에 마커 추가
            if (!droneInitialized.ContainsKey(currentDrone) || !droneInitialized[currentDrone])
            {
                AddMarker(GetRandomCoordinate(36.1, 128.34, 0.02), currentDrone);
                droneInitialized[currentDrone] = true;
            }
        }

        private PointLatLng GetRandomCoordinate(double centerLat, double centerLng, double range)
        {
            // 중심 좌표를 기준으로 일정 범위 내에서 랜덤 좌표 생성
            Random rand = new Random();
            double lat = centerLat + (rand.NextDouble() - 0.5) * range;
            double lng = centerLng + (rand.NextDouble() - 0.5) * range;

            return new PointLatLng(lat, lng);
        }


        private void InfoButton_Click(object sender, EventArgs e)
        {
            GetMarkerInfo();
        }

        private void GetMarkerInfo()
        {
            foreach (var overlay in DroneOverlays.Values)
            {
                Console.WriteLine($"Markers in Overlay {overlay.Id}:");
                foreach (var marker in overlay.Markers)
                {
                    Console.WriteLine($"Marker Position: {marker.Position.Lat}, {marker.Position.Lng}");
                }
                Console.WriteLine($"Total Markers in Overlay {overlay.Id}: {overlay.Markers.Count}");
            }
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            // 현재 선택한 드론이 없으면 메시지 표시
            if (currentDrone == null)
            {
                MessageBox.Show("Please select a drone first.");
                return;
            }
        }
        GMapMarkerCustom gMarker;
        public void AddMarker(PointLatLng p, string droneName)
        {
            if (!DroneOverlays.ContainsKey(droneName))
            {
                GMapOverlay newOverlay = new GMapOverlay(droneName);
                this.App.Overlays.Add(newOverlay);
                DroneOverlays.Add(droneName, newOverlay);

                List<PointLatLng> newMarkerPoints = new List<PointLatLng>();
                DroneMarkerPoints.Add(droneName, newMarkerPoints);
            }

            GMapOverlay overlay = DroneOverlays[droneName];
            List<PointLatLng> markerPoints = DroneMarkerPoints[droneName];

            Bitmap customMarkerImage;


            switch (droneName)
            {
                case "드론1":
                    if (markerPoints.Count == 0)
                    {
                        customMarkerImage = new Bitmap(@"..\redDrone.png");
                    }
                    else
                    {
                        customMarkerImage = new Bitmap(@"..\placeholder.png"); //빨간색 마커 이미지
                    }
                    //customMarkerImage = new Bitmap(@"..\placeholder.png"); //빨간색 마커 이미지
                    gMarker = new GMapMarkerCustom(p, customMarkerImage);
                    gMarker.ToolTipMode = MarkerTooltipMode.OnMouseOver;
                    gMarker.ToolTipText = "New Marker1";

                    overlay.Markers.Add(gMarker);
                    break;
                case "드론2":
                    if (markerPoints.Count == 0)
                    {
                        customMarkerImage = new Bitmap(@"..\blueDrone.png");
                    }
                    else
                    {
                        customMarkerImage = new Bitmap(@"..\location.png"); //파란색 마커 이미지
                    }
                    //customMarkerImage = new Bitmap(@"..\location.png"); //파란색 마커 이미지
                    gMarker = new GMapMarkerCustom(p, customMarkerImage);
                    gMarker.ToolTipMode = MarkerTooltipMode.OnMouseOver;
                    gMarker.ToolTipText = "New Marker2";
                    overlay.Markers.Add(gMarker);
                    break;
            }

            markerPoints.Add(p);
            RedrawRoute(droneName);

            UpdateLabel(droneName, $"Lat: {p.Lat:F3}, Lng: {p.Lng:F3}");
        }

        private void RedrawRoute(string droneName)
        {
            GMapOverlay overlay = DroneOverlays[droneName];
            List<PointLatLng> markerPoints = DroneMarkerPoints[droneName];

            overlay.Routes.Clear();

            if (markerPoints.Count >= 1)
            {
                GMapRoute route = new GMapRoute(markerPoints, "route");
                switch (droneName)
                {
                    case "드론1":
                        route.Stroke = new Pen(Color.Red, 2);
                        break;
                    case "드론2":
                        route.Stroke = new Pen(Color.Blue, 2);
                        break;
                    default:
                        break;
                }

                overlay.Routes.Add(route);
            }
        }

        private void MouseDown(object sender, MouseEventArgs e)
        {
            PointLatLng p = App.FromLocalToLatLng(e.X, e.Y);

            if (e.Button == MouseButtons.Left && currentDrone != null)
            {
                AddMarker(p, currentDrone);
            }
        }

        private void UpdateLabel(string droneName, string coordinates)
        {
            switch (droneName)
            {
                case "드론1":
                    drone01Label.Text = coordinates;
                    break;
                case "드론2":
                    drone02Label.Text = coordinates;
                    break;
                default:
                    break;
            }
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            // 각 드론의 현재 위치를 초기 위치로 설정
            List<PointLatLng> drone1Positions = DroneMarkerPoints["드론1"];
            //List<PointLatLng> drone2Positions = DroneMarkerPoints["드론2"];

            // 초기 위치가 없는 경우에는 메시지 표시 후 리턴
            if (drone1Positions.Count < 2 )//|| drone2Positions.Count < 2)
            {
                MessageBox.Show("Please add at least 2 markers for both drones first.");
                return;
            }

            // 드론1 위치를 업데이트할 타이머
            Timer timer1 = new Timer();
            timer1.Interval = 1; // 20밀리초마다 이동
            int currentMarkerIndex1 = 1; // 다음 위치로 이동할 때마다 인덱스 증가
            GMapMarkerCustom droneMarker1 = (GMapMarkerCustom)DroneOverlays["드론1"].Markers[0]; // 초기 마커
            PointLatLng currentPosition1 = drone1Positions[0]; // 현재 위치 초기화
            PointLatLng nextPosition1 = drone1Positions[currentMarkerIndex1]; // 다음 위치 초기화


            timer1.Tick += (s, evt) =>
            {
                // 현재 위치와 다음 위치 사이의 거리 계산
                double distance = CalculateDistance(currentPosition1, nextPosition1);

                // 이동할 거리 계산
                double moveDistance = 1; // 픽셀 단위로 이동할 거리
                if (distance > moveDistance)
                {
                    // 다음 위치로 향하는 방향 벡터 계산
                    double dx = (nextPosition1.Lng - currentPosition1.Lng) / distance * moveDistance;
                    double dy = (nextPosition1.Lat - currentPosition1.Lat) / distance * moveDistance;

                    // 새로운 위치 계산
                    double newLat = currentPosition1.Lat + dy;
                    double newLng = currentPosition1.Lng + dx;

                    currentPosition1 = new PointLatLng(newLat, newLng);

                    // 마커 위치 업데이트
                    droneMarker1.Position = currentPosition1;
                    UpdateLabel("드론1", $"Lat: {currentPosition1.Lat:F3}, Lng: {currentPosition1.Lng:F3}");

                    Bitmap droneImg = new Bitmap(@"..\redDrone.png"); //빨간색 드론 이미지
                    droneMarker1.ChangeImage(droneImg);

                    App.Refresh(); // 지도 갱신
                }
                else
                {
                    // 다음 위치로 이동할 거리보다 짧은 경우 다음 위치로 이동
                    currentPosition1 = nextPosition1;

                    // 현재 마커 지우기
                    DroneOverlays["드론1"].Markers.Remove(droneMarker1);

                    currentMarkerIndex1++;

                    if (currentMarkerIndex1 < drone1Positions.Count)
                    {
                        nextPosition1 = drone1Positions[currentMarkerIndex1];
                        droneMarker1 = (GMapMarkerCustom)DroneOverlays["드론1"].Markers[0]; // 새로운 마커 설정
                    }
                    else
                    {
                        // 다음 위치가 없으면 타이머 중지
                        timer1.Stop();
                    }
                }
            };
            timer1.Start();
/*
            // 드론2 위치를 업데이트할 타이머
            Timer timer2 = new Timer();
            timer2.Interval = 1; // 20밀리초마다 이동
            int currentMarkerIndex2 = 1; // 다음 위치로 이동할 때마다 인덱스 증가
            GMapMarkerCustom droneMarker2 = (GMapMarkerCustom)DroneOverlays["드론2"].Markers[0]; // 초기 마커
            PointLatLng currentPosition2 = drone2Positions[0]; // 현재 위치 초기화
            PointLatLng nextPosition2 = drone2Positions[currentMarkerIndex2]; // 다음 위치 초기화

            timer2.Tick += (s, evt) =>
            {
                // 현재 위치와 다음 위치 사이의 거리 계산
                double distance = CalculateDistance(currentPosition2, nextPosition2);

                // 이동할 거리 계산
                double moveDistance = 1; // 픽셀 단위로 이동할 거리
                if (distance > moveDistance)
                {
                    // 다음 위치로 향하는 방향 벡터 계산
                    double dx = (nextPosition2.Lng - currentPosition2.Lng) / distance * moveDistance;
                    double dy = (nextPosition2.Lat - currentPosition2.Lat) / distance * moveDistance;

                    // 새로운 위치 계산
                    double newLat = currentPosition2.Lat + dy;
                    double newLng = currentPosition2.Lng + dx;

                    currentPosition2 = new PointLatLng(newLat, newLng);

                    // 마커 위치 업데이트
                    droneMarker2.Position = currentPosition2;
                    UpdateLabel("드론2", $"Lat: {currentPosition2.Lat:F3}, Lng: {currentPosition2.Lng:F3}");
                    Bitmap droneImg = new Bitmap(@"..\blueDrone.png");  //파란색 드론 이미지
                    droneMarker2.ChangeImage(droneImg);
                    App.Refresh(); // 지도 갱신
                }
                else
                {
                    // 다음 위치로 이동할 거리보다 짧은 경우 다음 위치로 이동
                    currentPosition2 = nextPosition2;

                    // 현재 마커 지우기
                    DroneOverlays["드론2"].Markers.Remove(droneMarker2);

                    currentMarkerIndex2++;

                    if (currentMarkerIndex2 < drone2Positions.Count)
                    {
                        nextPosition2 = drone2Positions[currentMarkerIndex2];
                        droneMarker2 = (GMapMarkerCustom)DroneOverlays["드론2"].Markers[0]; // 새로운 마커 설정
                    }
                    else
                    {
                        // 다음 위치가 없으면 타이머 중지
                        timer2.Stop();
                    }
                }
            };
            timer2.Start();*/

        }

        private void CurrentPOSButton_Click(object sender, EventArgs e)
        {
            double Lat = VSM_Conversion.MSG_VSM.getLat();
            double Long = VSM_Conversion.MSG_VSM.getLong();
      
            this.App.Position = new PointLatLng(Lat, Long);
            Console.WriteLine("--------------------------------------------------");
            Console.WriteLine(Lat);
            Console.WriteLine(Long);
            Console.WriteLine("--------------------------------------------------");
        }

        private double CalculateDistance(PointLatLng point1, PointLatLng point2)
        {
            const double earthRadius = 6378137; // 지구 반지름 (미터)

            double dLat = (point2.Lat - point1.Lat) * (Math.PI / 180); // 위도 차이 (라디안)
            double dLng = (point2.Lng - point1.Lng) * (Math.PI / 180); // 경도 차이 (라디안)

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(point1.Lat * (Math.PI / 180)) * Math.Cos(point2.Lat * (Math.PI / 180)) *
                       Math.Sin(dLng / 2) * Math.Sin(dLng / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            double distance = earthRadius * c; // 거리 (미터)

            return distance;
        }
        // 마커 클릭 이벤트 핸들러
        private void MarkerClick(GMapMarker item, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right) // 오른쪽 마우스 클릭 확인
            {
                // 마커가 속한 오버레이 찾기
                foreach (var overlay in DroneOverlays.Values)
                {
                    ShowPopupContextMenu(item, e.Location);
                }
            }
        }
        private void ShowPopupContextMenu(GMapMarker item, Point location)
        {
            ContextMenuStrip contextMenu = new ContextMenuStrip();
            ToolStripMenuItem deleteMenuItem = new ToolStripMenuItem("Delete Marker");
            ToolStripMenuItem alldeleteMenuItem = new ToolStripMenuItem("Delete All Marker");
            ToolStripMenuItem vsmMenuItem = new ToolStripMenuItem("VSM / GIRO");
            ToolStripMenuItem AddMenuItem = new ToolStripMenuItem("Add Marker");

            Console.WriteLine("!!!!!!!!!!!!" + (GMapMarkerCustom)DroneOverlays["드론1"].Markers[0]);
           // Console.WriteLine("!!!!!!!!!!!!" + (GMapMarkerCustom)DroneOverlays["드론2"].Markers[0]);


            // 해당 마커를 포함한 오버레이 찾기
            foreach (var overlay in DroneOverlays.Values)
            {

                if (overlay.Markers.Contains(item))
                {
                    contextMenu.Items.Add(AddMenuItem);
                    Console.WriteLine("@@@@" + overlay.Markers);
                    contextMenu.Items.Add(deleteMenuItem);

                    deleteMenuItem.Click += (sender, e) =>
                    {
                        // 마커 제거
                        overlay.Markers.Remove(item);
                        // 해당 드론의 경로와 마커 모두 제거
                        string droneName = null;
                        foreach (var entry in DroneMarkerPoints)
                        {
                            if (entry.Value.Contains(item.Position))
                            {
                                droneName = entry.Key;
                                break;
                            }
                        }
                        if (droneName != null)
                        {
                            DroneMarkerPoints[droneName].Remove(item.Position);
                            RedrawRoute(droneName); // 경로 다시 그리기
                        }
                        // 모든 경로 다시 그리기
                        foreach (var key in DroneMarkerPoints.Keys)
                        {
                            RedrawRoute(key);
                        }
                    };

                    AddMenuItem.Click += (sender, e) =>
                    {
                        if (item.ToolTipText.Equals("New Marker1"))
                        {
                            currentDrone = "드론1";
                        };
                        /*if (item.ToolTipText.Equals("New Marker2"))
                        {
                            currentDrone = "드론2";
                        };*/
                    };


                    // 첫 번째 마커면 실행
                    if (overlay.Markers.IndexOf(item) == 0)
                    {
                        contextMenu.Items.Add(alldeleteMenuItem);
                        contextMenu.Items.Add(vsmMenuItem);

                        alldeleteMenuItem.Click += (sender, e) =>
                        {
                            string droneName = null;
                            foreach (var entry in DroneMarkerPoints)
                            {
                                if (entry.Value.Contains(item.Position))
                                {
                                    droneName = entry.Key;
                                    break;
                                }
                            }
                            PointLatLng positionToRemove = item.Position;
                            List<PointLatLng> markerPoints = DroneMarkerPoints[droneName];

                            // item.Position을 제외한 모든 요소 제거
                            markerPoints.RemoveAll(point => point != positionToRemove);
                            RedrawRoute(droneName); // 경로 다시 그리기

                            // 해당 마커를 제외한 나머지 마커들 제거
                            foreach (var marker in overlay.Markers.ToArray())
                            {
                                if (marker != item)
                                {
                                    overlay.Markers.Remove(marker);
                                    DroneMarkerPoints[droneName].Remove(marker.Position);
                                }
                            }
                        };

                        //giro 통신 받는 부분
                        vsmMenuItem.Click += (sender, e) =>
                        {
                            if (item.ToolTipText.Equals("New Marker1"))
                            {
                                GCS.TestVal = 0;
                            }
                            if (item.ToolTipText.Equals("New Marker2"))
                            {
                                GCS.TestVal = -51;
                            }
                            pictureBox1.Visible = true;
                        };
                    }
                }
            }
            contextMenu.Show(App, location);
        }
    }

    public class GMapMarkerCustom : GMapMarker
    {
        private Bitmap markerImage;

        public GMapMarkerCustom(PointLatLng p, Bitmap image)
            : base(p)
        {
            markerImage = image;
            Size = new Size(image.Width, image.Height);
            Offset = new Point(-Size.Width / 2, -Size.Height / 2);
        }

        public override void OnRender(Graphics g)
        {
            g.DrawImage(markerImage, LocalPosition.X, LocalPosition.Y, Size.Width, Size.Height);
        }

        public void ChangeImage(Bitmap newImage)
        {
            markerImage = newImage;
        }

    }
}