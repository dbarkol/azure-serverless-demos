using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeWantTheFunc
{
    public class Feedback
    {
        // Unique ID for feedback record
        public Guid Id { get; set; }

        // Feedback message
        public string Message { get; set; }

        // Score given by text analysis
        public int Score { get; set; }

        // Sentiment
        public string Sentiment { get; set; }
    }
}
