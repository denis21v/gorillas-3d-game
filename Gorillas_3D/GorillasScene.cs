////////////////////////////////////////////////////////////////////////////////
//                                                                            //
//      Submitted for the BSc in Computer Science for Games Development       //
//      Project code: SG7 (3D Gorillas)                                       //
//      By Denis Volosin                                                      //
//                                                                            //
//      GorillasScene.cs                                                      //
//                                                                            //
//      Gorillas game main scene                                              //
//                                                                            //
////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Input;
using GameEngine_3D;

namespace Gorillas_3D
{
    ///////////////////////////////////////////////////////////////////////////
    // Scene class

    public class GorillasScene: Scene, IRenderTarget
    {
        ///////////////////////////////////////////////////////////////////////
        // Private class types


        ///////////////////////////////////////////////////////////////////////
        // Player object
        class Player
        {
            public int Index;                 // Player index 0 or 1
            public int Score;                 // Hit score
            public int LaunchSpeed;           // Projectile launch speed (m/s)
            public int LaunchVerticalAngle;   // Projectile vertical launch angle
            public int LaunchHorizontalAngle; // Projectile horizontal launch angle
            public Vector3 LaunchDirection;   // Projectile launch direction vector
            public Vector3 LaunchPosition;    // Projectile launch position
            public IVector3 Position;         // Player's gorilla position
            public ModelNode Gorilla;         // Player's gorilla model node
            public Camera Camera;             // Player camera

            public Player(int index)
            {
                // We only want to know player index at creation time
                Index = index;
            }
        }


        ///////////////////////////////////////////////////////////////////////
        // Building object

        class Building
        {
            public int BuildingHeight;    // Building height (number of floors)
            public Node BuildingNode;     // Reference to building node
            public List<Node> FloorNodes; // References to floor nodes
            public Player Player;         // Player positioned on top of this building or null

            public Building()
            {
                BuildingHeight = 0;
                BuildingNode = null;
                FloorNodes = new List<Node>();
                Player = null;
            }
        }


        ///////////////////////////////////////////////////////////////////////
        // Hit test result object

        class HitTestResult
        {
            public enum HitType
            {
                Ground,    // Hit ground
                Building,  // Hit building
                Player     // Hit player
            };

            // Constructor (ground)
            public HitTestResult()
            {
                this.Type = HitType.Ground;
                this.Building = null;
                this.Player = null;
            }

            // Constructor (building)
            public HitTestResult(IVector3 tile, Building building)
            {
                this.Type = HitType.Building;
                this.Tile = tile;
                this.Building = building;
                this.Player = null;
            }

            // Constructor (player)
            public HitTestResult(IVector3 tile, Player player)
            {
                this.Type = HitType.Player;
                this.Tile = tile;
                this.Building = null;
                this.Player = player;
            }

            public HitType Type;      // Hit type
            public IVector3 Tile;     // Tile position
            public Building Building; // Hit building (HitType.Building only)
            public Player Player;     // Hit building (HitType.Player only)
        }

        ///////////////////////////////////////////////////////////////////////
        // Explosion particle object

        class ExplosionParticle
        {
            public Node ParticleNode;         // Reference to particle node
            public Vector3 ParticleDirection; // Direction of move
            public float ParticleSpeed;       // Move speed
            public float ParticleScale;       // Scale
        }


        ///////////////////////////////////////////////////////////////////////
        // Private class data

        // Constants
        const int LEVEL_MAP_MIN_SIZE                     = 7;
        const int LEVEL_MAP_MAX_SIZE                     = 15;
        const int LEVEL_MAP_BORDER_SIZE                  = 1;
        const int LEVEL_BUILDING_MIN_HEIGHT              = 3;
        const int LEVEL_BUILDING_MAX_HEIGHT              = 10;
        const int LEVEL_BUILDING_MAX_SKINS               = 6;
        const float LEVEL_GENERATION_ANIMATION_DURATION  = 5.0f;  // 5 s
        const float PLAYER_SELECTION_ANIMATION_DURATION  = 4.0f;  // 4 s
        const float PLAYER_SELECTION_BLINK_DURATION      = 0.2f;  // .2 s
        const float PLAYER_SELECTION_BLINK_COUNT         = 9;
        const int PROJECTILE_SPEED_MIN                   = 10;   // 10 m/s
        const int PROJECTILE_SPEED_MAX                   = 40;   // 40 m/s
        const int PROJECTILE_VANGLE_MIN                  = -85;  // -85 deg
        const int PROJECTILE_VANGLE_MAX                  = 85;   // +85 deg
        const float PROJECTILE_CAMERA_DISTANCE           = 0.3f;
        const int WIND_SPEED_MIN                         = 5;   // 5 m/s
        const int WIND_SPEED_MAX                         = 30;  // 30 m/s
        const int EXPLOSION_MAX_PARTICLES                = 64;
        const float EXPLOSION_MIN_PARTICLE_SPEED         = 0.5f;
        const float EXPLOSION_MAX_PARTICLE_SPEED         = 2.5f;
        const float EXPLOSION_MIN_PARTICLE_SCALE         = 0.5f;
        const float EXPLOSION_MAX_PARTICLE_SCALE         = 0.5f;
        const float EXPLOSION_ANIMATION_DURATION         = 0.5f; // .5 s
        const float BUILDING_COLLAPSE_ANIMATION_DURATION = 0.2f; // .2 s
        const float PHYSICS_GRAVITY                      = -9.8f; // Earth's gravity of 9.8 m/s
        const float PHYSICS_TILE_SCALE                   = 0.2f;  // 1 tile = ~5m

        // Game data
        GorillasGame mGame;                          // Reference to Gorillas game object
        Random mRandom;                              // Random object for random number generation
        Camera mRoamingCamera;                       // Roaming camera
        Vector3 mCameraAnimationStartPosition;       // Camera animation starting position
        Vector3 mCameraAnimationStartLookAt;         // Camera animation starting look-at target
        Vector3 mCameraAnimationStopPosition;        // Camera animation ending position
        Vector3 mCameraAnimationStopLookAt;          // Camera animation ending look-at target
        float mCameraAnimationDuration;              // Camera animation duration (s)
        int mWindAngle;                              // Wind horizontal angle (degrees)
        int mWindSpeed;                              // Wind speed (m/s)
        Vector3 mWindDirection;                      // Wind direction
        float mProjectileFlightTime;                 // Expected projectile flight time
        HitTestResult mProjectileHit;                // Projectile hit test result
        bool mProjectileMouseControl;                // Projectile mouse control active
        Player[] mPlayers;                           // Player array
        Player mActivePlayer;                        // Active player
        Player mInactivePlayer;                      // Inactive player
        int mPlayerSelectionBlink;                   // Player selection blinking state
        int mNumberOfTurns;                          // Number of turns
        IVector3 mLevelSize;                         // Level dimensions (in number of tiles)
        Vector3 mLevelBoundary;                      // Level boundary in world space
        Building[,] mLevelMap;                       // A 2D array of Building objects
        Node mLevelMapNode;                          // Reference to level map node
        Node mLevelGroundNode;                       // Reference to level ground node
        Node mLevelPlayersNode;                      // Reference to level players node
        Node mProjectileNode;                        // Reference to projectile node
        Node mExplosionNode;                         // Reference to explosion node
        List<ExplosionParticle> mExplosionParticles; // Explosion particles
        Geometry mBuildingGeometry;                  // Building floor geometry
        Geometry mQuadGeometry;                      // Generic quad geometry



        ///////////////////////////////////////////////////////////////////////
        // Constructors

