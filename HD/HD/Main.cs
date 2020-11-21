using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections;
using System.Collections.Specialized;
using System.Reflection;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.GamerServices;
using Steamworks;

namespace HD
{
    public class Main : Microsoft.Xna.Framework.Game
    {
        const string BackStory =
@"-= ASTERIA =-

2017 - NASA's Terrestrial Planet Finder mission detects an
earth like planet orbiting the star Pi3 Orionis, 26.33 light
years away. Spectral analysis of the light from the planet's
atmosphere shows a strong line of oxygen, a sure sign of life.

2039 - Asteroid mining conglomerate Planetary Resources completes
and launches Project Daedalus, a 191 meter interstellar starship
powered by the fusion of deuterium and helium-3 harvested from
Jupiter's atmosphere.

2137 - After traveling at 29% of the speed of light, the unmanned 
starship arrives and lands on the planet, now known as Asteria.
The mother ship thaws and successfully incubates 353 embryos,
the first interstellar humans are born and raised by machine.

2153 - A group of powerful natives attack and overrun the Daedalus,
obliterating it in a nuclear meltdown. You are Ryker 241 and as
far as you know, you're the only surviving human...";

        const string Help =
@"-= GAME CONTROLS =-

WASD/Arrows - Move
Space - Jump
Left Mouse - Use Selected Action Bar Item
Right Mouse - Use R Action Bar Slot Item

Tab, E or I - Inventory
1-9 - Use/Select Action Bar Item
Mouse Wheel - Select Action Bar Item

Alt+Enter - Toggle Full Screen
F10 - Toggle Vertical Sync";

        const string AdminHelp = @"

Tilde (~) - Toggle God Mode

F1 - Material Creative Mode
F2 - Placeable Creative Mode
F3 - Enemy Creative Mode
F4 - Item Creative Mode

F5 - Debug Info
F6 - Toggle Background Rendering
F7 - Toggle Wall Rendering
F8 - Toggle Terrain Rendering (useful when editing walls)

F9 - Toggle Lighting

Creative Mode Left Mouse - Place
Creative Mode Right Mouse - Remove";

        const string Credits =
@"-= GAME CREDITS =-

Creator & Lead Programming and Design
 Bryan Livingston

Shader Programming
 Rodrigo Diaz
 Javad Kouchakzadeh
 Bryan Livingston

Additional Programming
 Sam Bryan
 Chase Davies
 Isaac Herring
 Randee Shirts
 Zachary Thorpe

Lead Artist
 Porter Nielsen

Environment Art
 Patrick Thompson

Logo Design
 Mihai-Cristian Agape

Trailer Design
 Josh Krieger (Blitzkriegsler)

Additional Artists
 Tim Von Rueden
 Dave Swan
 Wiktor Morzewski

Sound Design
 Sam Bryan
 Philip Shirts
 Zachary Thorpe

Content Design
 Daniel Beatty
 Michael Breese
 Sam Bryan
 Jason Danielson
 Cavan Helps
 Philip Shirts
 Zachary Thorpe

Testers
 Jason Danielson
 Josh Jones
 Scott Knudsen
 McKay Salisbury";

        const string EndStory =
@"-= YOU HAVE WON THE GAME! =-

Congratulations, you've defeated the alien menace.

Upon defeating the final boss you discover fellow human survivors and have
secured the survival of the human race on Asteria. The colony can survive
and flourish now.

You have become a legend that will always be remembered as a hero on this
planet.

As a reward for your victory you've been given the Keys of the Kingdom that
make you all powerful. Use them wisely.



";

        const int mouseScrollHitZone = 5;
        const int scrollDisplayTextMarginHeight = 150;
        const int slotMargin = 3;

        public static Color DefaultColor = Color.FromNonPremultiplied(173, 255, 255, 200);//Color.LightSkyBlue; //Color.LightSteelBlue;
        public static Color DisabledColor = Color.DarkGray;
        public static Color ErrorColor = Color.OrangeRed;
        public static Color HoverColor = Color.LightCyan;
        public static Color SelectedColor = Color.GreenYellow;
        public static Color WindowColor = Color.SteelBlue;
        public static Color GridColor = Color.FromNonPremultiplied(255, 255, 255, 48);

        public static Color[] DayNightGradientData;

        public static Texture2D ChatBoxTexture { get; set; }
        public static Texture2D MenuTextBoxTexture { get; set; }
        public static Texture2D TitleLogoTexture { get; set; }
        public static Texture2D LegendStudioLogoTexture { get; set; }
        public static Texture2D Starfield { get; set; }
        public static Texture2D Cursor { get; set; }
        public static Texture2D Crosshair { get; set; }
        public static Texture2D GUITop { get; set; }
        public static Texture2D GUIBottom { get; set; }
        public static Texture2D LifeCounter { get; set; }

        public static Main Instance;

        public static int ResolutionWidth;
        public static int ResolutionHeight;
        public static Point ScreenOffset;
        Vector2 menuScreenOffset;

        public static Entity MouseOverEntity;
        public static bool RenderingEnabled = false;
        public static bool IsShowingInventory;

        public static GameClient GameClient;
        public static Map Map;
        public static Map NullMap;
        static Player player;
        public static Player Player
        {
            set
            {
                player = value;
                if (player != null)
                {
                    wasGameCompleted = player.IsGameCompleted;
                    player.ClientResolutionWidth = ResolutionWidth;
                    player.ClientResolutionHeight = ResolutionHeight;
                    player.GetScreenOffset();
                }
            }
            get { return player; }
        }

        public static MouseState LastMouse;
        public static MouseState CurrentMouse;
        public static KeyboardState LastKeyboard;
        public static KeyboardState CurrentKeyboard;
        public static GamePadState LastGamePad;
        public static GamePadState CurrentGamePad;
        public bool IsAimModeEnabled;
        public static bool IsLeftMouseButtonHandled;

        static DateTime displayTextSetTime;
        static DateTime minimumDisplayTextTime;
        static string displayText;
        public static string DisplayText
        {
            get { return displayText; }
            set { displayText = value; displayTextSetTime = DateTime.UtcNow; }
        }

        static GraphicsDeviceManager graphics;
        static SpriteBatch borderBatch;
        static SpriteBatch spriteBatch;
        static SpriteBatch wallBatch;
        static SpriteBatch foreBatch;

        public static Menu CurrentMenu;

        static bool wasGameCompleted;

        FuchsGUI.TextBox chatTextBox;
        int chatLogIndex;
        static FuchsGUI.Control currentControl;
        TimeSpan elapsedTime;
        int frameCounter;
        public static int FrameRate;
        int lastScrollWheelValue;
        bool updateRunning;
        bool showGui = true;
        bool showDebug = false;
        bool disableLighting;
        internal static Menu StartMenu;
        internal static Menu RegisterMenu;
        internal static Menu LoginMenu;
        Menu pauseMenu;
        Menu settingsMenu;
        Menu resolutionMenu;
        Menu createWorldMenu;
        Menu createPlayerMenu;
        Menu serverSelectMenu;
        bool toggleFullscreenFlag;
        ItemType MouseOverItemType;
        int MouseOverItemAmount;
        int MouseOverSlotNumber;
        SlotType MouseOverSlotType;
        Menu multiplayerPauseMenu;
        bool leftClickHandled;
        //FileSystemWatcher watcher;
        bool renderBackground = true;
        bool renderForeground = true;
        bool renderWall = true;
        Vector2 menuScreenPanSpeed;
        static Recipe selectedRecipe;
        static bool isCraftingBasic;
        int inventoryPage;
        static Item splittingItem;
        bool loadingScreenRendered;
        static bool isEditingActivePlaceable;
        public static bool ForceModal;
        public static bool IsMouseOverControl;
        DateTime runningSince;
        private float aimAngle;

        public static bool IsFullScreen { get { return graphics != null ? graphics.IsFullScreen : false; } }
        StringCollection chatLog { get { return GameSettings.Default.ChatLog; } }
        public bool IsModal
        {
            get
            {
                return CurrentMenu != null
                    || (Player != null && Player.ActivePlaceable != null && Player.ActivePlaceable.Inventory == null)
                    || isCraftingBasic
                    || splittingItem != null
                    || ForceModal;
            }
        }

        public static readonly char[] InvalidFileNameChars = { '\"', '<', '>', '|', '\0', (Char)1, (Char)2, (Char)3, (Char)4, (Char)5, (Char)6, (Char)7, (Char)8, (Char)9, (Char)10, (Char)11, (Char)12, (Char)13, (Char)14, (Char)15, (Char)16, (Char)17, (Char)18, (Char)19, (Char)20, (Char)21, (Char)22, (Char)23, (Char)24, (Char)25, (Char)26, (Char)27, (Char)28, (Char)29, (Char)30, (Char)31 };

        public Main()
        {
            Instance = this;
            runningSince = DateTime.UtcNow;

            Player.OnPlayerSwap += (oldPlayer, newPlayer) =>
            {
                Player = newPlayer;
                Map = Player.Map;
                Background.ClearMap();
            };

            FuchsGUI.TextBox.CursorColor = Main.DefaultColor;

            if (GameSettings.Default.ChatLog == null)
                GameSettings.Default.ChatLog = new StringCollection();

#if WINDOWS
            Content.RootDirectory = Utility.ContentDirectory;
#else
            Content.RootDirectory = "Content";
            Utility.Content = Content;
            Components.Add(new GamerServicesComponent(this));
#endif

            Utility.LogMessage(Utility.GameName + " Client, version " + Utility.Version);
            Window.Title = Utility.GameName;

            World.RegisterPlugins();


#if STEAM
            //try
            //{
            var steamInitalized = SteamAPI.Init();
            if (!steamInitalized)
            {
                Utility.LogMessage("Sorry, Asteria must be run from within Steam.");
                System.Windows.Forms.MessageBox.Show("Sorry, Asteria must be run from within Steam.");
                Exit();
            }

            Utility.LogMessage("Is Steam Running: " + SteamAPI.IsSteamRunning());

#if DEBUG
                Utility.LogMessage("SteamUserStats.ResetAllStats(true)");
                SteamUserStats.ResetAllStats(true);
#endif

            Counters.Initalize();

            //Callback<RemoteStoragePublishedFileSubscribed_t>.Create((result) =>
            //{
            //    Utility.LogMessage("[" + RemoteStoragePublishedFileSubscribed_t.k_iCallback + " - RemoteStoragePublishedFileSubscribed] - " + result.m_nPublishedFileId + " -- " + result.m_nAppID);
            //});
            //Callback<RemoteStoragePublishedFileUnsubscribed_t>.Create((result) =>
            //{
            //    Utility.LogMessage("[" + RemoteStoragePublishedFileUnsubscribed_t.k_iCallback + " - RemoteStoragePublishedFileUnsubscribed] - " + result.m_nPublishedFileId + " -- " + result.m_nAppID);
            //});

            //Utility.LogMessage("Enumerating Subscribed files");
            //CallResult<RemoteStorageEnumerateUserSubscribedFilesResult_t>.Create((result, isOk) =>
            //{
            //    Utility.LogMessage("[" + RemoteStorageEnumerateUserSubscribedFilesResult_t.k_iCallback + " - RemoteStorageEnumerateUserSubscribedFilesResult] - " + result.m_eResult + " -- " + result.m_nResultsReturned + " -- " + result.m_nTotalResultCount + " -- " + result.m_rgPublishedFileId + " -- " + result.m_rgRTimeSubscribed);
            //}).Set(SteamRemoteStorage.EnumerateUserSubscribedFiles(0));

            //}
            //catch (InvalidOperationException e)
            //{
            //    Utility.LogMessage(e.ToString());
            //}
#endif


            graphics = new GraphicsDeviceManager(this);

            if (GameSettings.Default.ResolutionWidth == 0)
                GameSettings.Default.ResolutionWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            if (GameSettings.Default.ResolutionHeight == 0)
                GameSettings.Default.ResolutionHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;

            graphics.PreferredBackBufferWidth = GameSettings.Default.ResolutionWidth;
            graphics.PreferredBackBufferHeight = GameSettings.Default.ResolutionHeight;
            graphics.PreferMultiSampling = true;
            graphics.IsFullScreen = GameSettings.Default.IsFullscreen;

            Utility.LogMessage("Attempting to switch to {0}x{1} Fullscreen: {2} Format: {3}.", graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight, graphics.IsFullScreen, graphics.PreferredBackBufferFormat);

            // http://stackoverflow.com/questions/11283294/how-to-resize-window-using-xna/11287316#11287316
            // "And do not call ApplyChanges()"
            //graphics.ApplyChanges();
            //Utility.LogMessage("Graphics mode changed successfully.");

            IsMouseVisible = false;
            IsFixedTimeStep = false;

            //this.IsFixedTimeStep = true;
            //this.TargetElapsedTime = TimeSpan.FromSeconds(1 / 30.0);

            LastMouse = Mouse.GetState();
            LastKeyboard = Keyboard.GetState();

            DisplayText = BackStory;
        }

        protected override void LoadContent()
        {
            Utility.LogMessage("Started loading game assets.");

            spriteBatch = new SpriteBatch(GraphicsDevice);
            borderBatch = new SpriteBatch(GraphicsDevice);
            wallBatch = new SpriteBatch(GraphicsDevice);
            foreBatch = new SpriteBatch(GraphicsDevice);

            // needed for title screen
            TitleLogoTexture = Utility.LoadTexture(GraphicsDevice, "Title Logo.png");
            LegendStudioLogoTexture = Utility.LoadTexture(GraphicsDevice, "Legend Studio Logo.png");
            Starfield = Utility.LoadTexture(GraphicsDevice, "Starfield.jpg");

            Cursor = Utility.LoadTexture(GraphicsDevice, "Cursor.png");
            Crosshair = Utility.LoadTexture(GraphicsDevice, "Crosshair.png");

            Utility.Font = Content.Load<SpriteFont>("Large Font");
            Utility.Font.Spacing -= 2;
            Utility.Font.DefaultCharacter = '*';
            Utility.MediumFont = Content.Load<SpriteFont>("Medium Font");
            Utility.MediumFont.Spacing -= 2;
            Utility.MediumFont.DefaultCharacter = '*';
            Utility.SmallFont = Content.Load<SpriteFont>("Small Font");
            Utility.SmallFont.Spacing -= 2;
            Utility.SmallFont.DefaultCharacter = '*';

            Audio.Songs["Menu & Skyrealm"] = Utility.LoadSong("Menu & Skyrealm");
            Audio.PlaySong("Menu & Skyrealm");

            RenderingEnabled = false;
            loadingScreenRendered = false;

            ThreadPool.QueueUserWorkItem((state) =>
            {
                Utility.BlendEffect = Content.Load<Effect>("BlendEffect");

                Lighting.LightSpreadEffect = Content.Load<Effect>("LightSpreadEffect");
                Lighting.Initialize(GraphicsDevice);

                LoadMainTextures();

                ItemBase.LoadContent(GraphicsDevice);
                ProjectileBase.LoadContent(GraphicsDevice);
                Background.LoadContent(GraphicsDevice);
                Enemy.LoadContent(GraphicsDevice);
                MaterialInfo.LoadContent(GraphicsDevice);
                Lighting.Reset();

                Utility.PixelTexture = new Texture2D(GraphicsDevice, 1, 1);
                Utility.PixelTexture.SetData(new Color[] { Color.White });
                FuchsGUI.Slider.PixelTexture = Utility.PixelTexture;

                // audio
                Audio.LoadContent(Content);

                Utility.LogMessage("Finished loading game assets.");

                CreateMenus();

                RenderingEnabled = true;
            }
            );
        }

