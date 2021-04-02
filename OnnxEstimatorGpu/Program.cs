using OnnxEstimatorLib;

namespace OnnxEstimatorGpu
{
    class Program
    {
        static int Main(string[] args)
        {
            return OnnxEstimator.Run(args);
        }
    }
}