        // Standard scene constructor
        public GorillasScene(GorillasGame game) :
            base()
        {
            // Invisible by default
            this.Visible = false;

            // Save reference to game object as we'll need it later
            mGame = game;

            // Seed random generator with current time so we get new numbers
            // every time we start the game
            int seed = (DateTime.Now.Hour + 1) * (DateTime.Now.Minute + 1) *
               (DateTime.Now.Second + 1) * (DateTime.Now.Millisecond + 1);
            mRandom = new Random(seed);
        }


        ///////////////////////////////////////////////////////////////////////
        // Gameplay control methods

        // Start game
        public void StartGame()
        {
            // Zero out player scores
            foreach (Player player in mPlayers)
                player.Score = 0;

            // Always start a new game with second player as active. The players
            // get swapped on the very first turn so we end up with correct player
            // serving at the beginning of the game.
            mInactivePlayer = mPlayers[0];
            mActivePlayer = mPlayers[1];

            // Make ourselves visible
            this.Visible = true;

            // Stop new level
            StartNextLevel();
        }

        // Start game
        public void StopGame()
        {
            // Abort any active sounds
            mGame.StopAllSounds();

            // Hide game scene
            this.Visible = false;

            // Jump back to main menu
            mGame.State = GorillasGame.GameState.Main_Menu;
        }

        // Start next level
        void StartNextLevel()
        {
            // Wipe the old level map scene content
            mLevelMapNode.RemoveAllChildren();
            mLevelGroundNode.RemoveAllChildren();

            // Find out level map dimensions. This could be either random or
            // hardcoded. Potentially we can also increse the map size after
            // every new game to make it gradually more challenging.
            #if false

            // Generate random map dimensions
            mLevelSize.X = mRandom.Next(LEVEL_MAP_MIN_SIZE, LEVEL_MAP_MAX_SIZE + 1);
            mLevelSize.Z = mRandom.Next(LEVEL_MAP_MIN_SIZE, LEVEL_MAP_MAX_SIZE + 1);

            #else

            // Use hardcoded map dimensions
            mLevelSize.X = LEVEL_MAP_MAX_SIZE;
            mLevelSize.Z = LEVEL_MAP_MAX_SIZE;

            #endif

            // Round the dimensions to multiples of 3 as the street layout has
            // a fixed pattern of a chessboard:
            //
            //  ##    ##    ##
            //
            //  ##    ##    ##
            //
            //  ##    ##    ##
            //

            if ((mLevelSize.X % 2) == 0)
                mLevelSize.X ++;
            if ((mLevelSize.Z % 2) == 0)
                mLevelSize.Z ++;

            // Level height determined below after we know the max building height
            mLevelSize.Y = 0;

            // Adjust level boundary
            mLevelBoundary.X = -(0.5f * mLevelSize.X);
            mLevelBoundary.Y = 0;
            mLevelBoundary.Z = -(0.5f * mLevelSize.Z);

            // Generate random player positions. Player positions can not be very close
            // otherwise it is not fun! We want them as far as 70% of the map size apart.
            float minPlayerDistance = 0.7f * Math.Max(mLevelSize.X, mLevelSize.Z);

            do
            {
                foreach (Player player in mPlayers)
                {
                    // Random Position.X
                    do
                    {
                        player.Position.X = mRandom.Next(0, mLevelSize.X);
                    }
                    while (((player.Position.X % 2) != 0) || ((player.Position.X > 4) && ((player.Position.X < (mLevelSize.X - 5)))));

                    // Random Position.Z
                    do
                    {
                        player.Position.Z = mRandom.Next(0, mLevelSize.Z);
                    }
                    while (((player.Position.Z % 2) != 0) || ((player.Position.Z > 4) && ((player.Position.Z < (mLevelSize.Z - 5)))));
                }
            }
            while (Utility.GetDistance2D(mPlayers[0].Position, mPlayers[1].Position) < minPlayerDistance);

            // Random player height, launch speed and orientation
            Vector3 playersCenter = Vector3.Zero;
            foreach (Player player in mPlayers)
            {
                player.Gorilla.Visible = false;
                player.Position.Y = mRandom.Next(LEVEL_BUILDING_MIN_HEIGHT, LEVEL_BUILDING_MAX_HEIGHT + 1);
                player.Gorilla.Transform.Translation = TilePositionToScenePosition(player.Position);
                player.LaunchSpeed = PROJECTILE_SPEED_MIN + (PROJECTILE_SPEED_MAX - PROJECTILE_SPEED_MIN) / 2;
                player.LaunchVerticalAngle = 0;
                playersCenter += player.Gorilla.Transform.Translation;
            }

            // Rotate players to face each other at a random +/-35 deg angle so that
            // the shot direction is not immediately obvious at the beginng of the game
            playersCenter *= 0.5f;
            int angleOZ = 90 - (int)Utility.PointOnCircleGetAngle2D(playersCenter.X, playersCenter.Z,
                mPlayers[0].Gorilla.Transform.Translation.X, mPlayers[0].Gorilla.Transform.Translation.Z);

            // Rotate player 0
            mPlayers[0].LaunchHorizontalAngle = angleOZ + mRandom.Next(-35, 35);
            Utility.NormalizeAngle(ref mPlayers[0].LaunchHorizontalAngle);
            Vector3 leveledOpponentPosition = new Vector3(mPlayers[1].Gorilla.Transform.Translation.X,
                mPlayers[0].Gorilla.Transform.Translation.Y, mPlayers[1].Gorilla.Transform.Translation.Z);

            // Rotate player 1
            mPlayers[1].LaunchHorizontalAngle = angleOZ + 180 + mRandom.Next(-35, 35);
            Utility.NormalizeAngle(ref mPlayers[1].LaunchHorizontalAngle);
            leveledOpponentPosition = new Vector3(mPlayers[0].Gorilla.Transform.Translation.X,
                mPlayers[1].Gorilla.Transform.Translation.Y, mPlayers[0].Gorilla.Transform.Translation.Z);

            // Preload ground textures
            Texture groundEmtyTexture = AssetManager.LoadTexture("GroundEmpty.png");
            Texture groundCornerTexture = AssetManager.LoadTexture("GroundCorner.png");
            Texture groundJunctionTexture = AssetManager.LoadTexture("GroundJunction.png");
            Texture groundJunctionTTexture = AssetManager.LoadTexture("GroundJunctionT.png");
            Texture groundStreetTexture = AssetManager.LoadTexture("GroundStreet.png");

            // Add ground node
            ModelNode groundTileNode = new ModelNode("Ground_Empty", mQuadGeometry, groundEmtyTexture);
            groundTileNode.Transform.ScaleTo(mLevelSize.X + 4, 1, mLevelSize.Z + 4);
            groundTileNode.Transform.TranslateTo(0, -0.1f, 0);
            groundTileNode.UseLighting = false;
            mLevelGroundNode.AddChild(groundTileNode);

            // Allocate level buildings array
            mLevelMap = new Building[mLevelSize.X, mLevelSize.Z];

            // Populate level scene
            for (int x = -LEVEL_MAP_BORDER_SIZE; x < (mLevelSize.X + LEVEL_MAP_BORDER_SIZE); x ++)
            {
                for (int z = -LEVEL_MAP_BORDER_SIZE; z < (mLevelSize.Z + LEVEL_MAP_BORDER_SIZE); z ++)
                {
                    if (IsBuildingLevelTile(x, z))
                    {
                        // Building tile
                        // Alloc new building object
                        Building building = new Building();

                        // Limit building heights adjacent to player positions to reasonable height
                        int maxBuildingHeight = LEVEL_BUILDING_MAX_HEIGHT;
                        foreach (Player player in mPlayers)
                        {
                            if (Utility.GetDistance2D(x, z, player.Position.X, player.Position.Z) < 3.0f)
                                maxBuildingHeight = Math.Min(maxBuildingHeight, player.Position.Y);
                        }

                        // Random building height (numebr of floors)
                        building.BuildingHeight = mRandom.Next(LEVEL_BUILDING_MIN_HEIGHT, maxBuildingHeight);

                        // Update level height based on tallest building
                        if (mLevelSize.Y < building.BuildingHeight)
                            mLevelSize.Y = building.BuildingHeight;

                        // Add building node
                        building.BuildingNode = new Node("Building_X" + x + "_Z" + z);

                        // Move to correct spot in the scene
                        building.BuildingNode.Transform.TranslateTo(
                            mLevelBoundary.X + x + 0.5f,
                            mLevelBoundary.Y + 0.5f,
                            mLevelBoundary.Z + z + 0.5f);

                        // Add to scene
                        mLevelMapNode.AddChild(building.BuildingNode);

                        // Random building texture
                        int buildingSkinIndex = mRandom.Next(0, LEVEL_BUILDING_MAX_SKINS);
                        Texture buildingTexture = AssetManager.LoadTexture("Building" + buildingSkinIndex + ".png");

                        // Populate building floors. These are created as separate scene nodes
                        // as they can be individually destroyed by projectiles.
                        for (int y = 0; y < building.BuildingHeight; y ++)
                        {
                            // Create new model node for the building floor
                            ModelNode floorNode = new ModelNode(
                                "Floor_X" + x + "_Z" + z + "_Y" + y,
                                mBuildingGeometry, buildingTexture);
                            floorNode.Visible = false;

                            // Move to correct spot in the scene
                            floorNode.Transform.TranslateYTo(y);

                            // Add to building node
                            building.BuildingNode.AddChild(floorNode);
                            building.FloorNodes.Add(floorNode);
                        }

                        // Also add to level map for quick lookups
                        mLevelMap[x, z] = building;
                    }
                    else
                    {
                        // Ground tile
                        Texture groundTileTexture = null;
                        int groundTileRotation = 0;
                        bool tJunction = false;

                        if (IsCornerLevelTile(x, z, ref groundTileRotation))
                        {
                            // Corner tile
                            groundTileTexture = groundCornerTexture;
                        }
                        else if (IsStreetLevelTile(x, z, ref groundTileRotation))
                        {
                            // Street tile
                            groundTileTexture = groundStreetTexture;
                        }
                        else if (IsJunctoionLevelTile(x, z, ref groundTileRotation, ref tJunction))
                        {
                            // Junction tile
                            groundTileTexture = tJunction ? groundJunctionTTexture : groundJunctionTexture;
                        }

                        if (groundTileTexture != null)
                        {
                            groundTileNode = new ModelNode("Ground_X" + x + "_Z" + z,
                                mQuadGeometry, groundTileTexture);

                            // Move to correct spot in the scene
                            groundTileNode.Transform.TranslateTo(
                                mLevelBoundary.X + x + 0.5f,
                                mLevelBoundary.Y,
                                mLevelBoundary.Z + z + 0.5f);

                            // Rotate if needed
                            if (groundTileRotation != 0)
                                groundTileNode.Transform.RotateYBy(groundTileRotation);

                            // Add to ground node
                            mLevelGroundNode.AddChild(groundTileNode);
                        }
                    }
                }
            }

            // Generate random wind parameters
            mWindAngle = mRandom.Next(0, 360);
            mWindSpeed = 0; //mRandom.Next(WIND_SPEED_MIN, WIND_SPEED_MAX + 1);
            mWindDirection = Utility.RotateVector(Vector3.UnitX, 0, mWindAngle, 0);

            // Reset game state
            this.Camera = mRoamingCamera;
            mProjectileNode.Visible = false;
            mProjectileMouseControl = false;
            mPlayerSelectionBlink = 0;
            mNumberOfTurns = 0;

            // Update player positions
            foreach (Player player in mPlayers)
            {
                mLevelMap[player.Position.X, player.Position.Z].Player = player;
                UpdatePlayer(player);
            }

            // Play construction sound
            mGame.PlaySound("Rising.wav");

            // Same as original game, animate level generation by rising buildings
            // from the ground
            mGame.State = GorillasGame.GameState.Play_LevelGeneration;
        }

