using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Label = System.Windows.Controls.Label;

namespace SimpleEnigmaMachine
{
    public partial class MainWindow : Window
    {
        private readonly Dictionary<Key, Label> labelMappings;
        private readonly DataTable dataTable = new();
        private readonly List<List<int>> rings = new();

        public DataTable RingTable { get; private set; }

        public MainWindow()
        {
            InitializeComponent();

            labelMappings = new Dictionary<Key, Label>
            {
                //`12345
                { Key.Oem3, LabelGrave },
                { Key.D1, LabelOne },
                { Key.D2, LabelTwo },
                { Key.D3, LabelThree },
                { Key.D4, LabelFour },
                { Key.D5, LabelFive },
                { Key.D6, LabelSix },
                { Key.D7, LabelSeven },
                { Key.D8, LabelEight },
                { Key.D9, LabelNine },
                { Key.D0, LabelZero },
                { Key.OemMinus, LabelMinus },
                { Key.OemPlus, LabelEquals },

                //qwerty
                { Key.Q, LabelQ },
                { Key.W, LabelW },
                { Key.R, LabelR },
                { Key.E, LabelE },
                { Key.T, LabelT },
                { Key.Y, LabelY },
                { Key.U, LabelU },
                { Key.I, LabelI },
                { Key.O, LabelO },
                { Key.P, LabelP },
                { Key.OemOpenBrackets, LabelOpeningBracket },
                { Key.OemCloseBrackets, LabelClosingBracket },
                { Key.OemBackslash, LabelBackSlash },

                //asdfg
                { Key.A, LabelA },
                { Key.S, LabelS },
                { Key.D, LabelD },
                { Key.F, LabelF },
                { Key.G, LabelG },
                { Key.H, LabelH },
                { Key.J, LabelJ },
                { Key.K, LabelK },
                { Key.L, LabelL },
                { Key.OemSemicolon, LabelSemicolon },
                { Key.OemQuotes, LabelSingleQuote },

                //zxcvb
                { Key.Z, LabelZ },
                { Key.X, LabelX },
                { Key.C, LabelC },
                { Key.V, LabelV },
                { Key.B, LabelB },
                { Key.N, LabelN },
                { Key.M, LabelM },
                { Key.OemComma, LabelComma },
                { Key.OemPeriod, LabelDot },
                { Key.OemQuestion, LabelSlash },

                //Special Char
                { Key.Space, LabelSpace },
                { Key.LeftShift, LabelShift },
                { Key.RightShift, LabelShift },
                { Key.CapsLock, LabelCapsLk }
            };
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (labelMappings.TryGetValue(e.Key, out var targetLabel))
            {
                SolidColorBrush blueGrayBrush = new(Color.FromArgb(255, 96, 153, 194));
                targetLabel.Background = blueGrayBrush;
            }
        }

        private void MainWindow_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.CapsLock && Keyboard.IsKeyToggled(Key.CapsLock))
            {
                return;
            }

