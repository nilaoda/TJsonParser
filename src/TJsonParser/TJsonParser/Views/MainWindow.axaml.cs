using Avalonia.Controls;
using Avalonia.Input;
using TJsonParser.ViewModels;

namespace TJsonParser.Views;

public partial class MainWindow : Window
{
    private MainWindowViewModel? ViewModel => DataContext as MainWindowViewModel;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel(this);

        // 为 TextBox 添加 DragOver 和 Drop 事件处理程序
        Txt_InputJson.AddHandler(DragDrop.DragOverEvent, DragOverHandler);
        Txt_InputJson.AddHandler(DragDrop.DropEvent, DropHandler);
    }

    private void DragOverHandler(object? sender, DragEventArgs e)
    {
        ViewModel!.DragOverCommand.Execute(e);
    }

    private async void DropHandler(object? sender, DragEventArgs e)
    {
        await ViewModel!.DropCommand.ExecuteAsync(e);
    }
}