        // Start next turn
        void StartNextTurn()
        {
            if (mNumberOfTurns < 3)
            {
                // Animate from previous orbiting camera position
                mCameraAnimationStartPosition = mRoamingCamera.Position;
                mCameraAnimationStartLookAt = mRoamingCamera.LookAt;
                mPlayerSelectionBlink = 0;
            }
            else
            {
                // Animate from active player camera
                mCameraAnimationStartPosition = mActivePlayer.Camera.Position;
                mCameraAnimationStartLookAt = mActivePlayer.Camera.LookAt;
            }

            if (mNumberOfTurns < 2)
            {
                 // Animate to slightly behind the inactive player's gorilla
                 // (player position preview animation at the beginning of new game)
                mCameraAnimationStopPosition = mInactivePlayer.Camera.Position -
                    mInactivePlayer.LaunchDirection + Vector3.UnitY * 2;
                mCameraAnimationStopLookAt = mInactivePlayer.Camera.Position;
            }
            else
            {
                // Animate to inactive player camera
                mCameraAnimationStopPosition = mInactivePlayer.Camera.Position;
                mCameraAnimationStopLookAt = mInactivePlayer.Camera.LookAt;
            }

            if (mNumberOfTurns == 0)
            {
                // Half the full animation cycle as we only need to descend
                mCameraAnimationDuration = PLAYER_SELECTION_ANIMATION_DURATION * .5f;

                // Play short transition sound
                mGame.PlaySound("TransitionShort.wav");
            }
            else
            {
                // Full animation cycle as we need to both rise and descend
                mCameraAnimationDuration = PLAYER_SELECTION_ANIMATION_DURATION;

                // Play long transition sound
                mGame.PlaySound("TransitionLong.wav");
            }

            mNumberOfTurns ++;

            // Flip active player
            mInactivePlayer = mActivePlayer;
            mActivePlayer = mPlayers[(mActivePlayer.Index + 1) % 2];
            mGame.UI.ShowPlayerText(mActivePlayer.Index, true);
            mGame.UI.ShowPlayerText(mInactivePlayer.Index, true);
            mGame.UI.SetActivePlayer(-1);

            // Use roaming camera for animation
            this.Camera = mRoamingCamera;

            // Keep projectile invisible
            mProjectileNode.Visible = false;

            // Cancel any pending mouse input
            mProjectileMouseControl = false;

            // Enter player selection state
            mGame.State = GorillasGame.GameState.Play_PlayerSelection;
        }

        // Launch projectile
        void LaunchProjectile()
        {
            // First we must find out the total projectile in-flight time
            // in order to correctly animate in-flight camera.
            Vector3 projectilePosition;
            mProjectileFlightTime = 0;
            while (true)
            {
                mProjectileFlightTime += 0.01f;
                projectilePosition = SimulateProjectilePhysics(mProjectileFlightTime);
                if (HitTest(projectilePosition) != null)
                    break;
            }

            // Back to roaming camera
            mRoamingCamera.Position = mActivePlayer.Camera.Position;
            mRoamingCamera.LookAt = mActivePlayer.Camera.LookAt;
            this.Camera = mRoamingCamera;

            // Cancel any pending mouse input
            mProjectileMouseControl = false;

            // Play throw woosh sound
            mGame.PlaySound("Throw.wav");

            // Enter projectile selection state
            mGame.State = GorillasGame.GameState.Play_ProjectileFlight;
        }

