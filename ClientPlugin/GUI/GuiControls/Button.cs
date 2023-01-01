using Sandbox;
using Sandbox.Engine.Utils;
using Sandbox.Graphics;
using Sandbox.Graphics.GUI;
using System;
using VRage.Audio;
using VRage.Input;
using VRage.Utils;
using VRageMath;

namespace Rdr2ThemedMenus.GUI.GuiControls
{
    internal class Button : MyGuiControlBase
    {
        public MyKeys ActionKey = MyKeys.None;

        private bool isKeyPressed = false;

        public Action<Button> ButtonClicked;

        public string Text = "";

        public Button(MyKeys key = MyKeys.None, Vector2? position = null, string text = "", Action<Button> onClick = null) : base(position: position ?? Vector2.Zero, originAlign: MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM)
        {
            CanPlaySoundOnMouseOver = false;
            ActionKey = key;
            Text = text;
            ButtonClicked = onClick;
            DEBUG_CONTROL_BORDERS = true;

            Size = new Vector2(MyGuiManager.MeasureString("RDRLino", Text, 1).X + 0.048f, 0.05f);

        }

        public override void Draw(float transitionAlpha, float backgroundTransitionAlpha)
        {
            DrawElements(transitionAlpha, backgroundTransitionAlpha);
            DrawBorder(transitionAlpha);


            DrawButton(transitionAlpha);
            MyGuiManager.DrawString("RDRLino", Text, GetPositionAbsoluteCenterLeft() + new Vector2(0.005f, 0), 1, new Color(1, 1, 1, transitionAlpha), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
        }

        private void DrawButton(float transitionAlpha)
        {
            Vector2 buttonOffset = new Vector2(0.0305f, 0.014f);

            CustomGuiTools.DrawRectangle(GetPositionAbsoluteCenterRight() - buttonOffset, new Vector2(0.028f, 0.028f), new Color(0, 0, 0, transitionAlpha)); //Black
            CustomGuiTools.DrawRectangle(GetPositionAbsoluteCenterRight() - buttonOffset, new Vector2(0.027f, 0.028f), new Color(0.5294f, 0.5294f, 0.5294f, transitionAlpha)); //Grey
            CustomGuiTools.DrawRectangle(GetPositionAbsoluteCenterRight() - buttonOffset, new Vector2(0.027f, 0.027f), new Color(1, 1, 1, transitionAlpha)); //White
            //MyGuiManager.DrawString("RDRLino", "S", GetPositionAbsoluteCenterRight() - buttonOffset, 1, new Color(1, 1, 1, transitionAlpha), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            if (isKeyPressed)
            {
                CustomGuiTools.DrawRectangle(GetPositionAbsoluteCenterRight() - buttonOffset - new Vector2(0.006f, 0.008f), new Vector2(0.043f, 0.043f), new Color(1, 1, 1, transitionAlpha - 0.25f));
            }
        }

        public override MyGuiControlBase HandleInput()
        {
            MyGuiControlBase myGuiControlBase = base.HandleInput();
            if (myGuiControlBase == null)
            {
                if (MyInput.Static.IsKeyPress(ActionKey) || (IsMouseOver && MyInput.Static.IsButtonPressed(MySharedButtonsEnum.Primary)))
                {
                    isKeyPressed = true;
                }
                else
                {
                    isKeyPressed = false;
                }

                if (MyInput.Static.IsNewKeyReleased(ActionKey) || (IsMouseOver && MyInput.Static.IsNewLeftMouseReleased()))
                {
                    if (MyAudio.Static != null)
                    {
                        MyCueId cueId = new MyCueId(MyStringHash.TryGet("RDR2GuiButtonClick"));
                        MyAudio.Static.PlaySound(cueId);
                    }
                    ButtonClicked.InvokeIfNotNull(this);
                    myGuiControlBase = this;
                }

            }
            

            return myGuiControlBase;
        }


    }
}