        protected override void UnloadContent()
        {
            lock (this) // ensure that draw has finished.
            {
                Utility.LogMessage("Unloading content.");
                RenderingEnabled = false;
                base.UnloadContent();
            }
        }

        void LoadMainTextures()
        {
            Utility.EmptySlotTexture = Utility.LoadTexture(GraphicsDevice, "Empty Slot.png");
            Utility.FilledSlotTexture = Utility.LoadTexture(GraphicsDevice, "Filled Slot.png");
            ChatBoxTexture = Utility.LoadTexture(GraphicsDevice, "Chat Box.png");
            MenuTextBoxTexture = Utility.LoadTexture(GraphicsDevice, "Menu Text Box.png");
            Utility.BrushTexture = Utility.LoadTexture(GraphicsDevice, "Brushes.png");
            Button.ButtonTexture = Utility.LoadTexture(GraphicsDevice, "Button.png");

            GUITop = Utility.LoadTexture(GraphicsDevice, "GUI Top.png");
            GUIBottom = Utility.LoadTexture(GraphicsDevice, "GUI Bottom.png");
            LifeCounter = Utility.LoadTexture(GraphicsDevice, "Life Counter.png");

            MenuTextBoxTexture = Utility.LoadTexture(GraphicsDevice, "Menu Text Box.png");

            //BrushWalker.GenerateDataFromTexture(BrushTexture);

            Player.Textures = new Texture2D[8];
            Player.Textures[0] = Utility.LoadTexture(GraphicsDevice, "Hero Style 1.png");
            Player.Textures[1] = Utility.LoadTexture(GraphicsDevice, "Hero Style 2.png");
            Player.Textures[2] = Utility.LoadTexture(GraphicsDevice, "Hero Style 3.png");
            Player.Textures[3] = Utility.LoadTexture(GraphicsDevice, "Hero Style 4.png");
            Player.Textures[4] = Utility.LoadTexture(GraphicsDevice, "Hero Style 5.png");
            Player.Textures[5] = Utility.LoadTexture(GraphicsDevice, "Hero Style 6.png");
            Player.Textures[6] = Utility.LoadTexture(GraphicsDevice, "Hero Style 7.png");
            Player.Textures[7] = Utility.LoadTexture(GraphicsDevice, "Hero Style 8.png");

            Player.ArmTextures = new Texture2D[8];
            Player.ArmTextures[0] = Utility.LoadTexture(GraphicsDevice, "Hero Style 1 Arm.png");
            Player.ArmTextures[1] = Utility.LoadTexture(GraphicsDevice, "Hero Style 2 Arm.png");
            Player.ArmTextures[2] = Utility.LoadTexture(GraphicsDevice, "Hero Style 3 Arm.png");
            Player.ArmTextures[3] = Utility.LoadTexture(GraphicsDevice, "Hero Style 4 Arm.png");
            Player.ArmTextures[4] = Utility.LoadTexture(GraphicsDevice, "Hero Style 5 Arm.png");
            Player.ArmTextures[5] = Utility.LoadTexture(GraphicsDevice, "Hero Style 6 Arm.png");
            Player.ArmTextures[6] = Utility.LoadTexture(GraphicsDevice, "Hero Style 7 Arm.png");
            Player.ArmTextures[7] = Utility.LoadTexture(GraphicsDevice, "Hero Style 8 Arm.png");

            var dayNightGradient = Utility.LoadTexture(GraphicsDevice, "DayNightGradient.png");
            DayNightGradientData = new Color[dayNightGradient.Width * dayNightGradient.Height];
            dayNightGradient.GetData<Color>(DayNightGradientData);

            Particles.LoadContent(GraphicsDevice);

            //if (watcher == null)
            //{
            //    watcher = new FileSystemWatcher(Path.Combine(Utility.ContentDirectory));
            //    watcher.Created += (sender, e) => { LoadMainTextures(); };
            //    watcher.Changed += (sender, e) => { LoadMainTextures(); };
            //    watcher.EnableRaisingEvents = true;
            //}
        }

        protected override void BeginRun()
        {
#if WINDOWS
            if (!String.IsNullOrWhiteSpace(GameSettings.Default.AccountName) && !String.IsNullOrWhiteSpace(GameSettings.Default.AccountPassword))
            {
                ThreadPool.QueueUserWorkItem((state) =>
                {
                    WebServiceClient.Login(GameSettings.Default.AccountName, GameSettings.Default.AccountPassword);
                    if (Program.Arguments.autoload)
                        AutoLoad();
                });
            }
#endif
        }

        void CreateMenus()
        {
            LoginMenu = new Menu() { CloseOnEscape = false };
            LoginMenu.Items.Add(new TextureMenuItem() { Texture = TitleLogoTexture });
            LoginMenu.Items.Add(new MenuItem());
            LoginMenu.Items.Add(new MenuItem() { Text = "store.playasteria.com Account Name or Email Address" });
            var loginMenuAccount = new TextBoxMenuItem() { Text = GameSettings.Default.AccountName, IsFocused = true };
            LoginMenu.Items.Add(loginMenuAccount);
            LoginMenu.Items.Add(new MenuItem() { Text = "Password" });
            var loginMenuPassword = new TextBoxMenuItem() { Text = GameSettings.Default.AccountPassword, IsFocused = false, IsPassword = true };
            LoginMenu.Items.Add(loginMenuPassword);
            LoginMenu.Items.Add(new MenuItem());
            LoginMenu.Items.Add(new MenuItem() { Text = "Login", OnClick = tag => { WebServiceClient.Login(loginMenuAccount.TypedText, loginMenuPassword.TypedText); } });
            LoginMenu.Items.Add(new MenuItem() { Text = "Register a New Account", OnClick = tag => { RegisterMenu.Show(); } });
            LoginMenu.Items.Add(new MenuItem());
            LoginMenu.Items.Add(new MenuItem() { Text = "Exit Game", OnClick = tag => { Exit(); } });

            RegisterMenu = new Menu() { CloseOnEscape = false };
            RegisterMenu.Items.Add(new TextureMenuItem() { Texture = TitleLogoTexture });
            RegisterMenu.Items.Add(new MenuItem());
            RegisterMenu.Items.Add(new MenuItem() { Text = "Account Name" });
            var registerAccountName = new TextBoxMenuItem() { Text = GameSettings.Default.AccountName, IsFocused = true };
            RegisterMenu.Items.Add(registerAccountName);
            RegisterMenu.Items.Add(new MenuItem() { Text = "Email Address" });
            var registerEmailAddress = new TextBoxMenuItem() { Text = "", IsFocused = false };
            RegisterMenu.Items.Add(registerEmailAddress);
            RegisterMenu.Items.Add(new MenuItem() { Text = "Password" });
            var registerPassword = new TextBoxMenuItem() { Text = "", IsFocused = false, IsPassword = true };
            RegisterMenu.Items.Add(registerPassword);
            RegisterMenu.Items.Add(new MenuItem() { Text = "Password Confirm" });
            var registerPasswordConfirm = new TextBoxMenuItem() { Text = "", IsFocused = false, IsPassword = true };
            RegisterMenu.Items.Add(registerPasswordConfirm);
            RegisterMenu.Items.Add(new MenuItem());
            RegisterMenu.Items.Add(new MenuItem() { Text = "Register", OnClick = tag => { WebServiceClient.Register(registerAccountName.TypedText, registerEmailAddress.TypedText, registerPassword.TypedText, registerPasswordConfirm.TypedText); } });
            RegisterMenu.Items.Add(new MenuItem());
            RegisterMenu.Items.Add(new MenuItem() { Text = "Exit Game", OnClick = tag => { Exit(); } });

            StartMenu = new Menu() { CloseOnEscape = false };
            StartMenu.Items.Add(new TextureMenuItem() { Texture = TitleLogoTexture });
            //StartMenu.Items.Add(new MenuItem());
            StartMenu.Items.Add(new MenuItem() { Text = "Single Player", OnClick = tag => { ShowPlayerLoadMenu(); } });
            StartMenu.Items.Add(new MenuItem() { Text = "Multi Player", OnClick = tag => { if (WebServiceClient.IsDemo) ShowMessageBox("Sorry, multiplayer is not available in demo mode."); else serverSelectMenu.Show(); } });
            StartMenu.Items.Add(new MenuItem());
            StartMenu.Items.Add(new MenuItem() { Text = "Settings", OnClick = tag => { settingsMenu.Show(); } });
            StartMenu.Items.Add(new MenuItem() { Text = "Controls", OnClick = tag => { ShowHelp(); } });
            StartMenu.Items.Add(new MenuItem() { Text = "Story", OnClick = tag => { DisplayText = BackStory; } });
            StartMenu.Items.Add(new MenuItem() { Text = "Credits", OnClick = tag => { DisplayText = Credits; } });

            StartMenu.Items.Add(new MenuItem()
            {
                Text = "Visit PlayAsteria.com",
                OnClick = tag =>
                    {
#if WINDOWS
                        System.Diagnostics.Process.Start("http://playasteria.com");
#endif
                    }
            });
            StartMenu.Items.Add(new MenuItem());
            StartMenu.Items.Add(new MenuItem() { Text = "Exit Game", OnClick = tag => { Exit(); } });

            pauseMenu = new Menu() { OnEscapeClose = () => { Map.IsPaused = false; } };
            pauseMenu.Items.Add(new MenuItem() { Text = "Help", OnClick = tag => { ShowHelp(); } });
            pauseMenu.Items.Add(new MenuItem() { Text = "Credits", OnClick = tag => { DisplayText = (Player.IsGameCompleted ? EndStory : "") + Credits; } });
            pauseMenu.Items.Add(new MenuItem() { Text = "Change Settings", OnClick = tag => { settingsMenu.Show(); } });
            pauseMenu.Items.Add(new MenuItem());
            pauseMenu.Items.Add(new MenuItem() { Text = "Return To Overworld Start", OnClick = tag => { Player.ReturnToOverworld(); CurrentMenu = null; Map.IsPaused = false; } });
            pauseMenu.Items.Add(new MenuItem());
            pauseMenu.Items.Add(new MenuItem() { Text = "Save", OnClick = tag => { Save(); } });
            pauseMenu.Items.Add(new MenuItem() { Text = "Save and Quit", OnClick = tag => { SaveAndQuit(); } });
            pauseMenu.Items.Add(new MenuItem() { Text = "Quit without Saving", OnClick = tag => { ShowMessageBox("Are you sure you want to quit without saving?", tag2 => { Quit(); }); } });
            pauseMenu.Items.Add(new MenuItem());
            pauseMenu.Items.Add(new MenuItem() { Text = "Return to Game", OnClick = tag => { CurrentMenu = null; Map.IsPaused = false; } });

            multiplayerPauseMenu = new Menu();
            multiplayerPauseMenu.Items.Add(new MenuItem() { Text = "Help", OnClick = tag => { ShowHelp(); } });
            multiplayerPauseMenu.Items.Add(new MenuItem() { Text = "Credits", OnClick = tag => { DisplayText = (Player.IsGameCompleted ? EndStory : "") + Credits; } });
            multiplayerPauseMenu.Items.Add(new MenuItem() { Text = "Change Settings", OnClick = tag => { settingsMenu.Show(); } });
            multiplayerPauseMenu.Items.Add(new MenuItem());
            multiplayerPauseMenu.Items.Add(new MenuItem() { Text = "Return To Overworld Start", OnClick = tag => { GameClient.ReturnToOverworld = true; CurrentMenu = null; } });
            multiplayerPauseMenu.Items.Add(new MenuItem());
            multiplayerPauseMenu.Items.Add(new MenuItem() { Text = "Disconnect", OnClick = tag => { Quit(); } });
            multiplayerPauseMenu.Items.Add(new MenuItem());
            multiplayerPauseMenu.Items.Add(new MenuItem() { Text = "Return to Game", OnClick = tag => { CurrentMenu = null; } });

            var textBoxMenuItem = new TextBoxMenuItem();
            createPlayerMenu = new Menu()
            {
                OnShow = (menu) =>
                    {
#if WINDOWS
                        textBoxMenuItem.Text = Environment.UserName;
#else
                textBoxMenuItem.Text = "";// Microsoft.Xna.Framework.GamerServices.Gamer.SignedInGamers[PlayerIndex.One].Gamertag;
#endif
                    }
            };
            createPlayerMenu.Items.Add(new MenuItem() { Text = "Create Character" });
            createPlayerMenu.Items.Add(new MenuItem());
            createPlayerMenu.Items.Add(new MenuItem() { Text = "Name" });
            createPlayerMenu.Items.Add(textBoxMenuItem);
            createPlayerMenu.Items.Add(new MenuItem() { Text = "Style" });
            createPlayerMenu.Items.Add(new CharacterSelectMenuItem());
            createPlayerMenu.Items.Add(new MenuItem());
            createPlayerMenu.Items.Add(new MenuItem() { Text = "Accept", Tag = textBoxMenuItem, OnClick = tag => { CreatePlayer(((TextBoxMenuItem)tag).TypedText); } });
            createPlayerMenu.Items.Add(new MenuItem() { Text = "Cancel", OnClick = tag => { StartMenu.Show(); } });

            var worldTextBoxMenuItem = new TextBoxMenuItem();
            createWorldMenu = new Menu() { OnShow = (menu) => { worldTextBoxMenuItem.SetText(DateTime.Now.ToString("MM-dd-yyyy HH.mm.ss")); } };
            createWorldMenu.Items.Add(new MenuItem() { Text = "Create World" });
            createWorldMenu.Items.Add(new MenuItem());
            createWorldMenu.Items.Add(new MenuItem() { Text = "World Name" });
            createWorldMenu.Items.Add(worldTextBoxMenuItem);
            createWorldMenu.Items.Add(new MenuItem());
            createWorldMenu.Items.Add(new MenuItem() { Text = "Accept", Tag = worldTextBoxMenuItem, OnClick = tag => { CreateWorld(((TextBoxMenuItem)tag).TypedText); } });
            createWorldMenu.Items.Add(new MenuItem() { Text = "Cancel", OnClick = tag => { ReturnToPreviousMenu(); } });

            settingsMenu = new Menu();
            settingsMenu.Items.Add(new MenuItem() { Text = "Settings" });
            settingsMenu.Items.Add(new MenuItem());
            settingsMenu.Items.Add(new MenuItem() { Text = "Music Volume" });
            settingsMenu.Items.Add(new SliderMenuItem() { Value = GameSettings.Default.MusicVolume, OnChange = value => { GameSettings.Default.MusicVolume = value; } });
            settingsMenu.Items.Add(new MenuItem() { Text = "Sound Effects Volume" });
            settingsMenu.Items.Add(new SliderMenuItem() { Value = GameSettings.Default.SoundEffectVolume, OnChange = value => { GameSettings.Default.SoundEffectVolume = value; } });
            settingsMenu.Items.Add(new MenuItem());
            settingsMenu.Items.Add(new MenuItem() { Text = "Screen Resolution", OnClick = tag => { resolutionMenu.Show(); } });
            settingsMenu.Items.Add(new MenuItem() { Text = "Toggle Fullscreen", OnClick = tag => { toggleFullscreenFlag = true; } });
            if (!WebServiceClient.IsDRMFree)
            {
                settingsMenu.Items.Add(new MenuItem());
                settingsMenu.Items.Add(new MenuItem() { Text = "Log Off PlayAsteria.com Account", OnClick = tag => { WebServiceClient.SignOff(); } });
            }
            settingsMenu.Items.Add(new MenuItem());
            settingsMenu.Items.Add(new MenuItem() { Text = "Return", OnClick = tag => { ReturnToPreviousMenu(); } });

            resolutionMenu = new Menu();
            resolutionMenu.Items.Add(new MenuItem() { Text = "Choose Game Resolution" });
            resolutionMenu.Items.Add(new MenuItem());
            var resolutionList = new ListMenuItem<DisplayMode>() { OnClick = (value) => { SetResolution((DisplayMode)value); } };
            foreach (var displayMode in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes)
            {
                if (displayMode.Format == SurfaceFormat.Color && displayMode.Height >= 768 && displayMode.Width >= 1024)
                {
                    resolutionList.AddItem(String.Format("{0}x{1} ({2:N2})", displayMode.Width, displayMode.Height, displayMode.AspectRatio), displayMode);
                    Console.WriteLine(displayMode.ToString());
                }
            }
            resolutionMenu.Items.Add(resolutionList);
            resolutionMenu.Items.Add(new MenuItem());
            resolutionMenu.Items.Add(new MenuItem() { Text = "Accept", OnClick = tag => { ReturnToPreviousMenu(); } });

            serverSelectMenu = new Menu();
            serverSelectMenu.Items.Add(new MenuItem() { Text = "Server Address" });
            var serverAddress = new TextBoxMenuItem() { Text = GameSettings.Default.Server };
            serverSelectMenu.Items.Add(serverAddress);
            serverSelectMenu.Items.Add(new MenuItem() { Text = "Server Port" });
            var serverPort = new TextBoxMenuItem() { Text = GameSettings.Default.ServerPort, IsFocused = false };
            serverSelectMenu.Items.Add(serverPort);
            serverSelectMenu.Items.Add(new MenuItem());

            serverSelectMenu.Items.Add(new MenuItem() { Text = "Name" });
            if (String.IsNullOrEmpty(GameSettings.Default.LoginName))
            {
#if WINDOWS
                GameSettings.Default.LoginName = Environment.UserName;
#else
                GameSettings.Default.LoginName = "";// Microsoft.Xna.Framework.GamerServices.Gamer.SignedInGamers[0].Gamertag;
#endif
            }
            var loginName = new TextBoxMenuItem() { Text = GameSettings.Default.LoginName, IsFocused = false };
            serverSelectMenu.Items.Add(loginName);
            serverSelectMenu.Items.Add(new MenuItem() { Text = "Password" });
            var password = new TextBoxMenuItem() { Text = GameSettings.Default.Password, IsFocused = false, IsPassword = true };
            serverSelectMenu.Items.Add(password);
            serverSelectMenu.Items.Add(new MenuItem() { Text = "Style" });
            serverSelectMenu.Items.Add(new CharacterSelectMenuItem());
            serverSelectMenu.Items.Add(new MenuItem());
            serverSelectMenu.Items.Add(new MenuItem() { Text = "Login or Create Player", OnClick = tag => { GameClient.Connect(serverAddress.TypedText, serverPort.TypedText, loginName.TypedText, password.TypedText); } });
            serverSelectMenu.Items.Add(new MenuItem() { Text = "Cancel", OnClick = tag => { ReturnToPreviousMenu(); } });

            if (WebServiceClient.IsDRMFree)
                WebServiceClient.IsLoggedIn = true;

            if (!WebServiceClient.IsLoggedIn)
                LoginMenu.Show();
            else
            {
                if (Player == null && Map == null)
                    StartMenu.Show();
            }
        }

