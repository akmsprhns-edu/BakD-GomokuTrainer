using OnnxEstimatorLib;
using System.Linq;

namespace OnnxEstimatorCpu
{
    class Program
    {
        static int Main(string[] args)
        {
            var filtredArgs = args.Where(x => !x.Equals("--gpu", System.StringComparison.OrdinalIgnoreCase)).ToArray();
            return OnnxEstimator.Run(filtredArgs);
        }
    }
}
