using ResourceModel;

namespace SelfOrganizingMap
{
    public class SoMapTrainer
    {
        private readonly SoMap _map;
        private readonly double[,] _input;
        private int _iteration;
        private double _learningRate;

        public SoMapTrainer(SoMap map, double[,] input)
        {
            _map = map;
            _input = input;
            _iteration = 0;
            _learningRate = _map.LearningRate;
        }

        public bool Train()
        {
            if (_iteration >= _map.NumberOfIterations) return false;
            
            _iteration = _map.TrainIteration(_input, _iteration, ref _learningRate);

            return true;
        }
    }
}