        static void ShowHelp()
        {
            DisplayText = Help;

            if (Player != null && Player.IsAdmin)
            {
                DisplayText = Help + AdminHelp;
            }

            displayTextSetTime = DateTime.MinValue;
        }

        internal static void OnExit()
        {
            WebServiceClient.LogOff();

            if (GameClient != null)
            {
                GameClient.Disconnect();
                GameClient = null;
            }
            else
            {
                if (Player != null && Map != null)
                    Save();
            }

            GameSettings.Default.IsFullscreen = graphics.IsFullScreen;
            GameSettings.Default.Save();

#if STEAM
            SteamAPI.Shutdown();
#endif
        }


        void SetResolution(DisplayMode displayMode)
        {
            GameSettings.Default.ResolutionWidth = graphics.PreferredBackBufferWidth = displayMode.Width;
            GameSettings.Default.ResolutionHeight = graphics.PreferredBackBufferHeight = displayMode.Height;
            Lighting.Reset();

            if (GameClient != null)
                GameClient.ServerNeedsResolution = true;
            graphics.ApplyChanges();

            ResolutionWidth = graphics.GraphicsDevice.Viewport.Width;
            ResolutionHeight = graphics.GraphicsDevice.Viewport.Height;

            if (Player != null)
            {
                Player.ClientResolutionWidth = ResolutionWidth;
                Player.ClientResolutionHeight = ResolutionHeight;
            }
        }

        public void AutoLoad()
        {
            // DO NOT LOAD A SAVEGAME BEFORE CONTENT IS LOADED, OR ELSE BOUNDING BOXES WILL BE BROKEN.
            while (!RenderingEnabled)
            {
                Thread.Sleep(0);
            }

            var saveFile = new DirectoryInfo(Utility.SavePath).GetFiles("*.Player").OrderByDescending(f => f.LastWriteTime).FirstOrDefault();
            if (saveFile != null)
                LoadPlayer(saveFile.FullName);

            if (Player != null)
            {
                saveFile = new DirectoryInfo(Utility.SavePath).GetFiles("*.World").OrderByDescending(f => f.LastWriteTime).FirstOrDefault();
                if (saveFile != null)
                {
                    LoadWorld(saveFile.FullName);
                    DisplayText = null;
                }
            }
        }

        void CreateWorld(string name)
        {
            foreach (char c in InvalidFileNameChars)
            {
                name = name.Replace(c.ToString(), "");
            }

            if (name.Length < 3)
            {
                ShowMessageBox("Please enter at least 3 letters.", tag => { ShowWorldLoadMenu(); }, false);
                return;
            }

            if (File.Exists(World.GetSaveFilename(name)))
            {
                ShowMessageBox("There is already a world with that name.", tag => { ShowWorldLoadMenu(); }, false);
                return;
            }

            CurrentMenu = null;
            new Thread(() =>
            {
                World.CreateWorld(name);
                Player.Map = World.GetMap("Start");
                //Player.Map = World.Overworld;
                Map = Player.Map;
                Map.AddEntity(Player);
                Player.Respawn();
                Background.ClearMap();
            }).Start();
        }

        public static void ReturnToPreviousMenu()
        {
            if (CurrentMenu != null)
            {
                if (CurrentMenu.PreviousMenu != null)
                    CurrentMenu = CurrentMenu.PreviousMenu;
                else
                    CurrentMenu = null;
            }
        }

        void ShowPlayerLoadMenu()
        {
            var menu = new Menu();

            menu.Items.Add(new MenuItem() { Text = "Select a Player" });
            menu.Items.Add(new MenuItem());
            var count = 0;
            var listMenuItem = new ListMenuItem<string>() { OnClick = tag => { LoadPlayer((string)tag); } };
            foreach (var saveFile in World.GetPlayerFilenames())
            {
                count++;
                listMenuItem.AddItem(Path.GetFileNameWithoutExtension(saveFile), saveFile);
            }
            menu.Items.Add(listMenuItem);
            if (count == 0)
            {
                createPlayerMenu.Show();
                return;
            }
            menu.Items.Add(new MenuItem());
            menu.Items.Add(new MenuItem() { Text = "Create a New Player", OnClick = tag => { createPlayerMenu.Show(); } });
            menu.Items.Add(new MenuItem());
            menu.Items.Add(new MenuItem() { Text = "Delete", OnClick = tag => { ShowPlayerDeleteMenu(); } });
            menu.Items.Add(new MenuItem() { Text = "Cancel", OnClick = tag => { StartMenu.Show(); } });

            menu.Show();
        }

        private void ShowPlayerDeleteMenu()
        {
            var menu = new Menu();

            menu.Items.Add(new MenuItem() { Text = "Select a Player to Delete" });
            var listMenuItem = new ListMenuItem<string>() { OnClick = tag => { ShowMessageBox("Are you sure you want to delete " + Path.GetFileNameWithoutExtension((string)tag) + "?", tag2 => { DeletePlayer((string)tag); }); } };
            foreach (var saveFile in World.GetPlayerFilenames())
            {
                listMenuItem.AddItem(Path.GetFileNameWithoutExtension(saveFile), saveFile);
            }
            menu.Items.Add(listMenuItem);
            menu.Items.Add(new MenuItem());
            menu.Items.Add(new MenuItem() { Text = "Cancel", OnClick = tag => { ReturnToPreviousMenu(); } });

            menu.Show(false);
        }

        void LoadPlayer(string filename)
        {
            Player = Player.Load(filename);
            ShowWorldLoadMenu();
        }

        void DeletePlayer(string filename)
        {
            World.DeleteFile(filename);
            CurrentMenu = StartMenu;
            ShowPlayerLoadMenu();
        }

        void CreatePlayer(string name)
        {
            foreach (char c in InvalidFileNameChars)
            {
                name = name.Replace(c.ToString(), "");
            }

            if (name.Length < 3)
            {
                ShowMessageBox("Please enter at least 3 letters.", tag => { ShowPlayerLoadMenu(); }, false);
                return;
            }

            if (File.Exists(Player.GetSaveFilename(name)))
            {
                ShowMessageBox("There is already a player with that name.", tag => { ShowPlayerLoadMenu(); }, false);
                return;
            }

            Player = new Player() { Name = name, IsMainPlayer = true, Skin = GameSettings.Default.SelectedSkin };

#if WINDOWS
            if (Program.Arguments.cheat || CurrentKeyboard.IsShift())
#else
            if (CurrentGamePad.Buttons.Back == ButtonState.Pressed || CurrentKeyboard.IsShift())
#endif
            {
                Player.SetSlot(Player.GiveItem(ItemId.TheKeysOfTheKingdom), SlotType.Equipment, 7);
            }

            Player.Save();
            ShowWorldLoadMenu();
        }

        void ShowWorldLoadMenu()
        {
            var menu = new Menu();

            menu.Items.Add(new MenuItem() { Text = "Select a World" });
            menu.Items.Add(new MenuItem());

            var count = 0;

            var listMenuItem = new ListMenuItem<string>() { OnClick = tag => { LoadWorld((string)tag); } };
            foreach (var saveFile in World.GetWorldFilenames())
            {
                count++;
                listMenuItem.AddItem(Path.GetFileNameWithoutExtension(saveFile), saveFile);
            }
            menu.Items.Add(listMenuItem);

            if (count == 0)
            {
                if (createPlayerMenu == null)
                    CreateMenus();
                createWorldMenu.Show();
                return;
            }

            menu.Items.Add(new MenuItem());
            menu.Items.Add(new MenuItem() { Text = "Create a New World", OnClick = tag => { createWorldMenu.Show(); } });
            menu.Items.Add(new MenuItem());
            menu.Items.Add(new MenuItem() { Text = "Delete", OnClick = tag => { ShowWorldDeleteMenu(); } });
            menu.Items.Add(new MenuItem() { Text = "Cancel", OnClick = tag => { StartMenu.Show(); } });
            menu.Show();
        }

        void ShowWorldDeleteMenu()
        {
            var menu = new Menu();

            menu.Items.Add(new MenuItem() { Text = "Select a World to Delete" });
            menu.Items.Add(new MenuItem());
            var listMenuItem = new ListMenuItem<string>() { OnClick = tag => { ShowMessageBox("Are you sure you want to delete " + Path.GetFileNameWithoutExtension((string)tag) + "?", tag2 => { DeleteWorld((string)tag); }); } };
            foreach (var saveFile in World.GetWorldFilenames())
            {
                listMenuItem.AddItem(Path.GetFileNameWithoutExtension(saveFile), saveFile);
            }
            menu.Items.Add(listMenuItem);
            menu.Items.Add(new MenuItem());
            menu.Items.Add(new MenuItem() { Text = "Cancel", OnClick = tag => { ShowPlayerLoadMenu(); } });
            menu.Show(false);
        }

        void LoadWorld(string filename)
        {
            World.Load(filename);
            if (String.IsNullOrEmpty(Player.SaveWorldName) || Player.SaveWorldName == World.Name)
            {
                Player.Map = World.GetMap(Player.SaveMap);
            }
            else
            {
                Player.StartPosition = Vector2.Zero;
            }

            if (Player.Map == null)
                Player.Map = World.GetMap("Start");
            if (Player.Map == null)
                Player.Map = World.Overworld;

            Map = Player.Map;

            Background.ClearMap();

            Map.AddEntity(Player);
            Player.Respawn();

            CurrentMenu = null;
        }

        void DeleteWorld(string filename)
        {
            File.Delete(filename);
            filename = filename.Substring(0, filename.Length - ".World".Length) + ".Overworld.png";
            File.Delete(filename);
            CurrentMenu = StartMenu;
            ShowWorldLoadMenu();
        }

        public static void Save()
        {
            Map.IsPaused = true;
            new Thread(() =>
            {
                CurrentMenu = null;
                Player.Save();
                World.Save();
                if (Map != null)
                    Map.IsPaused = false;
            }).Start();
        }

