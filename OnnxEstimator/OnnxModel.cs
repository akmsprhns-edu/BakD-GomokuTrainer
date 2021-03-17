using System.Text.RegularExpressions;

namespace OnnxEstimator
{
    public class OnnxModel
    {
        public string Path { get; set; }
        public int Score { get; set; } = 0;

        public int Number { get {

                return int.Parse(Regex.Match(Path, @"\.(\d+)\.onnx").Groups[1].Value);
            } }
    }
}
