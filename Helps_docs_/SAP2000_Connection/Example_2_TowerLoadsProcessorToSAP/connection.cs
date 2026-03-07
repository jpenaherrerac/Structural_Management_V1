using SAP2000v1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.ExceptionServices;
using System.Threading;

// Infraestructura genérica para ejecutar llamadas COM (SAP2000) en un hilo STA dedicado con message loop.
// Se reutiliza en este proyecto para mantener todas las interacciones con SAP2000 en un solo hilo STA.
public static class SapStaHost
{
 // STA runner implemented with a native message loop (no WinForms dependency).
 // Purpose: run all COM interactions on a dedicated STA thread with a message loop
 // so SAP2000 COM objects are created/released on an STA thread.
 public sealed class StaComRunner : IDisposable
 {
 const int WM_USER =0x0400;
 const int WM_INVOKE = WM_USER +1;

 [StructLayout(LayoutKind.Sequential)]
 struct POINT { public int x; public int y; }

 [StructLayout(LayoutKind.Sequential)]
 struct MSG
 {
 public IntPtr hwnd;
 public uint message;
 public IntPtr wParam;
 public IntPtr lParam;
 public uint time;
 public POINT pt;
 }

 [DllImport("user32.dll", SetLastError = true)]
 static extern bool PeekMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);

 [DllImport("user32.dll", SetLastError = true)]
 static extern int GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

 [DllImport("user32.dll")]
 static extern bool TranslateMessage([In] ref MSG lpMsg);

 [DllImport("user32.dll")]
 static extern IntPtr DispatchMessage([In] ref MSG lpMsg);

 [DllImport("user32.dll", SetLastError = true)]
 static extern bool PostThreadMessage(uint idThread, uint Msg, IntPtr wParam, IntPtr lParam);

 [DllImport("kernel32.dll")]
 static extern uint GetCurrentThreadId();

 [DllImport("ole32.dll")]
 static extern int CoInitializeEx(IntPtr pvReserved, uint dwCoInit);

 [DllImport("ole32.dll")]
 static extern void CoUninitialize();

 const uint COINIT_APARTMENTTHREADED =0x2;
 const uint PM_NOREMOVE =0x0000;

 readonly Thread staThread;
 uint threadId;
 readonly AutoResetEvent ready = new AutoResetEvent(false);
 bool disposed;

 public StaComRunner()
 {
 // Start STA thread and wait until its COM and message queue are ready.
 staThread = new Thread(Run) { IsBackground = true };
 staThread.SetApartmentState(ApartmentState.STA);
 staThread.Start();
 // Wait for thread to initialize COM and message queue
 ready.WaitOne();
 }

 void Run()
 {
 // Initialize COM for STA explicitly
 CoInitializeEx(IntPtr.Zero, COINIT_APARTMENTTHREADED);

 // Force creation of message queue for this thread
 MSG msg;
 PeekMessage(out msg, IntPtr.Zero,0,0, PM_NOREMOVE);

 threadId = GetCurrentThreadId();
 ready.Set();

 // Standard message loop
 while (true)
 {
 int res = GetMessage(out msg, IntPtr.Zero,0,0);
 if (res ==0)
 break; // WM_QUIT
 if (res == -1)
 break; // error

 if (msg.message == WM_INVOKE)
 {
 // wParam contains a GCHandle to an InvocationInfo
 var h = GCHandle.FromIntPtr(msg.wParam);
 try
 {
 var info = (InvocationInfo)h.Target;
 try
 { info.Action(); }
 catch (Exception ex) { info.Exception = ex; }
 finally
 { info.Event.Set(); }
 }
 finally { h.Free(); }

 continue;
 }

 TranslateMessage(ref msg);
 DispatchMessage(ref msg);
 }

 CoUninitialize();
 }

 class InvocationInfo
 {
 public Action Action;
 public ManualResetEventSlim Event = new ManualResetEventSlim(false);
 public Exception Exception;
 }

 public void Invoke(Action action)
 {
 if (disposed) throw new ObjectDisposedException(nameof(StaComRunner));
 var info = new InvocationInfo { Action = action };
 var handle = GCHandle.Alloc(info);
 bool posted = PostThreadMessage(threadId, WM_INVOKE, GCHandle.ToIntPtr(handle), IntPtr.Zero);
 if (!posted)
 {
 handle.Free();
 throw new InvalidOperationException("Failed to post message to STA thread.");
 }

 // Wait for completion
 info.Event.Wait();
 if (info.Exception != null)
 {
 // Re-throw original exception preserving stack to reveal root cause
 ExceptionDispatchInfo.Capture(info.Exception).Throw();
 }
 }

 public T Invoke<T>(Func<T> func)
 {
 T result = default;
 Invoke(() => { result = func(); });
 return result;
 }

 public void Dispose()
 {
 if (disposed) return;
 disposed = true;
 // Post WM_QUIT to end message loop
 PostThreadMessage(threadId,0x0012 /* WM_QUIT */, IntPtr.Zero, IntPtr.Zero);
 // Wait for thread to exit
 staThread.Join();
 ready.Dispose();
 }
 }

 // Método de ayuda de alto nivel para usar SapProcessor desde un hilo STA dedicado.
 // Ejemplo de uso desde la UI:
 // using (var runner = SapStaHost.CreateRunner()) {
 // runner.Invoke(() => {
 // var proc = new SapProcessor();
 // proc.ConnectAndInit();
 // proc.UnlockAndRefreshView();
 // ...
 // proc.ReleaseCom();
 // });
 // }
 public static StaComRunner CreateRunner()
 {
 return new StaComRunner();
 }
}
