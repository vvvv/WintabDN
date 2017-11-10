///////////////////////////////////////////////////////////////////////////////
// TestForm.cs - Windows Forms test dialog for WintabDN
//
// Copyright (c) 2010, Wacom Technology Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
///////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;
using WintabDN;
using System.Threading;
using System.Windows;
using System.Diagnostics;

namespace FormTestApp
{
    public partial class TestForm : Form
    {
        private CWintabContext m_logContext = null; 
        private CWintabData m_wtData = null;
        private UInt32 m_maxPkts = 1;   // max num pkts to capture/display at a time

        private Int32 m_pkX = 0;
        private Int32 m_pkY = 0;
        private UInt32 m_pressure = 0;
        private UInt32 m_pkTime = 0;
        private UInt32 m_pkTimeLast = 0;

        private Point m_lastPoint = Point.Empty;
        private Graphics m_graphics;
        private Pen m_pen;
        private Pen m_backPen;

		 // These constants can be used to force Wintab X/Y data to map into a
		 // a 10000 x 10000 grid, as an example of mapping tablet data to values
		 // that make sense for your application.
        private const Int32 m_TABEXTX = 10000;
        private const Int32 m_TABEXTY = 10000;

        private bool m_showingTextButton = true;

        ///////////////////////////////////////////////////////////////////////
        public TestForm()
        {
            InitializeComponent();

            this.FormClosing += new FormClosingEventHandler(TestForm_FormClosing);
        }

        ///////////////////////////////////////////////////////////////////////
        public HCTX HLogContext { get { return m_logContext.HCtx; } }

         ///////////////////////////////////////////////////////////////////////
        private void TestForm_FormClosing(Object sender, FormClosingEventArgs e)
        {
            CloseCurrentContext();
        }

        ///////////////////////////////////////////////////////////////////////
        private void testButton_Click(object sender, EventArgs e)
        {
            // Close whatever context is open.
            CloseCurrentContext();

            if (m_showingTextButton)
            {   
                // Clear display and shut off scribble if it's on.
                ClearDisplay();
                Enable_Scribble(false); 
         
                // Set up to STOP the next time button is pushed.
                testButton.Text = "STOP";
                testButton.BackColor = Color.Orange;
                testLabel.Text = "Press STOP button to stop testing. (May take a few seconds to stop.)";
                m_showingTextButton = false;
             
                // Run the tests
                Test_IsWintabAvailable();
                Test_GetDeviceInfo();
                Test_GetDefaultDigitizingContext();
                Test_GetDefaultSystemContext();
                Test_GetDefaultDeviceIndex();
                Test_GetDeviceAxis();
                Test_GetDeviceOrientation();
                Test_GetDeviceRotation();
                Test_GetNumberOfDevices();
                Test_IsStylusActive();
                Test_GetStylusName();
                Test_GetExtensionMask();
                Test_Context();
                Test_DataPacketQueueSize();
                Test_MaxPressure();
                Test_GetDataPackets(1);
                Test_QueryDataPackets();     // opens up another form
            }
            else
            {
                testButton.Text = "Test...";
                testButton.BackColor = Color.Lime;
                testLabel.Text = "Press Test... button to start testing.";
                m_showingTextButton = true;
            }
        }

        ///////////////////////////////////////////////////////////////////////
        private void scribbleButton_Click(object sender, EventArgs e)
        {
            ClearDisplay();
            Enable_Scribble(true);

            // Control the system cursor with the pen.
            // TODO: set to false to NOT control the system cursor with pen.
            bool controlSystemCursor = true;

            // Open a context and try to capture pen data;
            InitDataCapture(m_TABEXTX, m_TABEXTY, controlSystemCursor);
        }

 
        ///////////////////////////////////////////////////////////////////////
        private void clearButton_Click(object sender, EventArgs e)
        {
            ClearDisplay();
        }

        ///////////////////////////////////////////////////////////////////////
        private void Test_IsWintabAvailable()
        {
            if (CWintabInfo.IsWintabAvailable())
            {
                TraceMsg("Wintab was found!\n");
            }
            else
            {
                TraceMsg("Wintab was not found!\nCheck to see if tablet driver service is running.\n");
            }
        }

