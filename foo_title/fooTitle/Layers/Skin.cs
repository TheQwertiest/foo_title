/*
    Copyright 2005 - 2006 Roman Plasil
	http://foo-title.sourceforge.net
    This file is part of foo_title.

    foo_title is free software; you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as published by
    the Free Software Foundation; either version 2.1 of the License, or
    (at your option) any later version.

    foo_title is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License
    along with foo_title; if not, write to the Free Software
    Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
*/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using fooManagedWrapper;
using fooTitle.Geometries;

namespace fooTitle.Layers
{ 
    /// <summary>
    ///     Loads itself from an xml file and handles all drawing.
    /// </summary>
    public class Skin : Layer, IPlayCallbackSender
    {
        private readonly XElement _skin;
        public List<Layer> DynamicLayers = new List<Layer>();

        

        /// <summary>
        ///     Loads the skin from the specified xml file
        /// </summary>
        /// <param name="path">Path to the skin directory.</param>
        public Skin(string path)
        {
            SkinDirectory = path;

            // load the skin xml file
            XDocument document = XDocument.Load(GetSkinFilePath("skin.xml"));

            // read the xml document for the basic properties
            _skin = document.Elements("skin").First();

            int width = int.Parse(_skin.Attribute("width").Value);
            int height = int.Parse(_skin.Attribute("height").Value);
            geometry = new AbsoluteGeometry(new Rectangle(0, 0, width, height), width, height, new Point(0, 0));

            // register to main for playback events
            Main.GetInstance().OnPlaybackNewTrackEvent += OnPlaybackNewTrack;
            Main.GetInstance().OnPlaybackTimeEvent += OnPlaybackTime;
            Main.GetInstance().OnPlaybackPauseEvent += OnPlaybackPause;
            Main.GetInstance().OnPlaybackStopEvent += OnPlaybackStop;
        }

        /// <summary>
        ///     Returns the directory of the skin. Can be used for loading images and other data files.
        /// </summary>
        private string SkinDirectory { get; }

        public ToolTip ToolTip { get; private set; }

        public void Init(Display display)
        {
            Display = display;

            // register to mouse events
            Display.MouseMove += Display_MouseMove;
            Display.MouseDown += Display_MouseDown;
            Display.MouseUp += Display_MouseUp;
            Display.MouseLeave += Display_MouseLeave;
            Display.MouseWheel += Display_MouseWheel;
            Display.MouseDoubleClick += Display_MouseDoubleClick;

            InitAnchor();

            LoadLayers(_skin);

            SkinState state = Main.GetInstance().SkinState;
            if (!state.IsStateValid(this))
            {
                state.ResetState();
                state.SaveState(this);
            }
            else
            {
                state.LoadState(this);
            }

            if (HasToolTipLayer(this))
                ToolTip = new ToolTip(display, this);

            geometry.Update(new Rectangle(0, 0, ((AbsoluteGeometry)geometry).Width,
                ((AbsoluteGeometry)geometry).Height));
        }

        /// <summary>
        ///     Call to free this skin (unregistering events, unregistering layer events,...)
        /// </summary>
        public void Free()
        {
            Main.GetInstance().OnPlaybackNewTrackEvent -= OnPlaybackNewTrack;
            Main.GetInstance().OnPlaybackTimeEvent -= OnPlaybackTime;
            Main.GetInstance().OnPlaybackStopEvent -= OnPlaybackStop;
            Main.GetInstance().OnPlaybackPauseEvent -= OnPlaybackPause;

            Display.MouseMove -= Display_MouseMove;
            Display.MouseDown -= Display_MouseDown;
            Display.MouseUp -= Display_MouseUp;
            Display.MouseLeave -= Display_MouseLeave;
            Display.MouseWheel -= Display_MouseWheel;
            Display.MouseDoubleClick -= Display_MouseDoubleClick;
        }

        protected override Size GetMinimalSizeImpl()
        {
            // don't ask geometry..
            return GetContentSize();
        }