        public static void SaveAndQuit()
        {
            Map.IsPaused = true;
            CurrentMenu = null;

            if (Player != null)
            {
                if (Player.PlayingSoundEffect != null)
                    Player.PlayingSoundEffect.Stop();

                var player = Player;
                var map = Map;
                Map = null;
                Player = null;

                new Thread(() =>
                {
                    player.Save();
                    World.Save();

                    Quit();
                }).Start();
            }
        }

        public static void Quit()
        {
            ClearModal();
            Map = null;
            NullMap = null;
            ScreenOffset = Point.Zero;
            if (Player != null && Player.PlayingSoundEffect != null)
                Player.PlayingSoundEffect.Stop();

            Player = null;
            CurrentMenu = StartMenu;
            if (GameClient != null)
            {
                GameClient.Disconnect();
                GameClient = null;
            }
        }

        public static void ShowMessageBox(string message, Action<Object> onOkay = null, bool showCancel = true)
        {
            message = Utility.Font.WrapText(message, 900);

            var previousMenu = CurrentMenu;
            var messageBoxMenu = new Menu();
            messageBoxMenu.Items.Add(new MenuItem() { Text = message });
            if (onOkay != null)
                messageBoxMenu.Items.Add(new MenuItem() { Text = "Okay", OnClick = onOkay });
            if (showCancel)
                messageBoxMenu.Items.Add(new MenuItem() { Text = "Cancel", OnClick = (tag) => { previousMenu.Show(false); } });
            messageBoxMenu.Show();
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();

            ResolutionWidth = graphics.GraphicsDevice.Viewport.Width;
            ResolutionHeight = graphics.GraphicsDevice.Viewport.Height;
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
#if !WINDOWS
            if ((World.Device == null || !World.Device.IsConnected) && !Guide.IsVisible)
            {
                StorageDevice.BeginShowSelector(PlayerIndex.One, (result) =>
                {
                    World.Device = StorageDevice.EndShowSelector(result);
                }, null);
            }
#endif

            if (!RenderingEnabled)
            {
                base.Update(gameTime);
                return;
            }

            if (!updateRunning)
            {
                updateRunning = true;

                elapsedTime += gameTime.ElapsedGameTime;
                if (elapsedTime > TimeSpan.FromSeconds(1))
                {
                    elapsedTime -= TimeSpan.FromSeconds(1);
                    FrameRate = frameCounter;
                    frameCounter = 0;
                    Audio.UpdateMusic(gameTime);
                }

                LastKeyboard = CurrentKeyboard;
                LastMouse = CurrentMouse;
                LastGamePad = CurrentGamePad;
                lastScrollWheelValue = CurrentMouse.ScrollWheelValue;

                CurrentKeyboard = Keyboard.GetState();
                CurrentGamePad = GamePad.GetState(PlayerIndex.One, GamePadDeadZone.Circular);

                if (CurrentGamePad.IsPressed(LastGamePad, Buttons.RightStick) && Player != null && !IsModal)
                {
                    IsAimModeEnabled = !IsAimModeEnabled;
                }

                if (Player != null && !IsModal && IsAimModeEnabled)
                {
                    if (CurrentGamePad.ThumbSticks.Right != Vector2.Zero && CurrentGamePad.ThumbSticks.Right.LengthSquared() > 0.5)
                    {
                        aimAngle = new Vector2(CurrentGamePad.ThumbSticks.Right.X, -CurrentGamePad.ThumbSticks.Right.Y).GetAngle();
                    }
                    var screenOffset = player.GetScreenOffset();
                    var position = Vector2.Transform(new Vector2(150, 0), Matrix.CreateRotationZ(aimAngle)) + (Player.Position - screenOffset.ToVector2());
                    Mouse.SetPosition((int)position.X, (int)position.Y);
                }
                else
                {
                    if (CurrentGamePad.ThumbSticks.Right != Vector2.Zero)
                    {
                        Mouse.SetPosition((int)(CurrentMouse.X + CurrentGamePad.ThumbSticks.Right.X * gameTime.ElapsedGameTime.TotalSeconds * 1000), (int)(CurrentMouse.Y + -CurrentGamePad.ThumbSticks.Right.Y * gameTime.ElapsedGameTime.TotalSeconds * 1000));
                    }
                }

                if (IsModal)
                {
                    if (CurrentGamePad.ThumbSticks.Left != Vector2.Zero)
                    {
                        Mouse.SetPosition((int)(CurrentMouse.X + CurrentGamePad.ThumbSticks.Left.X * gameTime.ElapsedGameTime.TotalSeconds * 1000), (int)(CurrentMouse.Y + -CurrentGamePad.ThumbSticks.Left.Y * gameTime.ElapsedGameTime.TotalSeconds * 1000));
                    }
                }

                CurrentMouse = Mouse.GetState();
                if (CurrentGamePad.IsButtonDown(Buttons.LeftTrigger) || CurrentGamePad.IsButtonDown(Buttons.RightTrigger) || IsModal && CurrentGamePad.IsButtonDown(Buttons.A))
                {
                    CurrentMouse = new MouseState
                        (
                            CurrentMouse.X,
                            CurrentMouse.Y,
                            CurrentMouse.ScrollWheelValue,
                            CurrentMouse.LeftButton == ButtonState.Pressed || CurrentGamePad.IsButtonDown(Buttons.RightTrigger) || (IsModal && CurrentGamePad.IsButtonDown(Buttons.A)) ? ButtonState.Pressed : ButtonState.Released,
                            CurrentMouse.MiddleButton,
                            CurrentMouse.RightButton == ButtonState.Pressed || CurrentGamePad.IsButtonDown(Buttons.LeftTrigger) ? ButtonState.Pressed : ButtonState.Released,
                            CurrentMouse.XButton1,
                            CurrentMouse.XButton2
                        );
                }

                if (CurrentKeyboard.IsKeyDown(Keys.RightAlt) && CurrentKeyboard.IsKeyDown(Keys.Enter) || CurrentKeyboard.IsKeyDown(Keys.LeftAlt) && CurrentKeyboard.IsKeyDown(Keys.Enter))
                {
                    toggleFullscreenFlag = true;
                }

                if (Player != null && Player.IsAdmin)
                {
                    if (IsPressed(Keys.F5))
                        showDebug = !showDebug;

                    if (IsPressed(Keys.F6))
                        renderBackground = !renderBackground;

                    if (IsPressed(Keys.F7))
                        renderWall = !renderWall;

                    if (IsPressed(Keys.F8))
                        renderForeground = !renderForeground;

                    if (IsPressed(Keys.F9) && Player != null && Player.IsAdmin)
                        disableLighting = !disableLighting;
                }
                else
                {
                    showDebug = false;
                    renderBackground = true;
                    renderWall = true;
                    renderForeground = true;
                    disableLighting = false;
                }

                if (IsPressed(Keys.F10))
                {
                    graphics.SynchronizeWithVerticalRetrace = !graphics.SynchronizeWithVerticalRetrace;
                    graphics.ApplyChanges();
                    if (Player != null)
                        Player.MessageToClient("Vertical Sync " + (graphics.SynchronizeWithVerticalRetrace ? "Enabled" : "Disabled"), MessageType.System);
                }

                if (IsPressed(Keys.F11))
                {
                    showGui = !showGui;
                }

                if (DisplayText != null)
                {
                    if (CurrentKeyboard.GetPressedKeys().Length > 0 || (CurrentMouse.LeftButton == ButtonState.Pressed && LastMouse.LeftButton == ButtonState.Released) || (CurrentMouse.RightButton == ButtonState.Pressed && LastMouse.RightButton == ButtonState.Released))
                    {
                        if (DateTime.UtcNow > minimumDisplayTextTime)
                        {
                            DisplayText = null;
                            ForceModal = false;
                        }
                    }
                }
                else
                {
                    if (CurrentMenu != null)
                        CurrentMenu.Update(CurrentMouse, LastMouse, CurrentKeyboard, LastKeyboard);
                    else
                        HandlePlayerInput();

                    if (CurrentKeyboard.IsKeyDown(Keys.X))
                    {
                        gameTime = new GameTime(gameTime.TotalGameTime, TimeSpan.FromMilliseconds(10));
                    }

                    if (GameClient != null)
                    {
                        GameClient.Update(gameTime);

                        if (Map != null)
                        {
                            Map.GameTime = Map.GameTime.AddMinutes(gameTime.ElapsedGameTime.TotalSeconds);
                            Map.Now = DateTime.UtcNow;
                            //Map.EntityThink(gameTime); // for client side prediction
                            //Map.FlushEntities();
                        }
                    }
                    else
                    {
                        if (Map != null)
                            World.Update(gameTime);
                    }

                    if (Map != null && Player != null)
                    {
                        ScreenOffset = Player.GetScreenOffset();

                        Particles.Update(Map.Now, gameTime);

                        if (!disableLighting)
                        {
                            //if (!useShaderLighting)
                            Lighting.Update(GraphicsDevice);
                        }

                        if (!wasGameCompleted && Player.IsGameCompleted)
                        {
                            wasGameCompleted = true;
                            DisplayText = EndStory + Credits;
                            ForceModal = true;
                            minimumDisplayTextTime = DateTime.UtcNow.AddSeconds(30);
                        }

#if WINDOWS
                        if (Map.Name != "Start" && WebServiceClient.IsDemo)
                        {
                            ShowMessageBox("Sorry, but you've reached the end of the demo. The game will now save and exit so that you can continue after purchasing.", (x) => { System.Diagnostics.Process.Start("http://store.playasteria.com"); SaveAndQuit(); }, false);
                        }
#endif
                    }
                }

                SteamAPI.RunCallbacks();

                base.Update(gameTime);
                updateRunning = false;
            }
        }

        void HandlePlayerInput()
        {
            if (!IsActive)
                return;

            if (Player != null && Map != null)
            {
                var keyboardHandled = false;

                if (currentControl != null)
                {
                    keyboardHandled = true;
                    currentControl.Update(CurrentMouse, CurrentKeyboard);
                }

                if (IsModal)
                {
                    if (IsPressed(Keys.Escape) || CurrentGamePad.IsPressed(LastGamePad, Buttons.B))
                        ClearModal();
                    return;
                }

                if (Player.IsMapChanged)
                {
                    Map = Player.Map;
                    Player.IsMapChanged = false;
                }

                Player.ClientResolutionWidth = ResolutionWidth;
                Player.ClientResolutionHeight = ResolutionHeight;

                if (chatTextBox != null)
                {
                    keyboardHandled = true;
                    chatTextBox.Update(CurrentMouse, CurrentKeyboard);
                    if (IsPressed(Keys.Escape) || CurrentGamePad.IsPressed(LastGamePad, Buttons.B))
                        chatTextBox = null;
                    else if (IsPressed(Keys.Up))
                    {
                        if (chatLog.Count > 0)
                        {
                            if (chatLogIndex < 0)
                                chatLogIndex = chatLog.Count;
                            chatLogIndex--;
                            if (chatLogIndex < 0)
                                chatLogIndex = 0;
                            chatTextBox.Text = chatLog[chatLogIndex];
                            chatTextBox.End();
                        }
                    }
                    else if (IsPressed(Keys.Down))
                    {
                        if (chatLogIndex >= 0 && chatLogIndex < chatLog.Count)
                        {
                            chatLogIndex++;
                            if (chatLogIndex >= chatLog.Count)
                                chatLogIndex = chatLog.Count - 1;
                            chatTextBox.Text = chatLog[chatLogIndex];
                            chatTextBox.End();
                        }
                    }
                    else if (IsPressed(Keys.Enter) && !CurrentKeyboard.IsKeyDown(Keys.LeftAlt) && !CurrentKeyboard.IsKeyDown(Keys.RightAlt))
                    {
                        SendChat(chatTextBox.Text);
                        chatTextBox = null;
                    }
                }
                else
                {
                    if (IsPressed(Keys.Escape) || CurrentGamePad.IsPressed(LastGamePad, Buttons.Start) && Player.CreativeMode == CreativeMode.None)
                    {
                        Player.ActivePlaceable = null;
                        if (IsShowingInventory)
                            IsShowingInventory = false;
                        else
                        {
                            if (GameClient == null)
                            {
                                Map.IsPaused = true;
                                pauseMenu.Show();
                            }
                            else
                                multiplayerPauseMenu.Show();
                        }
                    }

                    if (IsPressed(Keys.OemQuestion) ||
                        IsPressed(Keys.Divide) ||
                        IsPressed(Keys.Enter) &&
                        !CurrentKeyboard.IsKeyDown(Keys.LeftAlt) &&
                        !CurrentKeyboard.IsKeyDown(Keys.RightAlt))
                    {
                        if (chatTextBox == null)
                        {
                            chatTextBox = new FuchsGUI.TextBox("Chat", "", 70, new Rectangle(10, ResolutionHeight - 45 - ChatBoxTexture.Height, ChatBoxTexture.Width, ChatBoxTexture.Height), null, Utility.SmallFont, Color.White) { Focus = true, Enabled = true };
                            chatLogIndex = -1;
                        }
                        else
                            chatTextBox = null;
                    }

                    if (IsPressed(Keys.Tab) || IsPressed(Keys.I) || IsPressed(Keys.E) || CurrentGamePad.IsPressed(LastGamePad, Buttons.Back))
                    {
                        if (Player.CreativeMode != CreativeMode.None)
                        {
                            IsShowingInventory = true;
                            Player.CreativeMode = CreativeMode.None;
                            if (GameClient != null)
                                GameClient.ClearCreativeMode = true;
                        }
                        else
                        {
                            IsShowingInventory = !IsShowingInventory;
                            if (!IsShowingInventory)
                                ClearModal();
                        }
                    }

                    if (IsPressed(Keys.D1) || CurrentGamePad.IsPressed(LastGamePad, Buttons.X))
                        Player.SelectedActionSlot = 0;
                    if (IsPressed(Keys.D2) || CurrentGamePad.IsPressed(LastGamePad, Buttons.Y))
                        Player.SelectedActionSlot = 1;
                    if (IsPressed(Keys.D3) || CurrentGamePad.IsPressed(LastGamePad, Buttons.DPadUp))
                        Player.SelectedActionSlot = 2;
                    if (IsPressed(Keys.D4) || CurrentGamePad.IsPressed(LastGamePad, Buttons.DPadRight))
                        Player.SelectedActionSlot = 3;
                    if (IsPressed(Keys.D5) || CurrentGamePad.IsPressed(LastGamePad, Buttons.DPadDown))
                        Player.SelectedActionSlot = 4;
                    if (IsPressed(Keys.D6) || CurrentGamePad.IsPressed(LastGamePad, Buttons.DPadLeft))
                        Player.SelectedActionSlot = 5;
                    if (IsPressed(Keys.D7))
                        Player.SelectedActionSlot = 6;
                    if (IsPressed(Keys.D8))
                        Player.SelectedActionSlot = 7;
                    if (IsPressed(Keys.D9))
                        Player.SelectedActionSlot = 8;
                }

                if (Player.ActivePlaceable != null && Player.ActivePlaceable.Inventory != null)
                    IsShowingInventory = true;

                if (CurrentMouse.ScrollWheelValue < lastScrollWheelValue || CurrentGamePad.IsPressed(LastGamePad, Buttons.LeftShoulder))
                    Player.SelectPrevTool();
                else if (CurrentMouse.ScrollWheelValue > lastScrollWheelValue || CurrentGamePad.IsPressed(LastGamePad, Buttons.RightShoulder))
                    Player.SelectNextTool();

                if ((CurrentMouse.LeftButton == ButtonState.Pressed && LastMouse.LeftButton == ButtonState.Released) || CurrentGamePad.IsPressed(LastGamePad, Buttons.A))
                {
                    if (!leftClickHandled && MouseOverSlotType != SlotType.None)
                    {
                        if (CurrentKeyboard.IsShift() && Player.IsPlayerSlot(MouseOverSlotType))
                        {
                            splittingItem = Player.GetItem(MouseOverSlotType, MouseOverSlotNumber);
                            if (splittingItem != null && splittingItem.Amount == 1)
                                ClearModal();
                        }

                        if (splittingItem == null)
                            DoDrag(MouseOverSlotType, MouseOverSlotNumber);

                        leftClickHandled = true;
                        Player.MouseLeftButtonPressed = false;
                    }

                    if (MouseOverSlotType == SlotType.ActionBar)
                    {
                        if (MouseOverSlotNumber < 9)
                            Player.SelectedActionSlot = (byte)MouseOverSlotNumber;
                    }
                }
                else
                {
                    if ((CurrentMouse.LeftButton == ButtonState.Released && LastMouse.LeftButton == ButtonState.Pressed) || LastGamePad.IsPressed(CurrentGamePad, Buttons.A))
                    {
                        if (GameClient != null)
                        {
                            GameClient.DropSlotType = MouseOverSlotType;
                            GameClient.DropSlotNumber = MouseOverSlotNumber;
                        }
                        else
                            Player.DoDrop(MouseOverSlotType, MouseOverSlotNumber);

                        leftClickHandled = false;
                        Player.MouseLeftButtonPressed = false;
                    }
                }

                Player.MousePosition = new Point(CurrentMouse.X + ScreenOffset.X, CurrentMouse.Y + ScreenOffset.Y);

                if (!keyboardHandled)
                {
                    Player.GamePad = CurrentGamePad;
                    Player.Keyboard = CurrentKeyboard;
                }

                if (!leftClickHandled && !IsLeftMouseButtonHandled)
                    Player.MouseLeftButtonPressed = CurrentMouse.LeftButton == ButtonState.Pressed;
                else
                {
                    Player.MouseLeftButtonPressed = false;
                    IsLeftMouseButtonHandled = false;
                }

                Player.MouseRightButtonPressed = CurrentMouse.RightButton == ButtonState.Pressed;
            }

            //if (Player != null)
            //    Utility.DebugMessage2 = String.Format("LeftClickHandled {0} Player.MouseLeftButtonPressed {1}", leftClickHandled, Player.MouseLeftButtonPressed);
        }

