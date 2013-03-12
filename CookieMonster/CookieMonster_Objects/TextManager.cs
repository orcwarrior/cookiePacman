using System;
using System.Collections.Generic;
using System.Text;
using QuickFont;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using EngineApp;
using System.Drawing;
namespace CookieMonster.CookieMonster_Objects
{
    class Text : engineReference
    {
        public QFont fontFace { get; private set; }
        ProcessedText txt;
        public int layer { get; set; } // which layer it will be rendered at
        public bool active { get { if (timeActive != null) return timeActive.enabled; return true; } }
        public bool updatedThisFrame { get; set; }
        public bool addedToViewport { get; set; }
        public float x { get; private set; }
        public float y { get; private set; }
        public float orgX { get; private set; }
        public float orgY { get; private set; }
        Timer timeActive;
        Point perPreRenderMove;
        private String _msg; 
        public String msg 
        {   get { return _msg; }
            set
            {
                _msg = value;
            }
        }
        private void _correctLayer()
        {
            if (Layer.currentlyWorkingLayer >= 0) layer = Layer.currentlyWorkingLayer;
        }
        public Text(QFont qf,float _x,float _y,string m)
        {
            layer = Layer.textFG;
            _correctLayer();
            updatedThisFrame = addedToViewport = true;
            fontFace = qf; 
            txt = new ProcessedText();
            x  = orgX = _x; y = orgY = _y;
            _msg = m;
            engine.textManager.addText(this);
        }
        public Text(QFont qf,float _x,float _y,string m, QFontAlignment align, int maxWidth)
            :this(qf,_x,_y,m)
        {
            txt = qf.ProcessText(m, maxWidth, align);
        }
        public Text(int _layer, QFont qf, float _x, float _y, string m)
            :this(qf,_x,_y,m)
        {
            layer = _layer;
        }

        public void changeFont(QFont newFnt)
        {
            fontFace = newFnt;
        }
        public void changeText(string newText)
        {
            _msg = newText;
        }
        public void Update()
        {
            if (active)
            {
                if (!addedToViewport) engine.textManager.addText(this);

                updatedThisFrame = true;
                if (perPreRenderMove != null)
                    Move(perPreRenderMove.X, perPreRenderMove.Y);  
                //now render occurs through Text Manager:
                //fontFace.Print(_msg, new Vector2(x, y));
                }
        }
        /// <summary>
        /// USE ONLY WHEN NECESSARY!!!
        /// Normaly text are printed by textManager
        /// but there are some exceptions when you might need to render it manually.
        /// </summary>
        public void Render()
        {
           fontFace.Print(_msg, new Vector2(x, y));
        }
        public void Move(int _x, int _y)
        {
            x += (float)_x; y += (float)_y;
        }
        /// <summary>
        /// Set new position absolute
        /// </summary>
        public void MoveAbs(int _x, int _y)
        {
            x = (float)_x; y = (float)_y;
        }
        public void setLifeTime(float ms)
        {
            timeActive = new Timer(Timer.eUnits.MSEC, (int)ms);
            timeActive.start();
        }
        public void setAnimationMove(Point p)
        {
            perPreRenderMove = p;
        }
    }
    class QFontConstructorData
    {
        public float size { get; private set; }
        public FontStyle style { get; private set; }
        public bool dropShadow { get; private set; }
        public OpenTK.Graphics.Color4 color { get; set; }
        public QFont font { get; private set; }

        public QFontConstructorData(float siz, FontStyle styl, bool shadow, OpenTK.Graphics.Color4 col, QFont fnt)
        {
            size = siz;
            style = styl;
            dropShadow = shadow;
            color = col;
            font = fnt;
        }
        public override string ToString()
        {
            return size.ToString() + "pt. " + style.ToString() + " " + color.ToString() + " shadow: " + dropShadow.ToString();
        }
    }
    class TextManager
    {
        const int screenMargin = 32;//in px;
        static public QFont font_default;
        static public QFont font_default_20;
        List<List<Text>> onScreenTexts = new List<List<Text>>();