        // Start explosion animation
        void StartExplosion(Vector3 explosionPostion, bool groundHit)
        {
            // Generate random particle direction, speed and size
            float angleXMinRad = groundHit ? -45 : 0;
            float angleXMaxRad = groundHit ? 45 : 360;

            foreach (ExplosionParticle particle in mExplosionParticles)
            {
                // Random particle direction
                particle.ParticleDirection = Utility.RotateVector(Vector3.UnitY,
                    Utility.RandomFloat(mRandom, angleXMinRad, angleXMaxRad),
                    Utility.RandomFloat(mRandom, 0.0f, 360),
                    0);

                // Random particle speed
                particle.ParticleSpeed = Utility.RandomFloat(mRandom, EXPLOSION_MIN_PARTICLE_SPEED,
                    EXPLOSION_MAX_PARTICLE_SPEED);

                // Random particle scale
                particle.ParticleScale = Utility.RandomFloat(mRandom, EXPLOSION_MIN_PARTICLE_SCALE,
                    EXPLOSION_MAX_PARTICLE_SCALE);
            }

            // Position explosion node to desired location in the scene
            mExplosionNode.Transform.Translation = explosionPostion;
            mExplosionNode.Visible = true;

            // Play explosion sound
            mGame.PlaySound("Explosion.wav");

            // Enter projectile selection state
            mGame.State = GorillasGame.GameState.Play_ProjectileExplosion;
        }

        // Start building collapse animation
        void StartBuildingCollapse()
        {
            // Play collapse sound
            mGame.PlaySound("Collapse.wav");

            // Enter building collapse state
            mGame.State = GorillasGame.GameState.Play_BuildingCollapse;
        }


        ///////////////////////////////////////////////////////////////////////
        // IRenderTarget interface methods

        // Engine load handler
        public void OnLoad()
        {
            ///////////////////////////////////////////////////////////////////
            // Prepare game scene

            // Add ambient light
            Light light = new Light();
            light.Type = Light.LightType.Ambient;   // Ambient light
            light.Intensity = 0.6f;                 // Intensity 60%
            light.Colour = Vector3.One;             // Pure white light
            this.AddLight(light);

            // Also add directional Sun light
            light = new Light();
            light.Type = Light.LightType.Directional;   // Directional light
            light.Intensity = 0.6f;                     // Intensity 60%
            light.Colour = Vector3.One;                 // Pure white light
            light.Position = new Vector3(100, 100, 0);  // Appx match the moon position on skybox
            this.AddLight(light);

            // Preload geometries
            Shader standardShader = AssetManager.LoadShader("Standard");
            mBuildingGeometry = AssetManager.LoadGeometry(standardShader, "Building.obj");
            mQuadGeometry = AssetManager.LoadGeometry(standardShader, "Quad.obj");

            // Create skybox node
            Shader cubemapShader = AssetManager.LoadShader("Cubemap");
            Geometry cubemapGeometry = AssetManager.LoadGeometry(cubemapShader, "Cubemap.obj");
            Texture skyboxTexture = AssetManager.LoadTexture("Skybox.png", true);
            ModelNode skyboxNode = new ModelNode("Skybox", cubemapGeometry, skyboxTexture);
            skyboxNode.Transform.ScaleTo(512, 512, 512);
            skyboxNode.Transform.RotateYBy(120);
            this.AddChild(skyboxNode);

            // Create level map node
            mLevelMapNode = new Node("LevelMap");
            this.AddChild(mLevelMapNode);

            // Create level map node
            mLevelGroundNode = new Node("LevelGround");
            this.AddChild(mLevelGroundNode);

            // Create roaming camera. This camera is used for player transition animations
            // and projectile follow-up. In addition to roaming camera there are two
            // separate per-player cameras created below
            mRoamingCamera = new Camera();
            mRoamingCamera.FieldOfView = 60; // FOV 60deg
            mRoamingCamera.NearClipPlane = 0.1f; // Clip box 0.1 .. 1000.0
            mRoamingCamera.FarClipPlane = 1000.0f;
            this.Camera = mRoamingCamera;

            // Create player objects
            mLevelPlayersNode = new Node("LevelPlayers");
            this.AddChild(mLevelPlayersNode);

            mPlayers = new Player[2];
            Geometry gorillaGeometry = AssetManager.LoadGeometry(standardShader, "Gorilla.obj");
            Texture gorillaTexture = AssetManager.LoadTexture("Gorilla.png");

            for (int i = 0; i < 2; i ++)
            {
                mPlayers[i] = new Player(i);
                mPlayers[i].Gorilla = new ModelNode("Gorilla" + i, gorillaGeometry, gorillaTexture);
                mPlayers[i].Camera = new Camera();
                mPlayers[i].Camera.FieldOfView = 60; // FOV 60deg
                mPlayers[i].Camera.NearClipPlane = 0.1f; // Clip box 0.1 .. 1000.0
                mPlayers[i].Camera.FarClipPlane = 1000.0f;
                mLevelPlayersNode.AddChild(mPlayers[i].Gorilla);
            }

            // Create projectile node
            Geometry bananaGeometry = AssetManager.LoadGeometry(standardShader, "Banana.obj");
            Texture bananaTexture = AssetManager.LoadTexture("ColourYellow.png");
            mProjectileNode = new ModelNode("Projectile", bananaGeometry, bananaTexture);
            mProjectileNode.Transform.ScaleTo(0.2f, 0.2f, 0.2f);
            mProjectileNode.Visible = false;
            this.AddChild(mProjectileNode);

            // Create explosion node
            mExplosionNode = new Node("Explosion");
            mExplosionNode.Visible = false;
            this.AddChild(mExplosionNode);

            // Create explosion particle objects
            Geometry particleGeometry = AssetManager.LoadGeometry(standardShader, "Particle.obj");
            Texture particleTexture0 = AssetManager.LoadTexture("ColourYellow.png");
            Texture particleTexture1 = AssetManager.LoadTexture("ColourRed.png");
            mExplosionParticles = new List<ExplosionParticle>();
            for (int i = 0; i < EXPLOSION_MAX_PARTICLES; i ++)
            {
                // Alternate yellow and red particles to simulate fire burst
                ExplosionParticle particle = new ExplosionParticle();
                particle.ParticleNode = new ModelNode("Particle" + i, particleGeometry,
                    ((i % 2) == 0) ? particleTexture0 : particleTexture1);
                mExplosionNode.AddChild(particle.ParticleNode);
                mExplosionParticles.Add(particle);
            }
        }

        // Window resize handler
        public void OnResize()
        {
            // Update scene's aspect ratio
            AspectRatio = (float)mGame.ViewportWidth / mGame.ViewportHeight;
        }

        // Frame update handler
        public void OnUpdate()
        {
            switch (mGame.State)
            {
            case GorillasGame.GameState.Play_LevelGeneration:

                // Animated random level generation
                OnUpdateLevelGeneration();
                break;

            case GorillasGame.GameState.Play_PlayerSelection:

                // Animated camera player selection
                OnUpdatePlayerSelection();
                break;

            case GorillasGame.GameState.Play_ProjectileParameters:

                // Player enters projectile trajectory parameters (angles, speed)
                OnUpdateProjectileParameters();
                break;

            case GorillasGame.GameState.Play_ProjectileFlight:

                // Projectile flying towards opponent's gorilla
                OnUpdateProjectileFlight();
                break;

            case GorillasGame.GameState.Play_ProjectileExplosion:

                // Animated projectile explosion on hit
                OnUpdateProjectileExplosion();
                break;

            case GorillasGame.GameState.Play_BuildingCollapse:

                // Animated building collapse
                OnUpdateBuildingCollapse();
                break;
            }
        }

        // Frame render handler
        public void OnRender()
        {
            // Let the base class do all the hard work
            base.OnRender(Matrix4.Identity);
        }

