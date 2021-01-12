using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Inputer
{
    public class KeysConv
    {
        public static string ConvertToString(Keys key)
        {
            string str = null;            
            var match = Regex.Match(key.ToString(), "^([A-z])$");
            if (!string.IsNullOrEmpty(match.Value))
            {
                str = match.Groups[1].Value;
                return str;
            }
            match = Regex.Match(key.ToString(), "^D([0-9])$");
            if (!string.IsNullOrEmpty(match.Value))
            {
                str = match.Groups[1].Value;
                return str;
            }

            switch (key)
            {
                case Keys.KeyCode:
                    break;
                case Keys.Modifiers:
                    break;
                case Keys.None:
                    break;
                case Keys.LButton:
                    break;
                case Keys.RButton:
                    break;
                case Keys.Cancel:
                    break;
                case Keys.MButton:
                    break;
                case Keys.XButton1:
                    break;
                case Keys.XButton2:
                    break;
                case Keys.Back:
                    break;
                case Keys.Tab:
                    break;
                case Keys.LineFeed:
                    break;
                case Keys.Clear:
                    break;
                case Keys.Return:
                    //case Keys.Enter:
                    break;
                case Keys.ShiftKey:
                    break;
                case Keys.ControlKey:
                    break;
                case Keys.Menu:
                    break;
                case Keys.Pause:
                    break;
                case Keys.Capital:
                    break;
                //case Keys.CapsLock:
                //break;
                case Keys.KanaMode:
                    break;
                //case Keys.HanguelMode:
                //break;
                //case Keys.HangulMode:
                //break;
                case Keys.JunjaMode:
                    break;
                case Keys.FinalMode:
                    break;
                case Keys.HanjaMode:
                    break;
                //case Keys.KanjiMode:
                //break;
                case Keys.Escape:
                    break;
                case Keys.IMEConvert:
                    break;
                case Keys.IMENonconvert:
                    break;
                case Keys.IMEAccept:
                    break;
                //case Keys.IMEAceept:
                //break;
                case Keys.IMEModeChange:
                    break;
                case Keys.Space:
                    str = " ";
                    break;
                case Keys.Prior:
                    break;
                //case Keys.PageUp:
                //break;
                case Keys.Next:
                    break;
                //case Keys.PageDown:
                //break;
                case Keys.End:
                    break;
                case Keys.Home:
                    break;
                case Keys.Left:
                    break;
                case Keys.Up:
                    break;
                case Keys.Right:
                    break;
                case Keys.Down:
                    break;
                case Keys.Select:
                    break;
                case Keys.Print:
                    break;
                case Keys.Execute:
                    break;
                case Keys.Snapshot:
                    break;
                //case Keys.PrintScreen:
                //break;
                case Keys.Insert:
                    break;
                case Keys.Delete:
                    break;
                case Keys.Help:
                    break;
                //case Keys.D0:
                //    break;
                //case Keys.D1:
                //    break;
                //case Keys.D2:
                //    break;
                //case Keys.D3:
                //    break;
                //case Keys.D4:
                //    break;
                //case Keys.D5:
                //    break;
                //case Keys.D6:
                //    break;
                //case Keys.D7:
                //    break;
                //case Keys.D8:
                //    break;
                //case Keys.D9:
                //    break;
                //case Keys.A:
                //    break;
                //case Keys.B:
                //    break;
                //case Keys.C:
                //    break;
                //case Keys.D:
                //    break;
                //case Keys.E:
                //    break;
                //case Keys.F:
                //    break;
                //case Keys.G:
                //    break;
                //case Keys.H:
                //    break;
                //case Keys.I:
                //    break;
                //case Keys.J:
                //    break;
                //case Keys.K:
                //    break;
                //case Keys.L:
                //    break;
                //case Keys.M:
                //    break;
                //case Keys.N:
                //    break;
                //case Keys.O:
                //    break;
                //case Keys.P:
                //    break;
                //case Keys.Q:
                //    break;
                //case Keys.R:
                //    break;
                //case Keys.S:
                //    break;
                //case Keys.T:
                //    break;
                //case Keys.U:
                //    break;
                //case Keys.V:
                //    break;
                //case Keys.W:
                //    break;
                //case Keys.X:
                //    break;
                //case Keys.Y:
                //    break;
                //case Keys.Z:
                //    break;
                //case Keys.LWin:
                //break;
                //case Keys.RWin:
                //break;
                //case Keys.Apps:
                //break;
                //case Keys.Sleep:
                //break;
                //case Keys.NumPad0:
                //    break;
                //case Keys.NumPad1:
                //    break;
                //case Keys.NumPad2:
                //    break;
                //case Keys.NumPad3:
                //    break;
                //case Keys.NumPad4:
                //    break;
                //case Keys.NumPad5:
                //    break;
                //case Keys.NumPad6:
                //    break;
                //case Keys.NumPad7:
                //    break;
                //case Keys.NumPad8:
                //    break;
                //case Keys.NumPad9:
                //    break;
                case Keys.Multiply:
                    break;
                case Keys.Add:
                    break;
                case Keys.Separator:
                    break;
                case Keys.Subtract:
                    break;
                case Keys.Decimal:
                    break;
                case Keys.Divide:
                    break;
                case Keys.F1:
                    break;
                case Keys.F2:
                    break;
                case Keys.F3:
                    break;
                case Keys.F4:
                    break;
                case Keys.F5:
                    break;
                case Keys.F6:
                    break;
                case Keys.F7:
                    break;
                case Keys.F8:
                    break;
                case Keys.F9:
                    break;
                case Keys.F10:
                    break;
                case Keys.F11:
                    break;
                case Keys.F12:
                    break;
                case Keys.F13:
                    break;
                case Keys.F14:
                    break;
                case Keys.F15:
                    break;
                case Keys.F16:
                    break;
                case Keys.F17:
                    break;
                case Keys.F18:
                    break;
                case Keys.F19:
                    break;
                case Keys.F20:
                    break;
                case Keys.F21:
                    break;
                case Keys.F22:
                    break;
                case Keys.F23:
                    break;
                case Keys.F24:
                    break;
                case Keys.NumLock:
                    break;
                case Keys.Scroll:
                    break;
                case Keys.LShiftKey:
                    break;
                case Keys.RShiftKey:
                    break;
                case Keys.LControlKey:
                    break;
                case Keys.RControlKey:
                    break;
                case Keys.LMenu:
                    break;
                case Keys.RMenu:
                    break;
                case Keys.BrowserBack:
                    break;
                case Keys.BrowserForward:
                    break;
                case Keys.BrowserRefresh:
                    break;
                case Keys.BrowserStop:
                    break;
                case Keys.BrowserSearch:
                    break;
                case Keys.BrowserFavorites:
                    break;
                case Keys.BrowserHome:
                    break;
                case Keys.VolumeMute:
                    break;
                case Keys.VolumeDown:
                    break;
                case Keys.VolumeUp:
                    break;
                case Keys.MediaNextTrack:
                    break;
                case Keys.MediaPreviousTrack:
                    break;
                case Keys.MediaStop:
                    break;
                case Keys.MediaPlayPause:
                    break;
                case Keys.LaunchMail:
                    break;
                case Keys.SelectMedia:
                    break;
                case Keys.LaunchApplication1:
                    break;
                case Keys.LaunchApplication2:
                    break;
                //case Keys.OemSemicolon:
                    //break;
                case Keys.Oem1:
                    str = ";";
                    break;
                case Keys.Oemplus:
                    str = "=";
                    break;
                case Keys.Oemcomma:
                    str = ",";
                    break;
                case Keys.OemMinus:
                    str = "-";
                    break;
                case Keys.OemPeriod:
                    str = ".";
                    break;
                case Keys.OemQuestion:
                    str = "/";
                    break;
                //case Keys.Oem2:
                //    break;
                case Keys.Oemtilde:
                    str = "`";
                    break;
                //case Keys.Oem3:
                //    break;
                case Keys.OemOpenBrackets:
                    str = "[";
                    break;
                //case Keys.Oem4:
                //    break;
                //case Keys.OemPipe:
                    //break;
                case Keys.Oem5:
                    str = "\\";
                    break;
                //case Keys.OemCloseBrackets:
                //break;
                case Keys.Oem6:
                    str = "]";
                    break;
                //case Keys.OemQuotes:
                    //break;
                case Keys.Oem7:
                    str = "'";
                    break;
                case Keys.Oem8:
                    break;
                case Keys.OemBackslash:
                    break;
                //case Keys.Oem102:
                //    break;
                case Keys.ProcessKey:
                    break;
                case Keys.Packet:
                    break;
                case Keys.Attn:
                    break;
                case Keys.Crsel:
                    break;
                case Keys.Exsel:
                    break;
                case Keys.EraseEof:
                    break;
                case Keys.Play:
                    break;
                case Keys.Zoom:
                    break;
                case Keys.NoName:
                    break;
                case Keys.Pa1:
                    break;
                case Keys.OemClear:
                    break;
                case Keys.Shift:
                    break;
                case Keys.Control:
                    break;
                case Keys.Alt:
                    break;
                default:
                    break;
            }

            return str;
        }
    }
}
