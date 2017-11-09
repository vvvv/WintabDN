///////////////////////////////////////////////////////////////////////////////
// ExtensionTestDrawControls.cs - WintabDN extensions render code
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
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Collections;
using System.Windows.Forms;

namespace FormExtTestApp
{
    public class TabletGraphicList : ArrayList
    {
        
    }

    /// <summary>
    /// Sets up parameters necessary to draw a specific control.
    /// </summary>
    public delegate void DrawControlsSetupFunction(int tabletIndex_I, int controlIndex_I, int functionIndex_I, bool available_I, int locationIndex_I, int min_I, int max_I);

    public struct TabletGraphic
    {
        public int tabletID;

        public ExpressKeyGraphic[] expKeys;
        public LinearControlGraphic[] touchRings;
        public LinearControlGraphic[] touchStrips;
    }

    public struct ExpressKeyGraphic
    {
        public bool available;
        public bool down;
        public int location;
    }

    public struct LinearControlGraphic
    {
        public LinearControlMode[] modes;
        public bool available;
        public bool down;
        public int position;
        public int min;
        public int max;
        public int location;
    }

    public struct LinearControlMode
    {
        public bool active;
    }

    /// <summary>
    /// Maintains state for extension controls that are rendered.
    /// </summary>
    class ExtensionControlState
    {
        private TabletGraphic[]  mTablets;

        public TabletGraphic[] Tablets
        {
            get { return mTablets; }
        }

        public ExtensionControlState()
        {
            // Assume at least one tablet.
            mTablets = new TabletGraphic[0];
        }

        // TODO - add some control inheritance here -

        /// <summary>
        /// Adds a new tablet if needed; else it's  NO-OP.
        /// </summary>
        /// <param name="tabletIndex_I">index of tablet to add</param>
        public void AddTablet(int tabletIndex_I)
        {
            if (tabletIndex_I + 1 > mTablets.Length)
            {
                Array.Resize(ref mTablets, mTablets.Length + 1);
            }
        }

        /// <summary>
        /// Adds a new Express Key for specified tablet if not already created; else it's a NO-OP.
        /// </summary>
        /// <param name="tabletIndex_I">index of tablet where control to be added</param>
        /// <param name="controlIndex_I">index of control to add</param>
        public void AddExpressKey(int tabletIndex_I, int controlIndex_I)
        {
            AddTablet(tabletIndex_I);

            if (mTablets[tabletIndex_I].expKeys == null)
            {
                mTablets[tabletIndex_I].expKeys = new ExpressKeyGraphic[1];
            }

            if (controlIndex_I + 1 > mTablets[tabletIndex_I].expKeys.Length)
            {
                Array.Resize(ref mTablets[tabletIndex_I].expKeys, mTablets[tabletIndex_I].expKeys.Length + 1);
            }
        }

        /// <summary>
        /// Adds a new Touch Ring for specified tablet if not already created; else it's a NO-OP.
        /// </summary>
        /// <param name="tabletIndex_I">index of tablet where control to be added</param>
        /// <param name="controlIndex_I">index of control to add</param>
        public void AddTouchRing(int tabletIndex_I, int controlIndex_I, int modeIndex_I)
        {
            AddTablet(tabletIndex_I);

            if (mTablets[tabletIndex_I].touchRings == null)
            {
                mTablets[tabletIndex_I].touchRings = new LinearControlGraphic[1];
            }

            if (controlIndex_I >= (int)mTablets[tabletIndex_I].touchRings.Length)
            {
                Array.Resize(ref mTablets[tabletIndex_I].touchRings, mTablets[tabletIndex_I].touchRings.Length + 1);
            }

            if (mTablets[tabletIndex_I].touchRings[controlIndex_I].modes == null)
            {
                mTablets[tabletIndex_I].touchRings[controlIndex_I].modes = new LinearControlMode[1];
            }

            if (controlIndex_I + 1 > mTablets[tabletIndex_I].touchRings.Length)
            {
                Array.Resize(ref mTablets[tabletIndex_I].touchRings, mTablets[tabletIndex_I].touchRings.Length + 1);
            }

            if (modeIndex_I >= (int)mTablets[tabletIndex_I].touchRings[controlIndex_I].modes.Length)
            {
                Array.Resize(ref mTablets[tabletIndex_I].touchRings[controlIndex_I].modes, mTablets[tabletIndex_I].touchRings[controlIndex_I].modes.Length + 1);
            }
        }

