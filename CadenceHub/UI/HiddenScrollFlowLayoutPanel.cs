using System.Runtime.InteropServices;

namespace CadenceHub.UI;

public sealed class HiddenScrollFlowLayoutPanel : FlowLayoutPanel
{
    private const int ScrollBarBoth = 3;

    public HiddenScrollFlowLayoutPanel()
    {
        AutoScroll = true;
        FlowDirection = FlowDirection.TopDown;
        WrapContents = false;
        DoubleBuffered = true;
    }

    [DllImport("user32.dll")]
    private static extern bool ShowScrollBar(IntPtr hWnd, int wBar, bool bShow);

    protected override void OnControlAdded(ControlEventArgs e)
    {
        base.OnControlAdded(e);
        HideScrollBars();
    }

    protected override void OnLayout(LayoutEventArgs levent)
    {
        base.OnLayout(levent);
        HideScrollBars();
    }

    protected override void OnResize(EventArgs eventargs)
    {
        base.OnResize(eventargs);
        HideScrollBars();
    }

    protected override void WndProc(ref Message m)
    {
        base.WndProc(ref m);
        HideScrollBars();
    }

    private void HideScrollBars()
    {
        if (!IsHandleCreated)
        {
            return;
        }

        ShowScrollBar(Handle, ScrollBarBoth, false);
    }
}
