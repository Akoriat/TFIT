﻿using Lab0.Classes;
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
                var filePath = txtFilePath.Text.Trim();
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

            txtDetailedOutput.Clear();

            var oldWriter = Console.Out;

            try
            {

                Console.SetOut(new TextBoxStreamWriter(txtDetailedOutput));

                var inputWord = txtInputWord.Text.Trim();
                var result = _automath.ProcessInputLine(inputWord);

                if (result)
                    Console.WriteLine("Слово принято автоматом.");
                else
                    Console.WriteLine("Слово отклонено автоматом.");
            }
            finally
            {
                Console.SetOut(oldWriter);
            }
        }


        private Automath CreateAutomathFromFields()
        {
            var typeStr = (cbType.SelectedItem as ComboBoxItem)?.Content as string;
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

            string[] states = txtStates.Text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                             .Select(s => s.Trim()).ToArray();
            if (states.Length == 0)
            {
                MessageBox.Show("Введите хотя бы одно состояние.");
                return null;
            }
            if (type == TypeAutomaton.ENKA && !states.Contains("ε"))
            {
                MessageBox.Show("Для ЕНКА должно быть задано состояние ε.");
                return null;
            }

            string[] inputs = txtAlphabet.Text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                              .Select(s => s.Trim()).ToArray();
            if (inputs.Length == 0)
            {
                MessageBox.Show("Введите хотя бы один символ алфавита.");
                return null;
            }

            var initState = txtInitialState.Text.Trim();
            if (string.IsNullOrEmpty(initState))
            {
                MessageBox.Show("Введите начальное состояние.");
                return null;
            }
            if(initState.Split(", ").Length > 1 && type == TypeAutomaton.DKA)
            {
                MessageBox.Show("Для ДКА нельзя задать больше одного начального состояния");
                return null;
            }

            string[] finalStates = txtFinalStates.Text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                      .Select(s => s.Trim()).ToArray();
            if (finalStates.Length == 0)
            {
                MessageBox.Show("Введите хотя бы одно финальное состояние.");
                return null;
            }

            var transitions = new Dictionary<string, List<string>>();
            string[] lines = txtTransitions.Text.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length != states.Length)
            {
                MessageBox.Show("Количество строк в таблице переходов должно совпадать с количеством состояний.");
                return null;
            }

            for (var i = 0; i < states.Length; i++)
            {
                string[] parts = lines[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != inputs.Length)
                {
                    MessageBox.Show($"В строке для состояния {states[i]} количество переходов ({parts.Length}) не совпадает с количеством символов алфавита ({inputs.Length}).");
                    return null;
                }

                List<string> transitionList = new List<string>();
                foreach (var part in parts)
                {
                    if (part.StartsWith("{") && part.EndsWith("}"))
                    {
                        if (type == TypeAutomaton.DKA)
                        {
                            MessageBox.Show("Для ДКА нельзя задавать множественные состояния.");
                            return null;
                        }
                        else
                        {
                            transitionList.Add(part);
                        }
                    }
                    else
                    {
                        transitionList.Add(part);
                    }
                }

                transitions.Add(states[i], transitionList);
            }

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
