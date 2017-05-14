using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using wock.Models;

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

        public Form1()
        {
            InitializeComponent();

            var handlers = new HotkeyHandlers();

            // VSC
            gotoVisualStudioCode.KeyPressed +=
                new EventHandler<KeyPressedEventArgs>(handlers.hook_gotoVisualStudioCode_KeyPressed);
            // register the control + alt + F12 combination as hot key.
            gotoVisualStudioCode.RegisterHotKey(
                wock.Models.ModifierKeys.Control | wock.Models.ModifierKeys.Alt,
                Keys.D1);

            // CMDER
            gotoCmder.KeyPressed +=
                new EventHandler<KeyPressedEventArgs>(handlers.hook_gotoCmder_KeyPressed);
            // register the control + alt + F12 combination as hot key.
            gotoCmder.RegisterHotKey(
                wock.Models.ModifierKeys.Control | wock.Models.ModifierKeys.Alt,
                Keys.D2);

            // CHROME
            gotoChrome.KeyPressed +=
                new EventHandler<KeyPressedEventArgs>(handlers.hook_gotoChrome_KeyPressed);
            // register the control + alt + F12 combination as hot key.
            gotoChrome.RegisterHotKey(
                wock.Models.ModifierKeys.Control | wock.Models.ModifierKeys.Alt,
                Keys.D3);

            // VS
            gotoVisualStudio.KeyPressed +=
                new EventHandler<KeyPressedEventArgs>(handlers.hook_visualstudio_KeyPressed);
            // register the control + alt + F12 combination as hot key.
            gotoVisualStudio.RegisterHotKey(
                wock.Models.ModifierKeys.Control | wock.Models.ModifierKeys.Alt,
                Keys.D4);

            /// FULLSCREEN 
            fullscreenHook.KeyPressed +=
                new EventHandler<KeyPressedEventArgs>(handlers.hook_fullscreen_KeyPressed);
            // register the control + alt + F12 combination as hot key.
            fullscreenHook.RegisterHotKey(
                wock.Models.ModifierKeys.Control | wock.Models.ModifierKeys.Alt,
                Keys.F);

            // NEXT SCREEN
            sendToNextScreenHook.KeyPressed +=
                new EventHandler<KeyPressedEventArgs>(handlers.hook_nextWindow_KeyPressed);
            // register the control + alt + F12 combination as hot key.
            sendToNextScreenHook.RegisterHotKey(
                wock.Models.ModifierKeys.Control | wock.Models.ModifierKeys.Alt,
                Keys.E);

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
