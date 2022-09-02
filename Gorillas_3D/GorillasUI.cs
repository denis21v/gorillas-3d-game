////////////////////////////////////////////////////////////////////////////////
//                                                                            //
//      Submitted for the BSc in Computer Science for Games Development       //
//      Project code: SG7 (3D Gorillas)                                       //
//      By Denis Volosin                                                      //
//                                                                            //
//      GorillasUI.cs                                                         //
//                                                                            //
//      Gorillas game UI                                                      //
//                                                                            //
////////////////////////////////////////////////////////////////////////////////

using System;
using System.Drawing;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Input;
using GameEngine_3D;

namespace Gorillas_3D
{
    ///////////////////////////////////////////////////////////////////////////
    // GorillasUI class

    public class GorillasUI: UI, IRenderTarget
    {
        ///////////////////////////////////////////////////////////////////////
        // Private class types

        // UI elements
        enum UIElement
        {
            UI_Title,
            UI_Background,
            MainMenu_Button_NewGame,
            MainMenu_Button_Controls,
            MainMenu_Button_About,
            MainMenu_Button_Exit,
            ModalDialog_Content,
            ModalDialog_Button_Back,
            HUD_Text_Player1,
            HUD_Text_Player2,
            HUD_Text_Params
        }


        ///////////////////////////////////////////////////////////////////////
        // Private class data

        GorillasGame mGame;               // Reference to Gorillas game object
        UI.TextElement mHudTextParams;    // HUD throw params text
        UI.TextElement[] mHudTextPlayer;  // HUD player text
        Color mHudActiveColour;           // HUD active colour
        Color mHudInactiveColour;         // HUD inactive colour


        ///////////////////////////////////////////////////////////////////////
        // Constructors

        // Standard scene constructor
        public GorillasUI(GorillasGame game) :
            base()
        {
            // Save reference to game object as we'll need it later
            mGame = game;

            // HUD player texts
            mHudTextPlayer = new UI.TextElement[2];

            // HUD colours
            mHudActiveColour = Color.FromArgb(255, 0, 255, 255);
            mHudInactiveColour = Color.FromArgb(255, 175, 220, 220);
        }


        ///////////////////////////////////////////////////////////////////////
        // Public methods

        // Show main menu
        public void ShowMainMenu()
        {
            UpdateControls();
        }

        // Show game controls information modal dialog
        public void ShowControlsDialog()
        {
            UpdateControls();
        }

        // Show about dialog
        public void ShowAboutDialog()
        {
            UpdateControls();
        }

        // Show game HUD
        public void ShowGameHUD()
        {
            UpdateControls();
        }

        // Show/hide player text
        public void ShowPlayerText(int player, bool show)
        {
            // Update player text colour
            mHudTextPlayer[player].TextColour = show ?
                Color.Black : mHudInactiveColour;

            // Force redraw
            ForceRedraw = true;
        }

        // Set active player
        public void SetActivePlayer(int player)
        {
            // Update player texts background colour
            if (player >= 0)
            {
                // One active, one inactive
                mHudTextPlayer[player].BkColour = mHudActiveColour;
                mHudTextPlayer[(player + 1) % 2].BkColour = mHudInactiveColour;
            }
            else
            {
                // Both inactive
                mHudTextPlayer[0].BkColour = mHudInactiveColour;
                mHudTextPlayer[1].BkColour = mHudInactiveColour;
                mHudTextParams.Text = "";
            }

            // Force redraw
            ForceRedraw = true;
        }

        // Set player score
        public void SetPlayerScore(int player, int score)
        {
            // Update player texts
            if (player == 0)
                mHudTextPlayer[0].Text = "PLAYER 1       " + score.ToString();
            else
                mHudTextPlayer[1].Text = score.ToString() + "       PLAYER 2";

            // Force redraw
            ForceRedraw = true;
        }

        // Set throw params
        public void SetThrowParams(int speed, int hangle, int vangle)
        {
            // Update params text
            mHudTextParams.Text =
                "SPEED:  " + speed.ToString() + " m/s      " +
                "ANGLE:  " + hangle.ToString() + "°H  " + vangle.ToString() + "°V";

            // Force redraw
            ForceRedraw = true;
        }


        ///////////////////////////////////////////////////////////////////////
        // IRenderTarget interface methods

        // Engine load handler
        public void OnLoad()
        {
           // At launch go to main menu screen
           mGame.State = GorillasGame.GameState.Main_Menu;
        }