        /// <summary>
        ///     Resizes the skin and all layers, but not the form.
        /// </summary>
        /// <param name="newSize">The new size the skin should have</param>
        public void Resize(Size newSize)
        {
            ((AbsoluteGeometry) geometry).Width = newSize.Width;
            ((AbsoluteGeometry) geometry).Height = newSize.Height;
            foreach (Layer l in layers) l.UpdateGeometry(ClientRect);
        }

        /// <summary>
        ///     Asks layers for optimal size and resizes itself and the display in case it's needed.
        /// </summary>
        public void CheckSize()
        {
            Size size = GetMinimalSize();
            if (size.Width != ((AbsoluteGeometry) geometry).Width ||
                size.Height != ((AbsoluteGeometry) geometry).Height)
            {
                Resize(size);
                Main.GetInstance().Display.SetSize(ClientRect.Width, ClientRect.Height);
                Main.GetInstance().RequestRedraw(true);
            }
        }

        /// <summary>
        ///     Asks layers for optimal size and resizes itself and the display in case it's needed.
        ///     Called when the skin is loaded - does full size checking
        /// </summary>
        public void FirstCheckSize()
        {
            Size size = GetMinimalSize();
            Resize(size);
            Main.GetInstance().Display.SetSize(ClientRect.Width, ClientRect.Height);
        }

        /// <summary>
        ///     Optimized version of updateGeometry - processes only the dynamic layers
        /// </summary>
        /// <param name="parentRect">parent rectangle - the geometry should fit in it</param>
        public void UpdateDynamicGeometry(Rectangle parentRect)
        {
            foreach (Layer l in DynamicLayers)
                l.UpdateThisLayerGeometry(l.ParentLayer.ClientRect);
        }

        /// <summary>
        ///     Creates a Bitmap from a file located in the skin folder. Use this to prevent the
        ///     file from being locked all the time.
        /// </summary>
        public Bitmap GetSkinImage(string fileName)
        {
            Image img = Image.FromFile(GetSkinFilePath(fileName)); 

            MemoryStream mstr = new MemoryStream();
            img.Save(mstr, img.RawFormat);

            Bitmap retImg = new Bitmap(mstr);

            img.Dispose();

            return retImg;
        }
        public class SkinInfo
        {
            public string Author;
            public string Name;
        }
        public static SkinInfo GetSkinInfo(string skinPath)
        {
            string skinFullPath = Path.Combine(skinPath, "skin.xml");
            if (!File.Exists(skinFullPath))
                return null;

            try
            {
                XDocument document = XDocument.Load(skinFullPath);

                XElement skin = document.Elements("skin").First();

                return new SkinInfo
                {
                    Name = GetAttributeValue(skin, "name", null),
                    Author = GetAttributeValue(skin, "author", null)
                };
            }
            catch (XmlException e)
            {
                CConsole.Write($"Failed to parse skin from {skinPath}:\n{e}");
                return null;
            }
        }

        /// <summary>
        ///     Does not check for exceptions
        /// </summary>
        /// <param name="eventObj">This must be a delegate</param>
        /// <param name="p">Parameters for the delegate</param>
        protected static void SendEvent(object eventObj, params object[] p)
        {
            Delegate d = (Delegate) eventObj;
            d?.DynamicInvoke(p);
        }

        protected static void SendEventCatch(object eventObj, params object[] p)
        {
            try
            {
                SendEvent(eventObj, p);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Error");
            }
        }

        /// <summary>
        ///     Used for easily finding out what is the path to a skin's file (image, xml, extension,...)
        /// </summary>
        /// <param name="fileName">Name of the file which is searched for.</param>
        /// <returns>Path composed of app's data directory and skin's directory.</returns>
        private string GetSkinFilePath(string fileName)
        {
            return Path.Combine(SkinDirectory, fileName);
        }

