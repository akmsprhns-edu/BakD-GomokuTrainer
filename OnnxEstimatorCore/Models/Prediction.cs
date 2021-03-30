using Microsoft.ML.Data;

namespace OnnxEstimatorLib.Models
{
    public class Prediction
    {
        [VectorType(1)]
        [ColumnName("output")]
        public float[] Output { get; set; }

        public float Evaluation { get => Output[0]; }
    }
}
