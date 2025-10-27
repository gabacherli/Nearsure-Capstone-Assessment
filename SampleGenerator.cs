using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevSample
{
    class SampleGenerator
    {


        private readonly DateTime _sampleStartDate;
        private readonly TimeSpan _sampleIncrement;

        private readonly List<Sample> _sampleList;


        public SampleGenerator(DateTime sampleStartDate, TimeSpan sampleIncrement)
        {
            _sampleList = new List<Sample>();
            _sampleStartDate = sampleStartDate;
            _sampleIncrement = sampleIncrement;
        }


        /// <summary>
        /// Samples should be a time-descending ordered list
        /// </summary>
        public List<Sample> Samples { get { return _sampleList; } }


        public int SamplesValidated { get; private set; }


        public void LoadSamples(int samplesToGenerate)
        {

            //TODO: can we load samples faster?

            _sampleList.Clear();

            DateTime date = _sampleStartDate;

            for (int i = 0; i < samplesToGenerate; i++)
            {
                Sample s = new Sample(i == 0);
                s.LoadSampleAtTime(date);

                _sampleList.Insert(0, s);

                date += _sampleIncrement;
            }


        }


        public void ValidateSamples()
        {

            //TODO: can we validate samples faster?

            int samplesValidated = 0;

            for (int i = 0; i < _sampleList.Count; i++)
            {

                if (_sampleList[i].ValidateSample(i < _sampleList.Count - 1 ? _sampleList[i + 1] : null, _sampleIncrement)) //in this sample the ValidateSample is always true but assume that's not always the case
                    samplesValidated++;

            }


            SamplesValidated = samplesValidated;

        }



    }
}