            if (labelMappings.TryGetValue(e.Key, out var targetLabel))
            {
                targetLabel.Background = Brushes.LightGray;
            }
        }

        private static string? OpenCsvFileDialog()
        {
            OpenFileDialog openFileDialog = new()
            {
                Filter = "CSV File (*.csv)|*.csv",
                Title = "Select a CSV File"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                return openFileDialog.FileName;
            }

            return null;
        }

        private void ImportMenu_Click(object sender, RoutedEventArgs e)
        {
            string? selectedFilePath = OpenCsvFileDialog();

            FilePathTextBlock.Text = "File Path: " + selectedFilePath;

            if (!string.IsNullOrEmpty(selectedFilePath))
            {
                using StreamReader reader = new(selectedFilePath);
                string? headerLine = reader.ReadLine();
                string[] headers = headerLine.Split(',');

                for (int i = 0; i < headers.Length; i++)
                {
                    dataTable.Columns.Add("Ring" + (i));
                }

                while (!reader.EndOfStream)
                {
                    string? dataLine = reader.ReadLine();
                    string[] fields = dataLine.Split(',');

                    DataRow row = dataTable.NewRow();

                    for (int i = 0; i < Math.Min(fields.Length, headers.Length); i++)
                    {
                        row["Ring" + (i)] = fields[i];
                    }

                    dataTable.Rows.Add(row);
                }

                PopulateRingsFromDataTable();

                RingCountTextBlock.Text = "Total Number of Rings: " + (dataTable.Columns.Count).ToString();

                MessageTextBox.IsReadOnly = false;
                MessageTextBox.Text = string.Empty;
                MessageTextBox.Focus();
            }
        }

        private void PopulateRingsFromDataTable()
        {
            rings.Clear();

            for (int columnIndex = 0; columnIndex < dataTable.Columns.Count; columnIndex++)
            {
                List<int> ringData = new();

                foreach (DataRow row in dataTable.Rows)
                {
                    if (row[columnIndex] != DBNull.Value)
                    {
                        if (int.TryParse(row[columnIndex].ToString(), out int value))
                        {
                            ringData.Add(value);
                        }
                    }
                }

                rings.Add(ringData);
            }
        }

        private void EncryptMessage(string message)
        {
            if (rings.Count > 0 && rings[0] != null && rings[0].Count > 0)
            {
                List<int> positions = new();

                foreach (char c in message)
                {
                    int asciiValue = (int)c;

                    for (int ringIndex = 0; ringIndex < rings.Count; ringIndex++)
                    {
                        List<int> currentRing = rings[ringIndex];
                        int currentPosition = currentRing.IndexOf(asciiValue);

                        if (currentPosition != -1)
                        {
                            int newPosition = (currentPosition + positions.Count) % currentRing.Count;
                            asciiValue = currentRing[newPosition];
                        }
                    }

                    positions.Add(asciiValue);
                }

                StringBuilder encryptedMessage = new();
                foreach (int position in positions)
                {
                    encryptedMessage.Append((char)position);
                }

                EncryptedTextBox.Text = encryptedMessage.ToString();
            }
            else
            {
                MessageBox.Show("Ring is empty or not initialized.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public string ReflectMessage(string message)
        {
            List<int> asciiValues = new();

            foreach (char c in message)
            {
                asciiValues.Add((int)c);
            }

            EncryptedTextBox.Text = "";

            if (rings.Count > 0 && rings[0] != null && rings[0].Count > 0)
            {
                List<int> positions = new(asciiValues);

                for (int ringIndex = rings.Count - 1; ringIndex >= 1; ringIndex--)
                {
                    List<int> currentRing = rings[ringIndex];

                    for (int i = 0; i < positions.Count; i++)
                    {
                        int asciiValue = positions[i];
                        int currentPosition = currentRing.IndexOf(asciiValue);

                        if (currentPosition != -1)
                        {
                            int newPosition = (currentPosition - i + currentRing.Count) % currentRing.Count;

                            positions[i] = currentRing[newPosition];
                        }
                    }

                    StringBuilder reflectedMessage = new StringBuilder();
                    foreach (int position in positions)
                    {
                        reflectedMessage.Append((char)position);
                    }

                    EncryptedTextBox.Text = reflectedMessage.ToString();
                }

                StringBuilder finalReflectedMessage = new();
                foreach (int position in positions)
                {
                    finalReflectedMessage.Append((char)position);
                }

                return finalReflectedMessage.ToString();
            }
            else
            {
                throw new InvalidOperationException("Ring0 is empty or not initialized.");
            }
        }

        private void ReflectorModeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (ReflectorModeMenu.IsChecked)
            {
                MachineModeTextBlock.Text = "Machine Mode: Reflect";
            }
            else
            {
                MachineModeTextBlock.Text = "Machine Mode: Encrypt";
            }
        }

        private void MessageTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string currentMessage = MessageTextBox.Text;

            if (ReflectorModeMenu.IsChecked)
            {
                string encryptedMessage = ReflectMessage(currentMessage);
                EncryptedTextBox.Text = encryptedMessage;
            }
            else
            {
                EncryptMessage(currentMessage);
            }
        }
    }
}