        ///////////////////////////////////////////////////////////////////////
        private void Test_GetDeviceInfo()
        {
            //TraceMsg("DeviceInfo: " + CWintabInfo.GetDeviceInfo() + "\n");
            string devInfo = CWintabInfo.GetDeviceInfo();
            TraceMsg("DeviceInfo: " + devInfo + "\n");
        }


        ///////////////////////////////////////////////////////////////////////
        private void Test_GetDefaultDigitizingContext()
        {
            CWintabContext context = CWintabInfo.GetDefaultDigitizingContext();

            TraceMsg("Default Digitizing Context:\n");
            TraceMsg("\tSysOrgX, SysOrgY, SysExtX, SysExtY\t[" +
                context.SysOrgX + "," + context.SysOrgY + "," +
                context.SysExtX + "," + context.SysExtY + "]\n");

            TraceMsg("\tInOrgX, InOrgY, InExtX, InExtY\t[" +
                context.InOrgX + "," + context.InOrgY + "," +
                context.InExtX + "," + context.InExtY + "]\n");

            TraceMsg("\tOutOrgX, OutOrgY, OutExtX, OutExt\t[" +
                context.OutOrgX + "," + context.OutOrgY + "," +
                context.OutExtX + "," + context.OutExtY + "]\n");
        }

        ///////////////////////////////////////////////////////////////////////
        private void Test_GetDefaultSystemContext()
        {
            CWintabContext context = CWintabInfo.GetDefaultSystemContext();

            TraceMsg("Default System Context:\n");
            TraceMsg("\tSysOrgX, SysOrgY, SysExtX, SysExtY\t[" +
                context.SysOrgX + "," + context.SysOrgY + "," +
                context.SysExtX + "," + context.SysExtY + "]\n");

            TraceMsg("\tInOrgX, InOrgY, InExtX, InExtY\t[" +
                context.InOrgX + "," + context.InOrgY + "," +
                context.InExtX + "," + context.InExtY + "]\n");

            TraceMsg("\tOutOrgX, OutOrgY, OutExtX, OutExt\t[" +
                context.OutOrgX + "," + context.OutOrgY + "," +
                context.OutExtX + "," + context.OutExtY + "]\n");
        }

        ///////////////////////////////////////////////////////////////////////
        private void Test_GetDefaultDeviceIndex()
        {
            Int32 devIndex = CWintabInfo.GetDefaultDeviceIndex();

            TraceMsg("Default device index is: " + devIndex + (devIndex == -1 ? " (virtual device)\n" : "\n"));
        }

        ///////////////////////////////////////////////////////////////////////
        private void Test_GetDeviceAxis()
        {
            WintabAxis axis;

            // Get virtual device axis for X, Y and Z.
            axis = CWintabInfo.GetDeviceAxis(-1, EAxisDimension.AXIS_X);

            TraceMsg("Device axis X for virtual device:\n");
            TraceMsg("\taxMin, axMax, axUnits, axResolution: " + axis.axMin + "," + axis.axMax + "," + axis.axUnits + "," + axis.axResolution.ToString() + "\n");

            axis = CWintabInfo.GetDeviceAxis(-1, EAxisDimension.AXIS_Y);
            TraceMsg("Device axis Y for virtual device:\n");
            TraceMsg("\taxMin, axMax, axUnits, axResolution: " + axis.axMin + "," + axis.axMax + "," + axis.axUnits + "," + axis.axResolution.ToString() + "\n");

            axis = CWintabInfo.GetDeviceAxis(-1, EAxisDimension.AXIS_Z);
            TraceMsg("Device axis Z for virtual device:\n");
            TraceMsg("\taxMin, axMax, axUnits, axResolution: " + axis.axMin + "," + axis.axMax + "," + axis.axUnits + "," + axis.axResolution.ToString() + "\n");
        }

