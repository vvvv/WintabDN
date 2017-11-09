///////////////////////////////////////////////////////////////////////////////
// HelpInfo.cs - WintabDN doxygen help info
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

/*! \mainpage WintabDN
 * 
 *  \brief WintabDN is a wrapper of the Wintab32 API that supports writing .NET
 *  compatible applications for Wacom digitizing tablets.
 *  
 *  \author Robert Cohn, Wacom Technology Corporation (rcohn@wacom.com)
 *  
 *  \par Revision History
 * <TABLE border="1">
 *      <TR> <TH>Revision Date</TH><TH>Revisor's Name (Email)</TH><TH>Change Description</TH><TH>Version</TH> </TR>
 *      <TR> <TD>11/13/2010</TD><TD>Robert Cohn (rcohn@wacom.com)</TD><TD>Initial Version</TD><TD>1.0</TD> </TR>
 *      <TR> <TD>03/15/2013</TD><TD>Robert Cohn (rcohn@wacom.com)</TD><TD>Added Wintab Extensions Support</TD><TD>1.1</TD> </TR>
 *      <TR> <TD>08/13/2013</TD><TD>Robert Cohn (rcohn@wacom.com)</TD><TD>Now requires .NET 4; fixed WtPacketsGet testing; marshalling improvements (from brett@brett.net.au)</TD><TD>1.2</TD> </TR>
 * </TABLE>
 * 
 * \section intro_sec Introduction
 *
 * The Wintab32 API (originally developed by LCS/Telegraphics in the early 1990s) was created to 
 * provide a standardized programming interface to digitizing tablets, and was early adopted by
 * Wacom Technology Corporation to support writing Windows operating system native C++ applications
 * for its pen digitizing tablets. A complete description of the Wintab32 API can be found in the 
 * <a href="Wintab_v140.htm">Wintab 1.4 specification</a>.
 * 
 * The WintabDN API was created to aid the development of managed code applications for Wacom's 
 * digital tablets.  This new API is .NET 2 compatible and will support the writing of applications
 * in any .NET supported language (such as C# or VB.NET).
 * 
 * With WintabDN, an application developer can, for example, easily write a .NET application to 
 * set up and capture pen data indicating X/Y location and pressure.  Other applications can be 
 * written to monitor pen tilt or rotation.  The API can be incorporated into many software 
 * applications where precise pen location data would be useful (such as MATLAB, or CAD applications).
 * 
 * WintabDN is a work in progress.  Version 1.0 only wraps a subset of the extensive Wintab32 native
 * implementation, and it is hoped that a growing community of developers will use WintabDN and 
 * contribute to its maintenance and extension.
 * 
 * \section contact_sec Contact Info
 * 
 * \brief If you have questions about using WintabDN, you can send email to this address: DeveloperEmailGroup@wacom.com
 * 
 * \brief Also, visit <a href="http://wacomeng.com/windows/index.html">Wacom Software Developer Support</a>
 * for general Wintab32 tablet programming support.
 * 
 * \page page1 Getting Started
 *
 * \section install_sec Installation
 * 
 * \subsection step1 Step 1: Install the Wacom tablet driver
 * WintabDN communicates with the Wacom tablet via the native code Wintab32.dll module.
 * This DLL is installed as part of the tablet driver software. It is best to get
 * the latest tablet driver software from the Wacom driver installation site at:
 * <a href="http://www.wacom.com/downloads/drivers.php">http://www.wacom.com/downloads/drivers.php</a>.
 * 
 * Simply install the latest driver software for your tablet type and operating system.
 * You may be prompted to reboot your system when the installation completes.
 * 
 * \subsection step2 Step 2: Test tablet driver installation
 * 
 * After installation, plug in your tablet and make sure you can move the mouse cursor
 * with the tablet pen.  Then, use your pen to open the table preferences dialog either
 * from the  Windows Start menu or the control panel.  For example, to open the preferences
 * dialog for a Bamboo tablet, you would select All Programs | Bamboo | Bamboo Preferences.
 * 
 * You can do a quick check to make sure that Wintab is communicating with the tablet 
 * driver before using WintabDN.  Go to the Wacom Software Developer Support plugin site
 * at: <a href="http://wacomeng.com/web/index.html">http://wacomeng.com/web/index.html</a>
 * and test one of the web plugins (for example, the Table demo).  If you can see the tablet
 * properties being updated as you move the pen around, the tablet driver software is
 * working correctly and communicating through Wintab.
 * 
 * Finally, test the sample application: FormTestApp, that comes with the WintabDN code
 * distribution.  Start that app and use your pen to press the "Test" button.  You should
 * see a stream of test output scroll down the left side of the dialog.  You should also
 * see varying pen X/Y/Pressure data as you press and lift the pen.
 * 
 * When you've had enough fun doing that, use your pen to press the "Scribble" button.  
 * Now you should be able to draw lines of varying thickness with your pen.  Note that the 
 * application will not move the system cursor when operating in the Sribble mode, so you
 * will have to use your mouse cursor (or track pad) to close the app.
 * 
 * The WintabDN distribution has full sources for this test application.
 * 
 * \subsection step3 Step 3: Building WintabDN.DLL and FormTestApp
 * 
 * The WintabDN project was built using Visual Studio 2010.  Other than the tablet driver
 * software (which includes installing the native Wintab32.dll), there are no other software
 * dependencies needed to build WintabDN.dll.  Just press "Build" and you're good to go.
 * 
 * The WintabDN solution will build both WintabDN.DLL and the FormTestApp application.
 * 
 * \subsection step4 Step 4: Building WintabDN Documentation
 * 
 * WintabDN documentation is generated using the doxygen document-generating tool, which can
 * be freely downloaded from: 
 * <a href="http://www.stack.nl/~dimitri/doxygen/">http://www.stack.nl/~dimitri/doxygen/</a>.
 * 
 * In addition to generating HTML web-help files, doxygen also uses the Windows HTML Help
 * tool to generate Windows compiled help (CHM) files. This tool can be freely downloaded from: 
 * <a href="http://msdn.microsoft.com/en-us/library/ms669985%28VS.85%29.aspx">
 * http://msdn.microsoft.com/en-us/library/ms669985%28VS.85%29.aspx</a>.
 * 
 * Document generation relies on comments within the code, as well as a doxygen configuration
 * file.  The configuration file for WintabDN is called, wintabdn.dox.
 * 
 * To generate documentation, execute the command: \par
 * <tt> <b>doxygen wintabdn.dox</b> </tt>
 *
 * \page page2 A WintabDN example
 * The WintabDN distribution comes with a sample application, FormTestApp, which exercises
 * most of the WintabDN functionality, as well as demonstrates how to use the API to build
 * a simple scribble application.
 * 
 * \section scribbleDemo_sec Scribble Demo
 * The Scribble demo shows how to set up a context, register for Wintab data packets,
 * provide a handler for Wintab events, and display graphics corresponding to pen X/Y 
 * position and pen pressure data.
 * 
 * The following code segment shows the "Scribble" button handler, which opens sets
 * up for pen data capture using the <tt><b>InitDataCapture()</b></tt> function.
 * \code
    private void scribbleButton_Click(object sender, EventArgs e)
    {
        ClearDisplay();
        CloseCurrentContext();
        Enable_Scribble(true);

        // Open a context and try to capture pen data;
        // Do not control system cursor.

        InitDataCapture(m_TABEXTX, m_TABEXTY, false);
    }
 * \endcode
 * 
 * <tt><b>InitDataCapture()</b></tt> makes sure it closes the current Wintab context, 
 * opens up a new context with <tt><b>OpenTestDigitizerContext()</b></tt>, creates a
 * Wintab data object with <tt><b>new CWintabData(m_logContext)</b></tt>, which uses
 * the context just created, and finally sets up a packet event handler for being 
 * notified when pen data comes in.  Note that the call to <tt><b>InitDataCapture()</b></tt>
 * is made with ctrlSysCursor_I = false, because we cannot be controlling the system
 * cursor while scribbling.
 * 
 * \code
    private void InitDataCapture(
    int ctxHeight_I = m_TABEXTX, int ctxWidth_I = m_TABEXTY, bool ctrlSysCursor_I = true)
    {
        try
        {
            // Close context from any previous test.
            CloseCurrentContext();

            m_logContext = OpenTestDigitizerContext(ctxWidth_I, ctxHeight_I,  ctrlSysCursor_I);

            if (m_logContext == null)
            {
                return;
            }

            // Create a data object and set its WT_PACKET handler.
            m_wtData = new CWintabData(m_logContext);
            m_wtData.SetWTPacketEventHandler(MyWTPacketEventHandler);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.ToString());
        }
    }
 * \endcode
 * 
 * When creating the digitizer context, we start with getting the default context using
 * <tt><b>CWintabInfo.GetDefaultDigitizingContext(ECTXOptionValues.CXO_MESSAGES)</b></tt>,
 * which specifies that we want to receive Wintab messages to be notified of pen data.
 * Since we are not specifying that we want to control the system cursor, the only other 
 * thing we have to override is the description of the logical extent of the tablet
 * coordinates.  For this example, we specify a logical tablet size of 10000 x 10000.
 * That's pretty much it.  We just tell WintabDN to open this context using
 * <tt><b>logContext.Open()</b></tt>.
 * 
 * \code
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
                return null;
            }

            // Modify the digitizing region.
            logContext.Name = "WintabDN Event Data Context";

            // output in a 10000 x 10000 grid
            logContext.OutOrgX = logContext.OutOrgY = 0;
            logContext.OutExtX = width_I;
            logContext.OutExtY = height_I;


            // Open the context, which will also tell Wintab to send data packets.
            status = logContext.Open();
        }
        catch (Exception ex)
        {
            TraceMsg("OpenTestDigitizerContext ERROR: " + ex.ToString());
        }

        return logContext;
    }
 * \endcode
 * 
 * Finally, we show that the event handler can easily access the pen data using
 * <tt><b>m_wtData.GetDataPacket(pktID)</b></tt>.  The packet contains values for the 
 * X/Y position and pen normal pressure.  The example also makes use of the packet timestamp,
 * which helps determine whether to draw a line between points or just draw a rectangle at the 
 * point. This allows the drawn lines to be less choppy when the user moves the pen quickly.
 * 
 * \code
    public void MyWTPacketEventHandler(Object sender_I, MessageReceivedEventArgs eventArgs_I)
    {
        try
        {
            if (m_maxPkts == 1)
            {
                uint pktID = (uint)eventArgs_I.Message.WParam;
                WintabPacket pkt = m_wtData.GetDataPacket(pktID);

                if (pkt.pkContext != 0)
                {
                    m_pkX = pkt.pkX;
                    m_pkY = pkt.pkY;
                    m_pressure = pkt.pkNormalPressure.pkAbsoluteNormalPressure;

                    m_pkTime = pkt.pkTime;

                    if (m_graphics != null)
                    {
                        // scribble mode
                        int clientWidth = scribblePanel.Width;
                        int clientHeight = scribblePanel.Height;

                        int X = (int)((double)(m_pkX * clientWidth) / (double)m_TABEXTX);
                        int Y = (int)((double)clientHeight - 
                            ((double)(m_pkY * clientHeight) / (double)m_TABEXTY));

                        Point tabPoint = new Point(X, Y);

                        if (m_lastPoint.Equals(Point.Empty))
                        {
                            m_lastPoint = tabPoint;
                            m_pkTimeLast = m_pkTime;
                        }

                        m_pen.Width = (float)(m_pressure / 200);
                        if (m_pressure > 0)
                        {
                            if (m_pkTime - m_pkTimeLast < 5)
                            {
                                m_graphics.DrawRectangle(m_pen, X, Y, 1, 1);
                            }
                            else
                            {
                                m_graphics.DrawLine(m_pen, tabPoint, m_lastPoint);
                            }
                        }

                        m_lastPoint = tabPoint;
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
 * \endcode
 * 
 * \section testWTInfo_sec Testing CWintabInfo
 * The testing in this section is just a demonstration of the various CWintabDN.CWintabInfo 
 * properties.  
 * 
 * Here is one of the first calls an application might make to determine if Wintab is properly
 * connected on the system:
 * \code
        bool isWintabAvailable = CWintabInfo.IsWintabAvailable();
 * \endcode
 * 
 * This is an example of how to find the number of tablet devices connected:
 * \code
        UInt32 numDevices = CWintabInfo.GetNumberOfDevices();
 * \endcode
 * 
 * Here is an example of how easy it is to get a default digitizing context (which is used in
 * the \ref scribbleDemo_sec example):
 * \code
        CWintabContext context = CWintabInfo.GetDefaultDigitizingContext();
 * \endcode
 * 
 * You can look through the other tests to see how some of the other global Wintab properties can
 * be accessed.
 * 
 * One of the tests, <tt><b>Test_GetDataPackets()</b></tt> gives a demonstration
 * of capturing data packets and writing them out to the list.  This demo is very similar
 * to the \ref scribbleDemo_sec, so we won't go into much detail here except to 
 * note that the call to <tt><b>InitDataCapture()</b></tt> is made with <tt><b>ctrlSysCursor_I</b></tt>
 * being true, so that the system cursor can be controlled with the pen.
 * 
 *  
 * \page page3 Primary WintabDN Classes
 * 
 * \section cwintabinfo_sec CWintabInfo
 * This section describes the class \ref WintabDN.CWintabInfo, which may be used to 
 * query and set global attributes for the connected tablet.  Such attributes include: 
 * tablet coordinates, physical dimensions, capabilities, and cursor types.
 * 
 * \section cwintabcontext_sec CWintabContext
 * 
 * This section describes the class \ref WintabDN.CWintabContext, which may be used 
 * when opening and manipulating Wintab contexts. This class contains everything 
 * applications and tablet managers need to know about a context. To simplify context 
 * manipulations, applications may want to take advantage of the default digitizing
 * context available via \ref WintabDN.CWintabInfo.GetDefaultDigitizingContext().
 * 
 * \section cwintabdata_sec CWintabData
 * 
 * This section describes the class \ref WintabDN.CWintabData, which may be used to
 * capture pen data from the target digitizing tablet.  Such data includes pen X/Y
 * location, pen pressure, time stamp, and much more.  See \ref WintabDN.WintabPacket
 * for a complete list of all supported pen tablet data.
 * 
 * 
 */