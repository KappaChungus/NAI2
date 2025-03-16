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
            double? value = GetDoubleFromUser("Enter a value:");
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

            List<double> randomStartingVector =
                [random.Next(0, 10), random.Next(0, 10), random.Next(0, 10), random.Next(0, 10)];

            Debug.Assert(_theta != null, nameof(_theta) + " != null");
            Debug.Assert(_alpha != null, nameof(_alpha) + " != null");
            var perceptron = new Perceptron(_alpha.Value, _theta.Value, randomStartingVector);
            Debug.Assert(_randomizedTrain != null, nameof(_randomizedTrain) + " != null");
            foreach (var entry in _randomizedTrain)
            {
                perceptron.MakeDecision(entry.Key, entry.Value);
            }

            perceptron.TrainMode = false;
            
            int correct = 0;
            Debug.Assert(_randomizedTest != null, nameof(_randomizedTest) + " != null");
            foreach (var entry in _randomizedTest)
            {
                var y = perceptron.MakeDecision(entry.Key, entry.Value);
                correct += y == entry.Value ? 1 : 0;
            }

            MessageBox.Show(100.0 * correct / _randomizedTest.Count + "%");
        }
        
        private double? GetDoubleFromUser(string prompt)
        {
            var inputForm = new Form();
            inputForm.Text = prompt;
            inputForm.Size = new Size(300, 150);
            inputForm.StartPosition = FormStartPosition.CenterScreen;

            var label = new Label() { Text = prompt, Location = new Point(10, 20) };
            var textBox = new TextBox() { Location = new Point(10, 50), Width = 260 };
            var okButton = new Button() { Text = "OK", Location = new Point(10, 80) };

            double? result = null;
            okButton.Click += (_, _) =>
            {
                if (double.TryParse(textBox.Text, out double parsedValue))
                {
                    result = parsedValue;
                    inputForm.Close();
                    inputForm.Dispose();
                }
                else
                {
                    MessageBox.Show("Please enter a valid number.");
                }
            };

            inputForm.Controls.Add(label);
            inputForm.Controls.Add(textBox);
            inputForm.Controls.Add(okButton);
            inputForm.ShowDialog();
            return result;
        }



        private Dictionary<List<double>, bool> Preprocess(String path)
        {
            var data = new Dictionary<List<double>, bool>();
            string? firstValue = null;
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
                        firstValue ??= part;
                        data.Add(values, part.Equals(firstValue));
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