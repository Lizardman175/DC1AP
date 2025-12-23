using DC1AP.ViewModels;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace DC1AP.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        InitializeTitleBarDragging();
        if (!(DataContext is MainWindowViewModel viewModel)) return;
        var logListBox = this.FindControl<ListBox>("Log");
        var hintListBox = this.FindControl<ListBox>("HintList");
        var itemListBox = this.FindControl<ListBox>("ItemList");

        viewModel.LogList.CollectionChanged += (o, e) => ScrollToEnd(logListBox);
        viewModel.HintList.CollectionChanged += (o, e) => ScrollToEnd(hintListBox);
        viewModel.ItemList.CollectionChanged += (o, e) => ScrollToEnd(itemListBox);
    }

    private void InitializeTitleBarDragging()
    {
        var titleBar = this.FindControl<Border>("HeaderBar");
        titleBar.PointerPressed += (s, e) =>
        {
            if (e.GetCurrentPoint(titleBar).Properties.IsLeftButtonPressed)
            {
                this.BeginMoveDrag(e);
            }
        };
    }

    public void ScrollToEnd(ListBox listBox)
    {
        if (!(DataContext is MainWindowViewModel viewModel)) return;
        if (viewModel.AutoscrollEnabled)
        {
            if (listBox != null && listBox.ItemCount > 0)
            {
                listBox.ScrollIntoView(listBox.ItemCount - 1);
            }
        }
    }
}
