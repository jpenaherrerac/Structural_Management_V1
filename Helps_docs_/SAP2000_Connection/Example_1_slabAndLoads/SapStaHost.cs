    using System;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace App.Infrastructure.Sap2000
{
 /// <summary>
 /// Runs SAP2000 COM calls on a dedicated STA thread with a native Win32 message loop.
 /// Pattern based on docs/Referencia de otro proyecto_Connection_to_sap_2000_via_com_api.md.
 /// </summary>
 public static class SapStaHost
 {
 public sealed class StaComRunner : IDisposable
 {
 private const int WM_USER =0x0400;
 private const int WM_INVOKE = WM_USER +1;
 private const int WM_QUIT =0x0012;

 private readonly Thread _staThread;
 private uint _threadId;
 private readonly AutoResetEvent _ready = new AutoResetEvent(false);
 private bool _disposed;

 private sealed class InvocationInfo
 {
 public Action Action;
 public ManualResetEventSlim Done = new ManualResetEventSlim(false);
 public Exception Exception;
 }

 public StaComRunner()
 {
 _staThread = new Thread(Run) { IsBackground = true };
 _staThread.SetApartmentState(ApartmentState.STA);
 _staThread.Start();
 _ready.WaitOne();
 }

 public void Invoke(Action action)
 {
 if (action == null) throw new ArgumentNullException(nameof(action));
 if (_disposed) throw new ObjectDisposedException(nameof(StaComRunner));

 var info = new InvocationInfo { Action = action };
 var handle = GCHandle.Alloc(info);
 try
 {
 if (!PostThreadMessage(_threadId, WM_INVOKE, GCHandle.ToIntPtr(handle), IntPtr.Zero))
 throw new InvalidOperationException("Failed to post invoke message to STA thread.");

 info.Done.Wait();

 if (info.Exception != null)
 ExceptionDispatchInfo.Capture(info.Exception).Throw();
 }
 finally
 {
 // Freed by STA thread when message is processed; if PostThreadMessage failed, free here.
 // If posted successfully, STA thread frees.
 }
 }

 public T Invoke<T>(Func<T> func)
 {
 if (func == null) throw new ArgumentNullException(nameof(func));
 T result = default(T);
 Invoke(() => result = func());
 return result;
 }

 private void Run()
 {
 CoInitializeEx(IntPtr.Zero, COINIT_APARTMENTTHREADED);
 try
 {
 // Force message queue creation.
 MSG msg;
 PeekMessage(out msg, IntPtr.Zero,0,0, PM_NOREMOVE);

 _threadId = GetCurrentThreadId();
 _ready.Set();

 while (true)
 {
 int res = GetMessage(out msg, IntPtr.Zero,0,0);
 if (res ==0 || res == -1) break;

 if (msg.message == WM_INVOKE)
 {
 var h = GCHandle.FromIntPtr(msg.wParam);
 try
 {
 var info = (InvocationInfo)h.Target;
 try
 {
 info.Action();
 }
 catch (Exception ex)
 {
 info.Exception = ex;
 }
 finally
 {
 info.Done.Set();
 }
 }
 finally
 {
 h.Free();
 }
 continue;
 }

 TranslateMessage(ref msg);
 DispatchMessage(ref msg);
 }
 }
 finally
 {
 CoUninitialize();
 }
 }

 public void Dispose()
 {
 if (_disposed) return;
 _disposed = true;

 try
 {
 PostThreadMessage(_threadId, WM_QUIT, IntPtr.Zero, IntPtr.Zero);
 }
 catch { }

 try { _staThread.Join(2000); } catch { }
 try { _ready.Dispose(); } catch { }
 }

 // ===== Win32 + COM interop =====

 private const uint COINIT_APARTMENTTHREADED =0x2;
 private const uint PM_NOREMOVE =0x0000;

 [StructLayout(LayoutKind.Sequential)]
 private struct POINT { public int x; public int y; }

 [StructLayout(LayoutKind.Sequential)]
 private struct MSG
 {
 public IntPtr hwnd;
 public uint message;
 public IntPtr wParam;
 public IntPtr lParam;
 public uint time;
 public POINT pt;
 }

 [DllImport("kernel32.dll")]
 private static extern uint GetCurrentThreadId();

 [DllImport("user32.dll")]
 private static extern bool PeekMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);

 [DllImport("user32.dll")]
 private static extern int GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

 [DllImport("user32.dll")]
 private static extern bool TranslateMessage([In] ref MSG lpMsg);

 [DllImport("user32.dll")]
 private static extern IntPtr DispatchMessage([In] ref MSG lpMsg);

 [DllImport("user32.dll")]
 private static extern bool PostThreadMessage(uint idThread, int Msg, IntPtr wParam, IntPtr lParam);

 [DllImport("ole32.dll")]
 private static extern int CoInitializeEx(IntPtr pvReserved, uint dwCoInit);

 [DllImport("ole32.dll")]
 private static extern void CoUninitialize();
 }

 public static StaComRunner CreateRunner() => new StaComRunner();
 }
}
