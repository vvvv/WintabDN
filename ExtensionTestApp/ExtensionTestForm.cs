///////////////////////////////////////////////////////////////////////////////
// ExtensionTestForm.cs - WintabDN extensions test dialog
//
// Copyright (c) 2013, Wacom Technology Corporation
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
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing.Drawing2D;

using WintabDN;

// This definition is somewhat equivalent to a C++ typedef.
using WTPKT = System.UInt32;

namespace FormExtTestApp
{
    /// <summary>
    /// Exercise the CWintabExtensions API.
    /// NOTE: This demo only supports handling a context for one tablet.
    /// 
    /// TODO - add support for multiple tablet contexts
    /// </summary>
    public partial class ExtensionTestForm : Form
    {
        // the one and only tablet used in this demo
        private UInt32 mTabletIndexDefault = 0;

        private UInt32 mExpKeysMask = 0;
        private UInt32 mTouchRingMask = 0;
        private UInt32 mTouchStripMask = 0;

        private ExtensionControlState mExtensionControlState = null;

        private CWintabContext mLogContext = null;

        private CWintabData m_wtData = null;

        // Graphics objects for rendering touch rings and touch strips.
        private Pen mPen = null;
        private Pen mFingerPen = null;

        // Constants used for touchring operation.
        private const float mPI = (float)3.14159265358979323846;
        private const float mPIDIV2 = (float)1.57079632679489661923;

        private String mTestImageIconPath = "";

        private const String mTestImageIDEIconPath = "..\\..\\sample.png";
        private const String mTestImageDefaultIconPath = ".\\sample.png";

        public ExtensionTestForm()
        {
            InitializeComponent();

            mExtensionControlState = new ExtensionControlState();

            mPen = new Pen(Color.Black);
            mPen.Width = 2;

            mFingerPen = new Pen(Color.Orange);
            mFingerPen.Width = 2;

            mTestImageIconPath =
                System.IO.File.Exists(mTestImageDefaultIconPath) ? mTestImageDefaultIconPath :
                System.IO.File.Exists(mTestImageIDEIconPath) ? mTestImageIDEIconPath :
                "";
        }

        private bool InitWintab()
        {
            bool status = false;

            try
            {
                mLogContext = CWintabInfo.GetDefaultDigitizingContext(ECTXOptionValues.CXO_MESSAGES);
                if (mLogContext == null)
                {
                    return false;
                    //throw new Exception("Oops - FAILED GetDefaultDigitizingContext");
                }

                // Control system cursor.
                mLogContext.Options |= (UInt32)ECTXOptionValues.CXO_SYSTEM;

                // Verify which extensions are available for targeting.
                // Once we know what the tablet supports, we can set up the data packet
                // definition to be sent events from those control types.

                // All tablets should have at least expresskeys.
                mExpKeysMask = CWintabExtensions.GetWTExtensionMask(EWTXExtensionTag.WTX_EXPKEYS2);

                if (mExpKeysMask > 0)
                {
                    mLogContext.PktData |= (WTPKT)mExpKeysMask;
                }
                else
                {
                    Debug.WriteLine("InitWintab: WTX_EXPKEYS2 mask not found!");
                    throw new Exception("Oops - FAILED GetWTExtensionMask for WTX_EXPKEYS2");
                }

                // It's not an error if either / both of these are zero.  It simply means
                // that those control types are not supported.
                mTouchRingMask = CWintabExtensions.GetWTExtensionMask(EWTXExtensionTag.WTX_TOUCHRING);
                if (mTouchRingMask > 0)
                {
                    mLogContext.PktData |= (WTPKT)mTouchRingMask;
                }

                mTouchStripMask = CWintabExtensions.GetWTExtensionMask(EWTXExtensionTag.WTX_TOUCHSTRIP);
                if (mTouchStripMask > 0)
                {
                    mLogContext.PktData |= (WTPKT)mTouchStripMask;
                }

                status = mLogContext.Open();
                if (!status)
                {
                    //throw new Exception("Oops - failed logContext.Open()");
                    return false;
                }

                // Setup controls and overrides for first tablet.
                SetupControlsForTablet(mTabletIndexDefault);

                // Create a data object and set its WT_PACKET handler.
                m_wtData = new CWintabData(mLogContext);
                m_wtData.SetWTPacketEventHandler(MyWTPacketEventHandler);

            }
            catch (Exception ex)
            {
                MessageBox.Show("FormExtTestApp: InitWintab: " + ex.ToString());
            }

            return true;
        }



        /// <summary>
        /// Iterate through and setup all controls on tablet.
        /// </summary>
        /// <param name="tabletIndex_I">Zero-based tablet index</param>
        void SetupControlsForTablet(UInt32 tabletIndex_I)
        {
            if (mExpKeysMask > 0)
            {
                SetupControlsForExtension(tabletIndex_I, EWTXExtensionTag.WTX_EXPKEYS2, mExtensionControlState.SetupExpressKey);
            }

            if (mTouchRingMask > 0)
            {
                SetupControlsForExtension(tabletIndex_I, EWTXExtensionTag.WTX_TOUCHRING, mExtensionControlState.SetupTouchRing);
            }

            if (mTouchStripMask > 0)
            {
                SetupControlsForExtension(tabletIndex_I, EWTXExtensionTag.WTX_TOUCHSTRIP, mExtensionControlState.SetupTouchStrip);
            }
        }