        void DoDrag(SlotType slotType, int slotNumber, int amount = 0)
        {
            if (GameClient != null)
            {
                GameClient.DragSlotType = slotType;
                GameClient.DragSlotNumber = slotNumber;
                GameClient.DragAmount = amount;
            }
            else
                Player.DoDrag(slotType, slotNumber, amount);
        }

        static void ClearModal()
        {
            if (isEditingActivePlaceable)
                CloseSign();

            currentControl = null;
            isCraftingBasic = false;

            if (Player != null)
                Player.ActivePlaceable = null;
            if (GameClient != null)
                GameClient.ClearActivePlaceable = true;
            selectedRecipe = null;
            splittingItem = null;
        }

        public void SendChat(string text)
        {
            chatLog.Add(text);
            if (chatLog.Count > 100)
                chatLog.RemoveAt(0);

            if (GameClient != null)
            {
                GameClient.MessageToServer(text);
            }
            else
            {
                World.MessageToServer(Player, text);
            }
        }

        public static bool IsPressed(Keys keys)
        {
            return CurrentKeyboard.IsPressed(LastKeyboard, keys);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            IsMouseOverControl = false;

            lock (this)
            {
                if (!RenderingEnabled)
                {
                    if (!loadingScreenRendered)
                    {
                        DrawLoadingScreen();
                        base.Draw(gameTime);
                        loadingScreenRendered = true;
                        return;
                    }
                    else
                    {
                        while (!RenderingEnabled)
                        {
                            Thread.Sleep(0);
                        }
                    }
                }

                if (toggleFullscreenFlag)
                {
                    graphics.ToggleFullScreen();
                    toggleFullscreenFlag = false;
                }

                frameCounter++;

                GraphicsDevice.Clear(renderBackground ? Color.Black : Color.Cyan);

                if (Map == null)
                {
                    if (NullMap == null)
                    {
                        NullMap = new Map() { Width = 5000, Height = 1000, SeaLevel = 500, LavaLevel = 750, GameTime = DateTime.MinValue.AddHours(17) };
                        Background.ClearMap();
                        menuScreenOffset.X = 0;
                        menuScreenOffset.Y = 250 * Map.BlockHeight;
                        //menuScreenPanSpeed = new Vector2(0.3f, 0.2f);
                        menuScreenPanSpeed = new Vector2(0.15f, 0.1f);

                        // for testing background
                        //menuScreenOffset.Y = NullMap.SeaLevel * Map.BlockHeight - ResolutionHeight;
                        //menuScreenPanSpeed = new Vector2(0.3f, 0f);
                    }

                    menuScreenOffset += menuScreenPanSpeed * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                    ScreenOffset = menuScreenOffset.ToPoint();
                    if (menuScreenPanSpeed.Y > 0)
                    {
                        if (ScreenOffset.Y + ResolutionHeight > NullMap.PixelHeight - 10)
                            menuScreenPanSpeed.Y *= -1;
                    }
                    else
                    {
                        if (ScreenOffset.Y <= 10)
                            menuScreenPanSpeed.Y *= -1;
                    }
                    //NullMap.GameTime = NullMap.GameTime.AddMinutes(gameTime.ElapsedGameTime.TotalSeconds * 65);
                    NullMap.GameTime = NullMap.GameTime.AddMinutes(gameTime.ElapsedGameTime.TotalSeconds * 30);
                }
                if (renderBackground)
                    Background.Draw(spriteBatch, Map ?? NullMap, ScreenOffset.X, ScreenOffset.Y, gameTime);

                if (Map != null && Player != null)
                {
                    wallBatch.Begin();
                    foreBatch.Begin();

                    MapRenderer.Draw(Map, renderWall ? wallBatch : null, renderForeground ? foreBatch : null, ScreenOffset.X, ScreenOffset.Y, ResolutionWidth, ResolutionHeight);
                    wallBatch.End();

                    DrawEntities(spriteBatch);

                    foreBatch.End();

                    Particles.Draw(spriteBatch, ScreenOffset);

                    if (!disableLighting)
                    {
                        Lighting.Draw(GraphicsDevice, spriteBatch, GraphicsDevice.Viewport.Bounds);
                    }

                    GraphicsDevice.SetRenderTarget(null);

                    spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointWrap, null, null);

                    if (Player.CreativeMode == CreativeMode.Material)
                        DrawGrid();

                    Enemy boss = null;
                    lock (Map.Entities)
                    {
                        foreach (var entity in Map.Entities)
                        {
                            if (Player.IsOnScreen(entity))
                            {
                                var enemy = entity as Enemy;
                                if (enemy != null && enemy.Type.IsBoss)
                                    boss = enemy;

                                entity.DrawTop(spriteBatch, ScreenOffset);
                            }
                        }
                    }

                    if (showGui)
                    {
                        //spriteBatch.Draw(MaterialInfo.MaterialsTexture, Vector2.Zero, Color.White);

                        DrawAllSlots();

                        if (Player.IsImmersed)
                        {
                            var airSupply = Player.MaxAirSupply - Player.UsedAirSupply;
                            var breathMessage = String.Format("Air Supply {0:n1}s", airSupply);
                            spriteBatch.DrawRectangle(new Rectangle((int)((ResolutionWidth / 2) - 285), ResolutionHeight - Utility.SmallFont.LineSpacing - Utility.SmallFont.LineSpacing - Utility.EmptySlotTexture.Height - 11, 545, Utility.SmallFont.LineSpacing), Color.FromNonPremultiplied(153, 153, 153, 128));
                            spriteBatch.DrawRectangle(new Rectangle((int)((ResolutionWidth / 2) - 285), ResolutionHeight - Utility.SmallFont.LineSpacing - Utility.SmallFont.LineSpacing - Utility.EmptySlotTexture.Height - 11, (int)((545) * (airSupply / Player.MaxAirSupply)), Utility.SmallFont.LineSpacing), airSupply < 10 ? Color.FromNonPremultiplied(255, 0, 0, 128) : Color.FromNonPremultiplied(170, 228, 255, 128));
                            spriteBatch.DrawString(Utility.SmallFont, breathMessage, new Vector2((int)(ResolutionWidth - Utility.SmallFont.MeasureString(breathMessage).X) / 2, ResolutionHeight - Utility.SmallFont.LineSpacing - Utility.SmallFont.LineSpacing - Utility.EmptySlotTexture.Height - 11), airSupply < 10 ? Color.Red : Color.White);
                        }

                        var healthMessage = String.Format("Energy {0:n0} / {1:n0}", Player.Health, Player.MaxHealth);
                        spriteBatch.DrawRectangle(new Rectangle((int)((ResolutionWidth / 2) - 285), ResolutionHeight - Utility.SmallFont.LineSpacing - Utility.EmptySlotTexture.Height - 8, 545, Utility.SmallFont.LineSpacing), Color.FromNonPremultiplied(153, 153, 153, 128));
                        spriteBatch.DrawRectangle(new Rectangle((int)((ResolutionWidth / 2) - 285), ResolutionHeight - Utility.SmallFont.LineSpacing - Utility.EmptySlotTexture.Height - 8, (int)((545) * ((float)Player.Health / (float)Player.MaxHealth)), Utility.SmallFont.LineSpacing), Player.Health < (Player.MaxHealth * .10) ? Color.FromNonPremultiplied(255, 0, 0, 128) : Color.FromNonPremultiplied(53, 246, 243, 128));
                        spriteBatch.DrawString(Utility.SmallFont, healthMessage, new Vector2((int)(ResolutionWidth - Utility.SmallFont.MeasureString(healthMessage).X) / 2, ResolutionHeight - Utility.SmallFont.LineSpacing - Utility.EmptySlotTexture.Height - 8), Color.White);

                        if (boss != null && !IsShowingInventory)
                        {
                            var bossHealthMessage = String.Format("Boss {0:n0} / {1:n0}", boss.Health, boss.MaxHealth);
                            spriteBatch.DrawRectangle(new Rectangle((int)((ResolutionWidth / 2) - 285), ResolutionHeight - Utility.SmallFont.LineSpacing - Utility.SmallFont.LineSpacing - Utility.SmallFont.LineSpacing - Utility.EmptySlotTexture.Height - 14, 545, Utility.SmallFont.LineSpacing), Color.FromNonPremultiplied(153, 153, 153, 128));
                            spriteBatch.DrawRectangle(new Rectangle((int)((ResolutionWidth / 2) - 285), ResolutionHeight - Utility.SmallFont.LineSpacing - Utility.SmallFont.LineSpacing - Utility.SmallFont.LineSpacing - Utility.EmptySlotTexture.Height - 14, (int)((545) * ((float)boss.Health / (float)boss.MaxHealth)), Utility.SmallFont.LineSpacing), boss.Health < (boss.MaxHealth * .30) ? (boss.Health < (boss.MaxHealth * .10) ? Color.FromNonPremultiplied(217, 0, 0, 128) : Color.FromNonPremultiplied(255, 250, 0, 128)) : Color.FromNonPremultiplied(0, 113, 89, 128));
                            //spriteBatch.DrawString(Utility.MediumFont, "Boss", new Vector2((ResolutionWidth / 2) - 285, ResolutionHeight - Utility.MediumFont.LineSpacing - Utility.MediumFont.LineSpacing - Utility.MediumFont.LineSpacing - Utility.EmptySlotTexture.Height - 10), Color.White);
                            spriteBatch.DrawString(Utility.SmallFont, bossHealthMessage, new Vector2((int)(ResolutionWidth - Utility.SmallFont.MeasureString(bossHealthMessage).X) / 2, ResolutionHeight - Utility.SmallFont.LineSpacing - Utility.SmallFont.LineSpacing - Utility.SmallFont.LineSpacing - Utility.EmptySlotTexture.Height - 14), Color.White);
                        }

                        //var statusMessage = String.Format("{0}\nDepth {1}\nLongitude {2}", Map.GameTime.ToShortTimeString(), Player.Position.Y - Map.SeaLevel, Player.Position.X);
                        //spriteBatch.DrawString(Utility.SmallFont, statusMessage, new Vector2(ResolutionWidth - 5, ResolutionHeight - 5) - Utility.SmallFont.MeasureString(statusMessage), Color.White);

                        if (Player.IsAdmin && !IsShowingInventory)
                        {
                            var position = new Vector2(ResolutionWidth - 130, 5);
                            position = Button.Draw(spriteBatch, "F1 Material", position, () => { SetCreativeMode(CreativeMode.Material); });
                            position = Button.Draw(spriteBatch, "F2 Placeable", position, () => { SetCreativeMode(CreativeMode.Placeable); });
                            position = Button.Draw(spriteBatch, "F3 Enemy", position, () => { SetCreativeMode(CreativeMode.Enemy); });
                            position = Button.Draw(spriteBatch, "F4 Item", position, () => { SetCreativeMode(CreativeMode.Item); });
                            position += new Vector2(0, 10);
                            position = Button.Draw(spriteBatch, "~ God Mode", position, () => { if (GameClient != null) GameClient.ToggleGodMode = !GameClient.ToggleGodMode; else Player.IsGod = !Player.IsGod; });
                            position = Button.Draw(spriteBatch, "F9 Lighting", position, () => { disableLighting = !disableLighting; });
                            //position = Button.Draw(spriteBatch, "Backgrnd", position, () => { renderBackground = !renderBackground; });
                            //position = Button.Draw(spriteBatch, "Walls", position, () => { renderWall = !renderWall; });
                            position = Button.Draw(spriteBatch, "F8 Terrain", position, () => { renderForeground = !renderForeground; });
                            position = Button.Draw(spriteBatch, "F5 Debug Info", position, () => { showDebug = !showDebug; });
                        }

                        if (IsModal)
                        {
                            spriteBatch.DrawRectangle(new Rectangle(0, 0, ResolutionWidth, ResolutionHeight), Color.FromNonPremultiplied(0, 0, 0, 128));
                            MouseOverItemType = null; // clear item tips of everything drawn behind the shadow.
                            MouseOverItemAmount = 0;
                        }

                        spriteBatch.End();

                        if (Player.ActivePlaceable != null && Player.ActivePlaceable.TypeId == ItemId.Sign)
                        {
                            DrawSign();
                        }
                        else
                        {
                            if (Player.ActivePlaceable != null && Player.ActivePlaceable.Inventory == null)
                                DrawCrafting(Player.ActivePlaceable.Type);

                            if (isCraftingBasic)
                                DrawCrafting(null);

                            if (splittingItem != null)
                                DrawSplitting();
                        }

                        spriteBatch.Begin();

                        if (chatTextBox != null)
                        {
                            spriteBatch.Draw(ChatBoxTexture, chatTextBox.Position + new Vector2(-8, -3), Color.White);
                            chatTextBox.Draw(spriteBatch);
                        }

                        DrawMessages();

                        if (showDebug || Player.CreativeMode == CreativeMode.Enemy || Player.CreativeMode == CreativeMode.Placeable)
                        {
                            foreach (var entity in Map.Entities)
                            {
                                entity.DrawDebug(spriteBatch, ScreenOffset);
                            }

                            foreach (var spawn in Map.Spawns)
                            {
                                spawn.DrawDebug(spriteBatch, ScreenOffset);
                            }
                        }

                        spriteBatch.End();

                        DrawItemTip();
                    }
                    else
                        spriteBatch.End();

                    Audio.PlaySoundEvents(Map.SoundEvents);
                }

                spriteBatch.Begin();

                if (showGui)
                {
                    if (!String.IsNullOrEmpty(Utility.StatusMessage))
                        spriteBatch.DrawString(Utility.Font, Utility.StatusMessage, new Vector2((int)(ResolutionWidth - Utility.Font.MeasureString(Utility.StatusMessage).X) / 2, (int)(ResolutionHeight - Utility.Font.LineSpacing) / 2), Color.White);

                    if (showDebug)
                    {
                        Utility.CursorPosition = Vector2.Zero;
                        Utility.WriteLine(spriteBatch, Utility.SmallFont, String.Format("fps: {0}", FrameRate));
                        if (Player != null)
                            Utility.WriteLine(spriteBatch, Utility.SmallFont, String.Format("{0}, {1}, IsJumping {2}", Player.Position.X.ToString(), Player.Position.Y.ToString(), Player.IsJumping));
                        if (Map != null)
                        {
                            foreach (var e in Map.Entities)
                            {
                                var enemy = e as Enemy;
                                if (enemy != null)
                                {
                                    var targetDistance = 0.0;
                                    if (enemy.Target != null)
                                        targetDistance = Math.Ceiling(Math.Sqrt(enemy.TargetDistanceSquared));
                                    Utility.WriteLine(spriteBatch, Utility.SmallFont, String.Format("{0}, Health: {1}, Target Distance: {2,5}, Animation: {3}", enemy.Name, enemy.Health, targetDistance, enemy.Animation));
                                }
                            }
                        }
                    }

                    if (WebServiceClient.IsDemo)
                        spriteBatch.DrawRightAlignedText(Utility.Font, "DEMO MODE", new Vector2(ResolutionWidth - 5, 0));

                    if (DisplayText != null)
                    {
                        //DrawDisplayText(spriteBatch);
                        ScrollDisplayText(spriteBatch, gameTime);
                        DrawLegendStudioLogo();
                    }
                    else
                    {
                        if (CurrentMenu != null)
                        {
                            CurrentMenu.Draw(spriteBatch);
                            DrawLegendStudioLogo();
                        }
                    }

                    if (Map == null)
                        spriteBatch.DrawString(Utility.SmallFont, Utility.Version, new Vector2(10, ResolutionHeight - Utility.SmallFont.LineSpacing), WebServiceClient.IsDRMFree ? Color.Green : Color.Gray);
                }

                DrawCursor();

                spriteBatch.End();

                base.Draw(gameTime);
            }
        }

