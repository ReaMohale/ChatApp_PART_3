using C;
using ChatApp_Part3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace ChatApp_Part3
{
    public partial class MainWindow : Window
    {
        private SecurityBot securityBot;
        private TaskStore taskStore;
        private LogBook logBook;
        private QuizMaster quizMaster;
        private int logDisplayCount = 10;

        public MainWindow()
        {
            InitializeComponent();
            securityBot = new SecurityBot();
            taskStore = new TaskStore();
            logBook = new LogBook();
            quizMaster = new QuizMaster(logBook);

            ThreatResponseEngine.ActivityLogged += OnActivityLogged;
            Loaded += MainWindow_Loaded;
        }

        private void OnActivityLogged(string description)
        {
            Dispatcher.Invoke(() => RefreshActivityLog());
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            logBook.Log("System started", "SecureShield Awareness System initialized");
            ThreatResponseEngine.WriteSessionMarker(starting: true);

            _ = Task.Run(() => PlayStartupAudio());

            await DisplayShieldMessage("🛡 SecureShield Awareness System is online.");
            await Task.Delay(200);
            await DisplayShieldMessage("Your digital security is our mission.");
            await Task.Delay(200);
            await DisplayShieldMessage("State your agent name to begin.");

            await Task.Run(() => taskStore.LoadTasks());
            RefreshTaskList();
        }

        private void ChatTabButton_Click(object sender, RoutedEventArgs e)
        {
            ShowChatPanel();
            UpdateTabHighlight(ChatTabButton);
        }

        private void TasksTabButton_Click(object sender, RoutedEventArgs e)
        {
            ShowTaskPanel();
            UpdateTabHighlight(TasksTabButton);
        }

        private void QuizTabButton_Click(object sender, RoutedEventArgs e)
        {
            ShowQuizPanel();
            UpdateTabHighlight(QuizTabButton);
        }

        private void ActivityTabButton_Click(object sender, RoutedEventArgs e)
        {
            ShowActivityLogPanel();
            UpdateTabHighlight(ActivityTabButton);
            RefreshActivityLog();
        }

        private void UpdateTabHighlight(Button activeTab)
        {
            foreach (var tab in new[] { ChatTabButton, TasksTabButton, QuizTabButton, ActivityTabButton })
            {
                tab.Foreground = tab == activeTab ?
                    new SolidColorBrush(Color.FromRgb(0x4A, 0xDE, 0x80)) :
                    new SolidColorBrush(Color.FromRgb(0x6B, 0x72, 0x80));
            }
        }

        private void ShowChatPanel()
        {
            TaskPanelBorder.Visibility = Visibility.Collapsed;
            QuizPanelBorder.Visibility = Visibility.Collapsed;
            ActivityLogPanelBorder.Visibility = Visibility.Collapsed;
            DrillPanelBorder.Visibility = Visibility.Collapsed;
        }

        private void ShowTaskPanel()
        {
            TaskPanelBorder.Visibility = Visibility.Visible;
            QuizPanelBorder.Visibility = Visibility.Collapsed;
            ActivityLogPanelBorder.Visibility = Visibility.Collapsed;
            DrillPanelBorder.Visibility = Visibility.Collapsed;
            RefreshTaskList();
        }

        private void ShowQuizPanel()
        {
            QuizPanelBorder.Visibility = Visibility.Visible;
            TaskPanelBorder.Visibility = Visibility.Collapsed;
            ActivityLogPanelBorder.Visibility = Visibility.Collapsed;
            DrillPanelBorder.Visibility = Visibility.Collapsed;
        }

        private void ShowActivityLogPanel()
        {
            ActivityLogPanelBorder.Visibility = Visibility.Visible;
            TaskPanelBorder.Visibility = Visibility.Collapsed;
            QuizPanelBorder.Visibility = Visibility.Collapsed;
            DrillPanelBorder.Visibility = Visibility.Collapsed;
            RefreshActivityLog();
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
            => await HandleAgentInput();

        private async void InputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Keyboard.Modifiers != ModifierKeys.Shift)
            {
                e.Handled = true;
                await HandleAgentInput();
            }
        }

        private async void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ChatPanel.Children.Clear();
            logBook.Log("Chat cleared", "User cleared the chat interface");
            await DisplayShieldMessage("🧹 Channel cleared. SecureShield is standing by.");
        }

        private async void StartDrillButton_Click(object sender, RoutedEventArgs e)
        {
            InputTextBox.Text = "drill";
            await HandleAgentInput();
        }

        private async void ViewTasksButton_Click(object sender, RoutedEventArgs e)
        {
            ShowTaskPanel();
            await DisplayShieldMessage(GetTaskSummary());
        }

        private async void ViewActivityLogButton_Click(object sender, RoutedEventArgs e)
        {
            ShowActivityLogPanel();
        }

        private async void ViewProfileButton_Click(object sender, RoutedEventArgs e)
        {
            InputTextBox.Text = "profile";
            await HandleAgentInput();
        }

        private async void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            InputTextBox.Text = "help";
            await HandleAgentInput();
        }

        private async void AddTaskButton_Click(object sender, RoutedEventArgs e)
        {
            string taskDescription = TaskInputTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(taskDescription))
            {
                await DisplayShieldMessage("⚠️ Please enter a task description.");
                return;
            }

            var task = new SecurityTask
            {
                Title = taskDescription.Length > 50 ? taskDescription.Substring(0, 50) : taskDescription,
                Description = taskDescription,
                CreatedAt = DateTime.Now,
                IsComplete = false
            };

            await Task.Run(() => taskStore.AddTask(task));
            logBook.Log($"Task added: {task.Title}", "User added a new cybersecurity task");
            TaskInputTextBox.Clear();
            RefreshTaskList();
            await DisplayShieldMessage($"✅ Task added: '{task.Title}'");
        }

        private async void CompleteTaskButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int taskId)
            {
                await Task.Run(() => taskStore.MarkTaskComplete(taskId));
                var task = taskStore.GetTask(taskId);
                logBook.Log($"Task completed: {task?.Title}", "User marked task as complete");
                RefreshTaskList();
                await DisplayShieldMessage($"✅ Task marked as complete!");
            }
        }

        private async void DeleteTaskButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int taskId)
            {
                var task = taskStore.GetTask(taskId);
                await Task.Run(() => taskStore.DeleteTask(taskId));
                logBook.Log($"Task deleted: {task?.Title}", "User deleted a task");
                RefreshTaskList();
                await DisplayShieldMessage($"🗑️ Task deleted.");
            }
        }

        private void RefreshTaskList()
        {
            TaskListPanel.Children.Clear();
            var tasks = taskStore.GetAllTasks();

            if (!tasks.Any())
            {
                TaskListPanel.Children.Add(new TextBlock
                {
                    Text = "📋 No tasks yet. Add one above!",
                    Foreground = new SolidColorBrush(Color.FromRgb(0x6B, 0x72, 0x80)),
                    FontSize = 13,
                    Margin = new Thickness(0, 10, 0, 0)
                });
                return;
            }

            foreach (var task in tasks.OrderByDescending(t => t.CreatedAt))
            {
                var border = new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(0x0D, 0x1A, 0x0D)),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(0x1B, 0x4D, 0x1B)),
                    BorderThickness = new Thickness(0, 0, 0, 1),
                    Padding = new Thickness(10, 8, 10, 8),
                    Margin = new Thickness(0, 2, 0, 2)
                };

                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var statusText = task.IsComplete ? "✅" : "⬜";
                var taskText = new TextBlock
                {
                    Text = $"{statusText} {task.Title}",
                    Foreground = task.IsComplete ?
                        new SolidColorBrush(Color.FromRgb(0x6B, 0x72, 0x80)) :
                        new SolidColorBrush(Color.FromRgb(0x86, 0xEF, 0xAC)),
                    FontSize = 13,
                    TextWrapping = TextWrapping.Wrap,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetColumn(taskText, 0);
                grid.Children.Add(taskText);

                if (!task.IsComplete)
                {
                    var completeBtn = new Button
                    {
                        Content = "✅ Complete",
                        Style = (Style)FindResource("TaskButton"),
                        Tag = task.Id,
                        Margin = new Thickness(5, 0, 0, 0),
                        Width = 80
                    };
                    completeBtn.Click += CompleteTaskButton_Click;
                    Grid.SetColumn(completeBtn, 1);
                    grid.Children.Add(completeBtn);
                }

                var deleteBtn = new Button
                {
                    Content = "🗑️",
                    Style = (Style)FindResource("DangerButton"),
                    Tag = task.Id,
                    Margin = new Thickness(5, 0, 0, 0),
                    Width = 40,
                    FontSize = 12
                };
                deleteBtn.Click += DeleteTaskButton_Click;
                Grid.SetColumn(deleteBtn, 2);
                grid.Children.Add(deleteBtn);

                border.Child = grid;
                TaskListPanel.Children.Add(border);
            }
        }

        private string GetTaskSummary()
        {
            var tasks = taskStore.GetAllTasks();
            int total = tasks.Count();
            int complete = tasks.Count(t => t.IsComplete);
            int pending = total - complete;

            return $"📋 TASK SUMMARY\n────────────────────────────────────────────────\n" +
                   $"Total tasks: {total}\n" +
                   $"✅ Completed: {complete}\n" +
                   $"⏳ Pending: {pending}\n\n" +
                   (total > 0 ? "Type 'show tasks' to view all tasks." : "No tasks yet. Add one by typing 'add task: [description]'");
        }

        private async void StartQuizButton_Click(object sender, RoutedEventArgs e)
        {
            await StartQuiz();
        }

        private async Task StartQuiz()
        {
            var question = quizMaster.StartQuiz();
            if (question == null)
            {
                await DisplayShieldMessage("⚠️ No quiz questions available.");
                return;
            }

            QuizScoreText.Text = $"Score: {quizMaster.Score} / {quizMaster.TotalQuestions}";
            QuizQuestionText.Text = question.Question;
            QuizFeedbackText.Text = string.Empty;

            QuizOptionsPanel.Children.Clear();
            for (int i = 0; i < question.Options.Count; i++)
            {
                int optionIndex = i;
                var btn = new Button
                {
                    Content = $"{(char)('A' + i)}. {question.Options[i]}",
                    Style = (Style)FindResource("QuizOptionButton"),
                    Tag = optionIndex,
                    Margin = new Thickness(0, 3, 0, 3),
                    FontSize = 12
                };
                btn.Click += async (s, args) => await HandleQuizAnswer(optionIndex);
                QuizOptionsPanel.Children.Add(btn);
            }
        }

        private async Task HandleQuizAnswer(int selectedIndex)
        {
            var result = quizMaster.AnswerQuestion(selectedIndex);
            QuizFeedbackText.Text = result.IsCorrect ?
                $"✅ Correct! {result.Explanation}" :
                $"❌ Incorrect. {result.Explanation}";
            QuizScoreText.Text = $"Score: {quizMaster.Score} / {quizMaster.TotalQuestions}";

            foreach (Button btn in QuizOptionsPanel.Children)
            {
                btn.IsEnabled = false;
            }

            logBook.Log($"Quiz answer: {(result.IsCorrect ? "Correct" : "Incorrect")}",
                $"Question: {result.QuestionText}");

            if (result.QuizComplete)
            {
                await Task.Delay(1500);
                await DisplayShieldMessage($"🎉 Quiz Complete!\n" +
                    $"Final Score: {quizMaster.Score} / {quizMaster.TotalQuestions}\n" +
                    $"{quizMaster.GetPerformanceMessage()}");
                await StartQuiz();
            }
            else
            {
                await Task.Delay(1000);
                await StartQuiz();
            }
        }

        private void RefreshActivityLog()
        {
            var entries = logBook.GetRecentEntries(logDisplayCount);
            ActivityLogItemsControl.ItemsSource = entries;
        }

        private void ShowMoreLogButton_Click(object sender, RoutedEventArgs e)
        {
            logDisplayCount += 10;
            RefreshActivityLog();
        }

        private void ClearLogButton_Click(object sender, RoutedEventArgs e)
        {
            logBook.ClearLog();
            RefreshActivityLog();
        }

        private void RefreshLogButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshActivityLog();
        }

        private async Task HandleAgentInput()
        {
            string agentInput = InputTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(agentInput)) return;

            if (agentInput.StartsWith("add task:", StringComparison.OrdinalIgnoreCase) ||
                agentInput.StartsWith("add task ", StringComparison.OrdinalIgnoreCase))
            {
                string taskDesc = agentInput.Substring(agentInput.IndexOf(':') + 1).Trim();
                if (string.IsNullOrEmpty(taskDesc))
                {
                    await DisplayShieldMessage("⚠️ Please specify a task description.");
                    return;
                }

                var task = new SecurityTask
                {
                    Title = taskDesc.Length > 50 ? taskDesc.Substring(0, 50) : taskDesc,
                    Description = taskDesc,
                    CreatedAt = DateTime.Now,
                    IsComplete = false
                };

                await Task.Run(() => taskStore.AddTask(task));
                logBook.Log($"Task added: {task.Title}", "User added a new cybersecurity task");
                RefreshTaskList();
                await DisplayShieldMessage($"✅ Task added: '{task.Title}'\n\nWould you like to set a reminder? Type 'remind [days]' for this task.");
                InputTextBox.Clear();
                InputTextBox.Focus();
                return;
            }

            if (agentInput.StartsWith("remind ", StringComparison.OrdinalIgnoreCase))
            {
                var parts = agentInput.Split(' ');
                if (parts.Length >= 2 && int.TryParse(parts[1], out int days))
                {
                    var tasks = taskStore.GetAllTasks().Where(t => !t.IsComplete).ToList();
                    if (tasks.Any())
                    {
                        var task = tasks.Last();
                        task.ReminderDate = DateTime.Now.AddDays(days);
                        await Task.Run(() => taskStore.UpdateTask(task));
                        logBook.Log($"Reminder set for task: {task.Title} in {days} days", "User set a task reminder");
                        await DisplayShieldMessage($"✅ Reminder set for '{task.Title}' in {days} days.");
                    }
                    else
                    {
                        await DisplayShieldMessage("⚠️ No pending tasks to set a reminder for.");
                    }
                    InputTextBox.Clear();
                    InputTextBox.Focus();
                    return;
                }
            }

            if (agentInput.StartsWith("complete task", StringComparison.OrdinalIgnoreCase))
            {
                var tasks = taskStore.GetAllTasks().Where(t => !t.IsComplete).ToList();
                if (tasks.Any())
                {
                    var task = tasks.Last();
                    await Task.Run(() => taskStore.MarkTaskComplete(task.Id));
                    logBook.Log($"Task completed: {task.Title}", "User marked task as complete");
                    RefreshTaskList();
                    await DisplayShieldMessage($"✅ Task '{task.Title}' marked as complete!");
                }
                else
                {
                    await DisplayShieldMessage("✅ No pending tasks to complete!");
                }
                InputTextBox.Clear();
                InputTextBox.Focus();
                return;
            }

            if (agentInput.Equals("show tasks", StringComparison.OrdinalIgnoreCase))
            {
                ShowTaskPanel();
                await DisplayShieldMessage(GetTaskSummary());
                InputTextBox.Clear();
                InputTextBox.Focus();
                return;
            }

            if (agentInput.Equals("show activity log", StringComparison.OrdinalIgnoreCase) ||
                agentInput.Equals("what have you done for me", StringComparison.OrdinalIgnoreCase))
            {
                ShowActivityLogPanel();
                await DisplayShieldMessage(logBook.GetFormattedLog(10));
                InputTextBox.Clear();
                InputTextBox.Focus();
                return;
            }

            RenderAgentBubble(agentInput);
            InputTextBox.Clear();
            InputTextBox.Focus();

            ToggleProcessingIndicator(true);

            string response = await Task.Run(() =>
                ThreatResponseEngine.Dispatch(agentInput, securityBot, taskStore, logBook));

            ToggleProcessingIndicator(false);
            await DisplayShieldMessage(response);

            RefreshDrillPanel();
            RefreshTaskList();
            RefreshActivityLog();

            await Task.Delay(50);
            ChatScrollViewer.ScrollToBottom();
        }

        private void RenderAgentBubble(string text)
        {
            Border bubble = new Border { Style = (Style)FindResource("AgentBubble") };
            TextBlock tb = new TextBlock
            {
                Text = $"🧑‍💻 {text}",
                TextWrapping = TextWrapping.Wrap,
                Foreground = Brushes.White,
                FontSize = 14,
            };
            bubble.Child = tb;
            ChatPanel.Children.Add(bubble);
            ChatScrollViewer.ScrollToBottom();
        }

        private async Task DisplayShieldMessage(string message)
        {
            Border bubble = new Border { Style = (Style)FindResource("ShieldBubble") };
            TextBlock tb = new TextBlock
            {
                Text = "🛡 ",
                TextWrapping = TextWrapping.Wrap,
                Foreground = new SolidColorBrush(Color.FromRgb(0x86, 0xEF, 0xAC)),
                FontSize = 14,
                FontFamily = new FontFamily("Consolas"),
            };
            bubble.Child = tb;
            ChatPanel.Children.Add(bubble);
            ChatScrollViewer.ScrollToBottom();

            for (int i = 0; i <= message.Length; i++)
            {
                tb.Text = $"🛡 {message[..i]}";
                await Task.Delay(3);
                ChatScrollViewer.ScrollToBottom();
            }

            ApplyGreenThemeFormatting(tb, message);
            ChatScrollViewer.ScrollToBottom();
            RefreshDrillPanel();
        }

        private void PlayStartupAudio()
        {
            try
            {
                string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "welcome.wav");
                if (System.IO.File.Exists(path))
                {
                    using SoundPlayer player = new SoundPlayer(path);
                    player.PlaySync();
                }
            }
            catch { }
        }

        private Border? _processingBubble;

        private void ToggleProcessingIndicator(bool show)
        {
            if (show)
            {
                _processingBubble = new Border { Style = (Style)FindResource("ShieldBubble") };
                TextBlock tb = new TextBlock
                {
                    Text = "🛡 processing…",
                    Foreground = new SolidColorBrush(Color.FromRgb(0x2D, 0x6A, 0x2D)),
                    FontStyle = FontStyles.Italic,
                    FontSize = 13,
                };
                _processingBubble.Child = tb;
                ChatPanel.Children.Add(_processingBubble);
                ChatScrollViewer.ScrollToBottom();
            }
            else
            {
                if (_processingBubble != null && ChatPanel.Children.Contains(_processingBubble))
                    ChatPanel.Children.Remove(_processingBubble);
                _processingBubble = null;
            }
        }

        private void RefreshDrillPanel()
        {
            if (!ThreatResponseEngine.DrillActive)
            {
                DrillPanelBorder.Visibility = Visibility.Collapsed;
                return;
            }

            DrillPanelBorder.Visibility = Visibility.Visible;
            BuildDrillChoiceButtons();
        }

        private void BuildDrillChoiceButtons()
        {
            DrillOptionsPanel.Children.Clear();

            foreach (string label in new[] { "1", "2", "3", "4" })
            {
                Button btn = new Button
                {
                    Content = $" {label}",
                    Style = (Style)FindResource("DrillButton"),
                    Tag = label,
                };
                btn.Click += DrillChoiceButton_Click;
                DrillOptionsPanel.Children.Add(btn);
            }

            DrillProgressText.Text = "Drill active — click a choice or type 1 / 2 / 3 / 4";
            DrillScoreText.Text = string.Empty;
        }

        private async void DrillChoiceButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string tag)
            {
                InputTextBox.Text = tag;
                await HandleAgentInput();
            }
        }

        private static void ApplyGreenThemeFormatting(TextBlock tb, string fullText)
        {
            tb.Text = string.Empty;
            tb.Inlines.Clear();

            foreach (string rawLine in fullText.Split('\n'))
            {
                Run run = new Run(rawLine)
                {
                    Foreground = ResolveLineColour(rawLine)
                };
                tb.Inlines.Add(run);
                tb.Inlines.Add(new LineBreak());
            }
        }

        private static Brush ResolveLineColour(string line)
        {
            if (line.StartsWith("🎣") || line.StartsWith("🔑") || line.StartsWith("🌐") ||
                line.StartsWith("🛡") || line.StartsWith("🔒") || line.StartsWith("🎭") ||
                line.StartsWith("🦠") || line.StartsWith("🔏") || line.StartsWith("🚨") ||
                line.StartsWith("📋") || line.StartsWith("📜") || line.StartsWith("⭐") ||
                line.StartsWith("🔐"))
                return new SolidColorBrush(Color.FromRgb(0x4A, 0xDE, 0x80));

            if (line.TrimStart().StartsWith("✓") || line.StartsWith("✅"))
                return new SolidColorBrush(Color.FromRgb(0x34, 0xD3, 0x99));

            if (line.TrimStart().StartsWith("✗") || line.StartsWith("❌") || line.StartsWith("⚠️"))
                return new SolidColorBrush(Color.FromRgb(0xFF, 0x6B, 0x6B));

            if (line.TrimStart().StartsWith("•"))
                return new SolidColorBrush(Color.FromRgb(0xA7, 0x8B, 0xFA));

            if (line.TrimStart().StartsWith("💡"))
                return new SolidColorBrush(Color.FromRgb(0xFB, 0xBF, 0x24));

            if (line.TrimStart().StartsWith("1.") || line.TrimStart().StartsWith("2.") ||
                line.TrimStart().StartsWith("3.") || line.TrimStart().StartsWith("4.") ||
                line.TrimStart().StartsWith("5.") || line.TrimStart().StartsWith("6.") ||
                line.TrimStart().StartsWith("7."))
                return new SolidColorBrush(Color.FromRgb(0xFB, 0xBF, 0x24));

            if (line.StartsWith("─") || line.StartsWith("═") || line.StartsWith("══"))
                return new SolidColorBrush(Color.FromRgb(0x2D, 0x6A, 0x2D));

            return new SolidColorBrush(Color.FromRgb(0x86, 0xEF, 0xAC));
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            logBook.Log("System shutdown", "SecureShield Awareness System closing");
            ThreatResponseEngine.WriteSessionMarker(starting: false);
            base.OnClosing(e);
        }
    }
}