        private void InitAnchor()
        {
            string anchorTypeStr = GetAttributeValue(_skin, "anchor", "top,left");
            float anchorDx = GetNumberFromAttribute(_skin, "anchor_dx", 0);
            float anchorDy = GetNumberFromAttribute(_skin, "anchor_dy", 0);

            DockAnchor.Type anchorType = DockAnchor.Type.None;
            foreach (string i in anchorTypeStr.ToLower().Split(new[] {',', ' '}, StringSplitOptions.RemoveEmptyEntries))
                switch (i)
                {
                    case "top":
                        anchorType |= DockAnchor.Type.Top;
                        break;
                    case "bottom":
                        anchorType |= DockAnchor.Type.Bottom;
                        break;
                    case "right":
                        anchorType |= DockAnchor.Type.Right;
                        break;
                    case "left":
                        anchorType |= DockAnchor.Type.Left;
                        break;
                    case "center":
                        anchorType |= DockAnchor.Type.Center;
                        break;
                }

            Display.InitializeAnchor(anchorType, anchorDx, anchorDy);
        }

        private static bool HasToolTipLayer(Layer layer)
        {
            foreach (Layer i in layer.SubLayers)
                if (i.HasToolTip || HasToolTipLayer(i))
                    return true;

            return false;
        }

        private static bool IsMouseOverButton(Layer layer)
        {
            foreach (Layer i in layer.SubLayers)
                if (i.Type == "button" && i.IsMouseOver || IsMouseOverButton(i))
                    return true;

            return false;
        }

        #region Event handling (public)

        #region IPlayCallbackSender Members

#pragma warning disable 0168, 219, 67
        public event OnPlaybackTimeDelegate OnPlaybackTimeEvent;
        public event OnPlaybackNewTrackDelegate OnPlaybackNewTrackEvent;
        public event OnQuitDelegate OnQuitEvent;
        public event OnInitDelegate OnInitEvent;
        public event OnPlaybackStopDelegate OnPlaybackStopEvent;
        public event OnPlaybackPauseDelegate OnPlaybackPauseEvent;
        public event OnPlaybackDynamicInfoTrackDelegate OnPlaybackDynamicInfoTrackEvent;
#pragma warning restore 0168, 219, 67

        #endregion

        public event MouseEventHandler OnMouseMove;
        public event MouseEventHandler OnMouseDown;
        public event MouseEventHandler OnMouseDoubleClick;
        public event MouseEventHandler OnMouseUp;
        public event EventHandler OnMouseLeave;
        public event MouseEventHandler OnMouseWheel;

        public void OnPlaybackTime(double time)
        {
            // pass it on
            SendEvent(OnPlaybackTimeEvent, time);
        }

        public void OnPlaybackNewTrack(CMetaDBHandle song)
        {
            // pass it on
            SendEvent(OnPlaybackNewTrackEvent, song);
            CheckSize();
        }

        public void OnPlaybackStop(IPlayControl.StopReason reason)
        {
            // pass it on
            SendEvent(OnPlaybackStopEvent, reason);
            if (reason != IPlayControl.StopReason.stop_reason_starting_another)
                CheckSize();
        }

        public void OnPlaybackPause(bool state)
        {
            // pass it on
            SendEvent(OnPlaybackPauseEvent, state);
        }

        #endregion // Event handling (public)

        #region Event handling (private)

        private void Display_MouseUp(object sender, MouseEventArgs e)
        {
            SendEventCatch(OnMouseUp, sender, e);
            ToolTip?.OnMouseUp(sender, e);
        }

        private void Display_MouseDown(object sender, MouseEventArgs e)
        {
            SendEventCatch(OnMouseDown, sender, e);
            ToolTip?.OnMouseDown(sender, e);
        }

        private void Display_MouseMove(object sender, MouseEventArgs e)
        {
            SendEventCatch(OnMouseMove, sender, e);
            ToolTip?.OnMouseMove(sender, e);
            Main.GetInstance().CanDragDisplay = !IsMouseOverButton(this);
        }

        private void Display_MouseLeave(object sender, EventArgs e)
        {
            Main.GetInstance().CanDragDisplay = true;
            SendEventCatch(OnMouseLeave, sender, e);
            ToolTip?.ClearToolTip();
        }

        private void Display_MouseWheel(object sender, EventArgs e)
        {
            SendEventCatch(OnMouseWheel, sender, e);
        }

        private void Display_MouseDoubleClick(object sender, EventArgs e)
        {
            SendEventCatch(OnMouseDoubleClick, sender, e);
        }

        #endregion // Event handling (private)
    }
}