        void SetCreativeMode(CreativeMode creativeMode)
        {
            if (GameClient != null)
                GameClient.SetCreativeMode = creativeMode;
            else
                Player.CreativeMode = creativeMode;
        }

        void DrawCursor()
        {
            // get current mouse state to reduce lag.
            var currentMouse = Mouse.GetState();

            if (Player != null && Map != null)
            {
                var draggingItem = Player.GetItem(SlotType.Dragging, 0);
                if (draggingItem != null)
                {
                    DrawIcon(new Vector2(currentMouse.X - 26, currentMouse.Y - 26), draggingItem, Color.White);
                    spriteBatch.DrawString(Utility.SmallFont, "(Click to Use, Shift-Click to Drop)", new Vector2(currentMouse.X - 26, currentMouse.Y - 26 + Utility.EmptySlotTexture.Height), Color.LightGray);
                    return;
                }

                if (MouseOverItemType != null || IsModal || IsMouseOverControl)
                    spriteBatch.Draw(Cursor, new Vector2(currentMouse.X - 1, currentMouse.Y - 1), Utility.GetFrameSourceRectangle(22, 32, 2, Utility.Oscillate((int)Player.Age / 100, 6)), Color.White);
                else
                    spriteBatch.Draw(Crosshair, new Vector2(currentMouse.X, currentMouse.Y) + new Vector2(-16, -16), Color.White);
                return;
            }

            if (Cursor != null)
                spriteBatch.Draw(Cursor, new Vector2(currentMouse.X - 1, currentMouse.Y - 1), Utility.GetFrameSourceRectangle(22, 32, 2, Utility.Oscillate((int)(DateTime.UtcNow - runningSince).TotalMilliseconds / 100, 6)), Color.White);
        }

        void DrawMessages()
        {
            var messagePosition = new Vector2(10, ResolutionHeight - 70);
            lock (Player.Messages)
            {
                foreach (var message in Player.Messages)
                {
                    if (chatTextBox == null && (Map.Now - message.Created).TotalSeconds > 15)
                        break;

                    var textSize = Utility.SmallFont.MeasureString(message.Text);
                    messagePosition.Y -= textSize.Y;
                    var color = Color.White;
                    switch (message.Type)
                    {
                        case MessageType.Error:
                            color = ErrorColor;
                            break;
                        case MessageType.System:
                            color = DefaultColor;
                            break;
                    }
                    spriteBatch.DrawString(Utility.SmallFont, message.Text, messagePosition, color);
                }
            }
        }

        static void DrawGrid()
        {
            var gridSize = Map.BlockWidth * 4;
            for (int x = gridSize - ScreenOffset.X % gridSize; x < ResolutionWidth; x += gridSize)
            {
                spriteBatch.DrawRectangle(new Rectangle(x, 0, 1, ResolutionHeight), GridColor);
            }
            for (int y = gridSize - ScreenOffset.Y % gridSize; y < ResolutionWidth; y += gridSize)
            {
                spriteBatch.DrawRectangle(new Rectangle(0, y, ResolutionWidth, 1), GridColor);
            }
        }

        void DrawLoadingScreen()
        {
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin();

            var maxSize = Math.Max(Main.ResolutionWidth, Main.ResolutionHeight);
            spriteBatch.Draw(Starfield, new Rectangle(0, 0, maxSize, maxSize), Color.FromNonPremultiplied(255, 255, 255, 192));

            spriteBatch.Draw(TitleLogoTexture, new Vector2((ResolutionWidth - TitleLogoTexture.Width) / 2, (ResolutionHeight - TitleLogoTexture.Height) / 2), Color.White);
            var loadingText = "Loading...";
            var textSize = Utility.Font.MeasureString(loadingText);
            spriteBatch.DrawString(Utility.Font, loadingText, new Vector2((ResolutionWidth - textSize.X) / 2, (ResolutionHeight - textSize.Y + TitleLogoTexture.Height + 100) / 2), Color.White);

            spriteBatch.DrawString(Utility.SmallFont, Utility.Version, new Vector2(0, ResolutionHeight - Utility.SmallFont.LineSpacing), Color.Gray);

            DrawLegendStudioLogo();

            spriteBatch.End();
        }

        static void DrawLegendStudioLogo()
        {
            spriteBatch.DrawString(Utility.SmallFont, "Created By", new Vector2(ResolutionWidth - LegendStudioLogoTexture.Width, ResolutionHeight - LegendStudioLogoTexture.Height - Utility.SmallFont.LineSpacing), Color.FromNonPremultiplied(244, 235, 152, 255));
            spriteBatch.Draw(LegendStudioLogoTexture, new Vector2(ResolutionWidth - LegendStudioLogoTexture.Width, ResolutionHeight - LegendStudioLogoTexture.Height), Color.White);
        }

        Vector2 DrawWindow(SpriteBatch batch, Vector2 position, int width, int height, string title)
        {
            spriteBatch.DrawExpandButtonTexture(GUITop, position, width, Color.White, 20);
            spriteBatch.DrawString(Utility.SmallFont, title, position + new Vector2(8, 8), Color.White);

            Button.Draw(spriteBatch, "x", position + new Vector2(width - 27, 5), () => { ClearModal(); });

            position += new Vector2(0, GUITop.Height);

            DrawFrame(batch, position, width, height, Color.White);

            position += new Vector2(5, 8);

            return position;
        }

        static void DrawFrame(SpriteBatch batch, Vector2 position, int width, int height, Color color)
        {
            batch.Draw(GUIBottom, position, new Rectangle(0, 0, 5, 5), color);
            batch.Draw(GUIBottom, new Rectangle((int)position.X + 5, (int)position.Y, width - 10, 5), new Rectangle(5, 0, 5, 5), color);
            batch.Draw(GUIBottom, position + new Vector2(width - 5, 0), new Rectangle(52, 0, 5, 5), color);

            batch.Draw(GUIBottom, new Rectangle((int)position.X, (int)position.Y + 5, 14, height - 14 - 5), new Rectangle(0, 5, 14, 16), color);
            batch.Draw(GUIBottom, new Rectangle((int)position.X + 14, (int)position.Y + 5, width - 14 - 5, height - 14 - 5), new Rectangle(5, 5, 1, 1), color);
            batch.Draw(GUIBottom, new Rectangle((int)position.X + width - 5, (int)position.Y + 5, 5, height - 14 - 5), new Rectangle(52, 5, 5, 16), color);

            batch.Draw(GUIBottom, position + new Vector2(0, height - 14), new Rectangle(0, 20, 14, 14), color);
            batch.Draw(GUIBottom, new Rectangle((int)position.X + 14, (int)position.Y + height - 14, width - 14 - 5, 14), new Rectangle(15, 20, 5, 14), color);
            batch.Draw(GUIBottom, position + new Vector2(width - 5, height - 14), new Rectangle(52, 20, 5, 14), color);
        }

        void DrawSplitting()
        {
            spriteBatch.Begin();

            var width = 350;
            var position = new Vector2((ResolutionWidth - width) / 2, (ResolutionHeight - 600) / 2);
            var startPosition = position;

            position = DrawWindow(spriteBatch, new Vector2((ResolutionWidth - width) / 2, (ResolutionHeight - 600) / 2), width, 70, "Splitting " + splittingItem.ToString());

            if (splittingItem == null)
            {
                spriteBatch.End();
                return;
            }

            //position.Y += Utility.SmallFont.LineSpacing + 10;

            spriteBatch.DrawString(Utility.SmallFont, "Amount", position, Color.White);
            var labelSize = Utility.SmallFont.MeasureString("Amount");

            if (currentControl == null)
            {
                currentControl = new FuchsGUI.TextBox("Amount", splittingItem.Amount.ToString(), 8, new Rectangle((int)position.X + (int)labelSize.X + 8, (int)position.Y, 50, ChatBoxTexture.Height), null, Utility.SmallFont, Color.White) { Focus = true, Enabled = true };
            }
            spriteBatch.DrawExpandButtonTexture(ChatBoxTexture, position + new Vector2(labelSize.X, -3), 95, Color.White, 20);
            currentControl.Draw(spriteBatch);

            position.Y += 25;

            position = Button.Draw(spriteBatch, "Split " + splittingItem.ToString(), position, () =>
            {
                int amount;
                if (Int32.TryParse(currentControl.Text, out amount))
                {
                    DoDrag(splittingItem.SlotType, splittingItem.SlotNumber, amount);
                    ClearModal();
                }
            });

            spriteBatch.End();
        }

        void DrawSign()
        {
            const int width = 500;
            const int height = 356;
            spriteBatch.Begin();
            var messageBoxPosition = new Vector2((ResolutionWidth - width) / 2, 20);
            messageBoxPosition = DrawWindow(spriteBatch, messageBoxPosition, width, height, Player.ActivePlaceable.Owner == null ? "Sign" : "Sign - Owned by: " + Player.ActivePlaceable.Owner);

            // the close button of the DrawWindow call above may have cleared the sign.
            if (Player.ActivePlaceable == null)
            {
                spriteBatch.End();
                return;
            }

            if (isEditingActivePlaceable)
            {
                if (currentControl == null)
                {
                    currentControl = new FuchsGUI.TextBox("Sign Editing", Player.ActivePlaceable.Value, 43, new Rectangle((int)messageBoxPosition.X, (int)messageBoxPosition.Y, width - 20, height - 100), null, Utility.SmallFont, Color.White) { Focus = true, Enabled = true, Rows = 16 };
                }
                currentControl.Draw(spriteBatch);
            }
            else
            {
                spriteBatch.DrawString(Utility.SmallFont, Player.ActivePlaceable.Value, messageBoxPosition, Color.White);
            }

            messageBoxPosition.Y += height - 39;
            messageBoxPosition.X += width - 57;
            if (!isEditingActivePlaceable && Player.ActivePlaceable.CanEdit(Player))
                Button.Draw(spriteBatch, "Edit", messageBoxPosition, () => { isEditingActivePlaceable = true; });
            spriteBatch.End();
        }

        static void CloseSign()
        {
            if (isEditingActivePlaceable)
            {
                isEditingActivePlaceable = false;
                Player.ActivePlaceable.Value = currentControl.Text;
                currentControl = null;
            }
            if (GameClient != null)
            {
                GameClient.SetValue = Player.ActivePlaceable.Value;
                GameClient.ClearActivePlaceable = true;
            }
            Player.ActivePlaceable = null;
        }

