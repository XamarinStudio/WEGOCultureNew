using CommunityToolkit.Mvvm.ComponentModel;

namespace WEGOCulture.Models;

public partial class Question : ObservableObject
{
    [ObservableProperty]
    private int _id;

    [ObservableProperty]
    private string _questionType = string.Empty;

    [ObservableProperty]
    private string _questionText = string.Empty;

    [ObservableProperty]
    private string _optionA = string.Empty;

    [ObservableProperty]
    private string _optionB = string.Empty;

    [ObservableProperty]
    private string _optionC = string.Empty;

    [ObservableProperty]
    private string _optionD = string.Empty;

    [ObservableProperty]
    private string _answer = string.Empty;

    [ObservableProperty]
    private string _analysis = string.Empty;
}