        /// <summary>
        /// Adds a new Touch Strip for specified tablet if not already created; else it's a NO-OP.
        /// </summary>
        /// <param name="tabletIndex_I">index of tablet where control to be added</param>
        /// <param name="controlIndex_I">index of control to add</param>
        public void AddTouchStrip(int tabletIndex_I, int controlIndex_I, int modeIndex_I)
        {
            AddTablet(tabletIndex_I);

            if (mTablets[tabletIndex_I].touchStrips == null)
            {
                mTablets[tabletIndex_I].touchStrips = new LinearControlGraphic[1];
            }

            if (controlIndex_I >= (int)mTablets[tabletIndex_I].touchStrips.Length)
            {
                Array.Resize(ref mTablets[tabletIndex_I].touchStrips, mTablets[tabletIndex_I].touchStrips.Length + 1);
            }

            if (mTablets[tabletIndex_I].touchStrips[controlIndex_I].modes == null)
            {
                mTablets[tabletIndex_I].touchStrips[controlIndex_I].modes = new LinearControlMode[1];
            }

            if (controlIndex_I + 1 > mTablets[tabletIndex_I].touchStrips.Length)
            {
                Array.Resize(ref mTablets[tabletIndex_I].touchStrips, mTablets[tabletIndex_I].touchStrips.Length + 1);
            }

            if (modeIndex_I >= (int)mTablets[tabletIndex_I].touchStrips[controlIndex_I].modes.Length)
            {
                Array.Resize(ref mTablets[tabletIndex_I].touchStrips[controlIndex_I].modes, mTablets[tabletIndex_I].touchStrips[controlIndex_I].modes.Length + 1);
            }
        }



        /// <summary>
        /// Set up ExpressKey structures to match on-tablet buttons.
        /// </summary>
        /// <param name="tabletIndex_I"></param>
        /// <param name="controlIndex_I"></param>
        /// <param name="functionIndex_I"></param>
        /// <param name="available_I"></param>
        /// <param name="locationIndex_I"></param>
        /// <param name="min_I"></param>
        /// <param name="max_I"></param>
        public void SetupExpressKey(int tabletIndex_I, int controlIndex_I, int functionIndex_I, bool available_I, int locationIndex_I, int min_I, int max_I) 
        {
            DumpSetup("EK", tabletIndex_I, controlIndex_I, functionIndex_I, available_I, locationIndex_I, min_I, max_I);

            AddExpressKey(tabletIndex_I, controlIndex_I);

            mTablets[tabletIndex_I].expKeys[controlIndex_I].available = available_I;
            mTablets[tabletIndex_I].expKeys[controlIndex_I].down = false;
            mTablets[tabletIndex_I].expKeys[controlIndex_I].location = locationIndex_I;
        }