        void DrawCrafting(ItemType location)
        {
            spriteBatch.Begin();
            var width = (Utility.EmptySlotTexture.Height + slotMargin) * 15 + 10 - slotMargin;
            var position = new Vector2((ResolutionWidth - width) / 2, (ResolutionHeight - 600) / 2);
            var startPosition = position;
            var selectedStartPosition = Vector2.Zero;
            position += new Vector2(5, 5 + GUITop.Height);
            var startX = position.X;

            //spriteBatch.DrawString(Utility.SmallFont, location == null ? "Basic Crafting" : location.ToString() + " Crafting", position, Color.White);
            //Button.Draw(spriteBatch, "x", position + new Vector2(width - 28, -3), () => { ClearModal(); });

            //position.Y += Utility.SmallFont.LineSpacing + 5;

            spriteBatch.DrawString(Utility.SmallFont, "Choose a crafting recipe.", position, Color.White);
            position.Y += Utility.SmallFont.LineSpacing;

            position.X = startX;

            var recipeSlot = 0;
            var locationId = location == null ? ItemId.None : location.Id;
            foreach (var recipe in RecipeBase.Recipes)
            {
                if (recipe.Location == locationId)
                {
                    if (recipeSlot % 15 == 0 && recipeSlot != 0)
                    {
                        position.X = startX;
                        position.Y += Utility.EmptySlotTexture.Height + slotMargin;
                    }

                    if (DrawSlot(recipe.Creates, position, recipe.CanBuild(Player) ? DefaultColor : Color.Gray))
                    {
                        MouseOverItemType = recipe.Creates.Type;
                        MouseOverItemAmount = recipe.CreateAmount;
                        if (CurrentMouse.LeftButton == ButtonState.Pressed)
                        {
                            selectedRecipe = recipe;
                            currentControl = null;
                        }
                    }

                    position.X += Utility.EmptySlotTexture.Width + slotMargin;
                    recipeSlot++;
                }
            }

            position.X = startX;
            position.Y += Utility.EmptySlotTexture.Height + 5;

            if (selectedRecipe != null)
            {
                selectedStartPosition = position;
                position += new Vector2(5, 5);
                startX = position.X;

                var canCreate = selectedRecipe.CanBuild(Player);
                if (DrawSlot(selectedRecipe.Creates, position, canCreate ? DefaultColor : Color.Gray))
                {
                    MouseOverItemType = selectedRecipe.Creates.Type;
                    MouseOverItemAmount = selectedRecipe.CreateAmount;
                }
                position.X += Utility.EmptySlotTexture.Width + slotMargin * 2;
                position.Y += 4;
                spriteBatch.DrawString(Utility.Font, selectedRecipe.Creates.ToString(), position, Color.White);

                position.X = startX;
                position.Y += Utility.EmptySlotTexture.Height + 10;

                spriteBatch.DrawString(Utility.SmallFont, "Requires", position, Color.White);
                position.Y += Utility.SmallFont.LineSpacing;

                foreach (var component in selectedRecipe.Components)
                {
                    if (DrawSlot(component.Type, component.Amount, position, Player.HasItem(component.TypeId, component.Amount) ? DefaultColor : Color.Gray))
                    {
                        MouseOverItemType = component.Type;
                        MouseOverItemAmount = component.Amount;
                    }
                    position.X += Utility.EmptySlotTexture.Width + slotMargin;
                }

                position.X = startX;
                position.Y += Utility.EmptySlotTexture.Height + 5;

                if (canCreate)
                {
                    position.Y += 10;
                    spriteBatch.DrawString(Utility.SmallFont, "Amount", position + new Vector2(0, 3), Color.White);

                    if (currentControl == null)
                    {
                        var maxCreateAmount = Int32.MaxValue;
                        foreach (var component in selectedRecipe.Components)
                        {
                            maxCreateAmount = Math.Min(maxCreateAmount, Player.GetItemAmount(component.TypeId) / component.Amount);
                        }
                        currentControl = new FuchsGUI.Slider("Amount", "1", new Rectangle((int)position.X + 80, (int)position.Y, 200, ChatBoxTexture.Height), Button.ButtonTexture, Utility.SmallFont, Color.White) { Focus = true, Enabled = true, IsAmountSelector = true, MaxAmount = maxCreateAmount, Value = selectedRecipe.Creates.Type.Category == ItemCategory.Component ? 1f : 0f };
                    }
                    currentControl.Draw(spriteBatch);
                    position.Y += 35;

                    position = Button.Draw(spriteBatch, "Create " + selectedRecipe.Creates.ToString(), position, () =>
                    {
                        var slider = (FuchsGUI.Slider)currentControl;
                        Player.CraftAmount = slider.Amount;
                        Player.CraftRecipe = selectedRecipe.Id;
                        ClearModal();
                    });
                }

                position.Y += 5;
            }

            var height = (int)position.Y - (int)startPosition.Y - 25;

            borderBatch.Begin();
            position = DrawWindow(borderBatch, startPosition, width, height, location == null ? "Basic Crafting" : location.ToString() + " Crafting");

            if (selectedRecipe != null)
            {
                DrawFrame(borderBatch, selectedStartPosition, width - 10, height - ((int)selectedStartPosition.Y - (int)startPosition.Y) + 29, Color.White);
                //                borderBatch.DrawRectangle(new Rectangle((int)selectedStartPosition.X, (int)selectedStartPosition.Y, width - 10, height - ((int)selectedStartPosition.Y - (int)startPosition.Y) - 5), Color.FromNonPremultiplied(0, 0, 0, 64));
            }
            borderBatch.End();

            spriteBatch.End();
        }

        public void DrawEntities(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();

            var mousePosition = new Point(ScreenOffset.X + CurrentMouse.X, ScreenOffset.Y + CurrentMouse.Y);
            MouseOverEntity = null;
            lock (Map.Entities)
            {
                foreach (var entity in Map.Entities)
                {
                    if (!(entity is Player))
                        DrawEntity(spriteBatch, entity, mousePosition);
                }

                foreach (var player in Map.Players)
                {
                    DrawEntity(spriteBatch, player, mousePosition);
                }
            }

            spriteBatch.End();
        }

        void DrawEntity(SpriteBatch spriteBatch, Entity entity, Point mousePosition)
        {
            // check if on screen
            if (Player.IsOnScreen(entity))
            {
                entity.Draw(spriteBatch, ScreenOffset);
                if (entity.ContainsPoint(mousePosition) && entity != Player)
                    MouseOverEntity = entity;
            }

            Audio.PlaySoundEvents(entity.SoundEvents);

            if (entity.PlayingSound != entity.PlayingSoundEffectCurrentId)
            {
                if (entity.PlayingSoundEffect != null)
                {
                    entity.PlayingSoundEffect.Stop();
                    entity.PlayingSoundEffect = null;
                    entity.PlayingSoundEffectCurrentId = Sound.None;
                }

                if (entity.PlayingSound != Sound.None)
                {
                    entity.PlayingSoundEffect = Audio.PlayLoopedSound(entity.PlayingSound);
                    entity.PlayingSoundEffectCurrentId = entity.PlayingSound;
                }
            }
        }

        void DrawAllSlots()
        {
            MouseOverItemType = null;
            MouseOverItemAmount = 0;
            MouseOverSlotType = SlotType.None;
            MouseOverSlotNumber = -1;

            var position = new Vector2(10, 10);
            var chestPosition = position;
            if (!IsModal)
            {
                switch (Player.CreativeMode)
                {
                    case CreativeMode.None:
                        if (IsShowingInventory)
                        {
                            var startOffset = inventoryPage * 90;
                            position = DrawSlotSet(String.Format("Hypercube of Holding ({0}/10, Shift-Click to Split)", inventoryPage + 1), position, Player.SlotIndex[SlotType.Inventory].Skip(startOffset).Take(90), SlotType.Inventory, -1, startOffset);
                            position = DrawSlotSet("Trash", position, Player.SlotIndex[SlotType.Trash], SlotType.Trash);
                            chestPosition = new Vector2(10, position.Y + Utility.EmptySlotTexture.Height + 5);
                            chestPosition += new Vector2(0, 10);
                            chestPosition = Button.Draw(spriteBatch, "Basic Item Crafting", chestPosition, () => { isCraftingBasic = true; });
                            chestPosition += new Vector2(0, 10);

                            position = new Vector2((Utility.EmptySlotTexture.Width + slotMargin) * 15 + 10, 10 + Utility.SmallFont.LineSpacing);
                            position = Button.Draw(spriteBatch, "Sort", position, () => { Player.SortInventory = true; });
                            position = Button.Draw(spriteBatch, "Prev", position, () => { inventoryPage--; }, inventoryPage > 0);
                            position = Button.Draw(spriteBatch, "Next", position, () => { inventoryPage++; }, inventoryPage < 9);
                        }
                        break;
                    case CreativeMode.Material:
                        position = DrawSlotSet("Materials (Left = Place, Shift-Left = Place Wall, Right = Erase, Shift-Right = Erase Wall, C = Copy, V = Paste, Shift-V = Paste Wall)", position, (from m in MaterialInfo.MaterialTypes where m.IsVisible select m.Item), SlotType.MaterialPalette, Player.Selected);
                        break;
                    case CreativeMode.Enemy:
                        position = DrawSlotSet("Enemies (Left = Place Spawn, Right = Remove, Shift = Place Instance", position, EnemyBase.Items, SlotType.MaterialPalette, Player.Selected);
                        break;
                    case CreativeMode.Placeable:
                        position = DrawSlotSet("Placeables (Left = Place, Right = Remove, Shift-Left = Move Closest Placeable to Mouse Position)", position, (from i in ItemBase.Items where i.Type.Category == ItemCategory.Placable select i), SlotType.MaterialPalette, Player.Selected);
                        break;
                    case CreativeMode.Item:
                        position = DrawSlotSet("Item Pickups", position, ItemBase.Items, SlotType.MaterialPalette, Player.Selected);
                        break;
                }

                if (IsShowingInventory)
                {
                    // draw chest
                    if (Player.ActivePlaceable != null)
                    {
                        position = chestPosition;

                        //position.X = 10;
                        //position.Y += EmptySlotTexture.Height + slotMargin;

                        spriteBatch.DrawString(Utility.SmallFont, Player.ActivePlaceable.Type.Name, position, Color.White);
                        position.Y += 15;

                        if (Player.ActivePlaceable.Inventory != null)
                        {
                            for (int slot = 0; slot < Player.ActivePlaceable.Inventory.Length; slot++)
                            {
                                if (slot % 8 == 0 && slot != 0)
                                {
                                    position.X = 10;
                                    position.Y += Utility.EmptySlotTexture.Height + slotMargin;
                                }

                                var item = Player.ActivePlaceable.Inventory[slot];
                                if (DrawSlot(item, position, Color.White))
                                {
                                    MouseOverItemType = item == null ? null : item.Type;
                                    MouseOverItemAmount = item == null ? 0 : item.Amount;
                                    MouseOverSlotType = SlotType.Chest;
                                    MouseOverSlotNumber = slot;
                                }

                                position.X += Utility.EmptySlotTexture.Width + slotMargin;
                            }
                        }
                    }

                    // draw equipment
                    spriteBatch.DrawRightAlignedText(Utility.SmallFont, "Equipment", new Vector2(ResolutionWidth - 10, 10));
                    position = new Vector2(ResolutionWidth - 10 - Utility.EmptySlotTexture.Width, 25);
                    for (int slot = 0; slot < Player.EquipmentSlots; slot++)
                    {
                        var item = Main.Player.GetItem(SlotType.Equipment, slot);
                        if (DrawSlot(item, position, Color.White))
                        {
                            MouseOverSlotType = SlotType.Equipment;
                            MouseOverSlotNumber = slot;
                            MouseOverItemType = item == null ? null : item.Type;
                            MouseOverItemAmount = item == null ? 0 : item.Amount;
                        }

                        if (slot <= 4)
                            spriteBatch.DrawRightAlignedText(Utility.SmallFont, ((EquipmentSlot)slot).ToString(), new Vector2(position.X - 3, position.Y + 12));
                        if (slot == 8)
                            spriteBatch.DrawRightAlignedText(Utility.SmallFont, "Keys", new Vector2(position.X - 3, position.Y + 12));

                        position.Y += Utility.EmptySlotTexture.Height + slotMargin;
                    }

                    position.X = ResolutionWidth - 10;
                    spriteBatch.DrawRightAlignedText(Utility.SmallFont, String.Format("Energy {0:n0}", Player.MaxHealth), position);
                    position.Y += Utility.SmallFont.LineSpacing;
                    spriteBatch.DrawRightAlignedText(Utility.SmallFont, String.Format("Regeneration {0:n0}", Player.Regeneration), position);
                    position.Y += Utility.SmallFont.LineSpacing;
                    spriteBatch.DrawRightAlignedText(Utility.SmallFont, String.Format("Defense {0:n0}", Player.Defense), position);
                    position.Y += Utility.SmallFont.LineSpacing;
                    spriteBatch.DrawRightAlignedText(Utility.SmallFont, String.Format("Speed {0:n0}", Player.MaxRunSpeed), position);
                    position.Y += Utility.SmallFont.LineSpacing;
                    spriteBatch.DrawRightAlignedText(Utility.SmallFont, String.Format("Jump {0:n0}", Player.MaxJumpLength), position);
                }
            }

            // draw action bar
            var actionSlotPosition = new Vector2((ResolutionWidth / 2) - ((Utility.EmptySlotTexture.Width + 5) * 5), ResolutionHeight - Utility.EmptySlotTexture.Height - 5);
            DrawSlotSet(null, actionSlotPosition, Player.SlotIndex[SlotType.ActionBar], SlotType.ActionBar, Player.SelectedActionSlot, 0,
                (slot, slotPosition, selected) =>
                {
                    if (slot < 9)
                    {
                        spriteBatch.DrawString(Utility.SmallFont, (slot + 1).ToString(), slotPosition + new Vector2(3, Utility.EmptySlotTexture.Height - 18), selected ? SelectedColor : DefaultColor);
                    }
                    else
                    {
                        if (Player.SlotIndex[SlotType.ActionBar][9] == null)
                            spriteBatch.DrawString(Utility.SmallFont, "Right\nMouse", slotPosition + new Vector2(3, 10), DefaultColor);
                    }
                });

            var inventoryButtonPosition = new Vector2((ResolutionWidth / 2) + (int)((Utility.EmptySlotTexture.Width + 5) * 5) - 15, ResolutionHeight - Utility.SmallFont.LineSpacing - 15);
            Button.Draw(spriteBatch, "Inventory", inventoryButtonPosition, () => { IsShowingInventory = !IsShowingInventory; });

            if (Map.ExtraLives > 0)
            {
                inventoryButtonPosition -= new Vector2(0, LifeCounter.Height + 5);
                spriteBatch.Draw(LifeCounter, inventoryButtonPosition, Color.White);
                inventoryButtonPosition += new Vector2(LifeCounter.Width + 5, 2);
                spriteBatch.DrawString(Utility.MediumFont, Player.Lives.ToString(), inventoryButtonPosition, DefaultColor);
            }

            // draw brushes
            if (Player.CreativeMode == CreativeMode.Material || (Player.CreativeMode == CreativeMode.None && Player.Tool != null && Player.Tool.Type.Material != Material.None))
            {
                position.X = (ResolutionWidth - (Utility.EmptySlotTexture.Width + slotMargin) * 14) / 2;
                position.Y = ResolutionHeight - (Utility.EmptySlotTexture.Height * 2) - 22;

                int slot = 0;
                foreach (var brush in Utility.GetEnumValues<Brush>())
                {
                    if (slot % 15 == 0 && slot != 0)
                    {
                        position.X = 10;
                        position.Y += Utility.EmptySlotTexture.Height + slotMargin;
                    }

                    if (brush != Brush.None && (brush != Brush.Mega || Player.CreativeMode == CreativeMode.Material))
                    {
                        if (DrawSlot("", position, Player.SelectedBrush == brush))
                        {
                            MouseOverSlotType = SlotType.BrushPalette;
                            MouseOverSlotNumber = slot;
                        }

                        DrawBrushIcon(brush, position + new Vector2(2, 2));

                        position.X += Utility.EmptySlotTexture.Width + slotMargin;
                        slot++;
                    }
                }
            }
        }

