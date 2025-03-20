using System.Diagnostics;
namespace Perceptron;

public class ButtonFactory
{
    private double? _alpha;
    private double? _theta;
    private Dictionary<List<double>, bool>? _trainData;
    private Dictionary<List<double>, bool>? _testData;
    private List<KeyValuePair<List<double>,bool>>? _randomizedTrain;
    private List<KeyValuePair<List<double>,bool>>?  _randomizedTest;
    private Func<Button> _getStartButton;
    private int _dimension;
    private Perceptron? _perceptron;
    private string? _firstVectorGroupName;
    private string? _secondVectorGroupName;

    public ButtonFactory(Func<Button>getStartButton)
    {
        _getStartButton = getStartButton;
    }
    public Button GetButton(string name,Action<object, EventArgs, string> clickHandler)
        {
            var button = new Button();
            button.Text = name;
            button.Size = new Size(150, 30);
            button.Font = new Font("Arial", 12,FontStyle.Regular);
            button.Click += (sender, e) => 
            {
                Debug.Assert(sender != null, nameof(sender) + " != null");
                clickHandler(sender, e, name);
            };
            return button;
        }
        public void BtnBrowseFileClick(object sender, EventArgs e, string name)
        {
            using var openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Select a file";
            openFileDialog.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                if (name.Equals("train"))
                {
                    try
                    {
                        _trainData = Preprocess(openFileDialog.FileName);
                        _randomizedTrain = _trainData.OrderBy(_ => Guid.NewGuid()).ToList();
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("sg went wrong\nplease try again");
                        return;
                    }
                }
                else
                {
                    try
                    {
                        _testData = Preprocess(openFileDialog.FileName);
                        _randomizedTest = _testData.OrderBy(_ => Guid.NewGuid()).ToList();
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("sg went wrong\nplease try again");
                        return;
                    }
                }
                CheckIfReady();
                MessageBox.Show("File " + name + " is correct.");
            }
        }
        public void SetButtonClick(object sender, EventArgs e, string name)
        {
            double? value = GetDoubleFromUser("Enter a value for " + name+":");
            if (value.HasValue)
            {
                if (name.Equals("α"))
                {
                    _alpha = value.Value;
                    ((Button)sender).Text= "α = "+value.Value;
                }
                else
                {
                    _theta = value.Value;
                    ((Button)sender).Text= "θ = "+value.Value;
                }

                CheckIfReady();
            }
            else
            {
                MessageBox.Show("Invalid value entered.");
            }
        }

        public void StartButtonClick(object sender, EventArgs e, string name)
        {
            
            var random = new Random();
            Debug.Assert(_trainData != null, nameof(_trainData) + " != null");
            _dimension = _trainData.First().Key.Count;
            
            var randomStartingVector = new List<Double>();
            
            for(var i = 0; i < _dimension; i++)
                randomStartingVector.Add(random.Next(0,10));

            Debug.Assert(_theta != null, nameof(_theta) + " != null");
            Debug.Assert(_alpha != null, nameof(_alpha) + " != null");
            _perceptron = new Perceptron(_alpha.Value, _theta.Value, randomStartingVector);
            Debug.Assert(_randomizedTrain != null, nameof(_randomizedTrain) + " != null");
            foreach (var entry in _randomizedTrain)
            {
                _perceptron.MakeDecision(entry.Key, entry.Value);
            }

            _perceptron.TrainMode = false;
            
            int correct = 0;
            Debug.Assert(_randomizedTest != null, nameof(_randomizedTest) + " != null");
            foreach (var entry in _randomizedTest)
            {
                var y = _perceptron.MakeDecision(entry.Key, entry.Value);
                correct += y == entry.Value ? 1 : 0;
            }

            MessageBox.Show(100.0 * correct / _randomizedTest.Count + "%");
            
        }
        
        public void TestForVectorButtonClick(object sender, EventArgs e, string name)
        {
            Tuple<List<double>, string?> vectorData = GetVectorFromUser("Enter a vector:", _dimension);
            List<double> vector = vectorData.Item1;
            string? dataName = vectorData.Item2;
            Debug.Assert(_perceptron != null, nameof(_perceptron) + " != null");
            _perceptron.TrainMode = false;
            bool expectedResult = dataName == _firstVectorGroupName;
            MessageBox.Show(_perceptron.MakeDecision(vector, expectedResult)==expectedResult?"Perceptron Succeded":"Perceptron Failed");

        }

        private Tuple<List<double>, string?> GetVectorFromUser(string prompt, int dimension, bool askForVectorName = true)
        {
            var inputForm = new Form
            {
                Text = prompt,
                Size = new Size(300, 200 + (dimension * 30)),
                StartPosition = FormStartPosition.CenterScreen
            };

            var label = new Label
            {
                Text = prompt,
                Location = new Point(10, 10),
                AutoSize = true
            };

            TextBox[] textBoxes = new TextBox[dimension];
            var nameLabel = new Label { Text = "Name:", Location = new Point(10, 40 + (dimension * 30)), AutoSize = true };
            if (askForVectorName)
                inputForm.Controls.Add(nameLabel);
            var nameComboBox = new ComboBox() { Location = new Point(60, 40 + (dimension * 30)), Width = 100, TabIndex = dimension };
            if (askForVectorName)
            {
                Debug.Assert(_firstVectorGroupName != null, nameof(_firstVectorGroupName) + " != null");
                Debug.Assert(_secondVectorGroupName != null, nameof(_secondVectorGroupName) + " != null");
                nameComboBox.Items.Add(_firstVectorGroupName);
                nameComboBox.Items.Add(_secondVectorGroupName);
            }

            for (int i = 0; i < dimension; i++)
            {
                if (askForVectorName)
                {
                    var xLabel = new Label
                    {
                        Text = "x" + i + ": ",
                        Location = new Point(10, 40 + (i * 30)),
                        AutoSize = true
                    };
                    inputForm.Controls.Add(xLabel);
                }

                textBoxes[i] = new TextBox
                {
                    Location = new Point(60, 40 + (i * 30)),
                    Width = 40,
                    TabIndex = i
                };

                textBoxes[i].KeyDown += (sender, e) =>
                {
                    if (e.KeyCode == Keys.Enter)
                    {
                        e.SuppressKeyPress = true;
                        int nextIndex = Array.IndexOf(textBoxes, sender) + 1;

                        if (nextIndex < dimension)
                            textBoxes[nextIndex].Focus();
                        else if (askForVectorName)
                            nameComboBox.Focus();
                    }
                };

                inputForm.Controls.Add(textBoxes[i]);
            }
            
            if (askForVectorName)
                inputForm.Controls.Add(nameComboBox);

            var okButton = new Button { Text = "OK", Location = new Point(10, 80 + (dimension * 30)), TabIndex = dimension + 1 };

            List<double> vector = new List<double>();
            string? vectorName = "";

            okButton.Click += (_, _) =>
            {
                vector.Clear();
                foreach (var textBox in textBoxes)
                {
                    if (double.TryParse(textBox.Text, out double parsedValue))
                    {
                        vector.Add(parsedValue);
                    }
                    else
                    {
                        MessageBox.Show("Please enter valid numbers.");
                        return;
                    }
                }
                if(askForVectorName)
                {
                    if (nameComboBox.SelectedItem is not null)
                    {
                        vectorName = nameComboBox.SelectedItem.ToString();
                        inputForm.Close();
                    }
                    else
                    {
                        MessageBox.Show("Please enter a valid name.");
                    }
                }
                else
                    inputForm.Close();
            };

            inputForm.Controls.Add(label);
            inputForm.Controls.Add(okButton);
            inputForm.ShowDialog();

            return new Tuple<List<double>, string?>(vector, vectorName);
        }


        
        private double? GetDoubleFromUser(string prompt)
        {
            return GetVectorFromUser(prompt, 1, false).Item1[0];
        }



        private Dictionary<List<double>, bool> Preprocess(String path)
        {
            var data = new Dictionary<List<double>, bool>();
            using var reader = new StreamReader(path);
            while (reader.ReadLine() is { } line)
            {
                var parts = line.Split(',');
                var values = new List<double>();
                foreach (var part in parts)
                {
                    try
                    {
                        values.Add(double.Parse(part));
                    }
                    catch (FormatException)
                    {
                        _firstVectorGroupName ??= part;
                        if (part == _firstVectorGroupName)
                        {
                            data.Add(values, true);
                        }
                        else if (( _secondVectorGroupName ??= part)  == _secondVectorGroupName)
                        {
                            data.Add(values, false);
                        }
                    }
                }
            }

            return data;
        }
        void CheckIfReady()
        {
            if (_alpha != null && _theta != null && _trainData != null && _testData != null)
                _getStartButton.Invoke().Enabled = true;
        }
}