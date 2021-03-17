using Microsoft.ML.Data;

namespace OnnxEstimator.Models
{
    public class InputData
    {
        [ColumnName("input")]
        [VectorType(450)]
        public float[] Input { get; set; }

    }
}