        /// <summary>
        /// Set up TouchRing structures to match on-tablet touch rings.
        /// </summary>
        /// <param name="tabletIndex_I"></param>
        /// <param name="controlIndex_I"></param>
        /// <param name="functionIndex_I"></param>
        /// <param name="available_I"></param>
        /// <param name="locationIndex_I"></param>
        /// <param name="min_I"></param>
        /// <param name="max_I"></param>
        public void SetupTouchRing(int tabletIndex_I, int controlIndex_I, int modeIndex_I, bool available_I, int locationIndex_I, int min_I, int max_I) 
        {
            DumpSetup("TR", tabletIndex_I, controlIndex_I, modeIndex_I, available_I, locationIndex_I, min_I, max_I);

            AddTouchRing(tabletIndex_I, controlIndex_I, modeIndex_I);

            mTablets[tabletIndex_I].touchRings[controlIndex_I].available = available_I;
            mTablets[tabletIndex_I].touchRings[controlIndex_I].min = min_I;
            mTablets[tabletIndex_I].touchRings[controlIndex_I].max = max_I - 1;
            mTablets[tabletIndex_I].touchRings[controlIndex_I].location = locationIndex_I;
            mTablets[tabletIndex_I].touchRings[controlIndex_I].down = false;
            mTablets[tabletIndex_I].touchRings[controlIndex_I].position = 0;
            mTablets[tabletIndex_I].touchRings[controlIndex_I].modes[modeIndex_I].active = false;
        }



        /// <summary>
        /// Set up TouchStrip structures to match on-tablet touch strips.
        /// </summary>
        /// <param name="tabletIndex_I"></param>
        /// <param name="controlIndex_I"></param>
        /// <param name="functionIndex_I"></param>
        /// <param name="available_I"></param>
        /// <param name="locationIndex_I"></param>
        /// <param name="min_I"></param>
        /// <param name="max_I"></param>
        public void SetupTouchStrip(int tabletIndex_I, int controlIndex_I, int modeIndex_I, bool available_I, int locationIndex_I, int min_I, int max_I) 
        {
            DumpSetup("TS", tabletIndex_I, controlIndex_I, modeIndex_I, available_I, locationIndex_I, min_I, max_I);

            AddTouchStrip(tabletIndex_I, controlIndex_I, modeIndex_I);

            mTablets[tabletIndex_I].touchStrips[controlIndex_I].available = available_I;
            mTablets[tabletIndex_I].touchStrips[controlIndex_I].min = min_I;
            mTablets[tabletIndex_I].touchStrips[controlIndex_I].max = max_I;
            mTablets[tabletIndex_I].touchStrips[controlIndex_I].location = locationIndex_I;
            mTablets[tabletIndex_I].touchStrips[controlIndex_I].down = false;
            mTablets[tabletIndex_I].touchStrips[controlIndex_I].position = 0;
            mTablets[tabletIndex_I].touchStrips[controlIndex_I].modes[modeIndex_I].active = false;
        }