        ///////////////////////////////////////////////////////////////////////
        private void Test_GetDeviceOrientation()
        {
            bool tiltSupported = false;
            WintabAxisArray axisArray = CWintabInfo.GetDeviceOrientation(out tiltSupported);
            TraceMsg("Device orientation:\n");
            TraceMsg("\ttilt supported for current tablet: " + (tiltSupported ? "YES\n" : "NO\n"));

            if (tiltSupported)
            {
                for (int idx = 0; idx < axisArray.array.Length; idx++)
                {
                TraceMsg("\t[" + idx + "] axMin, axMax, axResolution, axUnits: " +
                    axisArray.array[idx].axMin + "," +
                    axisArray.array[idx].axMax + "," +
                    axisArray.array[idx].axResolution + "," +
                    axisArray.array[idx].axUnits + "\n");
                }
            }
        }

        ///////////////////////////////////////////////////////////////////////
        private void Test_GetDeviceRotation()
        {
            bool rotationSupported = false;
            WintabAxisArray axisArray = CWintabInfo.GetDeviceRotation(out rotationSupported);
            TraceMsg("Device rotation:\n");
            TraceMsg("\trotation supported for current tablet: " + (rotationSupported ? "YES\n" : "NO\n"));

            if (rotationSupported)
            {
                for (int idx = 0; idx < axisArray.array.Length; idx++)
                {
                    TraceMsg("\t[" + idx + "] axMin, axMax, axResolution, axUnits: " +
                        axisArray.array[idx].axMin + "," +
                        axisArray.array[idx].axMax + "," +
                        axisArray.array[idx].axResolution + "," +
                        axisArray.array[idx].axUnits + "\n");
                }
            }
        }

        ///////////////////////////////////////////////////////////////////////
        private void Test_GetNumberOfDevices()
        {
            UInt32 numDevices = CWintabInfo.GetNumberOfDevices();
            TraceMsg("Number of tablets connected: " + numDevices + "\n");
        }


        ///////////////////////////////////////////////////////////////////////
        private void Test_IsStylusActive()
        {
            bool isStylusActive = CWintabInfo.IsStylusActive();
            TraceMsg("Is stylus active: " + (isStylusActive ? "YES\n" : "NO\n"));
        }


        ///////////////////////////////////////////////////////////////////////
        private void Test_GetStylusName()
        {
            TraceMsg("Stylus name (puck):   " + CWintabInfo.GetStylusName(EWTICursorNameIndex.CSR_NAME_PUCK) + "\n");
            TraceMsg("Stylus name (pen):    " + CWintabInfo.GetStylusName(EWTICursorNameIndex.CSR_NAME_PRESSURE_STYLUS) + "\n");
            TraceMsg("Stylus name (eraser): " + CWintabInfo.GetStylusName(EWTICursorNameIndex.CSR_NAME_ERASER) + "\n");
        }

        ///////////////////////////////////////////////////////////////////////
        private void Test_GetExtensionMask()
        {
            TraceMsg("Extension touchring mask:   0x" + CWintabExtensions.GetWTExtensionMask(EWTXExtensionTag.WTX_TOUCHRING).ToString("x") + "\n");
            TraceMsg("Extension touchstring mask: 0x" + CWintabExtensions.GetWTExtensionMask(EWTXExtensionTag.WTX_TOUCHSTRIP).ToString("x") + "\n");
            TraceMsg("Extension express key mask: 0x" + CWintabExtensions.GetWTExtensionMask(EWTXExtensionTag.WTX_EXPKEYS2).ToString("x") + "\n");
        }

