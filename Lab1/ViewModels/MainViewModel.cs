using Lab1.Models;
using Lab1.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Lab1.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private string _inputText;
        private readonly Lexer _lexer;

        public MainViewModel()
        {
            _lexer = new Lexer();
            Lexemes = new ObservableCollection<Lexeme>();
            AnalyzeCommand = new RelayCommand(Analyze);
        }

        /// <summary>
        /// Текст, введённый пользователем.
        /// </summary>
        public string InputText
        {
            get => _inputText;
            set
            {
                _inputText = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Коллекция лексем для отображения.
        /// </summary>
        public ObservableCollection<Lexeme> Lexemes { get; }

        /// <summary>
        /// Команда для выполнения анализа текста.
        /// </summary>
        public ICommand AnalyzeCommand { get; }

        /// <summary>
        /// Выполняет анализ текста и обновляет коллекцию лексем.
        /// </summary>
        public void Analyze()
        {
            Lexemes.Clear();

            if (string.IsNullOrWhiteSpace(InputText))
                return;

            _lexer.Analyze(InputText);
            foreach (var lexeme in _lexer.GetLexemes())
            {
                Lexemes.Add(lexeme);
            }
        }

        /// <summary>
        /// Событие для уведомления об изменении свойств.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// Реализация команды для привязки кнопки к методу.
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute();

        public void Execute(object parameter) => _execute();

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

}
