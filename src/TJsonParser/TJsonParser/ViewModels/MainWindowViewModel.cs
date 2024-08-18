using System;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MsBox.Avalonia;
using TJsonParser.Utils;

namespace TJsonParser.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public IRelayCommand<DragEventArgs> DragOverCommand { get; }
    public IAsyncRelayCommand<DragEventArgs> DropCommand { get; }
    private readonly Window _window;
    
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GoButtonClickCommand))]
    private string _inputJson = "";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GoButtonClickCommand))]
    private string _appVer = "13.9.0.1013";
    
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GoButtonClickCommand))]
    private string _platform = "670603";
    
    [ObservableProperty]
    private string _result1 = "";
    
    [ObservableProperty]
    private string _result2 = "";

    public MainWindowViewModel(Window window)
    {
        _window = window;
        DragOverCommand = new RelayCommand<DragEventArgs>(DragOverHandler);
        DropCommand = new AsyncRelayCommand<DragEventArgs>(DropHandler);
    }
    
    private void DragOverHandler(DragEventArgs? e)
    {
        if (e is null) return;
        // 检查是否有文件被拖入
        e.DragEffects = e.Data.Contains(DataFormats.Files) ? DragDropEffects.Copy : DragDropEffects.None;
    }

    private async Task DropHandler(DragEventArgs? e)
    {
        if (e is null) return;
        // 检查是否有文件被拖入
        if (!e.Data.Contains(DataFormats.Files)) return;
        
        var fileNames = e.Data.GetFiles()?.ToArray();
        if (fileNames is { Length: > 0 })
        {
            var filePath = fileNames[0].Path.LocalPath;
            var ext = Path.GetExtension(filePath);
            if (File.Exists(filePath) && ext is ".txt" or ".json")
            {
                var fileContent = await File.ReadAllTextAsync(filePath);
                InputJson = fileContent;
            }
        }
    }

    [RelayCommand(CanExecute = nameof(CanExecuteDoWork))]
    private async Task GoButtonClickAsync()
    {
        Result1 = "";
        Result2 = "";

        try
        {
            var json = InputJson;
            if (!json.Trim().StartsWith('{'))
            {
                json = OutputJsonRegex().Match(InputJson).Groups[1].Value;
            }
            
            var jsonNode = JsonNode.Parse(json)!;
            var tm = jsonNode["tm"].ToString();
            var vi = jsonNode["vl"]["vi"][0]!;
            var paramBase = vi["base"].GetValue<string>();
            var lnk = vi["lnk"].GetValue<string>();
            var title = vi["ti"].GetValue<string>();
            var video = vi["ul"]["ui"].AsArray().Last()!;
            var url = video["url"].GetValue<string>() + video["hls"]["pt"].GetValue<string>();

            Result2 = title;
            Result1 = url;
            
            var output = await NodeJSUtil.GetKeyAsync(paramBase, lnk, tm, AppVer, Platform);
            Result2 = output;
        }
        catch (Exception e)
        {
            await ShowErrorAsync(e);
        }
    }

    private bool CanExecuteDoWork() => !string.IsNullOrEmpty(InputJson) && !string.IsNullOrEmpty(AppVer) && !string.IsNullOrEmpty(Platform);
    
    private async Task ShowErrorAsync(Exception ex)
    {
        await MessageBoxManager.GetMessageBoxStandard("Error", ex.Message).ShowWindowDialogAsync(_window);
    }

    [GeneratedRegex("QZOutputJson=(.*);")]
    private static partial Regex OutputJsonRegex();
}