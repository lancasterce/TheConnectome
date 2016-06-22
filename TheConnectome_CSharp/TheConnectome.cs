using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheConnectome_CSharp
{
    public class TheConnectome
    {
        // Global Lists
        public static readonly List<Synapse> Connectome = new List<Synapse>();

        public static readonly List<PostSynapticNeuron> Postsynaptic = new List<PostSynapticNeuron>();

        // CSV Files

        // windows file strings (VS Debug Folder)
        public static readonly string ConnectomeFile = "connectome.csv";

        public static readonly string PostsynapticFile = "postsynaptic.csv";
        //public static readonly string connectomeFile = "edgelist.csv";
        //public static readonly string postsynapticFile = "synaptic.csv":

        // Global Variables
        public static int Threshold = 15;

        public static int MuscleFireCount = 0;
        public static int NeuronFireCount = 0;

        /*****  METHODS  *****/

        /*
         *  TestFilesWereRead()
         *  display contents of the Lists Connectome, Postsynaptic
         *  if data displays then the files were read correctly
         *
         */

        public static void TestFilesWereRead(StreamWriter writer)
        {
            writer.WriteLine("Displaying Contents of Connectome and Postsynaptic Lists:\n\n");

            int i;

            if (Connectome.Count > 0)
            {
                //Console.WriteLine("Connectome List size: " + Connectome.Count);
                writer.WriteLine("Connectome List size: " + Connectome.Count);

                for (i = 0; i < Connectome.Count; i++)
                {
                    //Console.WriteLine("\t" + i + " " + Connectome[i].NeuronA);
                    //Console.WriteLine("\t" + i + " " + Connectome[i].NeuronB);
                    //Console.WriteLine("\t" + i + " " + Connectome[i].Weight);

                    writer.WriteLine("\t" + i + " " + Connectome[i].NeuronA);
                    writer.WriteLine("\t" + i + " " + Connectome[i].NeuronB);
                    writer.WriteLine("\t" + i + " " + Connectome[i].Weight);
                }
            }
            else
            {
                Console.WriteLine("Connectome List has no data.");
                writer.WriteLine("Connectome List has no data.");
            }

            if (Postsynaptic.Count > 0)
            {
                Console.WriteLine("Postsynaptic List size: " + Postsynaptic.Count);

                for (i = 0; i < Postsynaptic.Count; i++)
                {
                    //Console.WriteLine("\t" + i + " " + Postsynaptic[i].NeuronA);
                    //Console.WriteLine("\t" + i + " " + Postsynaptic[i].Weight);

                    writer.WriteLine("\t" + i + " " + Postsynaptic[i].NeuronA);
                    writer.WriteLine("\t" + i + " " + Postsynaptic[i].Weight);
                }
            }
            else
            {
                //Console.WriteLine("Postsynaptic List has no data.");
                writer.WriteLine("Postsynaptic List has no data.");
            }

            writer.WriteLine("\n\n");
        }

        // end TestFilesWereRead()

        /*
         *  ReadConnectomeFile()
         *  read the .csv file being read into the Connectome List
         *  synapse(neuronA,neuronB,weight)
         *
         */

        public static void ReadConnectomeFile()
        {
            //int counter = 0;
            using (var reader = new StreamReader(File.OpenRead(ConnectomeFile)))
            {
                try
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();

                        // Console.WriteLine(line);

                        if (!string.IsNullOrEmpty(line))
                        {
                            var values = line.Split(',');

                            Connectome.Add(new Synapse(values[0], values[1], int.Parse(values[2])));
                            //Console.WriteLine("Added: [" + values[0] + "] [" + values[1] + "] [" + values[2] + "]");
                            //Console.WriteLine("Added " + Connectome[counter].NeuronA);
                            //counter++;
                        }
                        else
                        {
                            Console.WriteLine("line was null");
                        }
                    }
                    // end while
                }
                catch (Exception e)
                {
                    Console.WriteLine("File [" + ConnectomeFile + "] could not be read.");
                    Console.WriteLine("ERROR: " + e.Message);
                }
                // end try/catch

                reader.Close();
            }
            // end using
        }

        // end ReadConnectomeFile()

        /*
         *  ReadPostsynapticFile()
         *  read the .csv file being read into the Postsynaptic List
         *  Synapse(neuronA,weight)
         *
         */

        public static void ReadPostsynapticFile()
        {
            using (var reader = new StreamReader(File.OpenRead(PostsynapticFile)))
            {
                try
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();

                        // Console.WriteLine(line);

                        if (!string.IsNullOrEmpty(line))
                        {
                            var values = line.Split(',');

                            Postsynaptic.Add(new PostSynapticNeuron(values[0], 0));
                        }
                        else
                        {
                            Console.WriteLine("line was null");
                        }
                    }
                    // end while
                }
                catch (Exception e)
                {
                    Console.WriteLine("File [" + PostsynapticFile + "] could not be read.");
                    Console.WriteLine("ERROR: " + e.Message);
                }
                // end try/catch

                reader.Close();
            }
            // end using
        }

        // end ReadPostsynapticFile()

        /*
         *  DendriteAccumulate(Synapse)
         *
         *
         */

        public static void DendriteAccumulate(Synapse synapse, StreamWriter writer)
        {
            foreach (var con in Connectome)
            {
                if (con.NeuronA == synapse.NeuronA)
                {
                    foreach (var postsyn in Postsynaptic)
                    {
                        if (postsyn.NeuronA == con.NeuronB)
                        {
                            postsyn.Weight = con.Weight;

                            // output for file to show Postsynaptic Weight changes
                            Console.WriteLine("\tPostsynaptic Weight: {0} {1} + {2} =  {3}", postsyn.NeuronA, (postsyn.Weight - con.Weight), con.Weight, postsyn.Weight);
                            writer.WriteLine("\tPostsynaptic Weight: {0} {1} + {2} =  {3}", postsyn.NeuronA, (postsyn.Weight - con.Weight), con.Weight, postsyn.Weight);
                        }
                    }
                }
            }
        }

        // end DendriteAccumulate(Synapse)

        /*
         *  FireNeuron(Synapse)
         *
         *
         */

        public static void FireNeuron(Synapse neuron, StreamWriter writer)
        {
            DendriteAccumulate(neuron, writer);
            foreach (var postsyn in Postsynaptic)
            {
                if (Math.Abs(postsyn.Weight) > Threshold)
                {
                    if (postsyn.NeuronA == "PLMR" || postsyn.NeuronA == "PLML" ||
                        postsyn.NeuronA.Substring(0, 2) == "MV" ||
                        postsyn.NeuronA.Substring(0, 2) == "MD")
                    {
                        MuscleFireCount++;
                        Console.WriteLine("Fire Muscle {0}{1}", postsyn.NeuronA, postsyn.NeuronB);
                        writer.WriteLine("Fire Muscle {0}{1}", postsyn.NeuronA, postsyn.NeuronB);
                        postsyn.ResetWeight = 0;
                    }
                    else
                    {
                        NeuronFireCount++;
                        Console.WriteLine("Fire Neuron {0}", postsyn.NeuronA);
                        writer.WriteLine("Fire Neuron {0}", postsyn.NeuronA);
                        DendriteAccumulate(postsyn, writer);
                        postsyn.ResetWeight = 0;
                    }
                }
            }
        }

        // end FireNeuron(Synapse)

        /*
         *  RunConnectome(Synapse)
         *
         *
         */

        public static void RunConnectome(Synapse neuron, StreamWriter writer)
        {
            DendriteAccumulate(neuron, writer);

            foreach (var postsyn in Postsynaptic)
            {
                if (Math.Abs(postsyn.Weight) > Threshold)
                {
                    FireNeuron(postsyn, writer);
                    postsyn.ResetWeight = 0;
                }
            }
        }

        // end RunConnectome(Synapse)
    }

    // end class
}

// end