        /// <summary>
        /// Updates one ExpressKey state (up or down).
        /// Should be called when app receives a WT_PACKETEXT message.
        /// </summary>
        /// <param name="tabletIndex_I"></param>
        /// <param name="controlIndex_I"></param>
        /// <param name="locationIndex_I"></param>
        /// <param name="state_I"></param>
        public void UpdateExpressKey(int tabletIndex_I, int controlIndex_I, int locationIndex_I, uint state_I) 
        {
            DumpUpdate("EK", tabletIndex_I, controlIndex_I, locationIndex_I, (int)state_I);

            try
            {
                // Should always be ExpressKeys.
                if (mTablets[tabletIndex_I].expKeys == null)
                {
                    throw new Exception("Oops - expKeys not created for this tablet");
                }

                if (tabletIndex_I < mTablets.Length)
                {
                    if (controlIndex_I < mTablets[tabletIndex_I].expKeys.Length)
                    {
                        mTablets[tabletIndex_I].expKeys[controlIndex_I].down = (state_I != 0);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }



        /// <summary>
        /// Updates a touchring finger position. 
        /// Should be called when app receives a WT_PACKETEXT message.
        /// </summary>
        /// <param name="tabletIndex_I"></param>
        /// <param name="controlIndex_I"></param>
        /// <param name="modeIndex_I"></param>
        /// <param name="position_I"></param>
        public void UpdateTouchRing(int tabletIndex_I, int controlIndex_I, int modeIndex_I, uint position_I)
        {
            DumpUpdate("TR", tabletIndex_I, controlIndex_I, modeIndex_I, (int)position_I);

            try
            {
                if (tabletIndex_I < mTablets.Length)
                {
                    // This tablet may not have touch rings.
                    if (mTablets[tabletIndex_I].touchRings != null &&
                        controlIndex_I < (int)mTablets[tabletIndex_I].touchRings.Length)
                    {
                        mTablets[tabletIndex_I].touchRings[controlIndex_I].down = (position_I != 0);
                        mTablets[tabletIndex_I].touchRings[controlIndex_I].position = (int)(position_I - 1);

                        for (int index = 0; index < (int)mTablets[tabletIndex_I].touchRings[controlIndex_I].modes.Length; index++)
                        {
                            mTablets[tabletIndex_I].touchRings[controlIndex_I].modes[index].active = (index == modeIndex_I);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }



        /// <summary>
        /// Updates a touchstrip finger position.
        /// Should be called when app receives a WT_PACKETEXT message.
        /// </summary>
        /// <param name="tabletIndex_I"></param>
        /// <param name="controlIndex_I"></param>
        /// <param name="modeIndex_I"></param>
        /// <param name="position_I"></param>
        public void UpdateTouchStrip(int tabletIndex_I, int controlIndex_I, int modeIndex_I, uint position_I)
        {
            DumpUpdate("TS", tabletIndex_I, controlIndex_I, modeIndex_I, (int)position_I);

            try
            {
                if (tabletIndex_I < mTablets.Length)
                {
                    // This tablet may not have touch strips.
                    if (mTablets[tabletIndex_I].touchStrips != null && 
                        controlIndex_I < (int)mTablets[tabletIndex_I].touchStrips.Length)
                    {
                        mTablets[tabletIndex_I].touchStrips[controlIndex_I].down = (position_I != 0);
                        mTablets[tabletIndex_I].touchStrips[controlIndex_I].position = (int)(position_I - 1);

                        for (int index = 0; index < (int)mTablets[tabletIndex_I].touchStrips[controlIndex_I].modes.Length; index++)
                        {
                            mTablets[tabletIndex_I].touchStrips[controlIndex_I].modes[index].active = (index == modeIndex_I);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }



        /// <summary>
        /// Setup extension data dumped to debug output.
        /// </summary>
        /// <param name="ctrlType_I"></param>
        /// <param name="tabletIndex_I"></param>
        /// <param name="controlIndex_I"></param>
        /// <param name="functionIndex_I"></param>
        /// <param name="available_I"></param>
        /// <param name="locationIndex_I"></param>
        /// <param name="min_I"></param>
        /// <param name="max_I"></param>
        private void DumpSetup(String ctrlType_I, int tabletIndex_I, int controlIndex_I, int functionIndex_I, bool available_I, int locationIndex_I, int min_I, int max_I)
        {
            Debug.WriteLine(ctrlType_I +
                ": tab:" + tabletIndex_I +
                "; ctrl:" + controlIndex_I +
                "; func:" + functionIndex_I +
                "; avail:" + available_I.ToString() +
                "; loc:" + locationIndex_I +
                "; min:" + min_I +
                "; max:" + max_I);
        }



        /// <summary>
        /// Update extension data dumped to debug output.
        /// </summary>
        /// <param name="ctrlType_I"></param>
        /// <param name="tabletIndex_I"></param>
        /// <param name="controlIndex_I"></param>
        /// <param name="locationIndex_I"></param>
        /// <param name="state_I"></param>
        private void DumpUpdate(String ctrlType_I, int tabletIndex_I, int controlIndex_I, int locationIndex_I, int state_I)
        {
            Debug.WriteLine(ctrlType_I +  
                ": tab:" + tabletIndex_I + 
                "; ctrl:" + controlIndex_I + 
                "; loc:" + locationIndex_I + 
                "; state:" + state_I);
        }
    }
}
