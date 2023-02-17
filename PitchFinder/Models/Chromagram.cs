using System;
using System.Linq;

namespace PitchFinder.Models
{
    //https://en.wikipedia.org/wiki/Chroma_feature
    public class Chromagram
    {
        private double[][] _chromaFilter;

        public void Initialize(int sampleRate)
        {
            CreateChromaFilterBank(sampleRate);
        }

        public double[] GetChroma(double[] ampSpectrum)
        {
            return GenerateChroma(ampSpectrum);
        }

        //https://github.com/meyda/meyda/blob/main/src/extractors/chroma.ts
        private double[] GenerateChroma(double[] ampSpectrum)
        {
            if (ampSpectrum == null)
            {
                throw new Exception("Valid ampSpectrum is required to generate chroma");
            }

            double[] chromagram = new double[_chromaFilter.Length];
            for (int i = 0; i < _chromaFilter.Length; i++)
            {
                double acc = 0;
                for (int j = 0; j < ampSpectrum.Length; j++)
                {
                    acc += (ampSpectrum[j] * _chromaFilter[i][j]);
                }
                chromagram[i] = acc;
            }

            double maxVal = chromagram.Max();
            if (maxVal > 0)
            {
                for (int i = 0; i < chromagram.Length; i++)
                {
                    chromagram[i] = chromagram[i] / maxVal;
                }
            }

            return chromagram;
        }

        private double HzToOctaves(double freq, double A440) { return Math.Log2((16 * freq) / A440); }

        //https://github.com/meyda/meyda/blob/main/src/utilities.ts#L179
        private double[][] NormalizeByColumn(double[][] a)
        {
            double[] emptyRow = new double[a[0].Length];
            double[] colDenominators = a.Aggregate(emptyRow, (acc, row) =>
            {
                for (int j = 0; j < row.Length; j++)
                {
                    acc[j] += Math.Pow(row[j], 2);
                }
                return acc;
            }).Select(x => Math.Sqrt(x)).ToArray();

            return a.Select((row, i) => row.Select((v, j) => v / (colDenominators[j] == 0 ? 1 : colDenominators[j])).ToArray()).ToArray();
        }

        //https://github.com/meyda/meyda/blob/main/src/utilities.ts#L192
        private void CreateChromaFilterBank(int sampleRate, int numFilters = 12, int bufferSize = 4096, int centerOctave = 5, float octaveWidth = 2, bool baseC = true, float A440 = 440)
        {
            int numOutputBins = (int)Math.Floor(bufferSize / 2f) + 1;

            double[] frequencyBins = new double[bufferSize].Select((_, v) => numFilters * HzToOctaves((double)(sampleRate * v) / bufferSize, A440)).ToArray();
            // Set a value for the 0 Hz bin that is 1.5 octaves below bin 1
            // (so chroma is 50% rotated from bin 1, and bin width is broad)
            frequencyBins[0] = frequencyBins[1] - 1.5f * numFilters;

            double[] binWidthBins = frequencyBins.Skip(1).Select((v, i) => Math.Max(v - frequencyBins[i], 1)).Concat(new double[] { 1 }).ToArray();

            int halfNumFilters = (int)Math.Round(numFilters / 2.0f);
            double[][] filterPeaks = new double[numFilters].Select((_, v) => frequencyBins.Select(frq => ((10 * numFilters + halfNumFilters + frq - v) % numFilters) - halfNumFilters).ToArray()).ToArray();

            var weights = filterPeaks.Select((row, i) => row.Select((_, j) => Math.Exp(-0.5 * Math.Pow((2 * filterPeaks[i][j]) / binWidthBins[j], 2))).ToArray()).ToArray();
            weights = NormalizeByColumn(weights);

            if (octaveWidth > 0)
            {
                double[] octaveWeights = frequencyBins.Select(v => (double)Math.Exp(-0.5f * Math.Pow((v / numFilters - centerOctave) / octaveWidth, 2))).ToArray();

                weights = weights.Select(row => row.Select((cell, j) => cell * octaveWeights[j]).ToArray()).ToArray();
            }

            if (baseC)
            {
                weights = weights.Skip(3).Concat(weights.Take(3)).ToArray();
            }

            _chromaFilter = weights.Select(row => row.Take(numOutputBins).ToArray()).ToArray();
        }
    }
}