        // Mouse down handler
        public bool OnMouseDown(MouseButtonEventArgs args)
        {
            // Only handle in Projectile Parameters state
            if (mGame.State == GorillasGame.GameState.Play_ProjectileParameters)
            {
                if (args.Button == MouseButton.Left)
                {
                    // Enter drag mode
                    mProjectileMouseControl = true;
                }
                else if (args.Button == MouseButton.Right)
                {
                    // Shoot
                    LaunchProjectile();
                }

                return true;
            }

            return false;
        }

        // Mouse move handler
        public bool OnMouseMove(MouseMoveEventArgs args)
        {
            // Only handle in Projectile Parameters state
            if (mGame.State == GorillasGame.GameState.Play_ProjectileParameters)
            {
                if (mProjectileMouseControl)
                {
                    // Work out angle delta changes
                    int currentHorizontal = mActivePlayer.LaunchHorizontalAngle;
                    int newHorizontal = currentHorizontal - args.XDelta / 2;

                    Utility.NormalizeAngle(ref newHorizontal);

                    int currentVertical = mActivePlayer.LaunchVerticalAngle;
                    int newVertical = currentVertical + args.YDelta / 2;

                    if (newVertical < PROJECTILE_VANGLE_MIN)
                        newVertical = PROJECTILE_VANGLE_MIN;
                    else if (newVertical > PROJECTILE_VANGLE_MAX)
                        newVertical = PROJECTILE_VANGLE_MAX;

                    // Only update if the angles actually changed
                    if ((currentHorizontal != newHorizontal) || (currentVertical != newVertical))
                    {
                        mActivePlayer.LaunchHorizontalAngle = newHorizontal;
                        mActivePlayer.LaunchVerticalAngle = newVertical;
                        UpdatePlayer(mActivePlayer);
                    }
                }

                return true;
            }

            return false;
        }

        // Mouse up handler
        public bool OnMouseUp(MouseButtonEventArgs args)
        {
            // Abort mouse drag
            mProjectileMouseControl = false;

            if (mGame.State == GorillasGame.GameState.Play_ProjectileParameters)
                return true;

            return false;
        }

        // Mouse wheel handler
        public bool OnMouseWheel(MouseWheelEventArgs args)
        {
            // Only handle in Projectile Parameters state
            if (mGame.State == GorillasGame.GameState.Play_ProjectileParameters)
            {
                //Console.WriteLine("mouse wheel: {0}, {1}, {2}, {3}",
                //    args.Value, args.Delta, args.ValuePrecise, args.DeltaPrecise);

                // Work out speed change delta
                int delta;
                if (args.DeltaPrecise > 0.0f)
                    delta = 1;
                else if (args.DeltaPrecise < 0.0f)
                    delta = -1;
                else
                    delta = 0;

                int currentSpeed = mActivePlayer.LaunchSpeed;
                int newSpeed = currentSpeed + delta;

                if (newSpeed < PROJECTILE_SPEED_MIN)
                    newSpeed = PROJECTILE_SPEED_MIN;
                else if (newSpeed > PROJECTILE_SPEED_MAX)
                    newSpeed = PROJECTILE_SPEED_MAX;

                // Only update if the speed actaully changed
                if (currentSpeed != newSpeed)
                {
                    mActivePlayer.LaunchSpeed = newSpeed;
                    UpdatePlayer(mActivePlayer);
                }

                return true;
            }

            return false;
        }

        // Key down handler
        public bool OnKeyDown(KeyboardKeyEventArgs args)
        {
            // Only handle during gameplay
            if (mGame.State > GorillasGame.GameState.Play_NewGame)
            {
                if (args.Key == Key.Escape)
                {
                    // Stop game
                    StopGame();
                }
                else if (mGame.State == GorillasGame.GameState.Play_ProjectileParameters)
                {
                    int changeStep = args.Shift ? 1 : 5;
                    bool cameraChanged = false;
                    bool paramsChanged = false;

                    if (args.Key == Key.Enter)
                    {
                        // Shoot!
                        LaunchProjectile();
                    }
                    else if (args.Key == Key.Right)
                    {
                        // Turn right
                        mActivePlayer.LaunchHorizontalAngle -= changeStep;
                        Utility.NormalizeAngle(ref mActivePlayer.LaunchHorizontalAngle);
                        cameraChanged = true;
                        paramsChanged = true;
                    }
                    else if (args.Key == Key.Left)
                    {
                        // Turn left
                        mActivePlayer.LaunchHorizontalAngle += changeStep;
                        Utility.NormalizeAngle(ref mActivePlayer.LaunchHorizontalAngle);
                        cameraChanged = true;
                        paramsChanged = true;
                    }
                    else if (args.Key == Key.Up)
                    {
                        // Look up
                        int angle = Math.Max(mActivePlayer.LaunchVerticalAngle - changeStep, PROJECTILE_VANGLE_MIN);
                        if (mActivePlayer.LaunchVerticalAngle != angle)
                        {
                            mActivePlayer.LaunchVerticalAngle = angle;
                            cameraChanged = true;
                            paramsChanged = true;
                        }
                    }
                    else if (args.Key == Key.Down)
                    {
                        // Look down
                        int angle = Math.Min(mActivePlayer.LaunchVerticalAngle + changeStep, PROJECTILE_VANGLE_MAX);
                        if (mActivePlayer.LaunchVerticalAngle != angle)
                        {
                            mActivePlayer.LaunchVerticalAngle = angle;
                            cameraChanged = true;
                            paramsChanged = true;
                        }
                    }
                    else if (args.Key == Key.PageUp)
                    {
                        // Inc. speed
                        int speed = Math.Min(mActivePlayer.LaunchSpeed + 1, PROJECTILE_SPEED_MAX);
                        if (mActivePlayer.LaunchSpeed != speed)
                        {
                            mActivePlayer.LaunchSpeed = speed;
                            paramsChanged = true;
                        }
                    }
                    else if (args.Key == Key.PageDown)
                    {
                        // Dec. speed
                        int speed = Math.Max(mActivePlayer.LaunchSpeed - 1, PROJECTILE_SPEED_MIN);
                        if (mActivePlayer.LaunchSpeed != speed)
                        {
                            mActivePlayer.LaunchSpeed = speed;
                            paramsChanged = true;
                        }
                    }

                    // Only update if something actually changed
                    if (cameraChanged || paramsChanged)
                    {
                        UpdatePlayer(mActivePlayer);
                    }
                }

                return true;
            }

            return false;
        }

        // Key up handler
        public bool OnKeyUp(KeyboardKeyEventArgs args)
        {
            return false;
        }


        ///////////////////////////////////////////////////////////////////////
        // Gameplay update handlers

