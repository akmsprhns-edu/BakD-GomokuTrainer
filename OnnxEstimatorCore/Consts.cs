using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OnnxEstimatorLib
{
    public static class Consts
    {
        public static readonly int[] NNInputShape = new int[] { 1, 450};
        public static int NNInputLen { get => NNInputShape.Aggregate(1, (a, b) => a * b); }

        public static readonly string NNInputName = "input";
        public static readonly string NNOutputName = "output";
    }
}