        /// <summary>
        /// Remove all extension overrides for the tablet.
        /// </summary>
        /// <param name="tabletIndex_I"></param>
        void RemoveOverridesForTablet(UInt32 tabletIndex_I)
        {
            // Express Keys
            RemoveOverridesForExtension(tabletIndex_I, EWTXExtensionTag.WTX_EXPKEYS2);
            // Touch Rings
            RemoveOverridesForExtension(tabletIndex_I, EWTXExtensionTag.WTX_TOUCHRING);
            // Touch Strips
            RemoveOverridesForExtension(tabletIndex_I, EWTXExtensionTag.WTX_TOUCHSTRIP);

        }


        /// <summary>
        /// Remove application overrides for extension.
        /// </summary>
        /// <param name="tabletIndex_I"></param>
        /// <param name="extTagIndex_I"></param>
        void RemoveOverridesForExtension(
            UInt32 tabletIndex_I,
            EWTXExtensionTag extTagIndex_I)
        {
            UInt32 numCtrls = 0;
            UInt32 numFuncs = 0;
            UInt32 propOverride = 0;  // false

            try
            {
                // Get number of controls of this type.
                if ( !CWintabExtensions.ControlPropertyGet(
                    mLogContext.HCtx,
                    (byte)extTagIndex_I,
                    (byte)tabletIndex_I,
                    0, // ignored
                    0, // ignored
                    (ushort)EWTExtensionTabletProperty.TABLET_PROPERTY_CONTROLCOUNT,
                    ref numCtrls) )
                { throw new Exception("Oops - Failed ControlPropertyGet for TABLET_PROPERTY_CONTROLCOUNT"); }

                // All tablets should have ExpressKeys (we assume).
                if (numCtrls == 0 && EWTXExtensionTag.WTX_EXPKEYS2 == extTagIndex_I)
                { throw new Exception("Oops - SetupControlsForExtension didn't find any ExpressKeys!"); }

                // For each control, find its number of functions ...
                for (UInt32 controlIndex = 0; controlIndex < numCtrls; controlIndex++)
                {
                    if (!CWintabExtensions.ControlPropertyGet(
                        mLogContext.HCtx,
                        (byte)extTagIndex_I,
                        (byte)tabletIndex_I,
                        (byte)controlIndex,
                        0, // ignored
                        (ushort)EWTExtensionTabletProperty.TABLET_PROPERTY_FUNCCOUNT,
                        ref numFuncs))
                    { throw new Exception("Oops - Failed ControlPropertyGet for TABLET_PROPERTY_FUNCCOUNT"); }

                    // ... and override our setting for each function.
                    for (UInt32 functionIndex = 0; functionIndex < numFuncs; functionIndex++)
                    {
                        if (! CWintabExtensions.ControlPropertySet(
                            mLogContext.HCtx,
                            (byte)extTagIndex_I,
                            (byte)tabletIndex_I,
                            (byte)controlIndex,
                            (byte)functionIndex,
                            (ushort)EWTExtensionTabletProperty.TABLET_PROPERTY_OVERRIDE,
                            propOverride))
                        { throw new Exception("Oops - FAILED ControlPropertySet for TABLET_PROPERTY_OVERRIDE"); }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        /// Iterate through and setup all controls on this extension. 
        /// </summary>
        /// <param name="tabletIndex_I">tablet index</param>
        /// <param name="extTagIndex_I">extension index tag</param>
        /// <param name="setupFunc_I">function called to setup extension control layout</param>
        public void SetupControlsForExtension(
            UInt32 tabletIndex_I,
            EWTXExtensionTag extTagIndex_I,
            DrawControlsSetupFunction setupFunc_I)
        {
            UInt32 numCtrls = 0;

            try
            {
                // Get number of controls of this type.
                if ( !CWintabExtensions.ControlPropertyGet(
                    mLogContext.HCtx,
                    (byte)extTagIndex_I,
                    (byte)tabletIndex_I,
                    0, // ignored
                    0, // ignored
                    (ushort)EWTExtensionTabletProperty.TABLET_PROPERTY_CONTROLCOUNT,
                    ref numCtrls) )
                { throw new Exception("Oops - Failed ControlPropertyGet for TABLET_PROPERTY_CONTROLCOUNT"); }

                // All tablets should have ExpressKeys (we assume).
                if (numCtrls == 0 && EWTXExtensionTag.WTX_EXPKEYS2 == extTagIndex_I)
                { throw new Exception("Oops - SetupControlsForExtension didn't find any ExpressKeys!"); }

                for (UInt32 idx = 0; idx < numCtrls; idx++)
                {
                    SetupFunctionsForControl(tabletIndex_I, extTagIndex_I, idx, numCtrls, setupFunc_I);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Iterate through and setup all functions on this control.
        /// </summary>
        /// </summary>
        /// <param name="tabletIndex_I">tablet index</param>
        /// <param name="extTagIndex_I">extension index tag</param>
        /// <param name="controlIndex_I">control index</param>
        /// <param name="setupFunc_I">function called to setup extension control layout</param>
        public void SetupFunctionsForControl(
            UInt32 tabletIndex_I,
            EWTXExtensionTag extTagIndex_I,
            UInt32 controlIndex_I,
            UInt32 numControls_I,
            DrawControlsSetupFunction setupFunc_I)
        {
            UInt32 numFuncs = 0;

            try
            {
                // Get the number of functions for this control.
                if (!CWintabExtensions.ControlPropertyGet(
                    mLogContext.HCtx,
                    (byte)extTagIndex_I,
                    (byte)tabletIndex_I,
                    (byte)controlIndex_I,
                    0, // ignored
                    (ushort)EWTExtensionTabletProperty.TABLET_PROPERTY_FUNCCOUNT,
                    ref numFuncs))
                { throw new Exception("Oops - Failed ControlPropertyGet for TABLET_PROPERTY_FUNCCOUNT"); }

                Debug.Assert(numFuncs > 0);

                for (UInt32 funcIdx = 0; funcIdx < numFuncs; funcIdx++)
                {
                    SetupPropertiesForFunctions(tabletIndex_I, extTagIndex_I, controlIndex_I, funcIdx, numControls_I, setupFunc_I);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        /// Iterate through all functions on this control.
        /// </summary>
        /// </summary>
        /// <param name="tabletIndex_I">tablet index</param>
        /// <param name="extTagIndex_I">extension index tag</param>
        /// <param name="controlIndex_I">control index</param>
        /// <param name="functionIndex_I">control function index</param>
        /// <param name="setupFunc_I">function called to setup extension control layout</param>
        public void SetupPropertiesForFunctions(
            UInt32 tabletIndex_I,
            EWTXExtensionTag extTagIndex_I,
            UInt32 controlIndex_I,
            UInt32 functionIndex_I,
            UInt32 numControls_I,
            DrawControlsSetupFunction setupFunc_I)
        {
            bool bIsAvailable = false;

            try
            {
                WTPKT propOverride = 1;  // true
                UInt32 ctrlAvailable = 0;
                UInt32 ctrlLocation = 0;
                UInt32 ctrlMinRange = 0;
                UInt32 ctrlMaxRange = 0;
                String indexStr = extTagIndex_I == EWTXExtensionTag.WTX_EXPKEYS2 ? 
                    Convert.ToString(controlIndex_I) :
                    Convert.ToString(functionIndex_I);

                // NOTE - you can use strings in any language here.
                // The strings will be encoded to UTF8 before sent to the driver.
                // For example, you could use the string: "付録A" to indicate "EK" in Japanese.
                String ctrlname =
                    extTagIndex_I == EWTXExtensionTag.WTX_EXPKEYS2 ?   "EK: " + indexStr :
                    extTagIndex_I == EWTXExtensionTag.WTX_TOUCHRING ?  "TR: " + indexStr :
                    extTagIndex_I == EWTXExtensionTag.WTX_TOUCHSTRIP ? "TS: " + indexStr :
                    /* unknown control */                              "UK: " + indexStr;

                do
                {
                    // Ask if control is available for override.
                    if (!CWintabExtensions.ControlPropertyGet(
                        mLogContext.HCtx,
                        (byte)extTagIndex_I,
                        (byte)tabletIndex_I,
                        (byte)controlIndex_I,
                        (byte)functionIndex_I,
                        (ushort)EWTExtensionTabletProperty.TABLET_PROPERTY_AVAILABLE,
                        ref ctrlAvailable))
                    { throw new Exception("Oops - FAILED ControlPropertyGet for TABLET_PROPERTY_AVAILABLE"); }

                    bIsAvailable = (ctrlAvailable > 0);

                    if (!bIsAvailable)
                    {
                        Debug.WriteLine("Cannot override control");
                        break;
                    }

                    // Set flag indicating we're overriding the control.
                    if (!CWintabExtensions.ControlPropertySet(
                        mLogContext.HCtx,
                        (byte)extTagIndex_I,
                        (byte)tabletIndex_I,
                        (byte)controlIndex_I,
                        (byte)functionIndex_I,
                        (ushort)EWTExtensionTabletProperty.TABLET_PROPERTY_OVERRIDE,
                        propOverride))
                    { Debug.WriteLine("Oops - FAILED ControlPropertySet for TABLET_PROPERTY_OVERRIDE"); }

                    // Set the control name.
                    if (!CWintabExtensions.ControlPropertySet(
                        mLogContext.HCtx,
                        (byte)extTagIndex_I,
                        (byte)tabletIndex_I,
                        (byte)controlIndex_I,
                        (byte)functionIndex_I,
                        (ushort)EWTExtensionTabletProperty.TABLET_PROPERTY_OVERRIDE_NAME,
                        ctrlname))
                    { Debug.WriteLine("Oops - FAILED ControlPropertySet for TABLET_PROPERTY_OVERRIDE_NAME"); }

                    // Get the location of the control
                    if (!CWintabExtensions.ControlPropertyGet(
                        mLogContext.HCtx,
                        (byte)extTagIndex_I,
                        (byte)tabletIndex_I,
                        (byte)controlIndex_I,
                        (byte)functionIndex_I,
                        (ushort)EWTExtensionTabletProperty.TABLET_PROPERTY_LOCATION,
                        ref ctrlLocation))
                    { Debug.WriteLine("Oops - FAILED ControlPropertyGet for TABLET_PROPERTY_LOCATION"); }

                    if (!CWintabExtensions.ControlPropertyGet(
                        mLogContext.HCtx,
                        (byte)extTagIndex_I,
                        (byte)tabletIndex_I,
                        (byte)controlIndex_I,
                        (byte)functionIndex_I,
                        (ushort)EWTExtensionTabletProperty.TABLET_PROPERTY_MIN,
                        ref ctrlMinRange))
                    { Debug.WriteLine("Oops - FAILED ControlPropertyGet for TABLET_PROPERTY_MIN"); }

                    if (!CWintabExtensions.ControlPropertyGet(
                        mLogContext.HCtx,
                        (byte)extTagIndex_I,
                        (byte)tabletIndex_I,
                        (byte)controlIndex_I,
                        (byte)functionIndex_I,
                        (ushort)EWTExtensionTabletProperty.TABLET_PROPERTY_MAX,
                        ref ctrlMaxRange))
                    { Debug.WriteLine("Oops - FAILED ControlPropertyGet for TABLET_PROPERTY_MAX"); }

                    // Set tablet OLED with icon (if supported by the tablet).
                    // Ignore return value for now.
                    CWintabExtensions.SetDisplayProperty(
                        mLogContext, 
                        extTagIndex_I, 
                        tabletIndex_I, 
                        controlIndex_I, 
                        functionIndex_I, 
                        mTestImageIconPath);

                    // Finally, call function to setup control layout for rendering.
                    // Control will be updated when WT_PACKETEXT packets received. 
                    setupFunc_I(
                        (int)tabletIndex_I, 
                        (int)controlIndex_I, 
                        (int)functionIndex_I, 
                        bIsAvailable, 
                        (int)ctrlLocation, 
                        (int)ctrlMinRange, 
                        (int)ctrlMaxRange);

                    SetControlProperties(tabletIndex_I, extTagIndex_I, controlIndex_I, functionIndex_I, numControls_I, ctrlname);
                } while (false);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }



        ///////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Called when Wintab WT_PACKET events are received.
        /// </summary>
        /// <param name="sender_I">The EventMessage object sending the report.</param>
        /// <param name="eventArgs_I">eventArgs_I.Message.WParam contains ID of packet containing the data.</param>
        public void MyWTPacketEventHandler(Object sender_I, MessageReceivedEventArgs eventArgs_I)
        {
            if (m_wtData == null)
            {
                return;
            }

            if (eventArgs_I.Message.Msg == (Int32)EWintabEventMessage.WT_PACKETEXT)
            {
                //Debug.WriteLine("Received WT_PACKETEXT");
                var hCtx = eventArgs_I.Message.LParam;
                WTPKT pktID = (WTPKT)eventArgs_I.Message.WParam;
                WintabPacketExt pktExt = m_wtData.GetDataPacketExt(hCtx, pktID);

                if (pktExt.pkBase.nContext != mLogContext.HCtx)
                {
                    throw new Exception("Oops - got a message from unknown context: " + pktExt.pkBase.nContext.ToString());
                }

                if (pktExt.pkBase.nContext == mLogContext.HCtx)
                {
                    // Call updates on all control types, even though some updates will be NO-OPS
                    // because those controls will not be supported for the tablet.
                    mExtensionControlState.UpdateExpressKey((int)pktExt.pkExpKey.nTablet, (int)pktExt.pkExpKey.nControl, (int)pktExt.pkExpKey.nLocation, pktExt.pkExpKey.nState);
                    mExtensionControlState.UpdateTouchRing((int)pktExt.pkTouchRing.nTablet, (int)pktExt.pkTouchRing.nControl, (int)pktExt.pkTouchRing.nMode, pktExt.pkTouchRing.nPosition);
                    mExtensionControlState.UpdateTouchStrip((int)pktExt.pkTouchStrip.nTablet, (int)pktExt.pkTouchStrip.nControl, (int)pktExt.pkTouchStrip.nMode, pktExt.pkTouchStrip.nPosition);

                    // Refresh all supported controls based on current control state.
                    RefreshControls();
                }
            }
        }


        ///////////////////////////////////////////////////////////////////////
        // Control properties methods
        //

        /// <summary>
        /// Refresh all supported controls based on current control state.
        /// </summary>
        private void RefreshControls()
        {
            RefreshExpressKeys();
            RefreshTouchRings();
            RefreshTouchStrips();
        }


        private void RefreshExpressKeys()
        {
            int numExpKeys = mExtensionControlState.Tablets[mTabletIndexDefault].expKeys.Length;

            for (int ctrlIndex = 0; ctrlIndex < numExpKeys; ctrlIndex++)
            {
                int location = mExtensionControlState.Tablets[mTabletIndexDefault].expKeys[ctrlIndex].location;
                int uiIndex = location == 0 ?
                    (int)ctrlIndex :                       // left-side indicator
                    (int)(ctrlIndex + Math.Abs((Int32)(numExpKeys / 2 - 8)));    // right-side indicator

                bool down = mExtensionControlState.Tablets[mTabletIndexDefault].expKeys[ctrlIndex].down;

                switch (uiIndex)
                {
                    // Refresh left-side express keys
                    case 0: Tab0_L_EK1Panel.BackColor = Tab0_L_EK1Panel.Visible && down ? Color.DarkOrange : SystemColors.Control; continue;
                    case 1: Tab0_L_EK2Panel.BackColor = Tab0_L_EK2Panel.Visible && down ? Color.DarkOrange : SystemColors.Control; continue;
                    case 2: Tab0_L_EK3Panel.BackColor = Tab0_L_EK3Panel.Visible && down ? Color.DarkOrange : SystemColors.Control; continue;
                    case 3: Tab0_L_EK4Panel.BackColor = Tab0_L_EK4Panel.Visible && down ? Color.DarkOrange : SystemColors.Control; continue;
                    case 4: Tab0_L_EK5Panel.BackColor = Tab0_L_EK5Panel.Visible && down ? Color.DarkOrange : SystemColors.Control; continue;
                    case 5: Tab0_L_EK6Panel.BackColor = Tab0_L_EK6Panel.Visible && down ? Color.DarkOrange : SystemColors.Control; continue;
                    case 6: Tab0_L_EK7Panel.BackColor = Tab0_L_EK7Panel.Visible && down ? Color.DarkOrange : SystemColors.Control; continue;
                    case 7: Tab0_L_EK8Panel.BackColor = Tab0_L_EK8Panel.Visible && down ? Color.DarkOrange : SystemColors.Control; continue;
                    
                    // Refresh right-side express keys
                    case 8: Tab0_R_EK1Panel.BackColor = Tab0_R_EK1Panel.Visible && down ? Color.DarkOrange : SystemColors.Control; continue;
                    case 9: Tab0_R_EK2Panel.BackColor = Tab0_R_EK2Panel.Visible && down ? Color.DarkOrange : SystemColors.Control; continue;
                    case 10: Tab0_R_EK3Panel.BackColor = Tab0_R_EK3Panel.Visible && down ? Color.DarkOrange : SystemColors.Control; continue;
                    case 11: Tab0_R_EK4Panel.BackColor = Tab0_R_EK4Panel.Visible && down ? Color.DarkOrange : SystemColors.Control; continue;
                    case 12: Tab0_R_EK5Panel.BackColor = Tab0_R_EK5Panel.Visible && down ? Color.DarkOrange : SystemColors.Control; continue;
                    case 13: Tab0_R_EK6Panel.BackColor = Tab0_R_EK6Panel.Visible && down ? Color.DarkOrange : SystemColors.Control; continue;
                    case 14: Tab0_R_EK7Panel.BackColor = Tab0_R_EK7Panel.Visible && down ? Color.DarkOrange : SystemColors.Control; continue;
                    case 15: Tab0_R_EK8Panel.BackColor = Tab0_R_EK8Panel.Visible && down ? Color.DarkOrange : SystemColors.Control; continue;

                    default:
                        throw new Exception("Oops - unknown ExpressKey uiIndex: " + uiIndex.ToString());
                }

            }
        }


        private void RefreshTouchRings()
        {
            Tab0_TR1Panel.Invalidate();
            Tab0_TR2Panel.Invalidate();
        }

        private void RefreshTouchStrips()
        {
            Tab0_TS1Panel.Invalidate();
            Tab0_TS2Panel.Invalidate();
        }



        /// <summary>
        /// Sets properties for the specified control.
        /// </summary>
        /// <param name="tabletIndex_I"></param>
        /// <param name="extTagIndex_I"></param>
        /// <param name="controlIndex_I"></param>
        /// <param name="functionIndex_I"></param>
        /// <param name="ctrlName_I"></param>
        private void SetControlProperties(UInt32 tabletIndex_I, EWTXExtensionTag extTagIndex_I, UInt32 controlIndex_I, UInt32 functionIndex_I, UInt32 numControls_I, String ctrlName_I)
        {
            try
            {
                switch (extTagIndex_I)
                {
                    case EWTXExtensionTag.WTX_EXPKEYS2:
                        {
                            bool visible = mExtensionControlState.Tablets[tabletIndex_I].expKeys[controlIndex_I].available;

                            // Prepend indication of left or right side to control name.
                            // NOTE - don't forget that any string modifications will eventually be
                            // encoded to UTF8 before being sent to the driver.
                            int location = mExtensionControlState.Tablets[tabletIndex_I].expKeys[controlIndex_I].location;
                            String ctrlName = (location == 0 ? "L_" : "R_") + ctrlName_I;
                            int uiIndex = location == 0 ? 
                                (int)controlIndex_I :                           // left-side indicator
                                (int)(controlIndex_I + Math.Abs((Int32)(numControls_I/2 - 8)));    // right-side indicator

                            switch (uiIndex)
                            {
                                // map controlIndex_I to a left-side express key indicator.
                                case 0: Tab0_L_EK1Panel.Visible = visible; Tab0_L_EK1NameLabel.Visible = visible; Tab0_L_EK1NameLabel.Text = ctrlName; break;
                                case 1: Tab0_L_EK2Panel.Visible = visible; Tab0_L_EK2NameLabel.Visible = visible; Tab0_L_EK2NameLabel.Text = ctrlName; break;
                                case 2: Tab0_L_EK3Panel.Visible = visible; Tab0_L_EK3NameLabel.Visible = visible; Tab0_L_EK3NameLabel.Text = ctrlName; break;
                                case 3: Tab0_L_EK4Panel.Visible = visible; Tab0_L_EK4NameLabel.Visible = visible; Tab0_L_EK4NameLabel.Text = ctrlName; break;
                                case 4: Tab0_L_EK5Panel.Visible = visible; Tab0_L_EK5NameLabel.Visible = visible; Tab0_L_EK5NameLabel.Text = ctrlName; break;
                                case 5: Tab0_L_EK6Panel.Visible = visible; Tab0_L_EK6NameLabel.Visible = visible; Tab0_L_EK6NameLabel.Text = ctrlName; break;
                                case 6: Tab0_L_EK7Panel.Visible = visible; Tab0_L_EK7NameLabel.Visible = visible; Tab0_L_EK7NameLabel.Text = ctrlName; break;
                                case 7: Tab0_L_EK8Panel.Visible = visible; Tab0_L_EK8NameLabel.Visible = visible; Tab0_L_EK8NameLabel.Text = ctrlName; break;

                                // map controlIndex_I to a right-side express key indicator.
                                case 8: Tab0_R_EK1Panel.Visible = visible; Tab0_R_EK1NameLabel.Visible = visible; Tab0_R_EK1NameLabel.Text = ctrlName; break;
                                case 9: Tab0_R_EK2Panel.Visible = visible; Tab0_R_EK2NameLabel.Visible = visible; Tab0_R_EK2NameLabel.Text = ctrlName; break;
                                case 10: Tab0_R_EK3Panel.Visible = visible; Tab0_R_EK3NameLabel.Visible = visible; Tab0_R_EK3NameLabel.Text = ctrlName; break;
                                case 11: Tab0_R_EK4Panel.Visible = visible; Tab0_R_EK4NameLabel.Visible = visible; Tab0_R_EK4NameLabel.Text = ctrlName; break;
                                case 12: Tab0_R_EK5Panel.Visible = visible; Tab0_R_EK5NameLabel.Visible = visible; Tab0_R_EK5NameLabel.Text = ctrlName; break;
                                case 13: Tab0_R_EK6Panel.Visible = visible; Tab0_R_EK6NameLabel.Visible = visible; Tab0_R_EK6NameLabel.Text = ctrlName; break;
                                case 14: Tab0_R_EK7Panel.Visible = visible; Tab0_R_EK7NameLabel.Visible = visible; Tab0_R_EK7NameLabel.Text = ctrlName; break;
                                case 15: Tab0_R_EK8Panel.Visible = visible; Tab0_R_EK8NameLabel.Visible = visible; Tab0_R_EK8NameLabel.Text = ctrlName; break;

                                default:
                                    throw new Exception("Oops - unknown ExpressKey uiIndex: " + uiIndex.ToString());
                            }
                        }
                        break;

                    case EWTXExtensionTag.WTX_TOUCHRING:
                        {
                            bool visible = mExtensionControlState.Tablets[tabletIndex_I].touchRings[controlIndex_I].available;
                            int location = mExtensionControlState.Tablets[tabletIndex_I].touchRings[controlIndex_I].location;
                            String TRname = location == 0 ? "Left" : "Right";
                            String ctrlName = (location == 0 ? "L_" : "R_") + ctrlName_I;

                            switch (controlIndex_I)
                            {
                                case 0:
                                    Tab0_TR1NameLabel.Visible = visible;
                                    Tab0_TR1NameLabel.Text = TRname;

                                    // Set mode button state.
                                    switch (functionIndex_I)
                                    {
                                        case 0: Tab0_TR1Mode1Panel.Visible = visible; Tab0_TR1Mode1NameLabel.Visible = visible; Tab0_TR1Mode1NameLabel.Text = ctrlName; break;
                                        case 1: Tab0_TR1Mode2Panel.Visible = visible; Tab0_TR1Mode2NameLabel.Visible = visible; Tab0_TR1Mode2NameLabel.Text = ctrlName; break;
                                        case 2: Tab0_TR1Mode3Panel.Visible = visible; Tab0_TR1Mode3NameLabel.Visible = visible; Tab0_TR1Mode3NameLabel.Text = ctrlName; break;
                                        case 3: Tab0_TR1Mode4Panel.Visible = visible; Tab0_TR1Mode4NameLabel.Visible = visible; Tab0_TR1Mode4NameLabel.Text = ctrlName; break;
                                        default:
                                            throw new Exception("Oops - unknown mode index");
                                    }
                                    break;
                                case 1:
                                    Tab0_TR2NameLabel.Visible = visible;
                                    Tab0_TR2NameLabel.Text = TRname; 

                                    // Set mode button state.
                                    switch (functionIndex_I)
                                    {
                                        case 0: Tab0_TR2Mode1Panel.Visible = visible; Tab0_TR2Mode1NameLabel.Visible = visible; Tab0_TR2Mode1NameLabel.Text = ctrlName; break;
                                        case 1: Tab0_TR2Mode2Panel.Visible = visible; Tab0_TR2Mode2NameLabel.Visible = visible; Tab0_TR2Mode2NameLabel.Text = ctrlName; break;
                                        case 2: Tab0_TR2Mode3Panel.Visible = visible; Tab0_TR2Mode3NameLabel.Visible = visible; Tab0_TR2Mode3NameLabel.Text = ctrlName; break;
                                        case 3: Tab0_TR2Mode4Panel.Visible = visible; Tab0_TR2Mode4NameLabel.Visible = visible; Tab0_TR2Mode4NameLabel.Text = ctrlName; break;
                                        default:
                                            throw new Exception("Oops - unknown mode index");
                                    }
                                    break;
                                default:
                                    throw new Exception("Oops - unknown TouchRing controlIndex_I: " + controlIndex_I.ToString());
                            }
                        }
                        break;


                    case EWTXExtensionTag.WTX_TOUCHSTRIP:
                        {
                            bool visible = mExtensionControlState.Tablets[tabletIndex_I].touchStrips[controlIndex_I].available;
                            int location = mExtensionControlState.Tablets[tabletIndex_I].touchStrips[controlIndex_I].location;
                            String TRname = location == 0 ? "Left" : "Right";
                            String ctrlName = (location == 0 ? "L_" : "R_") + ctrlName_I;

                            switch (controlIndex_I)
                            {
                                case 0:
                                    Tab0_TS1NameLabel.Visible = visible;
                                    Tab0_TS1NameLabel.Text = TRname;

                                    // Set mode button state.
                                    switch (functionIndex_I)
                                    {
                                        case 0: Tab0_TS1Mode1Panel.Visible = visible; Tab0_TS1Mode1NameLabel.Visible = visible; Tab0_TS1Mode1NameLabel.Text = ctrlName; break;
                                        case 1: Tab0_TS1Mode2Panel.Visible = visible; Tab0_TS1Mode2NameLabel.Visible = visible; Tab0_TS1Mode2NameLabel.Text = ctrlName; break;
                                        case 2: Tab0_TS1Mode3Panel.Visible = visible; Tab0_TS1Mode3NameLabel.Visible = visible; Tab0_TS1Mode3NameLabel.Text = ctrlName; break;
                                        case 3: Tab0_TS1Mode4Panel.Visible = visible; Tab0_TS1Mode4NameLabel.Visible = visible; Tab0_TS1Mode4NameLabel.Text = ctrlName; break;
                                        default:
                                            throw new Exception("Oops - unknown mode index");
                                    }
                                    break;
                                case 1:
                                    Tab0_TS2NameLabel.Visible = visible;
                                    Tab0_TS2NameLabel.Text = TRname;

                                    // Set mode button state.
                                    switch (functionIndex_I)
                                    {
                                        case 0: Tab0_TS2Mode1Panel.Visible = visible; Tab0_TS2Mode1NameLabel.Visible = visible; Tab0_TS2Mode1NameLabel.Text = ctrlName; break;
                                        case 1: Tab0_TS2Mode2Panel.Visible = visible; Tab0_TS2Mode2NameLabel.Visible = visible; Tab0_TS2Mode2NameLabel.Text = ctrlName; break;
                                        case 2: Tab0_TS2Mode3Panel.Visible = visible; Tab0_TS2Mode3NameLabel.Visible = visible; Tab0_TS2Mode3NameLabel.Text = ctrlName; break;
                                        case 3: Tab0_TS2Mode4Panel.Visible = visible; Tab0_TS2Mode4NameLabel.Visible = visible; Tab0_TS2Mode4NameLabel.Text = ctrlName; break;
                                        default:
                                            throw new Exception("Oops - unknown mode index");
                                    }
                                    break;
                                default:
                                    throw new Exception("Oops - unknown TouchRing controlIndex_I: " + controlIndex_I.ToString());
                            }
                        }
                        break;

                    default:
                        throw new Exception("Oops - unknown tagIndex: " + extTagIndex_I.ToString());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }



        /// <summary>
        /// Refresh both touch ring panels (whether they need it or not)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="peArgs_I"></param>
        private void Tab0_TRPanel_Paint(object sender, PaintEventArgs peArgs_I)
        {
            int controlIndex =
                (sender as Panel).Name == "Tab0_TR1Panel" ? 0 :
                (sender as Panel).Name == "Tab0_TR2Panel" ? 1 : -1;

            if (controlIndex == -1)
            {
                throw new Exception("Oops - unknown panel sender");
            }

            // Make sure we don't try to paint a touch ring that's not there.
            if (mExtensionControlState.Tablets[mTabletIndexDefault].touchRings != null &&
                mExtensionControlState.Tablets[mTabletIndexDefault].touchRings.Length > controlIndex &&
                mExtensionControlState.Tablets[mTabletIndexDefault].touchRings[controlIndex].available)
            {
                bool down = mExtensionControlState.Tablets[mTabletIndexDefault].touchRings[controlIndex].down;

                const float circleLeft = 80;
                const float circleTop = 80;
                const float circleWidth = 60;
                const float circleHeight = 60;
                const float radius = circleWidth / 2;
                Point center = new Point((int)(circleLeft + circleWidth / 2), (int)(circleTop + circleHeight / 2));
                float fingerPosition = 
                    mExtensionControlState.Tablets[mTabletIndexDefault].touchRings[controlIndex].position -
                    mExtensionControlState.Tablets[mTabletIndexDefault].touchRings[controlIndex].min;

                float range =
                    mExtensionControlState.Tablets[mTabletIndexDefault].touchRings[controlIndex].max -
                    mExtensionControlState.Tablets[mTabletIndexDefault].touchRings[controlIndex].min;

                float angle = (fingerPosition / range) * 2 * mPI - mPIDIV2;


                int fingerX = (int)(Math.Cos(angle) * radius);
                int fingerY = (int)(Math.Sin(angle) * radius);
                Point fingerPoint = new Point(center.X + fingerX, center.Y + fingerY);

                peArgs_I.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                peArgs_I.Graphics.DrawEllipse(mPen, circleLeft, circleTop, circleWidth, circleHeight);

                if (down)
                {
                    // Draw a line from the center of the touchRing to the edge.
                    peArgs_I.Graphics.DrawLine(mFingerPen, center, fingerPoint);
                }

                // Update the mode buttons
                switch (controlIndex)
                {
                    case 0:
                        Tab0_TR1Mode1Panel.BackColor = Tab0_TR1Mode1Panel.Visible && mExtensionControlState.Tablets[mTabletIndexDefault].touchRings[controlIndex].modes[0].active ? Color.DarkGreen : SystemColors.Control;
                        Tab0_TR1Mode2Panel.BackColor = Tab0_TR1Mode2Panel.Visible && mExtensionControlState.Tablets[mTabletIndexDefault].touchRings[controlIndex].modes[1].active ? Color.DarkGreen : SystemColors.Control;
                        Tab0_TR1Mode3Panel.BackColor = Tab0_TR1Mode3Panel.Visible && mExtensionControlState.Tablets[mTabletIndexDefault].touchRings[controlIndex].modes[2].active ? Color.DarkGreen : SystemColors.Control;
                        Tab0_TR1Mode4Panel.BackColor = Tab0_TR1Mode4Panel.Visible && mExtensionControlState.Tablets[mTabletIndexDefault].touchRings[controlIndex].modes[3].active ? Color.DarkGreen : SystemColors.Control;
                    break;

                    case 1:
                        Tab0_TR2Mode1Panel.BackColor = Tab0_TR2Mode1Panel.Visible && mExtensionControlState.Tablets[mTabletIndexDefault].touchRings[controlIndex].modes[0].active ? Color.DarkGreen : SystemColors.Control;
                        Tab0_TR2Mode2Panel.BackColor = Tab0_TR2Mode2Panel.Visible && mExtensionControlState.Tablets[mTabletIndexDefault].touchRings[controlIndex].modes[1].active ? Color.DarkGreen : SystemColors.Control;
                        Tab0_TR2Mode3Panel.BackColor = Tab0_TR2Mode3Panel.Visible && mExtensionControlState.Tablets[mTabletIndexDefault].touchRings[controlIndex].modes[2].active ? Color.DarkGreen : SystemColors.Control;
                        Tab0_TR2Mode4Panel.BackColor = Tab0_TR2Mode4Panel.Visible && mExtensionControlState.Tablets[mTabletIndexDefault].touchRings[controlIndex].modes[3].active ? Color.DarkGreen : SystemColors.Control;
                    break;

                    default:
                    throw new Exception("Oops - bad controlIndex");
                }
            }
        }



        /// <summary>
        /// Refresh both touch strip panels (whether they need it or not)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="peArgs_I"></param>
        private void Tab0_TSPanel_Paint(object sender, PaintEventArgs peArgs_I)
        {
            int controlIndex =
                (sender as Panel).Name == "Tab0_TS1Panel" ? 0 :
                (sender as Panel).Name == "Tab0_TS2Panel" ? 1 : -1;

            if (controlIndex == -1)
            {
                throw new Exception("Oops - unknown panel sender");
            }

            // Make sure we don't try to paint a touch ring that's not there.
            if (mExtensionControlState.Tablets[mTabletIndexDefault].touchStrips != null &&
                mExtensionControlState.Tablets[mTabletIndexDefault].touchStrips.Length > controlIndex &&
                mExtensionControlState.Tablets[mTabletIndexDefault].touchStrips[controlIndex].available)
            {
                bool down = mExtensionControlState.Tablets[mTabletIndexDefault].touchStrips[controlIndex].down;

                float fingerPosition =
                    mExtensionControlState.Tablets[mTabletIndexDefault].touchStrips[controlIndex].position + 1 -
                    mExtensionControlState.Tablets[mTabletIndexDefault].touchStrips[controlIndex].min;

                Debug.WriteLine("fingerPosition: " + fingerPosition);

                const float left = 100;
                const float top = 50;
                const float width = 30;
                const float height = 100;

                Rectangle rect = new Rectangle((int)left, (int)top, (int)width, (int)height);

                peArgs_I.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                peArgs_I.Graphics.DrawRectangle(mPen, rect);

                if (down)
                {
                    float range =
                        mExtensionControlState.Tablets[mTabletIndexDefault].touchStrips[controlIndex].max -
                        mExtensionControlState.Tablets[mTabletIndexDefault].touchStrips[controlIndex].min;

                    float scaledFingerPos = fingerPosition * (height / range);

                    // Draw a horizontal line at the finger position on the strip.
                    int yPos = (int)(top + scaledFingerPos);

                    peArgs_I.Graphics.DrawLine(mFingerPen, left - 5, yPos, left + width + 5, yPos);
                }

                // Update the mode buttons
                switch (controlIndex)
                {
                    case 0:
                        Tab0_TS1Mode1Panel.BackColor = Tab0_TS1Mode1Panel.Visible && mExtensionControlState.Tablets[mTabletIndexDefault].touchStrips[controlIndex].modes[0].active ? Color.DarkGreen : SystemColors.Control;
                        Tab0_TS1Mode2Panel.BackColor = Tab0_TS1Mode2Panel.Visible && mExtensionControlState.Tablets[mTabletIndexDefault].touchStrips[controlIndex].modes[1].active ? Color.DarkGreen : SystemColors.Control;
                        Tab0_TS1Mode3Panel.BackColor = Tab0_TS1Mode3Panel.Visible && mExtensionControlState.Tablets[mTabletIndexDefault].touchStrips[controlIndex].modes[2].active ? Color.DarkGreen : SystemColors.Control;
                        Tab0_TS1Mode4Panel.BackColor = Tab0_TS1Mode4Panel.Visible && mExtensionControlState.Tablets[mTabletIndexDefault].touchStrips[controlIndex].modes[3].active ? Color.DarkGreen : SystemColors.Control;
                        break;

                    case 1:
                        Tab0_TS2Mode1Panel.BackColor = Tab0_TS2Mode1Panel.Visible && mExtensionControlState.Tablets[mTabletIndexDefault].touchStrips[controlIndex].modes[0].active ? Color.DarkGreen : SystemColors.Control;
                        Tab0_TS2Mode2Panel.BackColor = Tab0_TS2Mode2Panel.Visible && mExtensionControlState.Tablets[mTabletIndexDefault].touchStrips[controlIndex].modes[1].active ? Color.DarkGreen : SystemColors.Control;
                        Tab0_TS2Mode3Panel.BackColor = Tab0_TS2Mode3Panel.Visible && mExtensionControlState.Tablets[mTabletIndexDefault].touchStrips[controlIndex].modes[2].active ? Color.DarkGreen : SystemColors.Control;
                        Tab0_TS2Mode4Panel.BackColor = Tab0_TS2Mode4Panel.Visible && mExtensionControlState.Tablets[mTabletIndexDefault].touchStrips[controlIndex].modes[3].active ? Color.DarkGreen : SystemColors.Control;
                        break;

                    default:
                        throw new Exception("Oops - bad controlIndex");
                }
            }
        }



        /// <summary>
        /// Initialize Wintab when the test form is loaded.
        /// If you do it at form construction time, you could exception when closing.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExtensionTestForm_Load(object sender, EventArgs e)
        {
            if (!InitWintab())
            {
                String errmsg =
                    "Oops - couldn't initialize Wintab.\n" +
                    "Make sure the tablet driver is running and a tablet is plugged in.\n\n" +
                    "Bailing out...";
                MessageBox.Show(errmsg);
                Close();
            }
        }



        /// <summary>
        /// Restore the tablet control functions and close Wintab context.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExtensionTestForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (mLogContext != null && mLogContext.HCtx.IsValid)
            {
                RemoveOverridesForTablet(mTabletIndexDefault);
                mLogContext.Close();
            }
        }
    }
}
