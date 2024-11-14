using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace yf
{
    public class InverseDistributionRandom
    {
        private Random random;
        private double[] probabilities;
        private double[] cumulativeProbabilities;
        private int minValue;
        private int maxValue;

        public InverseDistributionRandom(int min, int max)
        {
            random = new Random();
            minValue = min;
            maxValue = max;

            int range = max - min + 1;
            probabilities = new double[range];
            cumulativeProbabilities = new double[range];

            // 反比例の確率を計算
            double sum = 0;
            for (int i = 0; i < range; i++)
            {
                // 1/(i+1) で反比例の重みを設定
                probabilities[i] = 1.0 / (i + 1);
                sum += probabilities[i];
            }

            // 確率の正規化（全確率の和を1にする）
            for (int i = 0; i < range; i++)
            {
                probabilities[i] /= sum;
            }

            // 累積確率の計算
            cumulativeProbabilities[0] = probabilities[0];
            for (int i = 1; i < range; i++)
            {
                cumulativeProbabilities[i] = cumulativeProbabilities[i - 1] + probabilities[i];
            }
        }

        public int Next()
        {
            double value = random.NextDouble();
            int index = Array.BinarySearch(cumulativeProbabilities, value);

            if (index < 0)
            {
                index = ~index;
            }

            return index + minValue;
        }

        // 特定の数値が出現する確率を取得
        public double GetProbability(int number)
        {
            if (number < minValue || number > maxValue)
                return 0;

            return probabilities[number - minValue];
        }
    }
}