        //default font builders:
        static QFontBuilderConfiguration defaultBuilder = new QFontBuilderConfiguration();
        static QFontBuilderConfiguration defaultBuilderDropShadow = new QFontBuilderConfiguration(true);
        static string fontDir = "../Data/Fonts/";
        static int fontsUsed = 0; //number of already loaded, different fonts through newQFont
        static List<string> fontFileNames = new List<string>();
        static List<List<QFontConstructorData>> definiedFonts = new List<List<QFontConstructorData>>();

        public int textsCount{get{int r=0;for(int i=0;i<onScreenTexts.Count;i++)r+=onScreenTexts[i].Count; return r;}}
        public TextManager()
        {
            font_default = newQFont("Tepeno Sans Regular.ttf", 14,true);
            font_default_20 = newQFont("Tepeno Sans Regular.ttf", 20,true);        

            defaultBuilder.charSet = "aąbcćdeęfghijkłlmnńoópqrsśtuvwxyzżźAĄBCĆDEĘFGHIJKŁLMNŃOÓPQRSŚTUVWXYZŻŹ1234567890.:,;'\"(!?)+-*/=_{}[]@~#\\<>|^%$£&";
            defaultBuilderDropShadow.charSet = "aąbcćdeęfghijkłlmnńoópqrsśtuvwxyzżźAĄBCĆDEĘFGHIJKŁLMNŃOÓPQRSŚTUVWXYZŻŹ1234567890.:,;'\"(!?)+-*/=_{}[]@~#\\<>|^%$£&";
        }
        public static int inspectFontFamily(string fileName)
        {
            for (int i = 0; i < fontFileNames.Count; i++)
                if (fontFileNames[i] == fileName)
                    return i;
            return -1;//no font famili founded
        }
        /// <summary>
        /// Inspect font, check if it was already created by textManager
        /// if it was found with same size etc. then returns it, elsewhere returns null
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static QFont inspectFont(int fntIdx, float size, FontStyle style, bool dropShadow, OpenTK.Graphics.Color4 color)
        {
              for(int j=0;j<definiedFonts[fntIdx].Count;j++)
              {
                  QFontConstructorData f = definiedFonts[fntIdx][j];
                  if (f.size == size && f.style == style
                     && f.dropShadow == dropShadow && f.color == color)
                  {//font matches!
                      return f.font;
                  }
              }            
            return null;
        }
        private static QFont internalNewQFont(string filename, float size, FontStyle style, bool dropShadow, OpenTK.Graphics.Color4 color)
        {//white is default QFont Color
            int idx = inspectFontFamily(filename);
            if (idx == -1)
            {
                fontFileNames.Add(filename);
                definiedFonts.Add(new List<QFontConstructorData>());
                fontsUsed++;
                idx = fontFileNames.Count - 1;
            }
            QFont f = inspectFont(idx, size, style, dropShadow,color);
            if (f == null) //no font founded:
            {
                if(dropShadow)
                    f = new QFont(fontDir+filename, size, style,defaultBuilderDropShadow);
                else
                    f = new QFont(fontDir+filename, size, style,defaultBuilder);
                f.Options.Colour = color;
                //add fontConstructor data
                definiedFonts[idx].Add(new QFontConstructorData(size,style,dropShadow,color,f));
                //Debug stuff:
                //new DebugMsg("New font #"+definiedFonts[idx].Count + " " + filename + " " + size.ToString()+"pt.");
            }
            return f;

        }
        public static QFont newQFont(string fileName,float size)
        {
            return internalNewQFont(fileName, size, FontStyle.Regular, false, new OpenTK.Graphics.Color4(255, 255, 255, 255));
        }
        public static QFont newQFont(string fileName, float size, bool dropShadow)
        {
            return internalNewQFont(fileName, size, FontStyle.Regular, dropShadow, new OpenTK.Graphics.Color4(255, 255, 255, 255));
        }
        public static QFont newQFont(string fileName, float size, FontStyle style, bool dropShadow)
        {
            return internalNewQFont(fileName, size, style, dropShadow, new OpenTK.Graphics.Color4(255, 255, 255, 255));
        }
        public static QFont newQFont(string fileName, float size, FontStyle style, bool dropShadow, OpenTK.Graphics.Color4 color)
        {
            return internalNewQFont(fileName, size, style, dropShadow, color);
        }
        public static QFont newQFont(string fileName, float size, bool dropShadow, OpenTK.Graphics.Color4 color)
        {
            return internalNewQFont(fileName, size, FontStyle.Regular, dropShadow, color);
        }

