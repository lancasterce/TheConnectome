namespace TheConnectome_CSharp
{
    public class Synapse
    {
        // variables

        private int _weight;

        // constructors
        public Synapse()
        {
            NeuronA = "";
            NeuronB = "";
            _weight = 0;
        }

        public Synapse(string a, int c)
        {
            NeuronA = a;
            _weight = c;
        }

        public Synapse(string a, string b, int c)
            : this(a, c)
        {
            NeuronB = b;
        }

        // methods

        public string NeuronA { get; set; }

        public string NeuronB { get; set; }

        public int Weight
        {
            get { return _weight; }
            set { _weight += value; }
        }

        public int ResetWeight
        {
            get { return 0; }
            set { _weight = value; }
        }
    }

    // end class
}

// end namespace