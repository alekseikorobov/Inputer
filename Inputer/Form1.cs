using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Inputer
{
    public partial class Form1 : Form
    {
        private GlobalKeyboardHook _globalKeyboardHook;
        public Form1()
        {
            InitializeComponent();

            _globalKeyboardHook = new GlobalKeyboardHook(true, false);

            _globalKeyboardHook.KeyboardPressed += EventHook_eventKey;

            //_globalKeyboardHook.eventMouse += _globalKeyboardHook_eventMouse;

            this.Resize += new System.EventHandler(this.Form1_Resize);

            this.WindowState = FormWindowState.Minimized;

        }
        private void Form1_Resize(object sender, EventArgs e)
        {
            Hide();            
        }


        public Form1(string test)
        {
        }
        private void Form1_Load(object sender, EventArgs e)
        {
        }

        List<Tuple<string, bool>> words = new List<Tuple<string, bool>>();

        private void EventHook_eventKey(object sender, GlobalKeyboardHookEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine($"key - {e.KeyboardData.Key}, shift -  {e.ShiftPressed}");

            if (e.KeyboardData.Key == Keys.Pause)
            {
                changeLeng_Click(null, null);
                return;
            }
            if (e.KeyboardData.Key == Keys.Back)
            {
                if (words.Count > 0)
                    words.RemoveAt(words.Count - 1);
                return;
            }
            var keyString = KeysConv.ConvertToString(e.KeyboardData.Key);
            if (keyString != null)
            {
                words.Add(Tuple.Create(keyString, e.ShiftPressed));
            }
            else
            {
                words.Clear();
            }
        }

        private string ConvertCharForSend(string w1)
        {
            string charReplace = "()\"}{+%^&:";
            //string w1 = "!@#$%^&*()_+ !\"№;%:?*()_+ -={}[]\\|;':\"<>?,./";
            StringBuilder newW1 = new StringBuilder();

            foreach (var w in w1)
            {
                newW1.Append(charReplace.Contains(w) ? "{" + w + "}" : w.ToString());
            }

            return newW1.ToString();
        }

        #region DLL

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool PostMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        static extern int LoadKeyboardLayout(string pwszKLID, uint Flags);

        StringBuilder Input = new StringBuilder(9);
        [DllImport("user32.dll")]
        static extern uint GetKeyboardLayoutList(int nBuff, [Out] IntPtr[] lpList);

        [DllImport("user32.dll")]
        static extern bool GetKeyboardLayoutName([Out] StringBuilder pwszKLID);
        [DllImport("user32.dll")]
        static extern IntPtr GetKeyboardLayout(uint idThread);
        [DllImport("user32.dll")]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId);
        [DllImport("user32.dll")]
        public static extern int ActivateKeyboardLayout(int HKL, int flags);


        #endregion

        public string GetInputLang()
        {
            /*https://stackoverrun.com/ru/q/5271980*/
            IntPtr layout = GetKeyboardLayout(GetWindowThreadProcessId(GetForegroundWindow(), IntPtr.Zero));
            ActivateKeyboardLayout((int)layout, 100);
            GetKeyboardLayoutName(Input);
            return Input.ToString();
        }

        string langEng = "00000409";
        string langRus = "00000419";
        private void changeLeng_Click(object sender, EventArgs e)
        {
            var nowLang = GetInputLang();

            System.Diagnostics.Debug.WriteLine($"now lang - {nowLang}");
            bool isRus = nowLang == langRus;

            ///https://ru.stackoverflow.com/questions/413208/%D0%A1%D0%BC%D0%B5%D0%BD%D0%B0-%D1%80%D0%B0%D1%81%D0%BA%D0%BB%D0%B0%D0%B4%D0%BA%D0%B8-%D0%BA%D0%BB%D0%B0%D0%B2%D0%B8%D0%B0%D1%82%D1%83%D1%80%D1%8B-%D0%B2-%D1%81%D0%B8%D1%81%D1%82%D0%B5%D0%BC%D0%B5           

            var newLang = isRus ? langEng : langRus;

            System.Diagnostics.Debug.WriteLine($"new lang - {newLang}");
            isRus = !isRus;

            int ret = LoadKeyboardLayout(newLang, 1);
            PostMessage(GetForegroundWindow(), 0x50, 1, ret);

            do
            {
                nowLang = GetInputLang();
            } while (newLang != nowLang);

            System.Diagnostics.Debug.WriteLine($"setting lang - {newLang}");

            if (words.Any())
            {
                System.Diagnostics.Debug.WriteLine($"isRus - {isRus}, start replace words - {string.Join(",", words.Select(c => c.Item1 + " - " + c.Item2))}");
                var l = words.Count;
                var w = words.Select(c => Tuple.Create(c.Item1, c.Item2)).ToList();
                for (int i = 0; i < l; i++)
                    SendKeys.Send("{BACKSPACE}");
                string w1 = Convert(w, !isRus);
                var newW = ConvertCharForSend(w1);
                System.Diagnostics.Debug.WriteLine($"result - {newW}");
                SendKeys.Send(newW);
            }
        }

        public string Convert(List<Tuple<string, bool>> words, bool isEng)
        {
            var alphabitFrom = alphabitEn;
            var alphabitTo = alphabitRu;
            var alphabitFromShift = alphabitEnShiftFrom;
            var alphabitToShift = alphabitRuShiftTo;

            if (isEng)
            {
                alphabitFrom = alphabitEn;
                alphabitTo = alphabitEn;
                alphabitFromShift = alphabitEnShiftFrom;
                alphabitToShift = alphabitEnShiftFrom;
            }
            StringBuilder resultWord = new StringBuilder();
            foreach (var word in words)
            {
                var ch = word.Item1;
                var isShift = word.Item2;
                //System.Diagnostics.Debug.WriteLine($"ch - {ch}, isShift - {isShift}");
                var index = (isShift ? alphabitFromShift : alphabitFrom).IndexOf(isShift ? ch : ch.ToLower());
                if (index != -1)
                {
                    var newCh = (isShift ? alphabitToShift : alphabitTo)[index];
                    resultWord.Append(newCh);
                }
                else
                {
                    resultWord.Append(ch);
                }
            }
            return resultWord.ToString();
        }

        const string alphabitEn = "qwertyuiop[]\\asdfghjkl;'zxcvbnm,./`1234567890-=";
        const string alphabitEnShiftFrom = "QWERTYUIOP[]\\ASDFGHJKL;'ZXCVBNM,./`1234567890-=";

        const string alphabitRu = "йцукенгшщзхъ\\фывапролджэячсмитьбю.ё1234567890-=";


        const string alphabitRuShiftTo = "ЙЦУКЕНГШЩЗХЪ\\ФЫВАПРОЛДЖЭЯЧСМИТЬБЮ,Ё!\"№;%:?*()_+";

        //const string alphabitRuShiftFrom = "ЙЦУКЕНГШЩЗХЪ/ФЫВАПРОЛДЖЭЯЧСМИТЬБЮ,1234567890_+";
        //const string alphabitEnShiftTo = "QWERTYUIOP{}|ASDFGHJKL:\"ZXCVBNM<>?~!@#$%^&*()_+";


        private void button1_Click(object sender, EventArgs e)
        {
            if (_globalKeyboardHook != null)
            {
                _globalKeyboardHook.isDebug = !_globalKeyboardHook.isDebug;
                if (_globalKeyboardHook.isDebug)
                {
                    System.Diagnostics.Debug.WriteLine("Set debug");
                }
            }
        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
