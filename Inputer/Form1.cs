using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
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
            try
            {
                logger = NLog.LogManager.GetLogger("Application");
                InitializeComponent();

                logger?.Trace($"Init hook");
                _globalKeyboardHook = new GlobalKeyboardHook(true, false);

                var useSwitchLanguage = ConfigurationManager.AppSettings["UseSwitchLanguage"]?.ToString() == "true";

                _globalKeyboardHook.UseSwitchLanguage = useSwitchLanguage;
                _globalKeyboardHook.KeyboardPressed += EventHook_eventKey;
                _globalKeyboardHook.SwitchLanguagePressed += _globalKeyboardHook_SwitchLanguagePressed;

                //if you need include event mouse
                //_globalKeyboardHook.eventMouse += _globalKeyboardHook_eventMouse;

                SetSwitchKey();

                this.Resize += new System.EventHandler(this.Form1_Resize);

                logger?.Trace($"set Minimized WindowState");
                this.WindowState = FormWindowState.Minimized;

                nowLang = GetInputLang();               
            }
            catch (Exception ex)
            {
                logger?.Error(ex);

                throw;
            }

        }

        private void _globalKeyboardHook_SwitchLanguagePressed(object sender, GlobalKeyboardHookEventArgs e)
        {
            ChangeLanguage();
        }

        private void SetSwitchKey()
        {
            var switchKeyString = ConfigurationManager.AppSettings["SwitchKey"].ToString();
            logger?.Trace($"Get switchKeyString from config - {switchKeyString}");
            if (!string.IsNullOrEmpty(switchKeyString) && !Enum.TryParse(switchKeyString, true, out switchKey))
            {
                switchKey = Keys.Pause;
                logger?.Trace($"Not Set from config set default {switchKey}");
            }
            if(switchKey == Keys.Pause)
            {
                pauseBrakeToolStripMenuItem_Click(null, null);
            }
            else if (switchKey == Keys.Insert)
            {
                insertToolStripMenuItem_Click(null, null);
            }

            logger?.Trace($"Set from config set default {switchKey}");
        }
        private Keys switchKey;

        private void Form1_Resize(object sender, EventArgs e)
        {
            logger?.Trace($"event Form1_Resize");
            Hide();
        }


        public Form1(string test)
        {
        }

        List<Tuple<string, bool>> words = new List<Tuple<string, bool>>();

        List<Tuple<string, bool>> tempString = new List<Tuple<string, bool>>();
        private string nowLang;
        

        private void EventHook_eventKey(object sender, GlobalKeyboardHookEventArgs e)
        {
            try
            {
                logger?.Trace($"key - {e.KeyboardData.Key}, shift -  {e.ShiftPressed}, ctr - {e.CtrlPressed}, state - {e.KeyboardState}");

                if (e.KeyboardData.Key == switchKey)
                {
                    SwitchTextLanguage();
                    return;
                }
                if (e.KeyboardData.Key == Keys.Back)
                {
                    if (words.Count > 0)
                    {
                        var ch = words[words.Count - 1];
                        words.RemoveAt(words.Count - 1);
                        logger?.Trace($"Remove last char - '{ch}' from words, now length - {words.Count}");
                    }
                    else
                    {
                        logger?.Trace($"words is empty");
                    }
                    return;
                }
                var keyString = !e.CtrlPressed ? KeysConv.ConvertToString(e.KeyboardData.Key) : null;
                logger?.Trace($"keyString - {keyString}");
                if (keyString != null)
                {
                    words.Add(Tuple.Create(keyString, e.ShiftPressed));
                    tempString.Add(Tuple.Create(keyString, e.ShiftPressed));

                    logger?.Trace($"now length words - {words.Count} - {string.Join(",", words)}");
                }
                else
                {
                    logger?.Trace($"Empty words");
                    words.Clear();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                throw;
            }
        }

        private string ConvertCharForSend(string wordString)
        {
            logger?.Trace($"start ConvertCharForSend, input - {wordString}");
            string charReplace = "()\"}{+%^&:";
            //string w1 = "!@#$%^&*()_+ !\"№;%:?*()_+ -={}[]\\|;':\"<>?,./";
            StringBuilder resultWordString = new StringBuilder();

            foreach (var w in wordString)
            {
                resultWordString.Append(charReplace.Contains(w) ? "{" + w + "}" : w.ToString());
            }

            var res = resultWordString.ToString();
            logger?.Trace($"end ConvertCharForSend, input - {res}");
            return res;
        }


        public string GetInputLang()
        {
            StringBuilder Input = new StringBuilder(9);

            /*https://stackoverrun.com/ru/q/5271980*/
            IntPtr layout = Win32.GetKeyboardLayout(Win32.GetWindowThreadProcessId(Win32.GetForegroundWindow(), IntPtr.Zero));
            Win32.ActivateKeyboardLayout((int)layout, 100);
            Win32.GetKeyboardLayoutName(Input);
            var res = Input.ToString();
            logger?.Trace($"GetInputLang result - {res} (is rus {res == langRus})");
            return res;

        }

        const string langEng = "00000409";
        const string langRus = "00000419";
        private void SwitchTextLanguage()
        {            
            bool isRus = ChangeLanguage();

            if (words.Any())
            {
                logger?.Trace($"input words");

                logger?.Trace($"isRus - {isRus}, start replace words - {string.Join(",", words)}");
                var count = words.Count;
                var wordsCopy = words.Select(c => Tuple.Create(c.Item1, c.Item2)).ToList();
                logger?.Trace($"delete {count} chars");
                for (int i = 0; i < count; i++)
                    SendKeys.Send("{BACKSPACE}");

                string newWordConvertedString = Convert(wordsCopy, !isRus);
                var sendedWordsString = ConvertCharForSend(newWordConvertedString);
                SendKeys.Send(sendedWordsString);
            }
        }

        private bool ChangeLanguage()
        {            
            logger?.Trace($"start ChangeLanguage");
            nowLang = GetInputLang();

            bool isRus = nowLang == langRus;

            ///https://ru.stackoverflow.com/questions/413208/%D0%A1%D0%BC%D0%B5%D0%BD%D0%B0-%D1%80%D0%B0%D1%81%D0%BA%D0%BB%D0%B0%D0%B4%D0%BA%D0%B8-%D0%BA%D0%BB%D0%B0%D0%B2%D0%B8%D0%B0%D1%82%D1%83%D1%80%D1%8B-%D0%B2-%D1%81%D0%B8%D1%81%D1%82%D0%B5%D0%BC%D0%B5           

            var newLang = isRus ? langEng : langRus;
            logger?.Trace($"switch to new lang - {newLang}");
            isRus = !isRus;

            int ret = Win32.LoadKeyboardLayout(newLang, 1);
            Win32.PostMessage(Win32.GetForegroundWindow(), 0x50, 1, ret);

            do
            {
                nowLang = GetInputLang();
            } while (newLang != nowLang);

            logger?.Trace($"Seted to new lang - {newLang}");
            return isRus;
        }

        public string Convert(IEnumerable<Tuple<string, bool>> words)
        {
            nowLang = GetInputLang();
            var isEng = nowLang == langEng;
            return Convert(words, isEng);
        }
        public string Convert(IEnumerable<Tuple<string, bool>> words, bool isEng)
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
                alphabitToShift = alphabitEnShiftTo;
            }
            StringBuilder resultWord = new StringBuilder();
            foreach (var word in words)
            {
                var ch = word.Item1;
                var isShift = word.Item2;
                logger?.Trace($"{word}");
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
        const string alphabitEnShiftTo = "QWERTYUIOP{}|ASDFGHJKL:\"ZXCVBNM<>?~!@#$%^&*()_+";
        const string alphabitRu = "йцукенгшщзхъ\\фывапролджэячсмитьбю.ё1234567890-=";
        const string alphabitRuShiftTo = "ЙЦУКЕНГШЩЗХЪ\\ФЫВАПРОЛДЖЭЯЧСМИТЬБЮ,Ё!\"№;%:?*()_+";

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

        private void insertToolStripMenuItem_Click(object sender, EventArgs e)
        {
            switchKey = Keys.Insert;
            _globalKeyboardHook.HotKey = switchKey;
            insertToolStripMenuItem.Checked = true;
            pauseBrakeToolStripMenuItem.Checked = false;
        }

        private void pauseBrakeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            switchKey = Keys.Pause;
            _globalKeyboardHook.HotKey = switchKey;
            insertToolStripMenuItem.Checked = false;
            pauseBrakeToolStripMenuItem.Checked = true;
        }
    }
}