        Vector2 DrawSlotSet(string name, Vector2 position, IEnumerable<Item> items, SlotType type, int selectedSlot = -1, int slotOffset = 0, Action<int, Vector2, bool> drawLabel = null)
        {
            if (name != null)
            {
                spriteBatch.DrawString(Utility.SmallFont, name, position, Color.White);
                position.Y += Utility.SmallFont.LineSpacing;
            }

            int slot = 0;
            foreach (var item in items)
            {
                if (slot % 15 == 0 && slot != 0)
                {
                    position.X = 10;
                    position.Y += Utility.EmptySlotTexture.Height + slotMargin;
                }

                var selected = false;
                if (item != null)
                    selected = item.SlotNumber == selectedSlot;
                else
                    selected = slot == selectedSlot;

                if (drawLabel != null)
                    drawLabel(slot, position, selected);

                if (DrawSlot(item, position, selected ? SelectedColor : DefaultColor))
                {
                    MouseOverSlotType = type;
                    MouseOverSlotNumber = item == null ? slot + slotOffset : item.SlotNumber;
                    MouseOverItemType = item == null ? null : item.Type;
                    MouseOverItemAmount = item == null ? 0 : item.Amount;
                }

                position.X += Utility.EmptySlotTexture.Width + slotMargin;
                slot++;
            }

            if (slot % 15 == 0 && slot != 0)
            {
                position.X = 10;
                position.Y += Utility.EmptySlotTexture.Height + slotMargin;
            }

            return position;
        }

        void DrawItemTip()
        {
            Utility.CursorPosition = new Vector2(CurrentMouse.X + 20, CurrentMouse.Y + 20);

            if (Utility.CursorPosition.X > ResolutionWidth - 330)
                Utility.CursorPosition.X = ResolutionWidth - 330;
            if (Utility.CursorPosition.Y > ResolutionHeight - 120)
                Utility.CursorPosition.Y = ResolutionHeight - 120;

            if (MouseOverItemType != null)
            {
                spriteBatch.Begin();

                var startPosition = Utility.CursorPosition;

                var maxWidth = spriteBatch.WriteLine(Utility.Font, Item.ToLongString(MouseOverItemType.Name, MouseOverItemAmount), MouseOverItemType.Rarity.GetColor());

                var width = spriteBatch.WriteLine
                    (
                        Utility.SmallFont,
                        (MouseOverItemType.Tier > 0) ? String.Format("Tier {0} {1}", MouseOverItemType.Tier, MouseOverItemType.CategoryString) : MouseOverItemType.CategoryString,
                        Color.LightGray
                    );
                maxWidth = Math.Max(maxWidth, width);

                if (!String.IsNullOrEmpty(MouseOverItemType.ActualDescription))
                {
                    width = spriteBatch.WriteLine(Utility.SmallFont, MouseOverItemType.ActualDescription, Color.LimeGreen);
                    maxWidth = Math.Max(maxWidth, width);
                }

                if (MouseOverItemType.ComponentFor != null)
                {
                    width = spriteBatch.WriteLine(Utility.SmallFont, "Component For", Color.White);
                    maxWidth = Math.Max(maxWidth, width);

                    var result = new StringBuilder();
                    var count = 0;
                    foreach (var recipe in MouseOverItemType.ComponentFor)
                    {
                        if (result.Length > 0)
                            result.Append(", ");

                        if (count > 0 && count % 4 == 0)
                            result.Append("\n");

                        result.Append(recipe);
                        count++;
                    }
                    width = spriteBatch.WriteLine(Utility.SmallFont, result.ToString(), Color.White);
                    maxWidth = Math.Max(maxWidth, width);
                }

                if (MouseOverSlotType == SlotType.Inventory || MouseOverSlotType == SlotType.Trash || MouseOverSlotType == SlotType.ActionBar || MouseOverSlotType == SlotType.Equipment)
                {
                    width = spriteBatch.WriteLine(Utility.SmallFont, "Ctrl+Click to (un)equip.", DisabledColor);
                    maxWidth = Math.Max(maxWidth, width);
                }

                borderBatch.Begin();
                borderBatch.DrawRectangle(new Rectangle((int)startPosition.X - 5, (int)startPosition.Y - 5, (int)maxWidth + 10, (int)Utility.CursorPosition.Y - (int)startPosition.Y + 5), Color.FromNonPremultiplied(64, 64, 64, 200));
                borderBatch.End();

                spriteBatch.End();

                return;
            }

            if (MouseOverEntity != null)
            {
                spriteBatch.Begin();

                var placeable = MouseOverEntity as Placeable;
                var text = MouseOverEntity.ToString();
                if (text != null && (placeable == null || placeable.Type.ShowToolTip || Player.CreativeMode != CreativeMode.None))
                {
                    if (placeable != null)
                    {
                        spriteBatch.WriteLine(Utility.Font, text);
                        spriteBatch.WriteLine(Utility.SmallFont, placeable.Type.Description);
                        if (placeable.Type.OnActivate != null && Player.WithinRange(placeable, placeable.Type.PlaceAndActivateRange))
                            spriteBatch.WriteLine(Utility.SmallFont, "Press Up or W to Activate", Color.LimeGreen);

                        //if (placeable.Type.OnItemTip != null)
                        //    placeable.Type.OnItemTip(placeable, spriteBatch);

                        if (Player.CreativeMode != CreativeMode.None)
                        {
                            if (placeable.Owner != null)
                                spriteBatch.WriteLine(Utility.SmallFont, "Owner: " + placeable.Owner, Color.White);
                            if (placeable.Name != null)
                                spriteBatch.WriteLine(Utility.SmallFont, "Name: " + placeable.Name, Color.White);
                            if (placeable.Value != null)
                                spriteBatch.WriteLine(Utility.SmallFont, "Value: " + placeable.Value, Color.White);
                            if (placeable.Flag)
                                spriteBatch.WriteLine(Utility.SmallFont, "Flag Set", Color.White);
                            if (placeable.Inventory != null)
                            {
                                foreach (var item in placeable.Inventory)
                                {
                                    if (item != null)
                                        spriteBatch.WriteLine(Utility.SmallFont, item.ToLongString(), Color.LightGray);
                                }
                            }
                        }
                        else
                        {
                            if (placeable.TypeId == ItemId.Hypercube || (placeable.TypeId == ItemId.PersonalHypercube && placeable.Owner == player.Name))
                            {
                                foreach (var item in placeable.Inventory)
                                {
                                    if (item != null)
                                        spriteBatch.WriteLine(Utility.SmallFont, item.ToLongString(), Color.LightGray);
                                }
                            }
                        }
                    }
                    else
                    {
                        var player = MouseOverEntity as Player;
                        if (player != null)
                        {
                            if (!player.IsMainPlayer)
                            {
                                spriteBatch.WriteLine(Utility.Font, text);
                                spriteBatch.WriteLine(Utility.SmallFont, String.Format("{0:n0} / {1:n0}", player.Health, player.MaxHealth), Color.LimeGreen);
                            }
                        }
                        else
                        {
                            var enemy = MouseOverEntity as Enemy;
                            if (enemy != null && Player.CreativeMode != CreativeMode.None)
                            {
                                spriteBatch.WriteLine(Utility.Font, text, Color.Red);
                                spriteBatch.WriteLine(Utility.SmallFont, String.Format("Tier {0}", enemy.Tier), Color.LightGray);
                                spriteBatch.WriteLine(Utility.SmallFont, String.Format("{0:n0} / {1:n0}", enemy.Health, enemy.MaxHealth), Color.OrangeRed);

                                spriteBatch.WriteLine(Utility.SmallFont, String.Format("Direction {0}", enemy.Direction), Color.LightGray);
                                spriteBatch.WriteLine(Utility.SmallFont, String.Format("Speed {0}", enemy.Speed), Color.LightGray);
                                spriteBatch.WriteLine(Utility.SmallFont, String.Format("Facing Left {0}", enemy.IsFacingLeft), Color.LightGray);
                                spriteBatch.WriteLine(Utility.SmallFont, String.Format("Animation {0}", enemy.Animation), Color.LightGray);

                                spriteBatch.WriteLine(Utility.SmallFont, String.Format("Distance from target {0}", Math.Ceiling(Math.Sqrt(enemy.TargetDistanceSquared))), Color.LightGray);
                            }
                            else
                            {
                                var pickup = MouseOverEntity as Pickup;
                                if (pickup != null)
                                {
                                    spriteBatch.WriteLine(Utility.Font, Item.ToLongString(text, pickup.Amount), pickup.Type.Rarity.GetColor());
                                }
                                //else
                                //    spriteBatch.WriteLine(Utility.Font, text);
                            }
                        }
                    }
                }

                spriteBatch.End();
            }
        }

        public static void DrawIcon(Vector2 location, Item item, Color color)
        {
            if (item == null)
                return;

            DrawIcon(location, item.Type, item.Amount, color);
        }

        public static void DrawIcon(Vector2 location, ItemType itemType, int amount, Color color)
        {
            if (itemType == null)
                return;

            if (itemType.IsTextureMissing)
            {
                spriteBatch.Draw(Utility.FilledSlotTexture, location, Color.White);
                spriteBatch.DrawString(Utility.SmallFont, Utility.SmallFont.WrapText(itemType.ToString(), 50), location + new Vector2(0, -2), itemType.Rarity.GetColor());
            }
            else
                itemType.DrawIcon(spriteBatch, location, color);

            if (amount > 1)
            {
                string amountString;
                if (amount < 1000)
                    amountString = amount.ToString();
                else if (amount < 10000)
                    amountString = String.Format("{0:N1}k", amount / 1000);
                else
                    amountString = String.Format("{0:N0}k", amount / 1000);

                spriteBatch.DrawString(Utility.SmallFont, amountString, location + new Vector2(0, 31), Color.White);
            }
        }

        public bool DrawSlot(Item item, Vector2 position, Color highlightColor)
        {
            if (item == null)
                return DrawSlot(null, 0, position, highlightColor);
            return DrawSlot(item.Type, item.Amount, position, highlightColor);
        }

        public bool DrawSlot(ItemType itemType, int amount, Vector2 position, Color highlightColor)
        {
            spriteBatch.Draw(Utility.EmptySlotTexture, position, highlightColor);

            if (highlightColor == SelectedColor)
            {
                spriteBatch.Draw(Utility.EmptySlotTexture, new Rectangle((int)position.X - 2, (int)position.Y - 2, Utility.EmptySlotTexture.Width + 4, Utility.EmptySlotTexture.Height + 4), SelectedColor);
                highlightColor = Color.White;
            }

            if (highlightColor == DefaultColor)
                highlightColor = Color.White;
            if (itemType != null)
                DrawIcon(position + new Vector2(3, 3), itemType, amount, highlightColor);

            if (CurrentMouse.X >= position.X && CurrentMouse.X <= position.X + Utility.EmptySlotTexture.Width && CurrentMouse.Y >= position.Y && CurrentMouse.Y <= position.Y + Utility.EmptySlotTexture.Height)
                return true;
            return false;
        }

        public bool DrawSlot(String text, Vector2 position, bool highlight = false)
        {
            var result = DrawSlot(null, 0, position, highlight ? SelectedColor : DefaultColor);

            if (highlight)
                spriteBatch.Draw(Utility.EmptySlotTexture, new Rectangle((int)position.X - 2, (int)position.Y - 2, Utility.EmptySlotTexture.Width + 4, Utility.EmptySlotTexture.Height + 4), SelectedColor);

            spriteBatch.DrawString(Utility.SmallFont, text, position + new Vector2(8, 8) - new Vector2(4, 4), Color.White);
            return result;
        }

        void DrawBrushIcon(Brush brush, Vector2 position)
        {
            spriteBatch.Draw(Utility.BrushTexture, new Rectangle((int)position.X + 1, (int)position.Y + 1, ItemType.IconSize, ItemType.IconSize), new Rectangle(16 * (int)brush, 0, 16, 16), Color.White);
        }

        public void ScrollDisplayText(SpriteBatch spriteBatch, GameTime time)
        {
            var y = -50 + ResolutionHeight + ((float)(displayTextSetTime - DateTime.UtcNow).TotalSeconds * 50);
            var textSize = Utility.MediumFont.MeasureString(DisplayText);
            var x = (ResolutionWidth - textSize.X) / 2;

            var maxScrollY = ResolutionHeight - scrollDisplayTextMarginHeight - textSize.Y;
            if (textSize.Y < ResolutionHeight)
                maxScrollY = (ResolutionHeight - textSize.Y) / 2;

            if (y < maxScrollY)
                y = maxScrollY;

            //spriteBatch.DrawString(Utility.Font, DisplayText, new Vector2(x, y), Color.White);

            foreach (var line in DisplayText.Split('\n'))
            {
                var color = Color.White;
                if (y > ResolutionHeight - scrollDisplayTextMarginHeight)
                {
                    var alpha = (100 - (y - (ResolutionHeight - scrollDisplayTextMarginHeight))) / 100;
                    if (alpha > 0)
                    {
                        color = Color.FromNonPremultiplied(255, 255, 255, (byte)(256 * alpha));
                    }
                    else
                        color = Color.Transparent;
                }

                spriteBatch.DrawString(Utility.MediumFont, line, new Vector2(x, y), color);
                y += Utility.MediumFont.LineSpacing;
            }
        }
    }
}