        // Animated random level generation
        void OnUpdateLevelGeneration()
        {
            // Update level rise aimation
            float animationUnitTime = Math.Min(mGame.StateElapsedTime / LEVEL_GENERATION_ANIMATION_DURATION, 1.0f);
            float levelRiseY = Utility.LinearAnimation(-mLevelSize.Y, 0, animationUnitTime);
            mLevelMapNode.Transform.TranslateYTo(levelRiseY);
            mLevelPlayersNode.Transform.TranslateYTo(levelRiseY);

            // Spin camera around the rising buildings
            Vector3 playersCenter = (mPlayers[0].Camera.Position + mPlayers[1].Camera.Position) * 0.5f;
            float playersHalfDistance = Utility.GetDistance2D(mPlayers[0].Camera.Position.X, mPlayers[0].Camera.Position.Z,
                mPlayers[1].Camera.Position.X, mPlayers[1].Camera.Position.Z) * 0.5f;
            float angleOZ = Utility.PointOnCircleGetAngle2D(playersCenter.X, playersCenter.Z,
                mPlayers[0].Camera.Position.X, mPlayers[0].Camera.Position.Z);
            float cameraAngleDeg = Utility.LinearAnimation(angleOZ, angleOZ + 360, animationUnitTime);
            float cameraAngleRad = MathHelper.DegreesToRadians(cameraAngleDeg);
            float cameraSpinRadiusMin = 1.0f;
            float cameraSpinRadiusMax = Math.Max(mLevelSize.X, mLevelSize.Y) * .5f + 5;
            //float cameraSpinRadius = cameraSpinRadiusMax;
            float cameraSpinRadius = Utility.LinearAnimation(cameraSpinRadiusMin, cameraSpinRadiusMax, 1.0f - animationUnitTime);
            float cameraX = cameraSpinRadius * (float)Math.Cos(cameraAngleRad);
            float cameraY = Utility.LinearAnimation(LEVEL_BUILDING_MIN_HEIGHT, mLevelSize.Y + 5, animationUnitTime);
            //float cameraY = 0;
            float cameraZ = cameraSpinRadius * (float)Math.Sin(cameraAngleRad);
            mRoamingCamera.Position = new Vector3(cameraX, cameraY, cameraZ);
            mRoamingCamera.LookAt = Vector3.Zero;

            // Make building floors visible as they rise from the ground
            int currentHeight = (int)((1.0f - animationUnitTime) * (mLevelSize.Y - 1));
            for (int x = 0; x < mLevelSize.X; x += 2)
            {
                for (int z = 0; z < mLevelSize.Z; z += 2)
                {
                    Building building = mLevelMap[x, z];
                    for (int h = currentHeight; h < building.BuildingHeight; h ++)
                        building.FloorNodes[h].Visible = true;
                }
            }

            // Make gorillas visible as they rise from the ground
            foreach (Player player in mPlayers)
                player.Gorilla.Visible = player.Position.Y >= currentHeight;

            // Level generation animation complete?
            if (animationUnitTime == 1.0f)
            {
                // Start next turn
                StartNextTurn();
            }
        }

        // Animated camera player selection
        void OnUpdatePlayerSelection()
        {
            // Update roaming camera position and look target animation
            float animationUnitTime = Math.Min(mGame.StateElapsedTime / mCameraAnimationDuration, 1.0f);
            if (animationUnitTime <= 1.0f)
            {
                // We want camera to rise above the city so adjust Y accordingly
                float heightAdjust = 5.0f + mLevelSize.Y - Math.Max(mCameraAnimationStartPosition.Y, mCameraAnimationStopPosition.Y);
                Vector3 cameraPosition = new Vector3(
                    0,
                    Utility.SineAnimation(mCameraAnimationStartPosition.Y, mCameraAnimationStopPosition.Y, animationUnitTime) +
                    (float)Math.Sin(animationUnitTime * MathHelper.Pi) * heightAdjust,
                    0);

                // We don't want to move the camera from player to player in a straight line otherwise it
                // is very easy to note the direction of the shot! We will use ellipse shaped orbit instead.
                Vector3 playersCenter = (mCameraAnimationStartPosition + mCameraAnimationStopPosition) * 0.5f;
                float playersHalfDistance = Utility.GetDistance2D(mCameraAnimationStartPosition.X, mCameraAnimationStartPosition.Z,
                    mCameraAnimationStopPosition.X, mCameraAnimationStopPosition.Z) * 0.5f;

                float angleOZ = Utility.PointOnCircleGetAngle2D(playersCenter.X, playersCenter.Z,
                    mCameraAnimationStartPosition.X, mCameraAnimationStartPosition.Z);

                float ellipseStartAngle, ellipseEndAngle, flipAngle;
                if (mActivePlayer.Index == 0)
                {
                    ellipseStartAngle = 0;
                    ellipseEndAngle = 180;
                    flipAngle = angleOZ;
                }
                else
                {
                    ellipseStartAngle = 180;
                    ellipseEndAngle = 360;
                    flipAngle = angleOZ + 180;
                }

                float ellipseAngle = Utility.SineAnimation(ellipseStartAngle, ellipseEndAngle, animationUnitTime);
                float ellipseAngleRad = MathHelper.DegreesToRadians(ellipseAngle);
                float ellipsePositionX = playersCenter.X + playersHalfDistance * (float)Math.Cos(ellipseAngleRad);
                float ellipsePositionZ = playersCenter.Z + 0.3f * playersHalfDistance * (float)Math.Sin(ellipseAngleRad);

                Utility.PointOnCircleRotatedByAngle2D(playersCenter.X, playersCenter.Z, ellipsePositionX, ellipsePositionZ,
                    flipAngle, ref cameraPosition.X, ref cameraPosition.Z);

                Vector3 cameraLookAt = Utility.SineAnimation(mCameraAnimationStartLookAt, mCameraAnimationStopLookAt, animationUnitTime);

                mRoamingCamera.Position = cameraPosition;
                mRoamingCamera.LookAt = cameraLookAt;

                if (mNumberOfTurns >= 3)
                {
                    // Show/hide players based on camera position. We don't want camera
                    // positioned inside gorilla 3D model!
                    foreach (Player player in mPlayers)
                    {
                        player.Gorilla.Visible = Utility.GetDistance3D(player.Gorilla.Transform.Translation,
                            cameraPosition) > 1.0f;
                    }

                    // Player selection animation complete?
                    if (animationUnitTime == 1.0f)
                    {
                        mGame.PlaySound("Beep.wav");

                        // Update player
                        UpdatePlayer(mActivePlayer);

                        // Update stats
                        UpdateStats();

                        // Switch to active player camera
                        this.Camera = mActivePlayer.Camera;

                        // Enter projectile trajectory parameters input state
                        mGame.State = GorillasGame.GameState.Play_ProjectileParameters;
                    }
                }
            }

            if ((mNumberOfTurns < 3) && (animationUnitTime == 1.0f))
            {
                float elapsedTime = mGame.StateElapsedTime - mCameraAnimationDuration;
                float blinkTime = PLAYER_SELECTION_BLINK_DURATION * PLAYER_SELECTION_BLINK_COUNT;
                if (elapsedTime <= blinkTime)
                {
                    int blink = (int)(elapsedTime / PLAYER_SELECTION_BLINK_DURATION);
                    if (mPlayerSelectionBlink != blink)
                    {
                        if ((blink % 2) == 0)
                        {
                            mGame.UI.SetActivePlayer(mActivePlayer.Index);
                            mGame.UI.ShowPlayerText(mActivePlayer.Index, true);
                            mGame.PlaySound("Beep.wav");
                            mActivePlayer.Gorilla.Visible = true;
                        }
                        else
                        {
                            mGame.UI.SetActivePlayer(-1);
                            mGame.UI.ShowPlayerText(mActivePlayer.Index, false);
                            mActivePlayer.Gorilla.Visible = false;
                        }

                        mPlayerSelectionBlink = blink;
                    }
                }
                else
                {
                    mActivePlayer.Gorilla.Visible = true;
                    StartNextTurn();
                }
            }
        }

        // Player enters projectile trajectory parameters (angles, speed)
        void OnUpdateProjectileParameters()
        {
        }

