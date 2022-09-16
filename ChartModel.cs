using System.IO.Ports;
using System.Threading.Tasks;
using System.Windows.Forms;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Threading;
using System.Data;

namespace SpasticityClientV2
{
    public partial class ChartModel : Form
    {
        private bool IsCancelled = false;

        private int keepRecords = 100;
        public bool IsRunning { get; set; }
        public List<SessionData> SessionDatas { get; set; }

        const int myWidth = 1800; // width of graphs
        const int perfScale = 1;
        const int perfWidth = myWidth / perfScale;

        Bitmap ForceA = new Bitmap(myWidth, 300);
        Bitmap EMGA = new Bitmap(myWidth, 300);
        Bitmap AngleA = new Bitmap(myWidth, 300);
        Bitmap ForceB = new Bitmap(myWidth, 300);
        Bitmap EMGB = new Bitmap(myWidth, 300);
        Bitmap AngleB = new Bitmap(myWidth, 300);

        Graphics ForceImageA;
        Graphics EMGImageA;
        Graphics AngleImageA;
        Graphics ForceImageB;
        Graphics EMGImageB;
        Graphics AngleImageB;

        Pen ForcePen = new Pen(System.Drawing.Color.Black, 2); // width 3
        Pen EMGPen = new Pen(System.Drawing.Color.Green, 3);
        Pen AnglePen = new Pen(System.Drawing.Color.Red, 3);
        Pen graticule = new Pen(System.Drawing.Color.Gray, 1);

        Rectangle eraseRectangle = new Rectangle(0, 0, 100, 100);

        int forceWrIndex = 0;
        int forceRdIndex = 0;

        int perfNumber;

        const int perfIndexMax = 1800; // 4096 samples
        const int dataIndexMax = perfIndexMax;

        int[] forceArray = new int[dataIndexMax];  
        int[] emgArray = new int[dataIndexMax];    
        int[] angleArray = new int[dataIndexMax];

        void RedrawForceGraphA()
        {
            int i;
            Point A = new Point(0, 0);
            Point B = new Point(0, 0);

            ForceImageA.FillRectangle(Brushes.White, eraseRectangle);

            A.X = 0;
            B.X = myWidth;
            // Draw 0mmHg line
            A.Y = 300;
            B.Y = 300;
            ForceImageA.DrawLine(graticule, A, B);
           
            // Draw Vertical lines for 30 seconds intervals
            A.Y = 0;
            B.Y = 299;
            for (i = 1; i < 6; i++)
            {
                A.X = i * 300;
                B.X = A.X;
                ForceImageA.DrawLine(graticule, A, B);
            }
        }

        void RedrawForceGraphB()
        {
            int i;
            Point A = new Point(0, 0);
            Point B = new Point(0, 0);

            ForceImageB.FillRectangle(Brushes.White, eraseRectangle);

            A.X = 0;
            B.X = myWidth;
            // Draw 0mmHg line
            A.Y = 300;
            B.Y = 300;
            ForceImageB.DrawLine(graticule, A, B);

            // Draw Vertical lines for 30 seconds intervals
            A.Y = 0;
            B.Y = 299;
            for (i = 1; i < 6; i++)
            {
                A.X = i * 300;
                B.X = A.X;
                ForceImageB.DrawLine(graticule, A, B);
            }
        }

        void RedrawEMGGraphA()
        {
            int i;
            Point A = new Point(0, 0);
            Point B = new Point(0, 0);

            EMGImageA.FillRectangle(Brushes.White, eraseRectangle);

            A.X = 0;
            B.X = myWidth;
            // Draw 0mmHg line
            A.Y = 300;
            B.Y = 300;
            EMGImageA.DrawLine(graticule, A, B);

            // Draw Vertical lines for 30 seconds intervals
            A.Y = 0;
            B.Y = 299;
            for (i = 1; i < 6; i++)
            {
                A.X = i * 300;
                B.X = A.X;
                EMGImageA.DrawLine(graticule, A, B);
            }
        }

