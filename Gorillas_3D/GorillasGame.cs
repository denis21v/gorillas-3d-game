////////////////////////////////////////////////////////////////////////////////
//                                                                            //
//      Submitted for the BSc in Computer Science for Games Development       //
//      Project code: SG7 (3D Gorillas)                                       //
//      By Denis Volosin                                                      //
//                                                                            //
//      GorillasGame.cs                                                       //
//                                                                            //
//      Gorillas game class                                                   //
//                                                                            //
////////////////////////////////////////////////////////////////////////////////

using System;
using GameEngine_3D;

namespace Gorillas_3D
{
    ///////////////////////////////////////////////////////////////////////////
    // GorillasGame class

    public class GorillasGame : GameEngine
    {
        ///////////////////////////////////////////////////////////////////////
        // Public class types

        // Game states
        public enum GameState
        {
            Main_Menu,                 // Show main menu
            Dialog_Controls,           // Show game controls information modal dialog
            Dialog_About,              // Show 'about' information modal dialog
            Play_NewGame,              // Start new game
            Play_LevelGeneration,      // Animated random level generation
            Play_PlayerSelection,      // Animated camera player selection
            Play_ProjectileParameters, // Player enters projectile trajectory parameters (angles, speed)
            Play_ProjectileFlight,     // Projectile flying towards opponent's gorilla
            Play_ProjectileExplosion,  // Animated projectile explosion on hit
            Play_BuildingCollapse      // Animated building collapse
        }


        ///////////////////////////////////////////////////////////////////////
        // Private class data

        GameState mState;       // Current game state
        double mStateEnterTime; // Time stamp of entering the current game state (seconds)
        GorillasScene mScene;   // Game scene
        GorillasUI mUI;         // Game UI



        ///////////////////////////////////////////////////////////////////////
        // Constructors

        // Default constructor
        public GorillasGame() :
           base(1024, 768, "3D Gorillas by Denis Volosin", true)
        {
            // Create game scene
            mScene = new GorillasScene(this);
            AddRenderTarget(mScene);

            // Create UI
            mUI = new GorillasUI(this);
            AddRenderTarget(mUI);
        }


        ///////////////////////////////////////////////////////////////////////
        // Properties

        // Access game state
        public GameState State
        {
            get { return mState; }
            set
            {
                mState = value;
                mStateEnterTime = GameTime;

                switch (mState)
                {
                case GameState.Main_Menu:

                    // Show main menu
                    mUI.ShowMainMenu();
                    break;

                case GameState.Dialog_Controls:

                    // Show game controls information modal dialog
                    mUI.ShowControlsDialog();
                    break;

                // Show 'about' information modal dialog
                case GameState.Dialog_About:

                    // Show about dialog
                    mUI.ShowAboutDialog();
                    break;

                case GameState.Play_NewGame:

					// Show game HUD
					mUI.ShowGameHUD();

                    // Start new game
                    mScene.StartGame();
                    break;
                }
            }
        }

        // Access game state enter time
        public double StateEnterTime
        {
            // Read-only
            get { return mStateEnterTime; }
        }

        // Access game state elapsed time
        public float StateElapsedTime
        {
            // Read-only
            get { return (float)(GameTime - mStateEnterTime); }
        }

        // Access game UI
        public GorillasUI UI
        {
            // Read-only
            get { return mUI; }
        }
    }
}
