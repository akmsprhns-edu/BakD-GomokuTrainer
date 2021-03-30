using Microsoft.ML.Data;

namespace OnnxEstimatorLib.Models
{
    public class InputData
    {
        [ColumnName("input")]
        [VectorType(450)]
        public float[] Input { get; set; }

    }
}