        ///////////////////////////////////////////////////////////////////////
        private void Test_Context()
        {
            bool status = false;
            CWintabContext logContext = null;

            try
            {
                logContext = OpenTestDigitizerContext();
                if (logContext == null)
                {
                    TraceMsg("Test_Context: FAILED OpenTestDigitizerContext - bailing out...\n");
                    return;
                }

                status = logContext.Enable(true);
                TraceMsg("Context Enable: " + (status ? "PASSED" : "FAILED") + "\n");

                status = logContext.SetOverlapOrder(false);
                TraceMsg("Context SetOverlapOrder to bottom: " + (status ? "PASSED" : "FAILED") + "\n");
                status = logContext.SetOverlapOrder(true);
                TraceMsg("Context SetOverlapOrder to top: " + (status ? "PASSED" : "FAILED") + "\n");

                TraceMsg("Modified Context:\n");
                TraceMsg("  Name: " + logContext.Name + "\n");
                TraceMsg("  Options: " + logContext.Options + "\n");
                TraceMsg("  Status: " + logContext.Status + "\n");
                TraceMsg("  Locks: " + logContext.Locks + "\n");
                TraceMsg("  MsgBase: " + logContext.MsgBase + "\n");
                TraceMsg("  Device: " + logContext.Device + "\n");
                TraceMsg("  PktRate: 0x" + logContext.PktRate.ToString("x") + "\n");
                TraceMsg("  PktData: 0x" + ((uint)logContext.PktData).ToString("x") + "\n");
                TraceMsg("  PktMode: 0x" + ((uint)logContext.PktMode).ToString("x") + "\n");
                TraceMsg("  MoveMask: " + logContext.MoveMask + "\n");
                TraceMsg("  BZtnDnMask: 0x" + logContext.BtnDnMask.ToString("x") + "\n");
                TraceMsg("  BtnUpMask: 0x" + logContext.BtnUpMask.ToString("x") + "\n");
                TraceMsg("  InOrgX: " + logContext.InOrgX + "\n");
                TraceMsg("  InOrgY: " + logContext.InOrgY + "\n");
                TraceMsg("  InOrgZ: " + logContext.InOrgZ + "\n");
                TraceMsg("  InExtX: " + logContext.InExtX + "\n");
                TraceMsg("  InExtY: " + logContext.InExtY + "\n");
                TraceMsg("  InExtZ: " + logContext.InExtZ + "\n");
                TraceMsg("  OutOrgX: " + logContext.OutOrgX + "\n");
                TraceMsg("  OutOrgY: " + logContext.OutOrgY + "\n");
                TraceMsg("  OutOrgZ: " + logContext.OutOrgZ + "\n");
                TraceMsg("  OutExtX: " + logContext.OutExtX + "\n");
                TraceMsg("  OutExtY: " + logContext.OutExtY + "\n");
                TraceMsg("  OutExtZ: " + logContext.OutExtZ + "\n");
                TraceMsg("  SensX: " + logContext.SensX + "\n");
                TraceMsg("  SensY: " + logContext.SensY + "\n");
                TraceMsg("  SensZ: " + logContext.SensZ + "\n");
                TraceMsg("  SysMode: " + logContext.SysMode + "\n");
                TraceMsg("  SysOrgX: " + logContext.SysOrgX + "\n");
                TraceMsg("  SysOrgY: " + logContext.SysOrgY + "\n");
                TraceMsg("  SysExtX: " + logContext.SysExtX + "\n");
                TraceMsg("  SysExtY: " + logContext.SysExtY + "\n");
                TraceMsg("  SysSensX: " + logContext.SysSensX + "\n");
                TraceMsg("  SysSensY: " + logContext.SysSensY + "\n");
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ToString());
            }
            finally
            {
                if (logContext != null)
                {
                    status = logContext.Close();
                    TraceMsg("Context Close: " + (status ? "PASSED" : "FAILED") + "\n");
                }
            }
        }

        ///////////////////////////////////////////////////////////////////////
        private CWintabContext OpenTestDigitizerContext(
            int width_I = m_TABEXTX, int height_I = m_TABEXTY, bool ctrlSysCursor = true)
        {
            bool status = false;
            CWintabContext logContext = null;

            try
            {
                // Get the default digitizing context.
                // Default is to receive data events.
                logContext = CWintabInfo.GetDefaultDigitizingContext(ECTXOptionValues.CXO_MESSAGES);

                // Set system cursor if caller wants it.
                if (ctrlSysCursor)
                {
                    logContext.Options |= (uint)ECTXOptionValues.CXO_SYSTEM;
                }

                if (logContext == null)
                {
                    TraceMsg("FAILED to get default digitizing context.\n");
                    return null;
                }

                // Modify the digitizing region.
                logContext.Name = "WintabDN Event Data Context";

                // output in a grid of the specified dimensions.
                logContext.OutOrgX = logContext.OutOrgY = 0;
                logContext.OutExtX = width_I;
                logContext.OutExtY = height_I;


                // Open the context, which will also tell Wintab to send data packets.
                status = logContext.Open();

                TraceMsg("Context Open: " + (status ? "PASSED [ctx=" + logContext.HCtx + "]" : "FAILED") + "\n");
            }
            catch (Exception ex)
            {
                TraceMsg("OpenTestDigitizerContext ERROR: " + ex.ToString());
            }

            return logContext;
        }

