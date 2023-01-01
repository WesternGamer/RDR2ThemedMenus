using Sandbox.Graphics;
using Sandbox.Graphics.GUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Audio;
using VRage.Input;
using VRage.Utils;
using VRageMath;

namespace Rdr2ThemedMenus.GUI.GuiControls
{
    internal class ImageButton : MyGuiControlBase
    {
        public string Text = "";

        public event Action<ImageButton> OnClick = null;

        public string Texture = "";

        private bool highlight = false;

        public ImageButton(Vector2? position = null, Vector2? size = null, MyGuiDrawAlignEnum originAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, string text = "", Action<ImageButton> onClick = null, string texture = "") : base(position, size, originAlign: originAlign)
        {
            CanPlaySoundOnMouseOver = false;
            Text = text;
            OnClick = onClick;
            Texture = texture;
        }

        public override MyGuiControlBase HandleInput()
        {
            MyGuiControlBase myGuiControlBase = base.HandleInput(); 
            if (myGuiControlBase == null)
            {
                if (IsMouseOver || IsMouseOver && MyInput.Static.IsButtonPressed(MySharedButtonsEnum.Primary))
                {
                    highlight = true;
                }
                else
                {
                    highlight = false;
                }

                if (IsMouseOver && MyInput.Static.IsNewLeftMouseReleased())
                {
                    if (MyAudio.Static != null)
                    {
                        MyAudio.Static.MusicAllowed = true;
                        MyAudio.Static.Mute = false;
                        MyCueId cueId = new MyCueId(MyStringHash.TryGet("RDR2GuiButtonClick"));
                        MyAudio.Static.PlaySound(cueId);
                    }
                    OnClick.InvokeIfNotNull(this);
                    myGuiControlBase = this;
                    return myGuiControlBase;
                }
            }

            return myGuiControlBase;
        }


        public override void Draw(float transitionAlpha, float backgroundTransitionAlpha)
        {
            MyGuiManager.DrawSpriteBatch(Texture, GetPositionAbsoluteTopLeft(), (Vector2)GetSize(), new Color(255, 255, 255, transitionAlpha), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, true);
            MyGuiManager.DrawString("RDRLino", Text, GetPositionAbsoluteBottomLeft() + new Vector2(0.005f, -0.005f), 1, null, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM, true);

            if (highlight)
            {
                DrawBorder();
            }
        }

        private void DrawBorder()
        {

        }
    }
}
