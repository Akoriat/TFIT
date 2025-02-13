using Lab0.Classes;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace WpfAutomath
{

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
        private Automath _automath;

        public MainWindow()
        {
            InitializeComponent();
            Console.SetOut(new TextBoxStreamWriter(txtOutput));
        }

        private void btnToDKA_Click(object sender, RoutedEventArgs e)
        {
            if (_automath == null)
            {
                MessageBox.Show("Сначала загрузите автомат.");
                return;
            }

            if (_automath.Type == TypeAutomaton.NKA)
            {
                _automath = (_automath as DnaAutomath)?.ToDka();
                if (_automath != null)
                {
                    _automath.ShowInfo();
                    Console.WriteLine();
                    _automath.ShowTable();
                }
            }
            else
            {
                MessageBox.Show("Преобразование в ДКА возможно только для НКА.");
            }
        }

        private void btnToNKA_Click(object sender, RoutedEventArgs e)
        {
            if (_automath == null)
            {
                MessageBox.Show("Сначала загрузите автомат.");
                return;
            }

            if (_automath.Type == TypeAutomaton.ENKA)
            {
                _automath = (_automath as NkaEpsilonAutomath)?.ToNka();
                if (_automath != null)
                {
                    _automath.ShowInfo();
                    Console.WriteLine();
                    _automath.ShowTable();
                }
            }
            else
            {
                MessageBox.Show("Преобразование в НКА возможно только для НКА с эпсилон-переходами.");
            }
        }

        private void rbFile_Checked(object sender, RoutedEventArgs e)
        {
            if (spFileInput != null)
                spFileInput.Visibility = Visibility.Visible;
            if (spManualInput != null)
                spManualInput.Visibility = Visibility.Collapsed;
        }

        private void rbManual_Checked(object sender, RoutedEventArgs e)
        {
            if (spManualInput != null)
                spManualInput.Visibility = Visibility.Visible;
            if (spFileInput != null)
                spFileInput.Visibility = Visibility.Collapsed;
        }

        private void btnDefaultFile_Click(object sender, RoutedEventArgs e)
        {
            txtFilePath.Text = "Resources/DefaultAutomath.txt";
        }

        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            txtOutput.Clear();
            if (rbFile.IsChecked == true)
            {
                string filePath = txtFilePath.Text.Trim();
                if (!File.Exists(filePath))
                {
                    MessageBox.Show("Файл не найден!");
                    return;
                }
                _automath = Automath.CreateFromFile(filePath);
            }
            else
            {
                _automath = CreateAutomathFromFields();
            }

            if (_automath != null)
            {
                _automath.ShowInfo();
                Console.WriteLine();
                _automath.ShowTable();
            }
        }

        private void btnProcess_Click(object sender, RoutedEventArgs e)
        {
            if (_automath == null)
            {
                MessageBox.Show("Сначала загрузите автомат.");
                return;
            }

            // Очищаем второй TextBox перед обработкой
            txtDetailedOutput.Clear();

            // 1) Сохраняем "старый" поток вывода (который сейчас идёт в txtOutput)
            TextWriter oldWriter = Console.Out;

            try
            {
                // 2) Временно перенаправим Console в txtDetailedOutput
                Console.SetOut(new TextBoxStreamWriter(txtDetailedOutput));

                // 3) Теперь все Console.WriteLine() внутри ProcessInputLine (и т.п.)
                //    будут попадать во второй TextBox:
                string inputWord = txtInputWord.Text.Trim();
                bool result = _automath.ProcessInputLine(inputWord);

                // Здесь же можно дополнительно дописать итоги
                if (result)
                    Console.WriteLine("Слово принято автоматом.");
                else
                    Console.WriteLine("Слово отклонено автоматом.");
            }
            finally
            {
                // 4) Восстанавливаем старый поток (снова пишем в txtOutput)
                Console.SetOut(oldWriter);
            }
        }


        private Automath CreateAutomathFromFields()
        {
            // Определение типа автомата
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

            // Получение состояний
            string[] states = txtStates.Text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                             .Select(s => s.Trim()).ToArray();
            if (states.Length == 0)
            {
                MessageBox.Show("Введите хотя бы одно состояние.");
                return null;
            }

            // Получение алфавита
            string[] inputs = txtAlphabet.Text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                              .Select(s => s.Trim()).ToArray();
            if (inputs.Length == 0)
            {
                MessageBox.Show("Введите хотя бы один символ алфавита.");
                return null;
            }

            // Получение начального состояния
            string initState = txtInitialState.Text.Trim();
            if (string.IsNullOrEmpty(initState))
            {
                MessageBox.Show("Введите начальное состояние.");
                return null;
            }

            // Получение финальных состояний
            string[] finalStates = txtFinalStates.Text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                      .Select(s => s.Trim()).ToArray();
            if (finalStates.Length == 0)
            {
                MessageBox.Show("Введите хотя бы одно финальное состояние.");
                return null;
            }

            // Получение таблицы переходов
            var transitions = new Dictionary<string, List<string>>();
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

                // Обработка множественных состояний (например, {2,3})
                List<string> transitionList = new List<string>();
                foreach (string part in parts)
                {
                    if (part.StartsWith("{") && part.EndsWith("}"))
                    {
                        // Убираем фигурные скобки и добавляем как одно значение
                        transitionList.Add(part.Trim('{', '}'));
                    }
                    else
                    {
                        // Одиночное состояние
                        transitionList.Add(part);
                    }
                }

                transitions.Add(states[i], transitionList);
            }

            // Создание автомата в зависимости от типа
            switch (type)
            {
                case TypeAutomaton.DKA:
                    return new DkaAutomath(states, inputs, finalStates, initState, transitions);
                case TypeAutomaton.NKA:
                    return new DnaAutomath(states, inputs, finalStates, initState, transitions);
                case TypeAutomaton.ENKA:
                    return new NkaEpsilonAutomath(states, inputs, finalStates, initState, transitions);
                default:
                    return null;
            }
        }
    }
}
