using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Inputer
{
    class GlobalKeyboardHookEventArgs : HandledEventArgs
    {
        public GlobalKeyboardHook.KeyboardState KeyboardState { get; private set; }
        public GlobalKeyboardHook.LowLevelKeyboardInputEvent KeyboardData { get; private set; }
        public bool CtrlPressed { get; internal set; }
        public bool ShiftPressed { get; internal set; }

        public GlobalKeyboardHookEventArgs(
            GlobalKeyboardHook.LowLevelKeyboardInputEvent keyboardData,
            GlobalKeyboardHook.KeyboardState keyboardState)
        {
            KeyboardData = keyboardData;
            KeyboardState = keyboardState;
        }
    }

    //Based on https://gist.github.com/Stasonix
    class GlobalKeyboardHook : IDisposable
    {
        public event EventHandler<GlobalKeyboardHookEventArgs> KeyboardPressed;
        public event GetEventMouse eventMouse;
        public delegate void GetEventMouse(TypeMouse type, int x, int y);
        
        // EDT: Added an optional parameter (registeredKeys) that accepts keys to restict
        // the logging mechanism.
        /// <summary>
        /// 
        /// </summary>
        /// <param name="registeredKeys">Keys that should trigger logging. Pass null for full logging.</param>
        public GlobalKeyboardHook(bool isKey = true, bool isMouse = true, Keys hotKey = Keys.Pause)
        {
            _hotKey = hotKey;
            if (isKey)
            {
                _windowsHookHandle = IntPtr.Zero;
                _user32LibraryHandle = IntPtr.Zero;
                _hookProc = LowLevelKeyboardProc; // we must keep alive _hookProc, because GC is not aware about SetWindowsHookEx behaviour.

                //KeyBoardProck = new KeyBoardProc(HookCallback);

                _user32LibraryHandle = LoadLibrary("User32");
                if (_user32LibraryHandle == IntPtr.Zero)
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    throw new Win32Exception(errorCode, $"Failed to load library 'User32.dll'. Error {errorCode}: {new Win32Exception(Marshal.GetLastWin32Error()).Message}.");
                }



                _windowsHookHandle = SetWindowsHookEx(WH_KEYBOARD_LL, _hookProc, _user32LibraryHandle, 0);
                if (_windowsHookHandle == IntPtr.Zero)
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    throw new Win32Exception(errorCode, $"Failed to adjust keyboard hooks for '{Process.GetCurrentProcess().ProcessName}'. Error {errorCode}: {new Win32Exception(Marshal.GetLastWin32Error()).Message}.");
                }
            }
            if (isMouse)
            {
                lowLevelMouseProc = HookCallbackM;
                _hookIDM = SetHookM();

            }
        }
        #region Mouse
        static IntPtr _hookIDM = IntPtr.Zero;
        private static LowLevelMouseProc lowLevelMouseProc;
        public delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);
        const int WH_MOUSE_LL = 14;
        public enum TypeMouse
        {
            LeftKey = 514,
            RightKey = 517
        }
        private enum MouseMessages
        {
            WM_LBUTTONDOWN = 0x0201,
            WM_LBUTTONUP = 0x0202,
            WM_MOUSEMOVE = 0x0200,
            WM_MOUSEWHEEL = 0x020A,
            WM_RBUTTONDOWN = 0x0204,
            WM_RBUTTONUP = 0x0205
        }

        private struct POINT
        {
            public int x;
            public int y;
        }
        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }
        public static IntPtr SetHookM()
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_MOUSE_LL, lowLevelMouseProc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }
        IntPtr HookCallbackM(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && (MouseMessages.WM_LBUTTONUP == (MouseMessages)wParam ||
                               MouseMessages.WM_RBUTTONUP == (MouseMessages)wParam))
            {
                MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));

                eventMouse?.Invoke((TypeMouse)wParam, hookStruct.pt.x, hookStruct.pt.y);
            }

            return CallNextHookEx(_hookIDM, nCode, wParam, lParam);
        } 
        #endregion

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // because we can unhook only in the same thread, not in garbage collector thread
                if (_windowsHookHandle != IntPtr.Zero)
                {
                    if (!UnhookWindowsHookEx(_windowsHookHandle))
                    {
                        int errorCode = Marshal.GetLastWin32Error();
                        throw new Win32Exception(errorCode, $"Failed to remove keyboard hooks for '{Process.GetCurrentProcess().ProcessName}'. Error {errorCode}: {new Win32Exception(Marshal.GetLastWin32Error()).Message}.");
                    }
                    _windowsHookHandle = IntPtr.Zero;

                    // ReSharper disable once DelegateSubtraction
                    _hookProc -= LowLevelKeyboardProc;
                }
            }

            if (_user32LibraryHandle != IntPtr.Zero)
            {
                if (!FreeLibrary(_user32LibraryHandle)) // reduces reference to library by 1.
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    throw new Win32Exception(errorCode, $"Failed to unload library 'User32.dll'. Error {errorCode}: {new Win32Exception(Marshal.GetLastWin32Error()).Message}.");
                }
                _user32LibraryHandle = IntPtr.Zero;
            }
        }

        ~GlobalKeyboardHook()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private IntPtr _windowsHookHandle;
        private IntPtr _user32LibraryHandle;
        private HookProc _hookProc;

        delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        #region dll
        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern bool FreeLibrary(IntPtr hModule);

        /// <summary>
        /// The SetWindowsHookEx function installs an application-defined hook procedure into a hook chain.
        /// You would install a hook procedure to monitor the system for certain types of events. These events are
        /// associated either with a specific thread or with all threads in the same desktop as the calling thread.
        /// </summary>
        /// <param name="idHook">hook type</param>
        /// <param name="lpfn">hook procedure</param>
        /// <param name="hMod">handle to application instance</param>
        /// <param name="dwThreadId">thread identifier</param>
        /// <returns>If the function succeeds, the return value is the handle to the hook procedure.</returns>
        [DllImport("USER32", SetLastError = true)]
        static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, int dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        /// <summary>
        /// The UnhookWindowsHookEx function removes a hook procedure installed in a hook chain by the SetWindowsHookEx function.
        /// </summary>
        /// <param name="hhk">handle to hook procedure</param>
        /// <returns>If the function succeeds, the return value is true.</returns>
        [DllImport("USER32", SetLastError = true)]
        public static extern bool UnhookWindowsHookEx(IntPtr hHook);

        /// <summary>
        /// The CallNextHookEx function passes the hook information to the next hook procedure in the current hook chain.
        /// A hook procedure can call this function either before or after processing the hook information.
        /// </summary>
        /// <param name="hHook">handle to current hook</param>
        /// <param name="code">hook code passed to hook procedure</param>
        /// <param name="wParam">value passed to hook procedure</param>
        /// <param name="lParam">value passed to hook procedure</param>
        /// <returns>If the function succeeds, the return value is true.</returns>
        [DllImport("USER32", SetLastError = true)]
        static extern IntPtr CallNextHookEx(IntPtr hHook, int code, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
        #endregion

        [StructLayout(LayoutKind.Sequential)]
        public struct LowLevelKeyboardInputEvent
        {
            /// <summary>
            /// A virtual-key code. The code must be a value in the range 1 to 254.
            /// </summary>
            public int VirtualCode;

            // EDT: added a conversion from VirtualCode to Keys.
            /// <summary>
            /// The VirtualCode converted to typeof(Keys) for higher usability.
            /// </summary>
            public Keys Key { get { return (Keys)VirtualCode; } }

            /// <summary>
            /// A hardware scan code for the key. 
            /// </summary>
            public int HardwareScanCode;

            /// <summary>
            /// The extended-key flag, event-injected Flags, context code, and transition-state flag. This member is specified as follows. An application can use the following values to test the keystroke Flags. Testing LLKHF_INJECTED (bit 4) will tell you whether the event was injected. If it was, then testing LLKHF_LOWER_IL_INJECTED (bit 1) will tell you whether or not the event was injected from a process running at lower integrity level.
            /// </summary>
            public int Flags;

            /// <summary>
            /// The time stamp stamp for this message, equivalent to what GetMessageTime would return for this message.
            /// </summary>
            public int TimeStamp;

            /// <summary>
            /// Additional information associated with the message. 
            /// </summary>
            public IntPtr AdditionalInformation;
        }

        public const int WH_KEYBOARD_LL = 13;

        public enum KeyboardState
        {
            KeyDown = 0x0100,
            KeyUp = 0x0101,
            SysKeyDown = 0x0104,
            SysKeyUp = 0x0105
        }

        const int KfAltdown = 0x2000;
        public const int LlkhfAltdown = (KfAltdown >> 8);
        bool ctrlPressed;
        bool shiftPressed;
        bool save = false;
        bool reset = false;
        public bool isDebug = false;
        private Keys _hotKey;

        public IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            bool fEatKeyStroke = false;
            //
            var wparamTyped = wParam.ToInt32();
            if (Enum.IsDefined(typeof(KeyboardState), wparamTyped))
            {
                object o = Marshal.PtrToStructure(lParam, typeof(LowLevelKeyboardInputEvent));
                LowLevelKeyboardInputEvent p = (LowLevelKeyboardInputEvent)o;
                if(isDebug)
                    System.Diagnostics.Debug.WriteLine($"system - p.Key {p.Key} - {((KeyboardState)wparamTyped)}");

                bool isKeyStrlOrShift = p.Key == Keys.LShiftKey || p.Key == Keys.RShiftKey
                    || p.Key == Keys.LControlKey || p.Key == Keys.RControlKey;
                if (isKeyStrlOrShift)
                {
                    if (p.Key == Keys.LControlKey || p.Key == Keys.RControlKey)
                        ctrlPressed = save || ((KeyboardState)wparamTyped) == KeyboardState.KeyDown
                                    || ((KeyboardState)wparamTyped) == KeyboardState.SysKeyDown;
                    if (p.Key == Keys.LShiftKey || p.Key == Keys.RShiftKey)
                        shiftPressed = save ||
                            ((KeyboardState)wparamTyped) == KeyboardState.KeyDown
                                   || ((KeyboardState)wparamTyped) == KeyboardState.SysKeyDown;

                    if(((KeyboardState)wparamTyped) == KeyboardState.KeyUp
                                   || ((KeyboardState)wparamTyped) == KeyboardState.SysKeyUp)
                    {
                        reset = true;
                    }
                }
                else
                {
                    save = ((KeyboardState)wparamTyped) == KeyboardState.KeyDown
                        || ((KeyboardState)wparamTyped) == KeyboardState.SysKeyDown;
                }

                var eventArguments = new GlobalKeyboardHookEventArgs(p, (KeyboardState)wparamTyped);
                eventArguments.CtrlPressed = ctrlPressed;
                eventArguments.ShiftPressed = shiftPressed;

                if (p.Key == _hotKey)
                {
                    if ((eventArguments.KeyboardState == KeyboardState.KeyUp
                    || eventArguments.KeyboardState == KeyboardState.SysKeyUp))
                    {
                        EventHandler<GlobalKeyboardHookEventArgs> handler = KeyboardPressed;
                        handler?.Invoke(this, eventArguments);
                    }
                    return (IntPtr)1;
                }


                var key = (Keys)p.VirtualCode;
                if ((eventArguments.KeyboardState == KeyboardState.KeyUp
                    || eventArguments.KeyboardState == KeyboardState.SysKeyUp)
                    && !isKeyStrlOrShift)
                {
                    EventHandler<GlobalKeyboardHookEventArgs> handler = KeyboardPressed;
                    handler?.Invoke(this, eventArguments);

                    fEatKeyStroke = eventArguments.Handled;
                    save = false;
                    if (reset)
                    {
                        ctrlPressed = false;
                        shiftPressed = false;
                        reset = false;
                    }
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"!!!!!!!!!!!!!!!! wParam - {wParam}");
            }

            return fEatKeyStroke ? (IntPtr)1 : CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }


    }
}