        ///////////////////////////////////////////////////////////////////////
        private CWintabContext OpenTestSystemContext(
            int width_I = m_TABEXTX, int height_I = m_TABEXTY, bool ctrlSysCursor = true)
        {
            bool status = false;
            CWintabContext logContext = null;

            try
            {
                // Get the default system context.
                // Default is to receive data events.
                //logContext = CWintabInfo.GetDefaultDigitizingContext(ECTXOptionValues.CXO_MESSAGES);
                logContext = CWintabInfo.GetDefaultSystemContext(ECTXOptionValues.CXO_MESSAGES);

                // Set system cursor if caller wants it.
                if (ctrlSysCursor)
                {
                    logContext.Options |= (uint)ECTXOptionValues.CXO_SYSTEM;
                }
                else
                {
                    logContext.Options &= ~(uint)ECTXOptionValues.CXO_SYSTEM;
                }

                if (logContext == null)
                {
                    TraceMsg("FAILED to get default digitizing context.\n");
                    return null;
                }

                // Modify the digitizing region.
                logContext.Name = "WintabDN Event Data Context";

                WintabAxis tabletX = CWintabInfo.GetTabletAxis(EAxisDimension.AXIS_X);
                WintabAxis tabletY = CWintabInfo.GetTabletAxis(EAxisDimension.AXIS_Y);

                logContext.InOrgX = 0;
                logContext.InOrgY = 0;
                logContext.InExtX = tabletX.axMax;
                logContext.InExtY = tabletY.axMax;

                // SetSystemExtents() is (almost) a NO-OP redundant if you opened a system context.
                SetSystemExtents( ref logContext );

                // Open the context, which will also tell Wintab to send data packets.
                status = logContext.Open();

                TraceMsg("Context Open: " + (status ? "PASSED [ctx=" + logContext.HCtx + "]" : "FAILED") + "\n");
            }
            catch (Exception ex)
            {
                TraceMsg("OpenTestDigitizerContext ERROR: " + ex.ToString());
            }

            return logContext;
        }

