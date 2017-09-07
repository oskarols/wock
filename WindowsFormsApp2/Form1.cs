using System;
using System.Windows.Forms;
using wock.Models;
using ModifierKeys = wock.Models.ModifierKeys;
namespace WindowsFormsApp2
{
    public partial class Form1 : Form
    {
        KeyboardHook fullscreenHook = new KeyboardHook();
        KeyboardHook gotoVisualStudio = new KeyboardHook();
        KeyboardHook sendToNextScreenHook = new KeyboardHook();
        KeyboardHook gotoChrome = new KeyboardHook();
        KeyboardHook gotoSpotify = new KeyboardHook();
        KeyboardHook gotoCmder = new KeyboardHook();
        KeyboardHook gotoVisualStudioCode = new KeyboardHook();
        KeyboardHook gotoExplorer = new KeyboardHook();

        KeyboardHook gotoLeftHalfOfScreen = new KeyboardHook();
        KeyboardHook gotoRightHalfOfScreen = new KeyboardHook();

        public Form1()
        {
            InitializeComponent();

            var handlers = new HotkeyHandlers();

            // TODO: find better alternative..
            // as unique as possible, to not clash with other key combos
            var hyperKey = 
                wock.Models.ModifierKeys.Control | 
                wock.Models.ModifierKeys.Alt | 
                wock.Models.ModifierKeys.Shift | 
                wock.Models.ModifierKeys.Win;

            // VSC
            gotoVisualStudioCode.KeyPressed += 
                new EventHandler<KeyPressedEventArgs>(handlers.hook_gotoVisualStudioCode_KeyPressed);
            // register the control + alt + F12 combination as hot key.
            gotoVisualStudioCode.RegisterHotKey(hyperKey, Keys.D1);

            // CMDER
            gotoCmder.KeyPressed +=
                new EventHandler<KeyPressedEventArgs>(handlers.hook_gotoCmder_KeyPressed);
            // register the control + alt + F12 combination as hot key.
            gotoCmder.RegisterHotKey(hyperKey, Keys.D2);

            // CHROME
            gotoChrome.KeyPressed +=
                new EventHandler<KeyPressedEventArgs>(handlers.hook_gotoChrome_KeyPressed);
            // register the control + alt + F12 combination as hot key.
            gotoChrome.RegisterHotKey(hyperKey, Keys.D3);

            // VS
            gotoVisualStudio.KeyPressed +=
                new EventHandler<KeyPressedEventArgs>(handlers.hook_visualstudio_KeyPressed);
            // register the control + alt + F12 combination as hot key.
            gotoVisualStudio.RegisterHotKey(hyperKey, Keys.D4);

            // SPOTIFY
            gotoSpotify.KeyPressed +=
                new EventHandler<KeyPressedEventArgs>(handlers.hook_gotoSpotify_KeyPressed);
            // register the control + alt + F12 combination as hot key.
            gotoSpotify.RegisterHotKey(hyperKey, Keys.D6);

            // EXPLORER
            gotoExplorer.KeyPressed +=
                new EventHandler<KeyPressedEventArgs>(handlers.hook_gotoExplorer_KeyPressed);
            gotoExplorer.RegisterHotKey(hyperKey, Keys.Z);

            /// FULLSCREEN 
            fullscreenHook.KeyPressed +=
                new EventHandler<KeyPressedEventArgs>(handlers.hook_fullscreen_KeyPressed);
            // register the control + alt + F12 combination as hot key.
            fullscreenHook.RegisterHotKey(hyperKey, Keys.F);

            // NEXT SCREEN
            sendToNextScreenHook.KeyPressed +=
                new EventHandler<KeyPressedEventArgs>(handlers.hook_nextWindow_KeyPressed);
            // register the control + alt + F12 combination as hot key.
            sendToNextScreenHook.RegisterHotKey(hyperKey, Keys.E);

            // LEFT HALF OF CURRENT SCREEN
            gotoLeftHalfOfScreen.KeyPressed +=
                new EventHandler<KeyPressedEventArgs>(handlers.gotoLeftHalfOfScreen);

            gotoLeftHalfOfScreen.RegisterHotKey(hyperKey, Keys.A);

            // LEFT HALF OF CURRENT SCREEN
            gotoRightHalfOfScreen.KeyPressed +=
                new EventHandler<KeyPressedEventArgs>(handlers.gotoRightHalfOfScreen);

            gotoRightHalfOfScreen.RegisterHotKey(hyperKey, Keys.D);

        }

        private NotifyIcon trayIcon;

        protected override void OnLoad(EventArgs e)
        {
            // need to set both Visible & ShowInTaskbar to hide it
            Visible = false;
            ShowInTaskbar = false;
            base.OnLoad(e);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            trayIcon = new NotifyIcon()
            {
                Icon = Resource1.Icon1,
                ContextMenu = new ContextMenu(new MenuItem[]
                {
                        new MenuItem("Exit", Exit)
                }),
                Visible = true
            };
            this.Visible = false;
        }

        void Exit(object sender, EventArgs e)
        {
            trayIcon.Visible = false;
            Application.Exit();
        }
    }
}
