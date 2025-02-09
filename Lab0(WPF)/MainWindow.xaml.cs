using Lab0.Classes;  // Подключите библиотеку с классами автомата
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace WpfAutomaton
{
    /// <summary>
    /// Класс для перенаправления Console.Out в TextBox.
    /// </summary>
    public class TextBoxStreamWriter : TextWriter
    {
        private TextBox _output;
        public override Encoding Encoding => Encoding.UTF8;
        public TextBoxStreamWriter(TextBox output)
        {
            _output = output;
        }
        public override void Write(char value)
        {
            _output.Dispatcher.Invoke(() =>
            {
                _output.AppendText(value.ToString());
            });
        }
        public override void Write(string value)
        {
            _output.Dispatcher.Invoke(() =>
            {
                _output.AppendText(value);
            });
        }
        public override void WriteLine(string value)
        {
            _output.Dispatcher.Invoke(() =>
            {
                _output.AppendText(value + Environment.NewLine);
            });
        }
    }

    public partial class MainWindow : Window
    {
        private Automaton _automaton;

        public MainWindow()
        {
            InitializeComponent(); // Создание всех элементов из XAML
            Console.SetOut(new TextBoxStreamWriter(txtOutput));
        }

        // При выборе режима "Из файла" делаем видимой панель файла и скрываем ручной ввод
        private void rbFile_Checked(object sender, RoutedEventArgs e)
        {
            if (spFileInput != null)
                spFileInput.Visibility = Visibility.Visible;
            if (spManualInput != null)
                spManualInput.Visibility = Visibility.Collapsed;
        }

        // При выборе режима "Ввести вручную" – наоборот
        private void rbManual_Checked(object sender, RoutedEventArgs e)
        {
            if (spManualInput != null)
                spManualInput.Visibility = Visibility.Visible;
            if (spFileInput != null)
                spFileInput.Visibility = Visibility.Collapsed;
        }

        // Кнопка для установки пути к файлу по умолчанию
        private void btnDefaultFile_Click(object sender, RoutedEventArgs e)
        {
            txtFilePath.Text = "Resources/example.txt";
        }

        // Загрузка автомата
        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            txtOutput.Clear(); // Очищаем вывод
            if (rbFile.IsChecked == true)
            {
                string filePath = txtFilePath.Text.Trim();
                if (!File.Exists(filePath))
                {
                    MessageBox.Show("Файл не найден!");
                    return;
                }
                _automaton = Automaton.CreateFromFile(filePath);
            }
            else // Режим "Ввести вручную"
            {
                _automaton = CreateAutomatonFromFields();
            }

            if (_automaton != null)
            {
                _automaton.ShowInfo();
                Console.WriteLine();
                _automaton.ShowTable();
            }
        }

        // Обработка входного слова
        private void btnProcess_Click(object sender, RoutedEventArgs e)
        {
            if (_automaton == null)
            {
                MessageBox.Show("Сначала загрузите автомат.");
                return;
            }

            string inputWord = txtInputWord.Text.Trim();
            Console.WriteLine();
            bool result = _automaton.ProcessInputLine(inputWord);
            Console.WriteLine();
            if (result)
                Console.WriteLine("Слово принято автоматом.");
            else
                Console.WriteLine("Слово отклонено автоматом.");
        }

        /// <summary>
        /// Создаёт автомат из данных, введённых в поля (ручной режим).
        /// </summary>
        /// <returns>Объект Automaton или null при ошибке</returns>
        private Automaton CreateAutomatonFromFields()
        {
            // 1. Получаем тип автомата из ComboBox
            string typeStr = (cbType.SelectedItem as ComboBoxItem)?.Content as string;
            TypeAutomaton type;
            if (typeStr == "DKA")
                type = TypeAutomaton.DKA;
            else if (typeStr == "NKA")
                type = TypeAutomaton.NKA;
            else if (typeStr == "NKA-E")
                type = TypeAutomaton.ENKA;
            else
            {
                MessageBox.Show("Некорректный тип автомата.");
                return null;
            }

            // 2. Получаем состояния
            string[] states = txtStates.Text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                             .Select(s => s.Trim()).ToArray();
            if (states.Length == 0)
            {
                MessageBox.Show("Введите хотя бы одно состояние.");
                return null;
            }

            // 3. Получаем алфавит
            string[] inputs = txtAlphabet.Text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                              .Select(s => s.Trim()).ToArray();
            if (inputs.Length == 0)
            {
                MessageBox.Show("Введите хотя бы один символ алфавита.");
                return null;
            }

            // 4. Начальное состояние
            string initState = txtInitialState.Text.Trim();
            if (string.IsNullOrEmpty(initState))
            {
                MessageBox.Show("Введите начальное состояние.");
                return null;
            }

            // 5. Финальные состояния
            string[] finalStates = txtFinalStates.Text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                      .Select(s => s.Trim()).ToArray();
            if (finalStates.Length == 0)
            {
                MessageBox.Show("Введите хотя бы одно финальное состояние.");
                return null;
            }

            // 6. Таблица переходов
            // Ожидается, что количество строк таблицы совпадает с количеством состояний.
            var transitions = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<string>>();
            string[] lines = txtTransitions.Text.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length != states.Length)
            {
                MessageBox.Show("Количество строк в таблице переходов должно совпадать с количеством состояний.");
                return null;
            }
            for (int i = 0; i < states.Length; i++)
            {
                string[] parts = lines[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != inputs.Length)
                {
                    MessageBox.Show($"В строке для состояния {states[i]} количество переходов ({parts.Length}) не совпадает с количеством символов алфавита ({inputs.Length}).");
                    return null;
                }
                transitions.Add(states[i], parts.ToList());
            }

            // 7. Создаём автомат нужного типа
            switch (type)
            {
                case TypeAutomaton.DKA:
                    return new DeterministicAutomaton(states, inputs, finalStates, initState, transitions);
                case TypeAutomaton.NKA:
                    return new NonDeterministicAutomaton(states, inputs, finalStates, initState, transitions);
                case TypeAutomaton.ENKA:
                    return new EpsilonAutomaton(states, inputs, finalStates, initState, transitions);
                default:
                    return null;
            }
        }
    }
}
