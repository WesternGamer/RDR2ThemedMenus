using Sandbox.Graphics.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using VRageMath;

namespace Rdr2ThemedMenus.GUI.GuiControls
{
    internal class ButtonPrompt
    {
        Vector2 currentPosition;

        IMyGuiControlsParent parent = null;

        MyGuiControlBase[] buttons = null;

        public ButtonPrompt(IMyGuiControlsParent parent, MyGuiControlBase[] buttons)
        {
            this.parent = parent;
            this.buttons = buttons.Reverse().ToArray();

            LayoutControls();

            
        }

        private void LayoutControls()
        {
            currentPosition = new Vector2(1.1030f, 0.945f);
            foreach (Button button in buttons)
            {               
                button.Position = currentPosition;
                currentPosition.X -= button.Size.X;
                parent.Controls.Add(button);
            }
        } 
    }
}
    