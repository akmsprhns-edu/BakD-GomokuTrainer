﻿using OnnxEstimatorLib;

namespace OnnxEstimatorCpu
{
    class Program
    {
        static int Main(string[] args)
        {
            return OnnxEstimator.Run(args);
        }
    }
}
