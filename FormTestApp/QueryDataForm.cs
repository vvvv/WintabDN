///////////////////////////////////////////////////////////////////////////////
// QueryDataForm.cs - Windows Forms test dialog for WintabDN
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
using System.Windows.Forms;

namespace WintabDN
{
    public partial class QueryDataForm : Form
    {
        private CWintabContext m_logContext = null; 
        private CWintabData m_wtData = null;

        UInt32 m_maxPackets = 10;
        const bool REMOVE = true;

        public QueryDataForm()
        {
            InitializeComponent();

            try
            {
                // Open a Wintab context that does not send Wintab data events.
                m_logContext = OpenQueryDigitizerContext();

                // Create a data object.
                m_wtData = new CWintabData(m_logContext);

                m_wtData.SetWTPacketEventHandler(MyWTPacketEventHandler);

                //TraceMsg("Press \"Test\" and touch pen to tablet.\n");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private CWintabContext OpenQueryDigitizerContext()
        {
            bool status = false;
            CWintabContext logContext = null;

            try
            {
                // Get the default digitizing context.  Turn off events.  Control system cursor.
                logContext = CWintabInfo.GetDefaultDigitizingContext(ECTXOptionValues.CXO_SYSTEM);

                logContext.Options |= (uint)ECTXOptionValues.CXO_MESSAGES;
                logContext.Options &= ~(uint)ECTXOptionValues.CXO_SYSTEM;

                if (logContext == null)
                {
                    TraceMsg("OpenQueryDigitizerContext: FAILED to get default digitizing context.\n");
                    //System.Diagnostics.Debug.WriteLine("FAILED to get default digitizing context.");
                    return null;
                }

                // Modify the digitizing region.
                logContext.Name = "WintabDN Query Data Context";

                //WintabAxis tabletX = CWintabInfo.GetTabletAxis(EAxisDimension.AXIS_X);
                //WintabAxis tabletY = CWintabInfo.GetTabletAxis(EAxisDimension.AXIS_Y);

               //// Output X/Y values 1-1 with tablet dimensions.
               // logContext.OutOrgX = logContext.OutOrgY = 0;
               // logContext.OutExtX = tabletX.axMax;
               // logContext.OutExtY = tabletY.axMax;

                logContext.SysOrgX = logContext.SysOrgY = 0;
                logContext.SysExtX = SystemInformation.PrimaryMonitorSize.Width;
                logContext.SysExtY = SystemInformation.PrimaryMonitorSize.Height;

                // Open the context, which will also tell Wintab to send data packets.
                status = logContext.Open();

                TraceMsg("Context Open: " + (status ? "PASSED [ctx=" + logContext.HCtx + "]" : "FAILED") + "\n");
                //System.Diagnostics.Debug.WriteLine("Context Open: " + (status ? "PASSED [ctx=" + logContext.HCtx + "]" : "FAILED"));
            }
            catch (Exception ex)
            {
                TraceMsg("OpenQueryDigitizerContext: ERROR : " + ex.ToString());
            }

            return logContext;
        }



        /// <summary>
        /// Responds to pen data by removing or peek/flushing data.
        /// </summary>
        /// <param name="sender_I"></param>
        /// <param name="eventArgs_I"></param>
        public void MyWTPacketEventHandler(Object sender_I, MessageReceivedEventArgs eventArgs_I)
        {
            UInt32 numPkts = 0;

            //System.Diagnostics.Debug.WriteLine("Received WT_PACKET event");
            if (m_wtData == null)
            {
                return;
            }

            bool removeData = this.removeRadioButton.Checked;

            try
            {
                // If removeData is true, packets are removed as they are read.
                // If removeData is false, peek at packets only (packets flushed below).
                WintabPacket[] packets = m_wtData.GetDataPackets(m_maxPackets, removeData, ref numPkts);

                if (numPkts > 0)
                {
                    for (int idx = 0; idx < packets.Length; idx++)
                    {
                        TraceMsg(
                            "Context:" + packets[idx].pkContext +
                            " Status:" + packets[idx].pkStatus +
                            " ID:" + packets[idx].pkSerialNumber +
                            " X:" + packets[idx].pkX +
                            " Y:" + packets[idx].pkY +
                            " P:" + packets[idx].pkNormalPressure + "\n");
                    }

                    // If the peek button was pressed, then flush the packets we just peeked at.
                    if (!removeData)
                    {
                        TraceMsg("Flushing " + numPkts.ToString() + " pending data packets...\n\n");
                        m_wtData.FlushDataPackets(numPkts);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("MyWTPacketEventHandler ERROR: " + ex.ToString());
            }
        }


        ///////////////////////////////////////////////////////////////////////
        private void clearButton_Click(object sender, EventArgs e)
        {
            testTextBox.Clear();
        }

        ///////////////////////////////////////////////////////////////////////
        void TraceMsg(string msg)
        {
            testTextBox.AppendText(msg);

            // Scroll to bottom of list.
            testTextBox.SelectionLength = 0;
            testTextBox.SelectionStart = testTextBox.Text.Length;
            testTextBox.ScrollToCaret();

            testTextBox.Update();
        }

    }
}
