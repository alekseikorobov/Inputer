using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
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
        ILogger logger;
        private GlobalKeyboardHook _globalKeyboardHook;
        public Form1()
        {
            logger = NLog.LogManager.GetLogger("Application");
            InitializeComponent();

            var switchKeyString = ConfigurationManager.AppSettings["SwitchKey"].ToString();
            logger.Trace($"Get switchKeyString from config - {switchKeyString}");
            if (!string.IsNullOrEmpty(switchKeyString) && !Enum.TryParse(switchKeyString, true, out switchKey))
            {
                switchKey = Keys.Pause;
                logger.Trace($"Not Set from config set default {switchKey}");
            }
            logger.Trace($"Set from config set default {switchKey}");


            logger.Trace($"Init hook");
            _globalKeyboardHook = new GlobalKeyboardHook(true, false, switchKey);


            _globalKeyboardHook.KeyboardPressed += EventHook_eventKey;

            //_globalKeyboardHook.eventMouse += _globalKeyboardHook_eventMouse;

            this.Resize += new System.EventHandler(this.Form1_Resize);

            logger.Trace($"set Minimized WindowState");
            this.WindowState = FormWindowState.Minimized;

        }
        private Keys switchKey;

        private void Form1_Resize(object sender, EventArgs e)
        {
            logger.Trace($"event Form1_Resize");
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
            logger.Trace($"key - {e.KeyboardData.Key}, shift -  {e.ShiftPressed}, ctr - {e.CtrlPressed}, state - {e.KeyboardState}");

            if (e.KeyboardData.Key == switchKey)
            {
                changeLeng_Click(null, null);
                return;
            }
            if (e.KeyboardData.Key == Keys.Back)
            {
                if (words.Count > 0)
                {
                    var ch = words[words.Count - 1];
                    words.RemoveAt(words.Count - 1);
                    logger.Trace($"Remove last char - '{ch}' from words, now length - {words.Count}");
                }
                else
                {
                    logger.Trace($"words is empty");
                }
                return;
            }
            var keyString = KeysConv.ConvertToString(e.KeyboardData.Key);
            logger.Trace($"keyString - {keyString}");
            if (keyString != null)
            {
                words.Add(Tuple.Create(keyString, e.ShiftPressed));
                logger.Trace($"now length words - {words.Count} - {string.Join(",", words)}");
            }
            else
            {
                logger.Trace($"Empty words");
                words.Clear();
            }
        }

        private string ConvertCharForSend(string w1)
        {
            logger.Trace($"start ConvertCharForSend, input - {w1}");
            string charReplace = "()\"}{+%^&:";
            //string w1 = "!@#$%^&*()_+ !\"№;%:?*()_+ -={}[]\\|;':\"<>?,./";
            StringBuilder newW1 = new StringBuilder();

            foreach (var w in w1)
            {
                newW1.Append(charReplace.Contains(w) ? "{" + w + "}" : w.ToString());
            }

            var res = newW1.ToString();
            logger.Trace($"end ConvertCharForSend, input - {res}");
            return res;
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
            var res = Input.ToString();
            logger.Trace($"GetInputLang result - {res} (is rus {res == langRus})");
            return res;

        }

        const string langEng = "00000409";
        const string langRus = "00000419";
        private void changeLeng_Click(object sender, EventArgs e)
        {
            logger.Trace($"start changeLeng_Click");
            var nowLang = GetInputLang();

            bool isRus = nowLang == langRus;

            ///https://ru.stackoverflow.com/questions/413208/%D0%A1%D0%BC%D0%B5%D0%BD%D0%B0-%D1%80%D0%B0%D1%81%D0%BA%D0%BB%D0%B0%D0%B4%D0%BA%D0%B8-%D0%BA%D0%BB%D0%B0%D0%B2%D0%B8%D0%B0%D1%82%D1%83%D1%80%D1%8B-%D0%B2-%D1%81%D0%B8%D1%81%D1%82%D0%B5%D0%BC%D0%B5           

            var newLang = isRus ? langEng : langRus;

            logger.Trace($"switch to new lang - {newLang}");
            isRus = !isRus;

            int ret = LoadKeyboardLayout(newLang, 1);
            PostMessage(GetForegroundWindow(), 0x50, 1, ret);

            do
            {
                nowLang = GetInputLang();
            } while (newLang != nowLang);

            logger.Trace($"Seted to new lang - {newLang}");

            if (words.Any())
            {
                logger.Trace($"input words");

                logger.Trace($"isRus - {isRus}, start replace words - {string.Join(",", words)}");
                var l = words.Count;
                var w = words.Select(c => Tuple.Create(c.Item1, c.Item2)).ToList();
                logger.Trace($"delete {l} chars");
                for (int i = 0; i < l; i++)
                    SendKeys.Send("{BACKSPACE}");

                string w1 = Convert(w, !isRus);
                var newW = ConvertCharForSend(w1);
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
                logger.Trace($"{word}");
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

        private void изменитьГорячуюКлавишуToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
    }
}
