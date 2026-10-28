using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WEGOCulture.Models;

namespace WEGOCulture.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private string _searchTitle = string.Empty;

        // 界面选择模式：0 - 刷题模式, 1 - 搜索模式
        [ObservableProperty]
        private int _selectedModeIndex = 0;

        // 方便 XAML 进行 IsVisible 绑定
        [ObservableProperty]
        private bool _isSearchMode = false;

        [ObservableProperty]
        private ObservableCollection<Question> _questionList = new();

        [ObservableProperty]
        private Question? _selectedQuestion;

        [ObservableProperty]
        private int _currentIndex = 0;

        // 用于前端显示的 1-based 题号 (1, 2, 3...)
        [ObservableProperty]
        private int _currentDisplayIndex = 1;

        [ObservableProperty]
        private Question? _currentQuestion;

        public MainViewModel()
        {
            // 初始化时默认加载所有题目（用于刷题模式）
            LoadAllQuestions();
        }

        // 监听模式切换
        partial void OnSelectedModeIndexChanged(int value)
        {
            IsSearchMode = value == 1;
        }

        // 监听当前索引改变，更新当前题目与显示题号
        partial void OnCurrentIndexChanged(int value)
        {
            CurrentDisplayIndex = value + 1;
            UpdateCurrentQuestion();
        }

        // 搜索框文本绑定
        public string SearchTitle
        {
            get => _searchTitle;
            set
            {
                if (SetProperty(ref _searchTitle, value))
                {
                    PerformSearch();
                }
            }
        }

        // 加载全部题目
        private void LoadAllQuestions()
        {
            try
            {
                var allQuestions = SqliteHelper.GetAllQuestions();
                QuestionList.Clear();
                foreach (var q in allQuestions)
                {
                    QuestionList.Add(q);
                }

                CurrentIndex = 0;
                UpdateCurrentQuestion();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"初始加载题目失败: {ex.Message}");
            }
        }

        // 实时搜索逻辑
        private void PerformSearch()
        {
            if (string.IsNullOrWhiteSpace(SearchTitle))
            {
                // 搜索框清空时，重新加载所有题目
                LoadAllQuestions();
                return;
            }

            var results = SqliteHelper.SearchQuestions(SearchTitle.Trim());

            QuestionList.Clear();
            foreach (var question in results)
            {
                QuestionList.Add(question);
            }

            SelectedQuestion = null;
            CurrentIndex = 0;
            UpdateCurrentQuestion();
        }

        // 刷新当前题目
        private void UpdateCurrentQuestion()
        {
            if (QuestionList != null && QuestionList.Count > 0)
            {
                int safeIndex = Math.Clamp(CurrentIndex, 0, QuestionList.Count - 1);
                CurrentQuestion = QuestionList[safeIndex];
            }
            else
            {
                CurrentQuestion = null;
            }
        }

        // 下一题命令
        [RelayCommand]
        private void NextQuestion()
        {
            if (QuestionList != null && CurrentIndex < QuestionList.Count - 1)
            {
                CurrentIndex++;
            }
        }

        // 上一题命令
        [RelayCommand]
        private void PreviousQuestion()
        {
            if (CurrentIndex > 0)
            {
                CurrentIndex--;
            }
        }
    }
}