using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace EventHookKeyAndMouse
{
    public class EventHookKeyAndMouseClass
    {
        public enum TypeKey
        {
            KeyDown = 256,
            KeyUp = 257
        }
        public enum TypeMouse
        {
            LeftKey = 514,
            RightKey = 517
        }
        public event GetEventKey eventKey;
        public event GetEventMouse eventMouse;

        public delegate void GetEventKey(string key, int c, TypeKey w);
        public delegate void GetEventMouse(TypeMouse type, int x, int y);

        public delegate IntPtr LLKbPc(int nCode, IntPtr wParam, IntPtr lParam);
        public delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);
        /// <summary>
        /// следить ли за мышкой
        /// </summary>
        public bool IsMouse { get; set; }
        /// <summary>
        /// Следить ли за клавиатурой
        /// </summary>
        public bool IsKey { get; set; }

        #region DLL
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LLKbPc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
        #endregion

        const int WH_KEYBOARD_LL = 13;
        const int WH_MOUSE_LL = 14;
        const int WM_KEYDOWN = 0x0100; //256
        private const int WM_KEYUP = 257;

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

        public IntPtr SetHook()
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }
        public static IntPtr SetHookM()
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_MOUSE_LL, lowLevelMouseProc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                if (wParam == (IntPtr)WM_KEYUP)
                {
                    int vkCode = Marshal.ReadInt32(lParam);
                    string s = ((System.Windows.Forms.Keys)vkCode).ToString();
                    eventKey?.Invoke(s, nCode, (TypeKey)wParam);
                }
                if (wParam == (IntPtr)WM_KEYDOWN)
                {
                    int vkCode = Marshal.ReadInt32(lParam);
                    string s = ((System.Windows.Forms.Keys)vkCode).ToString();
                    eventKey?.Invoke(s, nCode, (TypeKey)wParam);
                }
            }
            //Debug.WriteLine($"-- nCode {nCode}, wParam - {wParam}, lParam - {lParam}");

            return CallNextHookEx(_hookID, nCode, wParam, lParam);
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

        IntPtr _hookID = IntPtr.Zero;
        static IntPtr _hookIDM = IntPtr.Zero;
        LLKbPc proc;
        private static LowLevelMouseProc lowLevelMouseProc;
        /// <summary>
        /// начать слежение
        /// </summary>
        public void Start()
        {
            if (IsKey)
            {
                proc = HookCallback;
                _hookID = SetHook();
                //UnhookWindowsHookEx(_hookID);
            }
            if (IsMouse)
            {
                lowLevelMouseProc = HookCallbackM;
                _hookIDM = SetHookM();
            }
        }
        //~EventHookKeyAndMouseClass()
        //{
        //    Unhook();
        //}
        public void Unhook()
        {
            if (IsKey)
            {
                UnhookWindowsHookEx(_hookID);
            }
            if (IsMouse)
            {
                UnhookWindowsHookEx(_hookIDM);
            }

        }
    }
}