        void RedrawEMGGraphB()
        {
            int i;
            Point A = new Point(0, 0);
            Point B = new Point(0, 0);

            EMGImageB.FillRectangle(Brushes.White, eraseRectangle);

            A.X = 0;
            B.X = myWidth;
            // Draw 0mmHg line
            A.Y = 300;
            B.Y = 300;
            EMGImageB.DrawLine(graticule, A, B);

            // Draw Vertical lines for 30 seconds intervals
            A.Y = 0;
            B.Y = 299;
            for (i = 1; i < 6; i++)
            {
                A.X = i * 300;
                B.X = A.X;
                EMGImageB.DrawLine(graticule, A, B);
            }
        }

        void RedrawAngleGraphA()
        {
            int i;
            Point A = new Point(0, 0);
            Point B = new Point(0, 0);

            AngleImageA.FillRectangle(Brushes.White, eraseRectangle);

            A.X = 0;
            B.X = myWidth;
            // Draw 0mmHg line
            A.Y = 300;
            B.Y = 300;
            AngleImageA.DrawLine(graticule, A, B);

            // Draw Vertical lines for 30 seconds intervals
            A.Y = 0;
            B.Y = 299;
            for (i = 1; i < 6; i++)
            {
                A.X = i * 300;
                B.X = A.X;
                AngleImageA.DrawLine(graticule, A, B);
            }
        }

        void RedrawAngleGraphB()
        {
            int i;
            Point A = new Point(0, 0);
            Point B = new Point(0, 0);

            AngleImageB.FillRectangle(Brushes.White, eraseRectangle);

            A.X = 0;
            B.X = myWidth;
            // Draw 0mmHg line
            A.Y = 300;
            B.Y = 300;
            AngleImageB.DrawLine(graticule, A, B);

            // Draw Vertical lines for 30 seconds intervals
            A.Y = 0;
            B.Y = 299;
            for (i = 1; i < 6; i++)
            {
                A.X = i * 300;
                B.X = A.X;
                AngleImageB.DrawLine(graticule, A, B);
            }
        }

        public ChartModel()
        {
            int i;
            Point A = new Point(0, 0);
            Point B = new Point(0, 0);

            InitializeComponent();

            // Initialize data arrays
            for (i = 0; i < dataIndexMax; i++)
            {
                forceArray[i] = 0;
                emgArray[i] = 0;
                angleArray[i] = 0;
            }

            ForceImageA = Graphics.FromImage(ForceA);
            EMGImageA = Graphics.FromImage(EMGA);  
            AngleImageA = Graphics.FromImage(AngleA);
            ForceImageB = Graphics.FromImage(ForceB);
            EMGImageB = Graphics.FromImage(EMGB);
            AngleImageB = Graphics.FromImage(AngleB);

            eraseRectangle.Width = ForceA.Width - 1;
            eraseRectangle.Height = ForceA.Height - 1;

            RedrawForceGraphA();
            RedrawEMGGraphA();
            RedrawAngleGraphA();
            pictureBox1.Image = ForceA;
            pictureBox2.Image = EMGA;
            pictureBox3.Image = AngleA;

            forceWrIndex = 0;
            forceRdIndex = 0;
        }