        /*
        public void addText(QFont qf,string txt,float x,float y)
        {
            addText(qf, txt, x, y, QFontAlignment.Left);
        }
        public void addText(QFont qf, string txt, float x, float y,QFontAlignment align)
        {
            float actWidth = qf.Measure(txt).Width;
            float width = (float)engine.activeViewport.width;

            width -= screenMargin;
            ProcessedText pTxt;
            if (align != QFontAlignment.Right)
            {
                pTxt = qf.ProcessText(txt, width, align);
                onScreenTexts.Add(new Text(qf, pTxt, x, y, txt));
            }
            else
            {
                pTxt = qf.ProcessText(txt, actWidth, QFontAlignment.Left);
                onScreenTexts.Add(new Text(qf, pTxt, x - actWidth, y, txt));
            }
            
        }
         */
        public void addText(Text txt)
        {
            while (txt.layer >= onScreenTexts.Count)
                onScreenTexts.Add(new List<Text>());
            onScreenTexts[txt.layer].Add(txt);
            txt.addedToViewport = true;
        }

        public void Render(int layer)
        {
            // Requested layer isn't present in created layers of text
            if (layer >= onScreenTexts.Count) return;
            QFont.Begin();
            for (int i = 0; i < onScreenTexts[layer].Count; i++)
            {
                if (onScreenTexts[layer][i].active == false || onScreenTexts[layer][i].updatedThisFrame == false)
                { onScreenTexts[layer][i].addedToViewport = false; onScreenTexts[layer].RemoveAt(i); i--; }
                else if (onScreenTexts[layer][i].updatedThisFrame)
                {
                    Text txt = onScreenTexts[layer][i];
                    txt.fontFace.Print(txt.msg, new Vector2(txt.x, txt.y));
                    //onScreenTexts[i].Update();
                    onScreenTexts[layer][i].updatedThisFrame = false;
                }
            }
            QFont.End();
        }
        /// <summary>
        /// search for first Text with equal string with one in arguments
        /// then simply dropping it from the list
        /// </summary>
        /// <param name="txt"></param>
        public void removeText(string txt)
        {
            for (int i = 0; i < onScreenTexts.Count; i++)
                for (int j = 0; j < onScreenTexts[i].Count; j++)
                {
                    if (onScreenTexts[i][j].msg == txt)
                    {
                        onScreenTexts[i][j].addedToViewport = false;
                        onScreenTexts[i].RemoveAt(j); return;
                    }
                }
        }
        public void clearAll()
        {
            for (int i = 0; i < onScreenTexts.Count; i++)
                for (int j = 0; j < onScreenTexts[i].Count; j++)
                {
                    if (onScreenTexts[i][j]!= null)
                    {
                        onScreenTexts[i][j].addedToViewport = false;
                        onScreenTexts[i].RemoveAt(j); j--;
                    }
                }
        }
        public static string breakTextWithNewlines(string txt, int charsPerLine)
        {
            int lastSpaceAt=-1,lineLength=0;
            //string txtOut = new string(txt.ToCharArray());
            StringBuilder txtOut = new StringBuilder(txt);
            for (int i = 0; i < txtOut.Length; i++)
            {
                if (txtOut[i] == ' ')
                    lastSpaceAt = i;
                if (txtOut[i] == '\n') //new line found!
                {
                    lineLength = 0;
                    lastSpaceAt = -1;
                }
                lineLength++;
                if (lineLength >= charsPerLine && lastSpaceAt > 0)
                {//break to new line
                    txtOut[lastSpaceAt] = '\n';
                    lineLength = i-lastSpaceAt;
                    lastSpaceAt = -1;
                }
                else if (lineLength >= charsPerLine)
                { // no space in this line, just enter to next line and put "-" infront
                    txtOut[i] = '\n';
                    txtOut.Insert(i, "-");
                    i += 2;
                    lineLength = 0;
                    lastSpaceAt = -1;
                }
            }
            return txtOut.ToString();
        }   
    }
}