        ///////////////////////////////////////////////////////////////////////
        private void Test_DataPacketQueueSize()
        {
            bool status = false;
            UInt32 numPackets = 0;
            CWintabContext logContext = null;

            try
            {
                logContext = OpenTestDigitizerContext();

                if (logContext == null)
                {
                    TraceMsg("Test_DataPacketQueueSize: FAILED OpenTestDigitizerContext - bailing out...\n");
                    return;
                }

                CWintabData wtData = new CWintabData(logContext);
                TraceMsg("Creating CWintabData object: " + (wtData != null ? "PASSED" : "FAILED") + "\n");
                if (wtData == null)
                {
                    throw new Exception("Could not create CWintabData object.");
                }

                numPackets = wtData.GetPacketQueueSize();
                TraceMsg("Initial packet queue size: " + numPackets + "\n");

                status = wtData.SetPacketQueueSize(17);
                TraceMsg("Setting packet queue size: " + (status ? "PASSED" : "FAILED") + "\n");

                numPackets = wtData.GetPacketQueueSize();
                TraceMsg("Modified packet queue size: " + numPackets + "\n");
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ToString());
            }
            finally
            {
                if (logContext != null)
                {
                    status = logContext.Close();
                    TraceMsg("Context Close: " + (status ? "PASSED" : "FAILED") + "\n");
                }
            }
        }

        ///////////////////////////////////////////////////////////////////////
        private void Test_MaxPressure()
        {
            TraceMsg("Max normal pressure is: " + CWintabInfo.GetMaxPressure() + "\n");
            TraceMsg("Max tangential pressure is: " + CWintabInfo.GetMaxPressure(false) + "\n");
        }

        ///////////////////////////////////////////////////////////////////////
        private void Test_GetDataPackets(UInt32 maxNumPkts_I)
        {
            // Set up to capture/display maxNumPkts_I packet at a time.
            m_maxPkts = maxNumPkts_I;

            // Open a context and try to capture pen data.
            InitDataCapture();

            // Touch pen to the tablet.  You should see data appear in the TestForm window.
        }

        ///////////////////////////////////////////////////////////////////////
        private void Test_QueryDataPackets()
        {
            QueryDataForm qdForm = new QueryDataForm();

            qdForm.ShowDialog();

        }



        ///////////////////////////////////////////////////////////////////////
        private void Enable_Scribble(bool enable = false)
        {
            if (enable)
            {
                // Set up to capture 1 packet at a time.
                m_maxPkts = 1;

                // Init scribble graphics.
                //m_graphics = CreateGraphics();
                m_graphics = scribblePanel.CreateGraphics();
                m_graphics.SmoothingMode = SmoothingMode.AntiAlias;

                m_pen = new Pen(Color.Black);
                m_backPen = new Pen(Color.White);

                scribbleButton.BackColor = Color.Lime;
                scribbleLabel.Visible = true;
                testButton.BackColor = Color.WhiteSmoke;
                testLabel.Text = "Press Test... button to start testing.";

                // You should now be able to scribble in the scribblePanel.
            }
            else
            {
                // Remove scribble context.
                CloseCurrentContext();

                // Turn off graphics.
                if (m_graphics != null)
                {
                    scribblePanel.Invalidate();
                    m_graphics = null;
                }

                scribbleButton.BackColor = Color.WhiteSmoke;
                scribbleLabel.Visible = false;
                testButton.BackColor = Color.Lime;
                testLabel.Text = "Press Test button and hold pen on tablet to start testing.";
            }
        }


        ///////////////////////////////////////////////////////////////////////
        // Helper functions
        //

        ///////////////////////////////////////////////////////////////////////
        private void InitDataCapture(
				int ctxWidth_I = m_TABEXTX, int ctxHeight_I = m_TABEXTY, bool ctrlSysCursor_I = true)
        {
            try
            {
                // Close context from any previous test.
                CloseCurrentContext();

                TraceMsg("Opening context...\n");

                m_logContext = OpenTestSystemContext(ctxWidth_I, ctxHeight_I, ctrlSysCursor_I);

                if (m_logContext == null)
                {
                    TraceMsg("Test_DataPacketQueueSize: FAILED OpenTestSystemContext - bailing out...\n");
                    return;
                }

                // Create a data object and set its WT_PACKET handler.
                m_wtData = new CWintabData(m_logContext);
                m_wtData.SetWTPacketEventHandler(MyWTPacketEventHandler);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ToString());
            }
        }

        ///////////////////////////////////////////////////////////////////////
        private void CloseCurrentContext()
        {
            try
            {
                TraceMsg("Closing context...\n");
                if (m_logContext != null)
                {
                    m_logContext.Close();
                    m_logContext = null;
                    m_wtData = null;
                }

            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ToString());
            }
        }

        ///////////////////////////////////////////////////////////////////////
        void TraceMsg(string msg)
        {
            testTextBox.AppendText(msg);

            // Scroll to bottom of list.
            testTextBox.SelectionLength = 0;
            testTextBox.SelectionStart = testTextBox.Text.Length;
            testTextBox.ScrollToCaret();
        }

        ///////////////////////////////////////////////////////////////////////
        // Sets logContext.Out
        //
        // Note: 
        // SystemParameters.VirtualScreenLeft{Top} and SystemParameters.VirtualScreenWidth{Height} 
        // don't always give correct answers.
        //
        // Uncomment the TODO code below that enumerates all system displays 
        // if you want to customize.
        // Else assume the passed-in extents were already set by call to WTInfo,
        // in which case we still have to invert the Y extent.
        private void SetSystemExtents(ref CWintabContext logContext)
        {
           //TODO Rectangle rect = new Rectangle(int.MaxValue, int.MaxValue, int.MinValue, int.MinValue);

           //TODO foreach (Screen screen in Screen.AllScreens)
           //TODO    rect = Rectangle.Union(rect, screen.Bounds);

           //TODO logContext.OutOrgX = rect.Left;
           //TODO logContext.OutOrgY = rect.Top;
           //TODO logContext.OutExtX = rect.Width;
           //TODO logContext.OutExtY = rect.Height;

           // In Wintab, the tablet origin is lower left.  Move origin to upper left
           // so that it coincides with screen origin.
           logContext.OutExtY = -logContext.OutExtY;
        }

        ///////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Called when Wintab WT_PACKET events are received.
        /// </summary>
        /// <param name="sender_I">The EventMessage object sending the report.</param>
        /// <param name="eventArgs_I">eventArgs_I.Message.WParam contains ID of packet containing the data.</param>
        public void MyWTPacketEventHandler(Object sender_I, MessageReceivedEventArgs eventArgs_I)
        {
            //System.Diagnostics.Debug.WriteLine("Received WT_PACKET event");
            if (m_wtData == null)
            {
                return;
            }

            try
            {
                if (m_maxPkts == 1)
                {
                    uint pktID = (uint)eventArgs_I.Message.WParam;
                    WintabPacket pkt = m_wtData.GetDataPacket(eventArgs_I.Message.LParam, pktID);
                    //DEPRECATED WintabPacket pkt = m_wtData.GetDataPacket(pktID);

                    if (pkt.pkContext.IsValid)
                    {
                        m_pkX = pkt.pkX;
                        m_pkY = pkt.pkY;
                        m_pressure = pkt.pkNormalPressure;

								Trace.WriteLine("SCREEN: pkX: " + pkt.pkX + ", pkY:" + pkt.pkY + ", pressure: " + pkt.pkNormalPressure);

                        m_pkTime = pkt.pkTime;

                        if (m_graphics == null)
                        {
                            // display data mode
                            TraceMsg("Received WT_PACKET event[" + pktID + "]: X/Y/P = " + 
                                pkt.pkX + " / " + pkt.pkY + " / " + pkt.pkNormalPressure + "\n");
                        }
                        else
                        {
                            // scribble mode
                            int clientWidth = scribblePanel.Width;
                            int clientHeight = scribblePanel.Height;

                            // m_pkX and m_pkY are in screen (system) coordinates.

                            Point clientPoint = scribblePanel.PointToClient(new Point(m_pkX, m_pkY));
                            Trace.WriteLine("CLIENT:   X: " + clientPoint.X + ", Y:" + clientPoint.Y);

                            if (m_lastPoint.Equals(Point.Empty))
                            {
                                m_lastPoint = clientPoint;
                                m_pkTimeLast = m_pkTime;
                            }

                            m_pen.Width = (float)(m_pressure / 200);

                            if (m_pressure > 0)
                            {
                                if (m_pkTime - m_pkTimeLast < 5)
                                {
                                    m_graphics.DrawRectangle(m_pen, clientPoint.X, clientPoint.Y, 1, 1);
                                }
                                else
                                {
                                    m_graphics.DrawLine(m_pen, clientPoint, m_lastPoint);
                                }
                            }

                            m_lastPoint = clientPoint;
                            m_pkTimeLast = m_pkTime;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("FAILED to get packet data: " + ex.ToString());
            }
        }

        private void ClearDisplay()
        {
            testTextBox.Clear();
            scribblePanel.Invalidate();
        }

        private void testQDPButton_Click(object sender, EventArgs e)
        {
            Test_QueryDataPackets();
        }

		  private void scribblePanel_Resize(object sender, EventArgs e)
		  {
			  if (m_graphics != null)
			  {
				  m_graphics.Dispose();
				  m_graphics = scribblePanel.CreateGraphics();
				  m_graphics.SmoothingMode = SmoothingMode.AntiAlias;

				  Trace.WriteLine(
					  "ScribblePanel: X:" + scribblePanel.Left + ",Y:" +  scribblePanel.Top + 
					  ", W:" + scribblePanel.Width + ", H:" + scribblePanel.Height);
			  }
		  }
    }
}
