

namespace Perceptron;

public class Perceptron
{
    public bool TrainMode { get; set; }
    private double _alpha;
    private double _theta;
    private List<double> _weights;

    public Perceptron(double alpha, double theta, List<Double> weights)
    {
        _alpha = alpha;
        _theta = theta;
        _weights = weights;
        TrainMode = true;
    }

    public bool MakeDecision(List<double> x,bool d)
    {
        if(x.Count != _weights.Count)
            throw new ArgumentException("size mismatch");
        bool y = Scalar(x)>_theta;
        if (TrainMode && y != d)
        {
            Learn(x, d ? 1 : 0, y ? 1 : 0);
        }

        return y;
    }

    private void Learn(List<Double> x,int d, int y)
    {
        for (int i = 0; i < x.Count; i++)
            _weights[i]+=(d-y)*x[i]*_alpha;
        _theta = - _theta*(d-y)*_alpha;
    }

    private double Scalar(List<double> x)
    {
        return x.Select((t, i) => _weights[i] * t).Sum();
    }

    public List<double> GetVector()
    {
        return _weights;
    }
}