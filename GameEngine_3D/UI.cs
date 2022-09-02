////////////////////////////////////////////////////////////////////////////////
//                                                                            //
//      Submitted for the BSc in Computer Science for Games Development       //
//      Project code: SG7 (3D Gorillas)                                       //
//      By Denis Volosin                                                      //
//                                                                            //
//      UI.cs                                                                 //
//                                                                            //
//      Abstract UI object                                                    //
//                                                                            //
////////////////////////////////////////////////////////////////////////////////

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace GameEngine_3D
{
    ///////////////////////////////////////////////////////////////////////////
    // UI class

    public class UI
    {
        ///////////////////////////////////////////////////////////////////////
        // Private class types

        // Basic rectangular UI element (background fill only)
        public class BasicElement
        {
            public int ID;
            public Color BkColour;
            public Rectangle Rect;

            // Constructor
            public BasicElement(int id, Color bkColour)
			{
                // Save element params. These can be changed later at runtime too.
				ID = id;
                BkColour = bkColour;
                Rect = new Rectangle(0, 0, 0, 0);
        	}

            // Draw element
            public virtual void Draw(Graphics graphics)
            {
                SolidBrush bkBrush = new SolidBrush(BkColour);
			    graphics.FillRectangle(bkBrush, Rect);
            }
        }

        // Text UI element
        public class TextElement: BasicElement
        {
            public string Text;
            public StringAlignment TextAlign;
            public Color TextColour;
            public Font TextFont;

            // Constructor
            public TextElement(int id, Color bkColour, string text, int textSize, StringAlignment textAlign, Color textColour) :
                base(id, bkColour)
            {
                // Save element params. These can be changed later at runtime too.
                Text = text;
                TextAlign = textAlign;
                TextColour = textColour;
                TextFont = new Font("Arial", textSize);
            }

            // Draw element
            public override void Draw(Graphics graphics)
            {
                // Draw background
                base.Draw(graphics);

                // Draw text
                StringFormat stringFormat = new StringFormat();
                stringFormat.Alignment = TextAlign;
                stringFormat.LineAlignment = TextAlign;
                SolidBrush stringBrush = new SolidBrush(TextColour);
                graphics.DrawString(Text, TextFont, stringBrush, Rect, stringFormat);
            }
        }

        // Button UI element
        public class ButtonElement: TextElement
        {
            // Constructor
            public ButtonElement(int id, Color bkColour, string text, int textSize, StringAlignment textAlign, Color textColour) :
                base(id, bkColour, text, textSize, textAlign, textColour)
            {
            }

            // Hit test (used for mouse input)
            public bool HitTest(int x, int y)
            {
                // Simply check if coordinates are within the element rect
                return Rect.Contains(x, y);
            }
        }


        ///////////////////////////////////////////////////////////////////////
        // Private class data

        Bitmap mBitmap;                          // Bitmap the UI graphics is drawn into
        int mTextureID;                          // OpenGL texture ID
        Dictionary<int, BasicElement> mElements; // UI elements dictionary
        bool mForceRedraw;                       // Force UI redraw flag (e.g. window size change)


        ///////////////////////////////////////////////////////////////////////
        // Constructors

        // Default constructor
        public UI()
        {
            mBitmap = null; // Can't be created until we know viewport size
            mTextureID = 0; // Can't be created until we know viewport size
            mElements = new Dictionary<int, BasicElement>(); // Empty list
            mForceRedraw = false; // Nothing to draw yet
        }

        ///////////////////////////////////////////////////////////////////////
        // Properties

        // Access forced redraw flag
        public bool ForceRedraw
        {
            get { return mForceRedraw; }
            set { mForceRedraw = value; }
        }


        ///////////////////////////////////////////////////////////////////////
        // Public methods

        // Resize UI
        public void Resize(int width, int height)
        {
            // Rebuild the bitmap matching new UI size
            mBitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            // Recreate texture every time as the size may well have changed since the
            // previous call!
            if (mTextureID != 0)
                GL.DeleteTexture(mTextureID);

            mTextureID = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2D, mTextureID);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, mBitmap.Width, mBitmap.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            // Force redraw
            mForceRedraw = true;
        }

        // Render UI
        public void Render()
        {
            // Only if bitmap was created (i.e. UI size is known)
            if (mBitmap != null)
            {
                // Redraw UI elements if needed
                if (mForceRedraw)
                {
                    // Graphics for drawing
                    Graphics graphics = Graphics.FromImage(mBitmap);

                    // Wipe the content so we start with clean background
                    graphics.Clear(Color.FromArgb(0, 0, 0, 0));

                    // Draw elements
                    foreach (KeyValuePair<int, BasicElement> pair in mElements)
                        pair.Value.Draw(graphics);

                    // Upload texture data
                    GL.BindTexture(TextureTarget.Texture2D, mTextureID);
                    BitmapData data = mBitmap.LockBits(new Rectangle(0, 0, mBitmap.Width, mBitmap.Height),
                        ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, (int)mBitmap.Width, (int)mBitmap.Height, 0,
                        OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
                    mBitmap.UnlockBits(data);

                    GL.BindTexture(TextureTarget.Texture2D, 0);

                    mForceRedraw = false;
                }

                // Render texture quad
                GL.BindTexture(TextureTarget.Texture2D, mTextureID);

                GL.MatrixMode(MatrixMode.Projection);
                GL.LoadIdentity();
                GL.Ortho(0, mBitmap.Width, 0, mBitmap.Height, -1, 1);

                GL.Begin(PrimitiveType.Quads);
                GL.TexCoord2(0f, 1f); GL.Vertex2(0, 0);
                GL.TexCoord2(1f, 1f); GL.Vertex2(mBitmap.Width, 0);
                GL.TexCoord2(1f, 0f); GL.Vertex2(mBitmap.Width, mBitmap.Height);
                GL.TexCoord2(0f, 0f); GL.Vertex2(0, mBitmap.Height);
                GL.End();

                GL.BindTexture(TextureTarget.Texture2D, 0);
            }
        }

        // Add UI element
        public void AddElement(BasicElement element)
        {
            mElements.Add(element.ID, element);
            mForceRedraw = true;
        }

        // Find UI element
        public BasicElement FindElement(int id)
        {
            BasicElement element = null;
            mElements.TryGetValue(id, out element);
            return element;
        }

        // Remove UI element
        public void RemoveElement(int id)
        {
            mElements.Remove(id);
            mForceRedraw = true;
        }

        // Remove all elements
        public void RemoveAllElements()
        {
            mElements = new Dictionary<int, BasicElement>();
            mForceRedraw = true;
        }

        // Hit test (button elemets only!)
        public int HitTest(int x, int y)
        {
            foreach (KeyValuePair<int, BasicElement> pair in mElements)
            {
                ButtonElement button = pair.Value as ButtonElement;
                if ((button != null) && button.HitTest(x, y))
                    return button.ID;
            }

            return -1;
        }
    }
}