        // Projectile flying towards opponent's gorilla
        void OnUpdateProjectileFlight()
        {
            Vector3 projectilePositionLast = mProjectileNode.Transform.Translation;
            Vector3 projectilePositionNew = SimulateProjectilePhysics(mGame.StateElapsedTime);

            mProjectileNode.Transform.Translation = projectilePositionNew;
            mProjectileNode.Transform.RotateXBy(20.0f * mActivePlayer.LaunchSpeed * mGame.DeltaTime);

            // Camera follows the projectile
            mRoamingCamera.LookAt = projectilePositionNew;

            // Hit anything yet?
            mProjectileHit = HitTest(projectilePositionNew);
            if (mProjectileHit != null)
            {
                // Hide projectile
                mProjectileNode.Visible = false;

                // Depending on hit object
                switch (mProjectileHit.Type)
                {
                // Hit ground
                case HitTestResult.HitType.Ground:

                    // Just start explosion animation
                    StartExplosion(projectilePositionNew, true);
                    break;

                // Hit building
                case HitTestResult.HitType.Building:

                    // Remove the hit floor node and trigger explosion animation
                    Building building = mProjectileHit.Building;
                    Node floorNode = building.FloorNodes[mProjectileHit.Tile.Y];
                    building.FloorNodes.RemoveAt(mProjectileHit.Tile.Y);
                    building.BuildingNode.RemoveChild(floorNode.Name);
                    building.BuildingHeight --;
                    StartExplosion(TilePositionToScenePosition(mProjectileHit.Tile), false);
                    break;

                // Hit opponent's gorilla
                case HitTestResult.HitType.Player:

                    // Update active player's score
                    mActivePlayer.Score ++;
                    UpdateStats();

                    // Hide gorilla node and trigger explosion animation
                    mProjectileHit.Player.Gorilla.Visible = false;
                    StartExplosion(TilePositionToScenePosition(mProjectileHit.Player.Position), false);
                    break;
                }
            }
            else
            {
                // Camera follows the projectile but not too close so we don't
                // fly into explosion on impact!
                Vector3 projectileInvertedDirection = projectilePositionLast - projectilePositionNew;
                projectileInvertedDirection.Normalize();
                if (projectileInvertedDirection.Length != 0.0f)
                {
                    float timeToHit = Math.Max(mProjectileFlightTime - mGame.StateElapsedTime, 0.0f);
                    float cameraDistanceFactor;
                    if (timeToHit > 0.5f)
                        cameraDistanceFactor = PROJECTILE_CAMERA_DISTANCE;
                    else
                        cameraDistanceFactor = PROJECTILE_CAMERA_DISTANCE + 1.0f * (1.0f - (timeToHit / 0.5f));

                    mRoamingCamera.Position = projectilePositionNew + projectileInvertedDirection * cameraDistanceFactor;
                }
            }
        }

        // Animated projectile explosion on hit
        void OnUpdateProjectileExplosion()
        {
            // Update explosion animation
            float animationUnitTime = Math.Min(mGame.StateElapsedTime / EXPLOSION_ANIMATION_DURATION, 1.0f);
            if (animationUnitTime < 1.0f)
            {
                foreach (ExplosionParticle particle in mExplosionParticles)
                {
                    // Particle position
                    particle.ParticleNode.Transform.TranslateTo(
                        particle.ParticleDirection * particle.ParticleSpeed * animationUnitTime);

                    // Particle scale
                    float scale = Math.Max(particle.ParticleScale * (1.0f - animationUnitTime), 0.05f);
                    particle.ParticleNode.Transform.ScaleTo(scale, scale, scale);
                }
            }
            else
            {
                // Hide explosion node
                mExplosionNode.Visible = false;
            }

            // Give an extra delay before we start next turn
            float extraDelay = (mProjectileHit.Type == HitTestResult.HitType.Building) ? 0.0f : 0.2f;
            if ((mGame.StateElapsedTime - EXPLOSION_ANIMATION_DURATION) >= extraDelay)
            {
                // Depending on hit object
                switch (mProjectileHit.Type)
                {
                // Hit ground
                case HitTestResult.HitType.Ground:

                    // Next turn
                    StartNextTurn();
                    break;

                // Hit building
                case HitTestResult.HitType.Building:

					// Start building collapse
                    StartBuildingCollapse();
                    break;

                // Hit opponent's gorilla
                case HitTestResult.HitType.Player:

                    // Start new level
                    StartNextLevel();
                    break;
                }
            }
        }

        // Animated building collapse
        void OnUpdateBuildingCollapse()
        {
            float animationUnitTime = Math.Min(mGame.StateElapsedTime / BUILDING_COLLAPSE_ANIMATION_DURATION, 1.0f);
            if (animationUnitTime <= 1.0f)
            {
                float collapseOffset = 1.0f - Utility.SineAnimationInFactor(animationUnitTime);
                Building building = mProjectileHit.Building;

                // Slide down building floors
                for (int i = mProjectileHit.Tile.Y; i < building.BuildingHeight; i ++)
                    building.FloorNodes[i].Transform.TranslateYTo(collapseOffset + i);

                // If the player is on top of sliding building we need to update player's position
                if (building.Player != null)
                {
                    building.Player.Gorilla.Transform.TranslateYTo(collapseOffset + building.BuildingHeight + 0.5f);
                    building.Player.Gorilla.Transform.RotateTo(0, building.Player.LaunchHorizontalAngle, 0);

                    // If the player is active player (player shot the floor under himself) we need to
                    // also update active camera
                    if (building.Player == mActivePlayer)
                    {
                        mRoamingCamera.Position = building.Player.Gorilla.Transform.Translation;
                        mRoamingCamera.Rotation = new Vector3(building.Player.LaunchVerticalAngle, building.Player.LaunchHorizontalAngle, 0);
                    }
                }
            }

            // Give an extra 0.2s delay before we start next turn
            if ((mGame.StateElapsedTime - BUILDING_COLLAPSE_ANIMATION_DURATION) >= 0.2f)
            {
                // Update player if needed (but keep projectile invisible!)
                if (mProjectileHit.Building.Player != null)
                {
                    UpdatePlayer(mProjectileHit.Building.Player);
                    mProjectileNode.Visible = false;
                }

                // Start next turn
                StartNextTurn();
            }
        }


        ///////////////////////////////////////////////////////////////////////
        // Update

        // Update player position
        void UpdatePlayer(Player player)
        {
            // Place player's gorilla on the roof of building
            player.Position.Y = mLevelMap[player.Position.X, player.Position.Z].BuildingHeight;

            Vector3 playerPosition = TilePositionToScenePosition(player.Position);
            player.Gorilla.Transform.TranslateTo(playerPosition);

            // Gorilla is facing the direction of throw
            player.Gorilla.Transform.RotateTo(0, player.LaunchHorizontalAngle, 0);

            // Player's camera is positioned in the centre of gorilla model
            player.Camera.Position = player.Gorilla.Transform.Translation;
            player.Camera.Rotation = new Vector3(player.LaunchVerticalAngle, player.LaunchHorizontalAngle, 0);

            // Adjust projectile node position and orientation according
            // to launch parameters
            player.LaunchDirection = Utility.RotateVector(Vector3.UnitZ, player.Camera.Rotation);
            player.LaunchPosition = playerPosition + player.LaunchDirection *
                PROJECTILE_CAMERA_DISTANCE;

            // Is this active player?
            if ((mNumberOfTurns >= 3) && (player == mActivePlayer))
            {
                // Show projectile node for active player
                mProjectileNode.Visible = true;
                mProjectileNode.Transform.Translation = player.LaunchPosition;
                mProjectileNode.Transform.Rotation = player.Camera.Rotation;
                mGame.UI.SetActivePlayer(mActivePlayer.Index);
                mGame.UI.SetThrowParams(mActivePlayer.LaunchSpeed, mActivePlayer.LaunchHorizontalAngle,
                    -mActivePlayer.LaunchVerticalAngle);
            }
        }

        // Update game stats
        void UpdateStats()
        {
            mGame.UI.SetPlayerScore(mActivePlayer.Index, mActivePlayer.Score);
            mGame.UI.SetPlayerScore(mInactivePlayer.Index, mInactivePlayer.Score);
        }


        ///////////////////////////////////////////////////////////////////////
        // Helper level tile info methods

        // Check if tile is within level boundary
        bool IsLevelTile(int x, int z)
        {
            return ((x >= 0) && (x < mLevelSize.X) &&
                    (z >= 0) && (z < mLevelSize.Z));
        }

