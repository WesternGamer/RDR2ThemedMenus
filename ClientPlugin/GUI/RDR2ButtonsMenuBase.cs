using Rdr2ThemedMenus.GUI.GuiControls;
using Sandbox.Graphics;
using Sandbox.Graphics.GUI;
using Sandbox.Gui;
using System.IO;
using System.Text;
using VRage;
using VRage.Input;
using VRage.Utils;
using VRageMath;

namespace Rdr2ThemedMenus.GUI
{
    internal abstract class RDR2ButtonsMenuBase : MyGuiScreenBase
    {
        protected ButtonPrompt ButtonPrompt;

        public RDR2ButtonsMenuBase()
        {
            m_closeOnEsc = false;
            m_position = Vector2.Zero;
        }


        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);

            Button selectBtn = new Button(MyKeys.Enter, null, MyTexts.Get(MyCommonTexts.Select).ToString(), delegate { });

            Button backBtn = new Button(MyKeys.Escape, null, MyTexts.Get(MyCommonTexts.Back).ToString(), _ => CloseScreen());

            ButtonPrompt = new ButtonPrompt(this, new MyGuiControlBase[] { selectBtn, backBtn });

            
        }

        public override void LoadContent()
        {
            base.LoadContent();
            RecreateControls(true);
        }

        public override int GetTransitionOpeningTime()
        {
            return 800;
        }

        public override int GetTransitionClosingTime()
        {
            return 800;
        }

        public override bool Draw()
        {
            MyGuiManager.DrawSpriteBatch(Path.Combine(Plugin.Instance.ContentDirectory, @"Textures\RDR2ButtonsMenuBase\pause_background.dds"), Vector2.Zero, Vector2.One, new Color(255, 255, 255, m_transitionAlpha), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, true);
            MyGuiManager.DrawSpriteBatch(Path.Combine(Plugin.Instance.ContentDirectory, @"Textures\RDR2ButtonsMenuBase\divider_line.dds"), new Vector2(0.5f, 0.88612f), new Vector2(0.89583f, 0.00370f), new Color(255, 255, 255, m_transitionAlpha * 0.25f), MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP, true);
            MyGuiManager.DrawSpriteBatch(Path.Combine(Plugin.Instance.ContentDirectory, @"Textures\RDR2ButtonsMenuBase\divider_line.dds"), new Vector2(0.5f, 0.112037f), new Vector2(0.89583f, 0.00370f), new Color(255, 255, 255, m_transitionAlpha * 0.25f), MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP, true);
            
            return base.Draw();
        }
    }
}
