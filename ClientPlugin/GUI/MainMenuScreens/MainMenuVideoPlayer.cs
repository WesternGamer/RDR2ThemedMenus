using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Sandbox;
using Sandbox.Engine.Utils;
using Sandbox.Game;
using Sandbox.Game.Gui;
using Sandbox.Graphics;
using Sandbox.Graphics.GUI;
using VRage;
using VRage.FileSystem;
using VRage.Input;
using VRage.Utils;
using VRageMath;
using VRageRender;
using VRageRender.Messages;

namespace Rdr2ThemedMenus.GUI.MainMenuScreens
{
	//Copied from MyGuiScreenIntroVideo, this version supports custom video resolutions.
	internal class MainMenuVideoPlayer : MyGuiScreenBase
    {
		private uint videoID = uint.MaxValue;

		private bool playbackStarted;

		private string currentVideo = "";

		private int transitionTime = 300;

		private Vector4 colorMultiplier = Vector4.One;

		private Rectangle videoSize;

		public MainMenuVideoPlayer(string video,int transitionTime)
			: base(Vector2.Zero)
		{
			MyRenderProxy.Settings.RenderThreadHighPriority = true;
			Thread.CurrentThread.Priority = ThreadPriority.Highest;
			base.DrawMouseCursor = false;
			base.CanHaveFocus = false;
			m_closeOnEsc = false;
			m_drawEvenWithoutFocus = true;
			this.currentVideo = video;
			this.transitionTime = transitionTime;
			this.videoSize = new Rectangle(0, (int)(MySandboxGame.ScreenSize.Y * 0.3842592f), MySandboxGame.ScreenSize.X, (int)(MySandboxGame.ScreenSize.Y * 0.3907407f));
			MySandboxGame.Static.OnScreenSize += OnScreenSizeChanged;
		}

		private void OnScreenSizeChanged(Vector2I newSize)
		{
			videoSize.Y = (int)(newSize.Y * 0.3842592f);
            videoSize.Width = newSize.X;
			videoSize.Height = (int)(newSize.Y * 0.3907407f);
        }

		public override string GetFriendlyName()
		{
			return "MyGuiScreenIntroVideo";
		}

		public override void LoadContent()
		{
			if (currentVideo != "")
			{
				playbackStarted = false;
			}
			base.LoadContent();
		}

		public override void CloseScreenNow(bool isUnloading = false)
		{
			if (base.State != MyGuiScreenState.CLOSED)
			{
				UnloadContent();
			}
			MyRenderProxy.Settings.RenderThreadHighPriority = false;
			Thread.CurrentThread.Priority = ThreadPriority.Normal;
			base.CloseScreenNow(isUnloading);
		}

		private void CloseVideo()
		{
			if (videoID != uint.MaxValue)
			{
				MyRenderProxy.CloseVideo(videoID);
				videoID = uint.MaxValue;
			}
		}

		public override void UnloadContent()
		{
            MySandboxGame.Static.OnScreenSize -= OnScreenSizeChanged;
            CloseVideo();
			currentVideo = "";
			base.UnloadContent();
		}

		public override bool Update(bool hasFocus)
		{
			if (!base.Update(hasFocus))
			{
				return false;
			}
			if (!playbackStarted)
			{
				TryPlayVideo();
				playbackStarted = true;
			}
			else
			{
				if (MyRenderProxy.IsVideoValid(videoID) && MyRenderProxy.GetVideoState(videoID) != 0)
				{
                    TryPlayVideo();
                }
			}
			return true;
		}

		public override int GetTransitionOpeningTime()
		{
			return transitionTime;
		}

		public override int GetTransitionClosingTime()
		{
			return transitionTime;
		}

		private void TryPlayVideo()
		{
			if (MyFakes.ENABLE_VIDEO_PLAYER)
			{
				CloseVideo();
				if (File.Exists(currentVideo))
				{
					videoID = MyRenderProxy.PlayVideo(currentVideo, 0);
				}
			}
		}

		public override bool CloseScreen(bool isUnloading = false)
		{
			bool num = base.CloseScreen(isUnloading);
			MyRenderProxy.Settings.RenderThreadHighPriority = false;
			Thread.CurrentThread.Priority = ThreadPriority.Normal;
			if (num)
			{
				CloseVideo();
			}
			return num;
		}

		public override bool Draw()
		{
			if (MyRenderProxy.IsVideoValid(videoID))
			{
				MyRenderProxy.UpdateVideo(videoID);
				Vector4 vector = colorMultiplier * m_transitionAlpha;
				MyRenderProxy.DrawVideo(videoID, videoSize, new Color(vector), MyVideoRectangleFitMode.AutoFit, ignoreBounds: true);
			}
			return true;
		}
	}
}
