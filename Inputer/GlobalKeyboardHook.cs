using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Inputer
{
    class GlobalKeyboardHookEventArgs : HandledEventArgs
    {
        public Win32.KeyboardState KeyboardState { get; private set; }
        public Win32.LowLevelKeyboardInputEvent KeyboardData { get; private set; }
        public bool CtrlPressed { get; internal set; }
        public bool ShiftPressed { get; internal set; }

        public GlobalKeyboardHookEventArgs(
            Win32.LowLevelKeyboardInputEvent keyboardData,
            Win32.KeyboardState keyboardState)
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
        public delegate void GetEventMouse(Win32.TypeMouse type, int x, int y);
        
        // EDT: Added an optional parameter (registeredKeys) that accepts keys to restict
        // the logging mechanism.
        /// <summary>
        /// 
        /// </summary>
        /// <param name="registeredKeys">Keys that should trigger logging. Pass null for full logging.</param>
        public GlobalKeyboardHook(bool isKey = true, bool isMouse = true)
        {
            if (isKey)
            {
                _windowsHookHandle = IntPtr.Zero;
                _user32LibraryHandle = IntPtr.Zero;
                _hookProc = LowLevelKeyboardProc; // we must keep alive _hookProc, because GC is not aware about SetWindowsHookEx behaviour.

                //KeyBoardProck = new KeyBoardProc(HookCallback);

                _user32LibraryHandle = Win32.LoadLibrary("User32");
                if (_user32LibraryHandle == IntPtr.Zero)
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    throw new Win32Exception(errorCode, $"Failed to load library 'User32.dll'. Error {errorCode}: {new Win32Exception(Marshal.GetLastWin32Error()).Message}.");
                }



                _windowsHookHandle = Win32.SetWindowsHookEx(Win32.WH_KEYBOARD_LL, _hookProc, _user32LibraryHandle, 0);
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
        private static Win32.LowLevelMouseProc lowLevelMouseProc;        
        
        public static IntPtr SetHookM()
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return Win32.SetWindowsHookEx(Win32.WH_MOUSE_LL, lowLevelMouseProc, Win32.GetModuleHandle(curModule.ModuleName), 0);
            }
        }
        IntPtr HookCallbackM(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && (Win32.MouseMessages.WM_LBUTTONUP == (Win32.MouseMessages)wParam ||
                               Win32.MouseMessages.WM_RBUTTONUP == (Win32.MouseMessages)wParam))
            {
                Win32.MSLLHOOKSTRUCT hookStruct = (Win32.MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(Win32.MSLLHOOKSTRUCT));

                eventMouse?.Invoke((Win32.TypeMouse)wParam, hookStruct.pt.x, hookStruct.pt.y);
            }

            return Win32.CallNextHookEx(_hookIDM, nCode, wParam, lParam);
        } 
        #endregion

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // because we can unhook only in the same thread, not in garbage collector thread
                if (_windowsHookHandle != IntPtr.Zero)
                {
                    if (!Win32.UnhookWindowsHookEx(_windowsHookHandle))
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
                if (!Win32.FreeLibrary(_user32LibraryHandle)) // reduces reference to library by 1.
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
        private Win32.HookProc _hookProc;

        
        bool ctrlPressed;
        bool shiftPressed;
        bool save = false;
        bool reset = false;
        public bool isDebug = false;
        public Keys HotKey { get; set; } = Keys.Pause;

        public IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            bool fEatKeyStroke = false;
            //
            var wparamTyped = wParam.ToInt32();
            if (Enum.IsDefined(typeof(Win32.KeyboardState), wparamTyped))
            {
                object o = Marshal.PtrToStructure(lParam, typeof(Win32.LowLevelKeyboardInputEvent));
                Win32.LowLevelKeyboardInputEvent p = (Win32.LowLevelKeyboardInputEvent)o;
                if(isDebug)
                    System.Diagnostics.Debug.WriteLine($"system - p.Key {p.Key} - {((Win32.KeyboardState)wparamTyped)}");

                bool isKeyStrlOrShift = p.Key == Keys.LShiftKey || p.Key == Keys.RShiftKey
                    || p.Key == Keys.LControlKey || p.Key == Keys.RControlKey;
                if (isKeyStrlOrShift)
                {
                    if (p.Key == Keys.LControlKey || p.Key == Keys.RControlKey)
                        ctrlPressed = save || ((Win32.KeyboardState)wparamTyped) == Win32.KeyboardState.KeyDown
                                    || ((Win32.KeyboardState)wparamTyped) == Win32.KeyboardState.SysKeyDown;
                    if (p.Key == Keys.LShiftKey || p.Key == Keys.RShiftKey)
                        shiftPressed = save ||
                            ((Win32.KeyboardState)wparamTyped) == Win32.KeyboardState.KeyDown
                                   || ((Win32.KeyboardState)wparamTyped) == Win32.KeyboardState.SysKeyDown;

                    if(((Win32.KeyboardState)wparamTyped) == Win32.KeyboardState.KeyUp
                                   || ((Win32.KeyboardState)wparamTyped) == Win32.KeyboardState.SysKeyUp)
                    {
                        reset = true;
                    }
                }
                else
                {
                    save = ((Win32.KeyboardState)wparamTyped) == Win32.KeyboardState.KeyDown
                        || ((Win32.KeyboardState)wparamTyped) == Win32.KeyboardState.SysKeyDown;
                }

                var eventArguments = new GlobalKeyboardHookEventArgs(p, (Win32.KeyboardState)wparamTyped);
                eventArguments.CtrlPressed = ctrlPressed;
                eventArguments.ShiftPressed = shiftPressed;

                if (p.Key == HotKey)
                {
                    if ((eventArguments.KeyboardState == Win32.KeyboardState.KeyUp
                    || eventArguments.KeyboardState == Win32.KeyboardState.SysKeyUp))
                    {
                        EventHandler<GlobalKeyboardHookEventArgs> handler = KeyboardPressed;
                        handler?.Invoke(this, eventArguments);
                    }
                    return (IntPtr)1;
                }

                if ((eventArguments.KeyboardState == Win32.KeyboardState.KeyUp
                    || eventArguments.KeyboardState == Win32.KeyboardState.SysKeyUp)
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

            return fEatKeyStroke ? (IntPtr)1 : Win32.CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }


    }
}