        // Check if tile is within level boundary
        bool IsLevelTile(IVector3 pos)
        {
            return IsLevelTile(pos.X, pos.Z);
        }

        // Check if tile is a buidling tile
        bool IsBuildingLevelTile(int x, int z)
        {
            return ((x >= 0) && (x < mLevelSize.X) && ((x % 2) == 0) &&
                    (z >= 0) && (z < mLevelSize.Z) && ((z % 2) == 0));
        }

        // Check if tile is a buidling tile
        bool IsBuildingLevelTile(IVector3 pos)
        {
            return IsBuildingLevelTile(pos.X, pos.Z);
        }

        // Check if tile is a corner tile
        bool IsCornerLevelTile(int x, int z, ref int angle)
        {
            if ((x == -1) && (z == -1))
            {
                angle = 0;
                return true;
            }

            if ((x == -1) && (z == mLevelSize.Z))
            {
                angle = 90;
                return true;
            }

            if ((x == mLevelSize.X) && (z == -1))
            {
                angle = 270;
                return true;
            }

            if ((x == mLevelSize.X) && (z == mLevelSize.Z))
            {
                angle = 180;
                return true;
            }

            return false;
        }

        // Check if tile is a street tile
        bool IsStreetLevelTile(int x, int z, ref int angle)
        {
            if ((x >= -1) && (x <= mLevelSize.X) && (z >= -1) && (z <= mLevelSize.Z))
            {
                if ((x % 2) == 0)
                {
                    angle = 0;
                    return true;
                }

                if ((z % 2) == 0)
                {
                    angle = 90;
                    return true;
                }
            }

            return false;
        }

        // Check if tile is a junction tile
        bool IsJunctoionLevelTile(int x, int z, ref int angle, ref bool tJunction)
        {
            if ((x >= -1) && (x <= mLevelSize.X) && (z >= -1) && (z <= mLevelSize.Z) &&
                (((x % 2) != 0) || ((z % 2) != 0)))
            {
                angle = 0;
                tJunction = false;

                if (x == -1)
                {
                    angle = 90;
                    tJunction = true;
                }
                else if (x == mLevelSize.X)
                {
                    angle = 270;
                    tJunction = true;
                }
                else if (z == -1)
                {

                    tJunction = true;
                }
                else if (z == mLevelSize.Z)
                {
                    angle = 180;
                    tJunction = true;
                }

                return true;
            }

            return false;
        }

        // Lookup buidling at tile pos
        Building GetBuildingAtTile(int x, int z)
        {
            if ((x >= 0) && (x < mLevelSize.X) &&
                (z >= 0) && (z < mLevelSize.Z))
            {
                return mLevelMap[x, z];
            }

            return null;
        }

        // Lookup buidling at tile pos
        Building GetBuildingAtTile(IVector3 pos)
        {
            return GetBuildingAtTile(pos.X, pos.Z);
        }

        // Convert scene position to tile position
        IVector3 ScenePositionToTilePosition(Vector3 pos)
        {
            return new IVector3(
                (int)Math.Floor(pos.X - mLevelBoundary.X),
                (int)Math.Floor(pos.Y - mLevelBoundary.Y),
                (int)Math.Floor(pos.Z - mLevelBoundary.Z));
        }

        // Convert tile position to scene position
        Vector3 TilePositionToScenePosition(IVector3 pos)
        {
            return new Vector3(
                mLevelBoundary.X + pos.X + 0.5f,
                mLevelBoundary.Y + pos.Y + 0.5f,
                mLevelBoundary.Z + pos.Z + 0.5f);
        }

        // Hit test at tile position
        HitTestResult HitTest(IVector3 pos)
        {
            if (pos.Y < 0)
            {
                // Ground hit
                return new HitTestResult();
            }

            if (IsBuildingLevelTile(pos))
            {
                if (mInactivePlayer.Position == pos)
                {
                    // Player hit
                    return new HitTestResult(pos, mInactivePlayer);
                }

                Building building = GetBuildingAtTile(pos);
                if ((building != null) && (pos.Y < building.BuildingHeight))
                {
                    // Building hit
                    return new HitTestResult(pos, building);
                }
            }

            // Nothing
            return null;
        }

        // Hit test at scene position
        HitTestResult HitTest(Vector3 pos)
        {
            if (pos.Y < 0.0f)
            {
                // Ground hit
                return new HitTestResult();
            }
            else
            {
                // Could be a tile object
                IVector3 tilePos = ScenePositionToTilePosition(pos);
                return HitTest(tilePos);
            }
        }

        // Simulate flying projectile physics
        public Vector3 SimulateProjectilePhysics(float time)
        {
            // Compute initial trajectory velocity based on active user's projectile
            // launch parameters
            float launchVerticalAngleRad = MathHelper.DegreesToRadians(360.0f - mActivePlayer.LaunchVerticalAngle);
            Vector2 projectileVelocity2D = new Vector2(
                (float)mActivePlayer.LaunchSpeed * (float)Math.Cos(launchVerticalAngleRad),
                (float)mActivePlayer.LaunchSpeed * (float)Math.Sin(launchVerticalAngleRad));

            // Update projectile position and orientation
            float speedSquare = projectileVelocity2D.X * projectileVelocity2D.X +
                                projectileVelocity2D.Y * projectileVelocity2D.Y;
            float speedSquareRoot = (float)Math.Sqrt(speedSquare);

            // Although we aim to simulate real world physics to some extent, we are not
            // going to simulate air resistance (air drag) because this requires taking
            // into account complex parameters such as air density (depneds on temeprature
            // and altitude), projectile aerodynamics (depends on frontal area which for a
            // non-uniform spinning object such as banana is constantly changing with
            // roataion) etc. This is well beyond the scope of this project.

            float airResistanceCoefficient = 0; // Assume no air drag
            float airResistance = speedSquareRoot * airResistanceCoefficient;

            // Wind affect is calculater later after we have the 3D projectile
            // coordinate. At this point we assume no horizontal force affecting
            // projectile.
            float horizontalForce = 0.0f;

            // Take gravity into account
            float verticalForce = PHYSICS_GRAVITY;

            // Update projectile velocity based on delta time. This also takes into
            // account vertical acceleration of gravity.
            Vector2 projectilePosition2D = Vector2.Zero;
            float currentTime = time;
            float deltaTime = 0;

            do
            {
                deltaTime = Math.Min(currentTime, 0.01f);
                projectileVelocity2D.X += ((horizontalForce - projectileVelocity2D.X /
                    speedSquareRoot * airResistance) * deltaTime);
                projectileVelocity2D.Y += ((verticalForce - projectileVelocity2D.Y /
                    speedSquareRoot * airResistance) * deltaTime);
                projectilePosition2D += (projectileVelocity2D * deltaTime);
                currentTime -= deltaTime;
            }
            while (currentTime > 0.0f);

            projectilePosition2D *= PHYSICS_TILE_SCALE;

            // Update projectile position and spinning based on frame delta time
            float projectileX = 0, projectileZ = 0;
            Utility.PointOnCircleRotatedByAngle2D(
                0,
                0,
                projectilePosition2D.X,
                0,
                270.0f - mActivePlayer.LaunchHorizontalAngle,
                ref projectileX,
                ref projectileZ);

            Vector3 projectilePosition3D = new Vector3(
                mActivePlayer.LaunchPosition.X + projectileX,
                mActivePlayer.LaunchPosition.Y + projectilePosition2D.Y,
                mActivePlayer.LaunchPosition.Z + projectileZ);

            // Take wind effect into account
            if (mWindSpeed > 0)
                projectilePosition3D += (mWindDirection * (time * mWindSpeed * PHYSICS_TILE_SCALE));

            return projectilePosition3D;
        }
    }
}