        public void getAvailablePorts()
        {
            string[] ports = SerialPort.GetPortNames();
            comboBox2.Items.AddRange(ports);
        }
        private void button3_Click(object sender, EventArgs e)
        {
            string[] ports = SerialPort.GetPortNames();
            comboBox2.Items.AddRange(ports);
        }
        private void button4_Click(object sender, EventArgs e)
        {
            if (comboBox2.SelectedIndex > -1)
            {
                MessageBox.Show(String.Format("You selected port '{0}'", comboBox2.SelectedItem));
                Connect(comboBox2.SelectedItem.ToString());
                timer1.Start();
            }
            else
            {
                MessageBox.Show("Please select a port first");
            }
        }
        private void Connect(string portName)
        {
            var port = new SerialPort(portName);
            if (!port.IsOpen)
            {
                port.BaudRate = 57600;
                port.Parity = Parity.None;
                port.StopBits = StopBits.One;
                port.DataBits = 8;
                port.Open();

                port.DataReceived += new SerialDataReceivedEventHandler(serialDataRecievedEventHandler);
                //Continue here....
            }
        }
        private void ChartModel_Load (object sender, EventArgs e)
        {

        }
        private void serialDataRecievedEventHandler (object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sData = sender as SerialPort;

            try
            {
                //var nowstart = DateTime.Now;
                var remainHex = string.Empty;
                var packetRemainData = new List<string>();

                float timeDiff = 0;
                float lastElapsedTime = 0;
                float lastAngle = 0;
                float angleDiff = 0;
                float angVel = 0;

                float forceDiff = 0;
                float initialForce = 0;

                var counter = 1;
                var loopIndex = 0;
                List<float> forceCalArray = new List<float>();
                //var nowstart = DateTime.Now;

                //infinite loop will keep running and adding to EMGInfo until IsCancelled is set to true
                while (IsCancelled == false)
                {
                    //Check if any bytes of data received in serial buffer
                    var totalbytes = sData.BytesToRead;
                    Thread.Sleep(30);

                    if (loopIndex == 0) { var nowstart = DateTime.Now; };
                    if (totalbytes > 0)
                    {
                        //Load all the serial data to buffer
                        var buffer = new byte[totalbytes];
                        var nowticks = DateTime.Now;
                        sData.Read(buffer, 0, buffer.Length);

                        //convert bytes to hex to better visualize and parse. 
                        //TODO: it can be updated in the future to parse with byte to increase preformance if needed
                        var hexFull = BitConverter.ToString(buffer);

                        //remainhex is empty string
                        hexFull = remainHex + hexFull;

                        var packets = new List<XBeePacket>();

                        //remainHex is all that is left when legitimate packets have been added to packets
                        remainHex = XBeeFunctions.ParsePacketHex(hexFull.Split('-').ToList(), packets);

                        foreach (var packet in packets)
                        {
                            //Total transmitted data is [] byte long. 1 more byte should be checksum. prefixchar is the extra header due to API Mode
                            int prefixCharLength = 8;
                            int byteArrayLength = 25;
                            int checkSumLength = 1;
                            int totalExpectedCharLength = prefixCharLength + byteArrayLength + checkSumLength;

                            //Based on above variables to parse data coming from SerialPort. Next fun is performed sequentially to all packets
                            var packetDatas = XBeeFunctions.ParseRFDataHex(packet.Data, packetRemainData, totalExpectedCharLength);

                            foreach (var packetData in packetDatas)
                            {
                                //Make sure it's 25 charactors long. It's same as the arduino receiver code for checking the length. This was previously compared to totalExpectedCharLength but looks like packetDatas - packetData only contains the data part anyway therefore compare to byteArrayLength
                                //Also modify data defn to be packetData itself
                                if (packetData.Count == (prefixCharLength + byteArrayLength + checkSumLength))
                                {
                                    var data = packetData;

                                    //convert timestamp
                                    var TIME2MSB = Convert.ToByte(data[8], 16);
                                    var TIME2LSB = Convert.ToByte(data[9], 16);
                                    var TIME1MSB = Convert.ToByte(data[10], 16);
                                    var TIME1LSB = Convert.ToByte(data[11], 16);

                                    //convert rectified EMG
                                    var EMGMSB = Convert.ToByte(data[12], 16);
                                    var EMGLSB = Convert.ToByte(data[13], 16);

                                    //convert force
                                    var FORMSB = Convert.ToByte(data[14], 16);
                                    var FORLSB = Convert.ToByte(data[15], 16);

                                    //convert potentiometer edge computer angle and angvel values. 
                                    var POTANGLEMSB = Convert.ToByte(data[16], 16);
                                    var POTANGLELSB = Convert.ToByte(data[17], 16);

                                    float elapsedTime = (long)((TIME2MSB & 0xFF) << 24 | (TIME2LSB & 0xFF) << 16 | (TIME1MSB & 0xFF) << 8 | (TIME1LSB & 0xFF));
                                    float emg = (int)(EMGMSB & 0xFF) << 8 | (EMGLSB & 0xFF);
                                    float force = (int)((FORMSB & 0xFF) << 8 | (FORLSB & 0xFF));
                                    float angle = (int)((POTANGLEMSB & 0xFF) << 8 | (POTANGLELSB & 0xFF));

                                    string forceString = force.ToString();
                                    //serialData.Invoke((MethodInvoker)delegate { serialData.AppendText(forceString); });

                                    forceArray[loopIndex] = (int)force;
                                    emgArray[loopIndex] = (int)emg;
                                    angleArray[loopIndex] = (int)angle;

                                    forceWrIndex = (forceWrIndex + 1) & (dataIndexMax - 1);  // increment Cuff Write Index

                                    #region Calibrate out starting force bias 
                                    if (counter < 20)
                                    {
                                        forceCalArray.Add(force);
                                    }
                                    initialForce = forceCalArray.Min();
                                    forceDiff = force - initialForce;
                                    #endregion

                                    #region Send data to Excel collection
                                    //SessionDatas.Add(new SessionData
                                    //{
                                    //    TimeStamp = (long)elapsedTime,
                                    //    Angle_deg = angle,
                                    //    AngVel_degpersec = angVel,
                                    //    EMG_mV = emg,
                                    //    Force_N = forceDiff
                                    //}); ;
                                    #endregion

                                    counter++;
                                    loopIndex++;
                                    //forceWrIndex++;
                                    Thread.Sleep(30);
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                Dispose();
            }
        }
        public void SaveData()
        {
            if (!IsRunning)
            {
                using (ExcelEngine excelEngine = new ExcelEngine())
                {
                    IApplication application = excelEngine.Excel;
                    application.DefaultVersion = ExcelVersion.Excel2016;

                    //Create a workbook and enable calculations
                    IWorkbook workbook = application.Workbooks.Create(1);
                    IWorksheet worksheet = workbook.Worksheets[0];
                    worksheet.EnableSheetCalculations();

                    //Import data from SessionDatas
                    ExcelImportDataOptions importDataOptions = new ExcelImportDataOptions();
                    importDataOptions.FirstRow = 1;
                    importDataOptions.FirstColumn = 1;
                    importDataOptions.IncludeHeader = true;
                    importDataOptions.PreserveTypes = false;
                    worksheet.ImportData(SessionDatas, importDataOptions);

                    #region Calculate summary statistics and first quartile, third quartile, and interquartile range
                    //Set Labels
                    //Creating a new style with cell back color, fill pattern and font attribute
                    IStyle headingStyle = workbook.Styles.Add("NewStyle");
                    headingStyle.Color = Syncfusion.Drawing.Color.PowderBlue;
                    headingStyle.Font.Bold = true;

                    IStyle statisticStyle = workbook.Styles.Add("NewStyle2");
                    statisticStyle.Color = Syncfusion.Drawing.Color.DarkSlateBlue;
                    statisticStyle.Font.Bold = true;

                    IStyle tableBodyStyle = workbook.Styles.Add("NewStyle3");
                    tableBodyStyle.Color = Syncfusion.Drawing.Color.LightGray;

                    worksheet.Range["H1:K1"].CellStyle = headingStyle;
                    worksheet.Range["M1"].CellStyle = headingStyle;
                    worksheet.Range["G2:G11"].CellStyle = statisticStyle;
                    worksheet.Range["H2:K11"].CellStyle = tableBodyStyle;
                    worksheet.Range["M2:M11"].CellStyle = tableBodyStyle;
                    worksheet.Range["G1:K11"].CellStyle.Borders.Color = ExcelKnownColors.White;

                    worksheet.Range["M1"].Text = "Description";
                    worksheet.Range["G2"].Text = "Min";
                    worksheet.Range["M2"].Text = "Least value in the dataset";
                    worksheet.Range["G3"].Text = "Max";
                    worksheet.Range["M3"].Text = "Greatest value in the dataset";
                    worksheet.Range["G4"].Text = "Range";
                    worksheet.Range["M4"].Text = "Difference between least and greatest values";
                    worksheet.Range["G5"].Text = "Mean";
                    worksheet.Range["M5"].Text = "Average of data";
                    worksheet.Range["G6"].Text = "StDev";
                    worksheet.Range["M6"].Text = "Standard deviation of data";
                    worksheet.Range["G7"].Text = "Q1";
                    worksheet.Range["M7"].Text = "First quartile";
                    worksheet.Range["G8"].Text = "Q3";
                    worksheet.Range["M8"].Text = "Third quartile";
                    worksheet.Range["G9"].Text = "IQR";
                    worksheet.Range["M9"].Text = "Interquartile range - difference between first and third quartiles";
                    worksheet.Range["G10"].Text = "L Bound";
                    worksheet.Range["M10"].Text = "Lower bound based on 1.5x IQR";
                    worksheet.Range["G11"].Text = "U Bound";
                    worksheet.Range["M11"].Text = "Upper bound based on 1.5x IQR";
                    worksheet.Range["H1"].Text = "Angle";
                    worksheet.Range["I1"].Text = "AngVel";
                    worksheet.Range["J1"].Text = "EMG";
                    worksheet.Range["K1"].Text = "Force";

                    //Angle
                    worksheet.Range["H2"].Formula = "=MIN(B:B)";
                    worksheet.Range["H3"].Formula = "=MAX(B:B)";
                    worksheet.Range["H4"].Formula = "=ABS(H3-H2)";
                    worksheet.Range["H5"].Formula = "=AVERAGE(B:B)";
                    worksheet.Range["H6"].Formula = "=STDEV.P(B:B)";

                    worksheet.Range["H7"].Formula = "=QUARTILE(B:B,1)";
                    worksheet.Range["H8"].Formula = "=QUARTILE(B:B,3)";
                    worksheet.Range["H9"].Formula = "=ABS(H8-H7)";
                    worksheet.Range["H10"].Formula = "=H7-(H9*1.5)";
                    worksheet.Range["H11"].Formula = "=H8+(H9*1.5)";

                    //Angular Velocity
                    worksheet.Range["I2"].Formula = "=MIN(C:C)";
                    worksheet.Range["I3"].Formula = "=MAX(C:C)";
                    worksheet.Range["I4"].Formula = "=ABS(I3-I2)";
                    worksheet.Range["I5"].Formula = "=AVERAGE(C:C)";
                    worksheet.Range["I6"].Formula = "=STDEV.P(C:C)";

                    worksheet.Range["I7"].Formula = "=QUARTILE(C:C,1)";
                    worksheet.Range["I8"].Formula = "=QUARTILE(C:C,3)";
                    worksheet.Range["I9"].Formula = "=ABS(I8-I7)";
                    worksheet.Range["I10"].Formula = "=I7-(I9*1.5)";
                    worksheet.Range["I11"].Formula = "=I8+(I9*1.5)";

                    //EMG
                    worksheet.Range["J2"].Formula = "=MIN(D:D)";
                    worksheet.Range["J3"].Formula = "=MAX(D:D)";
                    worksheet.Range["J4"].Formula = "=ABS(J3-J2)";
                    worksheet.Range["J5"].Formula = "=AVERAGE(D:D)";
                    worksheet.Range["J6"].Formula = "=STDEV.P(D:D)";

                    worksheet.Range["J7"].Formula = "=QUARTILE(D:D,1)";
                    worksheet.Range["J8"].Formula = "=QUARTILE(D:D,3)";
                    worksheet.Range["J9"].Formula = "=ABS(J8-J7)";
                    worksheet.Range["J10"].Formula = "=J7-(J9*1.5)";
                    worksheet.Range["J11"].Formula = "=J8+(J9*1.5)";

                    //Force
                    worksheet.Range["K2"].Formula = "=MIN(E:E)";
                    worksheet.Range["K3"].Formula = "=MAX(E:E)";
                    worksheet.Range["K4"].Formula = "=ABS(K3-K2)";
                    worksheet.Range["K5"].Formula = "=AVERAGE(E:E)";
                    worksheet.Range["K6"].Formula = "=STDEV.P(E:E)";

                    worksheet.Range["K7"].Formula = "=QUARTILE(E:E,1)";
                    worksheet.Range["K8"].Formula = "=QUARTILE(E:E,3)";
                    worksheet.Range["K9"].Formula = "=ABS(K8-K7)";
                    worksheet.Range["K10"].Formula = "=K7-(K9*1.5)";
                    worksheet.Range["K11"].Formula = "=K8+(K9*1.5)";
                    #endregion

                    #region Highlight outliers
                    //Angle
                    //Applying conditional formatting to Angle column
                    IConditionalFormats _condition1 = worksheet.Range["B:B"].ConditionalFormats;
                    IConditionalFormat condition1 = _condition1.AddCondition();
                    condition1.FormatType = ExcelCFType.CellValue;
                    condition1.Operator = ExcelComparisonOperator.NotBetween;
                    condition1.FirstFormula = worksheet.Range["H10"].FormulaNumberValue.ToString();
                    condition1.SecondFormula = worksheet.Range["H11"].FormulaNumberValue.ToString();
                    condition1.BackColorRGB = Syncfusion.Drawing.Color.FromArgb(200, 100, 100);
                    worksheet.Range["B1"].ConditionalFormats.Remove();

                    //Angular Velocity
                    //Applying conditional formatting to Angular Velocity column
                    IConditionalFormats _condition2 = worksheet.Range["C:C"].ConditionalFormats;
                    IConditionalFormat condition2 = _condition2.AddCondition();
                    condition2.FormatType = ExcelCFType.CellValue;
                    condition2.Operator = ExcelComparisonOperator.NotBetween;
                    condition2.FirstFormula = worksheet.Range["I10"].FormulaNumberValue.ToString();
                    condition2.SecondFormula = worksheet.Range["I11"].FormulaNumberValue.ToString();
                    condition2.BackColorRGB = Syncfusion.Drawing.Color.FromArgb(200, 100, 100);
                    worksheet.Range["C1"].ConditionalFormats.Remove();

                    //EMG
                    //Applying conditional formatting to EMG column
                    IConditionalFormats _condition3 = worksheet.Range["D:D"].ConditionalFormats;
                    IConditionalFormat condition3 = _condition3.AddCondition();
                    condition3.FormatType = ExcelCFType.CellValue;
                    condition3.Operator = ExcelComparisonOperator.NotBetween;
                    condition3.FirstFormula = worksheet.Range["J10"].FormulaNumberValue.ToString();
                    condition3.SecondFormula = worksheet.Range["J11"].FormulaNumberValue.ToString();
                    condition3.BackColorRGB = Syncfusion.Drawing.Color.FromArgb(200, 100, 100);
                    worksheet.Range["D1"].ConditionalFormats.Remove();

                    //Force
                    //Applying conditional formatting to Force column
                    IConditionalFormats _condition4 = worksheet.Range["E:E"].ConditionalFormats;
                    IConditionalFormat condition4 = _condition4.AddCondition();
                    condition4.FormatType = ExcelCFType.CellValue;
                    condition4.Operator = ExcelComparisonOperator.NotBetween;
                    condition4.FirstFormula = worksheet.Range["K10"].FormulaNumberValue.ToString();
                    condition4.SecondFormula = worksheet.Range["K11"].FormulaNumberValue.ToString();
                    condition4.BackColorRGB = Syncfusion.Drawing.Color.FromArgb(200, 100, 100);
                    worksheet.Range["E1"].ConditionalFormats.Remove();

                    #endregion

                    #region Format data as table
                    //Create table with the data in given range
                    IListObject table = worksheet.ListObjects.Create("Table1", worksheet["A:E"]);
                    table.BuiltInTableStyle = TableBuiltInStyles.TableStyleMedium8;
                    #endregion

                    #region Autofit columns
                    worksheet.Range["A:E"].AutofitColumns();
                    worksheet.Range["H:K"].AutofitColumns();
                    worksheet.Range["M:M"].AutofitColumns();
                    #endregion

                    #region Charts

                    string rowcount = worksheet.UsedRange.LastRow.ToString();
                    string timeDataRange = "A2:A" + rowcount;
                    string angleDataRange = "B2:B" + rowcount;
                    string angvelDataRange = "C2:C" + rowcount;
                    string emgDataRange = "D2:D" + rowcount;
                    string forceDataRange = "E2:E" + rowcount;

                    //Add Angle Chart
                    IChartShape angleChart = worksheet.Charts.Add();
                    //Set first serie
                    IChartSerie Angle = angleChart.Series.Add("Angle");
                    Angle.Values = worksheet.Range[angleDataRange];
                    Angle.UsePrimaryAxis = true;
                    Angle.CategoryLabels = worksheet.Range[timeDataRange];
                    //Set chart details
                    angleChart.PlotArea.Border.AutoFormat = true;
                    angleChart.ChartType = ExcelChartType.Scatter_Line;
                    angleChart.HasTitle = false;
                    angleChart.IsSizeWithCell = true;
                    angleChart.Left = 584;
                    ((IChart)angleChart).Width = 836;
                    angleChart.Top = 240;
                    //Set primary value axis properties
                    angleChart.PrimaryValueAxis.Title = "Angle (�)";
                    angleChart.PrimaryCategoryAxis.Title = "Time (s)";
                    angleChart.PrimaryValueAxis.TitleArea.TextRotationAngle = -90;
                    angleChart.PrimaryCategoryAxis.HasMajorGridLines = false;
                    angleChart.PrimaryValueAxis.HasMajorGridLines = false;
                    angleChart.PrimaryValueAxis.IsAutoMax = true;
                    angleChart.PrimaryValueAxis.MinimumValue = 0;
                    //Legend position
                    angleChart.Legend.Position = ExcelLegendPosition.Bottom;
                    //View legend horizontally
                    angleChart.Legend.IsVerticalLegend = false;

                    //Add AngularVelocity Chart
                    IChartShape angvelChart = worksheet.Charts.Add();
                    //Set first serie
                    IChartSerie AngularVelocity = angvelChart.Series.Add("Angular Velocity");
                    AngularVelocity.Values = worksheet.Range[angvelDataRange];
                    AngularVelocity.UsePrimaryAxis = true;
                    AngularVelocity.CategoryLabels = worksheet.Range[timeDataRange];
                    //Set chart details
                    angvelChart.PlotArea.Border.AutoFormat = true;
                    angvelChart.ChartType = ExcelChartType.Scatter_Line;
                    angvelChart.HasTitle = false;
                    angvelChart.IsSizeWithCell = true;
                    angvelChart.Left = 584;
                    ((IChart)angvelChart).Width = 836;
                    angvelChart.Top = 640;
                    //Set primary value axis properties
                    angvelChart.PrimaryValueAxis.Title = "Angular Velocity (�/s)";
                    angvelChart.PrimaryCategoryAxis.Title = "Time (s)";
                    angvelChart.PrimaryValueAxis.TitleArea.TextRotationAngle = -90;
                    angvelChart.PrimaryCategoryAxis.HasMajorGridLines = false;
                    angvelChart.PrimaryValueAxis.HasMajorGridLines = false;
                    angvelChart.PrimaryValueAxis.IsAutoMax = true;
                    angvelChart.PrimaryValueAxis.MinimumValue = 0;
                    //Legend position
                    angvelChart.Legend.Position = ExcelLegendPosition.Bottom;
                    //View legend horizontally
                    angvelChart.Legend.IsVerticalLegend = false;

                    //Add EMG Chart
                    IChartShape emgChart = worksheet.Charts.Add();
                    //Set first serie
                    IChartSerie EMG = emgChart.Series.Add("EMG");
                    EMG.Values = worksheet.Range[emgDataRange];
                    EMG.UsePrimaryAxis = true;
                    EMG.CategoryLabels = worksheet.Range[timeDataRange];
                    //Set chart details
                    emgChart.PlotArea.Border.AutoFormat = true;
                    emgChart.ChartType = ExcelChartType.Scatter_Line;
                    emgChart.HasTitle = false;
                    emgChart.IsSizeWithCell = true;
                    emgChart.Left = 584;
                    ((IChart)emgChart).Width = 836;
                    emgChart.Top = 1040;
                    //Set primary value axis properties
                    emgChart.PrimaryValueAxis.Title = "EMG (mV)";
                    emgChart.PrimaryCategoryAxis.Title = "Time (s)";
                    emgChart.PrimaryValueAxis.TitleArea.TextRotationAngle = -90;
                    emgChart.PrimaryCategoryAxis.HasMajorGridLines = false;
                    emgChart.PrimaryValueAxis.HasMajorGridLines = false;
                    emgChart.PrimaryValueAxis.IsAutoMax = true;
                    emgChart.PrimaryValueAxis.MinimumValue = 0;
                    //Legend position
                    emgChart.Legend.Position = ExcelLegendPosition.Bottom;
                    //View legend horizontally
                    emgChart.Legend.IsVerticalLegend = false;

                    //Add Force Chart
                    IChartShape forceChart = worksheet.Charts.Add();
                    //Set first serie
                    IChartSerie Force = forceChart.Series.Add("Force");
                    Force.Values = worksheet.Range[forceDataRange];
                    Force.UsePrimaryAxis = true;
                    Force.CategoryLabels = worksheet.Range[timeDataRange];
                    //Set chart details
                    forceChart.PlotArea.Border.AutoFormat = true;
                    forceChart.ChartType = ExcelChartType.Scatter_Line;
                    forceChart.HasTitle = false;
                    forceChart.IsSizeWithCell = true;
                    forceChart.Left = 584;
                    ((IChart)forceChart).Width = 836;
                    forceChart.Top = 1440;
                    //Set primary value axis properties
                    forceChart.PrimaryValueAxis.Title = "Force (N)";
                    forceChart.PrimaryCategoryAxis.Title = "Time (s)";
                    forceChart.PrimaryValueAxis.TitleArea.TextRotationAngle = -90;
                    forceChart.PrimaryCategoryAxis.HasMajorGridLines = false;
                    forceChart.PrimaryValueAxis.HasMajorGridLines = false;
                    forceChart.PrimaryValueAxis.IsAutoMax = true;
                    forceChart.PrimaryValueAxis.MinimumValue = 0;
                    //Legend position
                    forceChart.Legend.Position = ExcelLegendPosition.Bottom;
                    //View legend horizontally
                    forceChart.Legend.IsVerticalLegend = false;
                    #endregion

                    //Set path and save
                    string spreadsheetNamePath = "acquiredData/";
                    string spreadsheetNameDate = DateTime.Now.ToString("dddd dd MMM y HHmmss");
                    string spreadsheetName = spreadsheetNamePath + spreadsheetNameDate;
                    //I CHANGED THIS
                    string path = Path.GetDirectoryName(Environment.ProcessPath)
                    + "\\acquiredData\\";

                    worksheet.DisableSheetCalculations();
                    // convert string to stream
                    byte[] byteArray = Encoding.UTF8.GetBytes(spreadsheetName + ".xlsx");

                    MemoryStream stream = new MemoryStream(byteArray);
                    workbook.SaveAs(stream);

                    #region View the Workbook

                    Process.Start(path + spreadsheetNameDate + ".xlsx");

                    // Now that the file has been created, delete contents of SessionDatas
                    // Thread.Sleep(500);
                    SessionDatas.Clear();
                    #endregion
                }
            }
        }

        private void timer1_Tick_1(object sender, EventArgs e)
        {
            int i, number, length;
            int imageIndex;
            Point A = new Point(0, 0);
            Point B = new Point(0, 300);


            // update cuff graphics
            if (forceWrIndex != forceRdIndex)
            {
                if (forceWrIndex % 2 == 0)
                {// ReDraw Graphs A
                    RedrawForceGraphA();
                    RedrawEMGGraphA();
                    RedrawAngleGraphA();

                    // set up first point
                    imageIndex = (forceWrIndex + (dataIndexMax - myWidth)) & (dataIndexMax - 1);
                    A.X = 0;
                    A.Y = 300 - angleArray[imageIndex];
                    //above used to be force
                    for (i = 1; i < myWidth; i++)
                    {
                        imageIndex = (imageIndex + 1) & (dataIndexMax - 1);
                        B.X = i;
                        B.Y = 300 - angleArray[imageIndex];
                        //used to be forcearray above
                        ForceImageA.DrawLine(ForcePen, A, B);
                        EMGImageA.DrawLine(EMGPen, A, B);
                        AngleImageA.DrawLine(AnglePen, A, B);
                        A.X = B.X;
                        A.Y = B.Y;
                    }
                    pictureBox1.Image = ForceA;
                    pictureBox2.Image = EMGA;
                    pictureBox3.Image = AngleA;
                }
                else
                {// Redraw Graphs B
                    RedrawForceGraphB();
                    RedrawEMGGraphB();
                    RedrawAngleGraphB();

                    // set up first point
                    imageIndex = (forceWrIndex + (dataIndexMax - myWidth)) & (dataIndexMax - 1);
                    A.X = 0;
                    A.Y = 300 - angleArray[imageIndex];
                    //used to be force above
                    for (i = 1; i < myWidth; i++)
                    {
                        imageIndex = (imageIndex + 1) & (dataIndexMax - 1);
                        B.X = i;
                        B.Y = 300 - angleArray[imageIndex];
                        //same here
                        ForceImageB.DrawLine(ForcePen, A, B);
                        EMGImageB.DrawLine(EMGPen, A, B);
                        AngleImageB.DrawLine(AnglePen, A, B);
                        A.X = B.X;
                        A.Y = B.Y;
                    }
                    pictureBox1.Image = ForceB;
                    pictureBox2.Image = EMGB;
                    pictureBox3.Image = AngleB;
                }
                forceRdIndex = (forceRdIndex + 1) & (dataIndexMax - 1);  // increment cuff pressure index
            }
        }
    }
}