        // Window resize handler
        public void OnResize()
        {
            // Resize UI
            base.Resize(mGame.ViewportWidth, mGame.ViewportHeight);

            // Reposition controls
            UpdateControls();
        }

        // Frame update handler
        public void OnUpdate()
        {
            // Nothing to do here really unless we add some UI animations later on
        }

        // Frame render handler
        public void OnRender()
        {
            // Let the base class do all the hard work
            base.Render();
        }

        // Mouse down handler
        public bool OnMouseDown(MouseButtonEventArgs args)
        {
            // Handle UI button clicks
            int elementID = base.HitTest(args.Position.X, args.Position.Y);
            if (elementID >= 0)
            {
                mGame.PlaySound("Click.wav");

                switch ((UIElement)elementID)
                {
                case UIElement.MainMenu_Button_NewGame:

                    // Start new game
                    mGame.State = GorillasGame.GameState.Play_NewGame;
                    break;

                case UIElement.MainMenu_Button_Controls:

                    // Show controls dialog
                    mGame.State = GorillasGame.GameState.Dialog_Controls;
                    break;

                case UIElement.MainMenu_Button_About:

                    // Show about dialog
                    mGame.State = GorillasGame.GameState.Dialog_About;
                    break;

                case UIElement.MainMenu_Button_Exit:

                    // Quit the app
                    mGame.Quit();
                    break;

                case UIElement.ModalDialog_Button_Back:

                    // Show main menu
                    mGame.State = GorillasGame.GameState.Main_Menu;
                    break;
                }

                return true;
            }

            return false;
        }

        // Mouse move handler
        public bool OnMouseMove(MouseMoveEventArgs args)
        {
            return false;
        }

        // Mouse up handler
        public bool OnMouseUp(MouseButtonEventArgs args)
        {
            return false;
        }

        // Mouse wheel handler
        public bool OnMouseWheel(MouseWheelEventArgs args)
        {
            return false;
        }

        // Key down handler
        public bool OnKeyDown(KeyboardKeyEventArgs args)
        {
            if ((mGame.State == GorillasGame.GameState.Dialog_Controls) ||
                (mGame.State == GorillasGame.GameState.Dialog_About))
            {
                // Trap ESC key
                if (args.Key == Key.Escape)
        {
                    // Jump back to main menu
                    mGame.State    = GorillasGame.GameState.Main_Menu;
                    return true;
                }
            }

            return false;
        }

        // Key up handler
        public bool OnKeyUp(KeyboardKeyEventArgs args)
        {
            return false;
        }


        ///////////////////////////////////////////////////////////////////////
        // Private methods

