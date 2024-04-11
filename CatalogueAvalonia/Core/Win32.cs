using System;
using System.Runtime.InteropServices;

namespace CatalogueAvalonia.Core;

public class Win32
{
    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern int MessageBox(IntPtr hwnd, String text, String caption, uint type);
    
    
}