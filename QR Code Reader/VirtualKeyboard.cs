using System;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace barcode_Reader
{
    public class VirtualKeyboard
    {

    
        [DllImport("user32.dll")]
        private static extern uint SendInput(uint nInputs, ref INPUT pInputs, int cbSize);

        [DllImport("user32.dll", EntryPoint = "GetMessageExtraInfo", SetLastError = true)]
        private static extern IntPtr GetMessageExtraInfo();

        //Simulate mouse   
        public static void Click()
        {
            var input_down = new INPUT();
            input_down.mi.dx = 0;
            input_down.mi.dy = 0;
            input_down.mi.mouseData = 0;
            input_down.mi.dwFlags = (int) MOUSEEVENTF.LEFTDOWN;
            SendInput(1, ref input_down, Marshal.SizeOf(input_down));
            var input_up = input_down;
            input_up.mi.dwFlags = (int) MOUSEEVENTF.LEFTUP;
            SendInput(1, ref input_up, Marshal.SizeOf(input_up));
        }

        //Simulate keystrokes  Send unicode characters to send any character
        
        public static void SendUnicode(string message)
        {
            for (var i = 0; i < message.Length; i++)
            {
                var input_down = new INPUT
                {
                    type = (int)InputType.INPUT_KEYBOARD
                };
                input_down.ki.dwFlags = (int) KEYEVENTF.UNICODE;
                input_down.ki.wScan = (short) message[i];
                input_down.ki.wVk = 0;
                SendInput(1, ref input_down, Marshal.SizeOf(input_down)); //keydown     
                var input_up = new INPUT
                {
                    type = (int)InputType.INPUT_KEYBOARD
                };
                input_up.ki.wScan = (short) message[i];
                input_up.ki.wVk = 0;
                input_up.ki.dwFlags = (int) (KEYEVENTF.KEYUP | KEYEVENTF.UNICODE);
                SendInput(1, ref input_up, Marshal.SizeOf(input_up)); //keyup      
            }
        }

        //Simulate keystrokes 
        public static void SendKeyBoradKey(short key)
        {
            var input_down = new INPUT
            {
                type = (int)InputType.INPUT_KEYBOARD
            };
            input_down.ki.dwFlags = 0;
            input_down.ki.wVk = key;
            SendInput(1, ref input_down, Marshal.SizeOf(input_down)); //keydown     

            var input_up = new INPUT
            {
                type = (int)InputType.INPUT_KEYBOARD
            };
            input_up.ki.wVk = key;
            input_up.ki.dwFlags = (int) KEYEVENTF.KEYUP;
            SendInput(1, ref input_up, Marshal.SizeOf(input_up)); //keyup      
        }

        //Send non-unicode characters, only send lowercase letters and numbers (发送非unicode字符，只能发送小写字母和数字)     
        public static void SendNoUnicode(string message)
        {
            var str = "abcdefghijklmnopqrstuvwxyz";
            for (var i = 0; i < message.Length; i++)
            {
                short sendChar;
                if (str.IndexOf(message[i].ToString().ToLower()) > -1)
                    sendChar = (short) GetKeysByChar(message[i]);
                else
                    sendChar = (short) message[i];
                var input_down = new INPUT
                {
                    type = (int)InputType.INPUT_KEYBOARD
                };
                input_down.ki.dwFlags = 0;
                input_down.ki.wVk = sendChar;
                SendInput(1, ref input_down, Marshal.SizeOf(input_down)); //keydown     
                var input_up = new INPUT
                {
                    type = (int)InputType.INPUT_KEYBOARD
                };
                input_up.ki.wVk = sendChar;
                input_up.ki.dwFlags = (int) KEYEVENTF.KEYUP;
                SendInput(1, ref input_up, Marshal.SizeOf(input_up)); //keyup      
            }
        }

        private static Key GetKeysByChar(char c)
        {
            var str = "abcdefghijklmnopqrstuvwxyz";
            var index = str.IndexOf(c.ToString().ToLower());
            switch (index)
            {
                case 0:

                    return Key.A;
                case 1:
                    return Key.B;
                case 2:
                    return Key.C;
                case 3:
                    return Key.D;
                case 4:
                    return Key.E;
                case 5:
                    return Key.F;
                case 6:
                    return Key.G;
                case 7:
                    return Key.H;
                case 8:
                    return Key.I;
                case 9:
                    return Key.J;
                case 10:
                    return Key.K;
                case 11:
                    return Key.L;
                case 12:
                    return Key.M;
                case 13:
                    return Key.N;
                case 14:
                    return Key.O;
                case 15:
                    return Key.P;
                case 16:
                    return Key.Q;
                case 17:
                    return Key.R;
                case 18:
                    return Key.S;
                case 19:
                    return Key.T;
                case 20:
                    return Key.U;
                case 21:
                    return Key.V;
                case 22:
                    return Key.W;
                case 23:
                    return Key.X;
                case 24:
                    return Key.Y;
                default:
                    return Key.Z;
            }
        }

        private enum InputType
        {
            INPUT_MOUSE = 0,
            INPUT_KEYBOARD = 1,
            INPUT_HARDWARE = 2
        }

        [Flags]
        private enum MOUSEEVENTF
        {
            MOVE = 0x0001, //mouse move     
            LEFTDOWN = 0x0002, //left button down     
            LEFTUP = 0x0004, //left button up     
            RIGHTDOWN = 0x0008, //right button down     
            RIGHTUP = 0x0010, //right button up     
            MIDDLEDOWN = 0x0020, //middle button down     
            MIDDLEUP = 0x0040, //middle button up     
            XDOWN = 0x0080, //x button down     
            XUP = 0x0100, //x button down     
            WHEEL = 0x0800, //wheel button rolled     
            VIRTUALDESK = 0x4000, //map to entire virtual desktop     
            ABSOLUTE = 0x8000 //absolute move     
        }

        [Flags]
        private enum KEYEVENTF
        {
            EXTENDEDKEY = 0x0001,
            KEYUP = 0x0002,
            UNICODE = 0x0004,
            SCANCODE = 0x0008
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct INPUT
        {
            [FieldOffset(0)] public int type; //0-MOUSEINPUT;1-KEYBDINPUT;2-HARDWAREINPUT     
            [FieldOffset(4)] public KEYBDINPUT ki;
            [FieldOffset(4)] public MOUSEINPUT mi;
            [FieldOffset(4)] public readonly HARDWAREINPUT hi;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public int mouseData;
            public int dwFlags;
            public readonly int time;
            public readonly IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KEYBDINPUT
        {
            public short wVk;
            public short wScan;
            public int dwFlags;
            public readonly int time;
            public readonly IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct HARDWAREINPUT
        {
            public readonly int uMsg;
            public readonly short wParamL;
            public readonly short wParamH;
        }
    }
}