        // Update UI controls
        void UpdateControls()
        {
            // Text size scaling. TBD: find a way of reading system DPI setting
            // and use it to work out appropriate text scale factor.
            const float textScaleFactor = 0.5f;

            // A flag indicating we need to resize HUD UI
            bool hudResizeRequred = false;

            // Remove old UI elements when switching between screens
            if (mGame.State <= GorillasGame.GameState.Play_NewGame)
                base.RemoveAllElements();

            // Add new UI elements depending on current screen
            switch (mGame.State)
            {
            case GorillasGame.GameState.Main_Menu:
                {
                    // Show main menu

                    // Metrics
                    const int menuTitleY = 30;
                    const int menuTitleHeight = 70;
                    const int menuTitleFontSize = (int)(textScaleFactor * 32);
                    const int menuItemWidth = 300;
                    const int menuItemHeight = 70;
                    const int menuItemSpacing = 30;
                    const int menuItemFontSize = (int)(textScaleFactor * 24);
                    const int menuItemCount = 4;
                    const int menuBorderSpacing = 60;
                    const int menuWidth = menuItemWidth + menuBorderSpacing * 2;
                    const int menuHeight = menuItemCount * menuItemHeight + (menuItemCount - 1) * menuItemSpacing + menuBorderSpacing * 2;
                    int menuX = mGame.ViewportWidth / 2 - menuWidth / 2;
                    int menuY = mGame.ViewportHeight / 2 - menuHeight / 2;

                    // Add title element
                    UI.TextElement textElement = new UI.TextElement((int)UIElement.UI_Title, Color.Black,
                        "3D Gorillas", menuTitleFontSize, StringAlignment.Center, Color.White);
                    textElement.Rect = new Rectangle(0, menuTitleY, mGame.ViewportWidth, menuTitleHeight);
                    base.AddElement(textElement);

                    // Add menu background
                    UI.BasicElement basicElement = new UI.BasicElement((int)UIElement.UI_Background, Color.Aqua);
                    basicElement.Rect = new Rectangle(menuX, menuY, menuWidth, menuHeight);
                    base.AddElement(basicElement);

                    // Add menu items
                    int itemY = menuY + menuBorderSpacing + (menuItemHeight + menuItemSpacing) * 0;
                    UI.ButtonElement buttonElement = new UI.ButtonElement((int)UIElement.MainMenu_Button_NewGame, Color.Blue,
                        "Play", menuItemFontSize, StringAlignment.Center, Color.Yellow);
                    buttonElement.Rect = new Rectangle(menuX + menuBorderSpacing, itemY, menuItemWidth, menuItemHeight);
                    base.AddElement(buttonElement);

                    itemY = menuY + menuBorderSpacing + (menuItemHeight + menuItemSpacing) * 1;
                    buttonElement = new UI.ButtonElement((int)UIElement.MainMenu_Button_Controls, Color.Blue,
                        "Controls", menuItemFontSize, StringAlignment.Center, Color.Yellow);
                    buttonElement.Rect = new Rectangle(menuX + menuBorderSpacing, itemY, menuItemWidth, menuItemHeight);
                    base.AddElement(buttonElement);

                    itemY = menuY + menuBorderSpacing + (menuItemHeight + menuItemSpacing) * 2;
                    buttonElement = new UI.ButtonElement((int)UIElement.MainMenu_Button_About, Color.Blue,
                        "About", menuItemFontSize, StringAlignment.Center, Color.Yellow);
                    buttonElement.Rect = new Rectangle(menuX + menuBorderSpacing, itemY, menuItemWidth, menuItemHeight);
                    base.AddElement(buttonElement);

                    itemY = menuY + menuBorderSpacing + (menuItemHeight + menuItemSpacing) * 3;
                    buttonElement = new UI.ButtonElement((int)UIElement.MainMenu_Button_Exit, Color.Blue,
                        "Exit", menuItemFontSize, StringAlignment.Center, Color.Yellow);
                    buttonElement.Rect = new Rectangle(menuX + menuBorderSpacing, itemY, menuItemWidth, menuItemHeight);
                    base.AddElement(buttonElement);
                }

                break;

            case GorillasGame.GameState.Dialog_Controls:
            case GorillasGame.GameState.Dialog_About:
                {
                    // Show modal dialogs

                    // Metrics
                    const int dialogTitleY = 30;
                    const int dialogTitleHeight = 70;
                    const int dialogTitleFontSize = (int)(textScaleFactor * 32);
                    const int dialogContentWidth = 640;
                    const int dialogContentHeight = 640;
                    const int dialogContentMargin = 80;
                    const int dialogContentFontSize = (int)(textScaleFactor * 20);
                    const int dialogButtonWidth = 200;
                    const int dialogButtonHeight = 70;
                    const int buttonFontSize = (int)(textScaleFactor * 24);
                    const int dialogWidth = dialogContentWidth + dialogContentMargin * 2;
                    const int dialogHeight = dialogContentHeight + dialogContentMargin * 2;
                    int dialogX = mGame.ViewportWidth / 2 - dialogWidth / 2;
                    int dialogY = mGame.ViewportHeight / 2 - dialogHeight / 2;

                    // Title and content differ specific to dialog
                    string title, content;
                    if (mGame.State == GorillasGame.GameState.Dialog_Controls)
                    {
                        title = "Controls Guide";
                        content = "MOUSE\n\n" +
                                  "[ Wheel ]  -  Adjust throw speed\n" +
                                  "[ Left drag ]  -  Adjust throw angle\n" +
                                  "[ Right click ]  -  Throw banana\n\n" +
                                  "KEYBOARD\n\n" +
                                  "[ PgUp, PgDn ]  -  Adjust throw speed\n" +
                                  "[ Left, Right, Up, Down ]  -  Adjust throw angle\n" +
                                  "[ Shift + Arrows ]  -  Fine tune throw angle\n" +
                                  "[ Enter ]  -  Throw banana\n" +
                                  "[ Esc ]  -  Exit to Main Menu";
                    }
                    else
                    {
                        title = "About Gorillas 3D";
                        content = "Submitted for the BSc in Computer Science for Games Development\n\n" +
                                  "Project code: SG7 (3D Gorillas)\n\n" +
                                  "By Denis Volosin";
                    }

                    // Add title element
                    UI.TextElement textElement = new UI.TextElement((int)UIElement.UI_Title, Color.Black,
                        title, dialogTitleFontSize, StringAlignment.Center, Color.White);
                    textElement.Rect = new Rectangle(0, dialogTitleY, mGame.ViewportWidth, dialogTitleHeight);
                    base.AddElement(textElement);

                    // Add dialog background
                    UI.BasicElement basicElement = new UI.BasicElement((int)UIElement.UI_Background, Color.Aqua);
                    basicElement.Rect = new Rectangle(dialogX, dialogY, dialogWidth, dialogHeight);
                    base.AddElement(basicElement);

                    // Add dialog content
                    textElement = new UI.TextElement((int)UIElement.ModalDialog_Content, Color.Aqua,
                        content, dialogContentFontSize, StringAlignment.Near, Color.Black);
                    textElement.Rect = new Rectangle(dialogX + dialogContentMargin, dialogY + dialogContentMargin,
                        dialogContentWidth, dialogContentHeight);
                    base.AddElement(textElement);

                    // Add back button
                    int buttonX = mGame.ViewportWidth / 2 - dialogButtonWidth / 2;
                    int buttonY = dialogY + dialogHeight - dialogContentMargin - dialogButtonHeight;
                    UI.ButtonElement buttonElement = new UI.ButtonElement((int)UIElement.ModalDialog_Button_Back, Color.Blue,
                        "Back", buttonFontSize, StringAlignment.Center, Color.Yellow);
                    buttonElement.Rect = new Rectangle(buttonX, buttonY, dialogButtonWidth, dialogButtonHeight);
                    base.AddElement(buttonElement);
                }

                break;

            case GorillasGame.GameState.Play_NewGame:
                {
                    // Show in-game HUD
                    const int hudParamsFontSize = (int)(textScaleFactor * 24);
                    const int hudPlayerFontSize = (int)(textScaleFactor * 32);
                    
                    // Params text
                    mHudTextParams = new UI.TextElement((int)UIElement.HUD_Text_Params,
                        mHudInactiveColour, "", hudParamsFontSize, StringAlignment.Center, Color.Black);
                    base.AddElement(mHudTextParams);

                    // Player 1 text
                    mHudTextPlayer[0] = new UI.TextElement((int)UIElement.HUD_Text_Player1,
                        mHudInactiveColour, "PLAYER 1", hudPlayerFontSize, StringAlignment.Center, Color.Black);
                    base.AddElement(mHudTextPlayer[0]);
                    //SetPlayerScore(0, 0);

                    // Player 2 text
                    mHudTextPlayer[1] = new UI.TextElement((int)UIElement.HUD_Text_Player2,
                        mHudInactiveColour, "PLAYER 2", hudPlayerFontSize, StringAlignment.Center, Color.Black);
                    base.AddElement(mHudTextPlayer[1]);
                    //SetPlayerScore(1, 0);

                    // Force HUD resize
                    hudResizeRequred = true;
                }

                break;

            case GorillasGame.GameState.Play_LevelGeneration:
            case GorillasGame.GameState.Play_PlayerSelection:
            case GorillasGame.GameState.Play_ProjectileParameters:
            case GorillasGame.GameState.Play_ProjectileFlight:
            case GorillasGame.GameState.Play_ProjectileExplosion:
            case GorillasGame.GameState.Play_BuildingCollapse:

                // Force HUD resize
                hudResizeRequred = true;
                break;
            }
        
            // Resize in-game HUD if needed
            if (hudResizeRequred)
            {
                // Metrics
                const int hudMargin = 20;
                const int hudPlayerWidth = 400;
                const int hudHeightHeight = 100;

                // Params text
                mHudTextParams.Rect = new Rectangle(hudMargin + hudPlayerWidth - 1, hudMargin,
                    mGame.ViewportWidth - hudPlayerWidth* 2 - hudMargin* 2 + 2, hudHeightHeight / 2);

                // Player 1 text
                mHudTextPlayer[0].Rect = new Rectangle(hudMargin, hudMargin,
                    hudPlayerWidth, hudHeightHeight);

                // Player 2 text
                mHudTextPlayer[1].Rect = new Rectangle(mGame.ViewportWidth - hudMargin - hudPlayerWidth, hudMargin,
                    hudPlayerWidth, hudHeightHeight);
            }
        }
    }
}
